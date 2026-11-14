using AutoMapper;
using ItHelpDesk.Api.Auth;
using ItHelpDesk.Api.Data.Dtos.Categories;
using ItHelpDesk.Api.Data.Entities;
using ItHelpDesk.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ItHelpDesk.Api.Controllers
{
    ///api/categories GET ALL 200
    ///api/categories/{categoryId} GET 200
    ///api/categories POST 201 (Admin)
    ///api/categories/{categoryId} PUT 200 (Admin)
    ///api/categories/{categoryId} DELETE 204 (Admin)

    [ApiController]
    [Route("api/categories")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoriesRepository categoriesRepository, IMapper mapper)
        {
            _categoriesRepository = categoriesRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
        {
            var categories = await _categoriesRepository.GetAll(includeInactive: IsAdmin());
            return Ok(categories.Select(c => _mapper.Map<CategoryDto>(c)));
        }

        [HttpGet("{categoryId}")]
        public async Task<ActionResult<CategoryDto>> Get(int categoryId)
        {
            var category = await _categoriesRepository.Get(categoryId);
            if (category == null || (!category.IsActive && !IsAdmin())) return NotFound();

            return Ok(_mapper.Map<CategoryDto>(category));
        }

        [HttpPost]
        [Authorize(Roles = ItHelpDeskRoles.Admin)]
        public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
        {
            if (await _categoriesRepository.NameExists(dto.Name))
                return Problem(detail: "A category with this name already exists.", statusCode: 409);

            var category = _mapper.Map<Category>(dto);
            await _categoriesRepository.Create(category);

            return CreatedAtAction(nameof(Get), new { categoryId = category.Id }, _mapper.Map<CategoryDto>(category));
        }

        [HttpPut("{categoryId}")]
        [Authorize(Roles = ItHelpDeskRoles.Admin)]
        public async Task<ActionResult<CategoryDto>> Put(int categoryId, UpdateCategoryDto dto)
        {
            var category = await _categoriesRepository.Get(categoryId);
            if (category == null) return NotFound();

            if (await _categoriesRepository.NameExists(dto.Name, excludeCategoryId: categoryId))
                return Problem(detail: "A category with this name already exists.", statusCode: 409);

            _mapper.Map(dto, category);
            await _categoriesRepository.Update(category);

            return Ok(_mapper.Map<CategoryDto>(category));
        }

        [HttpDelete("{categoryId}")]
        [Authorize(Roles = ItHelpDeskRoles.Admin)]
        public async Task<IActionResult> Delete(int categoryId)
        {
            var category = await _categoriesRepository.Get(categoryId);
            if (category == null) return NotFound();

            if (await _categoriesRepository.HasTickets(categoryId))
                return Problem(detail: "Category is used by tickets. Deactivate it instead.", statusCode: 409);

            await _categoriesRepository.Delete(category);
            return NoContent();
        }

        private bool IsAdmin() => User.IsInRole(ItHelpDeskRoles.Admin);
    }
}