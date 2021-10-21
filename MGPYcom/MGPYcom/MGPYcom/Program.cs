using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MGPYcom
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form_main());
            Form_main view = new Form_main();
            view.StartPosition = FormStartPosition.CenterScreen;
            IController controller = new IController(view);

            Application.Run(view);
        }
    }
}
