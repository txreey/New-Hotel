using System;
using System.Data;
using System.Drawing; // Tambahkan ini utk pewarnaan
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

            // 1. Setting Layar & Tema
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            ApplyElegantTheme();

            // 2. Isi Dropdown Role
            guna2Role.Items.Clear();
            guna2Role.Items.Add("admin");
            guna2Role.Items.Add("owner");
            guna2Role.Items.Add("resepsionis");

            // 3. Load Data
            LoadDataUser();
        }

        // ============================================================
        // 🎨 TEMA ELEGANT POP-UP (SAMA SEPERTI EDIT KAMAR)
        // ============================================================
        private void ApplyElegantTheme()
        {
            // Background Form
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

            // Styling Input (Username, Email, Password)
            StyleInput(guna2Username);
            StyleInput(guna2Email);
            StyleInput(guna2Password);

            // Styling ComboBox Role
            guna2Role.FillColor = Color.White;
            guna2Role.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            guna2Role.ForeColor = ColorTranslator.FromHtml("#333333");
        }

        private void StyleInput(Guna.UI2.WinForms.Guna2TextBox txt)
        {
            txt.FillColor = Color.White;
            txt.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            txt.ForeColor = ColorTranslator.FromHtml("#333333");
            txt.FocusedState.BorderColor = ColorTranslator.FromHtml("#C5A059"); // Fokus Emas
        }

        // ============================================================
        // LOGIKA DATA
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

                        // Set Role dengan aman
                        string role = reader["role"].ToString();
                        if (guna2Role.Items.Contains(role))
                            guna2Role.SelectedItem = role;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data user: " + ex.Message);
            }
        }

        private void guna2Simpan_Click(object sender, EventArgs e)
        {
            // VALIDASI
            if (string.IsNullOrWhiteSpace(guna2Username.Text) ||
                string.IsNullOrWhiteSpace(guna2Email.Text) ||
                string.IsNullOrWhiteSpace(guna2Password.Text) ||
                guna2Role.SelectedIndex == -1)
            {
                MessageBox.Show("❗ Semua data wajib diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validasi Regex
            if (!Regex.IsMatch(guna2Username.Text, @"^[a-zA-Z]+$"))
            {
                MessageBox.Show("❗ Username hanya boleh huruf!", "Peringatan");
                return;
            }

            if (!Regex.IsMatch(guna2Email.Text, @"^[a-zA-Z0-9._%+-]+@gmail\.com$"))
            {
                MessageBox.Show("❗ Email harus format nama@gmail.com!", "Peringatan");
                return;
            }

            string password = guna2Password.Text;
            if (password.Length < 6 || !Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*[\d\W]).+$"))
            {
                MessageBox.Show("❗ Password minimal 6 karakter (huruf + angka/simbol)!", "Peringatan");
                return;
            }

            // UPDATE DATABASE
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

                MessageBox.Show("✅ Data berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // Cukup Close, parent form (DataUser) akan refresh otomatis
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal simpan: " + ex.Message);
            }
        }

        private void guna2Batal_Click(object sender, EventArgs e)
        {
            this.Close(); // Langsung tutup saja
        }

        // Event Kosong
        private void EditUser_Load(object sender, EventArgs e) { }
        private void guna2Role_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}
