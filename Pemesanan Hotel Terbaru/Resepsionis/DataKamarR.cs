using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class DataKamarR : Form
    {
        private DataTable dataKamarTable;

        public DataKamarR()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Navigasi Sidebar
            guna2Dashboard.Click += (s, e) => PindahForm(new DashboardResepsionis());
            guna2DataKamar.Click += (s, e) => { LoadDataKamar(); }; // Refresh
            guna2DataTamu.Click += (s, e) => PindahForm(new DataTamuR());
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiR());
            guna2Reservasi.Click += (s, e) => PindahForm(new Reservasi());
            guna2TransaksiPembayaran.Click += (s, e) => PindahForm(new TransaksiPembayaran());
            guna2Logout.Click += (s, e) => Logout();

            // 3. Event Lain
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2ExportExcel.Click += guna2ExportExcel_Click;
            this.Load += DataKamarR_Load;
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (KONSISTEN)
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

            // Highlight Data Kamar
            guna2DataKamar.FillColor = ColorTranslator.FromHtml("#E2E8F0");
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
        private void DataKamarR_Load(object sender, EventArgs e)
        {
            LoadDataKamar();

            // Optional: Auto Refresh tiap 5 detik
            Timer timer = new Timer();
            timer.Interval = 5000;
            timer.Tick += (s, ev) => LoadDataKamar();
            // timer.Start(); // Uncomment kalau mau auto-refresh
        }

        private void LoadDataKamar()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM kamar";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    dataKamarTable = new DataTable();
                    adapter.Fill(dataKamarTable);
                    DisplayData(dataKamarTable);
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal load: " + ex.Message); }
        }

        private void DisplayData(DataTable dt)
        {
            guna2DataGridView1.Rows.Clear();
            guna2DataGridView1.Columns.Clear();

            // 1. Kolom Nomor Urut
            AddTextColumn("colNo", "No", 50);

            // 2. Kolom Data
            AddTextColumn("colTipe", "Tipe Kamar", 150);
            AddTextColumn("colNoKamar", "No Kamar", 100);
            AddTextColumn("colStatus", "Status", 100);
            AddTextColumn("colHarga", "Harga", 120);
            AddTextColumn("colDeskripsi", "Deskripsi", 200);

            // 3. Kolom Gambar
            DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
            imgCol.Name = "colPicture";
            imgCol.HeaderText = "Gambar";
            imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
            guna2DataGridView1.Columns.Add(imgCol);

            // Resepsionis: Read Only (Tanpa Tombol Edit/Hapus)
            guna2DataGridView1.AllowUserToAddRows = false;
            guna2DataGridView1.ReadOnly = true;

            // 4. Isi Data
            int nomor = 1;
            foreach (DataRow row in dt.Rows)
            {
                Image gambar = null;
                string path = row["picture"]?.ToString();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try { using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) { gambar = Image.FromStream(fs); } } catch { }
                }

                guna2DataGridView1.Rows.Add(
                    nomor++,
                    row["tipe_kamar"],
                    row["no_kamar"],
                    row["status"],
                    row["harga"],
                    row["deskripsi"],
                    gambar
                );
            }

            // 5. Fix Tampilan
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
            guna2DataGridView1.RowTemplate.Height = 80;
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
            string keyword = guna2Cari.Text.Trim();
            if (string.IsNullOrEmpty(keyword)) DisplayData(dataKamarTable);
            else
            {
                DataView dv = dataKamarTable.DefaultView;
                dv.RowFilter = $"tipe_kamar LIKE '%{keyword}%' OR no_kamar LIKE '%{keyword}%'";
                DisplayData(dv.ToTable());
            }
        }

        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataKamarTable == null || dataKamarTable.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data!"); return;
                }
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Kamar_Resepsionis.xlsx";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        // Buat tabel bersih utk export
                        DataTable dtExcel = new DataTable("Kamar");
                        dtExcel.Columns.Add("No");
                        dtExcel.Columns.Add("Tipe");
                        dtExcel.Columns.Add("Nomor");
                        dtExcel.Columns.Add("Status");
                        dtExcel.Columns.Add("Harga");
                        dtExcel.Columns.Add("Deskripsi");

                        foreach (DataGridViewRow r in guna2DataGridView1.Rows)
                        {
                            dtExcel.Rows.Add(
                                r.Cells["colNo"].Value, r.Cells["colTipe"].Value, r.Cells["colNoKamar"].Value,
                                r.Cells["colStatus"].Value, r.Cells["colHarga"].Value, r.Cells["colDeskripsi"].Value
                            );
                        }

                        wb.Worksheets.Add(dtExcel);
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
