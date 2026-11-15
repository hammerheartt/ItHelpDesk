using AutoMapper;
using ItHelpDesk.Api.Data.Dtos.Categories;
using ItHelpDesk.Api.Data.Dtos.Comments;
using ItHelpDesk.Api.Data.Dtos.Tickets;
using ItHelpDesk.Api.Data.Entities;
using ItHelpDesk.Api.Services;

namespace ItHelpDesk.Api.Data
{
    public class ItHelpDeskProfile : Profile
    {
        public ItHelpDeskProfile()
        {
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive!.Value));

            CreateMap<Ticket, TicketDto>()
                .ForCtorParam(nameof(TicketDto.CategoryName), o => o.MapFrom(s => s.Category.Name))
                .ForCtorParam(nameof(TicketDto.CreatedByName), o => o.MapFrom(s => s.CreatedBy.FullName))
                .ForCtorParam(nameof(TicketDto.AssignedToName), o => o.MapFrom(s => s.AssignedTo != null ? s.AssignedTo.FullName : null))
                .ForCtorParam(nameof(TicketDto.IsOverdue), o => o.MapFrom(s => TicketWorkflow.IsOverdue(s, DateTimeOffset.UtcNow)));

            CreateMap<TicketHistory, TicketHistoryDto>()
                .ForCtorParam(nameof(TicketHistoryDto.ChangedByName), o => o.MapFrom(s => s.ChangedBy.FullName));

            CreateMap<TicketComment, CommentDto>()
                .ForCtorParam(nameof(CommentDto.AuthorName), o => o.MapFrom(s => s.Author.FullName));
        }
    }
}