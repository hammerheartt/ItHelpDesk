using ItHelpDesk.Api.Auth;
using ItHelpDesk.Api.Data.Dtos.Stats;
using ItHelpDesk.Api.Data.Entities;
using ItHelpDesk.Api.Repositories;
using ItHelpDesk.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ItHelpDesk.Api.Controllers
{
    ///api/stats GET 200 (Admin)

    [ApiController]
    [Route("api/stats")]
    [Authorize(Roles = ItHelpDeskRoles.Admin)]
    public class StatsController : ControllerBase
    {
        private readonly IStatsRepository _statsRepository;
        private readonly UserManager<ItHelpDeskUser> _userManager;

        public StatsController(IStatsRepository statsRepository, UserManager<ItHelpDeskUser> userManager)
        {
            _statsRepository = statsRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<StatsDto>> Get()
        {
            var now = DateTimeOffset.UtcNow;

            var byStatus = await _statsRepository.CountByStatus();
            var ticketsByStatus = Enum.GetValues<TicketStatus>()
                .ToDictionary(s => s.ToString(), s => byStatus.GetValueOrDefault(s));

            var workload = (await _statsRepository.CountByAssignee()).ToDictionary(w => w.TechnicianId);
            var technicians = await _userManager.GetUsersInRoleAsync(ItHelpDeskRoles.Technician);

            var technicianDtos = technicians
                .OrderBy(t => t.FullName)
                .Select(t =>
                {
                    workload.TryGetValue(t.Id, out var w);
                    return new TechnicianWorkloadDto(t.Id, t.FullName, t.IsActive, w?.Open ?? 0, w?.Resolved ?? 0);
                })
                .ToList();

            return Ok(new StatsDto(
                TotalTickets: byStatus.Values.Sum(),
                TicketsByStatus: ticketsByStatus,
                UnassignedTickets: byStatus.GetValueOrDefault(TicketStatus.New),
                OpenTickets: TicketWorkflow.OpenStatuses.Sum(s => byStatus.GetValueOrDefault(s)),
                OverdueTickets: await _statsRepository.CountOverdue(now),
                ReopenedTickets: await _statsRepository.CountReopened(),
                AverageResolutionHours: await _statsRepository.AverageResolutionHours(),
                TicketsByCategory: await _statsRepository.CountByCategory(),
                Technicians: technicianDtos));
        }
    }
}