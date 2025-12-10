-- Script SQL pour remplir les tables de la base de données GestionRH
-- Base de données: GestionRH_DB
-- SGBD: MySQL

USE GestionRH_DB;

-- 
-- 1. INSERTION DANS AspNetUsers
-- 
-- Note: Les utilisateurs utilisent Identity, donc il faut générer des IDs et des hash de mot de passe
-- Pour simplifier, on utilise des IDs simples et des hash basiques
-- En production, utilisez UserManager pour créer les utilisateurs avec des mots de passe hashés correctement

-- Insertion d'un AdministrateurRH
INSERT INTO AspNetUsers (
    Id, Nom, Prenom, Role, TypeUtilisateur, Poste, Salaire,
    UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount
) VALUES (
    'admin-001',
    'Dupont',
    'Jean',
    'AdministrateurRH',
    'AdministrateurRH',
    NULL,
    NULL,
    'admin@gestrh.com',
    'ADMIN@GESTRH.COM',
    'admin@gestrh.com',
    'ADMIN@GESTRH.COM',
    1,
    'AQAAAAIAAYagAAAAEExampleHash1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ', -- Hash exemple (à remplacer par un vrai hash)
    'SecurityStamp123',
    'ConcurrencyStamp123',
    NULL,
    0,
    0,
    1,
    0
);

-- Insertion de Responsables
INSERT INTO AspNetUsers (
    Id, Nom, Prenom, Role, TypeUtilisateur, Poste, Salaire,
    UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount
) VALUES 
(
    'resp-001',
    'Martin',
    'Sophie',
    'Responsable',
    'Responsable',
    NULL,
    NULL,
    'sophie.martin@gestrh.com',
    'SOPHIE.MARTIN@GESTRH.COM',
    'sophie.martin@gestrh.com',
    'SOPHIE.MARTIN@GESTRH.COM',
    1,
    'AQAAAAIAAYagAAAAEExampleHash1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ',
    'SecurityStamp124',
    'ConcurrencyStamp124',
    NULL,
    0,
    0,
    1,
    0
),
(
    'resp-002',
    'Bernard',
    'Pierre',
    'Responsable',
    'Responsable',
    NULL,
    NULL,
    'pierre.bernard@gestrh.com',
    'PIERRE.BERNARD@GESTRH.COM',
    'pierre.bernard@gestrh.com',
    'PIERRE.BERNARD@GESTRH.COM',
    1,
    'AQAAAAIAAYagAAAAEExampleHash1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ',
    'SecurityStamp125',
    'ConcurrencyStamp125',
    NULL,
    0,
    0,
    1,
    0
);

-- Insertion d'Employes
INSERT INTO AspNetUsers (
    Id, Nom, Prenom, Role, TypeUtilisateur, Poste, Salaire, ManagerId,
    UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount
) VALUES 
(
    'emp-001',
    'Durand',
    'Marie',
    'Employe',
    'Employe',
    'Développeur',
    3500.00,
    'resp-001',
    'marie.durand@gestrh.com',
    'MARIE.DURAND@GESTRH.COM',
    'marie.durand@gestrh.com',
    'MARIE.DURAND@GESTRH.COM',
    1,
    'AQAAAAIAAYagAAAAEExampleHash1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ',
    'SecurityStamp126',
    'ConcurrencyStamp126',
    NULL,
    0,
    0,
    1,
    0
),
(
    'emp-002',
    'Lefebvre',
    'Thomas',
    'Employe',
    'Employe',
    'Analyste',
    3200.00,
    'resp-001',
    'thomas.lefebvre@gestrh.com',
    'THOMAS.LEFEBVRE@GESTRH.COM',
    'thomas.lefebvre@gestrh.com',
    'THOMAS.LEFEBVRE@GESTRH.COM',
    1,
    'AQAAAAIAAYagAAAAEExampleHash1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ',
    'SecurityStamp127',
    'ConcurrencyStamp127',
    NULL,
    0,
    0,
    1,
    0
),
(
    'emp-003',
    'Moreau',
    'Julie',
    'Employe',
    'Employe',
    'Designer',
    3000.00,
    'resp-002',
    'julie.moreau@gestrh.com',
    'JULIE.MOREAU@GESTRH.COM',
    'julie.moreau@gestrh.com',
    'JULIE.MOREAU@GESTRH.COM',
    1,
    'AQAAAAIAAYagAAAAEExampleHash1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ',
    'SecurityStamp128',
    'ConcurrencyStamp128',
    NULL,
    0,
    0,
    1,
    0
),
(
    'emp-004',
    'Petit',
    'Lucas',
    'Employe',
    'Employe',
    'Développeur Senior',
    4200.00,
    'resp-002',
    'lucas.petit@gestrh.com',
    'LUCAS.PETIT@GESTRH.COM',
    'lucas.petit@gestrh.com',
    'LUCAS.PETIT@GESTRH.COM',
    1,
    'AQAAAAIAAYagAAAAEExampleHash1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ',
    'SecurityStamp129',
    'ConcurrencyStamp129',
    NULL,
    0,
    0,
    1,
    0
),
(
    'emp-005',
    'Garcia',
    'Emma',
    'Employe',
    'Employe',
    'Chef de Projet',
    4000.00,
    'resp-001',
    'emma.garcia@gestrh.com',
    'EMMA.GARCIA@GESTRH.COM',
    'emma.garcia@gestrh.com',
    'EMMA.GARCIA@GESTRH.COM',
    1,
    'AQAAAAIAAYagAAAAEExampleHash1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ',
    'SecurityStamp130',
    'ConcurrencyStamp130',
    NULL,
    0,
    0,
    1,
    0
);

-- ============================================
-- 2. INSERTION DANS Conges
-- ============================================
INSERT INTO Conges (DateDebut, DateFin, Type, Statut, EmployeId) VALUES
('2024-12-15', '2024-12-20', 'CongesPayes', 'EnAttente', 'emp-001'),
('2024-12-22', '2024-12-27', 'CongesPayes', 'Approuve', 'emp-002'),
('2024-12-10', '2024-12-12', 'Maladie', 'Approuve', 'emp-003'),
('2024-12-18', '2024-12-19', 'CongesPayes', 'Rejete', 'emp-004'),
('2024-12-25', '2025-01-05', 'CongesPayes', 'EnAttente', 'emp-005'),
('2024-12-01', '2024-12-05', 'CongesPayes', 'ApprouveManager', 'emp-001'),
('2024-12-08', '2024-12-10', 'Maladie', 'Approuve', 'emp-002'),
('2024-12-28', '2024-12-31', 'CongesPayes', 'EnAttente', 'emp-003'),
('2025-01-10', '2025-01-15', 'CongesPayes', 'EnAttente', 'emp-004'),
('2025-01-20', '2025-01-25', 'CongesPayes', 'EnAttente', 'emp-005');

-- ============================================
-- 3. INSERTION DANS Paies
-- ============================================
INSERT INTO Paies (Montant, Mois, DateEmission, EmployeId) VALUES
(3500.00, 'Novembre 2024', '2024-11-30', 'emp-001'),
(3200.00, 'Novembre 2024', '2024-11-30', 'emp-002'),
(3000.00, 'Novembre 2024', '2024-11-30', 'emp-003'),
(4200.00, 'Novembre 2024', '2024-11-30', 'emp-004'),
(4000.00, 'Novembre 2024', '2024-11-30', 'emp-005'),
(3500.00, 'Octobre 2024', '2024-10-31', 'emp-001'),
(3200.00, 'Octobre 2024', '2024-10-31', 'emp-002'),
(3000.00, 'Octobre 2024', '2024-10-31', 'emp-003'),
(4200.00, 'Octobre 2024', '2024-10-31', 'emp-004'),
(4000.00, 'Octobre 2024', '2024-10-31', 'emp-005'),
(3500.00, 'Septembre 2024', '2024-09-30', 'emp-001'),
(3200.00, 'Septembre 2024', '2024-09-30', 'emp-002'),
(3000.00, 'Septembre 2024', '2024-09-30', 'emp-003'),
(4200.00, 'Septembre 2024', '2024-09-30', 'emp-004'),
(4000.00, 'Septembre 2024', '2024-09-30', 'emp-005');

-- ============================================
-- VÉRIFICATION DES DONNÉES INSÉRÉES
-- ============================================
-- Vous pouvez exécuter ces requêtes pour vérifier les insertions

-- SELECT COUNT(*) as TotalUsers FROM AspNetUsers;
-- SELECT COUNT(*) as TotalConges FROM Conges;
-- SELECT COUNT(*) as TotalPaies FROM Paies;

-- SELECT * FROM AspNetUsers WHERE TypeUtilisateur = 'Employe';
-- SELECT * FROM AspNetUsers WHERE TypeUtilisateur = 'Responsable';
-- SELECT * FROM AspNetUsers WHERE TypeUtilisateur = 'AdministrateurRH';
-- SELECT * FROM Conges;
-- SELECT * FROM Paies;

