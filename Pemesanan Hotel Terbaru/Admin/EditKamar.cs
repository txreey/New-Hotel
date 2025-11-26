using System;
using System.IO;
using System.Drawing; // Tambahkan ini untuk warna
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Pemesanan_Hotel_Terbaru.Admin
{
    public partial class EditKamar : Form
    {
        private string idKamar;
        private string selectedImagePath = "";
        private string originalImagePath = "";

        public EditKamar(string id)
        {
            InitializeComponent();
            idKamar = id;

            // 1. Setting Layar (Tengah) & Tema
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Biar ga bisa di-resize sembarangan
            this.MaximizeBox = false;
            ApplyElegantTheme();

            // 2. Isi Combo Box
            guna2Status.DropDownStyle = ComboBoxStyle.DropDownList;
            guna2Status.Items.Clear();
            guna2Status.Items.Add("Tersedia");
            guna2Status.Items.Add("Terisi");
            guna2Status.Items.Add("Perbaikan");

            // 3. Event Handler
            guna2UploadFile.Click += guna2UploadFile_Click;
            guna2Simpan.Click += guna2Simpan_Click;
            guna2Batal.Click += guna2Batal_Click;

            // 4. Load Data
            LoadDataKamar();
        }

        // ============================================================
        // 🎨 TEMA ELEGANT POP-UP (KONSISTEN DASHBOARD)
        // ============================================================
        private void ApplyElegantTheme()
        {
            // Background Form
            this.BackColor = ColorTranslator.FromHtml("#F4F6F8");

            // Label-Label (Warna Gelap)
            // Loop semua control, cari label dan ubah warnanya
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

            // Tombol Upload (Abu Muda / Biru Langit soft)
            guna2UploadFile.FillColor = ColorTranslator.FromHtml("#E2E8F0");
            guna2UploadFile.ForeColor = ColorTranslator.FromHtml("#333333");
            guna2UploadFile.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            guna2UploadFile.BorderThickness = 1;

            // Input Fields (TextBox & ComboBox)
            // Biar seragam background putih border halus
            StyleInput(guna2TipeKamar);
            StyleInput(guna2NoKamar);
            StyleInput(guna2Harga);
            StyleInput(guna2Deskripsi);

            guna2Status.FillColor = Color.White;
            guna2Status.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
        }

        private void StyleInput(Guna.UI2.WinForms.Guna2TextBox txt)
        {
            txt.FillColor = Color.White;
            txt.BorderColor = ColorTranslator.FromHtml("#CBD5E1"); // Abu halus
            txt.ForeColor = ColorTranslator.FromHtml("#333333"); // Teks gelap
            txt.FocusedState.BorderColor = ColorTranslator.FromHtml("#C5A059"); // Fokus jadi Emas
        }

        // ============================================================
        // 🛠️ LOGIKA DATA
        // ============================================================
        private void LoadDataKamar()
        {
            using (MySqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM kamar WHERE id_kamar = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", idKamar);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    guna2TipeKamar.Text = reader["tipe_kamar"].ToString();
                    guna2NoKamar.Text = reader["no_kamar"].ToString();
                    guna2Harga.Text = reader["harga"].ToString();

                    string status = reader["status"].ToString();
                    int index = guna2Status.Items.IndexOf(status);
                    guna2Status.SelectedIndex = (index >= 0) ? index : 0;

                    guna2Deskripsi.Text = reader["deskripsi"]?.ToString() ?? "";

                    selectedImagePath = reader["picture"].ToString();
                    originalImagePath = selectedImagePath;

                    if (!string.IsNullOrEmpty(selectedImagePath))
                        guna2UploadFile.Text = "📸 " + Path.GetFileName(selectedImagePath);
                }
            }
        }

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
            // VALIDASI
            if (string.IsNullOrWhiteSpace(guna2TipeKamar.Text) ||
                string.IsNullOrWhiteSpace(guna2NoKamar.Text) ||
                string.IsNullOrWhiteSpace(guna2Harga.Text) ||
                guna2Status.SelectedIndex == -1)
            {
                MessageBox.Show("❗ Semua data wajib diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hargaText = guna2Harga.Text.Replace(",", "").Replace(".", "").Trim();
            if (!decimal.TryParse(hargaText, out decimal hargaDecimal) || hargaDecimal <= 0)
            {
                MessageBox.Show("❗ Harga tidak valid!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string destinationPath = originalImagePath;

                // COPY GAMBAR BARU
                if (!string.IsNullOrEmpty(selectedImagePath) && selectedImagePath != originalImagePath)
                {
                    string fileName = Path.GetFileName(selectedImagePath);
                    string folderPath = Path.Combine(Application.StartupPath, "Images");
                    Directory.CreateDirectory(folderPath);
                    destinationPath = Path.Combine(folderPath, fileName);

                    if (File.Exists(destinationPath) && destinationPath != originalImagePath)
                    {
                        fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(fileName)}";
                        destinationPath = Path.Combine(folderPath, fileName);
                    }

                    File.Copy(selectedImagePath, destinationPath, true);
                }

                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = @"UPDATE kamar 
                                     SET tipe_kamar=@tipe, 
                                         no_kamar=@no, 
                                         status=@status, 
                                         harga=@harga, 
                                         deskripsi=@deskripsi,
                                         picture=@pic 
                                     WHERE id_kamar=@id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@tipe", guna2TipeKamar.Text.Trim());
                    cmd.Parameters.AddWithValue("@no", guna2NoKamar.Text.Trim());
                    cmd.Parameters.AddWithValue("@status", guna2Status.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@harga", hargaDecimal);
                    cmd.Parameters.AddWithValue("@deskripsi", guna2Deskripsi.Text.Trim());
                    cmd.Parameters.AddWithValue("@pic", destinationPath);
                    cmd.Parameters.AddWithValue("@id", idKamar);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Data berhasil disimpan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal update: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Batal_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Event Kosong
        private void guna2UploadFile_Click_2(object sender, EventArgs e) { }
        private void guna2Batal_Click_2(object sender, EventArgs e) { }
        private void guna2Simpan_Click_2(object sender, EventArgs e) { }
        private void guna2Status_SelectedIndexChanged(object sender, EventArgs e) { }
        private void guna2NoKamar_TextChanged_2(object sender, EventArgs e) { }
        private void guna2TipeKamar_TextChanged(object sender, EventArgs e) { }
        private void guna2Harga_TextChanged_2(object sender, EventArgs e) { }
        private void EditKamar_Load(object sender, EventArgs e) { }
        private void guna2Deskripsi_TextChanged(object sender, EventArgs e) { }
    }
}
