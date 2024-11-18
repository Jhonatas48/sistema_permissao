using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class Group
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Actived { get; set; }
        [JsonIgnore]
        public ICollection<User> Users { get; set; }

        public ICollection<System> Systems { get; set; }

        public ICollection<Permission> Permissions { get; set; }
        
    }
}
