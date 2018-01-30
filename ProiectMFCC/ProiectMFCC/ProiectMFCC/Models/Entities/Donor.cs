namespace ProiectMFCC.Models.Entities
{
    public class Donor
    {

        public Donor() { }

        public Donor(string name, string country)
        {
            Name = name;
            Country = country;
        }

        public Donor(int id, string name, string country)
        {
            Id = id;
            Name = name;
            Country = country;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }        
    }
}