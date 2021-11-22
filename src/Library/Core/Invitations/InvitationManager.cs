using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Linq;
using Library.Utils;

namespace Library.Core.Invitations
{
    /// <summary>
    /// This class acts as the highest level of abstraccion in invitation handling.
    /// </summary>
    public class InvitationManager
    {
        /// <summary>
        /// Gets a set of functions to remove invitations
        /// from their correspondent <see cref="InvitationList{T}" /> instances.
        /// </summary>
        private IList<Action<string>> removers = new List<Action<string>>();

        /// <summary>
        /// Adds a remover into the set.
        /// </summary>
        /// <param name="remover">The remover.</param>
        public void AddRemover(Action<string> remover)
        {
            if(!removers.Contains(remover))
                removers.Add(remover);
        }

        /// <summary>
        /// A list of all the invitations.
        /// </summary>
        private IList<Invitation> invitations = new List<Invitation>();

        /// <summary>
        /// Gets the number of invitations.
        /// </summary>
        public int InvitationCount => invitations.Count;

        /// <summary>
        /// Adds an invitation into the list.
        /// </summary>
        /// <param name="code">The invitation´s code.</param>
        /// <param name="f">Function that takes string like a parameter, and return an Invitation.</param>
        public void CreateInvitation<T>(string code, Func<string, T> f) where T : Invitation
        {
            if (f != null)
            {
                T invitation = f(code);
                if (!invitations.Any(inv => inv.Code == code))
                {
                    invitations.Add(invitation);
                    Singleton<InvitationList<T>>.Instance.AddInvitation(invitation);
                }
            }
        }

        /// <summary>
        /// Adds an invitation directly into the list.<br />
        /// This function should only be used by the <see cref="InvitationList{T}.SetInvitations(IEnumerable{T})" /> function
        /// to load invitations which come from JSON data.<br />
        /// For other contexts, use <see cref="InvitationManager.CreateInvitation{T}(string, Func{string, T})" />.
        /// </summary>
        /// <param name="invitation">The invitation to load.</param>
        public void AddInvitation(Invitation invitation)
        {
            invitations.Add(invitation);
        }

        /// <summary>
        /// Validates an invitation with a user id, returning the response message of the validation.
        /// </summary>
        /// <param name="invitationCode">The invitation's code.</param>
        /// <param name="userId">The id of the user who validated the invitation.</param>
        /// <returns>The response message of the validation of the invitation, or an error message if there wasn't.</returns>
        public string? ValidateInvitation(string invitationCode, string userId)
        {
            if (
                invitations.Where(invitation => invitation.Code == invitationCode).FirstOrDefault()
                is Invitation invitation)
            {
                string r = invitation.Validate(userId);
                invitations.Remove(invitation);
                foreach(Action<string> f in removers)
                {
                    f(invitationCode);
                }
                return r;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loads the invitations from a JSON file.
        /// </summary>
        /// <param name="path">The path of the main directory.</param>
        /// <param name="fileName">The file name associated with the type of the invitations.</param>
        /// <typeparam name="T">The type of the invitations to load.</typeparam>
        public void LoadInvitations<T>(string path, string fileName) where T : Invitation
        {
            T[] invitations = SerializationUtils.DeserializeJSON<T[]>(path + "/invitations/" + fileName);
            Singleton<InvitationList<T>>.Instance.SetInvitations(invitations);
        }
    }
}
