using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Self;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Catalog
{
    /// <summary>
    /// Represents a product review entity builder
    /// </summary>
    public partial class AppointmentBuilder : NopEntityBuilder<Appointment>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Appointment.CustomerId)).AsInt32().ForeignKey<Customer>()
                .WithColumn(nameof(Appointment.ResourceId)).AsInt32().ForeignKey<Product>();
        }

        #endregion
    }
}