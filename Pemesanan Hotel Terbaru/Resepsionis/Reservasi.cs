using MySql.Data.MySqlClient;
using System.Data;
using System;
using System.Drawing;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class Reservasi : Form
    {
        private DataTable dtReservasi;

        public Reservasi()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Navigasi Sidebar
            guna2Dashboard.Click += (s, e) => PindahForm(new DashboardResepsionis());
            guna2DataKamar.Click += (s, e) => PindahForm(new DataKamarR());
            guna2DataTamu.Click += (s, e) => PindahForm(new DataTamuR());
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiR());
            guna2Reservasi.Click += (s, e) => { LoadDataReservasi(); }; // Refresh
            guna2TransaksiPembayaran.Click += (s, e) => PindahForm(new TransaksiPembayaran());
            guna2Logout.Click += (s, e) => Logout();

            // 3. Event Lain (Manual Assign biar aman)
            // Jika di designer sudah ada +=, baris ini opsional tapi aman dibiarkan
            guna2Tambah.Click += guna2Tambah_Click;
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2Reset.Click += guna2Reset_Click;
            guna2ExportExcel.Click += guna2ExportExcel_Click;

            this.Load += Reservasi_Load;
        }

        // ============================================================
        // 🎨 TEMA ELEGANT
        // ============================================================
        private void ApplyElegantTheme()
        {
            this.BackColor = ColorTranslator.FromHtml("#F4F6F8");

            // Cek null agar tidak error
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

            guna2Tambah.FillColor = ColorTranslator.FromHtml("#C5A059");
            guna2Tambah.ForeColor = Color.White;
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

            guna2Reservasi.FillColor = ColorTranslator.FromHtml("#E2E8F0");
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
        // 🛠️ LOAD DATA
        // ============================================================
        private void Reservasi_Load(object sender, EventArgs e)
        {
            LoadDataReservasi();
        }

        private void LoadDataReservasi()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            r.id_reservasi,
                            t.nama_tamu,
                            k.tipe_kamar,
                            k.no_kamar,
                            r.check_in,
                            r.check_out,
                            r.status_pembayaran
                        FROM reservasi r
                        JOIN tamu t ON r.id_tamu = t.id_tamu
                        JOIN kamar k ON r.id_kamar = k.id_kamar
                        ORDER BY r.check_in DESC";

                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    dtReservasi = new DataTable();
                    da.Fill(dtReservasi);

                    DisplayData(dtReservasi);
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal memuat data: " + ex.Message); }
        }

        private void DisplayData(DataTable dt)
        {
            guna2DataGridView1.DataSource = null;
            guna2DataGridView1.Rows.Clear();
            guna2DataGridView1.Columns.Clear();

            AddTextColumn("colNo", "No", 50);
            AddTextColumn("colTamu", "Nama Tamu", 150);
            AddTextColumn("colTipe", "Tipe Kamar", 120);
            AddTextColumn("colNoKamar", "No Kamar", 80);
            AddTextColumn("colIn", "Check In", 120);
            AddTextColumn("colOut", "Check Out", 120);
            AddTextColumn("colStatus", "Status", 120);

            AddTextColumn("colID", "ID", 0);
            guna2DataGridView1.Columns["colID"].Visible = false;

            AddButtonColumn("colEdit", "Edit");
            AddButtonColumn("colDelete", "Hapus");

            guna2DataGridView1.AllowUserToAddRows = false;

            int nomor = 1;
            foreach (DataRow row in dt.Rows)
            {
                string tglIn = Convert.ToDateTime(row["check_in"]).ToString("dd MMM yyyy");
                string tglOut = Convert.ToDateTime(row["check_out"]).ToString("dd MMM yyyy");
                string status = row["status_pembayaran"].ToString();

                int idx = guna2DataGridView1.Rows.Add(
                    nomor++,
                    row["nama_tamu"],
                    row["tipe_kamar"],
                    row["no_kamar"],
                    tglIn, tglOut,
                    status,
                    row["id_reservasi"],
                    "Edit", "Hapus"
                );

                if (status.ToLower() == "sudah bayar")
                {
                    var cellEdit = (DataGridViewButtonCell)guna2DataGridView1.Rows[idx].Cells["colEdit"];
                    var cellDel = (DataGridViewButtonCell)guna2DataGridView1.Rows[idx].Cells["colDelete"];

                    cellEdit.FlatStyle = FlatStyle.Flat;
                    cellEdit.Style.ForeColor = Color.Gray;
                    cellEdit.Style.SelectionForeColor = Color.Gray;

                    cellDel.FlatStyle = FlatStyle.Flat;
                    cellDel.Style.ForeColor = Color.Gray;
                    cellDel.Style.SelectionForeColor = Color.Gray;
                }
            }
            FixTableStyle();
        }

        private void FixTableStyle()
        {
            guna2DataGridView1.EnableHeadersVisualStyles = false;
            guna2DataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

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
            }

            guna2DataGridView1.DefaultCellStyle.BackColor = Color.White;
            guna2DataGridView1.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2DataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F0E68C");
            guna2DataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;
            guna2DataGridView1.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FAFAFA");
            guna2DataGridView1.RowTemplate.Height = 50;
        }

        private void AddTextColumn(string name, string header, int width)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.Name = name;
            col.HeaderText = header;
            col.Width = width;
            guna2DataGridView1.Columns.Add(col);
        }

        private void AddButtonColumn(string name, string text)
        {
            DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
            btn.Name = name;
            btn.HeaderText = text;
            btn.Text = text;
            btn.UseColumnTextForButtonValue = true;
            btn.FlatStyle = FlatStyle.Flat;
            btn.DefaultCellStyle.BackColor = Color.WhiteSmoke;
            btn.DefaultCellStyle.ForeColor = Color.Black;
            guna2DataGridView1.Columns.Add(btn);
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string status = guna2DataGridView1.Rows[e.RowIndex].Cells["colStatus"].Value.ToString().ToLower();
            if (status == "sudah bayar") return;

            string idReservasi = guna2DataGridView1.Rows[e.RowIndex].Cells["colID"].Value.ToString();
            string colName = guna2DataGridView1.Columns[e.ColumnIndex].Name;

            if (colName == "colEdit")
            {
                new EditReservasi(idReservasi).ShowDialog();
                LoadDataReservasi();
            }
            else if (colName == "colDelete")
            {
                if (MessageBox.Show("Hapus reservasi ini?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = Koneksi.GetConnection())
                        {
                            conn.Open();
                            string cek = "SELECT COUNT(*) FROM transaksi WHERE id_reservasi = @id";
                            MySqlCommand cmdCek = new MySqlCommand(cek, conn);
                            cmdCek.Parameters.AddWithValue("@id", idReservasi);
                            if (Convert.ToInt32(cmdCek.ExecuteScalar()) > 0)
                            {
                                MessageBox.Show("Tidak bisa hapus karena sudah ada transaksi."); return;
                            }
                            new MySqlCommand($"DELETE FROM reservasi WHERE id_reservasi='{idReservasi}'", conn).ExecuteNonQuery();
                        }
                        LoadDataReservasi();
                    }
                    catch (Exception ex) { MessageBox.Show("Gagal: " + ex.Message); }
                }
            }
        }

        // ============================================================
        // 🔥 NAMA METHOD DISESUAIKAN DENGAN DESIGNER
        // ============================================================

        private void guna2DariTanggal_ValueChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private void guna2SampaiTanggal_ValueChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private void FilterData()
        {
            if (dtReservasi == null) return;

            string search = guna2Cari.Text.Trim();
            string dari = guna2DariTanggal.Value.ToString("yyyy-MM-dd");
            string sampai = guna2SampaiTanggal.Value.ToString("yyyy-MM-dd");

            DataView dv = dtReservasi.DefaultView;
            string filter = $"nama_tamu LIKE '%{search}%' AND check_in >= '{dari}' AND check_out <= '{sampai}'";

            dv.RowFilter = filter;
            DisplayData(dv.ToTable());
        }

        private void guna2Reset_Click(object sender, EventArgs e)
        {
            guna2Cari.Text = "";
            guna2DariTanggal.Value = DateTime.Now.AddMonths(-1);
            guna2SampaiTanggal.Value = DateTime.Now.AddMonths(1);
            DisplayData(dtReservasi);
        }

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
                        DataTable dtExport = new DataTable("Reservasi");
                        dtExport.Columns.Add("No");
                        dtExport.Columns.Add("Nama Tamu");
                        dtExport.Columns.Add("Tipe Kamar");
                        dtExport.Columns.Add("No Kamar");
                        dtExport.Columns.Add("Check In");
                        dtExport.Columns.Add("Check Out");
                        dtExport.Columns.Add("Status");

                        foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                        {
                            dtExport.Rows.Add(
                                row.Cells["colNo"].Value, row.Cells["colTamu"].Value,
                                row.Cells["colTipe"].Value, row.Cells["colNoKamar"].Value,
                                row.Cells["colIn"].Value, row.Cells["colOut"].Value,
                                row.Cells["colStatus"].Value
                            );
                        }

                        var ws = wb.Worksheets.Add(dtExport);
                        ws.Columns().AdjustToContents();
                        wb.SaveAs(save.FileName);
                    }
                    MessageBox.Show("Export berhasil!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal export: " + ex.Message); }
        }

        private void guna2Tambah_Click(object sender, EventArgs e)
        {
            new TambahReservasi().ShowDialog();
            LoadDataReservasi();
        }

        // NAVIGASI
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
    }
}
