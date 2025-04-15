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
        void AddTool(Tool tool);
        void UpdateTool(Tool tool);
        void DeleteTool(int id);
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
            return _context.Tools.ToList();
        }

        public Tool? GetToolById(int id)
        {
            return _context.Tools.Find(id);
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
    }
}
