using MySql.Data.MySqlClient;
using Pemesanan_Hotel_Terbaru.Admin;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Owner
{
    public partial class LaporanKeuangan : Form
    {
        private DataTable dataAsli = new DataTable();

        public LaporanKeuangan()
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
            guna2LaporanKeuangan.Click += (s, e) => { LoadDataKeuangan(); }; // Refresh
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiO());
            guna2Logout.Click += (s, e) => Logout();

            // 3. Event Lain
            this.Load += LaporanKeuangan_Load;
            guna2DariTanggal.ValueChanged += FilterTanggal;
            guna2SampaiTanggal.ValueChanged += FilterTanggal;
            guna2Reset.Click += guna2Reset_Click;
            guna2ExportExcel.Click += guna2ExportExcel_Click;
        }

        // =======================================================
        // 🎨 TEMA ELEGANT (KONSISTEN OWNER)
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

            // Label Total Pendapatan (Jika ada)
            if (guna2TotalPendapatan != null)
                guna2TotalPendapatan.ForeColor = ColorTranslator.FromHtml("#333333");

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

            // Highlight Laporan Keuangan
            guna2LaporanKeuangan.FillColor = ColorTranslator.FromHtml("#E2E8F0");
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
        private void LaporanKeuangan_Load(object sender, EventArgs e)
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

                    DisplayData(dataAsli);
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal load: " + ex.Message); }
        }

        private void DisplayData(DataTable dt)
        {
            // 1. Hitung Saldo & No Urut
            HitungSaldo(dt);

            if (!dt.Columns.Contains("No"))
                dt.Columns.Add("No", typeof(int)).SetOrdinal(0);

            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["No"] = i + 1;

            // 2. Tampilkan
            guna2DataGridView1.DataSource = dt;

            // 3. Update Total & Fix Style
            UpdateTotalPendapatan(dt);
            FixTableStyle();
        }

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

        private void UpdateTotalPendapatan(DataTable dt)
        {
            decimal total = 0;
            foreach (DataRow row in dt.Rows)
                total += Convert.ToDecimal(row["Kredit"]);

            if (guna2TotalPendapatan != null)
                guna2TotalPendapatan.Text = "Rp " + total.ToString("N0");
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
                if (col.Name == "Kredit" || col.Name == "Saldo") col.DefaultCellStyle.Format = "N0";
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
            DataView dv = new DataView(dataAsli);
            dv.RowFilter = $"Tanggal >= '{guna2DariTanggal.Value:yyyy-MM-dd}' AND Tanggal <= '{guna2SampaiTanggal.Value:yyyy-MM-dd}'";
            DisplayData(dv.ToTable());
        }

        private void guna2Reset_Click(object sender, EventArgs e)
        {
            guna2DariTanggal.Value = DateTime.Now.AddMonths(-1);
            guna2SampaiTanggal.Value = DateTime.Now;
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
                save.FileName = $"Laporan_Keuangan_Owner_{DateTime.Now:ddMMyy}.xlsx";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    using (var wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(exportTable, "Keuangan");
                        int lastRow = ws.LastRowUsed().RowNumber();
                        ws.Cell(lastRow + 2, 3).Value = "TOTAL PENDAPATAN:";
                        ws.Cell(lastRow + 2, 4).FormulaA1 = $"=SUM(D2:D{lastRow})";
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
            if (MessageBox.Show("Logout?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Hide(); new Login().Show();
            }
        }

        // Event Kosong
        private void guna2DariTanggal_ValueChanged(object sender, EventArgs e) { } // Event cadangan
        private void guna2SampaiTanggal_ValueChanged(object sender, EventHandler e) { }
        private void guna2TotalPendapatan_Click(object sender, EventArgs e) { }
    }
}
