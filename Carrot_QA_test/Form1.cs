using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using System.Diagnostics;
using Timer = System.Timers.Timer;

namespace Carrot_QA_test
{
    public partial class Form1 : Form
    {
        static UInt32 FIRMWARE_MAJOR_MASK = 0x10000000;
        static UInt32 FIRMWARE_MINOR_MASK = 0x00010000;
        static UInt32 FIRMWARE_PATCH_MASK = 0x00000001;
        /** BLE watcher */
        private BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();

        /** scanning state */
        private bool watchStarted = false;

        /** search filter */
        private string filter = String.Empty;

        /** list of showned tag linked to display */
        public ObservableCollection<Taginfo> tagColl = new ObservableCollection<Taginfo>();

        /** dictionnary of known tag */
        public Dictionary<string, Taginfo> tagList = new Dictionary<string, Taginfo>();

        /** search filter */
        private String _count = "0";

        private int _passCount = 0;
        private int modeFlag = 0;
        Timer listTimer;
        Timer dbTimer;
        Mydb mydb = new Mydb();

        public int ServerVersion { get; private set; }

        public static string GetInternalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        public static string GetExternalIPAddress()
        {
            string externalip = new WebClient().DownloadString("http://ipinfo.io/ip").Trim();

            if (String.IsNullOrWhiteSpace(externalip))
            {
                externalip = GetInternalIPAddress();//null경우 Get Internal IP를 가져오게 한다.
            }

            return externalip;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnStartBle = new System.Windows.Forms.Button();
            this.btnClearBle = new System.Windows.Forms.Button();
            this.BtnUpload = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.IMEI = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CCID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BLE = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RSSI = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Pass = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DB = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Reg = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Note = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listTimer = new System.Timers.Timer();
            this.dbTimer = new System.Timers.Timer();
            this.label4 = new System.Windows.Forms.Label();
            this.modeLabel = new System.Windows.Forms.Label();
            this.BtnMode = new System.Windows.Forms.Button();
            this.PassCount = new System.Windows.Forms.Label();
            this.Count = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.VersionCheck = new System.Windows.Forms.CheckBox();
            this.ServerVersionText = new System.Windows.Forms.Label();
            this.Scan = new System.Windows.Forms.Button();
            this.ble_label = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.listTimer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbTimer)).BeginInit();
            this.SuspendLayout();
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("굴림", 12F);
            this.txtSearch.Location = new System.Drawing.Point(667, 8);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(180, 35);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            // 
            // btnStartBle
            // 
            this.btnStartBle.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.btnStartBle.Location = new System.Drawing.Point(668, 40);
            this.btnStartBle.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnStartBle.Name = "btnStartBle";
            this.btnStartBle.Size = new System.Drawing.Size(77, 28);
            this.btnStartBle.TabIndex = 1;
            this.btnStartBle.Text = "Start";
            this.btnStartBle.UseVisualStyleBackColor = true;
            this.btnStartBle.Click += new System.EventHandler(this.BtnStartBle_Click);
            // 
            // btnClearBle
            // 
            this.btnClearBle.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.btnClearBle.Location = new System.Drawing.Point(749, 40);
            this.btnClearBle.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnClearBle.Name = "btnClearBle";
            this.btnClearBle.Size = new System.Drawing.Size(70, 28);
            this.btnClearBle.TabIndex = 2;
            this.btnClearBle.Text = "Clear";
            this.btnClearBle.UseVisualStyleBackColor = true;
            this.btnClearBle.Click += new System.EventHandler(this.BtnClearBle_Click);
            // 
            // BtnUpload
            // 
            this.BtnUpload.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.BtnUpload.Location = new System.Drawing.Point(822, 40);
            this.BtnUpload.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.BtnUpload.Name = "BtnUpload";
            this.BtnUpload.Size = new System.Drawing.Size(73, 28);
            this.BtnUpload.TabIndex = 3;
            this.BtnUpload.Text = "Upload";
            this.BtnUpload.UseVisualStyleBackColor = true;
            this.BtnUpload.Click += new System.EventHandler(this.BtnUpload_Click);
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(899, 40);
            this.btnSave.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(73, 28);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.IMEI,
            this.CCID,
            this.BLE,
            this.RSSI,
            this.Pass,
            this.DB,
            this.Reg,
            this.Note});
            this.listView1.Font = new System.Drawing.Font("굴림", 12F);
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(9, 74);
            this.listView1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(964, 676);
            this.listView1.TabIndex = 5;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
            // 
            // IMEI
            // 
            this.IMEI.Text = "IMEI";
            this.IMEI.Width = 155;
            // 
            // CCID
            // 
            this.CCID.Text = "CCID";
            this.CCID.Width = 230;
            // 
            // BLE
            // 
            this.BLE.Text = "BLE UUID";
            this.BLE.Width = 100;
            // 
            // RSSI
            // 
            this.RSSI.Text = "RSSI";
            this.RSSI.Width = 80;
            // 
            // Pass
            // 
            this.Pass.Text = "Pass";
            // 
            // DB
            // 
            this.DB.Text = "DB";
            // 
            // Reg
            // 
            this.Reg.Text = "Reg";
            // 
            // Note
            // 
            this.Note.Text = "Note";
            this.Note.Width = 600;
            // 
            // listTimer
            // 
            this.listTimer.Enabled = true;
            this.listTimer.Interval = 3000D;
            this.listTimer.SynchronizingObject = this;
            this.listTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.ListTimeUp);
            // 
            // dbTimer
            // 
            this.dbTimer.Enabled = true;
            this.dbTimer.Interval = 1000D;
            this.dbTimer.SynchronizingObject = this;
            this.dbTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.DbTimeUp);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("굴림", 12F);
            this.label4.Location = new System.Drawing.Point(540, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(181, 24);
            this.label4.TabIndex = 12;
            this.label4.Text = "BLE Connect ID:";
            // 
            // modeLabel
            // 
            this.modeLabel.AutoSize = true;
            this.modeLabel.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.modeLabel.Location = new System.Drawing.Point(8, 12);
            this.modeLabel.Name = "modeLabel";
            this.modeLabel.Size = new System.Drawing.Size(367, 24);
            this.modeLabel.TabIndex = 13;
            this.modeLabel.Text = "Carrot Plug v1.2 QA Test Mode";
            // 
            // BtnMode
            // 
            this.BtnMode.Font = new System.Drawing.Font("굴림", 12F);
            this.BtnMode.Location = new System.Drawing.Point(284, 11);
            this.BtnMode.Name = "BtnMode";
            this.BtnMode.Size = new System.Drawing.Size(126, 23);
            this.BtnMode.TabIndex = 14;
            this.BtnMode.Text = "Mode Change";
            this.BtnMode.UseVisualStyleBackColor = true;
            this.BtnMode.Click += new System.EventHandler(this.BtnMode_Click);
            // 
            // PassCount
            // 
            this.PassCount.AutoSize = true;
            this.PassCount.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.PassCount.Location = new System.Drawing.Point(346, 44);
            this.PassCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.PassCount.Name = "PassCount";
            this.PassCount.Size = new System.Drawing.Size(24, 24);
            this.PassCount.TabIndex = 11;
            this.PassCount.Text = "-";
            // 
            // Count
            // 
            this.Count.AutoSize = true;
            this.Count.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.Count.Location = new System.Drawing.Point(252, 44);
            this.Count.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Count.Name = "Count";
            this.Count.Size = new System.Drawing.Size(24, 24);
            this.Count.TabIndex = 10;
            this.Count.Text = "-";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("굴림", 12F);
            this.label2.Location = new System.Drawing.Point(110, 44);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(187, 24);
            this.label2.TabIndex = 8;
            this.label2.Text = "Number of Tag : ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("굴림", 12F);
            this.label3.Location = new System.Drawing.Point(280, 44);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 24);
            this.label3.TabIndex = 9;
            this.label3.Text = "Pass :";
            // 
            // VersionCheck
            // 
            this.VersionCheck.AutoSize = true;
            this.VersionCheck.Checked = true;
            this.VersionCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.VersionCheck.Location = new System.Drawing.Point(415, 15);
            this.VersionCheck.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.VersionCheck.Name = "VersionCheck";
            this.VersionCheck.Size = new System.Drawing.Size(111, 21);
            this.VersionCheck.TabIndex = 15;
            this.VersionCheck.Text = "버전 확인 모드";
            this.VersionCheck.UseVisualStyleBackColor = true;
            // 
            // ServerVersionText
            // 
            this.ServerVersionText.AutoSize = true;
            this.ServerVersionText.Font = new System.Drawing.Font("굴림", 12F);
            this.ServerVersionText.Location = new System.Drawing.Point(412, 44);
            this.ServerVersionText.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ServerVersionText.Name = "ServerVersionText";
            this.ServerVersionText.Size = new System.Drawing.Size(201, 24);
            this.ServerVersionText.TabIndex = 16;
            this.ServerVersionText.Text = "Firmware Version :";
            // 
            // Scan
            // 
            this.Scan.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.Scan.Location = new System.Drawing.Point(11, 38);
            this.Scan.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Scan.Name = "Scan";
            this.Scan.Size = new System.Drawing.Size(94, 28);
            this.Scan.TabIndex = 17;
            this.Scan.Text = "QR Scan";
            this.Scan.UseVisualStyleBackColor = true;
            this.Scan.Click += new System.EventHandler(this.Scan_Click);
            // 
            // ble_label
            // 
            this.ble_label.AutoSize = true;
            this.ble_label.Font = new System.Drawing.Font("굴림", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ble_label.Location = new System.Drawing.Point(850, 12);
            this.ble_label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ble_label.Name = "ble_label";
            this.ble_label.Size = new System.Drawing.Size(80, 24);
            this.ble_label.TabIndex = 18;
            this.ble_label.Text = "Ready";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(983, 761);
            this.Controls.Add(this.ble_label);
            this.Controls.Add(this.Scan);
            this.Controls.Add(this.ServerVersionText);
            this.Controls.Add(this.VersionCheck);
            this.Controls.Add(this.BtnMode);
            this.Controls.Add(this.modeLabel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.PassCount);
            this.Controls.Add(this.Count);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.BtnUpload);
            this.Controls.Add(this.btnClearBle);
            this.Controls.Add(this.btnStartBle);
            this.Controls.Add(this.txtSearch);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Carrot QA Program";
            ((System.ComponentModel.ISupportInitialize)(this.listTimer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbTimer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        int bleState = 0;

        private void DbTimeUp(object source, ElapsedEventArgs e)
        {
            try
            {
                if (ServerVersion == 0 )
                {
                    string url = "https://s3.ap-northeast-2.amazonaws.com/iot.luxrobo.com/Version_factory.txt";
                    string responseText = string.Empty;

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Timeout = 30 * 1000; // 30초

                    using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
                    {
                        HttpStatusCode status = resp.StatusCode;
                        Stream respStream = resp.GetResponseStream();
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            responseText = sr.ReadToEnd();
                        }

                        ServerVersion = int.Parse(responseText.Split('\n')[0]);
                        string majorVersion = ((uint)Convert.ToInt32(ServerVersion / FIRMWARE_MAJOR_MASK)).ToString();
                        string minorVersion = ((uint)Convert.ToInt32((ServerVersion % FIRMWARE_MAJOR_MASK) / FIRMWARE_MINOR_MASK)).ToString();
                        string patchVersion = ((uint)Convert.ToInt32((ServerVersion % FIRMWARE_MINOR_MASK) / FIRMWARE_PATCH_MASK)).ToString();
                        this.ServerVersionText.Text = "Firmware Version : v"+ majorVersion + "."+ minorVersion + "." + patchVersion;
                    }
                }
                string ip = GetExternalIPAddress();
                /*
                 * 생산현장 
                    고정 IP : 112.216.234.42
                    고정 IP : 112.216.234.43
                    고정 IP : 112.216.234.44  총 3개

                    GPS검사실 
                    고정 IP : 106.245.254.26

                    개통검사실
                    ​고정 IP : 112.216.238.122
                    
                    럭스로보 연구소
                    고정 IP : 175.209.190.173
                */
                if (!(ip == "175.209.190.173" || ip == "112.216.238.122" || ip == "106.245.254.26" || ip == "112.216.234.42" || ip == "112.216.234.43" || ip == "112.216.234.44"))
                {
                    this.dbTimer.Stop();
                    if (MessageBox.Show("IP 주소가 다릅니다. VPN과 인터넷 상태를 점검해주세요.","Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                    this.dbTimer.Start();
                }
                Taginfo tagTimeout = null;
                foreach (Taginfo tag in tagColl)
                {

                    if (tag.passFlagUpdate)
                    {
                        string icc_id = tag.TagIccID;
                        string imei = tag.TagIMEI;
                        int result;
                        if (modeFlag == 0)
                            result = mydb.UpdateQuery_qa2(imei, icc_id, tag.passFlag, tag.TagFlagString, tag.TagBleID);
                        else
                            result = mydb.UpdateQuery_qa3(imei, icc_id, tag.passFlag, tag.TagFlagString, tag.TagBleID);

                        if (result == 1)
                            tag.dbString = "OK";
                        else
                            tag.dbString = "Fail";
                        if (modeFlag == 1 || (modeFlag == 0 && tag.passFlag == "OK"))
                        {
                            int let = mydb.regist_server(imei);
                            if (let == 1)
                            {
                                tag.serverString = "OK";
                            }
                            else if (let == -1)
                            {
                                tag.serverString = "DB Fail";
                            }
                            else if (let == -2)
                            {
                                tag.serverString = "TS Fail";
                            }
                            else if (let == -3)
                            {
                                tag.serverString = "MS Fail";
                            }
                            else if (let == -4)
                            {
                                tag.serverString = "Time Out";
                                this.dbTimer.Stop();
                                if (MessageBox.Show("인터넷 연결 상태가 좋지 않습니다. 점검 해주세요.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                                {
                                    Application.Exit();
                                }
                                this.dbTimer.Start();
                            }
                        }
                        tag.passFlagUpdate = false;
                    }

                    TimeSpan timeDiff = DateTime.Now - tag.updateTime;
                    if((tag.TagRssi < -120) && (timeDiff.Seconds > 5))
                    {
                        tagTimeout = tag;
                        continue;
                    }
                    if (timeDiff.Seconds > 20)
                    {
                        tagTimeout = tag;
                        continue;
                    }
                }

                if (tagTimeout != null)
                {
                    tagList.Remove(tagTimeout.TagMac);
                    tagColl.Remove(tagTimeout);
                }
            }
            catch
            {

            }
        }
        private void ListTimeUp(object source, ElapsedEventArgs e)
        {
            if (this.watchStarted == true)
            {
                try
                {
                    listView1.BeginUpdate();
                    listView1.Items.Clear();
                    try
                    {
                        foreach (Taginfo tag in tagColl)
                        {
                            ListViewItem LVI = new ListViewItem(tag.TagIMEI);
                            LVI.SubItems.Add(tag.TagIccID);
                            LVI.SubItems.Add(tag.TagBleID.Substring(tag.TagBleID.Length - 12, 12));
                            LVI.SubItems.Add(Convert.ToString(tag.TagRssi));
                            LVI.SubItems.Add(tag.passFlag);
                            LVI.SubItems.Add(tag.dbString);
                            LVI.SubItems.Add(tag.serverString);
                            LVI.SubItems.Add(Convert.ToString(tag.TagFlagString));
                            listView1.Items.Add(LVI);
                        }
                    }
                    catch
                    {
                    }
                    _count = Convert.ToString(tagColl.Count);
                    Count.Text = _count;
                    PassCount.Text = Convert.ToString(_passCount);
                }
                finally
                {
                    listView1.EndUpdate();
                    Application.DoEvents();
                }
            }
        }
        private void BtnStartBle_Click(object sender, EventArgs e)
        {
            //start scanning
            if (this.watchStarted == false)
            {

                //if (this.modeFlag == 0)
                //    this.watcher.AdvertisementFilter.Advertisement.LocalName = "Q";
                //else
                //    this.watcher.AdvertisementFilter.Advertisement.LocalName = "O";
                this.watcher.Received += Tag_Received;
                this.watcher.ScanningMode = BluetoothLEScanningMode.Active;
                this.watcher.Start();
                this.watchStarted = true;

                //update pictogram
                this.btnStartBle.Text = "Stop";
                
            }
            //stop scanning
            else if (this.watchStarted == true)
            {
                this.watcher.Stop();
                this.watcher.Received -= Tag_Received;
                this.watchStarted = false;

                //update pictogram
                this.btnStartBle.Text = "Start";
            }
        }

        private void BtnMode_Click(object sender, EventArgs e)
        {
            if (this.modeFlag == 0)
            {
                this.modeFlag = 1;
                this.modeLabel.Text = "Carrot Plug v1.2 개통 Test Mode";
            }
            else
            {
                this.modeFlag = 0;
                this.modeLabel.Text = "Carrot Plug v1.2 QA Test Mode";
            }

            this.listView1.BeginUpdate();
            this.listView1.Items.Clear();
            this.listView1.EndUpdate();
            this.tagList.Clear();
            this.tagColl.Clear();
            Count.Text = "0";
            PassCount.Text = "0";
            _passCount = 0;

            if (this.watchStarted == true)
            {
                this.watcher.Stop();
                this.watcher.Received -= Tag_Received;
                this.watchStarted = false;

                //update pictogram
                this.btnStartBle.Text = "Start";
            }
            Application.DoEvents();
        }


        private void BtnClearBle_Click(object sender, EventArgs e)
        {
            this.listView1.BeginUpdate();
            this.listView1.Items.Clear();
            this.listView1.EndUpdate();
            this.tagList.Clear();
            this.tagColl.Clear();
            Count.Text = "0";
            PassCount.Text = "0";
            _passCount = 0;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "csv file|*.csv";
            saveFileDialog.Title = "Save an csv File";
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                System.IO.FileStream fs =
                    (System.IO.FileStream)saveFileDialog.OpenFile();
                StreamWriter sw = new StreamWriter(fs,Encoding.UTF8);

                String WriteLineBuffer = "";

                foreach (Taginfo tag in tagList.Values)
                {
                    if (tag.CarrotPlugFlag)
                    {
                        String WriteLine = tag.TagName + '\t' + tag.TagMenu;
                        WriteLineBuffer += WriteLine;
                        sw.WriteLine(WriteLine);
                    }
                }
                sw.Close();
                sw.Dispose();
            }

        }


        private void BtnUpload_Click(object sender, EventArgs e)
        {
            string msg = "";
            foreach (Taginfo tag in tagList.Values)
            {
                if (tag.CarrotPlugFlag)
                {
                    string icc_id = tag.TagMenu;
                    string imei = tag.TagName;
                    int result;
                    if (modeFlag == 0)
                        result = mydb.UpdateQuery_qa2(imei, icc_id, tag.passFlag, tag.TagFlagString, tag.TagBleID);
                    else
                        result = mydb.UpdateQuery_qa3(imei, icc_id, tag.passFlag, tag.TagFlagString, tag.TagBleID);

                    string result_string = "";
                    if (result == 1)
                        result_string = "OK";
                    else
                        result_string = "Fail";
                    string reg_result = "Fail";
                    int let = mydb.regist_server(imei);
                    if (let == 1)
                    {
                        reg_result = "Test, Main Server Register!";
                        tag.dbString = "OK";
                        tag.serverString = "OK";
                    }
                    else if (let == -1)
                    {
                        reg_result = "Not find DB data";
                        tag.dbString = "Fail";
                        tag.serverString = "Fail";

                    }
                    else if (let == -2)
                    {
                        reg_result = "Not Connect Test Server, check your IP(use VPN)";
                        tag.dbString = "OK";
                        tag.serverString = "Fail";
                    }
                    else if (let == -3)
                    {
                        reg_result = "Not Connect Main Server, check your IP(use VPN)";
                        tag.dbString = "OK";
                        tag.serverString = "Fail";
                    }
                    msg += imei + " Upload Result :" + result_string + ", " + reg_result + "\n";
                    tag.passFlagUpdate = false;
                }
            }
            MessageBox.Show(msg);
        }


        private void Scan_Click(object sender, EventArgs e)
        {
            Form2 dlg = new Form2(ServerVersion.ToString());
            dlg.Show();
        }

        private async void Tag_Received(BluetoothLEAdvertisementWatcher received, BluetoothLEAdvertisementReceivedEventArgs args)
        {

            //show only connectable tags
            //if (args.AdvertisementType == BluetoothLEAdvertisementType.NonConnectableUndirected || args.AdvertisementType == BluetoothLEAdvertisementType.ConnectableUndirected || args.AdvertisementType == BluetoothLEAdvertisementType.ScanResponse)
            {
                //get tag infos
                Taginfo taginfo = new Taginfo();
                taginfo.TagMac = args.BluetoothAddress.ToString("X");
                taginfo.TagName = args.Advertisement.LocalName;
                taginfo.updateTime = DateTime.Now;
                taginfo.CarrotPlugFlag = false;
                //ulong blAddress = args.BluetoothAddress;
                //BluetoothDevice blDevice = await Windows.Devices.Bluetooth.BluetoothDevice.FromBluetoothAddressAsync(blAddress);
                //Debug.WriteLine("ble device :" + taginfo.TagName);

                //get tag datas
                string datasection = String.Empty;
                if(taginfo.TagName.Length == 1 && ( taginfo.TagName == "O" || taginfo.TagName == "Q") || args.AdvertisementType == BluetoothLEAdvertisementType.ScanResponse)
                //if (taginfo.TagName.Length == 1 && (taginfo.TagName == "O" || taginfo.TagName == "Q"))
                    {
                    foreach (BluetoothLEAdvertisementDataSection section in args.Advertisement.DataSections)
                    {
                        var data = new byte[section.Data.Length];
                        using (var reader = DataReader.FromBuffer(section.Data))
                        {
                            reader.ReadBytes(data);
                            datasection = String.Format("{0}", BitConverter.ToString(data));
                            taginfo.TagDataRaw.Add(datasection);
                            taginfo.TagRssi = args.RawSignalStrengthInDBm;
                            Debug.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " Find device :" + taginfo.TagMac + "("+ args.AdvertisementType.ToString() + ") L:" + section.Data.Length +" Data: " + datasection);
                            try
                            {
                                if (section.Data.Length > 6)
                                {
                                    // 
                                    taginfo.btAddress = args.BluetoothAddress;
                                    taginfo.TagMenu = taginfo.TagDataRaw[2].Replace("-", "");
                                    string majorVersion = ((uint)Convert.ToInt32(taginfo.TagMenu.Substring(2, 1), 16)).ToString();
                                    string minorVersion = ((uint)Convert.ToInt32(taginfo.TagMenu.Substring(3, 1), 16)).ToString();
                                    string patchVersion = ((uint)Convert.ToInt32(taginfo.TagMenu.Substring(0, 2), 16)).ToString();
                                    taginfo.TagVersionNumber = Convert.ToUInt32(majorVersion) * FIRMWARE_MAJOR_MASK;
                                    taginfo.TagVersionNumber += Convert.ToUInt32(minorVersion) * FIRMWARE_MINOR_MASK;
                                    taginfo.TagVersionNumber += Convert.ToUInt32(patchVersion) * FIRMWARE_PATCH_MASK;
                                    taginfo.TagVersion = 'v' + majorVersion + '.' + minorVersion + '.' + patchVersion;                  // Firmware Version
                                    taginfo.TagIccID = "898205" + taginfo.TagMenu.Substring(4, 13);                                     // ICC ID
                                    taginfo.TagFlag = (uint)Convert.ToInt32(taginfo.TagMenu.Substring(17, 1), 16);                      // QA Flag    
                                    taginfo.TagBleID = "4C520000-E25D-11EB-BA80-" + taginfo.TagMenu.Substring(18, 12);                  // BLE UUID
                                    taginfo.TagIMEI = "3596271" + taginfo.TagMenu.Substring(30, 8);                                     // IMEI
                                    taginfo.TagFlagString = taginfo.TagVersion + " ";
                                    if (modeFlag == 0 && taginfo.TagName == "Q")
                                    {
                                        if ((taginfo.TagFlag & 0x01) == 0x01)
                                            taginfo.TagFlagString += "GPS Fail";
                                        else
                                            taginfo.TagFlagString += "GPS OK";
                                        taginfo.TagFlagString += ", ";
                                        if ((taginfo.TagFlag & 0x02) == 0x02)
                                            taginfo.TagFlagString += "BLE Fail";
                                        else
                                            taginfo.TagFlagString += "BLE OK";
                                        taginfo.TagFlagString += ", ";
                                        if ((taginfo.TagFlag & 0x0C) == 0x08)
                                            taginfo.TagFlagString += "CAP Low";
                                        else if ((taginfo.TagFlag & 0x0C) == 0x04)
                                            taginfo.TagFlagString += "CAP Over";
                                        else
                                            taginfo.TagFlagString += "CAP OK";
                                        taginfo.TagFlagString += ", ";
                                        if ((taginfo.TagFlag & 0xF0) != 0x00)
                                            taginfo.TagFlagString += "LTE Fail";
                                        else
                                            taginfo.TagFlagString += "LTE OK";
                                        if ((taginfo.TagFlag & 0xFF) == 0)
                                        {
                                            taginfo.passFlag = "OK";
                                        }
                                        taginfo.CarrotPlugFlag = true;
                                    }
                                    else if (modeFlag == 1 && taginfo.TagName == "O")
                                    {
                                        string progressString = "none";
                                        switch (taginfo.TagFlag)
                                        {
                                            case 0x0:
                                                progressString = "Booting -> CCID/IMEI check";
                                                taginfo.passFlag = "NG";
                                                break;
                                            case 0x1:
                                                progressString = "CCID/IMEI OK -> Phone number";
                                                taginfo.passFlag = "NG";
                                                break;
                                            case 0x2:
                                                progressString = "Phone number OK -> FOTA";
                                                taginfo.passFlag = "NG";
                                                break;
                                            case 0x3:
                                                progressString = "FOTA OK -> Provisioning";
                                                taginfo.passFlag = "NG";
                                                break;
                                            case 0x4:
                                                progressString = "Provisioning OK -> MQTT Connect";
                                                taginfo.passFlag = "NG";
                                                break;
                                            case 0xF:
                                                progressString = "Open Success";
                                                taginfo.passFlag = "NG";
                                                if (VersionCheck.Checked)
                                                {
                                                    if (ServerVersion <= taginfo.TagVersionNumber)
                                                    {
                                                        taginfo.passFlag = "OK";
                                                    }
                                                }
                                                else
                                                {
                                                    taginfo.passFlag = "OK";
                                                }
                                                break;
                                        }
                                        taginfo.TagFlagString += " " + progressString;
                                        taginfo.CarrotPlugFlag = true;
                                    }
                                }
                                else
                                {
                                    taginfo.TagMenu = "";
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }

                if (taginfo.CarrotPlugFlag)
                {
                    //add new tag
                    if (this.tagList.ContainsKey(taginfo.TagMac) == false)
                    {
                        this.tagList.Add(taginfo.TagMac, taginfo);
                        if (Filter(taginfo))
                        {
                            tagColl.Add(taginfo);
                        }
                    }
                    //update existing tag infos
                    else if (tagList.ContainsValue(taginfo) == false)
                    {
                        IEnumerable<Taginfo> existing = tagColl.Where(x => x.TagMac == taginfo.TagMac);
                        int a = tagColl.IndexOf(existing.FirstOrDefault());
                        if (Filter(taginfo))
                        {
                            if (a >= 0 && a < tagColl.Count())
                            {
                                tagColl[a].update(taginfo);
                            }
                        }
                        this.tagList[taginfo.TagMac].update(taginfo);
                    }
                    _passCount = 0;
                    foreach (Taginfo tag in tagColl)
                    {
                        if (tag.passFlag == "OK")
                        {
                            _passCount++;
                        }
                    }
                }
            }
        }

        /** @brief : filter tag mac adress or name
         * @return : true if tag matches false otherwise
         */
                private bool Filter(Taginfo taginfo)
        {
            if (taginfo.TagMac.ToLower().Contains(filter.ToLower()) || taginfo.TagName.ToLower().Contains(filter.ToLower()))
            {
                return true;
            }
            return false;
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                ListViewItem lvitem = listView1.SelectedItems[0];
                txtSearch.Text = lvitem.SubItems[0].Text;
            }
        }

        private async void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                StringBuilder sb = new StringBuilder();
                string str = this.txtSearch.Text;
                foreach (char c in str)
                {
                    if ((c >= '0' && c <= '9'))
                    {
                        sb.Append(c);
                    }
                }
                if (txtSearch.Text.Length - 15 < 0)
                {

                    this.ble_label.Text = "Input Error";
                    return;
                }
                string imei = this.txtSearch.Text.Substring(txtSearch.Text.Length - 15, 15);
                this.txtSearch.Text = "";
                foreach (char ch in imei)
                {
                    if (ch < '0' || ch > '9')
                    {
                        this.ble_label.Text = "Input Error";
                        return;
                    }
                }
                ulong btAddress = 0;
                foreach (Taginfo tag in tagColl)
                {
                    if (tag.TagIMEI == imei)
                    {
                        btAddress = tag.btAddress;
                        break;
                    }
                }
                if (btAddress != 0)
                {
                    var device = await BluetoothLEDevice.FromBluetoothAddressAsync(btAddress);
                    //DeviceUnpairingResult dupr = await device.DeviceInformation.Pairing.UnpairAsync();
                    Debug.WriteLine($"BLEWATCHER Found: {imei}");
                    if (!device.DeviceInformation.Pairing.IsPaired)
                    {
                        Debug.WriteLine($"{device.Name} Try Pairing");

                        var result = await device.DeviceInformation.Pairing.PairAsync();
                        this.ble_label.Text = "Connecting";
                        DeviceUnpairingResult dupr = await device.DeviceInformation.Pairing.UnpairAsync();
                        Debug.WriteLine($"BLEWATCHER Pairing Complete");
                        bleState = 1;
                        return;
                    }
                }
                else
                {
                    this.ble_label.Text = "Not Find";
                }
            }

        }
    }
}
