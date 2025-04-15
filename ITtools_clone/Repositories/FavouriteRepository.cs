using ITtools_clone.Models;

namespace ITtools_clone.Repositories
{
    public interface IFavouriteRepository
    {
        List<Tool> GetFavouriteToolsByUserId(int userId);
        void AddToFavourites(int userId, int toolId);
        void RemoveFromFavourites(int userId, int toolId);
        void RemoveFromFavouritesByUserId(int userId);
        void RemoveFromFavouritesByToolId(int toolId);

        bool IsFavourite(int userId, int toolId);
    }
    public class FavouriteRepository : IFavouriteRepository
    {
        private readonly AppDbContext _context;
        public FavouriteRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Tool> GetFavouriteToolsByUserId(int userId)
        {
            var favoriteToolIds = _context.Favorites
                                        .Where(f => f.usid == userId)
                                        .Select(f => f.tid)
                                        .ToList();

            var tools = _context.Tools
                                .Where(t => favoriteToolIds.Contains(t.tid) && t.enabled == true)
                                .ToList();

            return tools;
        }

        public void AddToFavourites(int userId, int toolId)
        {
            var favorite = new Favorite
            {
                usid = userId,
                tid = toolId
            };
            _context.Favorites.Add(favorite);
            _context.SaveChanges();
        }

        public void RemoveFromFavourites(int userId, int toolId)
        {
            var favorite = _context.Favorites
                                   .FirstOrDefault(f => f.usid == userId && f.tid == toolId);
            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                _context.SaveChanges();
            }
        }

        public void RemoveFromFavouritesByUserId(int userId)
        {
            var favorites = _context.Favorites
                                    .Where(f => f.usid == userId)
                                    .ToList();
            _context.Favorites.RemoveRange(favorites);
            _context.SaveChanges();
        }

        public void RemoveFromFavouritesByToolId(int toolId)
        {
            var favorites = _context.Favorites
                                    .Where(f => f.tid == toolId)
                                    .ToList();
            _context.Favorites.RemoveRange(favorites);
            _context.SaveChanges();
        }

        public bool IsFavourite(int userId, int toolId)
        {
            return _context.Favorites.Any(f => f.usid == userId && f.tid == toolId);
        }
    }
}