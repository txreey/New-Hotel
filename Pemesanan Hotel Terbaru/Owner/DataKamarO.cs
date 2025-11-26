using Pemesanan_Hotel_Terbaru.Admin; // Butuh referensi form Admin (untuk EditKamar jika perlu)
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Owner
{
    public partial class DataKamarO : Form
    {
        private DataTable dataKamarTable;

        public DataKamarO()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Navigasi Sidebar Owner
            guna2Dashboard.Click += (s, e) => PindahForm(new DashboardOwner());
            guna2DataKamar.Click += (s, e) => { LoadDataKamarOwner(); }; // Refresh
            guna2DataReservasi.Click += (s, e) => PindahForm(new DataReservasiO());
            guna2DataTamu.Click += (s, e) => PindahForm(new DataTamuO());
            guna2DataUser.Click += (s, e) => PindahForm(new DataUserO());
            guna2LaporanKeuangan.Click += (s, e) => PindahForm(new LaporanKeuangan());
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiO());
            guna2Logout.Click += (s, e) => Logout();

            // 3. Event Lain
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2ExportExcel.Click += guna2ExportExcel_Click;
            this.Load += DataKamarO_Load;
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (SAMA SEPERTI ADMIN)
        // ============================================================
        private void ApplyElegantTheme()
        {
            // Background Utama
            this.BackColor = ColorTranslator.FromHtml("#F4F6F8");

            // Sidebar & Header
            guna2Panel1.FillColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel1.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel5.FillColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel5.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2PictureBox1.BackColor = Color.Transparent;

            // Label Gelap
            foreach (Control c in Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2HtmlLabel) c.ForeColor = ColorTranslator.FromHtml("#333333");
            }

            // Jika ada label "Owner" (misal guna2HtmlLabel8), ubah juga
            // guna2HtmlLabel8.ForeColor = ColorTranslator.FromHtml("#333333");

            // Tombol Aksi (Export saja)
            guna2ExportExcel.FillColor = ColorTranslator.FromHtml("#2C3E50"); // Abu Gelap
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

            // Highlight Data Kamar (Aktif)
            guna2DataKamar.FillColor = ColorTranslator.FromHtml("#E2E8F0");
        }

        private void StyleSidebarButton(Guna.UI2.WinForms.Guna2Button btn)
        {
            btn.FillColor = Color.Transparent;
            btn.CheckedState.FillColor = Color.Transparent;
            btn.ForeColor = ColorTranslator.FromHtml("#333333");
            btn.HoverState.FillColor = ColorTranslator.FromHtml("#E2E8F0");
            btn.HoverState.ForeColor = Color.Black;
        }

        // ============================================================
        // 🛠️ LOAD & DISPLAY (FIX URUTAN)
        // ============================================================
        private void DataKamarO_Load(object sender, EventArgs e)
        {
            LoadDataKamarOwner();
        }

        private void LoadDataKamarOwner()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM kamar";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    dataKamarTable = new DataTable();
                    adapter.Fill(dataKamarTable);
                    DisplayData(dataKamarTable);
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal load data: " + ex.Message); }
        }

        private void DisplayData(DataTable dt)
        {
            guna2DataGridView1.Rows.Clear();
            guna2DataGridView1.Columns.Clear();

            // 1. Kolom Nomor Urut
            AddTextColumn("colNo", "No", 50);

            // 2. Kolom Data
            AddTextColumn("colTipe", "Tipe Kamar", 150);
            AddTextColumn("colNoKamar", "No Kamar", 100);
            AddTextColumn("colStatus", "Status", 100);
            AddTextColumn("colHarga", "Harga", 120);
            AddTextColumn("colDeskripsi", "Deskripsi", 200);

            // 3. Kolom Gambar
            DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
            imgCol.Name = "colPicture";
            imgCol.HeaderText = "Gambar";
            imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
            guna2DataGridView1.Columns.Add(imgCol);

            // 4. ID Database (Hidden)
            AddTextColumn("colID", "ID", 0);
            guna2DataGridView1.Columns["colID"].Visible = false;

            // OWNER BIASANYA TIDAK EDIT/HAPUS, TAPI JIKA PERLU:
            AddButtonColumn("colEdit", "Edit");
            AddButtonColumn("colDelete", "Hapus");

            guna2DataGridView1.AllowUserToAddRows = false;

            // 5. Isi Data
            int nomor = 1;
            foreach (DataRow row in dt.Rows)
            {
                Image gambar = null;
                string path = row["picture"]?.ToString();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try { using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) { gambar = Image.FromStream(fs); } } catch { }
                }

                guna2DataGridView1.Rows.Add(
                    nomor++,
                    row["tipe_kamar"],
                    row["no_kamar"],
                    row["status"],
                    row["harga"],
                    row["deskripsi"],
                    gambar,
                    row["id_kamar"], // ID Hidden
                    "Edit", "Hapus"
                );
            }

            // 6. Fix Tampilan Tabel
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
            guna2DataGridView1.RowTemplate.Height = 70;
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

        // ============================================================
        // PENCARIAN & EXPORT
        // ============================================================
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            string keyword = guna2Cari.Text.Trim();
            if (string.IsNullOrEmpty(keyword)) DisplayData(dataKamarTable);
            else
            {
                DataView dv = dataKamarTable.DefaultView;
                dv.RowFilter = $"tipe_kamar LIKE '%{keyword}%' OR no_kamar LIKE '%{keyword}%'";
                DisplayData(dv.ToTable());
            }
        }

        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Kamar_Owner.xlsx";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        // Buat DataTable bersih untuk Excel
                        DataTable dtExcel = new DataTable("Kamar");
                        dtExcel.Columns.Add("No");
                        dtExcel.Columns.Add("Tipe");
                        dtExcel.Columns.Add("Nomor");
                        dtExcel.Columns.Add("Status");
                        dtExcel.Columns.Add("Harga");
                        dtExcel.Columns.Add("Deskripsi");

                        foreach (DataGridViewRow r in guna2DataGridView1.Rows)
                        {
                            dtExcel.Rows.Add(
                                r.Cells["colNo"].Value, r.Cells["colTipe"].Value, r.Cells["colNoKamar"].Value,
                                r.Cells["colStatus"].Value, r.Cells["colHarga"].Value, r.Cells["colDeskripsi"].Value
                            );
                        }

                        wb.Worksheets.Add(dtExcel);
                        wb.SaveAs(save.FileName);
                    }
                    MessageBox.Show("Export Berhasil!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal: " + ex.Message); }
        }

        // ============================================================
        // NAVIGASI & AKSI
        // ============================================================
        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string colName = guna2DataGridView1.Columns[e.ColumnIndex].Name;
            string id = guna2DataGridView1.Rows[e.RowIndex].Cells["colID"].Value.ToString();

            if (colName == "colEdit")
            {
                // Owner boleh edit? Jika ya:
                this.Hide();
                new EditKamar(id).ShowDialog();
                this.Show();
                LoadDataKamarOwner();
            }
            else if (colName == "colDelete")
            {
                if (MessageBox.Show("Apakah Anda Ingin Menghapus Data Ini?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = Koneksi.GetConnection())
                        {
                            conn.Open();
                            new MySqlCommand($"DELETE FROM kamar WHERE id_kamar='{id}'", conn).ExecuteNonQuery();
                        }
                        LoadDataKamarOwner();
                    }
                    catch { }
                }
            }
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
    }
}
