namespace GestionRH.Models.Enums
{
    public enum StatutConge
    {
        EnAttente = 1,
        ApprouveManager = 2,
        ApprouveRH = 3,
        RejeteManager = 4,
        RejeteRH = 5,
        Annule = 6
    }

    public enum TypeConge
    {
        CongeAnnuel = 1,
        CongeMaladie = 2,
        CongeMaternite = 3,
        CongePaternite = 4,
        CongeSansSolde = 5,
        CongeExceptionnel = 6
    }

    public enum RoleUtilisateur
    {
        Administrateur = 1,
        GestionnaireRH = 2,
        Manager = 3,
        Employe = 4
    }

    public enum TypeLignePaie
    {
        Gain = 1,
        Retenue = 2
    }
}