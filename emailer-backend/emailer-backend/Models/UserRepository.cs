using emailer_backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace emailer_backend.Controllers
{
    public class UserRepository : IDisposable
    {
        private EmailerEntities context = new EmailerEntities();

        public User ValidateUser(string email, string password)
        {
            return context.Users.FirstOrDefault(user => user.Email == email && user.Password == password);
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}