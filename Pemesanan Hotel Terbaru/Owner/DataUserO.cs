using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClosedXML.Excel;
using Pemesanan_Hotel_Terbaru.Admin; // Butuh Admin utk Referensi Login
// using Pemesanan_Hotel_Terbaru.Owner; // Namespace diri sendiri tidak perlu using

namespace Pemesanan_Hotel_Terbaru.Owner
{
    public partial class DataUserO : Form
    {
        private DataTable dtUser;

        public DataUserO()
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
            guna2DataUser.Click += (s, e) => { LoadDataUser(); }; // Refresh
            guna2LaporanKeuangan.Click += (s, e) => PindahForm(new LaporanKeuangan());
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiO());
            guna2Logout.Click += (s, e) => Logout();

            // 3. Event Lain
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2ExportExcel.Click += guna2ExportExcel_Click;
            guna2Tambah.Click += guna2Tambah_Click;
            this.Load += DataUserO_Load;
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (KONSISTEN)
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

            // Label Judul
            foreach (Control c in Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2HtmlLabel) c.ForeColor = ColorTranslator.FromHtml("#333333");
            }

            // Tombol Aksi
            guna2Tambah.FillColor = ColorTranslator.FromHtml("#C5A059"); // Emas
            guna2Tambah.ForeColor = Color.White;
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

            // Highlight Data User
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
        // 🛠️ LOAD DATA (FIX URUTAN NOMOR)
        // ============================================================
        private void DataUserO_Load(object sender, EventArgs e)
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
                    string query = "SELECT id_user, username, email, password, role FROM user";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    dtUser = new DataTable();
                    adapter.Fill(dtUser);

                    DisplayData(dtUser);
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal load: " + ex.Message); }
        }

        private void DisplayData(DataTable dt)
        {
            // 1. Bersihkan
            guna2DataGridView1.DataSource = null;
            guna2DataGridView1.Rows.Clear();
            guna2DataGridView1.Columns.Clear();

            // 2. Kolom Manual
            AddTextColumn("colNo", "No", 50);
            AddTextColumn("colUser", "Username", 150);
            AddTextColumn("colEmail", "Email", 200);
            AddTextColumn("colPass", "Password", 100);
            AddTextColumn("colRole", "Role", 100);

            // ID Hidden
            AddTextColumn("colID", "ID", 0);
            guna2DataGridView1.Columns["colID"].Visible = false;

            // Tombol Aksi (Jika Owner boleh edit)
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
                    "*****", // Sensor password
                    row["role"],
                    row["id_user"],
                    "Edit", "Hapus"
                );
            }

            // 4. Fix Tampilan
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

        // ========================================================
        //  PENCARIAN
        // ========================================================
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            if (dtUser == null) return;
            string search = guna2Cari.Text.Trim();
            DataView dv = dtUser.DefaultView;

            if (!string.IsNullOrEmpty(search))
                dv.RowFilter = $"username LIKE '%{search}%' OR email LIKE '%{search}%' OR role LIKE '%{search}%'";
            else
                dv.RowFilter = "";

            DisplayData(dv.ToTable());
        }

        // ========================================================
        //  EXPORT
        // ========================================================
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel Files (*.xlsx)|*.xlsx";
                save.FileName = "Data_User_Owner.xlsx";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        DataTable dtExport = new DataTable("User");
                        dtExport.Columns.Add("No");
                        dtExport.Columns.Add("Username");
                        dtExport.Columns.Add("Email");
                        dtExport.Columns.Add("Role");

                        foreach (DataGridViewRow r in guna2DataGridView1.Rows)
                        {
                            dtExport.Rows.Add(r.Cells["colNo"].Value, r.Cells["colUser"].Value, r.Cells["colEmail"].Value, r.Cells["colRole"].Value);
                        }

                        var ws = wb.Worksheets.Add(dtExport);
                        ws.Columns().AdjustToContents();
                        wb.SaveAs(save.FileName);
                    }
                    MessageBox.Show("Export berhasil!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal: " + ex.Message); }
        }

        // ========================================================
        //  AKSI EDIT & DELETE
        // ========================================================
        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string colName = guna2DataGridView1.Columns[e.ColumnIndex].Name;
            string id = guna2DataGridView1.Rows[e.RowIndex].Cells["colID"].Value.ToString();

            if (colName == "colEdit")
            {
                // Pastikan form EditUserO ada dan constructornya support string ID
                // this.Hide(); // Jangan di-hide biar ga kedip
                new EditUserO(id).ShowDialog();
                LoadDataUser();
            }
            else if (colName == "colDelete")
            {
                if (MessageBox.Show("Hapus user ini?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = Koneksi.GetConnection())
                        {
                            conn.Open();
                            new MySqlCommand($"DELETE FROM user WHERE id_user='{id}'", conn).ExecuteNonQuery();
                        }
                        LoadDataUser();
                    }
                    catch (Exception ex) { MessageBox.Show("Gagal: " + ex.Message); }
                }
            }
        }

        // ========================================================
        //  NAVIGASI & TAMBAH
        // ========================================================
        private void guna2Tambah_Click(object sender, EventArgs e)
        {
            // Pastikan form TambahUserO ada
            new TambahUserO().ShowDialog();
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
