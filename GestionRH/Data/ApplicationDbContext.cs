using GestionRH.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace GestionRH.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilisateur>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Employe> Employes { get; set; }
        public DbSet<Responsable> Responsables { get; set; }
        public DbSet<AdministrateurRH> AdministrateursRH { get; set; }
        public DbSet<Conge> Conges { get; set; }
        public DbSet<Paie> Paies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Réduire les longueurs Identity pour éviter l'erreur MySQL
            modelBuilder.Entity<Utilisateur>(entity =>
            {
                // Limit key columns so indexes fit MySQL limits with utf8mb4
                entity.Property(u => u.Id).HasMaxLength(100);
                entity.Property(u => u.NormalizedEmail).HasMaxLength(100);
                entity.Property(u => u.NormalizedUserName).HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(100);
                entity.Property(u => u.UserName).HasMaxLength(100);
            });

            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.Property(r => r.Id).HasMaxLength(100);
                entity.Property(r => r.Name).HasMaxLength(100);
                entity.Property(r => r.NormalizedName).HasMaxLength(100);
            });

            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.Property(ur => ur.UserId).HasMaxLength(100);
                entity.Property(ur => ur.RoleId).HasMaxLength(100);
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.HasKey(l => new { l.LoginProvider, l.ProviderKey });
                entity.Property(l => l.LoginProvider).HasMaxLength(50);
                entity.Property(l => l.ProviderKey).HasMaxLength(50);
                entity.Property(l => l.UserId).HasMaxLength(50);
            });

            modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

                entity.Property(t => t.UserId).HasMaxLength(100);

                // Reduce to avoid exceeding MySQL’s key length limit
                entity.Property(t => t.LoginProvider).HasMaxLength(50);
                entity.Property(t => t.Name).HasMaxLength(50);
            });


            modelBuilder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.Property(uc => uc.UserId).HasMaxLength(100);
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.Property(rc => rc.RoleId).HasMaxLength(100);
            });

            // TPH pour Utilisateur
            modelBuilder.Entity<Utilisateur>()
                .HasDiscriminator<string>("TypeUtilisateur")
                .HasValue<Utilisateur>("Utilisateur")
                .HasValue<Employe>("Employe")
                .HasValue<Responsable>("Responsable")
                .HasValue<AdministrateurRH>("AdministrateurRH");

            // Conge et Paie
            modelBuilder.Entity<Conge>(entity =>
            {
                entity.HasKey(c => c.IdConge);
                entity.HasOne(c => c.Employe)
                    .WithMany(e => e.Conges)
                    .HasForeignKey(c => c.EmployeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(c => c.Type).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Statut).HasDefaultValue("EnAttente").HasMaxLength(50);
                entity.Property(c => c.EmployeId).HasMaxLength(100);
            });

            modelBuilder.Entity<Paie>(entity =>
            {
                entity.HasKey(p => p.IdPaie);
                entity.HasOne(p => p.Employe)
                    .WithMany(e => e.Paies)
                    .HasForeignKey(p => p.EmployeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(p => p.Montant).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(p => p.EmployeId).HasMaxLength(100);
            });

            modelBuilder.Entity<Conge>().HasIndex(c => c.Statut);
            modelBuilder.Entity<Paie>().HasIndex(p => p.Mois);
        }


    }
}