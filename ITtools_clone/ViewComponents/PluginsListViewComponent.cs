using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ITtools_clone.Services;

namespace ITtools_clone.ViewComponents
{
    public class PluginsListViewComponent : ViewComponent
    {
        private readonly IToolService _toolService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PluginsListViewComponent(IToolService toolService, IHttpContextAccessor httpContextAccessor)
        {
            _toolService = toolService;
            _httpContextAccessor = httpContextAccessor;
        }

        public IViewComponentResult Invoke()
        {
            // Check if user is admin
            bool isAdmin = _httpContextAccessor.HttpContext?.Session.GetInt32("isAdmin") == 1;

            if (isAdmin) {
                return View(_toolService.GetCategorizedTools(true, false));
            }
            else {
                // Check if user is premium
                bool isPremiumUser = _httpContextAccessor.HttpContext?.Session.GetInt32("Premium") == 1;
                return View(_toolService.GetCategorizedTools(false, isPremiumUser));
            }
        }
    }
}
