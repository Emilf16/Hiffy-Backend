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
    public class RolFamiliaMap : IEntityTypeConfiguration<RolFamilia>
    {
        public void Configure(EntityTypeBuilder<RolFamilia> builder)
        {
            builder.ToTable("RolFamilia")
                .HasKey(x => x.IdRolFamilia);
        }
    }
}
