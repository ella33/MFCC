using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProiectMFCC.Models.Main
{
    public class Client
    {
        public Client()
        {
                
        }

        public Client(string name, string email, int cartID)
        {
            Name = name;
            Email = email;
            CartId = cartID;
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public int CartId { get; set; }
    }
}