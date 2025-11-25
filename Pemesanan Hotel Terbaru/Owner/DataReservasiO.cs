using Pemesanan_Hotel_Terbaru.Admin;
using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClosedXML.Excel;

namespace Pemesanan_Hotel_Terbaru.Owner
{
    public partial class DataReservasiO : Form
    {
        private DataTable dtReservasi;

        public DataReservasiO()
        {
            InitializeComponent();

            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardOwner());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarO());
            guna2DataReservasi.Click += (s, e) => OpenForm(new DataReservasiO());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuO());
            guna2DataUser.Click += (s, e) => OpenForm(new DataUserO());
            guna2LaporanKeuangan.Click += (s, e) => OpenForm(new LaporanKeuangan());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiO());
            guna2Logout.Click += (s, e) => Logout();

            this.Load += DataReservasiO_Load;

            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2DariTanggal.ValueChanged += guna2DariTanggal_ValueChanged;
            guna2SampaiTanggal.ValueChanged += guna2SampaiTanggal_ValueChanged;
            guna2Reset.Click += guna2Reset_Click;
            guna2ExportExcel.Click += guna2ExportExcel_Click;
        }

        private void OpenForm(Form targetForm)
        {
            this.Hide();
            targetForm.ShowDialog();
            this.Close();
        }

        private void Logout()
        {
            DialogResult result = MessageBox.Show(
                "Apakah kamu yakin ingin logout?",
                "Konfirmasi Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.Hide();
                new Login().Show();
            }
        }

        // ======================================================
        //            LOAD DATA RESERVASI OWNER
        // ======================================================
        private void LoadDataReservasiOwner()
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
                        JOIN kamar k ON r.id_kamar = k.id_kamar
                        ORDER BY r.id_reservasi DESC
                    ";

                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    dtReservasi = new DataTable();
                    da.Fill(dtReservasi);

                    guna2DataGridView1.DataSource = dtReservasi;

                    // HILANGKAN ROW KOSONG
                    guna2DataGridView1.AllowUserToAddRows = false;
                    guna2DataGridView1.ReadOnly = true;
                    guna2DataGridView1.RowHeadersVisible = false;
                    guna2DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    guna2DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat data reservasi: {ex.Message}");
            }
        }

        private void DataReservasiO_Load(object sender, EventArgs e)
        {
            LoadDataReservasiOwner();
        }

        // ======================================================
        //                    SEARCHING
        // ======================================================
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            if (dtReservasi == null) return;

            string keyword = guna2Cari.Text.Trim();
            DataView dv = dtReservasi.DefaultView;

            if (string.IsNullOrEmpty(keyword))
            {
                dv.RowFilter = "";
            }
            else
            {
                dv.RowFilter = $"nama_tamu LIKE '%{keyword}%'";
            }

            guna2DataGridView1.DataSource = dv;
        }

        // ======================================================
        //                     FILTER TANGGAL
        // ======================================================
        private void ApplyDateFilter()
        {
            if (dtReservasi == null) return;

            DateTime dari = guna2DariTanggal.Value.Date;
            DateTime sampai = guna2SampaiTanggal.Value.Date;

            DataView dv = dtReservasi.DefaultView;

            dv.RowFilter =
                $"check_in >= '#{dari:yyyy-MM-dd}#' AND check_out <= '#{sampai:yyyy-MM-dd}#'";

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

        // ======================================================
        //                      RESET FILTER
        // ======================================================
        private void guna2Reset_Click(object sender, EventArgs e)
        {
            guna2Cari.Text = "";
            guna2DariTanggal.Value = DateTime.Now;
            guna2SampaiTanggal.Value = DateTime.Now;

            dtReservasi.DefaultView.RowFilter = "";
            guna2DataGridView1.DataSource = dtReservasi;
        }

        // ======================================================
        //                 EXPORT EXCEL OWNER
        // ======================================================
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Reservasi_Owner.xlsx";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(dtReservasi, "Reservasi Owner");

                        ws.Column(5).Style.DateFormat.Format = "dd MMMM yyyy";
                        ws.Column(6).Style.DateFormat.Format = "dd MMMM yyyy";

                        ws.Columns().AdjustToContents();

                        wb.SaveAs(save.FileName);
                    }

                    MessageBox.Show("Export berhasil!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal export: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Owner read-only
        }
    }
}

