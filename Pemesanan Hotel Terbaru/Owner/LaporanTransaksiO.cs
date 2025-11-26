using MySql.Data.MySqlClient;
using Pemesanan_Hotel_Terbaru.Admin;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Owner
{
    public partial class LaporanTransaksiO : Form
    {
        private DataTable dataAsli = new DataTable();

        public LaporanTransaksiO()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Navigasi Sidebar Owner
            guna2Dashboard.Click += (s, e) => PindahForm(new DashboardOwner());
            guna2DataKamar.Click += (s, e) => PindahForm(new DataKamarO());
            guna2DataReservasi.Click += (s, e) => PindahForm(new DataReservasiO());
            guna2DataTamu.Click += (s, e) => PindahForm(new DataTamuO());
            guna2DataUser.Click += (s, e) => PindahForm(new DataUserO());
            guna2LaporanKeuangan.Click += (s, e) => PindahForm(new LaporanKeuangan());
            guna2LaporanTransaksi.Click += (s, e) => { LoadLaporanTransaksiOwner(); }; // Refresh
            guna2Logout.Click += (s, e) => Logout();

            // 3. Event Lain
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2DariTanggal.ValueChanged += FilterTanggal;
            guna2SampaiTanggal.ValueChanged += FilterTanggal;
            guna2Reset.Click += guna2Reset_Click;
            guna2ExportExcel.Click += guna2ExportExcel_Click;

            this.Load += LaporanTransaksiO_Load;
        }

        // =======================================================
        // 🎨 TEMA ELEGANT
        // =======================================================
        private void ApplyElegantTheme()
        {
            // Background & Panel
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
            guna2Reset.FillColor = ColorTranslator.FromHtml("#C5A059");
            guna2Reset.ForeColor = Color.White;
            guna2ExportExcel.FillColor = ColorTranslator.FromHtml("#2C3E50");
            guna2ExportExcel.ForeColor = Color.White;

            // Reset Sidebar
            StyleSidebarButton(guna2Dashboard);
            StyleSidebarButton(guna2DataKamar);
            StyleSidebarButton(guna2DataReservasi);
            StyleSidebarButton(guna2DataTamu);
            StyleSidebarButton(guna2DataUser);
            StyleSidebarButton(guna2LaporanKeuangan);
            StyleSidebarButton(guna2LaporanTransaksi);
            StyleSidebarButton(guna2Logout);

            // Highlight Laporan Transaksi
            guna2LaporanTransaksi.FillColor = ColorTranslator.FromHtml("#E2E8F0");
        }

        private void StyleSidebarButton(Guna.UI2.WinForms.Guna2Button btn)
        {
            btn.FillColor = Color.Transparent;
            btn.CheckedState.FillColor = Color.Transparent;
            btn.ForeColor = ColorTranslator.FromHtml("#333333");
            btn.HoverState.FillColor = ColorTranslator.FromHtml("#E2E8F0");
            btn.HoverState.ForeColor = Color.Black;
        }

        // =======================================================
        // 🛠️ LOAD & DISPLAY (FIX URUTAN)
        // =======================================================
        private void LaporanTransaksiO_Load(object sender, EventArgs e)
        {
            LoadLaporanTransaksiOwner();
        }

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
                            tr.harga AS `Harga`,
                            tr.total_bayar AS `Total`,
                            tr.uang_masuk AS `Bayar`,
                            tr.kembalian AS `Kembali`,
                            tr.metode_pembayaran AS `Metode`,
                            tr.tanggal_transaksi AS `Tanggal`
                        FROM transaksi tr
                        JOIN reservasi r ON tr.id_reservasi = r.id_reservasi
                        JOIN kamar k ON r.id_kamar = k.id_kamar
                        ORDER BY tr.tanggal_transaksi DESC";

                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    dataAsli.Clear();
                    da.Fill(dataAsli);

                    DisplayData(dataAsli);
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal load: " + ex.Message); }
        }

        private void DisplayData(DataTable dt)
        {
            // 1. Tambah Kolom No Urut
            if (!dt.Columns.Contains("No")) dt.Columns.Add("No", typeof(int)).SetOrdinal(0);
            for (int i = 0; i < dt.Rows.Count; i++) dt.Rows[i]["No"] = i + 1;

            // 2. Tampilkan
            guna2DataGridView1.DataSource = dt;

            // 3. Fix Tampilan
            FixTableStyle();
        }

        private void FixTableStyle()
        {
            guna2DataGridView1.EnableHeadersVisualStyles = false;
            guna2DataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            guna2DataGridView1.AllowUserToAddRows = false;
            guna2DataGridView1.ReadOnly = true;

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
                if (col.Name == "Harga" || col.Name == "Total" || col.Name == "Bayar" || col.Name == "Kembali")
                {
                    col.DefaultCellStyle.Format = "N0";
                }
            }

            guna2DataGridView1.DefaultCellStyle.BackColor = Color.White;
            guna2DataGridView1.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2DataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F0E68C");
            guna2DataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;
            guna2DataGridView1.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FAFAFA");
            guna2DataGridView1.RowTemplate.Height = 50;
        }

        // =======================================================
        // FILTER & EXPORT
        // =======================================================
        private void FilterTanggal(object sender, EventArgs e)
        {
            if (dataAsli.Rows.Count == 0) return;

            DateTime dari = guna2DariTanggal.Value.Date;
            DateTime sampai = guna2SampaiTanggal.Value.Date.AddDays(1).AddSeconds(-1);

            DataView dv = new DataView(dataAsli);
            dv.RowFilter = $"[Tanggal] >= '{dari:yyyy-MM-dd HH:mm:ss}' AND [Tanggal] <= '{sampai:yyyy-MM-dd HH:mm:ss}'";
            DisplayData(dv.ToTable());
        }

        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            string keyword = guna2Cari.Text.Trim().Replace("'", "''");
            if (string.IsNullOrEmpty(keyword))
            {
                FilterTanggal(sender, e); return;
            }

            DataView dv = new DataView(dataAsli);
            dv.RowFilter = $"[Nama Tamu] LIKE '%{keyword}%' OR [No Kamar] LIKE '%{keyword}%' OR [Metode] LIKE '%{keyword}%'";
            DisplayData(dv.ToTable());
        }

        private void guna2Reset_Click(object sender, EventArgs e)
        {
            guna2Cari.Text = "";
            guna2DariTanggal.Value = DateTime.Now.Date;
            guna2SampaiTanggal.Value = DateTime.Now.Date;
            DisplayData(dataAsli);
        }

        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable exportTable = (guna2DataGridView1.DataSource as DataTable) ?? (guna2DataGridView1.DataSource as DataView)?.ToTable();
                if (exportTable == null || exportTable.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data!"); return;
                }

                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = $"Laporan_Transaksi_Owner_{DateTime.Now:ddMMyy}.xlsx";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    using (var wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(exportTable, "Transaksi");
                        int lastRow = ws.LastRowUsed().RowNumber();
                        ws.Cell(lastRow + 2, 7).Value = "TOTAL:";

                        decimal total = 0;
                        foreach (DataRow r in exportTable.Rows) total += Convert.ToDecimal(r["Total"]);

                        ws.Cell(lastRow + 2, 8).Value = total;
                        ws.Cell(lastRow + 2, 8).Style.NumberFormat.Format = "#,##0";

                        ws.Columns().AdjustToContents();
                        wb.SaveAs(save.FileName);
                    }
                    MessageBox.Show("Export Berhasil!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Export Gagal: " + ex.Message); }
        }

        // =======================================================
        // NAVIGASI
        // =======================================================
        private void PindahForm(Form targetForm)
        {
            targetForm.WindowState = FormWindowState.Maximized;
            targetForm.Show();
            this.Hide();
        }

        private void Logout()
        {
            if (MessageBox.Show("Yakin ingin logout?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Hide(); new Login().Show();
            }
        }

        // Event Kosong
        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void guna2LaporanKeuangan_Click(object sender, EventArgs e) { }
    }
}
