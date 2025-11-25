using MySql.Data.MySqlClient;
using System.Data;
using System;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class Reservasi : Form
    {
        private DataTable dtReservasi;

        public Reservasi()
        {
            InitializeComponent();
            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardResepsionis());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarR());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuR());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiR());
            guna2Reservasi.Click += (s, e) => OpenForm(new Reservasi());
            guna2TransaksiPembayaran.Click += (s, e) => OpenForm(new TransaksiPembayaran());
            guna2Logout.Click += (s, e) => Logout();
        }

        private void OpenForm(Form targetForm)
        {
            this.Hide();
            targetForm.ShowDialog();
            this.Close();
        }

        private void Logout()
        {
            DialogResult result = MessageBox.Show("Apakah kamu yakin ingin logout?",
                "Konfirmasi Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Hide();
                new Login().Show();
            }
        }

        private void Reservasi_Load(object sender, EventArgs e)
        {
            LoadDataReservasi();
        }

        // ============================================================
        // LOAD DATA DARI DATABASE
        // ============================================================
        private void LoadDataReservasi()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            r.id_reservasi,
                            t.nama_tamu,
                            k.tipe_kamar,
                            k.no_kamar,
                            r.check_in,
                            r.check_out,
                            r.status_pembayaran
                        FROM reservasi r
                        JOIN tamu t ON r.id_tamu = t.id_tamu
                        JOIN kamar k ON r.id_kamar = k.id_kamar";

                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    dtReservasi = new DataTable();
                    da.Fill(dtReservasi);

                    guna2DataGridView1.DataSource = dtReservasi;

                    AddActionButtons();
                    ApplyActionButtonRules();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data: " + ex.Message);
            }
        }

        // ============================================================
        //  TAMBAH TOMBOL EDIT & DELETE
        // ============================================================
        private void AddActionButtons()
        {
            if (!guna2DataGridView1.Columns.Contains("Edit"))
            {
                DataGridViewButtonColumn edit = new DataGridViewButtonColumn();
                edit.Name = "Edit";
                edit.HeaderText = "Edit";
                edit.Text = "Edit";
                edit.UseColumnTextForButtonValue = true;
                guna2DataGridView1.Columns.Add(edit);
            }

            if (!guna2DataGridView1.Columns.Contains("Delete"))
            {
                DataGridViewButtonColumn delete = new DataGridViewButtonColumn();
                delete.Name = "Delete";
                delete.HeaderText = "Delete";
                delete.Text = "Delete";
                delete.UseColumnTextForButtonValue = true;
                guna2DataGridView1.Columns.Add(delete);
            }
        }

        // ============================================================
        // ATUR TOMBOL SESUAI STATUS PEMBAYARAN
        // ============================================================
        private void ApplyActionButtonRules()
        {
            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                if (row.Cells["status_pembayaran"].Value == null) continue;

                string status = row.Cells["status_pembayaran"].Value.ToString().ToLower();

                if (status == "sudah bayar")
                {
                    row.Cells["Edit"].Value = "";
                    row.Cells["Delete"].Value = "";

                    row.Cells["Edit"].ReadOnly = true;
                    row.Cells["Delete"].ReadOnly = true;

                    row.Cells["Edit"].Style.ForeColor = System.Drawing.Color.Gray;
                    row.Cells["Delete"].Style.ForeColor = System.Drawing.Color.Gray;
                }
            }
        }

        // ============================================================
        //  EVENT KLIK EDIT / DELETE
        // ============================================================
        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string status = guna2DataGridView1.Rows[e.RowIndex]
                                .Cells["status_pembayaran"].Value.ToString().ToLower();

            if (status == "sudah bayar") return;

            string idReservasi = guna2DataGridView1.Rows[e.RowIndex]
                                    .Cells["id_reservasi"].Value.ToString();

            if (guna2DataGridView1.Columns[e.ColumnIndex].Name == "Edit")
            {
                new EditReservasi(idReservasi).ShowDialog();
                LoadDataReservasi();
            }
            else if (guna2DataGridView1.Columns[e.ColumnIndex].Name == "Delete")
            {
                DeleteReservasi(idReservasi);
                LoadDataReservasi();
            }
        }

        // ============================================================
        // DELETE RESERVASI
        // ============================================================
        private void DeleteReservasi(string id)
        {
            using (MySqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string cek = "SELECT COUNT(*) FROM transaksi_pembayaran WHERE id_reservasi = @id";
                MySqlCommand cmdCek = new MySqlCommand(cek, conn);
                cmdCek.Parameters.AddWithValue("@id", id);

                int count = Convert.ToInt32(cmdCek.ExecuteScalar());
                if (count > 0)
                {
                    MessageBox.Show("Reservasi ini sudah memiliki transaksi sehingga tidak dapat dihapus.",
                        "Tidak Bisa Hapus", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            DialogResult result = MessageBox.Show("Hapus reservasi ini?", "Konfirmasi", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand("DELETE FROM reservasi WHERE id_reservasi=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Data berhasil dihapus!");
            }
        }

        private void guna2Tambah_Click(object sender, EventArgs e)
        {
            new TambahReservasi().ShowDialog();
            LoadDataReservasi();
        }

        // ============================================================
        //  FITUR PENCARIAN
        // ============================================================
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            if (dtReservasi == null) return;

            string search = guna2Cari.Text.Trim();
            DataView dv = dtReservasi.DefaultView;

            if (string.IsNullOrEmpty(search))
                dv.RowFilter = "";
            else
                dv.RowFilter = $"nama_tamu LIKE '%{search}%'";

            guna2DataGridView1.DataSource = dv;
        }

        // ============================================================
        // FILTER TANGGAL
        // ============================================================
        private void ApplyDateFilter()
        {
            if (dtReservasi == null) return;

            DateTime dari = guna2DariTanggal.Value.Date;
            DateTime sampai = guna2SampaiTanggal.Value.Date;

            DataView dv = dtReservasi.DefaultView;
            dv.RowFilter = $"check_in >= '#{dari:yyyy-MM-dd}#' AND check_out <= '#{sampai:yyyy-MM-dd}#'";

            guna2DataGridView1.DataSource = dv;
        }

        private void guna2DariTanggal_ValueChanged(object sender, EventArgs e)
        {
            ApplyDateFilter();
        }

        private void guna2SampaiTanggal_ValueChanged(object sender, EventArgs e)
        {
            ApplyDateFilter();
        }

        // ============================================================
        // RESET FILTER
        // ============================================================
        private void guna2Reset_Click(object sender, EventArgs e)
        {
            guna2Cari.Text = "";
            guna2DariTanggal.Value = DateTime.Now;
            guna2SampaiTanggal.Value = DateTime.Now;

            dtReservasi.DefaultView.RowFilter = "";
            guna2DataGridView1.DataSource = dtReservasi;
        }

        // ============================================================
        // EXPORT EXCEL
        // ============================================================
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Reservasi_Resepsionis.xlsx";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(dtReservasi, "Reservasi");

                        ws.Column(5).Style.DateFormat.Format = "dd MMMM yyyy";
                        ws.Column(6).Style.DateFormat.Format = "dd MMMM yyyy";

                        ws.Columns().AdjustToContents();
                        wb.SaveAs(save.FileName);
                    }

                    MessageBox.Show("Export berhasil!", "Sukses");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal export: " + ex.Message);
            }
        }
    }
}

