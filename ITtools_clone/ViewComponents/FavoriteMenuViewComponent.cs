using ITtools_clone.Models;
using ITtools_clone.Services;
using Microsoft.AspNetCore.Mvc;

namespace ITtools_clone.ViewComponents
{
    public class FavoriteMenuViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFavouriteService _favouriteService;

        public FavoriteMenuViewComponent(IHttpContextAccessor httpContextAccessor, IFavouriteService favouriteService)
        {
            _httpContextAccessor = httpContextAccessor;
            _favouriteService = favouriteService;
        }

        public IViewComponentResult Invoke()
        {
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");
            if (userId != null)
            {
                var favorites = _favouriteService.GetFavouriteToolsByUserId(userId.Value);
                return View(favorites);
            }
            else
            {
                return View(new List<Tool>());
            }
        }
    }
}