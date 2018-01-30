namespace ProiectMFCC.Models.Entities
{
    public class Project
    {
        public Project(string name, string description) {
            Name = name;
            Description = description;
        }

        public Project(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;           
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}