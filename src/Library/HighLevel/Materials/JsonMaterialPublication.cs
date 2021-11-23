using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Library.HighLevel.Accountability;
using Ucu.Poo.Locations.Client;
using Library.Utils;

namespace Library.HighLevel.Materials
{
    /// <summary>
    /// This struct holds the JSON information of a <see cref="MaterialPublication" />.
    /// </summary>
    public struct JsonMaterialPublication : IJsonHolder<MaterialPublication>
    {
        /// <summary>
        /// Gets the publication's material.
        /// </summary>
        public JsonMaterial Material { get; set; }

        /// <summary>
        /// Gets the publication's amount of material.
        /// </summary>
        public JsonAmount Amount { get; set; }

        /// <summary>
        /// Gets the publication's price of the material.
        /// </summary>
        public JsonPrice Price { get; set; }

        /// <summary>
        /// Gets the publication's pick-up location of material.
        /// </summary>
        public Location PickupLocation { get; set; }

        /// <summary>
        /// Gets the type of the material publication.
        /// </summary>
        public JsonMaterialPublicationTypeData Type { get; set; }

        /// <summary>
        /// The list of keywords of the publication material.
        /// </summary>
        public IList<string> Keywords { get; set; }

        /// <inheritdoc />
        public void FromValue(MaterialPublication value)
        {
            this.Material = new JsonMaterial();
            this.Material.FromValue(value.Material);
            this.Amount = new JsonAmount();
            this.Amount.FromValue(value.Amount);
            this.Price = new JsonPrice();
            this.Price.FromValue(value.Price);
            this.PickupLocation = value.PickupLocation;
            this.Type = new JsonMaterialPublicationTypeData();
            this.Type.FromValue(value.Type);
            this.Keywords = value.Keywords.ToList();
        }

        /// <inheritdoc />
        public MaterialPublication ToValue() =>
            MaterialPublication.CreateInstance(
                this.Material.ToValue(),
                this.Amount.ToValue(),
                this.Price.ToValue(),
                this.PickupLocation,
                this.Type.ToValue(),
                this.Keywords).Unwrap();
    }
}