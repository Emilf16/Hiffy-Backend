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
    public class CertificacionVendedorMap : IEntityTypeConfiguration<CertificacionVendedor>
    {
        public void Configure(EntityTypeBuilder<CertificacionVendedor> builder)
        {
            builder.ToTable("CertificacionVendedor")
                .HasKey(x => x.IdCertificacion);
        }
    }
}
