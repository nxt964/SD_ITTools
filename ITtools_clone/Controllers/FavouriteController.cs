using ITtools_clone.Services;
using Microsoft.AspNetCore.Mvc;

namespace ITtools_clone.Controllers
{
    public class FavouriteController : Controller
    {
        private readonly IFavouriteService _favouriteService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FavouriteController(IFavouriteService favouriteService, IHttpContextAccessor httpContextAccessor)
        {
            _favouriteService = favouriteService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public IActionResult AddToFavourites([FromBody] int toolId)
        {
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId") ?? -1;
            if (userId == -1)
            {
                return Unauthorized();
            }

            try
            {
                _favouriteService.AddToFavourites(userId, toolId);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddToFavourites: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost]
        public IActionResult RemoveFromFavourites([FromBody] int toolId)
        {
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId") ?? -1;
            if (userId == -1)
            {
                return Unauthorized();
            }

            try
            {
                _favouriteService.RemoveFromFavourites(userId, toolId);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveFromFavourites: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}