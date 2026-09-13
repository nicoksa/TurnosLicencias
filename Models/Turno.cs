namespace TurnosLicencias.Models
{
    /// <summary>
    /// Representa un turno reservado en la Oficina de Licencias de Conducir.
    /// </summary>
    public class Turno
    {
        public int Id { get; set; }

        /// <summary>Nombre y apellido del ciudadano.</summary>
        public string NombreApellido { get; set; } = string.Empty;

        /// <summary>Documento Nacional de Identidad.</summary>
        public string Dni { get; set; } = string.Empty;

        /// <summary>Teléfono de contacto (opcional).</summary>
        public string? Telefono { get; set; }

        /// <summary>Número de oficina: 1 o 2.</summary>
        public int Oficina { get; set; }

        /// <summary>Fecha del turno (sin componente de hora).</summary>
        public DateTime Fecha { get; set; }

        /// <summary>Hora del turno, dentro de la grilla de 15 minutos.</summary>
        public TimeSpan Hora { get; set; }
    }
}
