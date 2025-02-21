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
    public class DispositivoInteligenteMap : IEntityTypeConfiguration<DispositivoInteligente>
    {
        public void Configure(EntityTypeBuilder<DispositivoInteligente> builder)
        {
            builder.ToTable("DispositivoInteligente")
                .HasKey(x => x.IdDispositivo);
        }
    }
}
