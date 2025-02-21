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
    public class CertificacionTipoServicioMap : IEntityTypeConfiguration<CertificacionTipoServicio>
    {
        public void Configure(EntityTypeBuilder<CertificacionTipoServicio> builder)
        {
            builder.ToTable("CertificacionTipoServicio")
                .HasKey(x => x.IdCertificacionTipoServicio);
        }
    }
}
