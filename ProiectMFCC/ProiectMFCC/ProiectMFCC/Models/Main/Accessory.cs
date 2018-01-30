using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProiectMFCC.Models.Main
{
    public class Accessory : Item
    {
        public Accessory(string name, double price, string manufacturerName, string instrumentName)
        {
            Name = name;
            Price = price;
            ManufacturerName = manufacturerName;
            InstrumentName = instrumentName;
        }

        public string Name { get; set; }
        public double Price { get; set; }
        public string ManufacturerName { get; set; }
        public string InstrumentName { get; set; }
    }
}