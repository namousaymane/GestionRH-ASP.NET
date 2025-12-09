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
        public string Poste { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Salaire")]
        public decimal Salaire { get; set; }

        // --- C'est cette ligne qui manque pour que le build fonctionne ---
        [Display(Name = "Manager")]
        public string? ManagerId { get; set; }
        // ----------------------------------------------------------------

        // Relations
        public virtual ICollection<Conge> Conges { get; set; }
        public virtual ICollection<Paie> Paies { get; set; }

        public Employe()
        {
            Conges = new HashSet<Conge>();
            Paies = new HashSet<Paie>();
            Role = "Employe";
        }

        public void DemanderConge(Conge conge) => Conges.Add(conge);
        public List<Conge> ConsulterEtatConges() => Conges.ToList();
        public List<Paie> ConsulterBulletinPaie() => Paies.ToList();
        public string ConsulterProfil() => $"{NomComplet} - {Poste} - {Salaire:C}";
    }
}