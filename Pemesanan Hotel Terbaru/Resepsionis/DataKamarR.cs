using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

using ClosedXML.Excel;                         // ✔ WORKBOOK + EXCEL
using DocumentFormat.OpenXml.Packaging;        // ✔ diperlukan ClosedXML
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class DataKamarR : Form
    {
        // Sama seperti Admin: simpan DataTable untuk pencarian & export
        private DataTable dataKamarTable;

        public DataKamarR()
        {
            InitializeComponent();

            // 🔹 Navigasi antar form
            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardResepsionis());
            //guna2Booking.Click += (s, e) => OpenForm(new Booking());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarR());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuR());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiR());
            guna2Reservasi.Click += (s, e) => OpenForm(new Reservasi());
            guna2TransaksiPembayaran.Click += (s, e) => OpenForm(new TransaksiPembayaran());
            guna2Logout.Click += (s, e) => Logout();

            // 🔍 Event untuk pencarian & export (sama seperti Admin)
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2ExportExcel.Click += guna2ExportExcel_Click;

            // 🔹 Attach load
            this.Load += DataKamarR_Load;
        }

        private void OpenForm(Form targetForm)
        {
            this.Hide();
            targetForm.ShowDialog();
            this.Close();
        }

        // 🚪 Logout
        private void Logout()
        {
            DialogResult result = MessageBox.Show(
                "Apakah kamu yakin ingin logout?",
                "Konfirmasi Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.Hide();
                Login loginForm = new Login();
                loginForm.Show();
            }
        }

        // 🔹 Saat form dibuka, load data dan auto-refresh tiap 5 detik
        private void DataKamarR_Load(object sender, EventArgs e)
        {
            LoadDataKamar();

            // Timer otomatis refresh
            Timer timer = new Timer();
            timer.Interval = 5000; // refresh setiap 5 detik
            timer.Tick += (s, ev) => LoadDataKamar();
            timer.Start();
        }

        // 🔹 Load data kamar dari database (sinkron dengan admin)
        private void LoadDataKamar()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM kamar";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);

                    dataKamarTable = new DataTable();
                    adapter.Fill(dataKamarTable);

                    DisplayData(dataKamarTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat data kamar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 🔹 DisplayData dipecah agar bisa dipanggil ulang setelah filter
        private void DisplayData(DataTable dt)
        {
            guna2DataGridView1.Rows.Clear();
            guna2DataGridView1.Columns.Clear();

            // 🔸 Kolom sesuai admin
            guna2DataGridView1.Columns.Add("colID", "ID");
            guna2DataGridView1.Columns.Add("colTipe", "Tipe Kamar");
            guna2DataGridView1.Columns.Add("colNo", "No Kamar");
            guna2DataGridView1.Columns.Add("colStatus", "Status");
            guna2DataGridView1.Columns.Add("colHarga", "Harga");
            guna2DataGridView1.Columns.Add("colDeskripsi", "Deskripsi");

            // 🔸 Kolom gambar
            DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
            imgCol.Name = "colPicture";
            imgCol.HeaderText = "Gambar";
            imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
            guna2DataGridView1.Columns.Add(imgCol);

            // 🔸 Pengaturan tampilan grid
            guna2DataGridView1.RowTemplate.Height = 100; // tinggi tetap
            guna2DataGridView1.AllowUserToAddRows = false;
            guna2DataGridView1.ReadOnly = true; // resepsionis read-only
            guna2DataGridView1.RowHeadersVisible = false;
            guna2DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            guna2DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 🔸 Masukkan data ke grid
            foreach (DataRow row in dt.Rows)
            {
                Image gambar = null;
                string path = row["picture"]?.ToString();

                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try
                    {
                        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            Image original = Image.FromStream(fs);
                            gambar = ResizeImage(original, 120, 100); // ⬅️ Ukuran tetap
                        }
                    }
                    catch
                    {
                        gambar = null;
                    }
                }

                guna2DataGridView1.Rows.Add(
                    row["id_kamar"],
                    row["tipe_kamar"],
                    row["no_kamar"],
                    row["status"],
                    row["harga"],
                    row["deskripsi"],
                    gambar
                );
            }
        }

        // 🔹 Fungsi bantu untuk resize gambar ke ukuran seragam
        private Image ResizeImage(Image img, int width, int height)
        {
            Bitmap b = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, width, height);
            }
            return b;
        }

        // 🔹 Tidak ada aksi klik (karena read-only)
        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Kosong - resepsionis tidak bisa edit/delete
        }

        // 🔍 SEARCHING — sama seperti Admin
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataKamarTable == null)
                {
                    return;
                }

                string keyword = guna2Cari.Text.Trim();

                if (string.IsNullOrEmpty(keyword))
                {
                    DisplayData(dataKamarTable);
                }
                else
                {
                    DataView dv = dataKamarTable.DefaultView;

                    // Filter pada tipe_kamar atau no_kamar (case-insensitive default depends on DB)
                    // Escape single quotes to avoid filter syntax error
                    string safe = keyword.Replace("'", "''");
                    dv.RowFilter = $"tipe_kamar LIKE '%{safe}%' OR no_kamar LIKE '%{safe}%'";
                    DisplayData(dv.ToTable());
                }
            }
            catch (Exception ex)
            {
                // Kalau ada error pada filter, tampilkan semua dan beri tahu (tidak crash)
                DisplayData(dataKamarTable);
                // Optional: log error jika perlu
                // MessageBox.Show($"Filter error: {ex.Message}");
            }
        }

        // 📌 EXPORT EXCEL — meniru Admin (ClosedXML)
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataKamarTable == null || dataKamarTable.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diexport.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Kamar.xlsx";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    // BUAT DATATABLE SAMA SEPERTI DI GRID (tanpa gambar)
                    DataTable dt = new DataTable();
                    dt.Columns.Add("ID");
                    dt.Columns.Add("Tipe Kamar");
                    dt.Columns.Add("No Kamar");
                    dt.Columns.Add("Status");
                    dt.Columns.Add("Harga");
                    dt.Columns.Add("Deskripsi");

                    foreach (DataRow row in dataKamarTable.Rows)
                    {
                        dt.Rows.Add(
                            row["id_kamar"].ToString(),
                            row["tipe_kamar"].ToString(),
                            row["no_kamar"].ToString(),
                            row["status"].ToString(),
                            row["harga"].ToString(),
                            row["deskripsi"].ToString()
                        );
                    }

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(dt, "Data Kamar");

                        // Format kolom harga agar dianggap angka (kolom ke-5, 1-based index)
                        ws.Column(5).Style.NumberFormat.Format = "#,##0";

                        // Autofit kolom
                        ws.Columns().AdjustToContents();

                        wb.SaveAs(save.FileName);
                    }

                    MessageBox.Show("Export berhasil!", "Sukses");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal export: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
