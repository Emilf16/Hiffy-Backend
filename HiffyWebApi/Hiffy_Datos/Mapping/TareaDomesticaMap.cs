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
    public class TareaDomesticaMap : IEntityTypeConfiguration<TareaDomestica>
    {
        public void Configure(EntityTypeBuilder<TareaDomestica> builder)
        {
            builder.ToTable("TareaDomestica")
                .HasKey(x => x.IdTareaDomestica);
        }
    }
}
