using System.ComponentModel.DataAnnotations;

namespace ITtools_clone.Models
{
    public class Tool
    {
        [Key]
        public int tid { get; set; }
        public string? tool_name { get; set; }
        public string? description { get; set; }
        public bool enabled { get; set; }
        public bool premium_required { get; set; }
        public string? category_name { get; set; }
        public string? file_name { get; set; }
    }
}
