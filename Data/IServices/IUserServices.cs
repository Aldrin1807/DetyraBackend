using Backend.Data.DTOs;
using Backend.Models;

namespace Backend.Data.IServices
{
    public interface IUserServices
    {
       abstract Task Register(UserDTO user);
       abstract Task<string> Login(Login login);

        abstract Task<List<User>> GetUsers();
        abstract Task<bool> ConfirmEmail(string token);
    }
}
