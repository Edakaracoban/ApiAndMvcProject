using System;
using hotelapi.Models;

namespace hotelapi.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public UserModel Authenticate(string username, string password)
        {
            // Basit örnek: şifreleri açık tutuyoruz, gerçek projede hash kullanılmalı!
            var user = _context.Users.SingleOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
                return null;

            return new UserModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
                // Token vs ekleyebilirsin
            };
        }
    }
}

