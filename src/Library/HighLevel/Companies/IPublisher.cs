using System.Collections.Generic;
using System.Collections.ObjectModel;
using Library.HighLevel.Accountability;
using Library.HighLevel.Materials;
using Ucu.Poo.Locations.Client;

namespace Library.HighLevel.Companies
{
    /// <summary>
    /// This interface represents the responsibility of managing material publications.
    /// We created this interface because of DIP, that way the classes depend of an abstraction.
    /// </summary>
    public interface IPublisher
    {
        /// <summary>
        /// Gets a private list of the publications.
        /// </summary>
        protected List<MaterialPublication> Publications { get; }

        /// <summary>
        /// Gets a public read-only list of the publications.
        /// </summary>
        public ReadOnlyCollection<MaterialPublication> PublicationsReadOnly => this.Publications.AsReadOnly();

        /// <summary>
        /// Publishes a material.
        /// </summary>
        /// <param name="material">The material to publish.</param>
        /// <param name="amount">The amount of material.</param>
        /// <param name="price">The price of the material.</param>
        /// <param name="location">The pick-up location of the material.</param>
        /// <param name="keywords">The keywords of the material.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool PublishMaterial(Material material, Amount amount, Price price, Location location, List<string> keywords)
        {
            if (MaterialPublication.CreateInstance(material, amount, price, location, keywords) is MaterialPublication publication)
            {
                this.Publications.Add(publication);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a material publication.
        /// </summary>
        /// <param name="index">The index of the publication.</param>
        /// <returns>Whether the removal was successful.</returns>
        public bool RemovePublication(int index)
        {
            if (index < 0 || index >= this.Publications.Count)
            {
                return false;
            }

            this.Publications.RemoveAt(index);
            return true;
        }
    }
}