using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ClosedXML.Excel;
using System.IO;

namespace Pemesanan_Hotel_Terbaru.Admin
{
    public partial class DataUser : Form
    {
        private DataTable dtUser;

        public DataUser()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Navigasi Sidebar (Pake logika PindahForm)
            guna2Dashboard.Click += (s, e) => PindahForm(new DashboardAdmin());
            guna2DataKamar.Click += (s, e) => PindahForm(new DataKamarA());
            guna2DataTamu.Click += (s, e) => PindahForm(new DataTamuA());
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiA());
            guna2DataReservasi.Click += (s, e) => PindahForm(new DataReservasi());
            guna2DataUser.Click += (s, e) => { LoadDataUser(); }; // Refresh diri sendiri
            guna2LaporanKeuangan.Click += (s, e) => PindahForm(new LaporanKeuangan2());
            guna2Logout.Click += (s, e) => Logout();

            // 3. Event Lain
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2ExportExcel.Click += guna2ExportExcel_Click;
            guna2Button1.Click += guna2Button1_Click; // Tombol Tambah

            this.Load += DataUser_Load;
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (KONSISTEN TOTAL)
        // ============================================================
        private void ApplyElegantTheme()
        {
            // Background & Panel
            this.BackColor = ColorTranslator.FromHtml("#F4F6F8");
            guna2Panel1.FillColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel1.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel5.FillColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel5.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2PictureBox1.BackColor = Color.Transparent;

            // Label Judul (Gelap)
            foreach (Control c in Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2HtmlLabel) c.ForeColor = ColorTranslator.FromHtml("#333333");
            }
            guna2HtmlLabel8.ForeColor = ColorTranslator.FromHtml("#333333");

            // Tombol Aksi 
            // Tambah User = Emas
            guna2Button1.FillColor = ColorTranslator.FromHtml("#C5A059");
            guna2Button1.ForeColor = Color.White;

            // Export = Abu Gelap
            guna2ExportExcel.FillColor = ColorTranslator.FromHtml("#2C3E50");
            guna2ExportExcel.ForeColor = Color.White;

            // Reset Sidebar
            StyleSidebarButton(guna2Dashboard);
            StyleSidebarButton(guna2DataKamar);
            StyleSidebarButton(guna2DataTamu);
            StyleSidebarButton(guna2LaporanTransaksi);
            StyleSidebarButton(guna2DataReservasi);
            StyleSidebarButton(guna2DataUser);
            StyleSidebarButton(guna2LaporanKeuangan);
            StyleSidebarButton(guna2Logout);

            // Highlight Data User (Aktif)
            guna2DataUser.FillColor = ColorTranslator.FromHtml("#E2E8F0");
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
        // 🛠️ LOAD DATA (FIX NOMOR URUT)
        // ============================================================
        private void DataUser_Load(object sender, EventArgs e)
        {
            LoadDataUser();
        }

        private void LoadDataUser()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    // Ambil semua kolom penting
                    string query = "SELECT id_user, username, email, password, role FROM user";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    dtUser = new DataTable();
                    adapter.Fill(dtUser);

                    DisplayData(dtUser);
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal: " + ex.Message); }
        }

        private void DisplayData(DataTable dt)
        {
            // 1. Bersihkan Tabel
            guna2DataGridView1.DataSource = null;
            guna2DataGridView1.Rows.Clear();
            guna2DataGridView1.Columns.Clear();

            // 2. Buat Kolom Manual
            AddTextColumn("colNo", "No", 50);
            AddTextColumn("colUser", "Username", 150);
            AddTextColumn("colEmail", "Email", 200);
            AddTextColumn("colPass", "Password", 100); // Bisa disensor nanti kalau mau
            AddTextColumn("colRole", "Role", 100);

            // ID Database (Disembunyikan)
            AddTextColumn("colID", "ID", 0);
            guna2DataGridView1.Columns["colID"].Visible = false;

            // Kolom Tombol
            AddButtonColumn("colEdit", "Edit");
            AddButtonColumn("colDelete", "Hapus");

            // 3. Isi Data
            int nomor = 1;
            foreach (DataRow row in dt.Rows)
            {
                guna2DataGridView1.Rows.Add(
                    nomor++,
                    row["username"],
                    row["email"],
                    "*****", // Sensor Password biar aman
                    row["role"],
                    row["id_user"], // ID tersimpan tapi ga kelihatan
                    "Edit", "Hapus"
                );
            }

            // 4. Fix Tampilan Header (Emas)
            FixTableStyle();
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

        private void FixTableStyle()
        {
            guna2DataGridView1.EnableHeadersVisualStyles = false;
            guna2DataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Header Emas
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

            // Isi Tabel
            guna2DataGridView1.DefaultCellStyle.BackColor = Color.White;
            guna2DataGridView1.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2DataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F0E68C");
            guna2DataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;
            guna2DataGridView1.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FAFAFA");
            guna2DataGridView1.RowTemplate.Height = 50;
            guna2DataGridView1.AllowUserToAddRows = false;
        }

        // =======================================================
        // 🔍 PENCARIAN
        // =======================================================
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            if (dtUser == null) return;
            string search = guna2Cari.Text.Trim();

            DataView dv = dtUser.DefaultView;
            if (!string.IsNullOrEmpty(search))
            {
                dv.RowFilter = $"username LIKE '%{search}%' OR email LIKE '%{search}%' OR role LIKE '%{search}%'";
            }
            else
            {
                dv.RowFilter = "";
            }

            DisplayData(dv.ToTable());
        }

        // =======================================================
        // AKSI TOMBOL (Edit & Hapus)
        // =======================================================
        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = guna2DataGridView1.Columns[e.ColumnIndex].Name;
            string idUser = guna2DataGridView1.Rows[e.RowIndex].Cells["colID"].Value.ToString();
            string username = guna2DataGridView1.Rows[e.RowIndex].Cells["colUser"].Value.ToString();

            if (colName == "colEdit")
            {
                // Pastikan punya form EditUser(string id)
                EditUser editForm = new EditUser(idUser);
                editForm.ShowDialog();
                LoadDataUser();
            }
            else if (colName == "colDelete")
            {
                DialogResult result = MessageBox.Show(
                    $"Hapus user '{username}'?", "Konfirmasi",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = Koneksi.GetConnection())
                        {
                            conn.Open();
                            new MySqlCommand($"DELETE FROM user WHERE id_user='{idUser}'", conn).ExecuteNonQuery();
                        }
                        MessageBox.Show("Terhapus.");
                        LoadDataUser();
                    }
                    catch (Exception ex) { MessageBox.Show("Gagal: " + ex.Message); }
                }
            }
        }

        // =======================================================
        // EXPORT & TAMBAH
        // =======================================================
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_User.xlsx";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    // Buat DataTable Bersih buat Excel
                    DataTable dtExport = new DataTable("Users");
                    dtExport.Columns.Add("No");
                    dtExport.Columns.Add("Username");
                    dtExport.Columns.Add("Email");
                    dtExport.Columns.Add("Role");

                    foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                    {
                        dtExport.Rows.Add(
                            row.Cells["colNo"].Value,
                            row.Cells["colUser"].Value,
                            row.Cells["colEmail"].Value,
                            row.Cells["colRole"].Value
                        );
                    }

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(dtExport);
                        ws.Columns().AdjustToContents();
                        wb.SaveAs(save.FileName);
                    }
                    MessageBox.Show("Export berhasil!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal: " + ex.Message); }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            TambahUser tambahUserForm = new TambahUser();
            this.Hide();
            tambahUserForm.ShowDialog();
            this.Show(); // Balik lagi ke sini setelah tambah
            LoadDataUser();
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
