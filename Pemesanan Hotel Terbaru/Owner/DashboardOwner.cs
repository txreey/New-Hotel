using Pemesanan_Hotel_Terbaru.Admin;
using System;
using System.Data;
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

            // Sidebar button event
            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardOwner());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarO());
            guna2DataReservasi.Click += (s, e) => OpenForm(new DataReservasiO());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuO());
            guna2DataUser.Click += (s, e) => OpenForm(new DataUserO());
            guna2LaporanKeuangan.Click += (s, e) => OpenForm(new LaporanKeuangan());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiO());
            guna2Logout.Click += (s, e) => Logout();
        }

        // Buka form lain
        private void OpenForm(Form targetForm)
        {
            this.Hide();
            targetForm.ShowDialog();
            this.Close();
        }

        // Logout
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

        private void DashboardOwner_Load(object sender, EventArgs e)
        {
            LoadAktivitas();          // tampilkan log aktivitas
            LoadDashboardSummary();   // tampilkan jumlah kamar, user, reservasi hari ini
        }

        // ======================================
        // 🔵 LOAD DATA AKTIVITAS KE DASHBOARD
        // ======================================
        private void LoadAktivitas()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            u.username AS `User`,
                            CONCAT(u.role, ' - ', l.aktivitas) AS `Aktivitas`,
                            l.waktu AS `Waktu Aktivitas`
                        FROM log_aktivitas l
                        INNER JOIN user u ON u.id_user = l.user_id
                        ORDER BY l.waktu DESC
                    ";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    dtAktivitas.Clear();
                    da.Fill(dtAktivitas);

                    guna2DataGridView1.DataSource = dtAktivitas;

                    // HILANGKAN ROW KOSONG
                    guna2DataGridView1.AllowUserToAddRows = false;
                    guna2DataGridView1.ReadOnly = true;

                    // styling header otomatis
                    guna2DataGridView1.Columns["User"].HeaderText = "User";
                    guna2DataGridView1.Columns["Aktivitas"].HeaderText = "Aktivitas";
                    guna2DataGridView1.Columns["Waktu Aktivitas"].HeaderText = "Waktu Aktivitas";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengambil data aktivitas: " + ex.Message);
            }
        }

        // ======================================
        // 🔵 LOAD JUMLAH SUMMARY DASHBOARD
        // ======================================
        private void LoadDashboardSummary()
        {
            guna2JumlahKamar.Text = GetJumlahKamar().ToString();
            //guna2JumlahUser.Text = GetJumlahUser().ToString();
            guna2ReservasiHariIni.Text = GetReservasiHariIni().ToString();
        }

        // ===============================
        // 🟣 JUMLAH KAMAR
        // ===============================
        private int GetJumlahKamar()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM kamar";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch
            {
                return 0;
            }
        }

        // ===============================
        // 🟣 JUMLAH USER
        // ===============================
        private int GetJumlahUser()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM user";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch
            {
                return 0;
            }
        }

        // ===============================
        // 🟣 RESERVASI HARI INI
        // ===============================
        private int GetReservasiHariIni()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM reservasi WHERE tanggal_checkin = CURDATE()";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch
            {
                return 0;
            }
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //
        }

        private void guna2JumlahKamar_Click(object sender, EventArgs e)
        {
            //
        }

        private void guna2ReservasiHariIni_Click(object sender, EventArgs e)
        {
            //
        }

        private void guna2JumlahUser_Click(object sender, EventArgs e)
        {
            //
        }

        private void guna2HtmlLabel3_Click(object sender, EventArgs e)
        {

        }
    }
}
