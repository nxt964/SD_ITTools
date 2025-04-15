using Microsoft.AspNetCore.Mvc;
using ITtools_clone.Models;
using ITtools_clone.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace ITtools_clone.Controllers
{
    public class AdminController : Controller
    {
        private readonly IToolService _toolService;
        private readonly IUserService _userService;
        private readonly IFavouriteService _favouriteService;

        public AdminController(IToolService toolService, IUserService userService, IFavouriteService favouriteService)
        {
            _toolService = toolService;
            _userService = userService;
            _favouriteService = favouriteService;
        }

        // Admin Dashboard
        public IActionResult Index()
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("isAdmin") != 1)
            {
                return RedirectToAction("Login", "Auth");
            }

            return RedirectToAction("ManageTools");
        }

        // Tool Management
        public IActionResult ManageTools()
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("isAdmin") != 1)
            {
                return RedirectToAction("Login", "Auth");
            }

            var tools = _toolService.GetAllTools();
            return View(tools);
        }

        [HttpPost]
        public IActionResult UpdateToolStatus(int id, bool enabled)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("isAdmin") != 1)
            {
                return RedirectToAction("Login", "Auth");
            }

            var tool = _toolService.GetToolById(id);
            if (tool == null)
            {
                return NotFound();
            }

            tool.enabled = !tool.enabled;
            _toolService.UpdateTool(tool);
            
            return RedirectToAction("ManageTools");
        }

        [HttpPost]
        public IActionResult UpdateToolPremium(int id, bool premium)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("isAdmin") != 1)
            {
                return RedirectToAction("Login", "Auth");
            }

            var tool = _toolService.GetToolById(id);
            if (tool == null)
            {
                return NotFound();
            }

            tool.premium_required = !tool.premium_required;
            _toolService.UpdateTool(tool);
            
            return RedirectToAction("ManageTools");
        }

        public IActionResult DeleteTool(int id)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("isAdmin") != 1)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get tool from database
            var tool = _toolService.GetToolById(id);
            if (tool == null)
            {
                return NotFound();
            }

            try
            {
                if (!string.IsNullOrEmpty(tool.file_name))
                {
                    string pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
                    string filePath = Path.Combine(pluginsPath, tool.file_name);
                    
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                            Console.WriteLine($"Deleted plugin file: {filePath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error deleting file: {ex.Message}");
                            // Consider marking for later deletion if file is locked
                        }
                    }
                }

                _toolService.DeleteTool(id);
                // Remove associated favourites
                _favouriteService.RemoveFromFavouritesByToolId(id);

                TempData["SuccessMessage"] = "Tool and associated files deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting tool: {ex.Message}";
                Console.WriteLine($"Error in DeleteTool: {ex.Message}");
            }
            
            return RedirectToAction("ManageTools");
        }

        // User Management
        public IActionResult ManageUsers()
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("isAdmin") != 1)
            {
                return RedirectToAction("Login", "Auth");
            }

            var users = _userService.GetAllUsers();
            return View(users);
        }

        [HttpPost]
        public IActionResult UpdateUserPremium(int id, bool premium)
        {
            var user = _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            user.premium = !user.premium;
            
            // If upgrading to premium, also clear the request flag
            if (user.premium)
            {
                user.request_premium = false;
            }

            _userService.UpdateUser(user);

            HttpContext.Session.SetInt32("Premium", user.premium ? 1 : 0);
            HttpContext.Session.SetInt32("RequestPremium", user.request_premium ? 1 : 0);
            
            TempData["Message"] = user.premium 
                ? $"Đã nâng cấp {user.username} lên Premium" 
                : $"Đã hạ cấp {user.username} xuống Free";
                
            return RedirectToAction("ManageUsers");
        }

        public IActionResult DeleteUser(int id)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("isAdmin") != 1)
            {
                return RedirectToAction("Login", "Auth");
            }

            _userService.DeleteUser(id);
            _favouriteService.RemoveFromFavouritesByUserId(id);
            return RedirectToAction("ManageUsers");
        }

        public IActionResult PremiumRequests()
        {
            // Only admin can access
            if (HttpContext.Session.GetInt32("isAdmin") != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            var users = _userService.GetAllUsers().Where(u => u.request_premium).ToList();
            return View(users);
        }

        [HttpPost]
        public IActionResult DenyPremiumRequest(int id)
        {
            // Only admin can access
            if (HttpContext.Session.GetInt32("isAdmin") != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            // Reset request_premium to false without upgrading to premium
            user.request_premium = false;
            _userService.UpdateUser(user);
            HttpContext.Session.SetInt32("Premium", user.premium ? 1 : 0);
            HttpContext.Session.SetInt32("RequestPremium", user.request_premium ? 1 : 0);

            TempData["Message"] = $"Đã từ chối yêu cầu nâng cấp Premium của người dùng {user.username}";
            return RedirectToAction("ManageUsers");
        }
    }
}