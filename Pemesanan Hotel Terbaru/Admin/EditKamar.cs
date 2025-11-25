using System;
using System.IO;
using System.Text.RegularExpressions;
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

            guna2Status.DropDownStyle = ComboBoxStyle.DropDownList;
            guna2Status.Items.Clear();
            guna2Status.Items.Add("Tersedia");
            guna2Status.Items.Add("Terisi");
            guna2Status.Items.Add("Perbaikan");

            guna2UploadFile.Click += guna2UploadFile_Click;
            guna2Simpan.Click += guna2Simpan_Click;
            guna2Batal.Click += guna2Batal_Click;

            LoadDataKamar();
        }

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

                    // 🆕 Isi kolom deskripsi
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
            if (string.IsNullOrWhiteSpace(guna2TipeKamar.Text) ||
                string.IsNullOrWhiteSpace(guna2NoKamar.Text) ||
                string.IsNullOrWhiteSpace(guna2Harga.Text) ||
                guna2Status.SelectedIndex == -1)
            {
                MessageBox.Show("❗ Semua data harus diisi lengkap!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string hargaText = guna2Harga.Text.Replace(",", "").Replace(".", "");
                if (!decimal.TryParse(hargaText, out decimal hargaDecimal))
                {
                    MessageBox.Show("❗ Format harga tidak valid!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string destinationPath = originalImagePath;

                if (!string.IsNullOrEmpty(selectedImagePath) && selectedImagePath != originalImagePath)
                {
                    string fileName = Path.GetFileName(selectedImagePath);
                    string folderPath = Path.Combine(Application.StartupPath, "Images");
                    Directory.CreateDirectory(folderPath);
                    destinationPath = Path.Combine(folderPath, fileName);
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
                    cmd.Parameters.AddWithValue("@tipe", guna2TipeKamar.Text);
                    cmd.Parameters.AddWithValue("@no", guna2NoKamar.Text);
                    cmd.Parameters.AddWithValue("@status", guna2Status.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@harga", hargaDecimal);
                    cmd.Parameters.AddWithValue("@deskripsi", guna2Deskripsi.Text);
                    cmd.Parameters.AddWithValue("@pic", destinationPath);
                    cmd.Parameters.AddWithValue("@id", idKamar);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Data kamar berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();
                new DataKamarA().ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengupdate kamar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Batal_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Yakin ingin membatalkan perubahan?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Hide();
                new DataKamarA().ShowDialog();
                this.Close();
            }
        }
        private void guna2UploadFile_Click_2(object sender, EventArgs e)
        {
            //
        }
        private void guna2Batal_Click_2(object sender, EventArgs e)
        {
            //
        }
        private void guna2Simpan_Click_2(object sender, EventArgs e)
        {
            //
        }
        private void guna2Status_SelectedIndexChanged(object sender, EventArgs e)
        {
            //
        }
        private void guna2NoKamar_TextChanged_2(object sender, EventArgs e)
        {
            //
        }
        private void guna2TipeKamar_TextChanged(object sender, EventArgs e)
        {
            //
        }
        private void guna2Harga_TextChanged_2(object sender, EventArgs e)
        {
            //
        }
        private void EditKamar_Load(object sender, EventArgs e)
        {
            //
        }

        private void guna2Deskripsi_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

