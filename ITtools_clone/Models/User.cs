using System.ComponentModel.DataAnnotations;

namespace ITtools_clone.Models
{
    public class User
    {
        [Key]
        public int usid { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public string? email { get; set; }
        public bool premium { get; set; }
        public bool is_admin { get; set; }
        public bool request_premium { get; set; }
    }
}
