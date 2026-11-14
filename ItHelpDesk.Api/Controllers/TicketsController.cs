using AutoMapper;
using ItHelpDesk.Api.Auth;
using ItHelpDesk.Api.Data.Dtos.Tickets;
using ItHelpDesk.Api.Data.Entities;
using ItHelpDesk.Api.Data.Paging;
using ItHelpDesk.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
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

        public TicketsController(ITicketsRepository ticketsRepository, ICategoriesRepository categoriesRepository,
                                 IMapper mapper, LinkGenerator linkGenerator)
        {
            _ticketsRepository = ticketsRepository;
            _categoriesRepository = categoriesRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
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
            ticket.History.Add(new TicketHistory
            {
                Action = "Created",
                NewValue = TicketStatus.New.ToString(),
                ChangedById = userId,
                ChangedAt = now
            });

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
            ticket.History.Add(new TicketHistory
            {
                Action = "Edited",
                ChangedById = CurrentUserId(),
                ChangedAt = DateTimeOffset.UtcNow
            });

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

        private async Task<bool> IsActiveCategory(int categoryId)
        {
            var category = await _categoriesRepository.Get(categoryId);
            return category != null && category.IsActive;
        }

        private string CurrentUserId() => User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
    }
}