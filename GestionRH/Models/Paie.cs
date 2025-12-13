using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionRH.Models
{
    public class Paie
    {
        [Key]
        public int IdPaie { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Montant")]
        public decimal Montant { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Mois")]
        public string Mois { get; set; }

        [Required]
        [Display(Name = "Date d'émission")]
        [DataType(DataType.Date)]
        public DateTime DateEmission { get; set; } = DateTime.Now;

        // Relation avec Employe
        [Required]
        public string EmployeId { get; set; }

        [ForeignKey("EmployeId")]
        public virtual Employe Employe { get; set; }

        // Relation avec LignesPaie
        public virtual ICollection<LignePaie> LignesPaie { get; set; } = new List<LignePaie>();

        // Méthodes
        public decimal CalculerSalaire()
        {
            // Logique de calcul à implémenter
            return Montant;
        }

        public string AfficherPaie()
        {
            return $"Paie de {Mois} - {Montant:C}";
        }
    }
}