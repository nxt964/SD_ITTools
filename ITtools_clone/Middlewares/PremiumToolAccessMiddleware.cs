using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ITtools_clone.Services;
using ITtools_clone.Helpers;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace ITtools_clone.Middlewares
{
    public class PremiumToolAccessMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public PremiumToolAccessMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Admin") ||
                context.Request.Path.StartsWithSegments("/Auth") ||
                context.Session.GetInt32("isAdmin") == 1)
            {
                await _next(context);
                return;
            }

            string path = context.Request.Path.Value?.TrimStart('/') ?? string.Empty;

            if (string.IsNullOrEmpty(path) || path == "Home" || path == "Home/Index")
            {
                await _next(context);
                return;
            }

            // Tạo scope mới để lấy `IToolService`
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var toolService = scope.ServiceProvider.GetRequiredService<IToolService>();
                var allTools = toolService.GetAllTools();
                var matchingTool = allTools.FirstOrDefault(t => t.tool_name != null &&
                                                            Utils.Slugify(t.tool_name) == path);

                if (matchingTool != null && matchingTool.premium_required)
                {
                    bool isLoggedIn = context.Session.GetInt32("UserId") != null;
                    bool isPremiumUser = context.Session.GetInt32("Premium") == 1;
                    if (!isLoggedIn || !isPremiumUser)
                    {
                        context.Response.Redirect("/Home/PremiumRequired");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
