namespace ITtools_clone.Models
{
    public class HomeViewModel
    {
        public List<Tool> AllTools { get; set; } = new List<Tool>();
        public List<Tool> FavouriteTools { get; set; } = new List<Tool>();
    }
}