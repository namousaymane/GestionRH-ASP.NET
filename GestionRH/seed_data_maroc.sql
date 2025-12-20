-- Script SQL pour remplir la base de données avec des données marocaines
-- Mot de passe par défaut pour tous les utilisateurs : "Password123!"

USE GestionRH_DB;

-- Hash PBKDF2 pour "Password123!" (utilisé pour tous les utilisateurs)
SET @passwordHash = 'AQAAAAEAACcQAAAAEHyZ3O3+5BX1qhcH7yguBxMtzFHp3f7I7urZ4yE5vJ8K9mN0pQrS6tUvWxYzA1B2C3D4==';

-- 1. ROLES
INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES
('role1', 'Employe', 'EMPLOYE', UUID()),
('role2', 'Responsable', 'RESPONSABLE', UUID()),
('role3', 'AdministrateurRH', 'ADMINISTRATEURRH', UUID());

-- 2. ADMINISTRATEURS RH
INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, Nom, Prenom, Role, TypeUtilisateur) VALUES
('admin1', 'admin.rh@company.ma', 'ADMIN.RH@COMPANY.MA', 'admin.rh@company.ma', 'ADMIN.RH@COMPANY.MA', 1, @passwordHash, UUID(), UUID(), '0612345678', 1, 0, 0, 0, 'Alaoui', 'Fatima', 'AdministrateurRH', 'AdministrateurRH'),
('admin2', 'admin2.rh@company.ma', 'ADMIN2.RH@COMPANY.MA', 'admin2.rh@company.ma', 'ADMIN2.RH@COMPANY.MA', 1, @passwordHash, UUID(), UUID(), '0623456789', 1, 0, 0, 0, 'Benali', 'Ahmed', 'AdministrateurRH', 'AdministrateurRH');

INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES
('admin1', 'role3'),
('admin2', 'role3');

-- 3. RESPONSABLES
INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, Nom, Prenom, Role, TypeUtilisateur) VALUES
('resp1', 'm.alami@company.ma', 'M.ALAMI@COMPANY.MA', 'm.alami@company.ma', 'M.ALAMI@COMPANY.MA', 1, @passwordHash, UUID(), UUID(), '0634567890', 1, 0, 0, 0, 'Alami', 'Mohammed', 'Responsable', 'Responsable'),
('resp2', 'a.bennani@company.ma', 'A.BENNANI@COMPANY.MA', 'a.bennani@company.ma', 'A.BENNANI@COMPANY.MA', 1, @passwordHash, UUID(), UUID(), '0645678901', 1, 0, 0, 0, 'Bennani', 'Aicha', 'Responsable', 'Responsable'),
('resp3', 'h.berrada@company.ma', 'H.BERRADA@COMPANY.MA', 'h.berrada@company.ma', 'H.BERRADA@COMPANY.MA', 1, @passwordHash, UUID(), UUID(), '0656789012', 1, 0, 0, 0, 'Berrada', 'Hassan', 'Responsable', 'Responsable');

INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES
('resp1', 'role2'),
('resp2', 'role2'),
('resp3', 'role2');

-- 4. DÉPARTEMENTS
INSERT INTO Departements (Nom, ChefId) VALUES
('Ressources Humaines', 'resp1'),
('Informatique', 'resp2'),
('Comptabilité', 'resp3'),
('Marketing', NULL),
('Ventes', NULL);

-- 5. EMPLOYÉS
INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, Nom, Prenom, Role, TypeUtilisateur, Poste, Salaire, ManagerId, SoldeConges, DepartementId) VALUES
('emp1', 'y.elamrani@company.ma', 'Y.ELAMRANI@COMPANY.MA', 'y.elamrani@company.ma', 'Y.ELAMRANI@COMPANY.MA', 1, @passwordHash, UUID(), UUID(), '0667890123', 1, 0, 0, 0, 'Elamrani', 'Youssef', 'Employe', 'Employe', 'Développeur Senior', 15000.00, 'resp2', 20, 2),
('emp2', 's.idrissi@company.ma', 'S.IDRISSI@COMPANY.MA', 's.idrissi@company.ma', 'S.IDRISSI@COMPANY.MA', 1, @passwordHash, UUID(), UUID(), '0678901234', 1, 0, 0, 0, 'Idrissi', 'Sanae', 'Employe', 'Employe', 'Développeuse', 12000.00, 'resp2', 18, 2),
('emp3', 'k.tazi@company.ma', 'K.TAZI@COMPANY.MA', 'k.tazi@company.ma', 'K.TAZI@COMPANY.MA', 1, @passwordHash, UUID(), UUID(), '0689012345', 1, 0, 0, 0, 'Tazi', 'Karim', 'Employe', 'Employe', 'Comptable', 11000.00, 'resp3', 22, 3),
('emp4', 'l.ouazzani@company.ma', 'L.OUAZZANI@COMPANY.MA', 'l.ouazzani@company.ma', 'L.OUAZZANI@COMPANY.MA', 1, @passwordHash, UUID(), UUID(), '0690123456', 1, 0, 0, 0, 'Ouazzani', 'Laila', 'Employe', 'Employe', 'Responsable RH', 13000.00, 'resp1', 19, 1),
('emp5', 'm.chraibi@company.ma', 'M.CHRAIBI@COMPANY.MA', 'm.chraibi@company.ma', 'M.CHRAIBI@COMPANY.MA', 1, @passwordHash, UUID(), UUID(), '0601234567', 1, 0, 0, 0, 'Chraibi', 'Mehdi', 'Employe', 'Employe', 'Chef de Projet', 14000.00, 'resp2', 21, 2),
('emp6', 'n.bensaid@company.ma', 'N.BENSAID@COMPANY.MA', 'n.bensaid@company.ma', 'N.BENSAID@COMPANY.MA', 1, @passwordHash, UUID(), UUID(), '0612345678', 1, 0, 0, 0, 'Bensaid', 'Nadia', 'Employe', 'Employe', 'Marketing Manager', 12500.00, NULL, 20, 4),
('emp7', 'o.filali@company.ma', 'O.FILALI@COMPANY.MA', 'o.filali@company.ma', 'O.FILALI@COMPANY.MA', 1, @passwordHash, UUID(), UUID(), '0623456789', 1, 0, 0, 0, 'Filali', 'Omar', 'Employe', 'Employe', 'Commercial', 10000.00, NULL, 18, 5),
('emp8', 'r.mezouar@company.ma', 'R.MEZOUAR@COMPANY.MA', 'r.mezouar@company.ma', 'R.MEZOUAR@COMPANY.MA', 1, @passwordHash, UUID(), UUID(), '0634567890', 1, 0, 0, 0, 'Mezouar', 'Rachid', 'Employe', 'Employe', 'Développeur Junior', 9000.00, 'resp2', 22, 2);

INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES
('emp1', 'role1'), ('emp2', 'role1'), ('emp3', 'role1'), ('emp4', 'role1'),
('emp5', 'role1'), ('emp6', 'role1'), ('emp7', 'role1'), ('emp8', 'role1');

-- 6. CONTRATS
INSERT INTO Contrats (EmployeId, TypeContrat, DateDebut, DateFin, SalaireBrut, Description, EstActif) VALUES
('emp1', 'CDI', '2023-01-15', NULL, 15000.00, 'Contrat à durée indéterminée - Développeur Senior', 1),
('emp2', 'CDI', '2023-03-01', NULL, 12000.00, 'Contrat à durée indéterminée - Développeuse', 1),
('emp3', 'CDI', '2022-06-10', NULL, 11000.00, 'Contrat à durée indéterminée - Comptable', 1),
('emp4', 'CDI', '2023-02-20', NULL, 13000.00, 'Contrat à durée indéterminée - Responsable RH', 1),
('emp5', 'CDI', '2023-05-01', NULL, 14000.00, 'Contrat à durée indéterminée - Chef de Projet', 1),
('emp6', 'CDD', '2024-01-01', '2024-12-31', 12500.00, 'Contrat à durée déterminée - Marketing', 1),
('emp7', 'CDI', '2023-09-15', NULL, 10000.00, 'Contrat à durée indéterminée - Commercial', 1),
('emp8', 'Stage', '2024-06-01', '2024-12-31', 5000.00, 'Stage de fin d''études - Développeur', 1);

-- 7. CONGÉS
INSERT INTO Conges (EmployeId, DateDebut, DateFin, Type, Statut) VALUES
('emp1', '2024-07-15', '2024-07-20', 'Annuel', 'Approuve'),
('emp2', '2024-08-01', '2024-08-05', 'Annuel', 'EnAttente'),
('emp3', '2024-06-10', '2024-06-12', 'Maladie', 'Approuve'),
('emp4', '2024-09-01', '2024-09-10', 'Annuel', 'ApprouveManager'),
('emp5', '2024-07-25', '2024-07-30', 'Annuel', 'RejeteManager'),
('emp6', '2024-08-15', '2024-08-20', 'Annuel', 'EnAttente'),
('emp1', '2024-12-20', '2024-12-31', 'Annuel', 'EnAttente'),
('emp3', '2024-10-01', '2024-10-03', 'Personnel', 'EnAttente');

-- 8. PAIES
INSERT INTO Paies (EmployeId, Montant, Mois, DateEmission) VALUES
('emp1', 15000.00, 'Janvier 2024', '2024-01-31'),
('emp1', 15000.00, 'Février 2024', '2024-02-29'),
('emp1', 15000.00, 'Mars 2024', '2024-03-31'),
('emp2', 12000.00, 'Janvier 2024', '2024-01-31'),
('emp2', 12000.00, 'Février 2024', '2024-02-29'),
('emp3', 11000.00, 'Janvier 2024', '2024-01-31'),
('emp3', 11000.00, 'Février 2024', '2024-02-29'),
('emp4', 13000.00, 'Janvier 2024', '2024-01-31'),
('emp4', 13000.00, 'Février 2024', '2024-02-29'),
('emp5', 14000.00, 'Janvier 2024', '2024-01-31'),
('emp5', 14000.00, 'Février 2024', '2024-02-29'),
('emp6', 12500.00, 'Janvier 2024', '2024-01-31'),
('emp7', 10000.00, 'Janvier 2024', '2024-01-31'),
('emp8', 5000.00, 'Janvier 2024', '2024-01-31');

-- 9. LIGNES DE PAIE
INSERT INTO LignesPaie (PaieId, Libelle, Montant, Type, Ordre) VALUES
-- Paie emp1 - Janvier
(1, 'Salaire de base', 15000.00, 'Gain', 1),
(1, 'CNSS', 900.00, 'Retenue', 2),
(1, 'AMO', 300.00, 'Retenue', 3),
(1, 'IR', 1500.00, 'Retenue', 4),
-- Paie emp2 - Janvier
(4, 'Salaire de base', 12000.00, 'Gain', 1),
(4, 'CNSS', 720.00, 'Retenue', 2),
(4, 'AMO', 240.00, 'Retenue', 3),
(4, 'IR', 1200.00, 'Retenue', 4),
-- Paie emp3 - Janvier
(6, 'Salaire de base', 11000.00, 'Gain', 1),
(6, 'CNSS', 660.00, 'Retenue', 2),
(6, 'AMO', 220.00, 'Retenue', 3),
(6, 'IR', 1100.00, 'Retenue', 4);

-- 10. POINTAGES
INSERT INTO Pointages (EmployeId, DatePointage, HeureArrivee, HeureDepart, DureeTravail, HeuresSupplementaires, EstAbsent, Remarques) VALUES
('emp1', '2024-01-15', '2024-01-15 08:30:00', '2024-01-15 17:30:00', '09:00:00', 1.0, 0, NULL),
('emp1', '2024-01-16', '2024-01-16 08:45:00', '2024-01-16 18:00:00', '09:15:00', 1.25, 0, NULL),
('emp2', '2024-01-15', '2024-01-15 09:00:00', '2024-01-15 17:00:00', '08:00:00', 0, 0, NULL),
('emp2', '2024-01-16', '2024-01-16 08:30:00', '2024-01-16 17:30:00', '09:00:00', 1.0, 0, NULL),
('emp3', '2024-01-15', '2024-01-15 08:00:00', '2024-01-15 17:00:00', '09:00:00', 1.0, 0, NULL),
('emp3', '2024-01-16', NULL, NULL, NULL, NULL, 1, 'Absence justifiée - Maladie'),
('emp4', '2024-01-15', '2024-01-15 08:15:00', '2024-01-15 17:15:00', '09:00:00', 1.0, 0, NULL),
('emp5', '2024-01-15', '2024-01-15 08:00:00', '2024-01-15 18:30:00', '10:30:00', 2.5, 0, 'Travail sur projet urgent'),
('emp6', '2024-01-15', '2024-01-15 09:00:00', '2024-01-15 17:00:00', '08:00:00', 0, 0, NULL),
('emp7', '2024-01-15', '2024-01-15 08:30:00', '2024-01-15 17:30:00', '09:00:00', 1.0, 0, NULL),
('emp8', '2024-01-15', '2024-01-15 09:00:00', '2024-01-15 17:00:00', '08:00:00', 0, 0, NULL);

-- 11. NOTIFICATIONS
INSERT INTO Notifications (UserId, Titre, Message, Type, EstLue, DateCreation, LienAction) VALUES
('emp1', 'Congé approuvé', 'Votre demande de congé du 15/07/2024 au 20/07/2024 a été approuvée.', 'Conge', 0, '2024-07-10 10:00:00', '/Conges/Index'),
('emp2', 'Nouvelle paie', 'Votre bulletin de paie de Janvier 2024 est disponible.', 'Paie', 0, '2024-01-31 12:00:00', '/Paies/Index'),
('emp3', 'Congé approuvé', 'Votre demande de congé maladie a été approuvée.', 'Conge', 1, '2024-06-08 09:00:00', '/Conges/Index'),
('resp1', 'Nouvelle demande de congé', 'Youssef Elamrani a demandé un congé.', 'Conge', 0, '2024-07-05 14:00:00', '/Conges/Index'),
('resp2', 'Pointage enregistré', 'Sanae Idrissi a pointé son arrivée.', 'Pointage', 0, '2024-01-15 08:30:00', '/Pointages/Index'),
('emp4', 'Congé en attente', 'Votre demande de congé est en attente de validation par votre manager.', 'Conge', 0, '2024-08-25 11:00:00', '/Conges/Index');
