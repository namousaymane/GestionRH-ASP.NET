using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using GestionRH.Models;

namespace GestionRH.Services
{
    public class PdfService
    {
        public byte[] GenererBulletinPaie(Paie paie)
        {
            // Vérifier que l'employé est chargé
            if (paie.Employe == null)
            {
                throw new InvalidOperationException("L'employé associé à cette paie n'est pas chargé.");
            }

            // Configuration de la licence gratuite (obligatoire pour QuestPDF)
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    // 1. En-tête (Header)
                    page.Header()
                        .Text($"Bulletin de Paie - {paie.Mois}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    // 2. Contenu (Content)
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        // Informations Employé
                        column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Employé :").Bold();
                                c.Item().Text(paie.Employe.NomComplet);
                                c.Item().Text(paie.Employe.Email);
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Détails :").Bold();
                                c.Item().Text($"Poste : {paie.Employe.Poste}");
                                c.Item().Text($"Date d'émission : {paie.DateEmission:dd/MM/yyyy}");
                            });
                        });

                        column.Item().Height(20); // Espace

                        // Tableau des montants
                        column.Item().Table(table =>
                        {
                            // Définition des colonnes
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                            });

                            // En-tête du tableau
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Rubrique");
                                header.Cell().Element(CellStyle).AlignRight().Text("Montant");
                            });

                            // Lignes
                            // Note : Comme on n'a pas stocké le détail Primes/Retenues en base,
                            // on affiche le salaire de base et le net final.

                            table.Cell().Element(CellStyle).Text("Salaire de Base");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{paie.Employe.Salaire:N2} DH");

                            // On affiche une ligne "Ajustements (Primes - Retenues)" calculée
                            decimal ajustement = paie.Montant - paie.Employe.Salaire;
                            if (ajustement != 0)
                            {
                                table.Cell().Element(CellStyle).Text("Primes / Retenues / Heures Sup.");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{ajustement:N2} DH");
                            }

                            // Total Net
                            table.Cell().Element(FooterStyle).Text("NET À PAYER").FontSize(14).Bold();
                            table.Cell().Element(FooterStyle).AlignRight().Text($"{paie.Montant:N2} DH").FontSize(14).Bold().FontColor(Colors.Green.Medium);
                        });
                    });

                    // 3. Pied de page (Footer)
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            });

            return document.GeneratePdf();
        }

        // Styles pour le tableau (pour éviter la répétition)
        static IContainer CellStyle(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        }

        static IContainer FooterStyle(IContainer container)
        {
            return container.BorderTop(1).BorderColor(Colors.Black).PaddingVertical(10);
        }
    }
}