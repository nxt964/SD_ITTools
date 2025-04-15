using ITtools_clone.Models;
using ITtools_clone.Repositories;

namespace ITtools_clone.Services
{
    public interface IFavouriteService
    {
        List<Tool> GetFavouriteToolsByUserId(int userId);
        void AddToFavourites(int userId, int toolId);
        void RemoveFromFavourites(int userId, int toolId);
        void RemoveFromFavouritesByUserId(int userId);
        void RemoveFromFavouritesByToolId(int toolId);
        bool IsFavourite(int userId, int toolId);
    }
    public class FavouriteService : IFavouriteService
    {
        private readonly IFavouriteRepository _favouriteRepository;

        public FavouriteService(IFavouriteRepository favouriteRepository)
        {
            _favouriteRepository = favouriteRepository;
        }

        public List<Tool> GetFavouriteToolsByUserId(int userId)
        {
            return _favouriteRepository.GetFavouriteToolsByUserId(userId);
        }

        public void AddToFavourites(int userId, int toolId)
        {
            _favouriteRepository.AddToFavourites(userId, toolId);
        }
        public void RemoveFromFavourites(int userId, int toolId)
        {
            _favouriteRepository.RemoveFromFavourites(userId, toolId);
        }
        public void RemoveFromFavouritesByUserId(int userId)
        {
            _favouriteRepository.RemoveFromFavouritesByUserId(userId);
        }
        public void RemoveFromFavouritesByToolId(int toolId)
        {
            _favouriteRepository.RemoveFromFavouritesByToolId(toolId);
        }
        public bool IsFavourite(int userId, int toolId)
        {
            return _favouriteRepository.IsFavourite(userId, toolId);
        }
        
    }
}