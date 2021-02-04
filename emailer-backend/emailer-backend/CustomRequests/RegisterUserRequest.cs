using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace emailer_backend.Models
{
    public class RegisterUserRequest
    {
        public string email { get; set; }
        public string password { get; set; }
        public string code { get; set; }
    }
}