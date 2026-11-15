using AutoMapper;
using ItHelpDesk.Api.Auth;
using ItHelpDesk.Api.Data.Dtos.Tickets;
using ItHelpDesk.Api.Data.Entities;
using ItHelpDesk.Api.Data.Paging;
using ItHelpDesk.Api.Repositories;
using ItHelpDesk.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ItHelpDesk.Api.Controllers
{
    ///api/tickets GET ALL 200 (filters: status, priority, categoryId, assignedToId, overdue)
    ///api/tickets/{ticketId} GET 200
    ///api/tickets POST 201 (Employee)
    ///api/tickets/{ticketId} PUT 200 (Employee, owner, only New)

    [ApiController]
    [Route("api/tickets")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;
        private readonly UserManager<ItHelpDeskUser> _userManager;

        public TicketsController(ITicketsRepository ticketsRepository, ICategoriesRepository categoriesRepository,
                                 IMapper mapper, LinkGenerator linkGenerator, UserManager<ItHelpDeskUser> userManager)
        {
            _ticketsRepository = ticketsRepository;
            _categoriesRepository = categoriesRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
            _userManager = userManager;
        }
        
        [HttpGet(Name = "GetTickets")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetAll(
            [FromQuery] TicketStatus? status, [FromQuery] TicketPriority? priority,
            [FromQuery] int? categoryId, [FromQuery] string? assignedToId, [FromQuery] bool? overdue,
            [FromQuery] SearchParameters searchParameters)
        {
            string? createdById = null;
            if (User.IsInRole(ItHelpDeskRoles.Employee)) { createdById = CurrentUserId(); }
            if (User.IsInRole(ItHelpDeskRoles.Technician)) { assignedToId = CurrentUserId(); }

            var tickets = await _ticketsRepository.GetFiltered(createdById, assignedToId, status, priority,
                                                               categoryId, overdue, searchParameters);

            Response.AddPaginationHeader(tickets.CreatePaginationMetadata(_linkGenerator, HttpContext, "GetTickets",
                new { status, priority, categoryId, assignedToId, overdue }));

            return Ok(tickets.Select(t => _mapper.Map<TicketDto>(t)));
        }

        [HttpGet("{ticketId}")]
        public async Task<ActionResult<TicketDto>> Get(int ticketId)
        {
            var ticket = await _ticketsRepository.Get(ticketId);
            if (ticket == null || !CanAccess(ticket)) return NotFound();

            return Ok(_mapper.Map<TicketDto>(ticket));
        }

        [HttpPost]
        [Authorize(Roles = ItHelpDeskRoles.Employee)]
        public async Task<ActionResult<TicketDto>> Create(CreateTicketDto dto)
        {
            if (!await IsActiveCategory(dto.CategoryId!.Value))
            {
                ModelState.AddModelError(nameof(dto.CategoryId), "Category does not exist or is inactive.");
                return ValidationProblem(ModelState);
            }

            var now = DateTimeOffset.UtcNow;
            var userId = CurrentUserId();

            var ticket = new Ticket
            {
                Title = dto.Title,
                Description = dto.Description,
                CategoryId = dto.CategoryId.Value,
                CreatedById = userId,
                CreatedAt = now,
                Status = TicketStatus.New
            };
            ticket.History.Add(HistoryEntry("Created", null, TicketStatus.New.ToString()));

            await _ticketsRepository.Create(ticket);

            var created = await _ticketsRepository.Get(ticket.Id);
            return CreatedAtAction(nameof(Get), new { ticketId = ticket.Id }, _mapper.Map<TicketDto>(created));
        }

        [HttpPut("{ticketId}")]
        [Authorize(Roles = ItHelpDeskRoles.Employee)]
        public async Task<ActionResult<TicketDto>> Put(int ticketId, UpdateTicketDto dto)
        {
            var ticket = await _ticketsRepository.Get(ticketId);
            if (ticket == null || ticket.CreatedById != CurrentUserId()) return NotFound();

            if (ticket.Status != TicketStatus.New)
                return Problem(detail: "Only new tickets can be edited.", statusCode: 409);

            if (dto.CategoryId != ticket.CategoryId && !await IsActiveCategory(dto.CategoryId!.Value))
            {
                ModelState.AddModelError(nameof(dto.CategoryId), "Category does not exist or is inactive.");
                return ValidationProblem(ModelState);
            }

            ticket.Title = dto.Title;
            ticket.Description = dto.Description;
            ticket.CategoryId = dto.CategoryId!.Value;

            await _ticketsRepository.SaveChanges();

            var updated = await _ticketsRepository.Get(ticketId);
            return Ok(_mapper.Map<TicketDto>(updated));
        }

        [HttpPost("{ticketId}/assign")]
        [Authorize(Roles = ItHelpDeskRoles.Admin)]
        public async Task<ActionResult<TicketDto>> Assign(int ticketId, AssignTicketDto dto)
        {
            var ticket = await _ticketsRepository.Get(ticketId);
            if (ticket == null) return NotFound();

            var isFirstAssignment = ticket.Status == TicketStatus.New;
            if (!isFirstAssignment && !TicketWorkflow.OpenStatuses.Contains(ticket.Status))
                return Problem(detail: $"A {ticket.Status} ticket cannot be assigned.", statusCode: 409);

            if (isFirstAssignment && dto.Priority == null)
                return FieldError(nameof(dto.Priority), "Priority is required for the first assignment.");

            if (!isFirstAssignment && dto.Priority != null)
                return FieldError(nameof(dto.Priority), "Priority can only be set at the first assignment.");

            var technician = await _userManager.FindByIdAsync(dto.TechnicianId);
            if (technician == null || !technician.IsActive ||
                !await _userManager.IsInRoleAsync(technician, ItHelpDeskRoles.Technician))
                return FieldError(nameof(dto.TechnicianId), "User is not an active technician.");

            if (ticket.AssignedToId == technician.Id)
                return Problem(detail: "Ticket is already assigned to this technician.", statusCode: 409);

            ticket.History.Add(HistoryEntry("Assigned", ticket.AssignedTo?.FullName, technician.FullName));
            ticket.AssignedTo = technician;

            if (isFirstAssignment)
            {
                ticket.Priority = dto.Priority;
                ticket.DueAt = DateTimeOffset.UtcNow + TicketWorkflow.DeadlineFor(dto.Priority!.Value);
                ticket.History.Add(HistoryEntry("StatusChanged", ticket.Status.ToString(), TicketStatus.Assigned.ToString()));
                ticket.Status = TicketStatus.Assigned;
            }

            await _ticketsRepository.SaveChanges();

            var updated = await _ticketsRepository.Get(ticketId);
            return Ok(_mapper.Map<TicketDto>(updated));
        }

        [HttpPatch("{ticketId}/status")]
        public async Task<ActionResult<TicketDto>> ChangeStatus(int ticketId, ChangeTicketStatusDto dto)
        {
            var ticket = await _ticketsRepository.Get(ticketId);
            if (ticket == null || !CanAccess(ticket)) return NotFound();

            var from = ticket.Status;
            var to = dto.Status!.Value;

            if (to == TicketStatus.Assigned)
                return Problem(detail: "Use the assign endpoint to assign a ticket.", statusCode: 400);

            if (!TicketWorkflow.IsValidTransition(from, to))
                return Problem(detail: $"Cannot change status from {from} to {to}.", statusCode: 409);

            if (!TicketWorkflow.IsTransitionAllowed(from, to, CurrentRole()))
                return Problem(detail: $"Your role cannot change status from {from} to {to}.", statusCode: 403);

            var now = DateTimeOffset.UtcNow;

            switch (to)
            {
                case TicketStatus.Resolved:
                    if (string.IsNullOrWhiteSpace(dto.Comment))
                        return FieldError(nameof(dto.Comment), "A resolution note is required.");
                    ticket.ResolutionNote = dto.Comment;
                    ticket.ResolvedAt = now;
                    break;

                case TicketStatus.Closed:
                    ticket.ClosedAt = now;
                    break;

                case TicketStatus.Reopened:
                    if (now - ticket.ResolvedAt!.Value > TicketWorkflow.ReopenWindow)
                        return Problem(detail: "Tickets can only be reopened within 7 days of being resolved.", statusCode: 409);
                    if (string.IsNullOrWhiteSpace(dto.Comment))
                        return FieldError(nameof(dto.Comment), "Please explain why the ticket is reopened.");
                    ticket.Comments.Add(new TicketComment
                    {
                        Text = dto.Comment,
                        AuthorId = CurrentUserId(),
                        CreatedAt = now
                    });
                    ticket.ReopenCount++;
                    ticket.ResolvedAt = null;
                    ticket.DueAt = now + TicketWorkflow.DeadlineFor(ticket.Priority!.Value);
                    break;
            }

            ticket.History.Add(HistoryEntry("StatusChanged", from.ToString(), to.ToString()));
            ticket.Status = to;

            await _ticketsRepository.SaveChanges();

            var updated = await _ticketsRepository.Get(ticketId);
            return Ok(_mapper.Map<TicketDto>(updated));
        }

        private bool CanAccess(Ticket ticket)
        {
            if (User.IsInRole(ItHelpDeskRoles.Admin)) return true;

            var userId = CurrentUserId();
            if (User.IsInRole(ItHelpDeskRoles.Employee)) return ticket.CreatedById == userId;
            if (User.IsInRole(ItHelpDeskRoles.Technician)) return ticket.AssignedToId == userId;
            return false;
        }

        private TicketHistory HistoryEntry(string action, string? oldValue, string? newValue) => new()
        {
            Action = action,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedById = CurrentUserId(),
            ChangedAt = DateTimeOffset.UtcNow
        };

        private async Task<bool> IsActiveCategory(int categoryId)
        {
            var category = await _categoriesRepository.Get(categoryId);
            return category != null && category.IsActive;
        }

        private string CurrentUserId() => User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
        private string CurrentRole() => ItHelpDeskRoles.All.First(User.IsInRole);

        private ActionResult FieldError(string field, string message)
        {
            ModelState.AddModelError(field, message);
            return ValidationProblem(ModelState);
        }

    }
}