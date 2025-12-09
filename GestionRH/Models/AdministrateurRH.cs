using System.Collections.Generic;

namespace GestionRH.Models
{
    public class AdministrateurRH : Utilisateur
    {
        public AdministrateurRH()
        {
            Role = "AdministrateurRH";
        }

        // Méthodes
        public void AjouterEmploye(Employe emp)
        {
            // À implémenter via le service
        }

        public void ModifierEmploye(Employe emp)
        {
            // À implémenter via le service
        }

        public void SupprimerEmploye(int empId)
        {
            // À implémenter via le service
        }

        public void GenererPaie(Employe emp)
        {
            // À implémenter via le service
        }

        public List<Conge> ConsulterToutesDemandes()
        {
            // À implémenter via le service
            return new List<Conge>();
        }
    }
}