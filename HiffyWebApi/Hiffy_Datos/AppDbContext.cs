using Hiffy_Datos.Mapping;
using Hiffy_Entidades.Entidades;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Datos
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<AreaDelHogar_Familia> AreaDelHogar_Familia { get; set; }
        public virtual DbSet<EstadoFamilia> EstadoFamilia { get; set; }
        public virtual DbSet<EstadoVendedor> EstadoVendedor { get; set; }
        public virtual DbSet<TareaAsignada> TareaAsignada { get; set; }
        public virtual DbSet<DispositivoFamilia> DispositivoFamilia { get; set; } 
        public virtual DbSet<Familia> Familia { get; set; }
        public virtual DbSet<LogActividad> LogActividad { get; set; }
        public virtual DbSet<LogError> LogError { get; set; }
        public virtual DbSet<Menu> Menu { get; set; }
        public virtual DbSet<MenuRol> MenuRol { get; set; }
        public virtual DbSet<Notificacion> Notificacion { get; set; }
        public virtual DbSet<RecurrenciaTareas> RecurrenciaTareas { get; set; }
        public virtual DbSet<Rol> Rol { get; set; }
        public virtual DbSet<RolFamilia> RolFamilia {get; set;}
        public virtual DbSet<TareaDomestica> TareaDomestica { get; set; } 
        public virtual DbSet<TipoTarea> TipoTarea { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }
        public virtual DbSet<TareasDesactivadas> TareasDesactivadas { get; set; }
        public virtual DbSet<EstadoTareas> EstadoTareas { get; set; }
        public virtual DbSet<CertificacionVendedor> CertificacionVendedor { get; set; }
        public virtual DbSet<Servicio> Servicio { get; set; }
        public virtual DbSet<TipoServicio> TipoServicio { get; set; }
        public virtual DbSet<CertificacionTipoServicio> CertificacionTipoServicio { get; set; }
        public virtual DbSet<ContratoPersonal> ContratoPersonal { get; set; }
        public virtual DbSet<TipoDocumento> TipoDocumento { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new AreaDelHogar_FamiliaMap());
            modelBuilder.ApplyConfiguration(new TareaAsignadaMap());
            modelBuilder.ApplyConfiguration(new ContratoPersonalMap());
            modelBuilder.ApplyConfiguration(new DispositivoFamiliaMap()); 
            modelBuilder.ApplyConfiguration(new FamiliaMap());
            modelBuilder.ApplyConfiguration(new LogActividadMap());
            modelBuilder.ApplyConfiguration(new LogErrorMap());
            modelBuilder.ApplyConfiguration(new MenuMap());
            modelBuilder.ApplyConfiguration(new EstadoVendedorMap());
            modelBuilder.ApplyConfiguration(new EstadoFamiliaMap());
            modelBuilder.ApplyConfiguration(new MenuRolMap());
            modelBuilder.ApplyConfiguration(new NotificacionMap());
            modelBuilder.ApplyConfiguration(new RecurrenciaTareasMap());
            modelBuilder.ApplyConfiguration(new RolMap());
            modelBuilder.ApplyConfiguration(new TareaDomesticaMap());
            modelBuilder.ApplyConfiguration(new TipoTareaMap());
            modelBuilder.ApplyConfiguration(new UsuarioMap());
            modelBuilder.ApplyConfiguration(new RolFamiliaMap());
            modelBuilder.ApplyConfiguration(new TareasDesactivadasMap());
            modelBuilder.ApplyConfiguration(new EstadoTareasMap());
            modelBuilder.ApplyConfiguration(new CertificacionVendedorMap());
            modelBuilder.ApplyConfiguration(new CertificacionTipoServicioMap());
            modelBuilder.ApplyConfiguration(new TipoServicioMap());
            modelBuilder.ApplyConfiguration(new ServicioMap());
            modelBuilder.ApplyConfiguration(new TipoDocumentoMap());

        }

    }
}
