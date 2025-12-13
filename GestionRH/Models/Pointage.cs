using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionRH.Models
{
    public class Pointage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EmployeId { get; set; }

        [ForeignKey("EmployeId")]
        public virtual Employe Employe { get; set; }

        [Required]
        [Display(Name = "Date de pointage")]
        [DataType(DataType.Date)]
        public DateTime DatePointage { get; set; } = DateTime.Today;

        [Display(Name = "Heure d'arrivée")]
        [DataType(DataType.Time)]
        public DateTime? HeureArrivee { get; set; }

        [Display(Name = "Heure de départ")]
        [DataType(DataType.Time)]
        public DateTime? HeureDepart { get; set; }

        [Display(Name = "Durée de travail")]
        public TimeSpan? DureeTravail { get; set; }

        [Display(Name = "Heures supplémentaires")]
        public decimal? HeuresSupplementaires { get; set; }

        [Display(Name = "Est absent")]
        public bool EstAbsent { get; set; } = false;

        [MaxLength(200)]
        [Display(Name = "Remarques")]
        public string? Remarques { get; set; }
    }
}

