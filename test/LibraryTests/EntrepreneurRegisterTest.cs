using System.Collections.Generic;
using Library.Core;
using Library.HighLevel.Entrepreneurs;
using Library.HighLevel.Materials;
using Library.Platforms.Telegram;
using NUnit.Framework;
using Ucu.Poo.Locations.Client;

namespace ProgramTests
{
    /// <summary>
    /// Test if an Entrepreneur can register into the platform.
    /// </summary>
    public class EntrepreneurRegisterTest
    {
        private TelegramId juanId;
        private Message nameMessage;
        private Message ageMessage;
        private LocationApiClient provider;
        private Location location;
        private Message headingMessage;
        private Message habilitationsMessage;
        private Message specializationsMessage;

        /// <summary>
        /// It´s create a message with the information correspondent.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.juanId = new TelegramId(2567104974);
            this.nameMessage = new Message("Juan", this.juanId);
            this.ageMessage = new Message("23", this.juanId);
            this.headingMessage = new Message("carpintero", this.juanId);
            this.habilitationsMessage = new Message("/command link1 link2", this.juanId);
            this.specializationsMessage = new Message("/command specialization1, specialization2", this.juanId);
            this.provider = new LocationApiClient();
            this.location = this.provider.GetLocationAsync("Av. 8 de Octubre 2738").Result;
        }

        /// <summary>
        /// This test evaluate if the entrepreneur is registered with their correct information.
        /// </summary>
        [Test]
        public void EntrepreneurRegister()
        {
            string[] habilitationsMessageSplitted = this.habilitationsMessage.Text.Trim().Split();
            List<Habilitation> habilitations = new List<Habilitation>();

            for (int i = 1; i < habilitationsMessageSplitted.Length; i++)
            {
                Habilitation habilitation = new Habilitation(habilitationsMessageSplitted[i]);
                habilitations.Add(habilitation);
            }

            string[] specializationMessageSplitted = this.habilitationsMessage.Text.Trim().Split();
            List<Specialization> specializations = new List<Specialization>();

            for (int i = 1; i < specializationMessageSplitted.Length; i++)
            {
                Specialization specialization = new Specialization(specializationMessageSplitted[i]);
                specializations.Add(specialization);
            }

            Entrepreneur juan = new Entrepreneur(this.juanId, this.nameMessage.Text, this.ageMessage.Text, this.location, this.headingMessage.Text, habilitations, specializations);
            Entrepreneur.entrepeneurList.Add(this.juanId);

            // The user must be in the list of entrepreneurs to be registered.
            UserId idExpected = this.nameMessage.Id;
            int indexnameUser = Entrepreneur.entrepeneurList.IndexOf(this.nameMessage.Id);
            Assert.AreEqual(Entrepreneur.entrepeneurList[indexnameUser], idExpected);

            // Evaluate if the habilitations, specializations and name are registered correctly.
            string nameExpected = this.nameMessage.Text;
            Assert.AreEqual(habilitations, juan.Habilitation);
            Assert.AreEqual(specializations, juan.Specialization);
            Assert.AreEqual(nameExpected, juan.Name);
        }
    }
}