using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Admin
{
    public partial class DataReservasi : Form
    {
        private DataTable dtReservasi;

        public DataReservasi()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Navigasi Sidebar (Pake logika PindahForm biar gak numpuk)
            guna2Dashboard.Click += (s, e) => PindahForm(new DashboardAdmin());
            guna2DataKamar.Click += (s, e) => PindahForm(new DataKamarA());
            guna2DataTamu.Click += (s, e) => PindahForm(new DataTamuA());
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiA());
            guna2DataReservasi.Click += (s, e) => { LoadDataReservasi(); }; // Refresh
            guna2DataUser.Click += (s, e) => PindahForm(new DataUser());
            guna2LaporanKeuangan.Click += (s, e) => PindahForm(new LaporanKeuangan2());
            guna2Logout.Click += (s, e) => Logout();

            // 3. Event Lain
            guna2Reset.Click += guna2Reset_Click;
            guna2ExportExcel.Click += guna2ExportExcel_Click;
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2DariTanggal.ValueChanged += guna2DariTanggal_ValueChanged;
            guna2SampaiTanggal.ValueChanged += guna2SampaiTanggal_ValueChanged;

            this.Load += DataReservasi_Load;
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (KONSISTEN DASHBOARD)
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

            // Label Judul (Gelap)
            // Pastikan nama label di Designermu benar (misal guna2HtmlLabel1)
            // Saya cek nama default saja, kalau error sesuaikan namanya
            foreach (Control c in Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2HtmlLabel) c.ForeColor = ColorTranslator.FromHtml("#333333");
            }
            guna2HtmlLabel8.ForeColor = ColorTranslator.FromHtml("#333333"); // Label Admin

            // Tombol Aksi (Reset & Export)
            guna2Reset.FillColor = ColorTranslator.FromHtml("#C5A059"); // Emas
            guna2Reset.ForeColor = Color.White;

            guna2ExportExcel.FillColor = ColorTranslator.FromHtml("#2C3E50"); // Abu Gelap
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

            // Highlight Data Reservasi (Aktif)
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

        // ============================================================
        // 🛠️ LOAD DATA (FIX URUTAN NOMOR)
        // ============================================================
        private void DataReservasi_Load(object sender, EventArgs e)
        {
            LoadDataReservasi();
        }

        private void LoadDataReservasi()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT 
                                        r.id_reservasi,
                                        t.nama_tamu,
                                        k.tipe_kamar,
                                        k.no_kamar,
                                        r.check_in,
                                        r.check_out
                                     FROM reservasi r
                                     JOIN tamu t ON r.id_tamu = t.id_tamu
                                     JOIN kamar k ON r.id_kamar = k.id_kamar
                                     ORDER BY r.check_in DESC"; // Urutkan dari yg terbaru

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    dtReservasi = new DataTable();
                    adapter.Fill(dtReservasi);

                    DisplayData(dtReservasi); // Tampilkan Data Manual Biar Rapi
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal: " + ex.Message);
            }
        }

        private void DisplayData(DataTable dt)
        {
            // 1. Bersihkan Tabel
            guna2DataGridView1.DataSource = null;
            guna2DataGridView1.Rows.Clear();
            guna2DataGridView1.Columns.Clear();

            // 2. Buat Kolom Manual (Agar Style Konsisten)
            AddTextColumn("colNo", "No", 50);
            AddTextColumn("colTamu", "Nama Tamu", 150);
            AddTextColumn("colTipe", "Tipe Kamar", 120);
            AddTextColumn("colNoKamar", "No Kamar", 80);
            AddTextColumn("colCheckIn", "Check In", 120);
            AddTextColumn("colCheckOut", "Check Out", 120);

            // Kolom ID Database (Sembunyikan)
            AddTextColumn("colID", "ID", 0);
            guna2DataGridView1.Columns["colID"].Visible = false;

            // 3. Isi Data dengan Nomor Urut (1, 2, 3...)
            int nomor = 1;
            foreach (DataRow row in dt.Rows)
            {
                // Format tanggal biar cantik (dd-MM-yyyy)
                string tglIn = Convert.ToDateTime(row["check_in"]).ToString("dd MMM yyyy");
                string tglOut = Convert.ToDateTime(row["check_out"]).ToString("dd MMM yyyy");

                guna2DataGridView1.Rows.Add(
                    nomor++,
                    row["nama_tamu"],
                    row["tipe_kamar"],
                    row["no_kamar"],
                    tglIn,
                    tglOut,
                    row["id_reservasi"]
                );
            }

            // 4. Fix Tampilan Tabel (Header Emas)
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

        // ============================================================
        // 🔍 FILTER & SEARCH
        // ============================================================
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            FilterData();
        }
        private void guna2DariTanggal_ValueChanged(object sender, EventArgs e) { FilterData(); }
        private void guna2SampaiTanggal_ValueChanged(object sender, EventArgs e) { FilterData(); }

        private void FilterData()
        {
            if (dtReservasi == null) return;

            string search = guna2Cari.Text.Trim();
            string tglDari = guna2DariTanggal.Value.ToString("yyyy-MM-dd");
            string tglSampai = guna2SampaiTanggal.Value.ToString("yyyy-MM-dd");

            DataView dv = dtReservasi.DefaultView;

            // Filter Gabungan (Nama DAN Tanggal)
            string filter = $"nama_tamu LIKE '%{search}%' AND check_in >= '{tglDari}' AND check_out <= '{tglSampai}'";
            dv.RowFilter = filter;

            DisplayData(dv.ToTable()); // Tampilkan ulang hasil filter
        }

        private void guna2Reset_Click(object sender, EventArgs e)
        {
            guna2Cari.Text = "";
            guna2DariTanggal.Value = DateTime.Now.AddMonths(-1); // Default sebulan ke belakang biar enak
            guna2SampaiTanggal.Value = DateTime.Now.AddMonths(1);

            if (dtReservasi != null) DisplayData(dtReservasi);
        }

        // ============================================================
        // 📤 EXPORT EXCEL
        // ============================================================
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Reservasi.xlsx";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    // Buat DataTable bersih untuk Excel (Tanpa kolom ID)
                    DataTable dtExport = new DataTable("Reservasi");
                    dtExport.Columns.Add("No");
                    dtExport.Columns.Add("Nama Tamu");
                    dtExport.Columns.Add("Tipe Kamar");
                    dtExport.Columns.Add("No Kamar");
                    dtExport.Columns.Add("Check In");
                    dtExport.Columns.Add("Check Out");

                    foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                    {
                        dtExport.Rows.Add(
                            row.Cells["colNo"].Value,
                            row.Cells["colTamu"].Value,
                            row.Cells["colTipe"].Value,
                            row.Cells["colNoKamar"].Value,
                            row.Cells["colCheckIn"].Value,
                            row.Cells["colCheckOut"].Value
                        );
                    }

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(dtExport);
                        ws.Columns().AdjustToContents();
                        wb.SaveAs(save.FileName);
                    }
                    MessageBox.Show("Export berhasil!", "Sukses");
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

        // Event Kosong
        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }
}
