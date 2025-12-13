using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionRH.Models
{
    public class Contrat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EmployeId { get; set; }

        [ForeignKey("EmployeId")]
        public virtual Employe Employe { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Type de contrat")]
        public string TypeContrat { get; set; } // "CDI", "CDD", "Stage", "Freelance"

        [Required]
        [Display(Name = "Date de début")]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }

        [Display(Name = "Date de fin")]
        [DataType(DataType.Date)]
        public DateTime? DateFin { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Salaire brut")]
        public decimal SalaireBrut { get; set; }

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [MaxLength(500)]
        [Display(Name = "Chemin du fichier")]
        public string? FichierContrat { get; set; } // Chemin du PDF

        [Display(Name = "Est actif")]
        public bool EstActif { get; set; } = true;
    }
}

