using System;
using hotelapi.Models;

namespace hotelapi.Services
{
	public interface IUserService
	{
        UserModel Authenticate(string username, string password);
    }
}

