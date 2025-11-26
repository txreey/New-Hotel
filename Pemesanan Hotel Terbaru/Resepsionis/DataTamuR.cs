using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class DataTamuR : Form
    {
        private DataTable dtTamu;

        public DataTamuR()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Navigasi Sidebar
            guna2Dashboard.Click += (s, e) => PindahForm(new DashboardResepsionis());
            guna2DataKamar.Click += (s, e) => PindahForm(new DataKamarR());
            guna2DataTamu.Click += (s, e) => { LoadDataTamu(); }; // Refresh
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiR());
            guna2Reservasi.Click += (s, e) => PindahForm(new Reservasi());
            guna2TransaksiPembayaran.Click += (s, e) => PindahForm(new TransaksiPembayaran());
            guna2Logout.Click += (s, e) => Logout();

            // 3. Event Lain
            this.Load += DataTamuR_Load;
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2ExportExcel.Click += guna2ExportExcel_Click;
        }

        // ============================================================
        // 🎨 TEMA ELEGANT
        // ============================================================
        private void ApplyElegantTheme()
        {
            // Background
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
            StyleSidebarButton(guna2DataTamu);
            StyleSidebarButton(guna2LaporanTransaksi);
            StyleSidebarButton(guna2Reservasi);
            StyleSidebarButton(guna2TransaksiPembayaran);
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
        // 🛠️ LOAD & DISPLAY (FIX URUTAN)
        // ============================================================
        private void DataTamuR_Load(object sender, EventArgs e)
        {
            LoadDataTamu();
        }

        private void LoadDataTamu()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM tamu ORDER BY nama_tamu ASC";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    dtTamu = new DataTable();
                    adapter.Fill(dtTamu);

                    DisplayData(dtTamu);
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal memuat data: " + ex.Message); }
        }

        private void DisplayData(DataTable dt)
        {
            guna2DataGridView1.Rows.Clear();
            guna2DataGridView1.Columns.Clear();

            // 1. Kolom Manual
            AddTextColumn("colNo", "No", 50);
            AddTextColumn("colNama", "Nama Tamu", 180);
            AddTextColumn("colNIK", "NIK", 120);
            AddTextColumn("colAlamat", "Alamat", 200);
            AddTextColumn("colHP", "No Handphone", 120);
            AddTextColumn("colEmail", "Email", 150);

            // Read Only
            guna2DataGridView1.AllowUserToAddRows = false;
            guna2DataGridView1.ReadOnly = true;

            // 2. Isi Data
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

            // 3. Fix Tampilan
            FixTableStyle();
        }

        private void FixTableStyle()
        {
            guna2DataGridView1.EnableHeadersVisualStyles = false;
            guna2DataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

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
        // SEARCH & EXPORT
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

        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtTamu == null || dtTamu.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data!"); return;
                }
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Tamu.xlsx";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    using (XLWorkbook wb = new XLWorkbook())
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

                        var ws = wb.Worksheets.Add(dtExport);
                        ws.Columns().AdjustToContents();
                        wb.SaveAs(save.FileName);
                    }
                    MessageBox.Show("Export Berhasil!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal: " + ex.Message); }
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
