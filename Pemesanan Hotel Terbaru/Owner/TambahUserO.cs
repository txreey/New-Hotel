using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Pemesanan_Hotel_Terbaru.Owner
{
    public partial class TambahUserO : Form
    {
        public TambahUserO()
        {
            InitializeComponent();

            // Isi dropdown role
            guna2Role.Items.Add("admin");
            guna2Role.Items.Add("owner");
            guna2Role.Items.Add("resepsionis");
        }

        private bool ValidasiInput()
        {
            // Username hanya huruf
            if (!Regex.IsMatch(guna2Username.Text, @"^[A-Za-z]+$"))
            {
                MessageBox.Show("Username hanya boleh huruf tanpa angka atau simbol!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Email harus format @gmail.com
            if (!Regex.IsMatch(guna2Email.Text, @"^[a-zA-Z0-9._%+-]+@gmail\.com$"))
            {
                MessageBox.Show("Email harus menggunakan format @gmail.com!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Password minimal 6 karakter dan harus kombinasi huruf & angka/simbol
            if (guna2Password.Text.Length < 6 ||
                !Regex.IsMatch(guna2Password.Text, @"^(?=.*[A-Za-z])(?=.*\d|.*[!@#$%^&*()_+=\-]).+$"))
            {
                MessageBox.Show("Password minimal 6 karakter dan harus kombinasi huruf + angka/simbol (contoh: tri66*)", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (guna2Role.SelectedIndex == -1)
            {
                MessageBox.Show("Pilih role user terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void guna2Simpan_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput()) return;

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

                this.Hide();
                DataUserO dataUserForm = new DataUserO();
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
                DataUserO dataUserForm = new DataUserO();
                dataUserForm.ShowDialog();
                this.Close();
            }
        }
        private void guna2Role_SelectedIndexChanged(object sender, EventArgs e)
        {
            //
        }
        private void guna2Password_TextChanged(object sender, EventArgs e)
        {
            //
        }
        private void guna2Email_TextChanged(object sender, EventArgs e)
        {
            //
        }
        private void guna2Username_TextChanged(object sender, EventArgs e)
        {
            //
        }

        private void TambahUserO_Load(object sender, EventArgs e)
        {

        }
    }
}
