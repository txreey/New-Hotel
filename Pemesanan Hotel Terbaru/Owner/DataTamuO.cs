using MySql.Data.MySqlClient;
using Pemesanan_Hotel_Terbaru.Admin;
using System;
using System.Data;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Owner
{
    public partial class DataTamuO : Form
    {
        private DataTable dtTamu; // Tempat penyimpanan data utama

        public DataTamuO()
        {
            InitializeComponent();

            // Navigasi
            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardOwner());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarO());
            guna2DataReservasi.Click += (s, e) => OpenForm(new DataReservasiO());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuO());
            guna2DataUser.Click += (s, e) => OpenForm(new DataUserO());
            guna2LaporanKeuangan.Click += (s, e) => OpenForm(new LaporanKeuangan());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiO());
            guna2Logout.Click += (s, e) => Logout();

            this.Load += DataTamuO_Load;
            guna2Cari.TextChanged += guna2Cari_TextChanged;
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
                Login loginForm = new Login();
                loginForm.Show();
            }
        }

        // ============================================================
        // LOAD DATA TAMU (Owner: Read Only)
        // ============================================================
        private void LoadDataTamuO()
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

                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    dtTamu = new DataTable();
                    da.Fill(dtTamu);

                    guna2DataGridView1.DataSource = dtTamu;

                    // READ ONLY (karena Owner hanya melihat) + HILANGKAN ROW KOSONG
                    guna2DataGridView1.ReadOnly = true;
                    guna2DataGridView1.AllowUserToAddRows = false;
                    guna2DataGridView1.AllowUserToDeleteRows = false;
                    guna2DataGridView1.AllowUserToResizeRows = false;

                    guna2DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    guna2DataGridView1.MultiSelect = false;
                    guna2DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data tamu: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataTamuO_Load(object sender, EventArgs e)
        {
            LoadDataTamuO();
        }

        // ============================================================
        // FITUR PENCARIAN — sama seperti Admin
        // ============================================================
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

        // ============================================================
        // EXPORT EXCEL — ClosedXML (sama dengan Admin)
        // ============================================================
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Tamu_Owner.xlsx";

                if (save.ShowDialog() != DialogResult.OK)
                    return;

                using (XLWorkbook wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Data Tamu");

                    // Header Excel
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
                        ws.Cell(rowExcel, 5).Value = "'" + row["no_handphone"].ToString(); // hindari auto-format
                        ws.Cell(rowExcel, 6).Value = row["email"].ToString();

                        rowExcel++;
                    }

                    ws.Columns().AdjustToContents();
                    wb.SaveAs(save.FileName);
                }

                MessageBox.Show("Export Excel berhasil!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal export: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Owner tidak bisa klik tombol apapun
        }
    }
}

