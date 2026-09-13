using TurnosLicencias.Data;
using TurnosLicencias.Helpers;
using TurnosLicencias.Models;
using TurnosLicencias.Services;

namespace TurnosLicencias.Forms
{
    public class MainForm : Form
    {
        private static readonly Color ColorLibre = Color.FromArgb(220, 245, 220);
        private static readonly Color ColorLibreHover = Color.FromArgb(190, 230, 190);
        private static readonly Color ColorOcupado = Color.FromArgb(255, 224, 178);
        private static readonly Color ColorOcupadoHover = Color.FromArgb(255, 204, 128);
        private static readonly Color ColorEncabezado = Color.FromArgb(25, 55, 90);
        private static readonly Color ColorBloqueado = Color.FromArgb(210, 210, 210);
        private static readonly Color ColorBloqueadoHover = Color.FromArgb(190, 190, 190);

        private DateTimePicker _dtpFecha = new();
        private Button _btnDiaAnterior = new();
        private Button _btnDiaSiguiente = new();
        private FlowLayoutPanel _panelOficina1 = new();
        private FlowLayoutPanel _panelOficina2 = new();
        private Button _btnProximoDisponible = new();
        private Button _btnImprimir = new();
        private Button _btnActualizar = new();
        private Label _lblTitulo = new();
        private Button _btnImprimirAlfabetico = new();

        public MainForm()
        {
            ConstruirInterfaz();
            CargarTurnos();
        }

        private void ConstruirInterfaz()
        {
            Text = "Turnos - Oficina de Licencias de Conducir";
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 650);
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.White;



            // ---- Panel superior (fecha, búsqueda, acciones) ----
            var panelSuperior = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(20, 15, 20, 15)
            };

            var lblFecha = new Label
            {
                Text = "Fecha:",
                Left = 20,
                Top = 28,
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            _dtpFecha.SetBounds(125, 22, 150, 30);
            _dtpFecha.Format = DateTimePickerFormat.Short;
            _dtpFecha.Font = new Font("Segoe UI", 11F);
            _dtpFecha.ValueChanged += (s, e) => CargarTurnos();

            _btnDiaAnterior.Text = "◀";
            _btnDiaAnterior.SetBounds(90, 20, 32, 34);
            EstilizarBotonAccion(_btnDiaAnterior, Color.FromArgb(120, 120, 120));
            _btnDiaAnterior.Click += (s, e) => _dtpFecha.Value = DiaHabilAnterior(_dtpFecha.Value);

            _btnDiaSiguiente.Text = "▶";
            _btnDiaSiguiente.SetBounds(280, 20, 32, 34);
            EstilizarBotonAccion(_btnDiaSiguiente, Color.FromArgb(120, 120, 120));
            _btnDiaSiguiente.Click += (s, e) => _dtpFecha.Value = DiaHabilSiguiente(_dtpFecha.Value);

            _btnActualizar.Text = "Actualizar";
            _btnActualizar.SetBounds(325, 20, 110, 36);
            EstilizarBotonAccion(_btnActualizar, Color.FromArgb(96, 96, 96));
            _btnActualizar.Click += (s, e) => CargarTurnos();

            _btnProximoDisponible.Text = "Próximo turno disponible";
            _btnProximoDisponible.SetBounds(445, 20, 220, 36);
            EstilizarBotonAccion(_btnProximoDisponible, Color.FromArgb(25, 118, 210));
            _btnProximoDisponible.Click += BtnProximoDisponible_Click;

            _btnImprimir.Text = "Imprimir listado del día";
            _btnImprimir.SetBounds(675, 20, 200, 36);
            EstilizarBotonAccion(_btnImprimir, Color.FromArgb(69, 90, 100));
            _btnImprimir.Click += BtnImprimir_Click;

            _btnImprimirAlfabetico.Text = " Ver por orden alfabético";
            _btnImprimirAlfabetico.SetBounds(885, 20, 200, 36);
            EstilizarBotonAccion(_btnImprimirAlfabetico, Color.FromArgb(69, 90, 100));
            _btnImprimirAlfabetico.Click += BtnImprimirAlfabetico_Click;

            panelSuperior.Controls.AddRange(new Control[]
            {
            lblFecha, _btnDiaAnterior, _dtpFecha, _btnDiaSiguiente,
            _btnActualizar, _btnProximoDisponible, _btnImprimir, _btnImprimirAlfabetico
            });

            // ---- Título ----
            _lblTitulo.Dock = DockStyle.Top;
            _lblTitulo.Height = 36;
            _lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            _lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _lblTitulo.BackColor = Color.White;

            // ---- Panel central con las dos columnas ----
            var panelCentral = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(15)
            };
            panelCentral.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panelCentral.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var columnaOficina1 = CrearColumnaOficina("Oficina 1", _panelOficina1);
            var columnaOficina2 = CrearColumnaOficina("Oficina 2", _panelOficina2);

            panelCentral.Controls.Add(columnaOficina1, 0, 0);
            panelCentral.Controls.Add(columnaOficina2, 1, 0);

            Controls.Add(panelCentral);
            Controls.Add(_lblTitulo);
            Controls.Add(panelSuperior);
        }

        private static void EstilizarBotonAccion(Button boton, Color color)
        {
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.BackColor = color;
            boton.ForeColor = Color.White;
            boton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            boton.Cursor = Cursors.Hand;
        }

        private Panel CrearColumnaOficina(string titulo, FlowLayoutPanel panelContenedor)
        {
            var contenedor = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            var encabezado = new Label
            {
                Text = titulo,
                Dock = DockStyle.Top,
                Height = 44,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                BackColor = ColorEncabezado,
                ForeColor = Color.White
            };

            panelContenedor.Dock = DockStyle.Fill;
            panelContenedor.FlowDirection = FlowDirection.TopDown;
            panelContenedor.WrapContents = false;
            panelContenedor.AutoScroll = true;
            panelContenedor.Padding = new Padding(10);
            panelContenedor.BackColor = Color.White;

            contenedor.Controls.Add(panelContenedor);
            contenedor.Controls.Add(encabezado);
            return contenedor;
        }

    
        /// Recarga la grilla completa de horarios para la fecha seleccionada,
        /// marcando cada uno como LIBRE u OCUPADO según lo guardado en la base.
  
        private void CargarTurnos()
        {
            var fecha = _dtpFecha.Value.Date;

            if (!HorarioHelper.EsDiaHabil(fecha))
            {
                _lblTitulo.Text = $"{fecha:dddd dd/MM/yyyy} — Fin de semana, no se atiende";
                _panelOficina1.Controls.Clear();
                _panelOficina2.Controls.Clear();
                return;
            }

            _lblTitulo.Text = $"Turnos del día {fecha:dddd dd/MM/yyyy}";

            using var context = new AppDbContext();
            var turnosDelDia = context.Turnos
                .Where(t => t.Fecha == fecha)
                .ToList();

            var turnosOficina1 = turnosDelDia.Where(t => t.Oficina == 1).ToDictionary(t => t.Hora);
            var turnosOficina2 = turnosDelDia.Where(t => t.Oficina == 2).ToDictionary(t => t.Hora);

            PoblarColumna(_panelOficina1, 1, fecha, turnosOficina1);
            PoblarColumna(_panelOficina2, 2, fecha, turnosOficina2);
        }

        private void PoblarColumna(FlowLayoutPanel panel, int oficina, DateTime fecha,
            Dictionary<TimeSpan, Turno> turnosOcupados)
        {
            panel.SuspendLayout();
            panel.Controls.Clear();

            foreach (var hora in HorarioHelper.GenerarHorarios())
            {
                var ocupado = turnosOcupados.TryGetValue(hora, out var turno);
                panel.Controls.Add(CrearBotonHorario(oficina, fecha, hora, ocupado, turno));
            }

            panel.ResumeLayout();
        }

        private Button CrearBotonHorario(int oficina, DateTime fecha, TimeSpan hora, bool ocupado, Turno? turno)
        {
            var boton = new Button
            {
                Width = 380,
                Height = 44,
                Margin = new Padding(3),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F),
                Cursor = Cursors.Hand
            };
            boton.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);

            var horaTexto = hora.ToString(@"hh\:mm");

            if (ocupado && turno != null)
            {
                boton.Text = $"{horaTexto}   -   {turno.NombreApellido}";
                boton.BackColor = ColorOcupado;
                boton.FlatAppearance.MouseOverBackColor = ColorOcupadoHover;
                boton.Click += (s, e) => AbrirTurnoExistente(turno);
            }
            else
            {
                boton.Text = $"{horaTexto}   -   LIBRE";
                boton.BackColor = ColorLibre;
                boton.FlatAppearance.MouseOverBackColor = ColorLibreHover;
                boton.Click += (s, e) => AbrirNuevoTurno(oficina, fecha, hora);
            }

            return boton;
        }

        private void AbrirNuevoTurno(int oficina, DateTime fecha, TimeSpan hora)
        {
            using var form = new TurnoForm(oficina, fecha, hora);
            form.ShowDialog(this);
            if (form.HuboCambios) CargarTurnos();
        }

        private void AbrirTurnoExistente(Turno turno)
        {
            using var form = new TurnoForm(turno);
            form.ShowDialog(this);
            if (form.HuboCambios) CargarTurnos();
        }

        /// <summary>
        /// Busca, a partir de la fecha seleccionada (o de hoy, lo que sea más tarde),
        /// el primer horario libre en cualquiera de las dos oficinas y ofrece cargarlo.
        /// </summary>
        private void BtnProximoDisponible_Click(object? sender, EventArgs e)
        {
            // Siempre se busca desde HOY en adelante, sin importar qué fecha
            // esté seleccionada en la grilla en ese momento.
            var fechaInicio = DateTime.Today;

            const int diasAExplorar = 120;
            var horarios = HorarioHelper.GenerarHorarios();

            using var context = new AppDbContext();

            for (int i = 0; i <= diasAExplorar; i++)
            {
                var fecha = fechaInicio.AddDays(i);
                var ocupadosDelDia = context.Turnos.Where(t => t.Fecha == fecha).ToList();

                foreach (var hora in horarios)
                {
                    // Si es hoy, no ofrecer horarios que ya pasaron
                    if (fecha == DateTime.Today && hora <= DateTime.Now.TimeOfDay)
                        continue;

                    foreach (var oficina in new[] { 1, 2 })
                    {
                        bool ocupado = ocupadosDelDia.Any(t => t.Oficina == oficina && t.Hora == hora);
                        if (ocupado) continue;

                        // Encontramos el primer horario libre
                        _dtpFecha.Value = fecha;
                        CargarTurnos();

                        var respuesta = MessageBox.Show(
                            $"Próximo turno disponible:\n\n" +
                            $"Oficina {oficina}\n" +
                            $"{fecha:dddd dd/MM/yyyy}\n" +
                            $"{hora:hh\\:mm} hs\n\n" +
                            "¿Desea cargarlo ahora?",
                            "Próximo turno disponible",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (respuesta == DialogResult.Yes)
                        {
                            AbrirNuevoTurno(oficina, fecha, hora);
                        }

                        return;
                    }
                }
            }

            MessageBox.Show(
                $"No se encontraron turnos disponibles en los próximos {diasAExplorar} días.",
                "Sin disponibilidad",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void BtnImprimir_Click(object? sender, EventArgs e)
        {
            var fecha = _dtpFecha.Value.Date;
            using var context = new AppDbContext();
            var turnosOficina1 = context.Turnos.Where(t => t.Fecha == fecha && t.Oficina == 1).ToList();
            var turnosOficina2 = context.Turnos.Where(t => t.Fecha == fecha && t.Oficina == 2).ToList();

            var servicioImpresion = new PrintService(fecha, turnosOficina1, turnosOficina2);

            try
            {
                servicioImpresion.GenerarYAbrirPdf();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo generar el PDF:\n{ex.Message}", "Error al generar el PDF",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnImprimirAlfabetico_Click(object? sender, EventArgs e)
        {
            var fecha = _dtpFecha.Value.Date;
            using var context = new AppDbContext();
            var turnosOficina1 = context.Turnos.Where(t => t.Fecha == fecha && t.Oficina == 1).ToList();
            var turnosOficina2 = context.Turnos.Where(t => t.Fecha == fecha && t.Oficina == 2).ToList();

            var servicioImpresion = new PrintService(fecha, turnosOficina1, turnosOficina2);

            try
            {
                servicioImpresion.GenerarYAbrirPdfOrdenAlfabetico();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo generar el PDF:\n{ex.Message}", "Error al generar el PDF",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static DateTime DiaHabilAnterior(DateTime fecha)
        {
            var f = fecha.AddDays(-1);
            while (!HorarioHelper.EsDiaHabil(f)) f = f.AddDays(-1);
            return f;
        }

        private static DateTime DiaHabilSiguiente(DateTime fecha)
        {
            var f = fecha.AddDays(1);
            while (!HorarioHelper.EsDiaHabil(f)) f = f.AddDays(1);
            return f;
        }
    }
}