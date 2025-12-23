using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GestionRH.Models
{
    public class Employe : Utilisateur
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "Poste")]
        public string Poste { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Salaire")]
        public decimal Salaire { get; set; }

        [Display(Name = "Manager")]
        public string? ManagerId { get; set; }

        [Display(Name = "Solde de congés (jours)")]
        public int SoldeConges { get; set; } = 18;

        [Display(Name = "Département")]
        public int? DepartementId { get; set; }

        [ForeignKey("DepartementId")]
        public virtual Departement? Departement { get; set; }

        // Relations
        // Relations
        public virtual ICollection<Paie> Paies { get; set; }

        public Employe()
        {
            Paies = new HashSet<Paie>();
            Role = "Employe";
        }

        public void DemanderConge(Conge conge) => Conges.Add(conge);
        public List<Conge> ConsulterEtatConges() => Conges.ToList();
        public List<Paie> ConsulterBulletinPaie() => Paies.ToList();
        public string ConsulterProfil() => $"{NomComplet} - {Poste} - {Salaire:C}";
    }
}