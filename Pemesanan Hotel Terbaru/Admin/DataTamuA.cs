using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;
using ClosedXML.Excel;   // EXPORT EXCEL

namespace Pemesanan_Hotel_Terbaru.Admin
{
    public partial class DataTamuA : Form
    {
        private DataTable dtTamu; // Penyimpanan data utama

        public DataTamuA()
        {
            InitializeComponent();

            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardAdmin());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarA());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuA());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiA());
            guna2DataReservasi.Click += (s, e) => OpenForm(new DataReservasi());
            guna2DataUser.Click += (s, e) => OpenForm(new DataUser());
            guna2LaporanKeuangan.Click += (s, e) => OpenForm(new LaporanKeuangan2());
            guna2Logout.Click += (s, e) => Logout();

            this.Load += DataTamuA_Load;
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
                Login loginForm = new Login();
                loginForm.Show();
            }
        }

        // =======================================================
        // LOAD DATA DARI DATABASE
        // =======================================================
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

        private void DataTamuA_Load(object sender, EventArgs e)
        {
            LoadDataTamu();
        }

        // =======================================================
        // FITUR PENCARIAN NAMA TAMU
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
        // EXPORT TO EXCEL (ClosedXML) — TANPA DOUBLE SAVE!
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

                        // Nomor HP jangan auto-format (diset jadi TEXT)
                        ws.Cell(rowExcel, 5).Value = "'" + row["no_handphone"].ToString();

                        ws.Cell(rowExcel, 6).Value = row["email"].ToString();

                        rowExcel++;
                    }

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

        }
    }
}


