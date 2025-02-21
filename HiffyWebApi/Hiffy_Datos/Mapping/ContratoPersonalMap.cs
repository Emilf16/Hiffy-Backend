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
    public class ContratoPersonalMap : IEntityTypeConfiguration<ContratoPersonal>
    {
        public void Configure(EntityTypeBuilder<ContratoPersonal> builder)
        {
            builder.ToTable("ContratoPersonal")
                .HasKey(x => x.IdContrato);
        }
    }
}
