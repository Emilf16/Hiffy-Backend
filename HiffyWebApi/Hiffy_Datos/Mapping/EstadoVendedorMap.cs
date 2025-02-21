using Hiffy_Entidades.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Datos.Mapping
{
    public class EstadoVendedorMap : IEntityTypeConfiguration<EstadoVendedor>
    {
        public void Configure(EntityTypeBuilder<EstadoVendedor> builder)
        {
            builder.ToTable("EstadoVendedor")
                .HasKey(x => x.IdEstadoVendedor);
        }
    }
}
