using ClosedXML.Excel;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace Pemesanan_Hotel_Terbaru.Admin
{
    public partial class LaporanKeuangan2 : Form
    {
        private DataTable dataAsli = new DataTable();

        public LaporanKeuangan2()
        {
            InitializeComponent();

            // ============= NAVIGATION ADMIN (SAMA SEPERTI DataTamuA) ==================
            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardAdmin());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarA());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuA());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiA());
            guna2DataReservasi.Click += (s, e) => OpenForm(new DataReservasi());
            guna2DataUser.Click += (s, e) => OpenForm(new DataUser());
            guna2LaporanKeuangan.Click += (s, e) => OpenForm(new LaporanKeuangan2());
            guna2Logout.Click += (s, e) => Logout();

            // Load awal
            this.Load += LaporanKeuangan2_Load;

            // Event filter & export
            guna2DariTanggal.ValueChanged += FilterTanggal;
            guna2SampaiTanggal.ValueChanged += FilterTanggal;
            guna2Reset.Click += guna2Reset_Click;
            guna2ExportExcel.Click += guna2ExportExcel_Click;
        }

        // =========================================================
        // OPEN FORM
        // =========================================================
        private void OpenForm(Form targetForm)
        {
            this.Hide();
            targetForm.ShowDialog();
            this.Close();
        }

        // =========================================================
        // LOGOUT
        // =========================================================
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

        // =========================================================
        // LOAD DATA
        // =========================================================
        private void LaporanKeuangan2_Load(object sender, EventArgs e)
        {
            LoadDataKeuangan();
        }

        private void LoadDataKeuangan()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            DATE(tanggal_transaksi) AS `Tanggal`,
                            COUNT(*) AS `JumlahKamar`,
                            SUM(uang_masuk) AS `Kredit`
                        FROM transaksi
                        GROUP BY DATE(tanggal_transaksi)
                        ORDER BY `Tanggal` DESC";

                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    dataAsli.Clear();
                    da.Fill(dataAsli);

                    HitungSaldo(dataAsli);
                    guna2DataGridView1.DataSource = dataAsli;

                    UpdateTotalPendapatan(dataAsli);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load laporan keuangan: " + ex.Message);
            }
        }

        // =========================================================
        // HITUNG SALDO
        // =========================================================
        private void HitungSaldo(DataTable dt)
        {
            if (!dt.Columns.Contains("Saldo"))
                dt.Columns.Add("Saldo", typeof(decimal));

            decimal saldo = 0;

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                saldo += Convert.ToDecimal(dt.Rows[i]["Kredit"]);
                dt.Rows[i]["Saldo"] = saldo;
            }
        }

        // =========================================================
        // TOTAL PENDAPATAN PANEL
        // =========================================================
        private void UpdateTotalPendapatan(DataTable dt)
        {
            decimal total = 0;

            foreach (DataRow row in dt.Rows)
                total += Convert.ToDecimal(row["Kredit"]);

            guna2TotalPendapatan.Text = "Rp " + total.ToString("N0");
        }

        // =========================================================
        // FILTER TANGGAL
        // =========================================================
        private void FilterTanggal(object sender, EventArgs e)
        {
            if (dataAsli.Rows.Count == 0)
                return;

            DateTime dari = guna2DariTanggal.Value.Date;
            DateTime sampai = guna2SampaiTanggal.Value.Date;

            DataView dv = new DataView(dataAsli);
            dv.RowFilter =
                $"Tanggal >= '{dari:yyyy-MM-dd}' AND Tanggal <= '{sampai:yyyy-MM-dd}'";

            DataTable hasil = dv.ToTable();
            HitungSaldo(hasil);

            guna2DataGridView1.DataSource = hasil;
            UpdateTotalPendapatan(hasil);
        }

        // =========================================================
        // RESET
        // =========================================================
        private void guna2Reset_Click(object sender, EventArgs e)
        {
            guna2DariTanggal.Value = DateTime.Now.Date;
            guna2SampaiTanggal.Value = DateTime.Now.Date;

            guna2DataGridView1.DataSource = dataAsli;
            UpdateTotalPendapatan(dataAsli);
        }

        // =========================================================
        // EXPORT EXCEL - UPDATED WITH DYNAMIC FILENAME & TOTAL
        // =========================================================
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable exportTable;

                if (guna2DataGridView1.DataSource is DataView dv)
                    exportTable = dv.ToTable();
                else
                    exportTable = (DataTable)guna2DataGridView1.DataSource;

                if (exportTable.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diexport!");
                    return;
                }

                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";

                // NAMA FILE DINAMIS SESUAI RANGE TANGGAL
                string dari = guna2DariTanggal.Value.ToString("dd-MMM-yyyy");
                string sampai = guna2SampaiTanggal.Value.ToString("dd-MMM-yyyy");
                save.FileName = $"Laporan_Keuangan_{dari}_sampai_{sampai}.xlsx";

                if (save.ShowDialog() != DialogResult.OK)
                    return;

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add(exportTable, "Keuangan Admin");

                    // TAMBAH TOTAL ROW DI EXCEL
                    int lastRow = ws.LastRowUsed().RowNumber();
                    ws.Cell(lastRow + 2, 2).Value = "TOTAL PENDAPATAN:";
                    ws.Cell(lastRow + 2, 2).Style.Font.Bold = true;

                    decimal totalPendapatan = 0;
                    foreach (DataRow row in exportTable.Rows)
                        totalPendapatan += Convert.ToDecimal(row["Kredit"]);

                    ws.Cell(lastRow + 2, 3).Value = totalPendapatan;
                    ws.Cell(lastRow + 2, 3).Style.Font.Bold = true;
                    ws.Cell(lastRow + 2, 3).Style.NumberFormat.Format = "#,##0";

                    ws.Columns().AdjustToContents();
                    wb.SaveAs(save.FileName);
                }

                MessageBox.Show("Export berhasil!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export gagal: " + ex.Message);
            }
        }

        private void guna2TotalPendapatan_Click(object sender, EventArgs e)
        {
            //
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //
        }

        private void guna2DariTanggal_ValueChanged(object sender, EventArgs e)
        {
            //
        }

        private void LaporanKeuangan2_Load_1(object sender, EventArgs e)
        {

        }
    }
}
