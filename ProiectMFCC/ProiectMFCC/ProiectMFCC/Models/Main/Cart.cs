using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProiectMFCC.Models.Main
{
    public class Cart
    {
        public Cart() { Items = new List<Item>(); }

        public Cart(int id, double totalPrice)
        {
            Id = id;
            TotalPrice = totalPrice;
            Items = new List<Item>();
        }

        public int Id { get; set; }
        public double TotalPrice { get; set; }
        public List<Item> Items { get; set; }
    }
}