using Hiffy_Entidades.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace Hiffy_Datos.Mapping
{
    public class TipoServicioMap : IEntityTypeConfiguration<TipoServicio>
    {
        public void Configure(EntityTypeBuilder<TipoServicio> builder)
        {
            builder.ToTable("TipoServicio")
                .HasKey(x => x.IdTipoServicio);
        }
    }
}
