using System.Collections.Generic;
using Library.Core;
using Library.HighLevel.Administers;
using Library.HighLevel.Companies;
using Library.HighLevel.Entrepreneurs;
using Library.HighLevel.Materials;
using Library.Platforms.Telegram;
using NUnit.Framework;
using Ucu.Poo.Locations.Client;

namespace ProgramTests
{
    /// <summary>
    /// This Test is for verificates if a Company can accept an invitation to the platform.
    /// </summary>
    public class AcceptInvitationTest
    {
        /// <summary>
        /// Test setup.
        /// </summary>
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Test if a company can accept an invitation and register.
        /// </summary>
        [Test]
        public void AcceptInvitation()
        {
            Administer.CreateCompanyInvitation();
            TelegramId id = new TelegramId(2066298868);

            // Message with the code.
            Message message = new Message("1234567", id);
            LocationApiClient provider = new LocationApiClient();

            // If the message with the code is equal with te code sended in an invitation,
            // the user can register the company.
            ContactInfo contactInfo;
            contactInfo.Email = "companysa@gmail.com";
            contactInfo.PhoneNumber = 098765432;
            Location location = provider.GetLocationAsync("Av. 8 de Octubre 2738", "Montevideo", "Montevideo", "Uruguay").Result;
            Company company = CompanyManager.CreateCompany("Company.SA", contactInfo, "Arroz", location);
            company.AddUser(message.Id);

            bool expected = company.HasUser(message.Id);
            Company expectedCompany = CompanyManager.GetByName("Company.SA");

            // If the message with the code is equal with an invitation sended, the user has to
            // be added in the representants list of the company.
            // The company is registered.
            Assert.That(expected, Is.True);
            Assert.AreEqual(company, expectedCompany);
        }

        /// <summary>
        /// If the user don´t have a code, it´s user is an Entrepreneur.
        /// </summary>
        [Test]
        public void NotAcceptInvitation()
        {
            TelegramId id = new TelegramId(2066298868);
            Message message = new Message("", id);
            Habilitation habilitation = new Habilitation("Link1", "description1");
            Habilitation habilitation2 = new Habilitation("Link2", "description2");
            List<Habilitation> habilitations = new List<Habilitation> { habilitation, habilitation2 };
            string specialization = "specialization1";
            string specialization2 = "specialization2";
            List<string> specializations = new List<string> {specialization, specialization2};
            LocationApiClient provider = new LocationApiClient();
            Location location = provider.GetLocationAsync("Av. 8 de Octubre 2738", "Montevideo", "Montevideo", "Uruguay").Result;
            Entrepreneur entrepreneur = new Entrepreneur(id, "Juan", "22", location, "Carpintero", habilitations, specializations);
            Entrepreneur.EntrepeneurList.Add(message.Id);
            bool expected = Entrepreneur.EntrepeneurList.Contains(message.Id);
            Assert.That(expected, Is.True);
        }
    }
}