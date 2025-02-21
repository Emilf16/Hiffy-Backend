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
    public class EstadoTareasMap : IEntityTypeConfiguration<EstadoTareas>
    {
        public void Configure(EntityTypeBuilder<EstadoTareas> builder)
        {
            builder.ToTable("EstadoTareas")
                .HasKey(x => x.IdEstadoTarea);
        }
    }
}
