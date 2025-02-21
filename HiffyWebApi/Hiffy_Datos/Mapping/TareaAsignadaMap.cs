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
    public class TareaAsignadaMap : IEntityTypeConfiguration<TareaAsignada>
    {
        public void Configure(EntityTypeBuilder<TareaAsignada> builder)
        {
            builder.ToTable("TareaAsignada")
                .HasKey(x => x.IdTareaAsignada);
        }
    }
}
