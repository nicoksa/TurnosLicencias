using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Diagnostics;
using TurnosLicencias.Helpers;
using TurnosLicencias.Models;

namespace TurnosLicencias.Services
{
 
    /// Genera el listado del día como un archivo PDF real (una página para la
    /// Oficina 1 y otra para la Oficina 2) y lo abre con el lector de PDF
    /// predeterminado de Windows. Desde ahí el usuario imprime con el botón
    /// de impresión de su propio lector (Edge, Adobe Reader, etc.).

    public class PrintService
    {
        private readonly DateTime _fecha;
        private readonly List<Turno> _turnosOficina1;
        private readonly List<Turno> _turnosOficina2;

        private const double MargenIzquierdo = 50;
        private const double AltoFila = 26;
        private const double AnchoColHora = 80;
        private const double AnchoColNombre = 300;
        private const double AnchoColDni = 110;

        // Margen inferior: cuando "Y" pasa de este valor ya no entra otra fila
        // en la página y hay que pasar a una nueva (usado solo en el listado
        // alfabético, que puede tener muchas más filas que el listado por oficina).
        private const double MargenInferiorPagina = 780;

        private static readonly XFont FontTitulo = new("Arial", 18, XFontStyleEx.Bold);
        private static readonly XFont FontSubtitulo = new("Arial", 13, XFontStyleEx.Bold);
        private static readonly XFont FontEncabezadoTabla = new("Arial", 12, XFontStyleEx.Bold);
        private static readonly XFont FontTexto = new("Arial", 12, XFontStyleEx.Regular);
        private static readonly XFont FontVacio = new("Arial", 12, XFontStyleEx.Italic);

        private const double AnchoColOficina = 90;

        public PrintService(DateTime fecha, List<Turno> turnosOficina1, List<Turno> turnosOficina2)
        {
            _fecha = fecha;
            _turnosOficina1 = turnosOficina1.OrderBy(t => t.Hora).ToList();
            _turnosOficina2 = turnosOficina2.OrderBy(t => t.Hora).ToList();
        }

        // ============================================================
        //  LISTADO POR OFICINA 
        // ============================================================


        /// Genera el PDF del listado del día y lo abre con la aplicación
        /// predeterminada de Windows para archivos PDF.

        public void GenerarYAbrirPdf()
        {
            using var documento = new PdfDocument();
            documento.Info.Title = $"Turnos {_fecha:dd-MM-yyyy}";

            DibujarPaginaOficina(documento, "Oficina 1", _turnosOficina1);
            DibujarPaginaOficina(documento, "Oficina 2", _turnosOficina2);

            var carpetaTemporal = Path.Combine(Path.GetTempPath(), "TurnosLicencias");
            Directory.CreateDirectory(carpetaTemporal);
            var rutaArchivo = Path.Combine(carpetaTemporal, $"Turnos_{_fecha:yyyy-MM-dd}.pdf");

            documento.Save(rutaArchivo);

            // Abre el PDF con el lector predeterminado del sistema (Edge, Adobe, etc.)
            Process.Start(new ProcessStartInfo
            {
                FileName = rutaArchivo,
                UseShellExecute = true
            });
        }

        private void DibujarPaginaOficina(PdfDocument documento, string tituloOficina, List<Turno> turnos)
        {
            var pagina = documento.AddPage();
            pagina.Size = PdfSharp.PageSize.A4;

            using var gfx = XGraphics.FromPdfPage(pagina);
            double y = 50;

            gfx.DrawString("Municipalidad - Oficina de Licencias de Conducir",
                FontTitulo, XBrushes.Black, new XPoint(MargenIzquierdo, y));
            y += 35;

            gfx.DrawString($"{tituloOficina}  -  Turnos del día {_fecha:dd/MM/yyyy}",
                FontSubtitulo, XBrushes.Black, new XPoint(MargenIzquierdo, y));
            y += 35;

            DibujarTabla(gfx, ref y, turnos);
        }

        private void DibujarTabla(XGraphics gfx, ref double y, List<Turno> turnos)
        {
            double anchoTotal = AnchoColHora + AnchoColNombre + AnchoColDni;
            double xHora = MargenIzquierdo;
            double xNombre = xHora + AnchoColHora;
            double xDni = xNombre + AnchoColNombre;

            var lapiz = new XPen(XColors.Black, 1.0);
            var fondoEncabezado = new XSolidBrush(XColor.FromArgb(225, 225, 225));

            // --- Fila de encabezado ---
            gfx.DrawRectangle(fondoEncabezado, xHora, y, anchoTotal, AltoFila);
            DibujarFilaConLineas(gfx, lapiz, xHora, xNombre, xDni, y, anchoTotal);

            DibujarTextoCentradoVerticalmente(gfx, "Hora", FontEncabezadoTabla, XBrushes.Black, xHora + 8, y);
            DibujarTextoCentradoVerticalmente(gfx, "Nombre y Apellido", FontEncabezadoTabla, XBrushes.Black, xNombre + 8, y);
            DibujarTextoCentradoVerticalmente(gfx, "DNI", FontEncabezadoTabla, XBrushes.Black, xDni + 8, y);
            y += AltoFila;

 
            var limite = new TimeSpan(13, 0, 0);
            var horarios = HorarioHelper.GenerarHorarios()
                .Where(h => h <= limite)
                .ToList();

            foreach (var t in turnos)
            {
                if (!horarios.Contains(t.Hora))
                    horarios.Add(t.Hora);
            }
            horarios = horarios.OrderBy(h => h).ToList();

            var turnosPorHora = turnos.ToDictionary(t => t.Hora);

            foreach (var hora in horarios)
            {
                DibujarFilaConLineas(gfx, lapiz, xHora, xNombre, xDni, y, anchoTotal);

                // La hora se imprime siempre, haya turno o no
                DibujarTextoCentradoVerticalmente(gfx, hora.ToString(@"hh\:mm"), FontTexto, XBrushes.Black, xHora + 8, y);

                // Nombre y DNI solo si el horario está ocupado; si no, quedan en blanco
                if (turnosPorHora.TryGetValue(hora, out var t))
                {
                    DibujarTextoCentradoVerticalmente(gfx, t.NombreApellido, FontTexto, XBrushes.Black, xNombre + 8, y);
                    DibujarTextoCentradoVerticalmente(gfx, t.Dni, FontTexto, XBrushes.Black, xDni + 8, y);
                }

                y += AltoFila;
            }
        }

        /// <summary>Dibuja el rectángulo exterior de la fila y las líneas verticales que separan las columnas.</summary>
        private static void DibujarFilaConLineas(XGraphics gfx, XPen lapiz, double xHora, double xNombre, double xDni, double y, double anchoTotal)
        {
            gfx.DrawRectangle(lapiz, xHora, y, anchoTotal, AltoFila);
            gfx.DrawLine(lapiz, xNombre, y, xNombre, y + AltoFila);
            gfx.DrawLine(lapiz, xDni, y, xDni, y + AltoFila);
        }

        private static void DibujarTextoCentradoVerticalmente(XGraphics gfx, string texto, XFont fuente, XBrush pincel, double x, double yFila)
        {
            gfx.DrawString(texto, fuente, pincel, new XPoint(x, yFila + (AltoFila / 2) + (fuente.Height / 3)));
        }

        // ============================================================
        //  LISTADO ALFABÉTICO
        // ============================================================

        /// <summary>
        /// Guarda el estado "vivo" de dibujo (página actual, gráfico actual y
        /// posición vertical) para poder cambiar de página desde funciones
        /// locales sin usar parámetros ref/out (que no está permitido ahí).
        /// Se usa únicamente en el listado alfabético.
        /// </summary>
        private class EstadoDibujo
        {
            public PdfDocument Documento = null!;
            public PdfPage Pagina = null!;
            public XGraphics Gfx = null!;
            public double Y;

            public void NuevaPagina()
            {
                Gfx.Dispose();
                Pagina = Documento.AddPage();
                Pagina.Size = PdfSharp.PageSize.A4;
                Gfx = XGraphics.FromPdfPage(Pagina);
                Y = 50;
            }
        }

        public void GenerarYAbrirPdfOrdenAlfabetico()
        {
            var turnosCombinados = _turnosOficina1
                .Concat(_turnosOficina2)
                .OrderBy(t => t.NombreApellido, StringComparer.OrdinalIgnoreCase)
                .ToList();

            using var documento = new PdfDocument();
            documento.Info.Title = $"Turnos {_fecha:dd-MM-yyyy} (orden alfabético)";

            var estado = new EstadoDibujo { Documento = documento };
            estado.Pagina = documento.AddPage();
            estado.Pagina.Size = PdfSharp.PageSize.A4;
            estado.Gfx = XGraphics.FromPdfPage(estado.Pagina);
            estado.Y = 50;

            estado.Gfx.DrawString("Municipalidad - Oficina de Licencias de Conducir",
                FontTitulo, XBrushes.Black, new XPoint(MargenIzquierdo, estado.Y));
            estado.Y += 35;

            estado.Gfx.DrawString($"Turnos del día {_fecha:dd/MM/yyyy}  -  Orden alfabético ",
                FontSubtitulo, XBrushes.Black, new XPoint(MargenIzquierdo, estado.Y));
            estado.Y += 35;

            DibujarTablaAlfabetica(estado, turnosCombinados);

            estado.Gfx.Dispose();

            var carpetaTemporal = Path.Combine(Path.GetTempPath(), "TurnosLicencias");
            Directory.CreateDirectory(carpetaTemporal);
            var rutaArchivo = Path.Combine(carpetaTemporal, $"Turnos_{_fecha:yyyy-MM-dd}_Alfabetico.pdf");

            documento.Save(rutaArchivo);

            Process.Start(new ProcessStartInfo
            {
                FileName = rutaArchivo,
                UseShellExecute = true
            });
        }

        private void DibujarTablaAlfabetica(EstadoDibujo estado, List<Turno> turnos)
        {
            double anchoTotal = AnchoColNombre + AnchoColDni;
            double xNombre = MargenIzquierdo;
            double xDni = xNombre + AnchoColNombre;

            var lapiz = new XPen(XColors.Black, 1.0);
            var fondoEncabezado = new XSolidBrush(XColor.FromArgb(225, 225, 225));

            void DibujarEncabezado()
            {
                estado.Gfx.DrawRectangle(fondoEncabezado, xNombre, estado.Y, anchoTotal, AltoFila);
                DibujarFilaConLineasAlfabetico(estado.Gfx, lapiz, xNombre, xDni, estado.Y, anchoTotal);

                DibujarTextoCentradoVerticalmente(estado.Gfx, "Nombre y Apellido", FontEncabezadoTabla, XBrushes.Black, xNombre + 8, estado.Y);
                DibujarTextoCentradoVerticalmente(estado.Gfx, "DNI", FontEncabezadoTabla, XBrushes.Black, xDni + 8, estado.Y);
                estado.Y += AltoFila;
            }

            void NuevaPaginaSiNecesario()
            {
                if (estado.Y + AltoFila <= MargenInferiorPagina) return;

                estado.NuevaPagina();
                DibujarEncabezado();
            }

            // --- Encabezado ---
            DibujarEncabezado();

            if (turnos.Count == 0)
            {
                DibujarFilaConLineasAlfabetico(estado.Gfx, lapiz, xNombre, xDni, estado.Y, anchoTotal);
                DibujarTextoCentradoVerticalmente(estado.Gfx, "(sin turnos reservados para este día)", FontVacio, XBrushes.Gray, xNombre + 8, estado.Y);
                estado.Y += AltoFila;
                return;
            }

            foreach (var t in turnos)
            {
                NuevaPaginaSiNecesario();

                DibujarFilaConLineasAlfabetico(estado.Gfx, lapiz, xNombre, xDni, estado.Y, anchoTotal);

                DibujarTextoCentradoVerticalmente(estado.Gfx, t.NombreApellido, FontTexto, XBrushes.Black, xNombre + 8, estado.Y);
                DibujarTextoCentradoVerticalmente(estado.Gfx, t.Dni, FontTexto, XBrushes.Black, xDni + 8, estado.Y);

                estado.Y += AltoFila;
            }
        }

        private static void DibujarFilaConLineasAlfabetico(XGraphics gfx, XPen lapiz, double xNombre, double xDni, double y, double anchoTotal)
        {
            gfx.DrawRectangle(lapiz, xNombre, y, anchoTotal, AltoFila);
            gfx.DrawLine(lapiz, xDni, y, xDni, y + AltoFila);
        }
    }
}