using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Pemesanan_Hotel_Terbaru.Owner
{
    public partial class EditUserO : Form
    {
        private string idUser;

        public EditUserO(string id)
        {
            InitializeComponent();
            idUser = id;

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

        private bool ValidasiInput()
        {
            if (!Regex.IsMatch(guna2Username.Text, @"^[A-Za-z]+$"))
            {
                MessageBox.Show("Username hanya boleh huruf tanpa angka atau simbol!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(guna2Email.Text, @"^[a-zA-Z0-9._%+-]+@gmail\.com$"))
            {
                MessageBox.Show("Email harus menggunakan format @gmail.com!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

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

                this.Hide();
                DataUserO dataUserForm = new DataUserO();
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
        private void EditUserO_Load(object sender, EventArgs e)
        {
            //
        }

    }
}
