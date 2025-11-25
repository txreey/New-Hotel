using MySql.Data.MySqlClient;
using System;
using System.Data;
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

            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardAdmin());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarA());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuA());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiA());
            guna2DataReservasi.Click += (s, e) => OpenForm(new DataReservasi());
            guna2DataUser.Click += (s, e) => OpenForm(new DataUser());
            guna2LaporanKeuangan.Click += (s, e) => OpenForm(new LaporanKeuangan2());
            guna2Logout.Click += (s, e) => Logout();

            this.Load += DataReservasi_Load;
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

        // ============================================================
        //  LOAD DATA DARI DATABASE
        // ============================================================
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
                                     JOIN kamar k ON r.id_kamar = k.id_kamar";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);

                    dtReservasi = new DataTable();
                    adapter.Fill(dtReservasi);

                    guna2DataGridView1.DataSource = dtReservasi;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data reservasi: " + ex.Message);
            }
        }

        private void DataReservasi_Load(object sender, EventArgs e)
        {
            LoadDataReservasi();
        }

        // ============================================================
        //  FITUR PENCARIAN
        // ============================================================
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            if (dtReservasi == null) return;

            string search = guna2Cari.Text.Trim();

            DataView dv = dtReservasi.DefaultView;

            if (string.IsNullOrEmpty(search))
            {
                dv.RowFilter = "";
            }
            else
            {
                dv.RowFilter = $"nama_tamu LIKE '%{search}%'";
            }

            guna2DataGridView1.DataSource = dv;
        }

        // ============================================================
        //   FILTER TANGGAL
        // ============================================================
        private void ApplyDateFilter()
        {
            if (dtReservasi == null) return;

            DateTime dari = guna2DariTanggal.Value.Date;
            DateTime sampai = guna2SampaiTanggal.Value.Date;

            DataView dv = dtReservasi.DefaultView;

            dv.RowFilter = $"check_in >= '#{dari:yyyy-MM-dd}#' AND check_out <= '#{sampai:yyyy-MM-dd}#'";

            guna2DataGridView1.DataSource = dv;
        }

        private void guna2DariTanggal_ValueChanged(object sender, EventArgs e)
        {
            ApplyDateFilter();
        }

        private void guna2SampaiTanggal_ValueChanged(object sender, EventArgs e)
        {
            ApplyDateFilter();
        }

        // ============================================================
        //  BUTTON RESET FILTER
        // ============================================================
        private void guna2Reset_Click(object sender, EventArgs e)
        {
            guna2Cari.Text = "";
            guna2DariTanggal.Value = DateTime.Now;
            guna2SampaiTanggal.Value = DateTime.Now;

            guna2DataGridView1.DataSource = dtReservasi;

            dtReservasi.DefaultView.RowFilter = "";
        }

        // ============================================================
        //  EXPORT EXCEL
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
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(dtReservasi, "Reservasi");

                        // Format tanggal supaya tidak jadi ####
                        ws.Column(5).Style.DateFormat.Format = "dd MMMM yyyy";
                        ws.Column(6).Style.DateFormat.Format = "dd MMMM yyyy";

                        // Autofit semua kolom
                        ws.Columns().AdjustToContents();

                        wb.SaveAs(save.FileName);
                    }

                    MessageBox.Show("Export berhasil!", "Sukses");
                }
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
