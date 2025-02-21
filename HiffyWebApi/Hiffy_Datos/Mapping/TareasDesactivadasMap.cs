using Hiffy_Entidades.Entidades;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Datos.Mapping
{
    public class TareasDesactivadasMap : IEntityTypeConfiguration<TareasDesactivadas>
    {
        public void Configure(EntityTypeBuilder<TareasDesactivadas> builder)
        {
            builder.ToTable("TareasDesactivadas")
                .HasKey(x => x.IdTareaDesactivada);
        }
    }
}
