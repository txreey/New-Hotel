using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Pemesanan_Hotel_Terbaru
{
    public partial class Login : Form
    {
        // Koneksi ke database MySQL
        string connectionString = "server=localhost;database=hotelnew;uid=root;pwd=;";

        public Login()
        {
            InitializeComponent();

            // ❌ SAYA SUDAH MENGHAPUS BARIS: guna2Login.Click += ...
            // Karena baris itulah yang bikin aplikasimu error muncul dua kali.
            // Visual Studio (Designer) sudah otomatis memasangnya.

            // Pastikan form login muncul di tengah saat dijalankan
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void guna2Login_Click(object sender, EventArgs e)
        {
            string username = guna2Username.Text.Trim();
            string password = guna2Password.Text.Trim();

            if (username == "" || password == "")
            {
                MessageBox.Show("Username dan Password tidak boleh kosong!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT * FROM user WHERE username = @username AND password = @password";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string role = reader["role"].ToString();
                        MessageBox.Show("Login Berhasil sebagai " + role, "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Form targetForm = null;

                        // Tentukan form tujuan berdasarkan role
                        if (role == "admin")
                        {
                            targetForm = new Admin.DashboardAdmin();
                        }
                        else if (role == "owner")
                        {
                            targetForm = new Owner.DashboardOwner();
                        }
                        else if (role == "resepsionis")
                        {
                            targetForm = new Resepsionis.DashboardResepsionis();
                        }

                        // Buka Form Tujuan
                        if (targetForm != null)
                        {
                            // KUNCI: Set Maximized DULU sebelum Show() biar ga kaget ukurannya
                            targetForm.WindowState = FormWindowState.Maximized;
                            targetForm.Show();

                            // Sembunyikan form Login
                            this.Hide();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Username atau Password salah!", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan koneksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event-event kosong (Biarkan saja, jangan dihapus kalau masih nyantol di Designer)
        private void guna2PictureBox1_Click(object sender, EventArgs e) { }
        private void guna2Panel1_Paint(object sender, PaintEventArgs e) { }
        private void guna2Password_TextChanged(object sender, EventArgs e) { }
        private void guna2Username_TextChanged(object sender, EventArgs e) { }
    }
}
