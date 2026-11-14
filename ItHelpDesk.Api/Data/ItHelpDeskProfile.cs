using AutoMapper;
using ItHelpDesk.Api.Data.Dtos.Categories;
using ItHelpDesk.Api.Data.Entities;

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
        }
    }
}