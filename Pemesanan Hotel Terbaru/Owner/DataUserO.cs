using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClosedXML.Excel;
using Pemesanan_Hotel_Terbaru.Admin;
using Pemesanan_Hotel_Terbaru.Owner;

namespace Pemesanan_Hotel_Terbaru.Owner
{
    public partial class DataUserO : Form
    {
        private DataTable dtUser;   // DATA GLOBAL UNTUK SEARCH & EXPORT

        public DataUserO()
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

            guna2Cari.TextChanged += guna2Cari_TextChanged;
            guna2ExportExcel.Click += guna2ExportExcel_Click;
            guna2Tambah.Click += guna2Tambah_Click;
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
                Login loginForm = new Login();
                loginForm.Show();
            }
        }

        private void DataUserO_Load(object sender, EventArgs e)
        {
            LoadDataUser();
        }

        // ========================================================
        //  LOAD DATA USER
        // ========================================================
        private void LoadDataUser()
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query =
                        "SELECT id_user AS 'ID', username AS 'Username', email AS 'Email', password AS 'Password', role AS 'Role' FROM user";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    dtUser = new DataTable();
                    adapter.Fill(dtUser);

                    guna2DataGridView1.DataSource = dtUser;
                }

                // Tambah tombol Edit jika belum ada
                if (!guna2DataGridView1.Columns.Contains("Edit"))
                {
                    DataGridViewButtonColumn btnEdit = new DataGridViewButtonColumn();
                    btnEdit.Name = "Edit";
                    btnEdit.HeaderText = "Edit";
                    btnEdit.Text = "✏️ Edit";
                    btnEdit.UseColumnTextForButtonValue = true;
                    guna2DataGridView1.Columns.Add(btnEdit);
                }

                // Tambah tombol Delete jika belum ada
                if (!guna2DataGridView1.Columns.Contains("Delete"))
                {
                    DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn();
                    btnDelete.Name = "Delete";
                    btnDelete.HeaderText = "Delete";
                    btnDelete.Text = "🗑️ Delete";
                    btnDelete.UseColumnTextForButtonValue = true;
                    guna2DataGridView1.Columns.Add(btnDelete);
                }

                // HILANGKAN ROW KOSONG
                guna2DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                guna2DataGridView1.RowHeadersVisible = false;
                guna2DataGridView1.AllowUserToAddRows = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data user: " + ex.Message);
            }
        }

        // ========================================================
        //  FITUR SEARCH — Sama seperti Admin
        // ========================================================
        private void guna2Cari_TextChanged(object sender, EventArgs e)
        {
            if (dtUser == null) return;

            string search = guna2Cari.Text.Trim();

            if (string.IsNullOrEmpty(search))
            {
                guna2DataGridView1.DataSource = dtUser;
            }
            else
            {
                DataView dv = dtUser.DefaultView;
                dv.RowFilter =
                    $"Username LIKE '%{search}%' OR Email LIKE '%{search}%' OR Role LIKE '%{search}%'";

                guna2DataGridView1.DataSource = dv.ToTable();
            }
        }

        // ========================================================
        //  EXPORT EXCEL — ClosedXML (Sama seperti Admin)
        // ========================================================
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            if (dtUser == null || dtUser.Rows.Count == 0)
            {
                MessageBox.Show("Tidak ada data untuk diexport!");
                return;
            }

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Excel Files (*.xlsx)|*.xlsx";
            save.FileName = "Data_User_Owner.xlsx";

            if (save.ShowDialog() == DialogResult.OK)
            {
                using (XLWorkbook wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Data User");
                    ws.Cell(1, 1).InsertTable(dtUser);

                    ws.Columns().AdjustToContents();

                    wb.SaveAs(save.FileName);
                }

                MessageBox.Show("Export Excel berhasil!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ========================================================
        //  EVENT CLICK EDIT / DELETE (DENGAN ALERT)
        // ========================================================
        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string idUser = guna2DataGridView1.Rows[e.RowIndex].Cells["ID"].Value.ToString();
            string username = guna2DataGridView1.Rows[e.RowIndex].Cells["Username"].Value.ToString();

            // ➤ EDIT
            if (guna2DataGridView1.Columns[e.ColumnIndex].Name == "Edit")
            {
                EditUserO editForm = new EditUserO(idUser);
                editForm.ShowDialog();
                LoadDataUser();
            }

            // ➤ DELETE (DENGAN KONFIRMASI DETAIL)
            if (guna2DataGridView1.Columns[e.ColumnIndex].Name == "Delete")
            {
                DialogResult result = MessageBox.Show(
                    $"Apakah Anda yakin ingin menghapus user ini?\n\n" +
                    $"Username: {username}\n\n" +
                    $"Data yang sudah dihapus tidak dapat dikembalikan!",
                    "Konfirmasi Hapus User",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    HapusUser(idUser, username);
                    LoadDataUser();
                }
            }
        }

        private void HapusUser(string idUser, string username)
        {
            try
            {
                using (MySqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM user WHERE id_user = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", idUser);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show(
                    $"User '{username}' berhasil dihapus!",
                    "Hapus Berhasil",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Gagal menghapus user: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // ➤ TOMBOL TAMBAH USER (Owner)
        private void guna2Tambah_Click(object sender, EventArgs e)
        {
            TambahUserO tambahUser = new TambahUserO();
            this.Hide();
            tambahUser.ShowDialog();
            this.Close();
        }
    }
}
