using System;
using System.Data;
using System.Drawing; // Wajib untuk tema
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

            // 1. Setting Layar & Tema
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            ApplyElegantTheme();

            // 2. Isi Role
            guna2Role.Items.Clear();
            guna2Role.Items.Add("admin");
            guna2Role.Items.Add("owner");
            guna2Role.Items.Add("resepsionis");

            // 3. Load Data
            LoadDataUser();
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (KONSISTEN)
        // ============================================================
        private void ApplyElegantTheme()
        {
            // Background
            this.BackColor = ColorTranslator.FromHtml("#F4F6F8");

            // Label Gelap
            foreach (Control c in this.Controls)
            {
                if (c is Label || c is Guna.UI2.WinForms.Guna2HtmlLabel)
                {
                    c.ForeColor = ColorTranslator.FromHtml("#333333");
                }
            }

            // Tombol Simpan (Emas)
            guna2Simpan.FillColor = ColorTranslator.FromHtml("#C5A059");
            guna2Simpan.ForeColor = Color.White;

            // Tombol Batal (Abu Gelap)
            guna2Batal.FillColor = ColorTranslator.FromHtml("#2C3E50");
            guna2Batal.ForeColor = Color.White;

            // Styling Input
            StyleInput(guna2Username);
            StyleInput(guna2Email);
            StyleInput(guna2Password);

            // ComboBox
            guna2Role.FillColor = Color.White;
            guna2Role.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            guna2Role.ForeColor = ColorTranslator.FromHtml("#333333");
        }

        private void StyleInput(Guna.UI2.WinForms.Guna2TextBox txt)
        {
            txt.FillColor = Color.White;
            txt.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            txt.ForeColor = ColorTranslator.FromHtml("#333333");
            txt.FocusedState.BorderColor = ColorTranslator.FromHtml("#C5A059");
        }

        // ============================================================
        // 🛠️ LOGIKA DATA & VALIDASI
        // ============================================================
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

                        string role = reader["role"].ToString();
                        if (guna2Role.Items.Contains(role)) guna2Role.SelectedItem = role;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal load: " + ex.Message); }
        }

        private bool ValidasiInput()
        {
            if (string.IsNullOrWhiteSpace(guna2Username.Text) || string.IsNullOrWhiteSpace(guna2Email.Text))
            {
                MessageBox.Show("Semua data wajib diisi!", "Peringatan"); return false;
            }

            if (!Regex.IsMatch(guna2Username.Text.Trim(), @"^[A-Za-z]+$"))
            {
                MessageBox.Show("Username hanya boleh huruf!", "Peringatan"); return false;
            }

            if (!Regex.IsMatch(guna2Email.Text.Trim(), @"^[a-zA-Z0-9._%+-]+@gmail\.com$"))
            {
                MessageBox.Show("Email harus format @gmail.com!", "Peringatan"); return false;
            }

            // Password minimal 6 karakter (Kombinasi Huruf + Angka/Simbol)
            if (guna2Password.Text.Length < 6 ||
                !Regex.IsMatch(guna2Password.Text, @"^(?=.*[A-Za-z])(?=.*\d|.*[!@#$%^&*()_+=\-]).+$"))
            {
                MessageBox.Show("Password min 6 karakter (Huruf + Angka/Simbol)!", "Peringatan"); return false;
            }

            if (guna2Role.SelectedIndex == -1)
            {
                MessageBox.Show("Pilih role!", "Peringatan"); return false;
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
                    cmd.Parameters.AddWithValue("@username", guna2Username.Text.Trim());
                    cmd.Parameters.AddWithValue("@email", guna2Email.Text.Trim());
                    cmd.Parameters.AddWithValue("@password", guna2Password.Text); // Pass tidak di-trim (biar spasi valid kalau ada)
                    cmd.Parameters.AddWithValue("@role", guna2Role.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@id", idUser);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Data user berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show("Gagal simpan: " + ex.Message); }
        }

        private void guna2Batal_Click(object sender, EventArgs e)
        {
            this.Close(); // Cukup tutup, jangan buka form baru
        }

        // Event Kosong
        private void guna2Role_SelectedIndexChanged(object sender, EventArgs e) { }
        private void guna2Password_TextChanged(object sender, EventArgs e) { }
        private void guna2Email_TextChanged(object sender, EventArgs e) { }
        private void guna2Username_TextChanged(object sender, EventArgs e) { }
        private void EditUserO_Load(object sender, EventArgs e) { }
    }
}
