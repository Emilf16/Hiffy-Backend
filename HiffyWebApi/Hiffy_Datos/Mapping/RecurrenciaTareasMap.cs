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
    public class RecurrenciaTareasMap : IEntityTypeConfiguration<RecurrenciaTareas>
    {
        public void Configure(EntityTypeBuilder<RecurrenciaTareas> builder)
        {
            builder.ToTable("RecurrenciaTareas")
                .HasKey(x => x.IdRecurrencia);
        }
    }
}
