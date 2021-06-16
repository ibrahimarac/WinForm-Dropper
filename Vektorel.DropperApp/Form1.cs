using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;

namespace Ibrahim.DropperApp
{
    public partial class Form1 : Form
    {
        string defaultCursorPath;
        bool dropperIsMoving = false;
        //Klavye ve Fare olaylarını takip edebilmek için kullanacağımız
        //MouseKeyboardActivityMonitor dll'i referans olarak eklenmektedir.
        MouseHookListener mouseListener;

        //Cursor'ı güncellemek için gerekli olan API metodu.
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        //Varolan Cursor'ı değiştirmeye çalışıyoruz.
        private void ChangeCursor(string curFile)
        {
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Cursors\", "Arrow", curFile);
            //Değişikliği güncelle
            SystemParametersInfo(0x0057, 0, IntPtr.Zero, 0);
        }

        //x ve y koordinatlarındaki pikseli alarak 
        //renk değerini hesaplıyoruz.
        public Color GetPixelColor(int x, int y)
        {
            Bitmap bmp = new Bitmap(1, 1);
            Graphics gr = Graphics.FromImage(bmp);
            gr.CopyFromScreen(x, y, 0, 0, new Size(1, 1));
            return bmp.GetPixel(0, 0);
        }
        
        public Form1()
        {
            InitializeComponent();

            mouseListener = new MouseHookListener(new GlobalHooker());

            //Mevcut Cursor'a ait yolu saklayalım.
            //Bu Cursor  HKEY_CURRENT_USER\Control Panel\Cursors 
            //regedit kaydına ait Arrow key değerini saklıyorum
            defaultCursorPath = Registry.CurrentUser.OpenSubKey("Control Panel").OpenSubKey("Cursors").GetValue("Arrow").ToString();
        }

        private void btnSelectColor_Click(object sender, EventArgs e)
        {
            //Butona tıklandığında yeni cursor'ı 
            //fare işaretçisi olarak değiştiriyorum.
            ChangeCursor(Application.StartupPath + "\\target.cur");
            //Renk seçicinin aktif olduğunu tespit etmek amacıyla
            //dropperIsMoving değişkenini kullanacağız.
            dropperIsMoving = true;
            //Klavye ve Fare takibi yapan sınıfımıza ait
            //olaylarımızı bağlıyoruz.
            mouseListener.MouseMove += MouseListener_MouseMove;
            mouseListener.MouseDown += MouseListener_MouseDown;
            //Klavye ve Fare dinleme sınıfları aktif hale getiriliyor.
            mouseListener.Enabled = true;
        }

        private void MouseListener_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button==MouseButtons.Left)
            {
                //Cursor'ı eski haline getiriyoruz.
                ChangeCursor(defaultCursorPath);
                //Renk seçiciyi pasif hale getiriyoruz.
                dropperIsMoving = false;
                //Kalvye ve Fare dinleyiciyi durduruyoruz.
                mouseListener.Enabled = false;
            }
        }
        
        private void MouseListener_MouseMove(object sender, MouseEventArgs e)
        {
            //Eğer dropper aktifse
            if(dropperIsMoving)
            {
                //Seçili pikselin rengini panelin erkaplanı yapıyoruz.
                pnlColor.BackColor = GetPixelColor(e.X + Cursor.Size.Width / 2, e.Y + Cursor.Size.Height / 2);
            }            
        }
        
    }
}
