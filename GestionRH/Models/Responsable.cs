using System.Collections.Generic;
using System.Linq;

namespace GestionRH.Models
{
    public class Responsable : Utilisateur
    {
        public Responsable()
        {
            Role = "Responsable";
        }

        // Méthodes
        public List<Conge> ConsulterDemandes()
        {
            // À implémenter via le service
            return new List<Conge>();
        }

        public void ValiderDemande(Conge conge)
        {
            conge.Statut = "ApprouveManager";
        }

        public void RefuserDemande(Conge conge)
        {
            conge.Statut = "RejeteManager";
        }
    }
}