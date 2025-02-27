﻿using Hiffy_Entidades.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Datos.Mapping
{
    public class EstadoFamiliaMap : IEntityTypeConfiguration<EstadoFamilia>
    {
        public void Configure(EntityTypeBuilder<EstadoFamilia> builder)
        {
            builder.ToTable("EstadoFamilia")
                .HasKey(x => x.IdEstadoFamilia);
        }
    }
}
