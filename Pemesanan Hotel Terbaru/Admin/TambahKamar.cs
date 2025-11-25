using System;
using System.IO;
using System.Text.RegularExpressions;
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

            guna2Status.DropDownStyle = ComboBoxStyle.DropDownList;
            guna2Status.Items.Clear();
            guna2Status.Items.Add("Tersedia");
            guna2Status.Items.Add("Terisi");
            guna2Status.Items.Add("Perbaikan");
        }

        private void guna2UploadFile_Click_1(object sender, EventArgs e)
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

        private void guna2Simpan_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(guna2TipeKamar.Text) ||
                string.IsNullOrWhiteSpace(guna2NoKamar.Text) ||
                string.IsNullOrWhiteSpace(guna2Harga.Text) ||
                string.IsNullOrWhiteSpace(guna2Deskripsi.Text) ||
                guna2Status.SelectedIndex == -1 ||
                string.IsNullOrEmpty(selectedImagePath))
            {
                MessageBox.Show("❗ Semua data harus diisi lengkap!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string fileName = Path.GetFileName(selectedImagePath);
                string folderPath = Path.Combine(Application.StartupPath, "Images");
                Directory.CreateDirectory(folderPath);
                string destinationPath = Path.Combine(folderPath, fileName);
                File.Copy(selectedImagePath, destinationPath, true);

                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = @"INSERT INTO kamar (tipe_kamar, no_kamar, status, harga, deskripsi, picture) 
                                     VALUES (@tipe, @no, @status, @harga, @deskripsi, @pic)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@tipe", guna2TipeKamar.Text);
                    cmd.Parameters.AddWithValue("@no", guna2NoKamar.Text);
                    cmd.Parameters.AddWithValue("@status", guna2Status.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@harga", guna2Harga.Text);
                    cmd.Parameters.AddWithValue("@deskripsi", guna2Deskripsi.Text);
                    cmd.Parameters.AddWithValue("@pic", destinationPath);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Data kamar berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();
                new DataKamarA().ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menambahkan kamar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Batal_Click_1(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Yakin ingin membatalkan penambahan kamar?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Hide();
                new DataKamarA().ShowDialog();
                this.Close();
            }
        }
        private void TambahKamar_Load(object sender, EventArgs e)
        {

        }

        private void guna2Deskripsi_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Harga_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void guna2Status_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2NoKamar_TextChanged_1(object sender, EventArgs e)
        {

        }


        private void guna2TipeKamar_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

