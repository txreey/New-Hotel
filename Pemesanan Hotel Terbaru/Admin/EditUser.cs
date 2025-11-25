using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Pemesanan_Hotel_Terbaru.Admin
{
    public partial class EditUser : Form
    {
        private string idUser;

        public EditUser(string id)
        {
            InitializeComponent();
            idUser = id;

            // Isi dropdown role
            guna2Role.Items.Add("admin");
            guna2Role.Items.Add("owner");
            guna2Role.Items.Add("resepsionis");

            LoadDataUser();
        }

        private void LoadDataUser()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM user WHERE id_user = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", idUser);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        guna2Username.Text = reader["username"].ToString();
                        guna2Email.Text = reader["email"].ToString();
                        guna2Password.Text = reader["password"].ToString();
                        guna2Role.SelectedItem = reader["role"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Simpan_Click(object sender, EventArgs e)
        {
            // 🔹 1️⃣ Validasi kosong
            if (string.IsNullOrWhiteSpace(guna2Username.Text) ||
                string.IsNullOrWhiteSpace(guna2Email.Text) ||
                string.IsNullOrWhiteSpace(guna2Password.Text) ||
                guna2Role.SelectedIndex == -1)
            {
                MessageBox.Show("❗ Semua data harus diisi lengkap!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 2️⃣ Validasi Username (hanya huruf)
            if (!Regex.IsMatch(guna2Username.Text, @"^[a-zA-Z]+$"))
            {
                MessageBox.Show("❗ Username hanya boleh berisi huruf tanpa angka atau simbol!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 3️⃣ Validasi Email (harus format nama@gmail.com)
            if (!Regex.IsMatch(guna2Email.Text, @"^[a-zA-Z0-9._%+-]+@gmail\.com$"))
            {
                MessageBox.Show("❗ Email harus menggunakan format nama@gmail.com!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 4️⃣ Validasi Password (minimal 6 karakter dan harus mengandung huruf + angka/simbol)
            string password = guna2Password.Text;
            if (password.Length < 6 || !Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*[\d\W]).+$"))
            {
                MessageBox.Show("❗ Password minimal 6 karakter dan harus mengandung huruf serta angka atau simbol!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 5️⃣ Update ke database
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE user SET username=@username, email=@email, password=@password, role=@role WHERE id_user=@id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", guna2Username.Text);
                    cmd.Parameters.AddWithValue("@email", guna2Email.Text);
                    cmd.Parameters.AddWithValue("@password", guna2Password.Text);
                    cmd.Parameters.AddWithValue("@role", guna2Role.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@id", idUser);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Data user berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Balik ke halaman DataUser
                this.Hide();
                DataUser dataUserForm = new DataUser();
                dataUserForm.ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Batal_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Yakin ingin membatalkan perubahan?",
                "Konfirmasi Batal",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.Hide();
                DataUser dataUserForm = new DataUser();
                dataUserForm.ShowDialog();
                this.Close();
            }
        }

        private void EditUser_Load(object sender, EventArgs e)
        {
            // Kosong (bisa diisi kalau mau ada event load)
        }

        private void guna2Role_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}
