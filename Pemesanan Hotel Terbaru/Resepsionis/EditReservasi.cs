using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace Pemesanan_Hotel_Terbaru.Resepsionis
{
    public partial class EditReservasi : Form
    {
        string idReservasi;

        public EditReservasi(string id)
        {
            InitializeComponent();
            idReservasi = id;
            LoadData();
        }

        private void LoadData()
        {
            using (MySqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = @"SELECT r.*, t.* 
                                 FROM reservasi r
                                 JOIN tamu t ON r.id_tamu = t.id_tamu
                                 WHERE r.id_reservasi=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", idReservasi);
                MySqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    guna2NamaTamu.Text = dr["nama_tamu"].ToString();
                    guna2NIK.Text = dr["nik"].ToString();
                    guna2Alamat.Text = dr["alamat"].ToString();
                    guna2NoHandphone.Text = dr["no_handphone"].ToString();
                    guna2Email.Text = dr["email"].ToString();
                    guna2Check_in.Value = Convert.ToDateTime(dr["check_in"]);
                    guna2Check_out.Value = Convert.ToDateTime(dr["check_out"]);
                }
            }
        }

        private void guna2Simpan_Click(object sender, EventArgs e)
        {
            using (MySqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = @"UPDATE tamu t
                                 JOIN reservasi r ON t.id_tamu = r.id_tamu
                                 SET t.nama_tamu=@nama, t.nik=@nik, t.alamat=@alamat, 
                                     t.no_handphone=@nohp, t.email=@email,
                                     r.check_in=@checkin, r.check_out=@checkout
                                 WHERE r.id_reservasi=@id";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@nama", guna2NamaTamu.Text);
                cmd.Parameters.AddWithValue("@nik", guna2NIK.Text);
                cmd.Parameters.AddWithValue("@alamat", guna2Alamat.Text);
                cmd.Parameters.AddWithValue("@nohp", guna2NoHandphone.Text);
                cmd.Parameters.AddWithValue("@email", guna2Email.Text);
                cmd.Parameters.AddWithValue("@checkin", guna2Check_in.Value.Date);
                cmd.Parameters.AddWithValue("@checkout", guna2Check_out.Value.Date);
                cmd.Parameters.AddWithValue("@id", idReservasi);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Data berhasil diperbarui!");
                this.Close();
            }
        }

        private void guna2Batal_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void guna2Deskripsi_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Email_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Check_out_ValueChanged(object sender, EventArgs e)
        {

        }

        private void guna2NoHandphone_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Check_in_ValueChanged(object sender, EventArgs e)
        {

        }

        private void guna2Alamat_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2NoKamar_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2NIK_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TipeKamar_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2NamaTamu_TextChanged(object sender, EventArgs e)
        {

        }

        private void EditReservasi_Load(object sender, EventArgs e)
        {

        }
    }
}
