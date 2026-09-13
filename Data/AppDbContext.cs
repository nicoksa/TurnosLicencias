using Microsoft.EntityFrameworkCore;
using TurnosLicencias.Models;

namespace TurnosLicencias.Data
{
    /// <summary>
    /// Contexto de base de datos. Usa un único archivo SQLite ubicado junto
    /// al ejecutable, por lo que no requiere ningún servidor ni configuración.
    /// </summary>
    public class AppDbContext : DbContext
    {
        // Se guarda "turnos.db" en la misma carpeta que el .exe
        private static readonly string DbPath =
            Path.Combine(AppContext.BaseDirectory, "turnos.db");

        public DbSet<Turno> Turnos => Set<Turno>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Turno>(entity =>
            {
                entity.ToTable("Turnos");
                entity.HasKey(t => t.Id);

                entity.Property(t => t.NombreApellido)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(t => t.Dni)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(t => t.Telefono)
                      .HasMaxLength(50);

                // Restricción: no se puede repetir el mismo horario
                // en la misma oficina el mismo día.
                entity.HasIndex(t => new { t.Fecha, t.Oficina, t.Hora })
                      .IsUnique();
            });
        }

        /// <summary>
        /// Crea el archivo de base de datos y las tablas si todavía no existen.
        /// Se debe llamar una sola vez, al iniciar la aplicación.
        /// </summary>
        public static void Inicializar()
        {
            using var context = new AppDbContext();
            context.Database.EnsureCreated();
        }
    }
}
