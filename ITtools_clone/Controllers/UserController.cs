using Microsoft.AspNetCore.Mvc;
using ITtools_clone.Models;
using ITtools_clone.Services;
using Microsoft.AspNetCore.Http;

namespace ITtools_clone.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult RequestPremium()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.request_premium = true;
            _userService.UpdateUser(user);
            
            // Update session
            HttpContext.Session.SetInt32("RequestPremium", 1);
            
            TempData["Message"] = "Yêu cầu nâng cấp Premium của bạn đã được gửi. Vui lòng chờ quản trị viên xác nhận.";
            return RedirectToAction("Index", "Home");
        }
    }
}