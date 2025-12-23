using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionRH.Models
{
    public class Departement
    {
        [Key]
        public int DepartementId { get; set; }

        [Required]
        [Display(Name = "Nom du département")]
        public string Nom { get; set; } = null!;

        // Le chef du département (Manager)
        [Display(Name = "Responsable du département")]
        public string? ChefId { get; set; }

        [ForeignKey("ChefId")]
        public virtual Utilisateur? Chef { get; set; }

        // Liste des employés dans ce département
        public virtual ICollection<Employe> Employes { get; set; } = new List<Employe>();
    }
}