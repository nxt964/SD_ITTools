using System.Collections.Generic;
using ITtools_clone.Models;
using ITtools_clone.Repositories;
using System.Linq;

namespace ITtools_clone.Services
{
    public interface IToolService
    {
        List<Tool> GetAllTools();
        List<Tool> GetEnabledTools();
        List<Tool> GetToolsForUser(bool isPremiumUser);
        Dictionary<string, List<string>> GetCategorizedTools(bool isAdmin);
        Tool GetToolById(int id);
        void AddTool(Tool tool);
        void UpdateTool(Tool tool);
        void DeleteTool(int id);
        bool DeleteToolWithRelatedData(int toolId);
        bool ValidateTool(Tool tool);
        Tool GetToolByName(string toolName);
        List<Tool> SearchTools(string query, bool isAdmin);
    }

    public class ToolService : IToolService
    {
        private readonly IToolRepository _toolRepository;
        private readonly IPluginRepository _pluginRepository;
        private readonly IFavouriteService _favouriteService;

        public ToolService(IToolRepository toolRepository, IPluginRepository pluginRepository, IFavouriteService favouriteService)
        {
            _toolRepository = toolRepository;
            _pluginRepository = pluginRepository;
            _favouriteService = favouriteService;
        }

        

        public List<Tool> GetAllTools()
        {
            return _toolRepository.GetAllTools();
        }

        public List<Tool> GetEnabledTools()
        {
            return _toolRepository.GetAllTools().Where(t => t.enabled).ToList();
        }

        // New method to get tools based on user's premium status
        public List<Tool> GetToolsForUser(bool isPremiumUser)
        {
            var enabledTools = GetEnabledTools();
            
            if (isPremiumUser)
            {
                // Premium users can access all enabled tools
                return enabledTools;
            }
            else
            {
                // Non-premium users can only access non-premium tools
                return enabledTools.Where(t => !t.premium_required).ToList();
            }
        }

        // Modified method to consider premium status
        public Dictionary<string, List<string>> GetCategorizedTools(bool isAdmin)
        {
            var toolsByCategory = _toolRepository.GetToolsByCategory(enabledOnly: !isAdmin);
            
            // Transform to required output format
            return toolsByCategory.ToDictionary(
                category => category.Key,
                category => category.Value.Select(t => t.tool_name!).ToList()
            );
        }

        public Tool GetToolById(int id)
        {
            return _toolRepository.GetToolById(id);
        }

        public void AddTool(Tool tool)
        {
            // Add business logic/validation before adding
            if (ValidateTool(tool))
            {
                _toolRepository.AddTool(tool);
            }
            else
            {
                throw new System.Exception("Tool validation failed");
            }
        }

        public void UpdateTool(Tool tool)
        {
            if (ValidateTool(tool))
            {
                _toolRepository.UpdateTool(tool);
            }
            else
            {
                throw new System.Exception("Tool validation failed");
            }
        }

        public void DeleteTool(int id)
        {
            _toolRepository.DeleteTool(id);
        }

        public bool DeleteToolWithRelatedData(int toolId)
        {
            try
            {
                // Get tool for its file name
                var tool = GetToolById(toolId);
                if (tool == null) return false;
                
                // Delete physical file if exists
                if (!string.IsNullOrEmpty(tool.file_name))
                {
                    _pluginRepository.DeletePluginFile(tool.file_name);
                }
                
                // Delete favorites
                _favouriteService.RemoveFromFavouritesByToolId(toolId);
                
                // Delete tool record
                DeleteTool(toolId);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteToolWithRelatedData: {ex.Message}");
                return false;
            }
        }

        public bool ValidateTool(Tool tool)
        {
            // Add business validation logic here
            if (string.IsNullOrEmpty(tool.tool_name))
                return false;

            if (string.IsNullOrEmpty(tool.description))
                return false;

            return true;
        }

        public Tool GetToolByName(string toolName)
        {
            return _toolRepository.GetToolByName(toolName);
        }

        public List<Tool> SearchTools(string query, bool isAdmin)
        {
            var searchResults = _toolRepository.SearchTools(query, !isAdmin);
            
            return searchResults;
        }
    }
}
