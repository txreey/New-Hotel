using MySql.Data.MySqlClient;
using Pemesanan_Hotel_Terbaru.Admin;
using System;
using System.Data;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Owner
{
    public partial class LaporanTransaksiO : Form
    {
        private DataTable dataAsli = new DataTable(); // simpan data awal

        public LaporanTransaksiO()
        {
            InitializeComponent();

            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardOwner());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarO());
            guna2DataReservasi.Click += (s, e) => OpenForm(new DataReservasiO());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuO());
            guna2DataUser.Click += (s, e) => OpenForm(new DataUserO());
            guna2LaporanKeuangan.Click += (s, e) => OpenForm(new LaporanKeuangan());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiO());
            guna2Logout.Click += (s, e) => Logout();

            // 🔥 Event sama seperti ADMIN
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2DariTanggal.ValueChanged += FilterTanggal;
            guna2SampaiTanggal.ValueChanged += FilterTanggal;
            guna2Reset.Click += guna2Reset_Click;
            guna2ExportExcel.Click += guna2ExportExcel_Click;

            this.Load += LaporanTransaksiO_Load;
        }

        private void OpenForm(Form targetForm)
        {
            this.Hide();
            targetForm.ShowDialog();
            this.Close();
        }

        private void Logout()
        {
            var result = MessageBox.Show(
                "Apakah kamu yakin ingin logout?",
                "Konfirmasi Logout",
                MessageBoxButtons.YesNo
            );

            if (result == DialogResult.Yes)
            {
                this.Hide();
                new Login().Show();
            }
        }

        private void LaporanTransaksiO_Load(object sender, EventArgs e)
        {
            LoadLaporanTransaksiOwner();
        }

        // =======================================================
        // LOAD DATA ASLI
        // =======================================================
        private void LoadLaporanTransaksiOwner()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            tr.id_transaksi AS `ID Transaksi`,
                            tr.nama_tamu AS `Nama Tamu`,
                            k.tipe_kamar AS `Tipe Kamar`,
                            k.no_kamar AS `No Kamar`,
                            r.check_in AS `Check-In`,
                            r.check_out AS `Check-Out`,
                            tr.harga AS `Harga per Malam`,
                            tr.total_bayar AS `Total Bayar`,
                            tr.uang_masuk AS `Uang Masuk`,
                            tr.kembalian AS `Kembalian`,
                            tr.metode_pembayaran AS `Metode Pembayaran`,
                            tr.tanggal_transaksi AS `Tanggal Transaksi`
                        FROM transaksi tr
                        JOIN reservasi r ON tr.id_reservasi = r.id_reservasi
                        JOIN kamar k ON r.id_kamar = k.id_kamar
                        ORDER BY tr.tanggal_transaksi DESC";

                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    dataAsli.Clear();
                    da.Fill(dataAsli);

                    guna2DataGridView1.DataSource = dataAsli;

                    // READ ONLY
                    guna2DataGridView1.ReadOnly = true;
                    guna2DataGridView1.AllowUserToAddRows = false;
                    guna2DataGridView1.AllowUserToDeleteRows = false;
                    guna2DataGridView1.AllowUserToResizeRows = false;

                    guna2DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                    // FORMAT
                    guna2DataGridView1.Columns["Harga per Malam"].DefaultCellStyle.Format = "C0";
                    guna2DataGridView1.Columns["Total Bayar"].DefaultCellStyle.Format = "C0";
                    guna2DataGridView1.Columns["Uang Masuk"].DefaultCellStyle.Format = "C0";
                    guna2DataGridView1.Columns["Kembalian"].DefaultCellStyle.Format = "C0";
                    guna2DataGridView1.Columns["Tanggal Transaksi"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat laporan transaksi (Owner): " + ex.Message);
            }
        }

        // =======================================================
        // FILTER TANGGAL
        // =======================================================
        private void FilterTanggal(object sender, EventArgs e)
        {
            if (dataAsli.Rows.Count == 0)
                return;

            DateTime dari = guna2DariTanggal.Value.Date;
            DateTime sampai = guna2SampaiTanggal.Value.Date.AddDays(1).AddSeconds(-1);

            string dariStr = dari.ToString("yyyy-MM-dd HH:mm:ss");
            string sampaiStr = sampai.ToString("yyyy-MM-dd HH:mm:ss");

            DataView dv = new DataView(dataAsli);
            dv.RowFilter = $"[Tanggal Transaksi] >= '{dariStr}' AND [Tanggal Transaksi] <= '{sampaiStr}'";

            guna2DataGridView1.DataSource = dv;
        }

        // =======================================================
        // SEARCH
        // =======================================================
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            string keyword = guna2Cari.Text.Trim().Replace("'", "''");

            if (string.IsNullOrEmpty(keyword))
            {
                FilterTanggal(sender, e);
                return;
            }

            DataView dv = new DataView(dataAsli);
            dv.RowFilter =
                $"[Nama Tamu] LIKE '%{keyword}%' OR " +
                $"[No Kamar] LIKE '%{keyword}%' OR " +
                $"[Tipe Kamar] LIKE '%{keyword}%' OR " +
                $"[Metode Pembayaran] LIKE '%{keyword}%'";

            guna2DataGridView1.DataSource = dv;
        }

        // =======================================================
        // RESET
        // =======================================================
        private void guna2Reset_Click(object sender, EventArgs e)
        {
            guna2Cari.Text = "";
            guna2DariTanggal.Value = DateTime.Now.Date;
            guna2SampaiTanggal.Value = DateTime.Now.Date;

            guna2DataGridView1.DataSource = dataAsli;
        }

        // =======================================================
        // EXPORT EXCEL - UPDATED WITH DYNAMIC FILENAME & TOTAL
        // =======================================================
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
                save.FileName = $"Laporan_Transaksi_Owner_{dari}_sampai_{sampai}.xlsx";

                if (save.ShowDialog() != DialogResult.OK)
                    return;

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add(exportTable, "Laporan Transaksi Owner");

                    // TAMBAH TOTAL ROW DI EXCEL
                    int lastRow = ws.LastRowUsed().RowNumber();
                    ws.Cell(lastRow + 2, 7).Value = "TOTAL:";
                    ws.Cell(lastRow + 2, 7).Style.Font.Bold = true;

                    // Hitung total (sesuaikan kolom index)
                    decimal totalBayar = 0;
                    decimal totalUangMasuk = 0;
                    decimal totalKembalian = 0;

                    foreach (DataRow row in exportTable.Rows)
                    {
                        totalBayar += Convert.ToDecimal(row["Total Bayar"]);
                        totalUangMasuk += Convert.ToDecimal(row["Uang Masuk"]);
                        totalKembalian += Convert.ToDecimal(row["Kembalian"]);
                    }

                    ws.Cell(lastRow + 2, 8).Value = totalBayar; // kolom Total Bayar
                    ws.Cell(lastRow + 2, 9).Value = totalUangMasuk; // kolom Uang Masuk
                    ws.Cell(lastRow + 2, 10).Value = totalKembalian; // kolom Kembalian

                    ws.Cell(lastRow + 2, 8).Style.Font.Bold = true;
                    ws.Cell(lastRow + 2, 8).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(lastRow + 2, 9).Style.Font.Bold = true;
                    ws.Cell(lastRow + 2, 9).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(lastRow + 2, 10).Style.Font.Bold = true;
                    ws.Cell(lastRow + 2, 10).Style.NumberFormat.Format = "#,##0";

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

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //
        }

        private void guna2LaporanKeuangan_Click(object sender, EventArgs e)
        {

        }
    }
}
