using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using MySql.Data.MySqlClient;

using ClosedXML.Excel;                         // ✔ WORKBOOK + EXCEL
using DocumentFormat.OpenXml.Packaging;        // ✔ diperlukan ClosedXML
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace Pemesanan_Hotel_Terbaru.Admin
{
    public partial class DataKamarA : Form
    {
        DataTable dataKamarTable;

        public DataKamarA()
        {
            InitializeComponent();
            guna2Dashboard.Click += (s, e) => OpenForm(new DashboardAdmin());
            guna2DataKamar.Click += (s, e) => OpenForm(new DataKamarA());
            guna2DataTamu.Click += (s, e) => OpenForm(new DataTamuA());
            guna2LaporanTransaksi.Click += (s, e) => OpenForm(new LaporanTransaksiA());
            guna2DataReservasi.Click += (s, e) => OpenForm(new DataReservasi());
            guna2DataUser.Click += (s, e) => OpenForm(new DataUser());
            guna2LaporanKeuangan.Click += (s, e) => OpenForm(new LaporanKeuangan2());
            guna2Logout.Click += (s, e) => Logout();

            guna2Tambah.Click += guna2Tambah_Click;
            guna2DataGridView1.CellContentClick += guna2DataGridView1_CellContentClick;
            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2ExportExcel.Click += guna2ExportExcel_Click;

            this.Load += DataKamarA_Load;
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

        private void DataKamarA_Load(object sender, EventArgs e)
        {
            LoadDataKamar();
        }

        private void LoadDataKamar()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM kamar";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);

                    dataKamarTable = new DataTable();
                    adapter.Fill(dataKamarTable);

                    DisplayData(dataKamarTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat data kamar: {ex.Message}");
            }
        }

        private void DisplayData(DataTable dt)
        {
            guna2DataGridView1.Rows.Clear();
            guna2DataGridView1.Columns.Clear();

            guna2DataGridView1.Columns.Add("colID", "ID");
            guna2DataGridView1.Columns.Add("colTipe", "Tipe Kamar");
            guna2DataGridView1.Columns.Add("colNo", "No Kamar");
            guna2DataGridView1.Columns.Add("colStatus", "Status");
            guna2DataGridView1.Columns.Add("colHarga", "Harga");
            guna2DataGridView1.Columns.Add("colDeskripsi", "Deskripsi");

            DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
            imgCol.Name = "colPicture";
            imgCol.HeaderText = "Gambar";
            imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
            guna2DataGridView1.Columns.Add(imgCol);

            DataGridViewButtonColumn editCol = new DataGridViewButtonColumn();
            editCol.Name = "colEdit";
            editCol.HeaderText = "Edit";
            editCol.Text = "Edit";
            editCol.UseColumnTextForButtonValue = true;
            guna2DataGridView1.Columns.Add(editCol);

            DataGridViewButtonColumn delCol = new DataGridViewButtonColumn();
            delCol.Name = "colDelete";
            delCol.HeaderText = "Hapus";
            delCol.Text = "Delete";
            delCol.UseColumnTextForButtonValue = true;
            guna2DataGridView1.Columns.Add(delCol);

            guna2DataGridView1.RowTemplate.Height = 80;
            guna2DataGridView1.AllowUserToAddRows = false;

            foreach (DataRow row in dt.Rows)
            {
                Image gambar = null;
                string path = row["picture"]?.ToString();

                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        gambar = Image.FromStream(fs);
                    }
                }

                guna2DataGridView1.Rows.Add(
                    row["id_kamar"],
                    row["tipe_kamar"],
                    row["no_kamar"],
                    row["status"],
                    row["harga"],
                    row["deskripsi"],
                    gambar,
                    "Edit",
                    "Hapus"
                );
            }
        }

        // 🔍 SEARCHING
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            string keyword = guna2Cari.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                DisplayData(dataKamarTable);
            }
            else
            {
                DataView dv = dataKamarTable.DefaultView;
                dv.RowFilter = $"tipe_kamar LIKE '%{keyword}%' OR no_kamar LIKE '%{keyword}%'";
                DisplayData(dv.ToTable());
            }
        }

        // 📌 EXPORT EXCEL — FIX GAMBAR 100% AMAN
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Excel File (*.xlsx)|*.xlsx";
                save.FileName = "Data_Kamar.xlsx";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    // BUAT DATATABLE SAMA SEPERTI RESERVASI
                    DataTable dt = new DataTable();
                    dt.Columns.Add("ID");
                    dt.Columns.Add("Tipe Kamar");
                    dt.Columns.Add("No Kamar");
                    dt.Columns.Add("Status");
                    dt.Columns.Add("Harga");
                    dt.Columns.Add("Deskripsi");

                    foreach (DataRow row in dataKamarTable.Rows)
                    {
                        dt.Rows.Add(
                            row["id_kamar"].ToString(),
                            row["tipe_kamar"].ToString(),
                            row["no_kamar"].ToString(),
                            row["status"].ToString(),
                            row["harga"].ToString(),
                            row["deskripsi"].ToString()
                        );
                    }

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(dt, "Data Kamar");

                        // Format harga seperti angka
                        ws.Column(5).Style.NumberFormat.Format = "#,##0";

                        // Autofit
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

        // ====================================================================
        // EDIT & DELETE - UPDATED WITH BETTER CONFIRMATION
        // ====================================================================
        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string col = guna2DataGridView1.Columns[e.ColumnIndex].Name;
            string id = guna2DataGridView1.Rows[e.RowIndex].Cells["colID"].Value.ToString();

            if (col == "colEdit")
            {
                this.Hide();
                new EditKamar(id).ShowDialog();
                this.Show();
                LoadDataKamar();
            }
            else if (col == "colDelete")
            {
                // AMBIL INFO KAMAR UNTUK KONFIRMASI
                string tipeKamar = guna2DataGridView1.Rows[e.RowIndex].Cells["colTipe"].Value.ToString();
                string noKamar = guna2DataGridView1.Rows[e.RowIndex].Cells["colNo"].Value.ToString();

                // KONFIRMASI DELETE DENGAN INFO DETAIL
                DialogResult confirm = MessageBox.Show(
                    $"Apakah Anda yakin ingin menghapus kamar ini?\n\n" +
                    $"Tipe Kamar: {tipeKamar}\n" +
                    $"No Kamar: {noKamar}\n\n" +
                    $"Data yang sudah dihapus tidak dapat dikembalikan!",
                    "Konfirmasi Hapus Kamar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = Koneksi.GetConnection())
                        {
                            conn.Open();
                            MySqlCommand cmd = new MySqlCommand("DELETE FROM kamar WHERE id_kamar=@id", conn);
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show(
                            $"Kamar {tipeKamar} - {noKamar} berhasil dihapus!",
                            "Hapus Berhasil",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        LoadDataKamar();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Gagal menghapus kamar: {ex.Message}",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            }
        }

        private void guna2Tambah_Click(object sender, EventArgs e)
        {
            using (TambahKamar t = new TambahKamar())
            {
                if (t.ShowDialog() == DialogResult.OK)
                {
                    LoadDataKamar();
                }
            }
        }
    }
}
