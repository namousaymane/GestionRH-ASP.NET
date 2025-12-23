# Application de Gestion des Ressources Humaines (GestionRH)

Ce projet est une application web développée en ASP.NET Core MVC destinée à la digitalisation des processus administratifs des Ressources Humaines. Elle permet la gestion centralisée des employés dont le pointage journalier, le traitement des demandes de congés et la génération des bulletins de paie.

## Fonctionnalités Principales

### 1. Gestion des Employés
* Gestion des comptes utilisateurs (Inscription, Connexion, Déconnexion).
* CRUD complet des fiches employés par l'Administrateur RH.
* Consultation du profil personnel pour chaque employé.
* Gestion des départements et hiérarchie (Responsable/Managers).

### 2. Gestion des Congés
* Demande de congés par les employés.
* Workflow de validation à deux niveaux 
    1. Validation par le Manager (Responsable hiéarchique).
    2. Validation par l'Administrateur RH.
* Calcul automatique et décrémentation du solde de congés lors de la validation.
* Historique et suivi des statuts (En attente, Validé, Refusé).

### 3. Gestion de la Paie
* Calcul des salaires mensuels incluant le salaire de base, les primes et les retenues.
* Génération automatique des bulletins de paie au format PDF.
* Téléchargement des bulletins par les employés.

### 4. Tableau de Bord (Dashboard)
* Indicateurs globaux pour l'Administrateur : effectif total, demandes en attente, masse salariale.
* Accès rapide aux fonctionnalités principales.

## Installation et Configuration
1. **Cloner le dépôt**
   ```bash
   git clone [https://github.com/namousaymane/GestionRH-ASP.NET.git](https://github.com/namousaymane/GestionRH-ASP.NET.git)
   cd GestionRH-ASP.NET
2. **Configuration de la BD**
Modifier le fichier `appsettings.json` avec vos identifiants MySql locaux.

3. **Application de la migration**
  ```bash
  Update-Database
