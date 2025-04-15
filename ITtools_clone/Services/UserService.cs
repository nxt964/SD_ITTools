using System.Collections.Generic;
using ITtools_clone.Models;
using ITtools_clone.Repositories;

namespace ITtools_clone.Services
{
    public interface IUserService
    {
        List<User> GetAllUsers();
        User GetUserById(int id);
        User GetUserByEmail(string email);
        User GetUserByUsername(string username);
        void RegisterUser(User user, string password);
        bool ValidateUserLogin(string email, string password);
        void UpdateUser(User user);
        void DeleteUser(int id);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public List<User> GetAllUsers()
        {
            return _userRepository.GetAllUsers();
        }

        public User GetUserById(int id)
        {
            return _userRepository.GetUserById(id);
        }

        public User GetUserByEmail(string email)
        {
            return _userRepository.GetUserByEmail(email);
        }

        public User GetUserByUsername(string username)
        {
            return _userRepository.GetUserByUsername(username);
        }

        public void RegisterUser(User user, string password)
        {
            // Hash the password
            user.password = BCrypt.Net.BCrypt.HashPassword(password);
            _userRepository.AddUser(user);
        }

        public bool ValidateUserLogin(string email, string password)
        {
            var user = _userRepository.GetUserByEmail(email);
            if (user == null)
                return false;

            return BCrypt.Net.BCrypt.Verify(password, user.password);
        }

        public void UpdateUser(User user)
        {
            _userRepository.UpdateUser(user);
        }

        public void DeleteUser(int id)
        {
            _userRepository.DeleteUser(id);
        }
    }
}
