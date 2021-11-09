using NUnit.Framework;
using Library.Core.Invitations;
using Library.HighLevel.Administers;

namespace ProgramTests
{
    /// <summary>
    /// Test if a Company can be invited to the platform.
    /// </summary>
    public class InviteCompanyTest
    {
        /// <summary>
        /// Test SetUp.
        /// </summary>
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// This test proves that as an admin I can create an invitation
        /// As we can't expect a certain invitation code because it's
        /// generated randomly, we check if the list of invitations has
        /// the same number of invitations as expected.
        /// </summary>
        [Test]
        public void InviteCompany()
        {
            Administer.CreateCompanyInvitation();
            int invitationsLength = InvitationManager.invitationsReadOnly.Count;
            Assert.AreEqual(2, invitationsLength);
        }
    }
}