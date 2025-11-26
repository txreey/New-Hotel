using System;
using System.Data;
using System.Drawing; // Wajib untuk tema
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class EditReservasi : Form
    {
        string idReservasi;

        public EditReservasi(string id)
        {
            InitializeComponent();
            idReservasi = id;

            // 1. Setting Layar & Tema
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Tidak bisa resize
            this.MaximizeBox = false;
            ApplyElegantTheme();

            // 2. Load Data
            LoadData();
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (SERAGAM DENGAN FORM EDIT LAIN)
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

            // Styling Input (TextBox)
            StyleInput(guna2NamaTamu);
            StyleInput(guna2NIK);
            StyleInput(guna2Alamat);
            StyleInput(guna2NoHandphone);
            StyleInput(guna2Email);

            // Styling DatePicker
            StyleDate(guna2Check_in);
            StyleDate(guna2Check_out);
        }

        private void StyleInput(Guna.UI2.WinForms.Guna2TextBox txt)
        {
            txt.FillColor = Color.White;
            txt.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            txt.ForeColor = ColorTranslator.FromHtml("#333333");
            txt.FocusedState.BorderColor = ColorTranslator.FromHtml("#C5A059"); // Fokus Emas
        }

        private void StyleDate(Guna.UI2.WinForms.Guna2DateTimePicker dtp)
        {
            dtp.FillColor = Color.White;
            dtp.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            dtp.ForeColor = ColorTranslator.FromHtml("#333333");
            dtp.BorderThickness = 1;
        }

        // ============================================================
        // 🛠️ LOGIKA LOAD & SIMPAN
        // ============================================================
        private void LoadData()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT r.*, t.* 
                                     FROM reservasi r
                                     JOIN tamu t ON r.id_tamu = t.id_tamu
                                     WHERE r.id_reservasi=@id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", idReservasi);
                    MySqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        guna2NamaTamu.Text = dr["nama_tamu"].ToString();
                        guna2NIK.Text = dr["nik"].ToString();
                        guna2Alamat.Text = dr["alamat"].ToString();
                        guna2NoHandphone.Text = dr["no_handphone"].ToString();
                        guna2Email.Text = dr["email"].ToString();
                        guna2Check_in.Value = Convert.ToDateTime(dr["check_in"]);
                        guna2Check_out.Value = Convert.ToDateTime(dr["check_out"]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Simpan_Click(object sender, EventArgs e)
        {
            // VALIDASI INPUT
            if (string.IsNullOrWhiteSpace(guna2NamaTamu.Text) ||
                string.IsNullOrWhiteSpace(guna2NIK.Text) ||
                string.IsNullOrWhiteSpace(guna2Alamat.Text) ||
                string.IsNullOrWhiteSpace(guna2NoHandphone.Text) ||
                string.IsNullOrWhiteSpace(guna2Email.Text))
            {
                MessageBox.Show("❗ Semua data harus diisi lengkap!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // VALIDASI TANGGAL
            if (guna2Check_out.Value.Date <= guna2Check_in.Value.Date)
            {
                MessageBox.Show("❗ Tanggal Check-Out harus lebih besar dari Check-In!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = @"UPDATE tamu t
                                     JOIN reservasi r ON t.id_tamu = r.id_tamu
                                     SET t.nama_tamu=@nama, t.nik=@nik, t.alamat=@alamat, 
                                         t.no_handphone=@nohp, t.email=@email,
                                         r.check_in=@checkin, r.check_out=@checkout
                                     WHERE r.id_reservasi=@id";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nama", guna2NamaTamu.Text.Trim());
                    cmd.Parameters.AddWithValue("@nik", guna2NIK.Text.Trim());
                    cmd.Parameters.AddWithValue("@alamat", guna2Alamat.Text.Trim());
                    cmd.Parameters.AddWithValue("@nohp", guna2NoHandphone.Text.Trim());
                    cmd.Parameters.AddWithValue("@email", guna2Email.Text.Trim());
                    cmd.Parameters.AddWithValue("@checkin", guna2Check_in.Value.Date);
                    cmd.Parameters.AddWithValue("@checkout", guna2Check_out.Value.Date);
                    cmd.Parameters.AddWithValue("@id", idReservasi);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("✅ Data berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Batal_Click(object sender, EventArgs e)
        {
            this.Close(); // Cukup tutup, jangan buka dialog baru
        }

        // Event Kosong
        private void guna2Deskripsi_TextChanged(object sender, EventArgs e) { }
        private void guna2Email_TextChanged(object sender, EventArgs e) { }
        private void guna2Check_out_ValueChanged(object sender, EventArgs e) { }
        private void guna2NoHandphone_TextChanged(object sender, EventArgs e) { }
        private void guna2Check_in_ValueChanged(object sender, EventArgs e) { }
        private void guna2Alamat_TextChanged(object sender, EventArgs e) { }
        private void guna2NoKamar_SelectedIndexChanged(object sender, EventArgs e) { }
        private void guna2NIK_TextChanged(object sender, EventArgs e) { }
        private void guna2TipeKamar_SelectedIndexChanged(object sender, EventArgs e) { }
        private void guna2NamaTamu_TextChanged(object sender, EventArgs e) { }
        private void EditReservasi_Load(object sender, EventArgs e) { }
    }
}
