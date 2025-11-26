using Pemesanan_Hotel_Terbaru.Admin;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Owner
{
    public partial class DataTamuO : Form
    {
        private DataTable dtTamu;

        public DataTamuO()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Navigasi Sidebar Owner
            guna2Dashboard.Click += (s, e) => PindahForm(new DashboardOwner());
            guna2DataKamar.Click += (s, e) => PindahForm(new DataKamarO());
            guna2DataReservasi.Click += (s, e) => PindahForm(new DataReservasiO());
            guna2DataTamu.Click += (s, e) => { LoadDataTamuO(); }; // Refresh
            guna2DataUser.Click += (s, e) => PindahForm(new DataUserO());
            guna2LaporanKeuangan.Click += (s, e) => PindahForm(new LaporanKeuangan());
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiO());
            guna2Logout.Click += (s, e) => Logout();

            // 3. Event Lain
            this.Load += DataTamuO_Load;
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2ExportExcel.Click += guna2ExportExcel_Click;
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (KONSISTEN)
        // ============================================================
        private void ApplyElegantTheme()
        {
            // Background & Panel
            this.BackColor = ColorTranslator.FromHtml("#F4F6F8");
            guna2Panel1.FillColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel1.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel5.FillColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel5.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2PictureBox1.BackColor = Color.Transparent;

            // Label Judul
            foreach (Control c in Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2HtmlLabel) c.ForeColor = ColorTranslator.FromHtml("#333333");
            }

            // Tombol Aksi
            guna2ExportExcel.FillColor = ColorTranslator.FromHtml("#2C3E50"); // Abu Gelap
            guna2ExportExcel.ForeColor = Color.White;

            // Reset Sidebar
            StyleSidebarButton(guna2Dashboard);
            StyleSidebarButton(guna2DataKamar);
            StyleSidebarButton(guna2DataReservasi);
            StyleSidebarButton(guna2DataTamu);
            StyleSidebarButton(guna2DataUser);
            StyleSidebarButton(guna2LaporanKeuangan);
            StyleSidebarButton(guna2LaporanTransaksi);
            StyleSidebarButton(guna2Logout);

            // Highlight Data Tamu
            guna2DataTamu.FillColor = ColorTranslator.FromHtml("#E2E8F0");
        }

        private void StyleSidebarButton(Guna.UI2.WinForms.Guna2Button btn)
        {
            btn.FillColor = Color.Transparent;
            btn.CheckedState.FillColor = Color.Transparent;
            btn.ForeColor = ColorTranslator.FromHtml("#333333");
            btn.HoverState.FillColor = ColorTranslator.FromHtml("#E2E8F0");
            btn.HoverState.ForeColor = Color.Black;
        }

        // ============================================================
        // 🛠️ LOAD DATA (FIX URUTAN NOMOR)
        // ============================================================
        private void DataTamuO_Load(object sender, EventArgs e)
        {
            LoadDataTamuO();
        }

        private void LoadDataTamuO()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM tamu ORDER BY nama_tamu ASC";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    dtTamu = new DataTable();
                    da.Fill(dtTamu);

                    DisplayData(dtTamu);
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal load: " + ex.Message); }
        }

        private void DisplayData(DataTable dt)
        {
            // 1. Bersihkan
            guna2DataGridView1.DataSource = null;
            guna2DataGridView1.Rows.Clear();
            guna2DataGridView1.Columns.Clear();

            // 2. Kolom Manual (No Urut, Nama, dll)
            AddTextColumn("colNo", "No", 50);
            AddTextColumn("colNama", "Nama Tamu", 180);
            AddTextColumn("colNIK", "NIK", 120);
            AddTextColumn("colAlamat", "Alamat", 200);
            AddTextColumn("colHP", "No Handphone", 120);
            AddTextColumn("colEmail", "Email", 150);

            // 3. Isi Data
            int nomor = 1;
            foreach (DataRow row in dt.Rows)
            {
                guna2DataGridView1.Rows.Add(
                    nomor++,
                    row["nama_tamu"],
                    row["nik"],
                    row["alamat"],
                    row["no_handphone"],
                    row["email"]
                );
            }

            // 4. Fix Tampilan (Header Emas, dll)
            FixTableStyle();
        }

        private void FixTableStyle()
        {
            guna2DataGridView1.EnableHeadersVisualStyles = false;
            guna2DataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            guna2DataGridView1.AllowUserToAddRows = false;
            guna2DataGridView1.ReadOnly = true;

            var headerStyle = new DataGridViewCellStyle();
            headerStyle.BackColor = ColorTranslator.FromHtml("#C5A059");
            headerStyle.ForeColor = Color.White;
            headerStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            headerStyle.SelectionBackColor = ColorTranslator.FromHtml("#C5A059");

            guna2DataGridView1.ColumnHeadersDefaultCellStyle = headerStyle;
            guna2DataGridView1.ColumnHeadersHeight = 40;

            foreach (DataGridViewColumn col in guna2DataGridView1.Columns)
            {
                col.HeaderCell.Style = headerStyle;
            }

            guna2DataGridView1.DefaultCellStyle.BackColor = Color.White;
            guna2DataGridView1.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2DataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F0E68C");
            guna2DataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;
            guna2DataGridView1.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FAFAFA");
            guna2DataGridView1.RowTemplate.Height = 50;
        }

        private void AddTextColumn(string name, string header, int width)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.Name = name;
            col.HeaderText = header;
            col.Width = width;
            guna2DataGridView1.Columns.Add(col);
        }

        // ============================================================
        // PENCARIAN
        // ============================================================
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            if (dtTamu == null) return;
            string search = guna2Cari.Text.Trim();
            DataView dv = dtTamu.DefaultView;

            if (!string.IsNullOrEmpty(search))
                dv.RowFilter = $"nama_tamu LIKE '%{search}%' OR nik LIKE '%{search}%'";
            else
                dv.RowFilter = "";

            DisplayData(dv.ToTable());
        }

        // ============================================================
        // EXPORT EXCEL
        // ============================================================
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Tamu_Owner.xlsx";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    DataTable dtExport = new DataTable("Tamu");
                    dtExport.Columns.Add("No");
                    dtExport.Columns.Add("Nama");
                    dtExport.Columns.Add("NIK");
                    dtExport.Columns.Add("Alamat");
                    dtExport.Columns.Add("HP");
                    dtExport.Columns.Add("Email");

                    foreach (DataGridViewRow r in guna2DataGridView1.Rows)
                    {
                        dtExport.Rows.Add(
                            r.Cells["colNo"].Value, r.Cells["colNama"].Value,
                            r.Cells["colNIK"].Value, r.Cells["colAlamat"].Value,
                            "'" + r.Cells["colHP"].Value, r.Cells["colEmail"].Value
                        );
                    }

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(dtExport);
                        ws.Columns().AdjustToContents();
                        wb.SaveAs(save.FileName);
                    }
                    MessageBox.Show("Export berhasil!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal export: " + ex.Message); }
        }

        // ============================================================
        // NAVIGASI
        // ============================================================
        private void PindahForm(Form targetForm)
        {
            targetForm.WindowState = FormWindowState.Maximized;
            targetForm.Show();
            this.Hide();
        }

        private void Logout()
        {
            if (MessageBox.Show("Logout?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Hide(); new Login().Show();
            }
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }
}
