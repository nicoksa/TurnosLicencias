namespace TurnosLicencias.Helpers
{

    /// Genera automáticamente la grilla de horarios disponibles para turnos.

    public static class HorarioHelper
    {
        public static readonly TimeSpan PrimerTurno = new(7, 45, 0);
        public static readonly TimeSpan UltimoTurno = new(13, 15, 0);
        public static readonly TimeSpan Duracion = TimeSpan.FromMinutes(15);

        /// <summary>
        /// Devuelve la lista de horarios posibles entre las 07:45 y las 13:00,
        /// en pasos de 15 minutos (ambos extremos incluidos).
        /// </summary>
        public static List<TimeSpan> GenerarHorarios()
        {
            var horarios = new List<TimeSpan>();
            var actual = PrimerTurno;

            while (actual <= UltimoTurno)
            {
                horarios.Add(actual);
                actual = actual.Add(Duracion);
            }

            return horarios;
        }

        /// Indica si la fecha es un día hábil (no sábado ni domingo).
        public static bool EsDiaHabil(DateTime fecha)
        {
            return fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday;
        }
    }
}
