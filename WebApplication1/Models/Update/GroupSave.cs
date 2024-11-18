namespace WebApplication1.Models.Update
{
    public class GroupSave
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<System> Systems { get; set; }

        public ICollection<Permission> Permissions { get; set; }
    }
}
