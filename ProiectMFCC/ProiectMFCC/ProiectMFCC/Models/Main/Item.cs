using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProiectMFCC.Models.Main
{
    public class Item
    {
        public Item() { }

        public Item(int id, string type, int cartId, string name)
        {
            Id = id;
            Type = type;
            CartId = cartId;
            Name = name;
        }

        public int Id { get; set; }
        public string Type { get; set; }
        public int CartId { get; set; }
        public virtual string Name { get; set; }
    }
}