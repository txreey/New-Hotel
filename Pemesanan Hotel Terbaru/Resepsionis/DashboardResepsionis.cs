using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing; // Wajib untuk pewarnaan
using System.Windows.Forms;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class DashboardResepsionis : Form
    {
        private DataTable dtAktivitas = new DataTable();

        public DashboardResepsionis()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Sidebar Navigation
            // Dashboard: Refresh data saja
            guna2Dashboard.Click += (s, e) => {
                LoadDashboardSummary();
                LoadAktivitas();
                MessageBox.Show("Dashboard diperbarui.", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            // Menu Lain: Pindah Form
            guna2DataKamar.Click += (s, e) => PindahForm(new DataKamarR());
            guna2DataTamu.Click += (s, e) => PindahForm(new DataTamuR());
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiR());
            guna2Reservasi.Click += (s, e) => PindahForm(new Reservasi());
            guna2TransaksiPembayaran.Click += (s, e) => PindahForm(new TransaksiPembayaran());

            guna2Logout.Click += (s, e) => Logout();
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (KONSISTEN DENGAN ADMIN & OWNER)
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

            // Label Judul & Statistik (Gelap agar terbaca)
            foreach (Control c in this.Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2HtmlLabel lbl)
                {
                    lbl.ForeColor = ColorTranslator.FromHtml("#333333");
                    lbl.AutoSize = true; // Biar tidak kepotong
                }
            }

            // Label Angka Statistik Spesifik
            guna2JumlahKamar.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2JumlahTamuHariini2.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2ReservasiHariIni.ForeColor = ColorTranslator.FromHtml("#333333");

            // Styling Tabel (Header Emas)
            guna2DataGridView1.BackgroundColor = Color.White;
            guna2DataGridView1.EnableHeadersVisualStyles = false;

            guna2DataGridView1.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#C5A059");
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            guna2DataGridView1.ColumnHeadersHeight = 40;

            guna2DataGridView1.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2DataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F0E68C");
            guna2DataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Reset Style Tombol Sidebar
            StyleSidebarButton(guna2Dashboard);
            StyleSidebarButton(guna2DataKamar);
            StyleSidebarButton(guna2DataTamu);
            StyleSidebarButton(guna2LaporanTransaksi);
            StyleSidebarButton(guna2Reservasi);
            StyleSidebarButton(guna2TransaksiPembayaran);
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
        private void DashboardResepsionis_Load(object sender, EventArgs e)
        {
            LoadDashboardSummary();
            LoadAktivitas();
        }

        private void LoadAktivitas()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            DATE_FORMAT(tanggal, '%Y-%m-%d %H:%i') AS `Tanggal`,
                            nama_tamu AS `Nama Tamu`,
                            aktivitas AS `Aktivitas`
                        FROM log_resepsionis
                        ORDER BY tanggal DESC LIMIT 50"; // Limit biar ringan

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    dtAktivitas.Clear();
                    da.Fill(dtAktivitas);

                    guna2DataGridView1.DataSource = dtAktivitas;
                    guna2DataGridView1.AllowUserToAddRows = false;
                    guna2DataGridView1.ReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat log aktivitas: " + ex.Message);
            }
        }

        private void LoadDashboardSummary()
        {
            guna2JumlahKamar.Text = GetCount("SELECT COUNT(*) FROM kamar").ToString();
            guna2JumlahTamuHariini2.Text = GetCount("SELECT COUNT(*) FROM reservasi WHERE tanggal_checkin = CURDATE()").ToString();
            guna2ReservasiHariIni.Text = GetCount("SELECT COUNT(*) FROM reservasi WHERE DATE(tanggal_reservasi) = CURDATE()").ToString();
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

        // Event kosong
        private void guna2HtmlLabel3_Click(object sender, EventArgs e) { }
        private void guna2Panel2_Paint(object sender, PaintEventArgs e) { }
        private void guna2HtmlLabel4_Click(object sender, EventArgs e) { }
        private void guna2HtmlLabel6_Click(object sender, EventArgs e) { }
        private void guna2HtmlLabel2_Click(object sender, EventArgs e) { }
    }
}
