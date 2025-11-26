using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Admin
{
    public partial class DataKamarA : Form
    {
        DataTable dataKamarTable;

        public DataKamarA()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Navigasi (Sama Persis Dashboard)
            guna2Dashboard.Click += (s, e) => PindahForm(new DashboardAdmin());
            guna2DataKamar.Click += (s, e) => { /* Refresh */ LoadDataKamar(); };
            guna2DataTamu.Click += (s, e) => PindahForm(new DataTamuA());
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiA());
            guna2DataReservasi.Click += (s, e) => PindahForm(new DataReservasi());
            guna2DataUser.Click += (s, e) => PindahForm(new DataUser());
            guna2LaporanKeuangan.Click += (s, e) => PindahForm(new LaporanKeuangan2());
            guna2Logout.Click += (s, e) => Logout();

            guna2Tambah.Click += guna2Tambah_Click;
            guna2ExportExcel.Click += guna2ExportExcel_Click;
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            this.Load += DataKamarA_Load;
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (KONSISTEN DASHBOARD)
        // ============================================================
        private void ApplyElegantTheme()
        {
            // Background Utama
            this.BackColor = ColorTranslator.FromHtml("#F4F6F8");

            // Sidebar & Header (Cream Terang)
            guna2Panel1.FillColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel1.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel5.FillColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel5.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2PictureBox1.BackColor = Color.Transparent;

            // Label Judul (Gelap)
            guna2HtmlLabel1.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2HtmlLabel8.ForeColor = ColorTranslator.FromHtml("#333333");

            // Tombol Aksi (Sama dengan tema)
            // Tambah = Emas (Biar mewah)
            guna2Tambah.FillColor = ColorTranslator.FromHtml("#C5A059");
            guna2Tambah.ForeColor = Color.White;

            // Export = Abu Gelap (Netral, Profesional)
            guna2ExportExcel.FillColor = ColorTranslator.FromHtml("#2C3E50");
            guna2ExportExcel.ForeColor = Color.White;

            // Style Tombol Sidebar
            StyleSidebarButton(guna2Dashboard);
            StyleSidebarButton(guna2DataKamar);
            StyleSidebarButton(guna2DataTamu);
            StyleSidebarButton(guna2LaporanTransaksi);
            StyleSidebarButton(guna2DataReservasi);
            StyleSidebarButton(guna2DataUser);
            StyleSidebarButton(guna2LaporanKeuangan);
            StyleSidebarButton(guna2Logout);

            // Highlight Data Kamar (Aktif)
            guna2DataKamar.FillColor = ColorTranslator.FromHtml("#E2E8F0");
        }

        private void StyleSidebarButton(Guna.UI2.WinForms.Guna2Button btn)
        {
            btn.FillColor = Color.Transparent;
            btn.CheckedState.FillColor = Color.Transparent;
            btn.ForeColor = ColorTranslator.FromHtml("#333333"); // Teks Abu Gelap
            btn.HoverState.FillColor = ColorTranslator.FromHtml("#E2E8F0"); // Hover Abu Muda
            btn.HoverState.ForeColor = Color.Black;
        }

        // ============================================================
        // 🛠️ TABEL: WARNA & NOMOR URUT
        // ============================================================
        private void DataKamarA_Load(object sender, EventArgs e)
        {
            LoadDataKamar();
            FixTableStyle(); // Panggil style setelah load
        }

        private void LoadDataKamar()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT * FROM kamar", conn);
                    dataKamarTable = new DataTable();
                    adapter.Fill(dataKamarTable);
                    DisplayData(dataKamarTable);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void DisplayData(DataTable dt)
        {
            guna2DataGridView1.Rows.Clear();
            guna2DataGridView1.Columns.Clear();

            // 1. Kolom NOMOR URUT (Bukan ID Database)
            AddTextColumn("colNoUrut", "No", 40);

            // 2. Kolom Data Lainnya
            // ID Database disembunyikan (Hidden) buat keperluan Edit/Hapus aja
            AddTextColumn("colID", "ID DB", 0);
            guna2DataGridView1.Columns["colID"].Visible = false;

            AddTextColumn("colTipe", "Tipe Kamar", 150);
            AddTextColumn("colNo", "No Kamar", 100);
            AddTextColumn("colStatus", "Status", 100);
            AddTextColumn("colHarga", "Harga", 120);
            AddTextColumn("colDeskripsi", "Deskripsi", 200);

            DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
            imgCol.Name = "colPicture";
            imgCol.HeaderText = "Gambar";
            imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
            guna2DataGridView1.Columns.Add(imgCol);

            AddButtonColumn("colEdit", "Edit");
            AddButtonColumn("colDelete", "Hapus");

            guna2DataGridView1.AllowUserToAddRows = false;

            // Loop Data & Buat Nomor Urut Manual
            int nomorUrut = 1; // Mulai dari 1

            foreach (DataRow row in dt.Rows)
            {
                Image gambar = null;
                string path = row["picture"]?.ToString();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try { using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) { gambar = Image.FromStream(fs); } } catch { }
                }

                guna2DataGridView1.Rows.Add(
                    nomorUrut++, // Masukkan Nomor Urut (1, 2, 3...)
                    row["id_kamar"], // ID Database (Disembunyikan)
                    row["tipe_kamar"],
                    row["no_kamar"],
                    row["status"],
                    row["harga"],
                    row["deskripsi"],
                    gambar,
                    "Edit", "Hapus"
                );
            }

            FixTableStyle(); // Apply warna ulang biar ID ga biru
        }

        private void FixTableStyle()
        {
            guna2DataGridView1.EnableHeadersVisualStyles = false;
            guna2DataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Header Emas (Semua Kolom)
            var headerStyle = new DataGridViewCellStyle();
            headerStyle.BackColor = ColorTranslator.FromHtml("#C5A059");
            headerStyle.ForeColor = Color.White;
            headerStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            headerStyle.SelectionBackColor = ColorTranslator.FromHtml("#C5A059");

            guna2DataGridView1.ColumnHeadersDefaultCellStyle = headerStyle;
            guna2DataGridView1.ColumnHeadersHeight = 40;

            foreach (DataGridViewColumn col in guna2DataGridView1.Columns)
            {
                col.HeaderCell.Style = headerStyle; // Paksa semua kolom Emas
            }

            // Isi Tabel
            guna2DataGridView1.DefaultCellStyle.BackColor = Color.White;
            guna2DataGridView1.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2DataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F0E68C"); // Kuning Lembut
            guna2DataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;
            guna2DataGridView1.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FAFAFA");
            guna2DataGridView1.RowTemplate.Height = 60;
        }

        private void AddTextColumn(string name, string header, int width)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.Name = name;
            col.HeaderText = header;
            col.Width = width;
            guna2DataGridView1.Columns.Add(col);
        }

        private void AddButtonColumn(string name, string text)
        {
            DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
            btn.Name = name;
            btn.HeaderText = text;
            btn.Text = text;
            btn.UseColumnTextForButtonValue = true;
            btn.FlatStyle = FlatStyle.Flat;
            btn.DefaultCellStyle.BackColor = Color.WhiteSmoke;
            btn.DefaultCellStyle.ForeColor = Color.Black;
            guna2DataGridView1.Columns.Add(btn);
        }

        // ============================================================
        // EVENT HANDLING
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
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Kamar.xlsx";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(dataKamarTable, "Data Kamar");
                        wb.SaveAs(save.FileName);
                    }
                    MessageBox.Show("Export Berhasil!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal: " + ex.Message); }
        }

        private void guna2Tambah_Click(object sender, EventArgs e)
        {
            using (TambahKamar t = new TambahKamar())
            {
                if (t.ShowDialog() == DialogResult.OK) LoadDataKamar();
            }
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string colName = guna2DataGridView1.Columns[e.ColumnIndex].Name;

            // Ambil ID Asli dari kolom tersembunyi (colID)
            string idAsli = guna2DataGridView1.Rows[e.RowIndex].Cells["colID"].Value.ToString();

            if (colName == "colEdit")
            {
                this.Hide();
                new EditKamar(idAsli).ShowDialog();
                this.Show();
                LoadDataKamar();
            }
            else if (colName == "colDelete")
            {
                if (MessageBox.Show("Apakah Anda Ingin Menghapus data ini?", "Hapus", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = Koneksi.GetConnection())
                        {
                            conn.Open();
                            new MySqlCommand($"DELETE FROM kamar WHERE id_kamar='{idAsli}'", conn).ExecuteNonQuery();
                        }
                        MessageBox.Show("Data Berhasil Terhapus.");
                        LoadDataKamar(); // Refresh biar nomor urut balik 1,2,3
                    }
                    catch (Exception ex) { MessageBox.Show("Gagal: " + ex.Message); }
                }
            }
        }

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
    }
}
