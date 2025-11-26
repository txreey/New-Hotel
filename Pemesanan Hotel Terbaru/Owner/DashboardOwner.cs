using Pemesanan_Hotel_Terbaru.Admin; // Opsional jika butuh referensi ke Admin
using System;
using System.Data;
using System.Drawing; // Wajib untuk warna
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Pemesanan_Hotel_Terbaru.Owner
{
    public partial class DashboardOwner : Form
    {
        private DataTable dtAktivitas = new DataTable();

        public DashboardOwner()
        {
            InitializeComponent();

            // 1. Setting Awal (Full Screen)
            this.WindowState = FormWindowState.Maximized;

            // 2. Terapkan Tema Elegant
            ApplyElegantTheme();

            // 3. Navigasi Sidebar (Logika PindahForm agar tidak numpuk)
            // Tombol Dashboard: Cukup Refresh data, jangan buka form baru
            guna2Dashboard.Click += (s, e) => {
                LoadAktivitas();
                LoadDashboardSummary();
                MessageBox.Show("Dashboard diperbarui.", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            // Tombol Menu Lain (Pindah Form)
            guna2DataKamar.Click += (s, e) => PindahForm(new DataKamarO());
            guna2DataReservasi.Click += (s, e) => PindahForm(new DataReservasiO());
            guna2DataTamu.Click += (s, e) => PindahForm(new DataTamuO());
            guna2DataUser.Click += (s, e) => PindahForm(new DataUserO());
            guna2LaporanKeuangan.Click += (s, e) => PindahForm(new LaporanKeuangan());
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiO());

            guna2Logout.Click += (s, e) => Logout();
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (SAMA SEPERTI ADMIN)
        // ============================================================
        private void ApplyElegantTheme()
        {
            // Background Utama
            this.BackColor = ColorTranslator.FromHtml("#F4F6F8");

            // Sidebar & Header (Cream)
            guna2Panel1.FillColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel1.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel5.FillColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel5.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2PictureBox1.BackColor = Color.Transparent;

            // Label Judul & Statistik (Gelap)
            // Loop semua label agar otomatis jadi gelap
            foreach (Control c in this.Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2HtmlLabel lbl)
                {
                    lbl.ForeColor = ColorTranslator.FromHtml("#333333");
                    lbl.AutoSize = true; // Biar tidak kepotong
                }
            }
            // Pastikan label angka statistik juga gelap
            guna2JumlahKamar.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2ReservasiHariIni.ForeColor = ColorTranslator.FromHtml("#333333");
            // Jika ada label Jumlah User, uncomment ini:
            // guna2JumlahUser.ForeColor = ColorTranslator.FromHtml("#333333");

            // Tabel (Header Emas)
            guna2DataGridView1.BackgroundColor = Color.White;
            guna2DataGridView1.EnableHeadersVisualStyles = false;

            guna2DataGridView1.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#C5A059");
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            guna2DataGridView1.ColumnHeadersHeight = 40;

            guna2DataGridView1.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2DataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F0E68C");
            guna2DataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Reset Tombol Sidebar
            StyleSidebarButton(guna2Dashboard);
            StyleSidebarButton(guna2DataKamar);
            StyleSidebarButton(guna2DataReservasi);
            StyleSidebarButton(guna2DataTamu);
            StyleSidebarButton(guna2DataUser);
            StyleSidebarButton(guna2LaporanKeuangan);
            StyleSidebarButton(guna2LaporanTransaksi);
            StyleSidebarButton(guna2Logout);

            // Highlight Dashboard (Aktif)
            guna2Dashboard.FillColor = ColorTranslator.FromHtml("#E2E8F0");
        }

        private void StyleSidebarButton(Guna.UI2.WinForms.Guna2Button btn)
        {
            btn.FillColor = Color.Transparent;
            btn.CheckedState.FillColor = Color.Transparent;
            btn.ForeColor = ColorTranslator.FromHtml("#333333"); // Teks Abu Gelap
            btn.HoverState.FillColor = ColorTranslator.FromHtml("#E2E8F0"); // Hover Abu Muda
            btn.HoverState.ForeColor = Color.Black;
        }

        // ============================================================
        // NAVIGASI
        // ============================================================
        private void PindahForm(Form targetForm)
        {
            targetForm.WindowState = FormWindowState.Maximized;
            targetForm.Show();
            this.Hide();
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
                new Login().Show();
            }
        }

        // ============================================================
        // LOGIKA DATA
        // ============================================================
        private void DashboardOwner_Load(object sender, EventArgs e)
        {
            LoadAktivitas();
            LoadDashboardSummary();
        }

        private void LoadAktivitas()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT u.username AS `User`, 
                                            CONCAT(u.role, ' - ', l.aktivitas) AS `Aktivitas`, 
                                            l.waktu AS `Waktu Aktivitas`
                                     FROM log_aktivitas l
                                     INNER JOIN user u ON u.id_user = l.user_id
                                     ORDER BY l.waktu DESC LIMIT 50"; // Limit biar ga berat

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    dtAktivitas.Clear();
                    da.Fill(dtAktivitas);

                    guna2DataGridView1.DataSource = dtAktivitas;
                    guna2DataGridView1.AllowUserToAddRows = false;
                    guna2DataGridView1.ReadOnly = true;
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal load aktivitas: " + ex.Message); }
        }

        private void LoadDashboardSummary()
        {
            guna2JumlahKamar.Text = GetCount("SELECT COUNT(*) FROM kamar").ToString();
            guna2ReservasiHariIni.Text = GetCount("SELECT COUNT(*) FROM reservasi WHERE tanggal_checkin = CURDATE()").ToString();
            // guna2JumlahUser.Text = GetCount("SELECT COUNT(*) FROM user").ToString();
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
        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void guna2JumlahKamar_Click(object sender, EventArgs e) { }
        private void guna2ReservasiHariIni_Click(object sender, EventArgs e) { }
        private void guna2JumlahUser_Click(object sender, EventArgs e) { }
        private void guna2HtmlLabel3_Click(object sender, EventArgs e) { }
    }
}
