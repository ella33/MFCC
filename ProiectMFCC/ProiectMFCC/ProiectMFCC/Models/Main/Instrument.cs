using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProiectMFCC.Models.Main
{
    public class Instrument : Item
    {
        public Instrument(string name, string category, double price, string manufacutrerName)
        {
            Name = name;
            Category = category;
            Price = price;
            ManufacutrerName = manufacutrerName;
        }

        public override string Name { get; set; }
        public string Category { get; set; }
        public double Price { get; set; }
        public string ManufacutrerName { get; set; }
    }
}