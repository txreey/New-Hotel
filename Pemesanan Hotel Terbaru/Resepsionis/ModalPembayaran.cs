using MySql.Data.MySqlClient;
using System;
using System.Net.Mail;
using System.Net;
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
        }

        private void ModalPembayaran_Load(object sender, EventArgs e)
        {
            guna2HargaPerMalam.Text = hargaPerMalam.ToString("N0");
            guna2TotalBayarY.Text = totalBayar.ToString("N0");
            guna2KembalianY.Text = "0";
        }

        // otomatis hitung kembalian
        private void guna2UangMasukY_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(guna2UangMasukY.Text.Replace(",", ""), out decimal uangMasuk))
            {
                decimal kembali = uangMasuk - totalBayar;
                guna2KembalianY.Text = kembali >= 0 ? kembali.ToString("N0") : "0";
            }
            else
            {
                guna2KembalianY.Text = "0";
            }
        }

        private void guna2Batal_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        //private void guna2Bayar_Click(object sender, EventArgs e)
        //{
        //    if (!decimal.TryParse(guna2UangMasukY.Text.Replace(",", ""), out decimal uangMasuk))
        //    {
        //        MessageBox.Show("Masukkan jumlah uang dengan benar!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    if (uangMasuk < totalBayar)
        //    {
        //        MessageBox.Show("Uang yang dimasukkan kurang!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    try
        //    {
        //        using (MySqlConnection conn = Koneksi.GetConnection())
        //        {
        //            conn.Open();
        //            using (MySqlTransaction trans = conn.BeginTransaction())
        //            {
        //                try
        //                {
        //                    // 1️⃣ Simpan transaksi ke tabel transaksi_pembayaran
        //                    string insert = @"INSERT INTO transaksi 
        //                        (id_reservasi, nama_tamu, harga, total_bayar, uang_masuk, kembalian, metode_pembayaran, tanggal_transaksi)
        //                        VALUES (@id, @nama, @harga, @total, @uang, @kembali, @metode, NOW())";

        //                    using (MySqlCommand cmd = new MySqlCommand(insert, conn, trans))
        //                    {
        //                        cmd.Parameters.AddWithValue("@id", idReservasi);
        //                        cmd.Parameters.AddWithValue("@nama", namaTamu);
        //                        cmd.Parameters.AddWithValue("@harga", hargaPerMalam);
        //                        cmd.Parameters.AddWithValue("@total", totalBayar);
        //                        cmd.Parameters.AddWithValue("@uang", uangMasuk);
        //                        cmd.Parameters.AddWithValue("@kembali", uangMasuk - totalBayar);
        //                        cmd.Parameters.AddWithValue("@metode", metodePembayaran);
        //                        cmd.ExecuteNonQuery();
        //                    }

        //                    // 2️⃣ Update status pembayaran di tabel reservasi
        //                    string updateReservasi = @"UPDATE reservasi 
        //                                               SET status_pembayaran='Sudah Bayar' 
        //                                               WHERE id_reservasi=@id";
        //                    using (MySqlCommand cmd = new MySqlCommand(updateReservasi, conn, trans))
        //                    {
        //                        cmd.Parameters.AddWithValue("@id", idReservasi);
        //                        cmd.ExecuteNonQuery();
        //                    }

        //                    // 3️⃣ Update status kamar menjadi 'Tersedia'
        //                    string updateKamar = @"UPDATE kamar 
        //                                           SET status='Tersedia' 
        //                                           WHERE id_kamar = (SELECT id_kamar FROM reservasi WHERE id_reservasi=@id)";
        //                    using (MySqlCommand cmd = new MySqlCommand(updateKamar, conn, trans))
        //                    {
        //                        cmd.Parameters.AddWithValue("@id", idReservasi);
        //                        cmd.ExecuteNonQuery();
        //                    }

        //                    trans.Commit();

        //                    MessageBox.Show("Pembayaran berhasil disimpan dan kamar kini tersedia!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                    this.DialogResult = DialogResult.OK;
        //                    this.Close();
        //                }
        //                catch (Exception ex)
        //                {
        //                    trans.Rollback();
        //                    MessageBox.Show("Gagal menyimpan transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Terjadi kesalahan koneksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        private void KirimInvoiceEmail(
    string emailTujuan,
    string namaTamu,
    string noKamar,
    string tipeKamar,
    string hargaPerMalam,
    string lamaMenginap,
    string totalBayar,
    string metodePembayaran,
    string tanggalTransaksi
)
        {
            try
            {
                string emailPengirim = "putrade145@gmail.com";
                string appPassword = "mfcz lqgk dfyp jkpm";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(emailPengirim, "NT Stay Hotel");
                mail.To.Add(emailTujuan);
                mail.Subject = "Pembayaran Berhasil – NT Stay Hotel";

                mail.Body =
        $@"Halo {namaTamu},

Terima kasih telah melakukan pembayaran reservasi Anda di NT Stay Hotel.
Berikut detail pembayaran Anda:

Nama tamu: {namaTamu}
Nomor kamar: {noKamar}
Tipe kamar: {tipeKamar}
Harga per malam: Rp {hargaPerMalam}
Lama menginap: {lamaMenginap} malam
Total bayar: Rp {totalBayar}
Metode pembayaran: {metodePembayaran}
Tanggal transaksi: {tanggalTransaksi}

Kami berharap Anda menikmati pengalaman menginap di NT Stay Hotel.
Sampai jumpa lagi!

Salam hangat,
NT Stay Hotel";

                mail.IsBodyHtml = false;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(emailPengirim, appPassword);
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengirim invoice email: " + ex.Message);
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
                            // 1️⃣ SIMPAN TRANSAKSI
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

                            // 2️⃣ UPDATE STATUS RESERVASI
                            string updateReservasi = @"UPDATE reservasi 
                                               SET status_pembayaran='Sudah Bayar' 
                                               WHERE id_reservasi=@id";
                            using (MySqlCommand cmd = new MySqlCommand(updateReservasi, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", idReservasi);
                                cmd.ExecuteNonQuery();
                            }

                            // 3️⃣ UPDATE STATUS KAMAR
                            string updateKamar = @"UPDATE kamar 
                                           SET status='Tersedia' 
                                           WHERE id_kamar = (SELECT id_kamar FROM reservasi WHERE id_reservasi=@id)";
                            using (MySqlCommand cmd = new MySqlCommand(updateKamar, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", idReservasi);
                                cmd.ExecuteNonQuery();
                            }

                            // 4️⃣ AMBIL DATA TAMBAHAN UNTUK EMAIL INVOICE
                            string email = "";
                            string noKamar = "";
                            string tipeKamar = "";
                            string lamaMenginap = "";

                            string ambil = @"
                        SELECT t.email, k.no_kamar, k.tipe_kamar,
                               DATEDIFF(r.check_out, r.check_in) + 1 AS lama
                        FROM reservasi r
                        JOIN tamu t ON r.id_tamu = t.id_tamu
                        JOIN kamar k ON r.id_kamar = k.id_kamar
                        WHERE r.id_reservasi = @id";

                            using (MySqlCommand cmd = new MySqlCommand(ambil, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", idReservasi);
                                using (MySqlDataReader dr = cmd.ExecuteReader())
                                {
                                    if (dr.Read())
                                    {
                                        email = dr.GetString("email");
                                        noKamar = dr.GetString("no_kamar");
                                        tipeKamar = dr.GetString("tipe_kamar");
                                        lamaMenginap = dr.GetInt32("lama").ToString();
                                    }
                                }
                            }

                            // 5️⃣ KIRIM INVOICE EMAIL
                            KirimInvoiceEmail(
                                email,
                                namaTamu,
                                noKamar,
                                tipeKamar,
                                hargaPerMalam.ToString("N0"),
                                lamaMenginap,
                                totalBayar.ToString("N0"),
                                metodePembayaran,
                                DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                            );

                            trans.Commit();

                            MessageBox.Show("Pembayaran berhasil! Invoice sudah dikirim ke email tamu.", "Sukses",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("Gagal menyimpan transaksi: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan koneksi: " + ex.Message);
            }
        }

        private void guna2TotalBayarY_TextChanged(object sender, EventArgs e) { }
        private void guna2KembalianY_TextChanged(object sender, EventArgs e) { }
        private void guna2HargaPerMalam_TextChanged(object sender, EventArgs e) { }
    }
}
