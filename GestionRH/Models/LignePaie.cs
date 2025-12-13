using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionRH.Models
{
    public class LignePaie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PaieId { get; set; }

        [ForeignKey("PaieId")]
        public virtual Paie Paie { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Libellé")]
        public string Libelle { get; set; } // "Salaire de base", "Prime", "CNSS", etc.

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Montant")]
        public decimal Montant { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Type")]
        public string Type { get; set; } // "Gain" ou "Retenue"

        [Display(Name = "Ordre d'affichage")]
        public int Ordre { get; set; } = 0; // Pour ordonner les lignes dans le bulletin
    }
}

