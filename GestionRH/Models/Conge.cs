using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionRH.Models
{
    public class Conge
    {
        [Key]
        public int IdConge { get; set; }

        [Required]
        [Display(Name = "Date de début")]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }

        [Required]
        [Display(Name = "Date de fin")]
        [DataType(DataType.Date)]
        public DateTime DateFin { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Type")]
        public string Type { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Statut")]
        public string Statut { get; set; } = "EnAttente";

        // Relation avec Employe
        [Required]
        public string EmployeId { get; set; }

        [ForeignKey("EmployeId")]
        public virtual Employe Employe { get; set; }

        // Méthodes
        public int CalculerDuree()
        {
            return (DateFin - DateDebut).Days + 1;
        }

        public void Valider()
        {
            Statut = "Approuve";
        }

        public void Refuser()
        {
            Statut = "Rejete";
        }

        [Display(Name = "Durée (jours)")]
        [NotMapped]
        public int Duree => CalculerDuree();
    }
}