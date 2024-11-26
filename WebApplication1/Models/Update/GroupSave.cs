namespace WebApplication1.Models.Update
{
    public class GroupSave
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ICollection<int>? Systems { get; set; }

        public ICollection<int>? Permissions { get; set; }
    }
}
