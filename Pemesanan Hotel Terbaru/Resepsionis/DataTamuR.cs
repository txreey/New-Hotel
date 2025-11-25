using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;
using ClosedXML.Excel;  // EXPORT EXCEL

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class DataTamuR : Form
    {
        private DataTable dtTamu; // 🔹 Sama seperti Admin: temp data utama

        public DataTamuR()
        {
            InitializeComponent();

            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardResepsionis());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarR());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuR());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiR());
            guna2Reservasi.Click += (s, e) => OpenForm(new Reservasi());
            guna2TransaksiPembayaran.Click += (s, e) => OpenForm(new TransaksiPembayaran());
            guna2Logout.Click += (s, e) => Logout();

            this.Load += DataTamuR_Load;

            // 🔎 Pencarian
            guna2Cari.TextChanged += guna2Cari_TextChanged;

            // 📤 Export Excel
            guna2ExportExcel.Click += guna2ExportExcel_Click;
        }

        private void OpenForm(Form targetForm)
        {
            this.Hide();
            targetForm.ShowDialog();
            this.Close();
        }

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
                new Login().Show();
            }
        }

        // =======================================================
        // LOAD DATA
        // =======================================================
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
                    string query = @"SELECT 
                                        id_tamu,
                                        nama_tamu,
                                        nik,
                                        alamat,
                                        no_handphone,
                                        email
                                     FROM tamu";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    dtTamu = new DataTable();
                    adapter.Fill(dtTamu);

                    guna2DataGridView1.DataSource = dtTamu;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data tamu: " + ex.Message);
            }
        }

        // =======================================================
        // 🔍 FITUR PENCARIAN — SAMA EXACT DENGAN ADMIN
        // =======================================================
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            if (dtTamu == null) return;

            string search = guna2Cari.Text.Trim();

            if (string.IsNullOrEmpty(search))
            {
                dtTamu.DefaultView.RowFilter = "";
            }
            else
            {
                dtTamu.DefaultView.RowFilter = $"nama_tamu LIKE '%{search}%'";
            }

            guna2DataGridView1.DataSource = dtTamu.DefaultView;
        }

        // =======================================================
        // 📤 EXPORT EXCEL — ClosedXML — Sama 100% seperti Admin
        // =======================================================
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Tamu.xlsx";

                if (save.ShowDialog() != DialogResult.OK)
                    return;

                using (XLWorkbook wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Data Tamu");

                    // Header
                    string[] headers =
                    {
                        "ID", "Nama Tamu", "NIK", "Alamat", "No Handphone", "Email"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        ws.Cell(1, i + 1).Value = headers[i];
                        ws.Cell(1, i + 1).Style.Font.Bold = true;
                        ws.Cell(1, i + 1).Style.Fill.SetBackgroundColor(XLColor.LightGray);
                    }

                    int rowExcel = 2;

                    foreach (DataRow row in dtTamu.Rows)
                    {
                        ws.Cell(rowExcel, 1).Value = row["id_tamu"].ToString();
                        ws.Cell(rowExcel, 2).Value = row["nama_tamu"].ToString();
                        ws.Cell(rowExcel, 3).Value = row["nik"].ToString();
                        ws.Cell(rowExcel, 4).Value = row["alamat"].ToString();

                        // Nomor HP jangan auto-format (dipaksa TEXT)
                        ws.Cell(rowExcel, 5).Value = "'" + row["no_handphone"].ToString();

                        ws.Cell(rowExcel, 6).Value = row["email"].ToString();

                        rowExcel++;
                    }

                    // Autofit kolom
                    ws.Columns().AdjustToContents();

                    wb.SaveAs(save.FileName);
                }

                MessageBox.Show("Export berhasil!", "Sukses", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal export: " + ex.Message);
            }
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Resepsionis tidak edit/delete → tidak ada aksi
        }
    }
}

