using TurnosLicencias.Data;
using TurnosLicencias.Forms;

namespace TurnosLicencias
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal de la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Configuración estándar de WinForms para .NET 8
            ApplicationConfiguration.Initialize();

            // Crea la base de datos SQLite si todavía no existe
            AppDbContext.Inicializar();

            Application.Run(new MainForm());
        }
    }
}
