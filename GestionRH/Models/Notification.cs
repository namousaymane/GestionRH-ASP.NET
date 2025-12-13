using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionRH.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Utilisateur User { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titre { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } // "Conge", "Paie", "Employe", etc.

        public bool EstLue { get; set; } = false;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public string? LienAction { get; set; } // URL pour rediriger l'utilisateur
    }
}

