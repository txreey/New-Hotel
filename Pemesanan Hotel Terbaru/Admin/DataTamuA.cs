using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Admin
{
    public partial class DataTamuA : Form
    {
        private DataTable dtTamu;

        public DataTamuA()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Navigasi Sidebar (Pake logika PindahForm biar aman)
            guna2Dashboard.Click += (s, e) => PindahForm(new DashboardAdmin());
            guna2DataKamar.Click += (s, e) => PindahForm(new DataKamarA());
            guna2DataTamu.Click += (s, e) => { LoadDataTamu(); }; // Refresh diri sendiri
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiA());
            guna2DataReservasi.Click += (s, e) => PindahForm(new DataReservasi());
            guna2DataUser.Click += (s, e) => PindahForm(new DataUser());
            guna2LaporanKeuangan.Click += (s, e) => PindahForm(new LaporanKeuangan2());
            guna2Logout.Click += (s, e) => Logout();

            // 3. Event Lain
            guna2ExportExcel.Click += guna2ExportExcel_Click;
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            this.Load += DataTamuA_Load;
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (KONSISTEN DASHBOARD & DATA LAIN)
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

            // Label Judul (Pastikan nama di designer benar, misal guna2HtmlLabel1)
            foreach (Control c in Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2HtmlLabel) c.ForeColor = ColorTranslator.FromHtml("#333333");
            }
            guna2HtmlLabel8.ForeColor = ColorTranslator.FromHtml("#333333"); // Label Admin

            // Tombol Export (Abu Gelap)
            guna2ExportExcel.FillColor = ColorTranslator.FromHtml("#2C3E50");
            guna2ExportExcel.ForeColor = Color.White;

            // Reset Tombol Sidebar
            StyleSidebarButton(guna2Dashboard);
            StyleSidebarButton(guna2DataKamar);
            StyleSidebarButton(guna2DataTamu);
            StyleSidebarButton(guna2LaporanTransaksi);
            StyleSidebarButton(guna2DataReservasi);
            StyleSidebarButton(guna2DataUser);
            StyleSidebarButton(guna2LaporanKeuangan);
            StyleSidebarButton(guna2Logout);

            // Highlight Data Tamu (Aktif)
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

        // =======================================================
        // 🛠️ LOAD DATA (FIX NOMOR URUT)
        // =======================================================
        private void DataTamuA_Load(object sender, EventArgs e)
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
                    string query = "SELECT * FROM tamu ORDER BY nama_tamu ASC"; // Urut Abjad
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    dtTamu = new DataTable();
                    adapter.Fill(dtTamu);

                    DisplayData(dtTamu);
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal: " + ex.Message); }
        }

        private void DisplayData(DataTable dt)
        {
            // 1. Bersihkan Tabel
            guna2DataGridView1.DataSource = null;
            guna2DataGridView1.Rows.Clear();
            guna2DataGridView1.Columns.Clear();

            // 2. Buat Kolom Manual
            AddTextColumn("colNo", "No", 50);
            AddTextColumn("colNama", "Nama Tamu", 180);
            AddTextColumn("colNIK", "NIK", 120);
            AddTextColumn("colAlamat", "Alamat", 200);
            AddTextColumn("colHP", "No Handphone", 120);
            AddTextColumn("colEmail", "Email", 150);

            // Kolom ID Database (Disembunyikan jika butuh edit/hapus nanti)
            AddTextColumn("colID", "ID", 0);
            guna2DataGridView1.Columns["colID"].Visible = false;

            // 3. Isi Data dengan Nomor Urut
            int nomor = 1;
            foreach (DataRow row in dt.Rows)
            {
                guna2DataGridView1.Rows.Add(
                    nomor++,
                    row["nama_tamu"],
                    row["nik"],
                    row["alamat"],
                    row["no_handphone"],
                    row["email"],
                    row["id_tamu"]
                );
            }

            // 4. Fix Tampilan Tabel (Wajib)
            FixTableStyle();
        }

        private void AddTextColumn(string name, string header, int width)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.Name = name;
            col.HeaderText = header;
            col.Width = width;
            guna2DataGridView1.Columns.Add(col);
        }

        private void FixTableStyle()
        {
            guna2DataGridView1.EnableHeadersVisualStyles = false;
            guna2DataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Header Emas
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

            // Isi Tabel
            guna2DataGridView1.DefaultCellStyle.BackColor = Color.White;
            guna2DataGridView1.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2DataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F0E68C");
            guna2DataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;
            guna2DataGridView1.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FAFAFA");
            guna2DataGridView1.RowTemplate.Height = 50;
            guna2DataGridView1.AllowUserToAddRows = false;
        }

        // =======================================================
        // 🔍 PENCARIAN
        // =======================================================
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            if (dtTamu == null) return;
            string search = guna2Cari.Text.Trim();

            DataView dv = dtTamu.DefaultView;
            if (!string.IsNullOrEmpty(search))
            {
                dv.RowFilter = $"nama_tamu LIKE '%{search}%' OR nik LIKE '%{search}%'";
            }
            else
            {
                dv.RowFilter = "";
            }

            DisplayData(dv.ToTable()); // Tampilkan ulang hasil filter
        }

        // =======================================================
        // 📤 EXPORT EXCEL
        // =======================================================
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Tamu.xlsx";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    // Buat DataTable bersih (Tanpa ID, cuma data yg mau ditampilkan)
                    DataTable dtExport = new DataTable("Data Tamu");
                    dtExport.Columns.Add("No");
                    dtExport.Columns.Add("Nama Tamu");
                    dtExport.Columns.Add("NIK");
                    dtExport.Columns.Add("Alamat");
                    dtExport.Columns.Add("No Handphone");
                    dtExport.Columns.Add("Email");

                    foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                    {
                        dtExport.Rows.Add(
                            row.Cells["colNo"].Value,
                            row.Cells["colNama"].Value,
                            row.Cells["colNIK"].Value,
                            row.Cells["colAlamat"].Value,
                            "'" + row.Cells["colHP"].Value, // Kasih petik biar ga jadi angka ilmiah
                            row.Cells["colEmail"].Value
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

        // =======================================================
        // NAVIGASI
        // =======================================================
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
