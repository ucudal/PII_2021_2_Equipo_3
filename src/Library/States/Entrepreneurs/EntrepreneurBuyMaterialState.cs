using System;
using Library.HighLevel.Accountability;
using Library.HighLevel.Entrepreneurs;
using Library.HighLevel.Companies;
using Library.Core.Processing;
using Library.InputHandlers;
using Library.InputHandlers.Abstractions;
using Library.Utils;
using System.Linq;
using Library.HighLevel.Materials;

namespace Library.States.Entrepreneurs
{
#warning TODO: make sure the entrepreneur has the chance to buy the remaining stock if the asked amount is higher than the stock
    /// <summary>
    /// This class has the responsibility of allow an entrepreneur user buy a material.
    /// </summary>
    public class EntrepreneurBuyMaterialState : InputProcessorState<(AssignedMaterialPublication, Amount)>
    {
        /// <summary>
        /// Initializes an instance of <see cref="EntrepreneurBuyMaterialState" /> class.
        /// </summary>
        /// <param name="id">The user´s id.</param>
        public EntrepreneurBuyMaterialState(string id) : base(
            exitState: () => (new EntrepreneurInitialMenuState(id, null), null),
            nextState: result =>
                {
                    var newState = new EntrepreneurInitialMenuState(id, null);
                    if (result.Item1.Publication.Amount.Substract(result.Item2) == 0)
                    {
                        DateTime time = DateTime.Today;
                        BoughtMaterialLine purchase = new BoughtMaterialLine(result.Item1.Company.Name, result.Item1.Publication.Material, time, result.Item1.Publication.Price, result.Item1.Publication.Amount);
                        Entrepreneur? entrepreneur = Singleton<EntrepreneurManager>.Instance.GetById(id);
                        entrepreneur!.BoughtMaterials.Add(purchase);

                        MaterialSalesLine sale = new MaterialSalesLine(result.Item1.Publication.Material, result.Item1.Publication.Amount, result.Item1.Publication.Price, time);
                        result.Item1.Company.MaterialSales.Add(sale);

                        return (newState, $"La compra se ha concretado, para coordinar el envío o el retiro del material te envío la información de contacto de la empresa:\nNúmero Telefónico: {result.Item1.Company.ContactInfo.PhoneNumber}\nCorreo Electrónico: {result.Item1.Company.ContactInfo.Email}\n{newState.GetDefaultResponse()}");
                    }
                    return (newState, $"Las cantidades del material y la compra, son inválidas entre sí.\n{newState.GetDefaultResponse()}");
                },
            processor: new CollectDataProcessor(id)
        )
        { }

        private class CollectDataProcessor : FormProcessor<(AssignedMaterialPublication, Amount)>
        {
            private Company? company;
            private MaterialPublication? publication;
            private Amount? amount;

            public CollectDataProcessor(string id)
            {
                this.inputHandlers = new InputHandler[]
                {
                    ProcessorHandler<string>.CreateInfallibleInstance(
                        cName =>
                        {
                            if (Singleton<CompanyManager>.Instance.GetByName(cName) is Company rCompany)
                            {
                                this.company = rCompany;
                            }
                        },
                        new BasicStringProcessor(() => "Por favor ingresa el nombre de la compañía que tiene publicado el material.")
                    ),
                    ProcessorHandler<string>.CreateInfallibleInstance(
                        pName =>
                        {
                            if (this.company!.Publications.Any(p => p.Material.Name == pName))
                            {
                                this.publication = this.company.Publications.Where(p => p.Material.Name == pName).FirstOrDefault();
                            }
                        },
                        new BasicStringProcessor(() => "Por favor ingresa el nombre del material de la publicación.")
                    ),
                    ProcessorHandler<Amount>.CreateInfallibleInstance(
                        a => this.amount = a,
                        new AmountProcessor(
                            () => "Por favor ingresa el valor numérico de la cantidad de material que desea adquirir y su unidad. (por ejemplo: 7, kg)",
                            () => this.publication!.Material.Measure
                        )
                    )
                };
            }

            protected override Result<(AssignedMaterialPublication, Amount), string> getResult() =>
                Result<(AssignedMaterialPublication, Amount), string>.Ok((new AssignedMaterialPublication(this.publication.Unwrap(), this.company.Unwrap()), this.amount.Unwrap()));
        }
    }
}