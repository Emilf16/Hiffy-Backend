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
    public  class DispositivoFamiliaMap : IEntityTypeConfiguration<DispositivoFamilia>
    {
        public void Configure(EntityTypeBuilder<DispositivoFamilia> builder)
        {
            builder.ToTable("DispositivoFamilia")
                .HasKey(x => x.IdDispositivoFamilia);
        }
    }
}
