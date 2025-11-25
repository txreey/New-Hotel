using MySql.Data.MySqlClient;

namespace Pemesanan_Hotel_Terbaru
{
    internal class Koneksi
    {
        public static MySqlConnection GetConnection()
        {
            // Ubah sesuai konfigurasi XAMPP / phpMyAdmin kamu
            string server = "localhost";
            string database = "hotelnew";
            string username = "root";
            string password = ""; // isi kalau phpMyAdmin kamu pakai password

            string connectionString = $"server={server};database={database};uid={username};pwd={password};";
            return new MySqlConnection(connectionString);
        }
    }
}

