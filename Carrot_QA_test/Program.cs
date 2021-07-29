using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Carrot_QA_test
{
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]

        static void Main()
        {
            try
            {
                int processCnt = 0;
                Process[] checkProcess = Process.GetProcesses();
                foreach (Process p in checkProcess)
                {
                    if (p.ProcessName.Equals("Carrot_QA_test") == true)
                        processCnt++;
                }
                if (processCnt > 1)
                {
                    MessageBox.Show("이미 실행중입니다.");
                    return;
                }
                else
                {

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program - Error");
            }
        }
    }
}
