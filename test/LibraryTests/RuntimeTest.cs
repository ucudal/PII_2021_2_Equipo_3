using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Library;
using Library.Core;
using Library.Core.Distribution;
using Library.Core.Invitations;
using Library.States.Admins;
using Library.HighLevel.Accountability;
using Library.HighLevel.Companies;
using Library.HighLevel.Entrepreneurs;
using Library.HighLevel.Materials;
using Library.Utils;
using NUnit.Framework;
using Ucu.Poo.Locations.Client;
using UnitTests.Utils;

#warning TODO: Add test of creating an entrepreneur report before buying materials.
#warning TODO: Add test of creating an entrepreneur report after buying materials.
#warning TODO: Add test of removing company.
#warning TODO: Add test of removing normal user.
#warning TODO: Add test of removing admin.

namespace UnitTests
{
    /// <summary>
    /// This class holds a single test which executes a long runtime code into a ConsolePlatform-like this.platform!.
    /// </summary>
    [TestFixture]
    public class RuntimeTest
    {
        private ProgramaticMultipleUserPlatform? platform;

        private static Regex entrepreneurReportRegex = new Regex(
                "\\(De (?<company>.+)\\) (?<materialname>.+?), "
              + "cantidad: (?<materialquantity>.+?), "
              + "precio: (?<materialprice>.+?), "
              + "ubicación: (?<materiallocation>.+), "
              + "tipo: (?<materialtype>.+)",
                RegexOptions.Compiled
            );

        private static Regex companyReportRegex = new Regex(
            "(?<amount>\\d+(?:\\.\\d+)? \\w+) de (?<material>[\\w ]+) vendido(s) al emprendedor (?<entrepreneur>[\\w ]+) a un precio de (?<price>.+?) el día (?<date>.+)",
            RegexOptions.Compiled);

        /// <summary>
        /// Performs a setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.platform = new ProgramaticMultipleUserPlatform();
        }

        /// <summary>
        /// Performs a generic runtime test.
        /// </summary>
        /// <param name="startFolderName">The name of the folder which holds the memory to deserialize.</param>
        /// <param name="action">The function to run.</param>
        /// <param name="endFolderName">The name of the folder which will hold the serialized memory.</param>
        public void BasicRuntimeTest(string startFolderName, Action action, string? endFolderName = null)
        {
            endFolderName ??= startFolderName;
            SerializationUtils.DeserializeAllFromJSON($"../../../Memories/-Memory-start/{startFolderName}");
            try
            {
                action();
            }
            finally
            {
                SerializationUtils.SerializeAllIntoJson($"../../../Memories/Memory-end/{endFolderName}");
                SerializationUtils.DeserializeAllFromJSON("../../../Memories/-Memory-void");
            }
        }

        /// <summary>
        /// Tests whether an admin can be successfully created from code.
        /// </summary>
        [Test]
        public void CreateAdminTest()
        {
            BasicRuntimeTest("create-admin", () =>
            {
                Singleton<SessionManager>.Instance.NewUser(
                    "Admin1",
                    new UserData("Martín", true, UserData.Type.ADMIN, null, null),
                    new AdminInitialMenuState());
                CheckUtils.CheckUserAndEquality(new UserData("Martín", true, UserData.Type.ADMIN, null, null), "Admin1");
            });
        }

        /// <summary>
        /// Tests the user story of signing up as an entrepreneur.
        /// </summary>
        [Test]
        public void CreateEntrepreneurTest()
        {
            BasicRuntimeTest("create-entrepreneur", () =>
            {
                this.platform!.ReceiveMessages(
                    "Entrepreneur1",
                    "/help",
                    "/start -e",
                    "Santiago",
                    "/esc",
                    "/esc",
                    "19",
                    "Av. 8 de Octubre, Montevideo, Montevideo, Uruguay",
                    "Maderas",
                    "/add",
                    "https://www.wikipedia.org",
                    "Description1",
                    "/finish",
                    "/add",
                    "E1",
                    "/add",
                    "E2",
                    "/finish");

                CheckUtils.CheckUserAndEquality(new UserData("Santiago", true, UserData.Type.ENTREPRENEUR, null, null), "Entrepreneur1");
            });
        }

        /// <summary>
        /// Tests the user story of creating invitations (admin).
        /// </summary>
        [Test]
        public void CreateInvitationTest()
        {
            BasicRuntimeTest("create-invitations", () =>
            {
                // Create a message invitation for "Company1"
                string? invitationCode;
                {
                    List<(string, string)> responses = this.platform!.ReceiveMessages(
                        "Admin1",
                        "/invitecompany");
                    invitationCode = AdminStatesTest.IsCreateInvitationResponseRegex(responses[0].Item2);
                    Assert.That(invitationCode, Is.Not.Null);
                }
                Assert.AreEqual(1, Singleton<Library.Core.Invitations.InvitationManager>.Instance.InvitationCount);

                // Create a message invitation for "Company2"
                string? invitationCode2;
                {
                    List<(string, string)> responses = this.platform!.ReceiveMessages(
                        "Admin1",
                        "/invitecompany");
                    invitationCode2 = AdminStatesTest.IsCreateInvitationResponseRegex(responses[0].Item2);
                    Assert.That(invitationCode2, Is.Not.Null);
                }
                Assert.AreEqual(2, Singleton<Library.Core.Invitations.InvitationManager>.Instance.InvitationCount);

                // Create a message invitation for "Company3"
                string? invitationCode3;
                {
                    List<(string, string)> responses = this.platform!.ReceiveMessages(
                        "Admin1",
                        "/invitecompany");
                    invitationCode3 = AdminStatesTest.IsCreateInvitationResponseRegex(responses[0].Item2);
                    Assert.That(invitationCode3, Is.Not.Null);
                }
                Assert.AreEqual(3, Singleton<Library.Core.Invitations.InvitationManager>.Instance.InvitationCount);
            });
        }

        /// <summary>
        /// Tests the user story of creating a company from an invitation.
        /// </summary>
        [Test]
        public void CreateCompanyTest()
        {
            BasicRuntimeTest("create-company", () =>
            {
                {
                    // Sign up an user of id "Company1" as company representative
                    this.platform!.ReceiveMessages(
                        "Company1",
                        $"/start n1AIPqHy",
                        "Roberto",
                        "/esc",
                        "/esc",
                        "Teogal",
                        "Maderas",
                        "Av. 8 de Octubre, Montevideo, Montevideo, Uruguay",
                        "098140124",
                        "teogal@gmail.com");
                    CheckUtils.CheckUserAndEquality(new UserData("Roberto", true, UserData.Type.COMPANY, null, null), "Company1");
                    Company? company = Singleton<CompanyManager>.Instance.GetByName("Teogal");
                    Assert.That(company, Is.Not.Null);
                    Assert.Contains("Company1", company!.Representants);
                }

                {
                    // Sign up an user of id "Company2" as company representative
                    this.platform!.ReceiveMessages(
                        "Company2",
                        $"/start pYzsMjCB",
                        "Ernesto",
                        "/esc",
                        "/esc",
                        "Compañía de vidrios",
                        "Vidrios",
                        "Av. 8 de Octubre, Montevideo, Montevideo, Uruguay",
                        "091695341",
                        "vi.drios@gmail.com");
                    CheckUtils.CheckUserAndEquality(new UserData("Ernesto", true, UserData.Type.COMPANY, null, null), "Company2");
                    Company? company = Singleton<CompanyManager>.Instance.GetByName("Compañía de vidrios");
                    Assert.That(company, Is.Not.Null);
                    Assert.Contains("Company2", company!.Representants);
                }

                {
                    // Sign up an user of id "Company2" as company representative
                    this.platform!.ReceiveMessages(
                        "Company3",
                        $"/start hVvI3DGe",
                        "Carlos",
                        "/esc",
                        "/esc",
                        "La Metalería",
                        "Metálicos",
                        "Av. 8 de Octubre, Montevideo, Montevideo, Uruguay",
                        "092130294",
                        "metaleria_comp@gmail.com");
                    CheckUtils.CheckUserAndEquality(new UserData("Carlos", true, UserData.Type.COMPANY, null, null), "Company3");
                    Company? company = Singleton<CompanyManager>.Instance.GetByName("La Metalería");
                    Assert.That(company, Is.Not.Null);
                    Assert.Contains("Company3", company!.Representants);
                }
            });
        }

        /// <summary>
        /// Tests the user story of adding a company representative to an already existing company.
        /// </summary>
        [Test]
        public void AddCompanyRepresentativeTest()
        {
            BasicRuntimeTest("add-representative-to-company", () =>
            {
                // Sign up an user of id "Company1" as company representative
                this.platform!.ReceiveMessages(
                    "Company2",
                    $"/start 01010101",
                    "José",
                    "/esc",
                    "jose@gmail.com",
                    "Teogal",
                    "Sí");
                CheckUtils.CheckUserAndEquality(new UserData("José", true, UserData.Type.COMPANY, "jose@gmail.com", null), "Company2");
                Company? company = Singleton<CompanyManager>.Instance.GetByName("Teogal");
                Assert.That(company, Is.Not.Null);
                Assert.AreEqual(new List<string>()
                {
                    "Company1", "Company2"
                }, company!.Representants);
            });
        }

        /// <summary>
        /// Tests the user story of publishing a normal material.
        /// </summary>
        [Test]
        public void PublishNormalMaterialTest()
        {
            BasicRuntimeTest("publish-normal-material", () =>
            {
                // Publish a material as Company1
                this.platform!.ReceiveMessages(
                    "Company1",
                    "/publish",
                    "Bujes de cartón",
                    "length",
                    "Celulósicos",
                    "30cm",
                    "15 U$/cm",
                    "Av. 8 de Octubre, Montevideo, Montevideo, Uruguay",
                    "/normal",
                    "/add",
                    "Bujes",
                    "/add",
                    "Cartón",
                    "/finish",
                    "/finish");
                {
                    IList<MaterialPublication> publications = Singleton<CompanyManager>.Instance.GetByName("Teogal")!.Publications;
                    Assert.AreEqual(1, publications.Count);
                    MaterialPublication publication = publications[0];
                    CheckUtils.CheckMaterialPublicationEquality(
                        MaterialPublication.CreateInstance(
                            Material.CreateInstance(
                                "Bujes de cartón",
                                Measure.Length,
                                MaterialCategory.GetByName("Celulósicos").Unwrap()),
                            new Amount(30, Unit.GetByAbbr("cm").Unwrap()),
                            new Price(15, Currency.Peso, Unit.GetByAbbr("cm").Unwrap()),
                            new Location()
                            {
                                Found = true,
                                AddresLine = "Avenida 8 de Octubre",
                                CountryRegion = "Uruguay",
                                FormattedAddress = "Avenida 8 de Octubre, Montevideo",
                                Locality = "Montevideo",
                                PostalCode = null,
                                Latitude = -34.87959,
                                Longitude = -56.14838
                            },
                            MaterialPublicationTypeData.Normal(),
                            new List<string>()
                            {
                            "Bujes", "Cartón"
                            },
                            new List<string>()).Unwrap(),
                        publication);
                }
            });
        }

        /// <summary>
        /// Tests the user story of searching materials by keyword.
        /// </summary>
        [Test]
        public void SearchByKeywordTest()
        {
            BasicRuntimeTest("search-material-by-keywords", () =>
            {
                List<(string, string)> responses = this.platform!.ReceiveMessages(
                    "Entrepreneur1",
                    "/searchFK",
                    "Bujes");

                string finalMessage = responses[responses.Count - 1].Item2;

                string[][] expected = new string[][]
                {
                    new string[]
                    {
                        "Teogal",
                        "Bujes de cartón",
                        "30.00 cm",
                        "15 U$/cm",
                        "Avenida 8 de Octubre, Montevideo, Uruguay",
                        "normal"
                    }
                }, actual = entrepreneurReportRegex.Matches(finalMessage)
                    .Select(m => new string[]
                    {
                        m.Groups["company"].Value,
                        m.Groups["materialname"].Value,
                        m.Groups["materialquantity"].Value,
                        m.Groups["materialprice"].Value,
                        m.Groups["materiallocation"].Value,
                        m.Groups["materialtype"].Value
                    }).ToArray();

                Assert.AreEqual(expected, actual);
            });
        }

        /// <summary>
        /// Tests the user story of creating a company report.
        /// </summary>
        [Test]
        public void CreateCompanyReportTest()
        {
            BasicRuntimeTest("create-company-report", () =>
            {
                List<(string, string)> messages = this.platform!.ReceiveMessages(
                    "Company1",
                    "/companyreport",
                    "23/11/2021");

                Assert.AreEqual("Reporte vacío.", messages[messages.Count - 1].Item2.Split('\n')[0]);
            });

        }

        /// <summary>
        /// Tests the user story of publishing a continuous material.
        /// </summary>
        [Test]
        public void PublishContinuousMaterialTest()
        {
            BasicRuntimeTest("publish-continuous-material", () =>
            {
                this.platform!.ReceiveMessages(
                    "Company3",
                    "/publish",
                    "Envase de vidrio",
                    "weight",
                    "Vidrio",
                    "500g",
                    "10 U$/g",
                    "Av. 8 de Octubre, Montevideo, Montevideo, Uruguay",
                    "/continuous",
                    "/add",
                    "Envase",
                    "/add",
                    "Vidrio",
                    "/finish",
                    "/finish");

                IList<MaterialPublication> publications = Singleton<CompanyManager>.Instance.GetByName("La Metalería")!.Publications;
                Assert.AreEqual(1, publications.Count);
                MaterialPublication publication = publications[0];
                CheckUtils.CheckMaterialPublicationEquality(
                    MaterialPublication.CreateInstance(
                        Material.CreateInstance(
                            "Envase de vidrio",
                            Measure.Weight,
                            MaterialCategory.GetByName("Vidrio").Unwrap()),
                        new Amount(500, Unit.GetByAbbr("g").Unwrap()),
                        new Price(10, Currency.Peso, Unit.GetByAbbr("g").Unwrap()),
                        new Location()
                        {
                            Found = true,
                            AddresLine = "Avenida 8 de Octubre",
                            CountryRegion = "Uruguay",
                            FormattedAddress = "Avenida 8 de Octubre, Montevideo",
                            Locality = "Montevideo",
                            PostalCode = null,
                            Latitude = -34.87959,
                            Longitude = -56.14838
                        },
                        MaterialPublicationTypeData.Continuous(),
                        new List<string>()
                        {
                            "Envase", "Vidrio"
                        },
                        new List<string>()).Unwrap(),
                    publication);
            });

        }

        /// <summary>
        /// Tests the user story of publishing a scheduled material.
        /// </summary>
        [Test]
        public void PublishScheduledMaterialTest()
        {
            BasicRuntimeTest("publish-scheduled-material", () =>
            {
                this.platform!.ReceiveMessages(
                    "Company2",
                    "/publish",
                    "Garrafas",
                    "weight",
                    "Metales",
                    "5kg",
                    "15 U$/kg",
                    "Av. 8 de Octubre, Montevideo, Montevideo, Uruguay",
                    "/scheduled",
                    "12/11/2021",
                    "/add",
                    "Metales",
                    "/add",
                    "Metálicos",
                    "/finish",
                    "/finish");

                IList<MaterialPublication> publications2 = Singleton<CompanyManager>.Instance.GetByName("Compañía de vidrios")!.Publications;
                Assert.AreEqual(1, publications2.Count);
                MaterialPublication publication2 = publications2[0];
                CheckUtils.CheckMaterialPublicationEquality(
                    MaterialPublication.CreateInstance(
                        Material.CreateInstance(
                            "Garrafas",
                            Measure.Weight,
                            MaterialCategory.GetByName("Metales").Unwrap()),
                        new Amount(5, Unit.GetByAbbr("kg").Unwrap()),
                        new Price(15, Currency.Peso, Unit.GetByAbbr("kg").Unwrap()),
                        new Location()
                        {
                            Found = true,
                            AddresLine = "Avenida 8 de Octubre",
                            CountryRegion = "Uruguay",
                            FormattedAddress = "Avenida 8 de Octubre, Montevideo",
                            Locality = "Montevideo",
                            PostalCode = null,
                            Latitude = -34.87959,
                            Longitude = -56.14838
                        },
                        MaterialPublicationTypeData.Scheduled(new DateTime(2021, 11, 30)),
                        new List<string>()
                        {
                            "Metales", "Metálicos"
                        },
                        new List<string>()).Unwrap(),
                    publication2);
            });
        }

        /// <summary>
        /// Tests the user story of purchasing a material.
        /// </summary>
        [Test]
        public void PurchaseMaterialTest()
        {
            BasicRuntimeTest("purchase-material", () =>
            {
                this.platform!.ReceiveMessages(
                    "Entrepreneur1",
                    "/buy",
                    "Teogal",
                    "Bujes de cartón",
                    "2 cm",
                    "Sí");

                {
                    List<MaterialPublication> publications = Singleton<CompanyManager>.Instance.GetByName("Teogal")!.Publications;
                    Assert.AreEqual(1, publications.Count);
                    MaterialPublication publication = publications[0];
                    CheckUtils.CheckMaterialPublicationEquality(
                        MaterialPublication.CreateInstance(
                            Material.CreateInstance(
                                "Bujes de cartón",
                                Measure.Length,
                                MaterialCategory.GetByName("Celulósicos").Unwrap()),
                            new Amount(8, Unit.GetByAbbr("cm").Unwrap()),
                            new Price(15, Currency.Peso, Unit.GetByAbbr("cm").Unwrap()),
                            new Location
                            {
                                Found = true,
                                AddresLine = "Avenida 8 de Octubre",
                                CountryRegion = "Uruguay",
                                FormattedAddress = "Avenida 8 de Octubre, Montevideo",
                                Locality = "Montevideo",
                                PostalCode = null,
                                Latitude = -34.87959,
                                Longitude = -56.14838
                            },
                            MaterialPublicationTypeData.Normal(),
                            new List<string>() { "Bujes", "Cartón" },
                            new List<string>()).Unwrap(),
                        publication);
                }
                {
                    BoughtMaterialLine[] purchases = Singleton<EntrepreneurManager>.Instance.GetById("Entrepreneur1")!.BoughtMaterials.ToArray();
                    Assert.AreEqual(1, purchases.Length);
                    BoughtMaterialLine purchase = purchases[0];
                    CheckUtils.CheckBoughtMaterialLineEquality(
                        new BoughtMaterialLine(
                            "Teogal",
                            Material.CreateInstance(
                                "Bujes de cartón",
                                Measure.Length,
                                MaterialCategory.GetByName("Celulósicos").Unwrap()),
                            DateTime.Today,
                            new Price(15, Currency.Peso, Unit.GetByAbbr("cm").Unwrap()),
                            new Amount(2, Unit.GetByAbbr("cm").Unwrap())),
                        purchase);
                }
                {
                    MaterialSalesLine[] sales = Singleton<CompanyManager>.Instance.GetByName("Teogal")!.MaterialSales.ToArray();
                    Assert.AreEqual(1, sales.Length);
                    MaterialSalesLine sale = sales[0];
                    CheckUtils.CheckMaterialSalesLineEquality(
                        new MaterialSalesLine(
                            Material.CreateInstance(
                                "Bujes de cartón",
                                Measure.Length,
                                MaterialCategory.GetByName("Celulósicos").Unwrap()),
                            new Amount(2, Unit.GetByAbbr("cm").Unwrap()),
                            new Price(15, Currency.Peso, Unit.GetByAbbr("cm").Unwrap()),
                            DateTime.Today,
                            "Santiago"),
                        sale);
                }
            });
        }

        /// <summary>
        /// Tests the user story of purchasing a material which doesn't exist.
        /// </summary>
        [Test]
        public void PurchaseNonExistentMaterialTest()
        {
            BasicRuntimeTest("purchase-non-existent-material", () =>
            {
                this.platform!.ReceiveMessages(
                    "Entrepreneur1",
                    "/buy",
                    "Teog",
                    "Teo",
                    "Teogal",
                    "A1",
                    "C2",
                    "\\");

                Assert.AreEqual(0, Singleton<EntrepreneurManager>.Instance.GetById("Entrepreneur1")!.BoughtMaterials.Count);
                Assert.AreEqual(0, Singleton<CompanyManager>.Instance.GetByName("Teogal")!.MaterialSales.Count);
            });
        }

        /// <summary>
        /// Tests the user story of purchasing a material with lower stock than asked.
        /// </summary>
        [Test]
        public void PurchaseUnderstockedMaterialTest()
        {
            BasicRuntimeTest("purchase-understocked-material", () =>
            {
                this.platform!.ReceiveMessages(
                    "Entrepreneur1",
                    "/buy",
                    "Teogal",
                    "Bujes de cartón",
                    "12 cm",
                    "Sí",
                    "Sí");

                {
                    List<MaterialPublication> publications = Singleton<CompanyManager>.Instance.GetByName("Teogal")!.Publications;
                    Assert.AreEqual(1, publications.Count);
                    MaterialPublication publication = publications[0];
                    CheckUtils.CheckMaterialPublicationEquality(
                        MaterialPublication.CreateInstance(
                            Material.CreateInstance(
                                "Bujes de cartón",
                                Measure.Length,
                                MaterialCategory.GetByName("Celulósicos").Unwrap()),
                            new Amount(0, Unit.GetByAbbr("cm").Unwrap()),
                            new Price(15, Currency.Peso, Unit.GetByAbbr("cm").Unwrap()),
                            new Location
                            {
                                Found = true,
                                AddresLine = "Avenida 8 de Octubre",
                                CountryRegion = "Uruguay",
                                FormattedAddress = "Avenida 8 de Octubre, Montevideo",
                                Locality = "Montevideo",
                                PostalCode = null,
                                Latitude = -34.87959,
                                Longitude = -56.14838
                            },
                            MaterialPublicationTypeData.Normal(),
                            new List<string>() { "Bujes", "Cartón" },
                            new List<string>()).Unwrap(),
                        publication);
                        Assert.That(publication.Sold, Is.True);
                }
                {
                    BoughtMaterialLine[] purchases = Singleton<EntrepreneurManager>.Instance.GetById("Entrepreneur1")!.BoughtMaterials.ToArray();
                    Assert.AreEqual(1, purchases.Length);
                    BoughtMaterialLine purchase = purchases[0];
                    CheckUtils.CheckBoughtMaterialLineEquality(
                        new BoughtMaterialLine(
                            "Teogal",
                            Material.CreateInstance(
                                "Bujes de cartón",
                                Measure.Length,
                                MaterialCategory.GetByName("Celulósicos").Unwrap()),
                            DateTime.Today,
                            new Price(15, Currency.Peso, Unit.GetByAbbr("cm").Unwrap()),
                            new Amount(10, Unit.GetByAbbr("cm").Unwrap())),
                        purchase);
                }
                {
                    MaterialSalesLine[] sales = Singleton<CompanyManager>.Instance.GetByName("Teogal")!.MaterialSales.ToArray();
                    Assert.AreEqual(1, sales.Length);
                    MaterialSalesLine sale = sales[0];
                    CheckUtils.CheckMaterialSalesLineEquality(
                        new MaterialSalesLine(
                            Material.CreateInstance(
                                "Bujes de cartón",
                                Measure.Length,
                                MaterialCategory.GetByName("Celulósicos").Unwrap()),
                            new Amount(10, Unit.GetByAbbr("cm").Unwrap()),
                            new Price(15, Currency.Peso, Unit.GetByAbbr("cm").Unwrap()),
                            DateTime.Today,
                            "Santiago"),
                        sale);
                }
            });
        }

        /// <summary>
        /// Tests the user story of making a company report after selling materials.
        /// </summary>
        [Test]
        public void CreateCompanyReportTest2()
        {
            BasicRuntimeTest("create-company-report-2", () =>
            {
                List<(string, string)> messages = this.platform!.ReceiveMessages(
                    "Company1",
                    "/companyreport",
                    "23/11/2021");

                Assert.AreEqual(
                    "10.00 cm de Bujes de cartón vendido(s) al emprendedor Santiago a precio de 15 U$/cm el día 28/11/2021",
                    messages[messages.Count - 1].Item2.Split('\n')[0]);
            });
        }
        /// <summary>
        /// Tests the user story of remove a company by an admin.
        /// </summary>
        [Test]

        public void RemoveCopanyTest()
        {
            BasicRuntimeTest("remove-company", () =>
            {
                platform!.ReceiveMessages(
                    "Admin1",
                    "/removecompany",
                    "Company2"
                );
                bool actual2= Singleton<CompanyManager>.Instance.RemoveCompany("Company2");
                Assert.That(actual2, Is.False);
            });
        }
        
        /// <summary>
        /// Tests the user story of remove a entrepreneur by an admin.
        /// </summary>
        [Test]
        public void RemoveEntrepreneurTest()
        {
            BasicRuntimeTest("remove-entrepreneur", () =>
            {
                platform!.ReceiveMessages(
                "Admin1",
                "/removeuser",
                "Santiago"
            );
            bool actual= Singleton<SessionManager>.Instance.RemoveUserByName("Santiago");
            Assert.That(actual, Is.False);
            });
        }
        /// <summary>
        /// Tests the user story of obtain a entrepreneur report
        /// </summary>
        [Test]
        public void ReportEntrepreneurTest()
        {
            BasicRuntimeTest("report-entrepreneur",() =>
            {
                List<(string, string)> messages = this.platform!.ReceiveMessages(
                    "Entrepreneur1",
                    "/ereport",
                    "23/11/2021"
                );

                Assert.AreEqual(
                    "10.00 cm de Bujes de cartón el día 28/11/2021 a precio de 15 U$/cm (U$ 150)",
                    messages[messages.Count - 1].Item2.Split('\n')[0]);
            });
        }


    }
}
