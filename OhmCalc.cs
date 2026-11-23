using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace OhmResistanceCalculator
{
    public class BandOption
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public Color DisplayColor { get; set; }

        public BandOption(string name, double value, Color displayColor)
        {
            Name = name;
            Value = value;
            DisplayColor = displayColor;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class MainForm : Form
    {
        // Pestaña 1: Ley de Ohm
        private ComboBox cmbMode;
        private TextBox txtVal1;
        private TextBox txtVal2;
        private TextBox txtVal3;
        private Label lblVal1;
        private Label lblVal2;
        private Label lblVal3;
        private Label lblResult;
        private Label lblExtra;
        private Button btnCalc;
        private Button btnClear;

        // Pestaña 2: Bandas de Resistencia
        private RadioButton rb4Bands;
        private RadioButton rb5Bands;
        private ComboBox cmbBand1;
        private ComboBox cmbBand2;
        private ComboBox cmbBand3;
        private ComboBox cmbMultiplier;
        private ComboBox cmbTolerance;
        private Label lblBand3;
        private Panel pnlResistor;
        private Label lblBandResult;
        private Label lblBandRange;

        public MainForm()
        {
            // 1. TÍTULO DE LA VENTANA
            Text = "OhmCalc - Calculadora Eléctrica";
            Size = new Size(500, 445);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 9.5F);

            // 2. LOGO EN LA BARRA DE TÍTULO (si existe logo.ico junto al ejecutable)
            try
            {
                if (System.IO.File.Exists("resistor.ico"))
                {
                    Icon = new Icon("resistor.ico");
                }
            }
            catch
            {
                // Si no se puede cargar el .ico externo, conserva el ícono por defecto
            }

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill
            };

            var tabOhm = new TabPage("Ley de Ohm");
            var tabBands = new TabPage("Bandas de Resistencia");
            var tabAbout = new TabPage("Acerca de");

            BuildOhmTab(tabOhm);
            BuildBandsTab(tabBands);
            BuildAboutTab(tabAbout);

            tabs.TabPages.Add(tabOhm);
            tabs.TabPages.Add(tabBands);
            tabs.TabPages.Add(tabAbout);
            Controls.Add(tabs);
        }

        // ==========================================
        // PESTAÑA 1: LEY DE OHM Y RESISTENCIA
        // ==========================================
        private void BuildOhmTab(TabPage page)
        {
            var lblTitle = new Label
            {
                Text = "Calculadora de Ley de Ohm",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblMode = new Label
            {
                Text = "Calcular:",
                Location = new Point(20, 58),
                AutoSize = true
            };

            cmbMode = new ComboBox
            {
                Location = new Point(140, 54),
                Width = 315,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbMode.Items.AddRange(new object[]
            {
                "Voltaje (V) desde Corriente (A) y Resistencia (Ω)",
                "Corriente (I) desde Voltaje (V) y Resistencia (Ω)",
                "Resistencia (R) desde Voltaje (V) y Corriente (A)",
                "Resistencia de cable: R = ρ·(L/A)"
            });
            cmbMode.SelectedIndex = 0;
            cmbMode.SelectedIndexChanged += delegate(object s, EventArgs e) { UpdateOhmFields(); };

            lblVal1 = new Label { Location = new Point(20, 100), Width = 185 };
            txtVal1 = new TextBox { Location = new Point(215, 97), Width = 240 };

            lblVal2 = new Label { Location = new Point(20, 138), Width = 185 };
            txtVal2 = new TextBox { Location = new Point(215, 135), Width = 240 };

            lblVal3 = new Label { Location = new Point(20, 176), Width = 185 };
            txtVal3 = new TextBox { Location = new Point(215, 173), Width = 240 };

            btnCalc = new Button
            {
                Text = "Calcular",
                Location = new Point(245, 215),
                Width = 100,
                Height = 34
            };
            btnCalc.Click += delegate(object s, EventArgs e) { CalculateOhm(); };

            btnClear = new Button
            {
                Text = "Limpiar",
                Location = new Point(355, 215),
                Width = 100,
                Height = 34
            };
            btnClear.Click += delegate(object s, EventArgs e) { ClearOhmFields(false); };

            lblResult = new Label
            {
                Text = "Resultado: —",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(20, 270),
                Width = 435,
                AutoSize = true
            };

            lblExtra = new Label
            {
                Text = "",
                ForeColor = Color.DimGray,
                Location = new Point(20, 302),
                Width = 435,
                AutoSize = true
            };

            page.Controls.AddRange(new Control[]
            {
                lblTitle, lblMode, cmbMode,
                lblVal1, txtVal1, lblVal2, txtVal2, lblVal3, txtVal3,
                btnCalc, btnClear, lblResult, lblExtra
            });

            UpdateOhmFields();
        }

        private void UpdateOhmFields()
        {
            ClearOhmFields(false);
            int idx = cmbMode.SelectedIndex;

            if (idx == 0)
            {
                lblVal1.Text = "Corriente I (A):";
                lblVal2.Text = "Resistencia R (Ω):";
                SetThirdField(false, "");
            }
            else if (idx == 1)
            {
                lblVal1.Text = "Voltaje V (V):";
                lblVal2.Text = "Resistencia R (Ω):";
                SetThirdField(false, "");
            }
            else if (idx == 2)
            {
                lblVal1.Text = "Voltaje V (V):";
                lblVal2.Text = "Corriente I (A):";
                SetThirdField(false, "");
            }
            else
            {
                lblVal1.Text = "Resistividad ρ (Ω·m):";
                lblVal2.Text = "Longitud L (m):";
                SetThirdField(true, "Área A (m²):");
            }
        }

        private void SetThirdField(bool visible, string labelText)
        {
            lblVal3.Visible = visible;
            txtVal3.Visible = visible;
            lblVal3.Text = labelText;
            txtVal3.Clear();
        }

        private bool TryParseNum(TextBox box, out double value)
        {
            string normalized = box.Text.Trim().Replace(',', '.');
            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private void CalculateOhm()
        {
            double v1, v2;
            if (!TryParseNum(txtVal1, out v1) || !TryParseNum(txtVal2, out v2))
            {
                MessageBox.Show("Estos espacios no pueden estar vacíos.", "Dato inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idx = cmbMode.SelectedIndex;

            if (idx == 0)
            {
                double v = v1 * v2;
                double p = v * v1;
                lblResult.Text = string.Format("Resultado: V = {0:G6} V", v);
                lblExtra.Text = string.Format("Potencia estimada (P = V·I): {0:G6} W", p);
            }
            else if (idx == 1)
            {
                if (Math.Abs(v2) < 1e-12)
                {
                    MessageBox.Show("La resistencia no puede ser 0. (Sería obvio si tu resistencia sí es de 0 por tener solo una banda)", "Error matemático", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                double i = v1 / v2;
                double p = v1 * i;
                lblResult.Text = string.Format("Resultado: I = {0:G6} A", i);
                lblExtra.Text = string.Format("Potencia estimada (P = V·I): {0:G6} W", p);
            }
            else if (idx == 2)
            {
                if (Math.Abs(v2) < 1e-12)
                {
                    MessageBox.Show("La corriente no puede ser 0.", "Error matemático", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                double r = v1 / v2;
                double p = v1 * v2;
                lblResult.Text = string.Format("Resultado: R = {0:G6} Ω", r);
                lblExtra.Text = string.Format("Potencia disipada (P = V·I): {0:G6} W", p);
            }
            else
            {
                double area;
                if (!TryParseNum(txtVal3, out area) || Math.Abs(area) < 1e-18)
                {
                    MessageBox.Show("Ingresa un área (A) válida y distinta de 0.", "Dato inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                double r = (v1 * v2) / area;
                lblResult.Text = string.Format("Resultado: R = {0:G6} Ω", r);
                lblExtra.Text = string.Format("Fórmula: R = ({0:G4} × {1:G4}) / {2:G4}", v1, v2, area);
            }
        }

        private void ClearOhmFields(bool keepResult)
        {
            txtVal1.Clear();
            txtVal2.Clear();
            txtVal3.Clear();
            if (!keepResult)
            {
                lblResult.Text = "Resultado: —";
                lblExtra.Text = "";
            }
        }

        // ==========================================
        // PESTAÑA 2: CÓDIGO DE COLORES DE RESISTENCIA
        // ==========================================
        private void BuildBandsTab(TabPage page)
        {
            var lblTitle = new Label
            {
                Text = "Código de Colores de Resistencia",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Location = new Point(20, 12),
                AutoSize = true
            };

            rb4Bands = new RadioButton
            {
                Text = "4 Bandas",
                Location = new Point(22, 46),
                Checked = true,
                AutoSize = true
            };
            rb4Bands.CheckedChanged += delegate(object s, EventArgs e) { ToggleBandMode(); };

            rb5Bands = new RadioButton
            {
                Text = "5 Bandas (Precisión)",
                Location = new Point(120, 46),
                AutoSize = true
            };
            rb5Bands.CheckedChanged += delegate(object s, EventArgs e) { ToggleBandMode(); };

            pnlResistor = new Panel
            {
                Location = new Point(20, 76),
                Size = new Size(435, 65),
                BackColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlResistor.Paint += PnlResistor_Paint;

            int yStart = 154;
            int stepY = 32;

            var lblB1 = new Label { Text = "1ª Banda (Dígito 1):", Location = new Point(20, yStart + 3), Width = 175 };
            cmbBand1 = CreateColorCombo(GetDigitOptions(false), 1);
            cmbBand1.Location = new Point(205, yStart);

            var lblB2 = new Label { Text = "2ª Banda (Dígito 2):", Location = new Point(20, yStart + stepY + 3), Width = 175 };
            cmbBand2 = CreateColorCombo(GetDigitOptions(true), 0);
            cmbBand2.Location = new Point(205, yStart + stepY);

            lblBand3 = new Label { Text = "3ª Banda (Dígito 3):", Location = new Point(20, yStart + stepY * 2 + 3), Width = 175 };
            cmbBand3 = CreateColorCombo(GetDigitOptions(true), 0);
            cmbBand3.Location = new Point(205, yStart + stepY * 2);

            var lblMult = new Label { Text = "Multiplicador:", Location = new Point(20, yStart + stepY * 3 + 3), Width = 175 };
            cmbMultiplier = CreateColorCombo(GetMultiplierOptions(), 2);
            cmbMultiplier.Location = new Point(205, yStart + stepY * 3);

            var lblTol = new Label { Text = "Tolerancia:", Location = new Point(20, yStart + stepY * 4 + 3), Width = 175 };
            cmbTolerance = CreateColorCombo(GetToleranceOptions(), 5);
            cmbTolerance.Location = new Point(205, yStart + stepY * 4);

            lblBandResult = new Label
            {
                Text = "Valor: 1 kΩ (±5%)",
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                Location = new Point(20, 322),
                Width = 435,
                AutoSize = true
            };

            lblBandRange = new Label
            {
                Text = "Rango permitido: 950 Ω — 1.05 kΩ",
                ForeColor = Color.DimGray,
                Location = new Point(20, 348),
                Width = 435,
                AutoSize = true
            };

            page.Controls.AddRange(new Control[]
            {
                lblTitle, rb4Bands, rb5Bands, pnlResistor,
                lblB1, cmbBand1, lblB2, cmbBand2, lblBand3, cmbBand3,
                lblMult, cmbMultiplier, lblTol, cmbTolerance,
                lblBandResult, lblBandRange
            });

            ToggleBandMode();
        }

        private ComboBox CreateColorCombo(BandOption[] items, int defaultIndex)
        {
            var cmb = new ComboBox
            {
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 20
            };
            cmb.Items.AddRange(items);
            cmb.SelectedIndex = defaultIndex;

            cmb.DrawItem += delegate(object sender, DrawItemEventArgs e)
            {
                if (e.Index < 0) return;
                e.DrawBackground();
                var opt = (BandOption)cmb.Items[e.Index];

                Rectangle colorRect = new Rectangle(e.Bounds.Left + 4, e.Bounds.Top + 3, 22, e.Bounds.Height - 6);
                using (var brush = new SolidBrush(opt.DisplayColor))
                {
                    e.Graphics.FillRectangle(brush, colorRect);
                }
                e.Graphics.DrawRectangle(Pens.Gray, colorRect);

                using (var textBrush = new SolidBrush(e.ForeColor))
                {
                    e.Graphics.DrawString(opt.Name, e.Font, textBrush, e.Bounds.Left + 32, e.Bounds.Top + 2);
                }
                e.DrawFocusRectangle();
            };

            cmb.SelectedIndexChanged += delegate(object s, EventArgs e) { CalculateBands(); };
            return cmb;
        }

        private void ToggleBandMode()
        {
            bool is5 = rb5Bands.Checked;
            lblBand3.Visible = is5;
            cmbBand3.Visible = is5;
            CalculateBands();
        }

        private void CalculateBands()
        {
            if (cmbBand1.SelectedItem == null || cmbBand2.SelectedItem == null ||
                cmbBand3.SelectedItem == null || cmbMultiplier.SelectedItem == null ||
                cmbTolerance.SelectedItem == null)
                return;

            var b1 = (BandOption)cmbBand1.SelectedItem;
            var b2 = (BandOption)cmbBand2.SelectedItem;
            var b3 = (BandOption)cmbBand3.SelectedItem;
            var mult = (BandOption)cmbMultiplier.SelectedItem;
            var tol = (BandOption)cmbTolerance.SelectedItem;

            double baseDigits = rb5Bands.Checked
                ? (b1.Value * 100.0 + b2.Value * 10.0 + b3.Value)
                : (b1.Value * 10.0 + b2.Value);

            double ohms = baseDigits * mult.Value;
            double tolPercent = tol.Value;
            double minOhms = ohms * (1.0 - tolPercent / 100.0);
            double maxOhms = ohms * (1.0 + tolPercent / 100.0);

            lblBandResult.Text = string.Format("Valor: {0}  (±{1}%)", FormatOhms(ohms), tolPercent);
            lblBandRange.Text = string.Format("Rango permitido: {0}  —  {1}", FormatOhms(minOhms), FormatOhms(maxOhms));

            pnlResistor.Invalidate();
        }

        private string FormatOhms(double value)
        {
            if (value >= 1e9) return string.Format(CultureInfo.InvariantCulture, "{0:0.###} GΩ", value / 1e9);
            if (value >= 1e6) return string.Format(CultureInfo.InvariantCulture, "{0:0.###} MΩ", value / 1e6);
            if (value >= 1e3) return string.Format(CultureInfo.InvariantCulture, "{0:0.###} kΩ", value / 1e3);
            return string.Format(CultureInfo.InvariantCulture, "{0:0.###} Ω", value);
        }

        private void PnlResistor_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int w = pnlResistor.Width;
            int h = pnlResistor.Height;

            using (var leadPen = new Pen(Color.Silver, 5))
            {
                g.DrawLine(leadPen, 35, h / 2, w - 35, h / 2);
            }

            Rectangle body = new Rectangle(95, 13, w - 190, h - 26);
            Color bodyColor = rb5Bands.Checked ? Color.FromArgb(165, 210, 235) : Color.FromArgb(238, 214, 175);
            using (var bodyBrush = new SolidBrush(bodyColor))
            {
                g.FillRectangle(bodyBrush, body);
            }
            g.DrawRectangle(Pens.DimGray, body);

            var b1 = (BandOption)cmbBand1.SelectedItem;
            var b2 = (BandOption)cmbBand2.SelectedItem;
            var b3 = (BandOption)cmbBand3.SelectedItem;
            var mult = (BandOption)cmbMultiplier.SelectedItem;
            var tol = (BandOption)cmbTolerance.SelectedItem;

            DrawBandStripe(g, body, 22, b1.DisplayColor);
            DrawBandStripe(g, body, 52, b2.DisplayColor);

            if (rb5Bands.Checked)
            {
                DrawBandStripe(g, body, 82, b3.DisplayColor);
                DrawBandStripe(g, body, 112, mult.DisplayColor);
            }
            else
            {
                DrawBandStripe(g, body, 88, mult.DisplayColor);
            }

            DrawBandStripe(g, body, body.Width - 32, tol.DisplayColor);
        }

        private void DrawBandStripe(Graphics g, Rectangle body, int offsetX, Color color)
        {
            Rectangle stripe = new Rectangle(body.Left + offsetX, body.Top + 1, 14, body.Height - 1);
            using (var brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, stripe);
            }
            using (var borderPen = new Pen(Color.FromArgb(80, 0, 0, 0)))
            {
                g.DrawRectangle(borderPen, stripe);
            }
        }

        // ==========================================
        // PESTAÑA 3: ACERCA DE
        // ==========================================
        private void BuildAboutTab(TabPage page)
        {
            var lblTitle = new Label
            {
                Text = "Acerca de OhmCalc",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(25, 24),
                AutoSize = true
            };

            var lblDesc = new Label
            {
                Text = "Utilidad portátil para cálculos de Ley de Ohm, resistencia de cable y código de colores de resistencias (4 y 5 bandas).",
                Location = new Point(25, 64),
                Size = new Size(425, 45)
            };

            var btnOpenModal = new Button
            {
                Text = "Abrir ventana Acerca de",
                Location = new Point(25, 120),
                Width = 180,
                Height = 34
            };
            btnOpenModal.Click += delegate(object s, EventArgs e)
            {
                MessageBox.Show(
                    "OhmCalc v0.1\n\nCálculos de Ley de Ohm, potencia, resistencia de conductor y bandas de color.\nEjecutable independiente compilado con C#. \nCreado por: \n@link.botw.117 en TikTok y YouTube. \nLinkTheDevAndRooter en GitHub. \n(son la misma persona)",
                    "Acerca de OhmCalc",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            };

            page.Controls.AddRange(new Control[] { lblTitle, lblDesc, btnOpenModal });
        }

        private BandOption[] GetDigitOptions(bool includeBlack)
        {
            var all = new BandOption[]
            {
                new BandOption("Negro (0)", 0, Color.Black),
                new BandOption("Marrón (1)", 1, Color.FromArgb(120, 66, 18)),
                new BandOption("Rojo (2)", 2, Color.Red),
                new BandOption("Naranja (3)", 3, Color.DarkOrange),
                new BandOption("Amarillo (4)", 4, Color.Gold),
                new BandOption("Verde (5)", 5, Color.ForestGreen),
                new BandOption("Azul (6)", 6, Color.RoyalBlue),
                new BandOption("Violeta (7)", 7, Color.DarkViolet),
                new BandOption("Gris (8)", 8, Color.Gray),
                new BandOption("Blanco (9)", 9, Color.White)
            };

            if (includeBlack) return all;

            var withoutBlack = new BandOption[all.Length - 1];
            Array.Copy(all, 1, withoutBlack, 0, withoutBlack.Length);
            return withoutBlack;
        }

        private BandOption[] GetMultiplierOptions()
        {
            return new BandOption[]
            {
                new BandOption("Negro (×1 Ω)", 1, Color.Black),
                new BandOption("Marrón (×10 Ω)", 10, Color.FromArgb(120, 66, 18)),
                new BandOption("Rojo (×100 Ω)", 100, Color.Red),
                new BandOption("Naranja (×1 kΩ)", 1000, Color.DarkOrange),
                new BandOption("Amarillo (×10 kΩ)", 10000, Color.Gold),
                new BandOption("Verde (×100 kΩ)", 100000, Color.ForestGreen),
                new BandOption("Azul (×1 MΩ)", 1000000, Color.RoyalBlue),
                new BandOption("Violeta (×10 MΩ)", 10000000, Color.DarkViolet),
                new BandOption("Gris (×100 MΩ)", 100000000, Color.Gray),
                new BandOption("Blanco (×1 GΩ)", 1000000000, Color.White),
                new BandOption("Dorado (×0.1 Ω)", 0.1, Color.Goldenrod),
                new BandOption("Plateado (×0.01 Ω)", 0.01, Color.Silver)
            };
        }

        private BandOption[] GetToleranceOptions()
        {
            return new BandOption[]
            {
                new BandOption("Marrón (±1%)", 1.0, Color.FromArgb(120, 66, 18)),
                new BandOption("Rojo (±2%)", 2.0, Color.Red),
                new BandOption("Verde (±0.5%)", 0.5, Color.ForestGreen),
                new BandOption("Azul (±0.25%)", 0.25, Color.RoyalBlue),
                new BandOption("Violeta (±0.1%)", 0.1, Color.DarkViolet),
                new BandOption("Dorado (±5%)", 5.0, Color.Goldenrod),
                new BandOption("Plateado (±10%)", 10.0, Color.Silver)
            };
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}