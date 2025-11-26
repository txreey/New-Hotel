using System;
using System.Data;
using System.Drawing; // Wajib untuk tema
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
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (SERAGAM DENGAN EDIT & TAMBAH LAINNYA)
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
            txt.FocusedState.BorderColor = ColorTranslator.FromHtml("#C5A059"); // Fokus Emas
        }

        // ============================================================
        // LOGIKA SIMPAN
        // ============================================================
        private void guna2Simpan_Click(object sender, EventArgs e)
        {
            // 1. Validasi Kosong
            if (string.IsNullOrWhiteSpace(guna2Username.Text) ||
                string.IsNullOrWhiteSpace(guna2Email.Text) ||
                string.IsNullOrWhiteSpace(guna2Password.Text) ||
                guna2Role.SelectedIndex == -1)
            {
                MessageBox.Show("❗ Semua data wajib diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Validasi Regex (Username huruf saja)
            if (!Regex.IsMatch(guna2Username.Text, @"^[a-zA-Z]+$"))
            {
                MessageBox.Show("❗ Username hanya boleh huruf!", "Peringatan");
                return;
            }

            // 3. Validasi Email
            if (!Regex.IsMatch(guna2Email.Text, @"^[a-zA-Z0-9._%+-]+@gmail\.com$"))
            {
                MessageBox.Show("❗ Email harus format nama@gmail.com!", "Peringatan");
                return;
            }

            // 4. Validasi Password (Kuat)
            string password = guna2Password.Text;
            if (password.Length < 6 || !Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*[\d\W]).+$"))
            {
                MessageBox.Show("❗ Password minimal 6 karakter (huruf + angka/simbol)!", "Peringatan");
                return;
            }

            // 5. Simpan ke Database
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

                MessageBox.Show("✅ User berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Tutup form Tambah, balik otomatis ke DataUser (Parent)
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal simpan: " + ex.Message);
            }
        }

        private void guna2Batal_Click(object sender, EventArgs e)
        {
            // Langsung tutup saja, jangan buka DataUser baru biar ga numpuk
            this.Close();
        }

        // Event Kosong
        private void TambahUser_Load(object sender, EventArgs e) { }
        private void guna2Role_SelectedIndexChanged(object sender, EventArgs e) { }
        private void guna2Password_TextChanged(object sender, EventArgs e) { }
        private void guna2Email_TextChanged(object sender, EventArgs e) { }
        private void guna2Username_TextChanged(object sender, EventArgs e) { }
    }
}
