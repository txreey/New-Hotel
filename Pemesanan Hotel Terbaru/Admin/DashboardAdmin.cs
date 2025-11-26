using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Pemesanan_Hotel_Terbaru.Admin
{
    public partial class DashboardAdmin : Form
    {
        private DataTable dtAktivitas = new DataTable();

        public DashboardAdmin()
        {
            InitializeComponent();

            // KUNCI: Paksa Full Screen di Constructor
            this.WindowState = FormWindowState.Maximized;

            // Terapkan Tema Elegant
            ApplyElegantTheme();

            // === PERBAIKAN LOGIKA TOMBOL SIDEBAR ===
            // Tombol Dashboard cuma REFRESH, jangan buka form baru
            guna2Dashboard.Click += (s, e) => {
                LoadDashboardSummary();
                LoadAktivitas();
                MessageBox.Show("Data Dashboard diperbarui.", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            // Tombol lain baru pindah form
            guna2DataKamar.Click += (s, e) => GantiForm(new DataKamarA());
            guna2DataTamu.Click += (s, e) => GantiForm(new DataTamuA());
            guna2LaporanTransaksi.Click += (s, e) => GantiForm(new LaporanTransaksiA());
            guna2DataReservasi.Click += (s, e) => GantiForm(new DataReservasi());
            guna2DataUser.Click += (s, e) => GantiForm(new DataUser());
            guna2LaporanKeuangan.Click += (s, e) => GantiForm(new LaporanKeuangan2());

            guna2Logout.Click += (s, e) => Logout();
        }

        // FUNGSI BARU UNTUK PINDAH FORM BIAR GAK TUMPUK
        private void GantiForm(Form targetForm)
        {
            targetForm.WindowState = FormWindowState.Maximized; // Pastikan form tujuan juga Full Screen
            targetForm.Show();
            this.Hide(); // Sembunyikan dashboard saat pindah
        }

        private void ApplyElegantTheme()
        {
            this.BackColor = ColorTranslator.FromHtml("#F4F6F8");

            // Sidebar & Header
            guna2Panel1.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel5.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2PictureBox1.BackColor = Color.Transparent;

            // Tabel
            guna2DataGridView1.BackgroundColor = Color.White;
            guna2DataGridView1.EnableHeadersVisualStyles = false;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#C5A059");
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            guna2DataGridView1.ColumnHeadersHeight = 40;
            guna2DataGridView1.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2DataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F0E68C");
            guna2DataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Fix Label Kepotong
            FixLabel(guna2HtmlLabel2);
            FixLabel(guna2HtmlLabel4);
            FixLabel(guna2HtmlLabel6);
            FixLabel(guna2HtmlLabel7);
            FixLabel(guna2HtmlLabel8); // Label Admin

            FixLabel(guna2JumlahKamar);
            FixLabel(guna2JumlahUser);
            FixLabel(guna2ReservasiHariIni);

            // Style Tombol Sidebar
            StyleSidebarButton(guna2Dashboard);
            StyleSidebarButton(guna2DataKamar);
            StyleSidebarButton(guna2DataTamu);
            StyleSidebarButton(guna2LaporanTransaksi);
            StyleSidebarButton(guna2DataReservasi);
            StyleSidebarButton(guna2DataUser);
            StyleSidebarButton(guna2LaporanKeuangan);
            StyleSidebarButton(guna2Logout);

        }

        private void StyleSidebarButton(Guna.UI2.WinForms.Guna2Button btn)
        {
            btn.FillColor = Color.Transparent;
            btn.ForeColor = ColorTranslator.FromHtml("#333333");
            btn.HoverState.FillColor = ColorTranslator.FromHtml("#E2E8F0");
            btn.HoverState.ForeColor = Color.Black;
        }

        private void FixLabel(Control lbl)
        {
            if (lbl != null)
            {
                if (lbl is Guna.UI2.WinForms.Guna2HtmlLabel gLabel) gLabel.AutoSize = true;
                lbl.ForeColor = ColorTranslator.FromHtml("#333333");
                lbl.BringToFront();
            }
        }

        private void Logout()
        {
            DialogResult result = MessageBox.Show("Apakah kamu yakin ingin logout?", "Konfirmasi Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Hide();
                new Login().Show();
            }
        }

        private void DashboardAdmin_Load(object sender, EventArgs e)
        {
            LoadAktivitas();
            LoadDashboardSummary();
        }

        // Database Logic (Sama Seperti Sebelumnya)
        private void LoadAktivitas()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT u.username AS `User`, CONCAT(u.role, ' - ', l.aktivitas) AS `Aktivitas`, l.waktu AS `Waktu Aktivitas` 
                                     FROM log_aktivitas l INNER JOIN user u ON u.id_user = l.user_id ORDER BY l.waktu DESC";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    dtAktivitas.Clear();
                    da.Fill(dtAktivitas);
                    guna2DataGridView1.DataSource = dtAktivitas;
                    guna2DataGridView1.AllowUserToAddRows = false;
                    guna2DataGridView1.ReadOnly = true;
                    if (guna2DataGridView1.Columns["User"] != null) guna2DataGridView1.Columns["User"].HeaderText = "User";
                }
            }
            catch { }
        }

        private void LoadDashboardSummary()
        {
            guna2JumlahKamar.Text = GetCount("SELECT COUNT(*) FROM kamar").ToString();
            guna2JumlahUser.Text = GetCount("SELECT COUNT(*) FROM user").ToString();
            guna2ReservasiHariIni.Text = GetCount("SELECT COUNT(*) FROM reservasi WHERE tanggal_checkin = CURDATE()").ToString();

            guna2JumlahKamar.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2JumlahUser.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2ReservasiHariIni.ForeColor = ColorTranslator.FromHtml("#333333");
        }

        private int GetCount(string query)
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch { return 0; }
        }

        // Event Kosong
        private void guna2Panel6_Paint(object sender, PaintEventArgs e) { }
        private void guna2HtmlLabel5_Click(object sender, EventArgs e) { }
        private void guna2DataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e) { }
        private void guna2Logout_Click(object sender, EventArgs e) { }
        private void guna2JumlahUser_Click(object sender, EventArgs e) { }
        private void guna2JumlahKamar_Click(object sender, EventArgs e) { }
    }
}
