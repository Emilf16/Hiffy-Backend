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
    public class AreaDelHogar_FamiliaMap : IEntityTypeConfiguration<AreaDelHogar_Familia>
    {
        public void Configure(EntityTypeBuilder<AreaDelHogar_Familia> builder)
        {
            builder.ToTable("AreaDelHogar_Familia")
                .HasKey(x => x.IdAreaFamilia);
        }
    }
}
