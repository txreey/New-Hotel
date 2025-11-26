using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class LaporanTransaksiR : Form
    {
        private DataTable dataAsli = new DataTable();

        public LaporanTransaksiR()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Navigasi Sidebar
            guna2Dashboard.Click += (s, e) => PindahForm(new DashboardResepsionis());
            guna2DataKamar.Click += (s, e) => PindahForm(new DataKamarR());
            guna2DataTamu.Click += (s, e) => PindahForm(new DataTamuR());
            guna2LaporanTransaksi.Click += (s, e) => { LoadLaporanTransaksi(); }; // Refresh
            guna2Reservasi.Click += (s, e) => PindahForm(new Reservasi());
            guna2TransaksiPembayaran.Click += (s, e) => PindahForm(new TransaksiPembayaran());
            guna2Logout.Click += (s, e) => Logout();

            // 3. Event Lain
            // Note: Event handler untuk tanggal & cari didefinisikan di bawah sesuai nama Designer
            guna2Reset.Click += guna2Reset_Click;
            guna2ExportExcel.Click += guna2ExportExcel_Click;

            this.Load += LaporanTransaksiR_Load;
        }

        // ============================================================
        // 🎨 TEMA ELEGANT
        // ============================================================
        private void ApplyElegantTheme()
        {
            this.BackColor = ColorTranslator.FromHtml("#F4F6F8");

            if (guna2Panel1 != null)
            {
                guna2Panel1.FillColor = ColorTranslator.FromHtml("#F9F7F2");
                guna2Panel1.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            }
            if (guna2Panel5 != null)
            {
                guna2Panel5.FillColor = ColorTranslator.FromHtml("#F9F7F2");
                guna2Panel5.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            }
            if (guna2PictureBox1 != null) guna2PictureBox1.BackColor = Color.Transparent;

            foreach (Control c in Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2HtmlLabel) c.ForeColor = ColorTranslator.FromHtml("#333333");
            }

            guna2Reset.FillColor = ColorTranslator.FromHtml("#C5A059");
            guna2Reset.ForeColor = Color.White;
            guna2ExportExcel.FillColor = ColorTranslator.FromHtml("#2C3E50");
            guna2ExportExcel.ForeColor = Color.White;

            StyleSidebarButton(guna2Dashboard);
            StyleSidebarButton(guna2DataKamar);
            StyleSidebarButton(guna2DataTamu);
            StyleSidebarButton(guna2LaporanTransaksi);
            StyleSidebarButton(guna2Reservasi);
            StyleSidebarButton(guna2TransaksiPembayaran);
            StyleSidebarButton(guna2Logout);

            guna2LaporanTransaksi.FillColor = ColorTranslator.FromHtml("#E2E8F0");
        }

        private void StyleSidebarButton(Guna.UI2.WinForms.Guna2Button btn)
        {
            if (btn == null) return;
            btn.FillColor = Color.Transparent;
            btn.CheckedState.FillColor = Color.Transparent;
            btn.ForeColor = ColorTranslator.FromHtml("#333333");
            btn.HoverState.FillColor = ColorTranslator.FromHtml("#E2E8F0");
            btn.HoverState.ForeColor = Color.Black;
        }

        // ============================================================
        // 🛠️ LOAD & DISPLAY
        // ============================================================
        private void LaporanTransaksiR_Load(object sender, EventArgs e)
        {
            LoadLaporanTransaksi();
        }

        private void LoadLaporanTransaksi()
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
            if (!dt.Columns.Contains("No")) dt.Columns.Add("No", typeof(int)).SetOrdinal(0);
            for (int i = 0; i < dt.Rows.Count; i++) dt.Rows[i]["No"] = i + 1;

            guna2DataGridView1.DataSource = dt;
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

        // ============================================================
        // 🔥 METHOD FILTER (NAMA DISESUAIKAN UTK DESIGNER)
        // ============================================================

        // Method utama untuk filtering
        private void RunFilter()
        {
            if (dataAsli.Rows.Count == 0) return;

            string keyword = guna2Cari.Text.Trim().Replace("'", "''");
            DateTime dari = guna2DariTanggal.Value.Date;
            DateTime sampai = guna2SampaiTanggal.Value.Date.AddDays(1).AddSeconds(-1);

            DataView dv = new DataView(dataAsli);

            // Filter Tanggal
            string filter = $"[Tanggal] >= '{dari:yyyy-MM-dd HH:mm:ss}' AND [Tanggal] <= '{sampai:yyyy-MM-dd HH:mm:ss}'";

            // Filter Keyword (jika ada)
            if (!string.IsNullOrEmpty(keyword))
            {
                filter += $" AND ([Nama Tamu] LIKE '%{keyword}%' OR [No Kamar] LIKE '%{keyword}%' OR [Metode] LIKE '%{keyword}%')";
            }

            dv.RowFilter = filter;
            DisplayData(dv.ToTable());
        }

        // Event Handler yang dicari oleh Designer
        private void guna2DariTanggal_ValueChanged(object sender, EventArgs e)
        {
            RunFilter();
        }

        private void guna2SampaiTanggal_ValueChanged(object sender, EventArgs e)
        {
            RunFilter();
        }

        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            RunFilter();
        }

        private void guna2Reset_Click(object sender, EventArgs e)
        {
            guna2Cari.Text = "";
            guna2DariTanggal.Value = DateTime.Now.Date;
            guna2SampaiTanggal.Value = DateTime.Now.Date;
            DisplayData(dataAsli);
        }

        // ============================================================
        // EXPORT & NAVIGASI
        // ============================================================
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
                save.FileName = $"Laporan_Transaksi_Resepsionis_{DateTime.Now:ddMMyy}.xlsx";

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

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void guna2Panel5_Paint(object sender, PaintEventArgs e) { }
    }
}
