using college_of_health_sciences.system_forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace college_of_health_sciences
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // عرض الـ SplashScreen أولًا
            SplashScreen splash = new SplashScreen();
            splash.ShowDialog(); // <-- ShowDialog يوقف Main حتى يغلق Splash

            // بعد إغلاق الـ Splash، افتح LoginForm
            Application.Run(new exams_form());
        }
    }
}
