using ITtools_clone.Models;
using ITtools_clone.Services;
using Microsoft.AspNetCore.Mvc;

namespace ITtools_clone.ViewComponents
{
    public class ToolCardViewComponent : ViewComponent
    {
        private readonly IFavouriteService _favouriteService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ToolCardViewComponent(IFavouriteService favouriteService, IHttpContextAccessor httpContextAccessor)
        {
            _favouriteService = favouriteService;
            _httpContextAccessor = httpContextAccessor;
        }

        public IViewComponentResult Invoke(Tool tool, bool isUserFavourite)
        {
            return View((tool, isUserFavourite));
        }
        
    }
}