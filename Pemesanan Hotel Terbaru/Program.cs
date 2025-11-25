using System;
using System.Windows.Forms;
using Pemesanan_Hotel_Terbaru.Admin;
using Pemesanan_Hotel_Terbaru.Owner;
using Pemesanan_Hotel_Terbaru.Resepsionis;

namespace Pemesanan_Hotel_Terbaru
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Login());
        }
    }
}
