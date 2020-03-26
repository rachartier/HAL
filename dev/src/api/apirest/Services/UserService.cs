using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apirest.Extensions;
using Entities.Models;

namespace apirest.Services
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password);
        Task<IEnumerable<User>> GetAll();
    }
    public class UserService : IUserService
    {
        private List<User> users = new List<User>
        {
            new User {Id = 1, Name = "admin", Password = "admin"},
        };

        public async Task<User> Authenticate(string username, string password)
        {
            var user = await Task.Run(() =>
                users.SingleOrDefault(
                    u => u.Name.Equals(username) && u.Password.Equals(password)
                )
            );

            return user?.WithoutPassword();
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await Task.Run(() => users.WithoutPasswords());
        } 
    }
}