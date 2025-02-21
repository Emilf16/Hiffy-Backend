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
    public class TipoTareaMap : IEntityTypeConfiguration<TipoTarea>
    {
        public void Configure(EntityTypeBuilder<TipoTarea> builder)
        {
            builder.ToTable("TipoTarea")
                .HasKey(x => x.IdTipoTarea);
        }
    }
}
