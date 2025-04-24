using System.Collections.Generic;
using System.Linq;
using ITtools_clone.Models;
using Microsoft.EntityFrameworkCore;

namespace ITtools_clone.Repositories
{
    public interface IToolRepository
    {
        List<Tool> GetAllTools();
        Tool GetToolById(int id);
        Tool GetToolByName(string toolName);
        Dictionary<string, List<Tool>> GetToolsByCategory(bool enabledOnly);
        void AddTool(Tool tool);
        void UpdateTool(Tool tool);
        void DeleteTool(int id);
        List<Tool> SearchTools(string query, bool enabledOnly);
    }

    public class ToolRepository : IToolRepository
    {
        private readonly AppDbContext _context;

        public ToolRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Tool> GetAllTools()
        {
            return _context.Tools.ToList().OrderBy(t => t.category_name).ToList();
        }

        public Tool? GetToolById(int id)
        {
            return _context.Tools.Find(id);
        }

        public Tool GetToolByName(string toolName)
        {
            return _context.Tools
                .FirstOrDefault(t => t.tool_name != null && 
                                 t.tool_name.ToLower() == toolName.ToLower());
        }

        public Dictionary<string, List<Tool>> GetToolsByCategory(bool enabledOnly)
        {
            IQueryable<Tool> query = _context.Tools;
            
            if (enabledOnly)
                query = query.Where(t => t.enabled);
            
            return query
                .Where(t => !string.IsNullOrEmpty(t.category_name))
                .GroupBy(t => t.category_name!)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        public void AddTool(Tool tool)
        {
            _context.Tools.Add(tool);
            _context.SaveChanges();
        }

        public void UpdateTool(Tool tool)
        {
            _context.Entry(tool).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void DeleteTool(int id)
        {
            var tool = _context.Tools.Find(id);
            if (tool != null)
            {
                _context.Tools.Remove(tool);
                _context.SaveChanges();
            }
        }

        public List<Tool> SearchTools(string query, bool enabledOnly)
        {
            IQueryable<Tool> toolsQuery = _context.Tools;
            
            if (enabledOnly)
            {
                toolsQuery = toolsQuery.Where(t => t.enabled);
            }
            
            return toolsQuery.Where(t => 
                t.tool_name!.Contains(query) || 
                t.description!.Contains(query)
            ).ToList();
        }
    }
}
