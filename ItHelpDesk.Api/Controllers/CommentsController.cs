using AutoMapper;
using ItHelpDesk.Api.Auth;
using ItHelpDesk.Api.Data.Dtos.Comments;
using ItHelpDesk.Api.Data.Entities;
using ItHelpDesk.Api.Repositories;
using ItHelpDesk.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ItHelpDesk.Api.Controllers
{
    ///api/tickets/{ticketId}/comments GET ALL 200
    ///api/tickets/{ticketId}/comments POST 201

    [ApiController]
    [Route("api/tickets/{ticketId}/comments")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ICommentsRepository _commentsRepository;
        private readonly IMapper _mapper;

        public CommentsController(ITicketsRepository ticketsRepository, ICommentsRepository commentsRepository, IMapper mapper)
        {
            _ticketsRepository = ticketsRepository;
            _commentsRepository = commentsRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetAll(int ticketId)
        {
            var ticket = await _ticketsRepository.Get(ticketId);
            if (ticket == null || !CanView(ticket)) return NotFound();

            var includeInternal = User.GetRole() != ItHelpDeskRoles.Employee;
            var comments = await _commentsRepository.GetForTicket(ticketId, includeInternal);

            return Ok(comments.Select(c => _mapper.Map<CommentDto>(c)));
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> Create(int ticketId, CreateCommentDto dto)
        {
            var ticket = await _ticketsRepository.Get(ticketId);
            if (ticket == null || !CanView(ticket)) return NotFound();

            if (dto.IsInternal && User.GetRole() == ItHelpDeskRoles.Employee)
                return Problem(detail: "Employees cannot write internal notes.", statusCode: 403);

            if (TicketWorkflow.FinalStatuses.Contains(ticket.Status))
                return Problem(detail: $"Cannot comment on a {ticket.Status} ticket.", statusCode: 409);

            var comment = new TicketComment
            {
                TicketId = ticketId,
                Text = dto.Text,
                IsInternal = dto.IsInternal,
                AuthorId = User.GetUserId(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _commentsRepository.Create(comment);

            return StatusCode(201, _mapper.Map<CommentDto>(comment));
        }

        private bool CanView(Ticket ticket) =>
            TicketWorkflow.CanView(ticket, User.GetUserId(), User.GetRole());
    }
}