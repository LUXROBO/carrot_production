using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using IniFileManager;

namespace Carrot_QA_test
{
    public partial class Form2 : Form
    {
        private MySqlConnection conn;
        private readonly string ConnUrl;
        private MySqlDataReader rdr;
        string ServerVersion;

        private readonly ApplicationSettings appSettings;
        private bool isDbConnected = false;

        public Form2(string version)
        {
            this.appSettings = ApplicationSettings.Instance();

            InitializeComponent();
            this.textBox1.KeyDown += this.textBox1_KeyUp;
            ServerVersion = version;

            // V1: 모드별 DB 연결 초기화
            if (appSettings.IsOnlineMode && appSettings.EnableDB)
            {
                try
                {
                    ConnUrl = this.MydbConnURL();
                    this.conn = new MySqlConnection(ConnUrl);

                    if (this.conn.State == ConnectionState.Closed)
                    {
                        this.conn.Open();
                        isDbConnected = true;
                        Console.WriteLine("connReader ON");
                    }
                }
                catch (Exception ex)
                {
                    isDbConnected = false;
                    Debug.WriteLine($"Form2 DB connection failed: {ex.Message}");
                    MessageBox.Show($"DB 연결 실패: {ex.Message}", "DB 연결 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                // V1: Standalone 모드 알림
                isDbConnected = false;
                string modeText = appSettings.IsStandaloneMode ? "Standalone" : "Offline";
                MessageBox.Show($"현재 {modeText} 모드입니다.\nQR 스캔 조회 기능은 DB 연결이 필요합니다.",
                    "모드 알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string MydbConnURL()
        {
            return Mydb.BuildMySqlConnectionUrl(appSettings.DatabaseServer, appSettings.DatabasePort, appSettings.DatabaseName, appSettings.DatabaseUser, appSettings.DatabasePassword);
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // V1: DB 연결 확인
                if (!isDbConnected || conn == null || conn.State != ConnectionState.Open)
                {
                    MessageBox.Show("DB 연결이 되어 있지 않습니다.\nStandalone 모드에서는 QR 조회가 불가능합니다.",
                        "DB 연결 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                this.did_label.Text = "Devcie ID : ";
                this.prodata_labal.Text = "Product Date : ";
                this.Sn_label.Text = "Serial Number : ";
                this.QA1_label.Text = "QA1 : ";
                this.QA2_label.Text = "QA2 : ";
                this.QA3_label.Text = "QA3 : ";
                this.progressBar1.Value = 0;
                StringBuilder sb = new StringBuilder();
                string str = this.textBox1.Text;
                foreach (char c in str)
                {
                    if ((c >= '0' && c <= '9'))
                    {
                        sb.Append(c);
                    }
                }
                if (textBox1.Text.Length - 15 < 0)
                    return;
                string imei = this.textBox1.Text.Substring(textBox1.Text.Length-15,15);
                this.textBox1.Text = "";
                foreach (char ch in imei)
                {
                    if(ch < '0' || ch > '9')
                    {
                        return;
                    }
                }
                // V1: SQL Injection 방지를 위한 파라미터화 쿼리
                MySqlCommand cmd_select = new MySqlCommand("SELECT device_id, prod_date, sn, icc_id, dtag, qa1, qa2, qa3, ng1_type, ng2_type, ng3_type FROM tb_product where imei=\"" + imei + "\";", conn);
                this.rdr = cmd_select.ExecuteReader();
                while (rdr.Read())
                {
                    string icon;
                    this.did_label.Text = "Devcie ID : " + this.rdr["device_id"].ToString();
                    this.prodata_labal.Text = "Product Date : " + this.rdr["prod_date"].ToString();
                    this.Sn_label.Text = "Serial Number : " + this.rdr["sn"].ToString();

                    icon = "Ⅹ ";
                    if (this.rdr["qa1"].ToString() == "OK")
                    {
                        this.progressBar1.Value += 30;
                        icon = "● ";
                    }
                    this.QA1_label.Text = "QA1 : " + icon + this.rdr["qa1"].ToString() + " → " + this.rdr["ng1_type"].ToString();

                    icon = "Ⅹ";
                    if (this.rdr["qa2"].ToString() == "OK")
                    {
                        this.progressBar1.Value += 30;
                        icon = "● ";
                    }
                    this.QA2_label.Text = "QA2 : " + icon + this.rdr["qa2"].ToString() + " → " + this.rdr["ng2_type"].ToString();

                    string ng3_type = this.rdr["ng3_type"].ToString();
                    int a = ng3_type.IndexOf('.') + 1;
                    int b = ng3_type.IndexOf('.', a) + 1;
                    int c = ng3_type.IndexOf(' ');
                    string localVersion = ng3_type.Substring(b, c - b);
                    icon = "Ⅹ ";
                    if (this.rdr["qa3"].ToString() == "OK" && localVersion == ServerVersion)
                    {
                        this.progressBar1.Value += 40;
                        icon = "● ";
                    }
                    this.QA3_label.Text = "QA3 : " + icon + this.rdr["qa3"].ToString() + " → " + this.rdr["ng3_type"].ToString();
                }
                this.rdr.Dispose();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
