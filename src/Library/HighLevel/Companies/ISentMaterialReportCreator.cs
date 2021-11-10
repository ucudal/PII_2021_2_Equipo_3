using System;
using System.Collections.Generic;
using System.Linq;
using Library.HighLevel.Accountability;

namespace Library.HighLevel.Companies
{
    /// <summary>
    /// This interface represents the responsibility of creating material reports.
    /// Created because of SRP.
    /// </summary>
    public interface ISentMaterialReportCreator
    {
        /// <summary>
        /// Gets the list of material sales.
        /// </summary>
        protected List<MaterialSalesLine> MaterialSales { get; }

        /// <summary>
        /// Builds a <see cref="SentMaterialReport" /> with the material sales that occured after a certain <see cref="DateTime" />.
        /// </summary>
        /// <param name="dateTime">The lower limit of the moment the sales happened.</param>
        /// <returns>The <see cref="SentMaterialReport" />.</returns>
        public SentMaterialReport GetMaterialReport(DateTime dateTime) =>
            new SentMaterialReport(this.MaterialSales.Where(line => line.DateTime < dateTime).ToList().AsReadOnly());
    }
}
