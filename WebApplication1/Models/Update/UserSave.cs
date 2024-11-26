using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Update
{
    public class UserSave
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        public string? Email { get; set; }

        // Lista opcional de IDs de grupos para associar ao usuário
        public List<int> GroupIds { get; set; } = new List<int>();
    }
}
