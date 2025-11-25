using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class LaporanTransaksiR : Form
    {
        private DataTable dataAsli = new DataTable(); // Simpan data awal

        public LaporanTransaksiR()
        {
            InitializeComponent();

            // Navigasi
            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardResepsionis());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarR());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuR());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiR());
            guna2Reservasi.Click += (s, e) => OpenForm(new Reservasi());
            guna2TransaksiPembayaran.Click += (s, e) => OpenForm(new TransaksiPembayaran());
            guna2Logout.Click += (s, e) => Logout();

            // EVENT PENCARIAN & FILTER
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2DariTanggal.ValueChanged += FilterTanggal;
            guna2SampaiTanggal.ValueChanged += FilterTanggal;
            guna2Reset.Click += guna2Reset_Click;
            guna2ExportExcel.Click += guna2ExportExcel_Click;

            this.Load += LaporanTransaksiR_Load;
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

        private void LaporanTransaksiR_Load(object sender, EventArgs e)
        {
            LoadLaporanTransaksi();
        }

        // ============================================================
        // LOAD DATA LAPORAN TRANSAKSI
        // ============================================================
        private void LoadLaporanTransaksi()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            tr.id_transaksi AS 'ID Transaksi',
                            tr.nama_tamu AS 'Nama Tamu',
                            k.tipe_kamar AS 'Tipe Kamar',
                            k.no_kamar AS 'No Kamar',
                            r.check_in AS 'Check-In',
                            r.check_out AS 'Check-Out',
                            tr.harga AS 'Harga per Malam',
                            tr.total_bayar AS 'Total Bayar',
                            tr.uang_masuk AS 'Uang Masuk',
                            tr.kembalian AS 'Kembalian',
                            tr.metode_pembayaran AS 'Metode Pembayaran',
                            tr.tanggal_transaksi AS 'Tanggal Transaksi'
                        FROM transaksi tr
                        JOIN reservasi r ON tr.id_reservasi = r.id_reservasi
                        JOIN kamar k ON r.id_kamar = k.id_kamar
                        ORDER BY tr.tanggal_transaksi DESC";

                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);

                    dataAsli.Clear();
                    da.Fill(dataAsli);

                    guna2DataGridView1.DataSource = dataAsli;

                    // format kolom harga
                    if (guna2DataGridView1.Columns.Count > 0)
                    {
                        guna2DataGridView1.Columns["Harga per Malam"].DefaultCellStyle.Format = "C0";
                        guna2DataGridView1.Columns["Total Bayar"].DefaultCellStyle.Format = "C0";
                        guna2DataGridView1.Columns["Uang Masuk"].DefaultCellStyle.Format = "C0";
                        guna2DataGridView1.Columns["Kembalian"].DefaultCellStyle.Format = "C0";
                        guna2DataGridView1.Columns["Tanggal Transaksi"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat laporan transaksi: " + ex.Message);
            }
        }

        // ============================================================
        // FILTER TANGGAL
        // ============================================================
        private void FilterTanggal(object sender, EventArgs e)
        {
            if (dataAsli.Rows.Count == 0) return;

            DateTime dari = guna2DariTanggal.Value.Date;
            DateTime sampai = guna2SampaiTanggal.Value.Date.AddDays(1).AddSeconds(-1);

            string dariStr = dari.ToString("yyyy-MM-dd HH:mm:ss");
            string sampaiStr = sampai.ToString("yyyy-MM-dd HH:mm:ss");

            DataView dv = new DataView(dataAsli);
            dv.RowFilter = $"[Tanggal Transaksi] >= '{dariStr}' AND [Tanggal Transaksi] <= '{sampaiStr}'";

            guna2DataGridView1.DataSource = dv;
        }

        // ============================================================
        // SEARCH / PENCARIAN
        // ============================================================
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

        // ============================================================
        // RESET FILTER
        // ============================================================
        private void guna2Reset_Click(object sender, EventArgs e)
        {
            guna2Cari.Text = "";
            guna2DariTanggal.Value = DateTime.Now.Date;
            guna2SampaiTanggal.Value = DateTime.Now.Date;

            guna2DataGridView1.DataSource = dataAsli;
        }

        // ============================================================
        // EXPORT EXCEL ClosedXML
        // ============================================================
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
                save.FileName = "Laporan_Transaksi_Resepsionis.xlsx";

                if (save.ShowDialog() != DialogResult.OK)
                    return;

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add(exportTable, "Laporan");
                    ws.Columns().AdjustToContents();
                    wb.SaveAs(save.FileName);
                } 

                MessageBox.Show("Export berhasil!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal export: " + ex.Message);
            }
        }

private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Kosongkan event ini, hanya untuk Designer
        }

        private void guna2Panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2DariTanggal_ValueChanged(object sender, EventArgs e)
        {

        }



        private void guna2SampaiTanggal_ValueChanged(object sender, EventArgs e)
        {

        }

    }
}

