using System;
using System.IO;
using System.Drawing; // Wajib untuk warna
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Pemesanan_Hotel_Terbaru.Admin
{
    public partial class TambahKamar : Form
    {
        private string selectedImagePath = "";

        public TambahKamar()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            ApplyElegantTheme();

            // 2. Isi Dropdown Status
            guna2Status.DropDownStyle = ComboBoxStyle.DropDownList;
            guna2Status.Items.Clear();
            guna2Status.Items.Add("Tersedia");
            guna2Status.Items.Add("Terisi");
            guna2Status.Items.Add("Perbaikan");
            guna2Status.SelectedIndex = 0; // Default Tersedia

            // 3. Event Handler
            guna2UploadFile.Click += guna2UploadFile_Click;
            guna2Simpan.Click += guna2Simpan_Click;
            guna2Batal.Click += guna2Batal_Click;
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (SERAGAM DENGAN EDIT KAMAR)
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

            // Tombol Upload (Abu Muda)
            guna2UploadFile.FillColor = ColorTranslator.FromHtml("#E2E8F0");
            guna2UploadFile.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2UploadFile.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            guna2UploadFile.BorderThickness = 1;

            // Styling Input
            StyleInput(guna2TipeKamar);
            StyleInput(guna2NoKamar);
            StyleInput(guna2Harga);
            StyleInput(guna2Deskripsi);

            // ComboBox
            guna2Status.FillColor = Color.White;
            guna2Status.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            guna2Status.ForeColor = ColorTranslator.FromHtml("#333333");
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
        private void guna2UploadFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            ofd.Title = "Pilih Foto Kamar";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedImagePath = ofd.FileName;
                guna2UploadFile.Text = "📸 " + Path.GetFileName(selectedImagePath);
            }
        }

        private void guna2Simpan_Click(object sender, EventArgs e)
        {
            // VALIDASI LENGKAP
            if (string.IsNullOrWhiteSpace(guna2TipeKamar.Text) ||
                string.IsNullOrWhiteSpace(guna2NoKamar.Text) ||
                string.IsNullOrWhiteSpace(guna2Harga.Text) ||
                string.IsNullOrWhiteSpace(guna2Deskripsi.Text) ||
                guna2Status.SelectedIndex == -1)
            {
                MessageBox.Show("❗ Semua data (termasuk foto) harus diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Cek Foto Wajib Ada
            if (string.IsNullOrEmpty(selectedImagePath))
            {
                MessageBox.Show("❗ Foto kamar belum dipilih!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hargaText = guna2Harga.Text.Replace(",", "").Replace(".", "").Trim();
            if (!decimal.TryParse(hargaText, out decimal hargaDecimal) || hargaDecimal <= 0)
            {
                MessageBox.Show("❗ Harga tidak valid!", "Peringatan");
                return;
            }

            try
            {
                string folderPath = Path.Combine(Application.StartupPath, "Images");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string fileName = Path.GetFileName(selectedImagePath);
                string destinationPath = Path.Combine(folderPath, fileName);

                // Handle nama file duplikat
                if (File.Exists(destinationPath))
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
                    destinationPath = Path.Combine(folderPath, fileName);
                }

                File.Copy(selectedImagePath, destinationPath, true);

                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = @"INSERT INTO kamar (tipe_kamar, no_kamar, status, harga, deskripsi, picture) 
                                     VALUES (@tipe, @no, @status, @harga, @deskripsi, @pic)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@tipe", guna2TipeKamar.Text.Trim());
                    cmd.Parameters.AddWithValue("@no", guna2NoKamar.Text.Trim());
                    cmd.Parameters.AddWithValue("@status", guna2Status.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@harga", hargaDecimal);
                    cmd.Parameters.AddWithValue("@deskripsi", guna2Deskripsi.Text.Trim());
                    cmd.Parameters.AddWithValue("@pic", destinationPath);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Kamar berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal simpan: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Batal_Click(object sender, EventArgs e)
        {
            this.Close(); // Langsung tutup
        }

        // Event Kosong
        private void TambahKamar_Load(object sender, EventArgs e) { }
        private void guna2Deskripsi_TextChanged(object sender, EventArgs e) { }
        private void guna2Harga_TextChanged_1(object sender, EventArgs e) { }
        private void guna2Status_SelectedIndexChanged(object sender, EventArgs e) { }
        private void guna2NoKamar_TextChanged_1(object sender, EventArgs e) { }
        private void guna2TipeKamar_TextChanged(object sender, EventArgs e) { }

        // Event tambahan dari designer (kalau ada)
        private void guna2UploadFile_Click_1(object sender, EventArgs e) => guna2UploadFile_Click(sender, e);
        private void guna2Simpan_Click_1(object sender, EventArgs e) => guna2Simpan_Click(sender, e);
        private void guna2Batal_Click_1(object sender, EventArgs e) => guna2Batal_Click(sender, e);
    }
}
