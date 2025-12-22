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
        public DbSet<LignePaie> LignesPaie { get; set; }
        public DbSet<Departement> Departements { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Pointage> Pointages { get; set; }
        public DbSet<Contrat> Contrats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1. FIX MYSQL : Réduire les longueurs des clés Identity ---
            modelBuilder.Entity<Utilisateur>(entity =>
            {
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

            // --- 2. CONFIGURATION TPH (Héritage) ---
            modelBuilder.Entity<Utilisateur>()
                .HasDiscriminator<string>("TypeUtilisateur")
                .HasValue<Utilisateur>("Utilisateur")
                .HasValue<Employe>("Employe")
                .HasValue<Responsable>("Responsable")
                .HasValue<AdministrateurRH>("AdministrateurRH");

            // --- 3. CONFIGURATION RH (Congés & Paie) ---
            modelBuilder.Entity<Conge>(entity =>
            {
                entity.HasKey(c => c.IdConge);
                entity.HasOne(c => c.Employe)
                    .WithMany(u => u.Conges) // Utilisateur.Conges
                    .HasForeignKey(c => c.EmployeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(c => c.Type).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Statut).HasDefaultValue("EnAttente").HasMaxLength(50);
                entity.Property(c => c.EmployeId).HasMaxLength(100);
                entity.HasIndex(c => c.Statut);
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
                entity.HasIndex(p => p.Mois);
            });

            // Configuration LignePaie
            modelBuilder.Entity<LignePaie>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.HasOne(l => l.Paie)
                    .WithMany()
                    .HasForeignKey(l => l.PaieId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(l => l.Montant).HasColumnType("decimal(18,2)").IsRequired();
            });

            // Configuration Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(n => n.UserId).HasMaxLength(100);
                entity.HasIndex(n => new { n.UserId, n.EstLue });
            });

            // Configuration Pointage
            modelBuilder.Entity<Pointage>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasOne(p => p.Employe)
                    .WithMany()
                    .HasForeignKey(p => p.EmployeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(p => p.EmployeId).HasMaxLength(100);
                entity.HasIndex(p => new { p.EmployeId, p.DatePointage });
            });

            // Configuration Contrat
            modelBuilder.Entity<Contrat>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasOne(c => c.Employe)
                    .WithMany()
                    .HasForeignKey(c => c.EmployeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(c => c.EmployeId).HasMaxLength(100);
                entity.Property(c => c.SalaireBrut).HasColumnType("decimal(18,2)").IsRequired();
                entity.HasIndex(c => new { c.EmployeId, c.EstActif });
            });

            // --- 4. CONFIGURATION DEPARTEMENT (C'est ce bloc qui manquait !) ---

            // Relation A : Un employé appartient à un département
            modelBuilder.Entity<Employe>()
                .HasOne(e => e.Departement)
                .WithMany(d => d.Employes)
                .HasForeignKey(e => e.DepartementId)
                .OnDelete(DeleteBehavior.SetNull); // Si on supprime le département, l'employé reste (sans département)

            // Relation B : Un département est dirigé par un chef (Employe)
            modelBuilder.Entity<Departement>()
                .HasOne(d => d.Chef)
                .WithMany() // Pas de liste "DepartementsDiriges" sur l'employé, on laisse vide
                .HasForeignKey(d => d.ChefId)
                .OnDelete(DeleteBehavior.SetNull); // Si le chef part, le département n'a plus de chef
        }
    }
}