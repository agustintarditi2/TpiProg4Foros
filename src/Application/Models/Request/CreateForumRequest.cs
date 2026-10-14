using System.ComponentModel.DataAnnotations;

namespace Application.Models.Requests;

// representa los datos necesarios para crear un foro
public record CreateForumRequest
(
    [Required(ErrorMessage = "El titulo es obligatorio")]
    string Title,

    [Required(ErrorMessage = "La descripcion es obligatoria")]
    string Description
);