using IniFileManager;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ConstrainedExecution;
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
                    // 구성파일(config.ini)로 부터 구성정보 가져오기
                    ApplicationSettings settings = AppConfigInit();

                    // 로그 시작 (로그 활성화 시)
                    AppLogInit();

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

        private static ApplicationSettings AppConfigInit()
        {
            const string configFileName = "config.ini";
            ApplicationSettings settings = ApplicationSettings.Instance(configFileName);

            settings.DisplaySettings();

            Console.WriteLine($"Version: {VersionManager.Version} {VersionManager.BuildDate}");

            return settings;
        }

        private static void AppLogInit()
        {
            const string configFileName = "config.ini";

            ApplicationSettings settings = ApplicationSettings.Instance(configFileName);

            if (!settings.EnableLogging)
            {
                return;
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string logFileName = $"trace_{timestamp}.log";
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);

            // 🔁 Trace 리스너 초기화
            Trace.Listeners.Clear();
            Trace.AutoFlush = true;

            // 📄 로그 파일에 append 모드로 열기
            StreamWriter writer = new StreamWriter(logFilePath, append: true);
            TextWriterTraceListener listener = new TextWriterTraceListener(writer);
            Trace.Listeners.Add(listener);
        }
    }
}
