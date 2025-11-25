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
        // ⬆️ Ganti uid & pwd sesuai dengan setting XAMPP kamu (biasanya root tanpa password)

        public Login()
        {
            InitializeComponent();

            // Event klik tombol login
            guna2Login.Click += guna2Login_Click;
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

                        // Buka form sesuai role
                        if (role == "admin")
                        {
                            Admin.DashboardAdmin adminForm = new Admin.DashboardAdmin();
                            adminForm.Show();
                            this.Hide();
                        }
                        else if (role == "owner")
                        {
                            Owner.DashboardOwner ownerForm = new Owner.DashboardOwner();
                            ownerForm.Show();
                            this.Hide();
                        }
                        else if (role == "resepsionis")
                        {
                            Resepsionis.DashboardResepsionis resepsionisForm = new Resepsionis.DashboardResepsionis();
                            resepsionisForm.Show();
                            this.Hide();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Username atau Password salah!", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    reader.Close();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan koneksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {
            //
        }
        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {
            //
        }
        private void guna2Password_TextChanged(object sender, EventArgs e)
        {
            //
        }
        private void guna2Username_TextChanged(object sender, EventArgs e)
        {
            //
        }
    }
}
