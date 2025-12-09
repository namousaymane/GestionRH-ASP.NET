using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GestionRH.Models
{
    public class Utilisateur : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Display(Name = "Rôle")]
        public string Role { get; set; } = "Employe"; // Par défaut

        // Méthodes
        public virtual bool SAuthentifier(string email, string motDePasse)
        {
            // Implémenté par Identity
            return true;
        }

        public virtual void SeDeconnecter()
        {
            // Implémenté par Identity
        }

        [Display(Name = "Nom complet")]
        public string NomComplet => $"{Prenom} {Nom}".Trim();
    }
}