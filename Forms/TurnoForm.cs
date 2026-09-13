using TurnosLicencias.Data;
using TurnosLicencias.Models;

namespace TurnosLicencias.Forms
{
    /// <summary>
    /// Formulario emergente para cargar un turno nuevo o editar/eliminar uno existente.
    /// </summary>
    public class TurnoForm : Form
    {
        private readonly bool _esNuevo;
        private readonly int _oficina;
        private readonly DateTime _fecha;
        private readonly TimeSpan _hora;
        private int _turnoId;

        private TextBox _txtNombre = new();
        private TextBox _txtDni = new();
        private TextBox _txtTelefono = new();
        private Button _btnGuardar = new();
        private Button _btnEliminar = new();
        private Button _btnCancelar = new();

        /// <summary>Se pone en true si se guardaron o eliminaron cambios, para que la pantalla principal recargue.</summary>
        public bool HuboCambios { get; private set; }

        /// <summary>Constructor para dar de alta un turno nuevo en un horario libre.</summary>
        public TurnoForm(int oficina, DateTime fecha, TimeSpan hora)
        {
            _esNuevo = true;
            _oficina = oficina;
            _fecha = fecha.Date;
            _hora = hora;
            ConstruirInterfaz();
        }

        /// <summary>Constructor para editar o eliminar un turno ya existente.</summary>
        public TurnoForm(Turno turnoExistente)
        {
            _esNuevo = false;
            _turnoId = turnoExistente.Id;
            _oficina = turnoExistente.Oficina;
            _fecha = turnoExistente.Fecha;
            _hora = turnoExistente.Hora;
            ConstruirInterfaz();

            _txtNombre.Text = turnoExistente.NombreApellido;
            _txtDni.Text = turnoExistente.Dni;
            _txtTelefono.Text = turnoExistente.Telefono;
        }

        private void ConstruirInterfaz()
        {
            Text = _esNuevo ? "Nuevo turno" : "Editar turno";
            Width = 420;
            Height = 380;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 10F);
            Padding = new Padding(15);

            var lblInfo = new Label
            {
                Text = $"Oficina {_oficina}   |   {_fecha:dd/MM/yyyy}   |   {_hora:hh\\:mm} hs",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Left = 20,
                Top = 15
            };

            var lblNombre = new Label { Text = "Nombre y apellido:", Left = 20, Top = 55, AutoSize = true };
            _txtNombre.SetBounds(20, 78, 360, 28);

            var lblDni = new Label { Text = "DNI:", Left = 20, Top = 115, AutoSize = true };
            _txtDni.SetBounds(20, 138, 360, 28);

            var lblTelefono = new Label { Text = "Teléfono (opcional):", Left = 20, Top = 175, AutoSize = true };
            _txtTelefono.SetBounds(20, 198, 360, 28);

            _btnGuardar.Text = "Guardar";
            _btnGuardar.SetBounds(20, 250, 110, 42);
            _btnGuardar.BackColor = Color.FromArgb(46, 125, 50);
            _btnGuardar.ForeColor = Color.White;
            _btnGuardar.FlatStyle = FlatStyle.Flat;
            _btnGuardar.Click += BtnGuardar_Click;

            _btnEliminar.Text = "Eliminar turno";
            _btnEliminar.SetBounds(140, 250, 130, 42);
            _btnEliminar.BackColor = Color.FromArgb(198, 40, 40);
            _btnEliminar.ForeColor = Color.White;
            _btnEliminar.FlatStyle = FlatStyle.Flat;
            _btnEliminar.Visible = !_esNuevo;
            _btnEliminar.Click += BtnEliminar_Click;

            _btnCancelar.Text = "Cancelar";
            _btnCancelar.SetBounds(280, 250, 100, 42);
            _btnCancelar.FlatStyle = FlatStyle.Flat;
            _btnCancelar.Click += (s, e) => Close();

            Controls.AddRange(new Control[]
            {
                lblInfo, lblNombre, _txtNombre, lblDni, _txtDni,
                lblTelefono, _txtTelefono, _btnGuardar, _btnEliminar, _btnCancelar
            });

            AcceptButton = _btnGuardar;
            CancelButton = _btnCancelar;
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            var nombre = _txtNombre.Text.Trim();
            var dni = _txtDni.Text.Trim();
            var telefono = _txtTelefono.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("El nombre y apellido es obligatorio.", "Datos incompletos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtNombre.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(dni))
            {
                MessageBox.Show("El DNI es obligatorio.", "Datos incompletos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtDni.Focus();
                return;
            }

            using var context = new AppDbContext();

            if (_esNuevo)
            {
                // Chequeo de seguridad por si el horario se ocupó justo antes de guardar
                bool yaOcupado = context.Turnos.Any(t =>
                    t.Fecha == _fecha && t.Oficina == _oficina && t.Hora == _hora);

                if (yaOcupado)
                {
                    MessageBox.Show("Ese horario acaba de ser reservado por otra carga. Cierre esta ventana y actualice la pantalla.",
                        "Horario ocupado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var nuevo = new Turno
                {
                    NombreApellido = nombre,
                    Dni = dni,
                    Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono,
                    Oficina = _oficina,
                    Fecha = _fecha,
                    Hora = _hora
                };

                context.Turnos.Add(nuevo);
            }
            else
            {
                var turno = context.Turnos.Find(_turnoId);
                if (turno == null)
                {
                    MessageBox.Show("El turno ya no existe (pudo haber sido eliminado).", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                    return;
                }

                turno.NombreApellido = nombre;
                turno.Dni = dni;
                turno.Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono;
            }

            context.SaveChanges();
            HuboCambios = true;
            Close();
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            var confirmacion = MessageBox.Show(
                $"¿Confirma que desea eliminar el turno de \"{_txtNombre.Text}\"?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes) return;

            using var context = new AppDbContext();
            var turno = context.Turnos.Find(_turnoId);
            if (turno != null)
            {
                context.Turnos.Remove(turno);
                context.SaveChanges();
            }

            HuboCambios = true;
            Close();
        }
    }
}
