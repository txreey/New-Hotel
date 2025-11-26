using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing; // Wajib untuk tema
using System.Windows.Forms;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class TransaksiPembayaran : Form
    {
        public TransaksiPembayaran()
        {
            InitializeComponent();

            // 1. Setting Layar & Tema
            this.WindowState = FormWindowState.Maximized;
            ApplyElegantTheme();

            // 2. Navigasi Sidebar
            guna2Dashboard.Click += (s, e) => PindahForm(new DashboardResepsionis());
            guna2DataKamar.Click += (s, e) => PindahForm(new DataKamarR());
            guna2DataTamu.Click += (s, e) => PindahForm(new DataTamuR());
            guna2LaporanTransaksi.Click += (s, e) => PindahForm(new LaporanTransaksiR());
            guna2Reservasi.Click += (s, e) => PindahForm(new Reservasi());
            guna2TransaksiPembayaran.Click += (s, e) => { LoadUnpaidReservations(); }; // Refresh diri sendiri
            guna2Logout.Click += (s, e) => Logout();

            // 3. Events & Load
            this.Load += TransaksiPembayaran_Load;
            guna2NamaTamu.SelectedIndexChanged += guna2NamaTamu_SelectedIndexChanged;
            guna2Bayar.Click += guna2Bayar_Click;

            // Isi Dropdown
            guna2Pembayaran.Items.Clear();
            guna2Pembayaran.Items.Add("Cash");
            guna2Pembayaran.Items.Add("Cashless");
            guna2Pembayaran.SelectedIndex = 0;
        }

        // ============================================================
        // 🎨 TEMA ELEGANT (KONSISTEN)
        // ============================================================
        private void ApplyElegantTheme()
        {
            // Background
            this.BackColor = ColorTranslator.FromHtml("#F4F6F8");
            guna2Panel1.FillColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel1.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel5.FillColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2Panel5.BackColor = ColorTranslator.FromHtml("#F9F7F2");
            guna2PictureBox1.BackColor = Color.Transparent;

            // Label Gelap
            foreach (Control c in Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2HtmlLabel) c.ForeColor = ColorTranslator.FromHtml("#333333");
            }

            // Tombol Bayar (Emas)
            guna2Bayar.FillColor = ColorTranslator.FromHtml("#C5A059");
            guna2Bayar.ForeColor = Color.White;

            // Reset Sidebar
            StyleSidebarButton(guna2Dashboard);
            StyleSidebarButton(guna2DataKamar);
            StyleSidebarButton(guna2DataTamu);
            StyleSidebarButton(guna2LaporanTransaksi);
            StyleSidebarButton(guna2Reservasi);
            StyleSidebarButton(guna2TransaksiPembayaran);
            StyleSidebarButton(guna2Logout);

            // Highlight Transaksi
            guna2TransaksiPembayaran.FillColor = ColorTranslator.FromHtml("#E2E8F0");

            // Styling Input (Biar rapi)
            StyleInput(guna2Kamar);
            StyleInput(guna2Total);
            StyleDate(guna2Check_in);
            StyleDate(guna2Check_out);
            StyleCombo(guna2NamaTamu);
            StyleCombo(guna2Pembayaran);
        }

        private void StyleSidebarButton(Guna.UI2.WinForms.Guna2Button btn)
        {
            btn.FillColor = Color.Transparent;
            btn.CheckedState.FillColor = Color.Transparent;
            btn.ForeColor = ColorTranslator.FromHtml("#333333");
            btn.HoverState.FillColor = ColorTranslator.FromHtml("#E2E8F0");
            btn.HoverState.ForeColor = Color.Black;
        }

        private void StyleInput(Guna.UI2.WinForms.Guna2TextBox txt)
        {
            txt.FillColor = Color.White;
            txt.BorderColor = ColorTranslator.FromHtml("#CBD5E1");
            txt.ForeColor = ColorTranslator.FromHtml("#333333");
            txt.ReadOnly = true; // Readonly karena data dari database
        }

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
            dtp.Enabled = false; // Readonly
        }

        // ============================================================
        // 🛠️ LOGIKA UTAMA
        // ============================================================
        private void TransaksiPembayaran_Load(object sender, EventArgs e)
        {
            LoadUnpaidReservations();
        }

        private void LoadUnpaidReservations()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    // Load tamu yg statusnya BELUM bayar
                    string query = @"
                        SELECT r.id_reservasi, t.nama_tamu
                        FROM reservasi r
                        JOIN tamu t ON r.id_tamu = t.id_tamu
                        WHERE COALESCE(r.status_pembayaran, '') != 'Sudah Bayar'
                        ORDER BY t.nama_tamu";

                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    guna2NamaTamu.DataSource = dt;
                    guna2NamaTamu.DisplayMember = "nama_tamu";
                    guna2NamaTamu.ValueMember = "id_reservasi";
                    guna2NamaTamu.SelectedIndex = -1;

                    // Bersihkan Field
                    guna2Kamar.Text = "";
                    guna2Total.Text = "";
                    guna2Check_in.Value = DateTime.Today;
                    guna2Check_out.Value = DateTime.Today;
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal load data: " + ex.Message); }
        }

        private void guna2NamaTamu_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2NamaTamu.SelectedValue == null) return;

            try
            {
                string idReservasi = guna2NamaTamu.SelectedValue.ToString();

                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT r.check_in, r.check_out, k.tipe_kamar, k.no_kamar, k.harga
                        FROM reservasi r
                        JOIN kamar k ON r.id_kamar = k.id_kamar
                        WHERE r.id_reservasi = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", idReservasi);

                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            DateTime checkIn = dr.GetDateTime("check_in");
                            DateTime checkOut = dr.GetDateTime("check_out");
                            string tipe = dr.GetString("tipe_kamar");
                            string noKamar = dr.GetString("no_kamar");
                            decimal harga = dr.GetDecimal("harga");

                            // Isi form otomatis
                            guna2Kamar.Text = $"{tipe} - {noKamar}";
                            guna2Check_in.Value = checkIn;
                            guna2Check_out.Value = checkOut;

                            // Hitung hari (Minimal 1 hari)
                            int lamaMenginap = (checkOut.Date - checkIn.Date).Days;
                            if (lamaMenginap < 1) lamaMenginap = 1;

                            decimal total = harga * lamaMenginap;
                            guna2Total.Text = total.ToString("N0");

                            // Simpan harga per malam untuk dikirim ke modal nanti
                            guna2Total.Tag = harga;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal ambil detail: " + ex.Message); }
        }

        private void guna2Bayar_Click(object sender, EventArgs e)
        {
            if (guna2NamaTamu.SelectedValue == null)
            {
                MessageBox.Show("Pilih nama tamu terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string idReservasi = guna2NamaTamu.SelectedValue.ToString();
            string namaTamu = guna2NamaTamu.Text;
            string metode = guna2Pembayaran.SelectedItem?.ToString() ?? "Cash";

            if (!decimal.TryParse(guna2Total.Text.Replace(",", ""), out decimal total))
            {
                MessageBox.Show("Total tagihan tidak valid."); return;
            }

            // Ambil harga per malam dari Tag (disimpan saat dropdown berubah)
            if (!decimal.TryParse(guna2Total.Tag?.ToString(), out decimal hargaPerMalam))
            {
                hargaPerMalam = 0;
            }

            // Buka Modal Pembayaran
            using (ModalPembayaran modal = new ModalPembayaran(idReservasi, namaTamu, hargaPerMalam, total, metode))
            {
                if (modal.ShowDialog() == DialogResult.OK)
                {
                    // Jika sukses, refresh dropdown (tamu yg sudah bayar hilang)
                    LoadUnpaidReservations();
                }
            }
        }

        // ============================================================
        // NAVIGASI
        // ============================================================
        private void PindahForm(Form targetForm)
        {
            targetForm.WindowState = FormWindowState.Maximized;
            targetForm.Show();
            this.Hide();
        }

        private void Logout()
        {
            if (MessageBox.Show("Logout?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Hide(); new Login().Show();
            }
        }

        // Event Kosong
        private void guna2Kamar_TextChanged(object sender, EventArgs e) { }
        private void guna2Check_in_ValueChanged(object sender, EventArgs e) { }
        private void guna2Check_out_ValueChanged(object sender, EventArgs e) { }
        private void guna2Total_TextChanged(object sender, EventArgs e) { }
        private void guna2Pembayaran_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}
