using MySql.Data.MySqlClient;
using System;
using System.Drawing; // Wajib untuk tema
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class ModalPembayaran : Form
    {
        private string idReservasi;
        private string namaTamu;
        private decimal hargaPerMalam;
        private decimal totalBayar;
        private string metodePembayaran;

        public ModalPembayaran(string idReservasi, string namaTamu, decimal hargaPerMalam, decimal totalBayar, string metodePembayaran)
        {
            InitializeComponent();
            this.idReservasi = idReservasi;
            this.namaTamu = namaTamu;
            this.hargaPerMalam = hargaPerMalam;
            this.totalBayar = totalBayar;
            this.metodePembayaran = metodePembayaran;

            // 1. Setting Layar & Tema
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            ApplyElegantTheme();
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

            // Tombol Bayar (Emas)
            guna2Bayar.FillColor = ColorTranslator.FromHtml("#C5A059");
            guna2Bayar.ForeColor = Color.White;

            // Tombol Batal (Abu Gelap)
            guna2Batal.FillColor = ColorTranslator.FromHtml("#2C3E50");
            guna2Batal.ForeColor = Color.White;

            // Input & Readonly Fields
            StyleInput(guna2HargaPerMalam);
            StyleInput(guna2TotalBayarY);
            StyleInput(guna2KembalianY);

            // Input Uang Masuk (Beda dikit biar menonjol)
            guna2UangMasukY.FillColor = Color.White;
            guna2UangMasukY.BorderColor = ColorTranslator.FromHtml("#C5A059"); // Border Emas
            guna2UangMasukY.ForeColor = Color.Black;
            guna2UangMasukY.FocusedState.BorderColor = ColorTranslator.FromHtml("#C5A059");
        }

        private void StyleInput(Guna.UI2.WinForms.Guna2TextBox txt)
        {
            txt.FillColor = ColorTranslator.FromHtml("#F9F7F2"); // Cream
            txt.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            txt.ForeColor = ColorTranslator.FromHtml("#333333");
            txt.ReadOnly = true; // Pastikan ini readonly untuk display
        }

        // ============================================================
        // 🛠️ LOGIKA UTAMA
        // ============================================================
        private void ModalPembayaran_Load(object sender, EventArgs e)
        {
            guna2HargaPerMalam.Text = hargaPerMalam.ToString("N0");
            guna2TotalBayarY.Text = totalBayar.ToString("N0");
            guna2KembalianY.Text = "0";
            guna2UangMasukY.Focus(); // Langsung fokus ke input uang
        }

        // Hitung Kembalian Otomatis
        private void guna2UangMasukY_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(guna2UangMasukY.Text.Replace(",", ""), out decimal uangMasuk))
            {
                decimal kembali = uangMasuk - totalBayar;
                guna2KembalianY.Text = kembali >= 0 ? kembali.ToString("N0") : "Kurang";

                // Visual feedback jika uang kurang
                if (kembali < 0) guna2KembalianY.ForeColor = Color.Red;
                else guna2KembalianY.ForeColor = ColorTranslator.FromHtml("#333333");
            }
            else
            {
                guna2KembalianY.Text = "0";
            }
        }

        private void guna2Bayar_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(guna2UangMasukY.Text.Replace(",", ""), out decimal uangMasuk))
            {
                MessageBox.Show("Masukkan jumlah uang dengan benar!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (uangMasuk < totalBayar)
            {
                MessageBox.Show("Uang yang dimasukkan kurang!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Simpan Transaksi
                            string insert = @"INSERT INTO transaksi 
                                (id_reservasi, nama_tamu, harga, total_bayar, uang_masuk, kembalian, metode_pembayaran, tanggal_transaksi)
                                VALUES (@id, @nama, @harga, @total, @uang, @kembali, @metode, NOW())";

                            using (MySqlCommand cmd = new MySqlCommand(insert, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", idReservasi);
                                cmd.Parameters.AddWithValue("@nama", namaTamu);
                                cmd.Parameters.AddWithValue("@harga", hargaPerMalam);
                                cmd.Parameters.AddWithValue("@total", totalBayar);
                                cmd.Parameters.AddWithValue("@uang", uangMasuk);
                                cmd.Parameters.AddWithValue("@kembali", uangMasuk - totalBayar);
                                cmd.Parameters.AddWithValue("@metode", metodePembayaran);
                                cmd.ExecuteNonQuery();
                            }

                            // 2. Update Status Pembayaran
                            string updateReservasi = "UPDATE reservasi SET status_pembayaran='Sudah Bayar' WHERE id_reservasi=@id";
                            using (MySqlCommand cmd = new MySqlCommand(updateReservasi, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", idReservasi);
                                cmd.ExecuteNonQuery();
                            }

                            // 3. Update Status Kamar -> Tersedia
                            string updateKamar = @"UPDATE kamar 
                                                   SET status='Tersedia' 
                                                   WHERE id_kamar = (SELECT id_kamar FROM reservasi WHERE id_reservasi=@id)";
                            using (MySqlCommand cmd = new MySqlCommand(updateKamar, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", idReservasi);
                                cmd.ExecuteNonQuery();
                            }

                            // 4. Ambil Data utk Email
                            string email = "", noKamar = "", tipeKamar = "", lamaMenginap = "";
                            string queryInfo = @"SELECT t.email, k.no_kamar, k.tipe_kamar,
                                                 DATEDIFF(r.check_out, r.check_in) AS lama
                                                 FROM reservasi r
                                                 JOIN tamu t ON r.id_tamu = t.id_tamu
                                                 JOIN kamar k ON r.id_kamar = k.id_kamar
                                                 WHERE r.id_reservasi = @id";

                            using (MySqlCommand cmd = new MySqlCommand(queryInfo, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", idReservasi);
                                using (MySqlDataReader dr = cmd.ExecuteReader())
                                {
                                    if (dr.Read())
                                    {
                                        email = dr["email"].ToString();
                                        noKamar = dr["no_kamar"].ToString();
                                        tipeKamar = dr["tipe_kamar"].ToString();
                                        lamaMenginap = dr["lama"].ToString();
                                    }
                                }
                            }

                            // 5. Kirim Email (Di luar Transaction Block agar tidak block DB lama2, tapi di dalam try-catch)
                            // Untuk performa, email bisa dipindah setelah commit, atau pakai async
                            KirimInvoiceEmail(
                                email, namaTamu, noKamar, tipeKamar,
                                hargaPerMalam.ToString("N0"), lamaMenginap,
                                totalBayar.ToString("N0"), metodePembayaran,
                                DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                            );

                            trans.Commit();

                            MessageBox.Show("Pembayaran Berhasil! Invoice terkirim ke email.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("Transaksi Gagal: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Koneksi Error: " + ex.Message); }
        }

        private void KirimInvoiceEmail(string emailTujuan, string nama, string noKamar, string tipe, string harga, string lama, string total, string metode, string tgl)
        {
            try
            {
                string senderEmail = "putrade145@gmail.com";
                string appPassword = "mfcz lqgk dfyp jkpm"; // Pastikan App Password benar

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(senderEmail, "NT Stay Hotel");
                mail.To.Add(emailTujuan);
                mail.Subject = "E-Invoice Pembayaran - NT Stay Hotel";

                mail.Body = $@"Halo {nama},

Terima kasih telah menginap di NT Stay Hotel.
Pembayaran Anda telah kami terima dengan rincian:

-----------------------------------
Kamar       : {tipe} - {noKamar}
Lama Inap   : {lama} Malam
Harga/Malam : Rp {harga}
Total Bayar : Rp {total}
Metode      : {metode}
Tanggal     : {tgl}
-----------------------------------

Kami tunggu kedatangan Anda kembali!

Salam Hangat,
Resepsionis NT Stay Hotel";

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(senderEmail, appPassword);
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal kirim email (transaksi tetap tersimpan): " + ex.Message);
            }
        }

        private void guna2Batal_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Event Kosong
        private void guna2TotalBayarY_TextChanged(object sender, EventArgs e) { }
        private void guna2KembalianY_TextChanged(object sender, EventArgs e) { }
        private void guna2HargaPerMalam_TextChanged(object sender, EventArgs e) { }
    }
}
