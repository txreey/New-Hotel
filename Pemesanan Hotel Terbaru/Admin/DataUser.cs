using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;
using ClosedXML.Excel;
using System.IO;

namespace Pemesanan_Hotel_Terbaru.Admin
{
    public partial class DataUser : Form
    {
        private DataTable dtUser;   // DATA GLOBAL UNTUK SEARCH & EXPORT

        public DataUser()
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

        private void DataUser_Load(object sender, EventArgs e)
        {
            TampilkanDataUser();
        }

        private void TampilkanDataUser()
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

                    // Tambah tombol edit/delete jika belum ada
                    if (!guna2DataGridView1.Columns.Contains("Edit"))
                    {
                        DataGridViewButtonColumn btnEdit = new DataGridViewButtonColumn();
                        btnEdit.Name = "Edit";
                        btnEdit.HeaderText = "Edit";
                        btnEdit.Text = "✏️ Edit";
                        btnEdit.UseColumnTextForButtonValue = true;
                        guna2DataGridView1.Columns.Add(btnEdit);
                    }

                    if (!guna2DataGridView1.Columns.Contains("Delete"))
                    {
                        DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn();
                        btnDelete.Name = "Delete";
                        btnDelete.HeaderText = "Delete";
                        btnDelete.Text = "🗑️ Delete";
                        btnDelete.UseColumnTextForButtonValue = true;
                        guna2DataGridView1.Columns.Add(btnDelete);
                    }

                    guna2DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    guna2DataGridView1.RowHeadersVisible = false;
                    guna2DataGridView1.AllowUserToAddRows = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menampilkan data user: " + ex.Message);
            }
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string idUser = guna2DataGridView1.Rows[e.RowIndex].Cells["ID"].Value.ToString();

            // EDIT
            if (guna2DataGridView1.Columns[e.ColumnIndex].Name == "Edit")
            {
                EditUser editForm = new EditUser(idUser);
                editForm.ShowDialog();
                TampilkanDataUser();
            }

            // DELETE
            if (guna2DataGridView1.Columns[e.ColumnIndex].Name == "Delete")
            {
                DialogResult result = MessageBox.Show(
                    "Yakin ingin menghapus user ini?",
                    "Konfirmasi Hapus",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    HapusUser(idUser);
                    TampilkanDataUser();
                }
            }
        }

        private void HapusUser(string idUser)
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

                MessageBox.Show("User berhasil dihapus!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghapus user: " + ex.Message);
            }
        }

        // ======================
        //  🔍 FITUR SEARCH
        // ======================
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

        // ======================
        //  📤 EXPORT EXCEL (ClosedXML)
        // ======================
        private void guna2ExportExcel_Click(object sender, EventArgs e)
        {
            if (dtUser == null || dtUser.Rows.Count == 0)
            {
                MessageBox.Show("Tidak ada data untuk diexport!");
                return;
            }

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Excel Files (*.xlsx)|*.xlsx";
            save.FileName = "Data_User.xlsx";

            if (save.ShowDialog() == DialogResult.OK)
            {
                using (XLWorkbook wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Data User");

                    // Isi data dari DataTable
                    ws.Cell(1, 1).InsertTable(dtUser);

                    // Autofit semuanya
                    ws.Columns().AdjustToContents();

                    wb.SaveAs(save.FileName);
                }

                MessageBox.Show("Export Excel berhasil!");
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            TambahUser tambahUserForm = new TambahUser();
            this.Hide();
            tambahUserForm.ShowDialog();
            this.Close();
        }
    }
}
