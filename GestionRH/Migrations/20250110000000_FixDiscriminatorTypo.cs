using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionRH.Migrations
{
    /// <inheritdoc />
    public partial class FixDiscriminatorTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Corriger la faute de frappe dans le discriminator
            migrationBuilder.Sql(@"
                UPDATE AspNetUsers 
                SET TypeUtilisateur = 'AdministrateurRH' 
                WHERE TypeUtilisateur = 'AdmnistrateurRH';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revenir en arrière si nécessaire
            migrationBuilder.Sql(@"
                UPDATE AspNetUsers 
                SET TypeUtilisateur = 'AdmnistrateurRH' 
                WHERE TypeUtilisateur = 'AdministrateurRH';
            ");
        }
    }
}

