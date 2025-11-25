using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Pemesanan_Hotel_Terbaru.Admin
{
    public partial class TambahUser : Form
    {
        public TambahUser()
        {
            InitializeComponent();

            // Isi dropdown role
            guna2Role.Items.Add("admin");
            guna2Role.Items.Add("owner");
            guna2Role.Items.Add("resepsionis");
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

            // 🔹 4️⃣ Validasi Password (minimal 6 karakter dan mengandung huruf + angka atau simbol)
            string password = guna2Password.Text;
            if (password.Length < 6 || !Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*[\d\W]).+$"))
            {
                MessageBox.Show("❗ Password minimal 6 karakter dan harus mengandung huruf serta angka atau simbol!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 5️⃣ Simpan data ke database
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO user (username, email, password, role) VALUES (@username, @email, @password, @role)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", guna2Username.Text);
                    cmd.Parameters.AddWithValue("@email", guna2Email.Text);
                    cmd.Parameters.AddWithValue("@password", guna2Password.Text);
                    cmd.Parameters.AddWithValue("@role", guna2Role.SelectedItem.ToString());
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Data user berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Setelah berhasil, balik ke halaman DataUser
                this.Hide();
                DataUser dataUserForm = new DataUser();
                dataUserForm.ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menambahkan user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Batal_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Yakin ingin membatalkan penambahan user?",
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

        private void guna2Role_SelectedIndexChanged(object sender, EventArgs e)
        {
            // opsional
        }

        private void guna2Password_TextChanged(object sender, EventArgs e) { }
        private void guna2Email_TextChanged(object sender, EventArgs e) { }
        private void guna2Username_TextChanged(object sender, EventArgs e) { }
    }
}

