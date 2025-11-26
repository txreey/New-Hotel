using Pemesanan_Hotel_Terbaru.Admin; // Referensi Admin
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Owner
{
    public partial class DataReservasiO : Form
    {
        private DataTable dtReservasi;

        public DataReservasiO()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Navigasi Sidebar Owner
            guna2Dashboard.Click += (s, e) => PindahForm(new DashboardOwner());
            guna2DataKamar.Click += (s, e) => PindahForm(new DataKamarO());
            guna2DataReservasi.Click += (s, e) => { LoadDataReservasiOwner(); }; // Refresh
            guna2DataTamu.Click += (s, e) => PindahForm(new DataTamuO());
            guna2DataUser.Click += (s, e) => PindahForm(new DataUserO());
            guna2LaporanKeuangan.Click += (s, e) => PindahForm(new LaporanKeuangan());
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiO());
            guna2Logout.Click += (s, e) => Logout();

            // 3. Event Lain
            this.Load += DataReservasiO_Load;
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2DariTanggal.ValueChanged += FilterData; // Gabungkan filter
            guna2SampaiTanggal.ValueChanged += FilterData; // Gabungkan filter
            guna2Reset.Click += guna2Reset_Click;
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
            // guna2HtmlLabel8.ForeColor = ColorTranslator.FromHtml("#333333"); // Label Owner (jika ada)

            // Tombol Aksi
            guna2Reset.FillColor = ColorTranslator.FromHtml("#C5A059"); // Emas
            guna2Reset.ForeColor = Color.White;
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

            // Highlight Data Reservasi
            guna2DataReservasi.FillColor = ColorTranslator.FromHtml("#E2E8F0");
        }

        private void StyleSidebarButton(Guna.UI2.WinForms.Guna2Button btn)
        {
            btn.FillColor = Color.Transparent;
            btn.CheckedState.FillColor = Color.Transparent;
            btn.ForeColor = ColorTranslator.FromHtml("#333333");
            btn.HoverState.FillColor = ColorTranslator.FromHtml("#E2E8F0");
            btn.HoverState.ForeColor = Color.Black;
        }

        // ======================================================
        // 🛠️ LOAD DATA (FIX URUTAN NOMOR)
        // ======================================================
        private void DataReservasiO_Load(object sender, EventArgs e)
        {
            LoadDataReservasiOwner();
        }

        private void LoadDataReservasiOwner()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            r.id_reservasi,
                            t.nama_tamu,
                            k.tipe_kamar,
                            k.no_kamar,
                            r.check_in,
                            r.check_out,
                            r.status_pembayaran
                        FROM reservasi r
                        JOIN tamu t ON r.id_tamu = t.id_tamu
                        JOIN kamar k ON r.id_kamar = k.id_kamar
                        ORDER BY r.check_in DESC"; // Urut dari terbaru

                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    dtReservasi = new DataTable();
                    da.Fill(dtReservasi);

                    DisplayData(dtReservasi);
                }
            }
            catch (Exception ex) { MessageBox.Show($"Gagal load: {ex.Message}"); }
        }

        private void DisplayData(DataTable dt)
        {
            // 1. Bersihkan
            guna2DataGridView1.DataSource = null;
            guna2DataGridView1.Rows.Clear();
            guna2DataGridView1.Columns.Clear();

            // 2. Kolom Manual
            AddTextColumn("colNo", "No", 50);
            AddTextColumn("colTamu", "Nama Tamu", 150);
            AddTextColumn("colTipe", "Tipe Kamar", 120);
            AddTextColumn("colNoKamar", "No Kamar", 80);
            AddTextColumn("colIn", "Check In", 120);
            AddTextColumn("colOut", "Check Out", 120);
            AddTextColumn("colStatus", "Status Bayar", 120);

            // 3. Isi Data
            int nomor = 1;
            foreach (DataRow row in dt.Rows)
            {
                string tglIn = Convert.ToDateTime(row["check_in"]).ToString("dd MMM yyyy");
                string tglOut = Convert.ToDateTime(row["check_out"]).ToString("dd MMM yyyy");

                guna2DataGridView1.Rows.Add(
                    nomor++,
                    row["nama_tamu"],
                    row["tipe_kamar"],
                    row["no_kamar"],
                    tglIn, tglOut,
                    row["status_pembayaran"]
                );
            }

            // 4. Fix Tampilan
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
            guna2DataGridView1.AllowUserToAddRows = false;
            guna2DataGridView1.ReadOnly = true;
        }

        private void AddTextColumn(string name, string header, int width)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.Name = name;
            col.HeaderText = header;
            col.Width = width;
            guna2DataGridView1.Columns.Add(col);
        }

        // ======================================================
        // FILTER & SEARCH
        // ======================================================
        private void guna2Cari_TextChanged(object sender, EventArgs e) => FilterData(sender, e);

        // Gabungkan Filter (Search + Tanggal)
        private void FilterData(object sender, EventArgs e)
        {
            if (dtReservasi == null) return;

            string keyword = guna2Cari.Text.Trim();
            string tglDari = guna2DariTanggal.Value.ToString("yyyy-MM-dd");
            string tglSampai = guna2SampaiTanggal.Value.ToString("yyyy-MM-dd");

            DataView dv = dtReservasi.DefaultView;
            string filter = $"nama_tamu LIKE '%{keyword}%' AND check_in >= '{tglDari}' AND check_out <= '{tglSampai}'";

            dv.RowFilter = filter;
            DisplayData(dv.ToTable());
        }

        private void guna2Reset_Click(object sender, EventArgs e)
        {
            guna2Cari.Text = "";
            guna2DariTanggal.Value = DateTime.Now.AddMonths(-1);
            guna2SampaiTanggal.Value = DateTime.Now.AddMonths(1);
            DisplayData(dtReservasi);
        }

        // ======================================================
        // EXPORT
        // ======================================================
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Reservasi_Owner.xlsx";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    // Buat DataTable Bersih untuk Export (Biar header excel bagus)
                    DataTable dtExport = new DataTable("Reservasi");
                    dtExport.Columns.Add("No");
                    dtExport.Columns.Add("Nama Tamu");
                    dtExport.Columns.Add("Tipe Kamar");
                    dtExport.Columns.Add("No Kamar");
                    dtExport.Columns.Add("Check In");
                    dtExport.Columns.Add("Check Out");
                    dtExport.Columns.Add("Status");

                    foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                    {
                        dtExport.Rows.Add(
                            row.Cells["colNo"].Value, row.Cells["colTamu"].Value,
                            row.Cells["colTipe"].Value, row.Cells["colNoKamar"].Value,
                            row.Cells["colIn"].Value, row.Cells["colOut"].Value,
                            row.Cells["colStatus"].Value
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

        // ======================================================
        // NAVIGASI
        // ======================================================
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

        // Event Kosong
        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void guna2DariTanggal_ValueChanged(object sender, EventArgs e) { } // Event cadangan
        private void guna2SampaiTanggal_ValueChanged(object sender, EventArgs e) { }
    }
}
