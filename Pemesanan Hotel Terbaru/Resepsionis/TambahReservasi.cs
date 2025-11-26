using System;
using System.Data;
using System.Drawing; // Wajib untuk tema
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class TambahReservasi : Form
    {
        public TambahReservasi()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            ApplyElegantTheme();

            // 2. Load Data Awal
            LoadTipeKamar();
            guna2Deskripsi.ReadOnly = true;

            // 3. Event Handler
            guna2TipeKamar.SelectedIndexChanged += guna2TipeKamar_SelectedIndexChanged;
            guna2NoKamar.SelectedIndexChanged += guna2NoKamar_SelectedIndexChanged;
            guna2Check_in.ValueChanged += ValidasiTanggal;
            guna2Check_out.ValueChanged += ValidasiTanggal;
            guna2Simpan.Click += guna2Simpan_Click;
            guna2Batal.Click += guna2Batal_Click;
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

            // Tombol Simpan (Emas)
            guna2Simpan.FillColor = ColorTranslator.FromHtml("#C5A059");
            guna2Simpan.ForeColor = Color.White;

            // Tombol Batal (Abu Gelap)
            guna2Batal.FillColor = ColorTranslator.FromHtml("#2C3E50");
            guna2Batal.ForeColor = Color.White;

            // Styling Input
            StyleInput(guna2NamaTamu);
            StyleInput(guna2NIK);
            StyleInput(guna2Alamat);
            StyleInput(guna2NoHandphone);
            StyleInput(guna2Email);
            StyleInput(guna2Deskripsi); // Deskripsi ReadOnly

            // Styling ComboBox & Date
            StyleCombo(guna2TipeKamar);
            StyleCombo(guna2NoKamar);
            StyleDate(guna2Check_in);
            StyleDate(guna2Check_out);
        }

        private void StyleInput(Guna.UI2.WinForms.Guna2TextBox txt)
        {
            txt.FillColor = Color.White;
            txt.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            txt.ForeColor = ColorTranslator.FromHtml("#333333");
            txt.FocusedState.BorderColor = ColorTranslator.FromHtml("#C5A059");
        }

        // 🔥 BAGIAN YANG SUDAH DIPERBAIKI (cm -> cmb)
        private void StyleCombo(Guna.UI2.WinForms.Guna2ComboBox cmb)
        {
            cmb.FillColor = Color.White;
            cmb.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            cmb.ForeColor = ColorTranslator.FromHtml("#333333");
        }

        private void StyleDate(Guna.UI2.WinForms.Guna2DateTimePicker dtp)
        {
            dtp.FillColor = Color.White;
            dtp.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            dtp.ForeColor = ColorTranslator.FromHtml("#333333");
            dtp.BorderThickness = 1;
        }

        // ============================================================
        // 🛠️ LOGIKA UTAMA
        // ============================================================
        private void LoadTipeKamar()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT DISTINCT tipe_kamar FROM kamar WHERE status = 'Tersedia'";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    guna2TipeKamar.DataSource = dt;
                    guna2TipeKamar.DisplayMember = "tipe_kamar";
                    guna2TipeKamar.ValueMember = "tipe_kamar";
                    guna2TipeKamar.SelectedIndex = -1;
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal load tipe: " + ex.Message); }
        }

        private void guna2TipeKamar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2TipeKamar.SelectedValue == null) return;

            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT id_kamar, no_kamar FROM kamar WHERE tipe_kamar = @tipe AND status = 'Tersedia'";
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
            catch (Exception ex) { MessageBox.Show("Gagal load nomor: " + ex.Message); }
        }

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
            catch { }
        }

        private void ValidasiTanggal(object sender, EventArgs e)
        {
            if (guna2Check_out.Value.Date <= guna2Check_in.Value.Date)
            {
                MessageBox.Show("Check-out harus lebih besar dari Check-in!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                guna2Check_out.Value = guna2Check_in.Value.Date.AddDays(1);
            }
        }

        // ============================================================
        // SIMPAN & KIRIM EMAIL
        // ============================================================
        private void guna2Simpan_Click(object sender, EventArgs e)
        {
            // 1. Validasi Input
            if (string.IsNullOrWhiteSpace(guna2NamaTamu.Text) ||
                string.IsNullOrWhiteSpace(guna2NIK.Text) ||
                string.IsNullOrWhiteSpace(guna2Alamat.Text) ||
                string.IsNullOrWhiteSpace(guna2NoHandphone.Text) ||
                string.IsNullOrWhiteSpace(guna2Email.Text) ||
                guna2NoKamar.SelectedValue == null)
            {
                MessageBox.Show("Semua data wajib diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Regex.IsMatch(guna2NIK.Text.Trim(), @"^\d{16}$"))
            {
                MessageBox.Show("NIK harus 16 digit angka!", "Peringatan"); return;
            }
            if (!Regex.IsMatch(guna2NoHandphone.Text.Trim(), @"^\d{10,15}$"))
            {
                MessageBox.Show("No HP harus 10-15 digit angka!", "Peringatan"); return;
            }
            if (!IsValidEmail(guna2Email.Text.Trim()))
            {
                MessageBox.Show("Email tidak valid!", "Peringatan"); return;
            }

            // 2. Simpan ke Database
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // Simpan Tamu
                            string qTamu = @"INSERT INTO tamu (nama_tamu, nik, alamat, no_handphone, email) 
                                             VALUES (@nama, @nik, @alamat, @hp, @email); SELECT LAST_INSERT_ID();";
                            MySqlCommand cmdTamu = new MySqlCommand(qTamu, conn, trans);
                            cmdTamu.Parameters.AddWithValue("@nama", guna2NamaTamu.Text.Trim());
                            cmdTamu.Parameters.AddWithValue("@nik", guna2NIK.Text.Trim());
                            cmdTamu.Parameters.AddWithValue("@alamat", guna2Alamat.Text.Trim());
                            cmdTamu.Parameters.AddWithValue("@hp", guna2NoHandphone.Text.Trim());
                            cmdTamu.Parameters.AddWithValue("@email", guna2Email.Text.Trim());
                            int idTamu = Convert.ToInt32(cmdTamu.ExecuteScalar());

                            // Simpan Reservasi
                            string qRes = @"INSERT INTO reservasi (id_tamu, id_kamar, check_in, check_out, status_pembayaran) 
                                            VALUES (@idTamu, @idKamar, @in, @out, 'Belum Bayar')";
                            MySqlCommand cmdRes = new MySqlCommand(qRes, conn, trans);
                            cmdRes.Parameters.AddWithValue("@idTamu", idTamu);
                            cmdRes.Parameters.AddWithValue("@idKamar", guna2NoKamar.SelectedValue);
                            cmdRes.Parameters.AddWithValue("@in", guna2Check_in.Value.Date);
                            cmdRes.Parameters.AddWithValue("@out", guna2Check_out.Value.Date);
                            cmdRes.ExecuteNonQuery();

                            // Update Status Kamar
                            new MySqlCommand($"UPDATE kamar SET status='Terisi' WHERE id_kamar={guna2NoKamar.SelectedValue}", conn, trans).ExecuteNonQuery();

                            trans.Commit();

                            // Kirim Email
                            KirimEmailInvoice(
                                guna2Email.Text.Trim(),
                                guna2NamaTamu.Text.Trim(),
                                guna2NoKamar.Text,
                                guna2TipeKamar.Text,
                                guna2Check_in.Value.Date,
                                guna2Check_out.Value.Date
                            );

                            MessageBox.Show("Reservasi Berhasil! Invoice terkirim.", "Sukses");
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("Gagal simpan: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Koneksi Error: " + ex.Message); }
        }

        private bool IsValidEmail(string email)
        {
            try { return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase); }
            catch { return false; }
        }

        private void KirimEmailInvoice(string email, string nama, string noKamar, string tipe, DateTime inDate, DateTime outDate)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient smtp = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("putrade145@gmail.com", "NT Stay Hotel");
                mail.To.Add(email);
                mail.Subject = "Konfirmasi Reservasi - NT Stay Hotel";
                mail.Body = $"Halo {nama},\n\nTerima kasih telah reservasi.\n\nDetail:\nKamar: {tipe} - {noKamar}\nCheck-in: {inDate:dd MMM yyyy}\nCheck-out: {outDate:dd MMM yyyy}\n\nSilakan lakukan pembayaran saat check-in.";

                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("putrade145@gmail.com", "mfcz lqgk dfyp jkpm");
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
            catch (Exception ex) { MessageBox.Show("Gagal kirim email: " + ex.Message); }
        }

        private void guna2Batal_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Event Kosong
        private void TambahReservasi_Load(object sender, EventArgs e) { }
        private void guna2Deskripsi_TextChanged(object sender, EventArgs e) { }
        private void guna2NamaTamu_TextChanged(object sender, EventArgs e) { }
        private void guna2Alamat_TextChanged(object sender, EventArgs e) { }
        private void guna2NIK_TextChanged(object sender, EventArgs e) { }
        private void guna2NoHandphone_TextChanged(object sender, EventArgs e) { }
        private void guna2Email_TextChanged(object sender, EventArgs e) { }
        private void guna2Check_in_ValueChanged(object sender, EventArgs e) { }
        private void guna2Check_out_ValueChanged(object sender, EventArgs e) { }
        private void guna2Panel2_Paint(object sender, PaintEventArgs e) { }
    }
}
