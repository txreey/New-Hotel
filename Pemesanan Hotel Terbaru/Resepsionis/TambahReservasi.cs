using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class TambahReservasi : Form
    {
        public TambahReservasi()
        {
            InitializeComponent();
            LoadTipeKamar();

            guna2Deskripsi.ReadOnly = true;

            guna2Check_in.ValueChanged += ValidasiTanggal;
            guna2Check_out.ValueChanged += ValidasiTanggal;
        }

        // 🟢 Load semua tipe kamar
        private void LoadTipeKamar()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT DISTINCT tipe_kamar FROM kamar WHERE status = 'Tersedia'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    guna2TipeKamar.DataSource = dt;
                    guna2TipeKamar.DisplayMember = "tipe_kamar";
                    guna2TipeKamar.ValueMember = "tipe_kamar";
                    guna2TipeKamar.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat tipe kamar: " + ex.Message);
            }
        }

        // 🟢 Saat tipe kamar dipilih
        private void guna2TipeKamar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2TipeKamar.SelectedValue == null) return;

            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT id_kamar, no_kamar, deskripsi FROM kamar WHERE tipe_kamar = @tipe AND status = 'Tersedia'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@tipe", guna2TipeKamar.SelectedValue);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    guna2NoKamar.DataSource = dt;
                    guna2NoKamar.DisplayMember = "no_kamar";
                    guna2NoKamar.ValueMember = "id_kamar";
                    guna2NoKamar.SelectedIndex = -1;

                    guna2Deskripsi.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat nomor kamar: " + ex.Message);
            }
        }

        // 🟢 Saat nomor kamar dipilih
        private void guna2NoKamar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2NoKamar.SelectedValue == null) return;

            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT deskripsi FROM kamar WHERE id_kamar = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", guna2NoKamar.SelectedValue);
                    object result = cmd.ExecuteScalar();
                    guna2Deskripsi.Text = result?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menampilkan deskripsi: " + ex.Message);
            }
        }

        // 🟢 Validasi check-in check-out
        private void ValidasiTanggal(object sender, EventArgs e)
        {
            if (guna2Check_out.Value.Date < guna2Check_in.Value.Date)
            {
                MessageBox.Show("Tanggal check-out tidak boleh lebih awal dari check-in!",
                                "Tanggal Tidak Valid",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                guna2Check_out.Value = guna2Check_in.Value.Date;
            }
        }

        // 🟢 Simpan data tamu + reservasi
        private void guna2Simpan_Click(object sender, EventArgs e)
        {
            // 🔹 Validasi input
            if (string.IsNullOrWhiteSpace(guna2NamaTamu.Text) ||
                string.IsNullOrWhiteSpace(guna2NIK.Text) ||
                string.IsNullOrWhiteSpace(guna2Alamat.Text) ||
                string.IsNullOrWhiteSpace(guna2NoHandphone.Text) ||
                string.IsNullOrWhiteSpace(guna2Email.Text) ||
                guna2NoKamar.SelectedValue == null)
            {
                MessageBox.Show("Semua data wajib diisi!", "Validasi Gagal",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Validasi NIK
            if (!Regex.IsMatch(guna2NIK.Text.Trim(), @"^\d{16}$"))
            {
                MessageBox.Show("NIK harus 16 digit angka!",
                                "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Validasi HP
            if (!Regex.IsMatch(guna2NoHandphone.Text.Trim(), @"^\d{10,15}$"))
            {
                MessageBox.Show("Nomor HP harus minimal 10 digit!",
                                "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Validasi Email
            if (!IsValidEmail(guna2Email.Text.Trim()))
            {
                MessageBox.Show("Format email tidak valid!",
                                "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🟢 Simpan database
            using (MySqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                MySqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // 1️⃣ Simpan tamu
                    string queryTamu = @"INSERT INTO tamu (nama_tamu, nik, alamat, no_handphone, email)
                                         VALUES (@nama, @nik, @alamat, @nohp, @email);
                                         SELECT LAST_INSERT_ID();";
                    MySqlCommand cmdTamu = new MySqlCommand(queryTamu, conn, trans);
                    cmdTamu.Parameters.AddWithValue("@nama", guna2NamaTamu.Text);
                    cmdTamu.Parameters.AddWithValue("@nik", guna2NIK.Text);
                    cmdTamu.Parameters.AddWithValue("@alamat", guna2Alamat.Text);
                    cmdTamu.Parameters.AddWithValue("@nohp", guna2NoHandphone.Text);
                    cmdTamu.Parameters.AddWithValue("@email", guna2Email.Text);
                    int idTamu = Convert.ToInt32(cmdTamu.ExecuteScalar());

                    // 2️⃣ Simpan reservasi
                    string queryReservasi = @"INSERT INTO reservasi (id_tamu, id_kamar, check_in, check_out)
                                              VALUES (@idTamu, @idKamar, @checkin, @checkout)";
                    MySqlCommand cmdRes = new MySqlCommand(queryReservasi, conn, trans);
                    cmdRes.Parameters.AddWithValue("@idTamu", idTamu);
                    cmdRes.Parameters.AddWithValue("@idKamar", guna2NoKamar.SelectedValue);
                    cmdRes.Parameters.AddWithValue("@checkin", guna2Check_in.Value.Date);
                    cmdRes.Parameters.AddWithValue("@checkout", guna2Check_out.Value.Date);
                    cmdRes.ExecuteNonQuery();

                    // 3️⃣ Update status kamar
                    string updateKamar = "UPDATE kamar SET status = 'Terisi' WHERE id_kamar = @id";
                    MySqlCommand cmdUpdate = new MySqlCommand(updateKamar, conn, trans);
                    cmdUpdate.Parameters.AddWithValue("@id", guna2NoKamar.SelectedValue);
                    cmdUpdate.ExecuteNonQuery();

                    trans.Commit();

                    // 🟢 KIRIM EMAIL
                    KirimEmailInvoice(
                        guna2Email.Text,
                        guna2NamaTamu.Text,
                        guna2NoKamar.Text,
                        guna2TipeKamar.Text,
                        guna2Check_in.Value.Date,
                        guna2Check_out.Value.Date
                    );

                    MessageBox.Show("Data reservasi berhasil disimpan!\nInvoice telah dikirim ke email tamu.",
                                    "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.Close();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Gagal menyimpan data: " + ex.Message);
                }
            }
        }

        // 🟢 VALIDASI EMAIL
        private bool IsValidEmail(string email)
        {
            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch { return false; }
        }

        // 🟢 KIRIM EMAIL INVOICE
        private void KirimEmailInvoice(string emailTujuan, string namaTamu, string noKamar, string tipeKamar, DateTime checkIn, DateTime checkOut)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient smtp = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("putrade145@gmail.com", "NT Stay Hotel");
                mail.To.Add(emailTujuan);
                mail.Subject = "Invoice Reservasi - NT Stay Hotel";

                mail.Body =
                    $"Halo {namaTamu},\n\n" +
                    $"Terima kasih telah melakukan reservasi di NT_Stay Hotel.\n" +
                    $"Berikut detail reservasi Anda:\n\n" +
                    $"• Tipe kamar : {tipeKamar}\n" +
                    $"• Nomor kamar : {noKamar}\n" +
                    $"• Check-in : {checkIn:dd MMMM yyyy}\n" +
                    $"• Check-out : {checkOut:dd MMMM yyyy}\n\n" +
                    $"Kami menantikan kedatangan Anda!\n\nSalam hangat,\nNT Stay Hotel";

                mail.IsBodyHtml = false;

                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential(
                    "putrade145@gmail.com",
                    "mfcz lqgk dfyp jkpm" // ← kamu isi sendiri di Visual Studio
                );
                smtp.EnableSsl = true;

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Email gagal dikirim: " + ex.Message);
            }
        }
        private void guna2Check_out_ValueChanged(object sender, EventArgs e)
        {
            //
        }
        private void guna2Check_in_ValueChanged(object sender, EventArgs e)
        {
            //
        }
        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {
            //
        }
        private void guna2Email_TextChanged(object sender, EventArgs e)
        {
            //
        }
        private void guna2NoHandphone_TextChanged(object sender, EventArgs e)
        {
            //
        }
        private void guna2NIK_TextChanged(object sender, EventArgs e)
        {
            //
        }
        private void guna2Alamat_TextChanged(object sender, EventArgs e)
        {
            //
        }
        private void guna2NamaTamu_TextChanged(object sender, EventArgs e)
        {
            //
        }
        private void guna2Deskripsi_TextChanged(object sender, EventArgs e)
        {
            //
        }
        private void TambahReservasi_Load(object sender, EventArgs e)
        {
            //
        }
        private void guna2Batal_Click(object sender, EventArgs e)
        {
            //
        }
    }
}
