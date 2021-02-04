using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace emailer_backend.Models
{
    public class SaveChangesRequest
    {
        public string Message { get; set; }
        public string Topic { get; set; }
    }
}