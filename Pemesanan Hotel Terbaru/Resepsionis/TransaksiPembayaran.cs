using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class TransaksiPembayaran : Form
    {
        public TransaksiPembayaran()
        {
            InitializeComponent();

            // 🔹 Navigasi (sama seperti form resepsionis lainnya)
            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardResepsionis());
            //guna2Booking.Click += (s, e) => OpenForm(new Booking());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarR());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuR());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiR());
            guna2Reservasi.Click += (s, e) => OpenForm(new Reservasi());
            guna2TransaksiPembayaran.Click += (s, e) => OpenForm(new TransaksiPembayaran());
            guna2Logout.Click += (s, e) => Logout();

            // 🔹 Events
            this.Load += TransaksiPembayaran_Load;
            guna2NamaTamu.SelectedIndexChanged += guna2NamaTamu_SelectedIndexChanged;
            guna2Bayar.Click += guna2Bayar_Click;

            // 🔹 Isi dropdown pembayaran
            guna2Pembayaran.Items.Clear();
            guna2Pembayaran.Items.Add("Cash");
            guna2Pembayaran.Items.Add("Cashless");
            guna2Pembayaran.SelectedIndex = 0;
        }

        private void OpenForm(Form targetForm)
        {
            this.Hide();
            targetForm.ShowDialog();
            this.Close();
        }

        private void Logout()
        {
            DialogResult result = MessageBox.Show("Apakah kamu yakin ingin logout?", "Konfirmasi Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Hide();
                new Login().Show();
            }
        }

        private void TransaksiPembayaran_Load(object sender, EventArgs e)
        {
            LoadUnpaidReservations();
        }

        // 🔹 Load nama tamu yang BELUM melakukan transaksi pembayaran
        private void LoadUnpaidReservations()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
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

                    // Kosongkan field
                    guna2Kamar.Text = "";
                    guna2Total.Text = "";
                    guna2Check_in.Value = DateTime.Today;
                    guna2Check_out.Value = DateTime.Today;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data tamu: " + ex.Message);
            }
        }


        // 🔹 Saat pilih nama tamu -> ambil data dari reservasi
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

                            // isi otomatis
                            guna2Kamar.Text = $"{tipe}-{noKamar}";
                            guna2Check_in.Value = checkIn;
                            guna2Check_out.Value = checkOut;

                            int lamaMenginap = (checkOut.Date - checkIn.Date).Days + 1;
                            if (lamaMenginap < 1) lamaMenginap = 1;

                            decimal total = harga * lamaMenginap;
                            guna2Total.Text = total.ToString("N0");

                            // simpan ke Tag agar bisa dikirim ke modal
                            guna2Total.Tag = harga; // simpan harga per malam
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengambil data reservasi: " + ex.Message);
            }
        }

        // 🔹 Klik tombol BAYAR
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
                MessageBox.Show("Total tidak valid.");
                return;
            }

            if (!decimal.TryParse(guna2Total.Tag?.ToString(), out decimal hargaPerMalam))
            {
                hargaPerMalam = 0;
            }

            // 🔸 buka modal pembayaran
            using (ModalPembayaran modal = new ModalPembayaran(idReservasi, namaTamu, hargaPerMalam, total, metode))
            {
                if (modal.ShowDialog() == DialogResult.OK)
                {
                    // reload dropdown setelah pembayaran sukses
                    LoadUnpaidReservations();
                }
            }
        }

        // event kosong (biar tidak error di designer)
        private void guna2Kamar_TextChanged(object sender, EventArgs e) { }
        private void guna2Check_in_ValueChanged(object sender, EventArgs e) { }
        private void guna2Check_out_ValueChanged(object sender, EventArgs e) { }
        private void guna2Total_TextChanged(object sender, EventArgs e) { }
        private void guna2Pembayaran_SelectedIndexChanged(object sender, EventArgs e) { }
    }

}


