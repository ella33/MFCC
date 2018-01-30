namespace ProiectMFCC.Models.Entities
{
    public class Country
    {
        public Country(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }
}