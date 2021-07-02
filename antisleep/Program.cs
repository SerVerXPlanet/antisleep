using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;


namespace antisleep
{
    static class Program
    {
        const uint ES_CONTINUOUS = 0x80000000;
        const uint ES_AWAYMODE_REQUIRED = 0x00000040;
        const uint ES_SYSTEM_REQUIRED = 0x00000001;
        const uint ES_DISPLAY_REQUIRED = 0x00000002;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint SetThreadExecutionState(uint esFlags);

        [DllImport("user32.dll")]
        static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase); // RECT lpRect ---> IntPtr lpRect


        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool isNew;
            Mutex singleInstanceMutex = new Mutex(true, "AntiSleepSingleInstanceMutex", out isNew);

            if (isNew)
            {
                prevent_sleep();

                Application.Run();
            }
            else
            {
                allow_sleep();

                int currentId = Process.GetCurrentProcess().Id;

                string exe = AppDomain.CurrentDomain.FriendlyName;

                Process[] processes = Process.GetProcessesByName(exe.Replace(".exe", String.Empty));

                foreach (var process in processes)
                {
                    if (process.Id != currentId)
                        process.Kill();
                }

                Application.Exit();
            }
        }


        static void prevent_sleep()
        {
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_AWAYMODE_REQUIRED | ES_DISPLAY_REQUIRED);
            DrawMessage("antisleep started");
        }


        static void allow_sleep()
        {
            SetThreadExecutionState(ES_CONTINUOUS);
            DrawMessage("antisleep stopped");
        }


        static void DrawMessage(string msg)
        {
            const int FONT_SIZE = 26;

            Graphics gr = Graphics.FromHwnd(IntPtr.Zero);

            Font font = new Font(FontFamily.GenericMonospace, FONT_SIZE, FontStyle.Bold);

            SizeF sizeF = gr.MeasureString(msg, font);
            Size size = new Size((int)Math.Floor(sizeF.Width), (int)Math.Floor(sizeF.Height));

            LinearGradientBrush linGrBrush = new LinearGradientBrush(
                new Point(0, 0),
                new Point(80, 20),
                Color.FromArgb(255, 0, 255, 0),
                Color.DarkOrange);

            Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;
            int x = workingRectangle.Width / 2 - size.Width / 2;
            int y = workingRectangle.Height / 2 - size.Height / 2;
            Point p = new Point(x, y);
            
            DateTime end = DateTime.Now.AddSeconds(1);

            while (DateTime.Now < end)
            {
                gr.DrawString(msg, font, linGrBrush, p);
            }

            InvalidateRect(IntPtr.Zero, IntPtr.Zero, false);
        }


    }


}
