using System.ComponentModel.DataAnnotations;

namespace Application.Models.Requests;

// representa los datos necesarios para modificar un foro
public class UpdateForumRequest
{
    [Required(ErrorMessage = "El titulo es obligatorio")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripcion es obligatoria")]
    public string Description { get; set; } = string.Empty;
}