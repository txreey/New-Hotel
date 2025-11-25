using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class DashboardResepsionis : Form
    {
        private DataTable dtAktivitas = new DataTable();

        public DashboardResepsionis()
        {
            InitializeComponent();

            // Sidebar Navigation
            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardResepsionis());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarR());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuR());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiR());
            guna2Reservasi.Click += (s, e) => OpenForm(new Reservasi());
            guna2TransaksiPembayaran.Click += (s, e) => OpenForm(new TransaksiPembayaran());
            guna2Logout.Click += (s, e) => Logout();
        }

        private void OpenForm(Form targetForm)
        {
            this.Hide();
            targetForm.ShowDialog();
            this.Close();
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

        private void DashboardResepsionis_Load(object sender, EventArgs e)
        {
            LoadDashboardSummary();
            LoadAktivitas();
        }

        // =======================================================
        // 🔵 LOAD LOG AKTIVITAS RESEPSIONIS
        // =======================================================
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
                        ORDER BY tanggal DESC
                    ";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    dtAktivitas.Clear();
                    da.Fill(dtAktivitas);

                    guna2DataGridView1.DataSource = dtAktivitas;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat log aktivitas: " + ex.Message);
            }
        }

        // =======================================================
        // 🔴 DASHBOARD SUMMARY (JUMLAH2)
        // =======================================================
        private void LoadDashboardSummary()
        {
            guna2JumlahKamar.Text = GetJumlahKamar().ToString();
            guna2JumlahTamuHariini2.Text = GetJumlahTamuHariIni().ToString();
            guna2ReservasiHariIni.Text = GetReservasiHariIni().ToString();
        }

        // ► Jumlah Kamar
        private int GetJumlahKamar()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM kamar", conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return 0; }
        }

        // ► Jumlah Tamu yang Check-in hari ini
        private int GetJumlahTamuHariIni()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM reservasi WHERE tanggal_checkin = CURDATE()", conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return 0; }
        }

        // ► Jumlah Reservasi hari ini
        private int GetReservasiHariIni()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM reservasi WHERE DATE(tanggal_reservasi) = CURDATE()", conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
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
