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
using System.Reflection;
using static System.Collections.Specialized.BitVector32;
using Windows.UI;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Policy;
using System.Threading.Tasks;

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
        public Dictionary<string, Taginfo> tagSearch = new Dictionary<string, Taginfo>();

        /** search filter */
        private String _count = "0";

        private int _passCount = 0;
        private int modeFlag = 0;
        Timer listTimer;
        Timer dbTimer;
        Mydb mydb = new Mydb();
        string pVersion;
        
        BlePublisher bleSender = BlePublisher.Instance;

        UInt32 sleepDevImei = 0;

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
            string externalip = null;
            
            try
            {
                //externalip = new WebClient().DownloadString("http://ipinfo.io/ip").Trim();

                String reqHtml = new WebClient().DownloadString("http://checkip.dyndns.org/");
                reqHtml = reqHtml.Substring(reqHtml.IndexOf("Current IP Address:"));
                reqHtml = reqHtml.Substring(0, reqHtml.IndexOf("</body>"));
                externalip = reqHtml.Split(':')[1].Trim();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

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
            this.btnSave = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.IMEI = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CCID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BLE = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RSSI = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Pass = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this.Scan = new System.Windows.Forms.Button();
            this.ble_label = new System.Windows.Forms.Label();
            this.mode_label = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.listTimer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbTimer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("굴림", 12F);
            this.txtSearch.Location = new System.Drawing.Point(941, 6);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(2);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(205, 30);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            // 
            // btnStartBle
            // 
            this.btnStartBle.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.btnStartBle.Location = new System.Drawing.Point(787, 41);
            this.btnStartBle.Margin = new System.Windows.Forms.Padding(2);
            this.btnStartBle.Name = "btnStartBle";
            this.btnStartBle.Size = new System.Drawing.Size(88, 35);
            this.btnStartBle.TabIndex = 1;
            this.btnStartBle.Text = "Start";
            this.btnStartBle.UseVisualStyleBackColor = true;
            this.btnStartBle.Click += new System.EventHandler(this.BtnStartBle_Click);
            // 
            // btnClearBle
            // 
            this.btnClearBle.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.btnClearBle.Location = new System.Drawing.Point(896, 41);
            this.btnClearBle.Margin = new System.Windows.Forms.Padding(2);
            this.btnClearBle.Name = "btnClearBle";
            this.btnClearBle.Size = new System.Drawing.Size(166, 35);
            this.btnClearBle.TabIndex = 2;
            this.btnClearBle.Text = "Clear";
            this.btnClearBle.UseVisualStyleBackColor = true;
            this.btnClearBle.Click += new System.EventHandler(this.BtnClearBle_Click);
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(1066, 41);
            this.btnSave.Margin = new System.Windows.Forms.Padding(2);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(83, 35);
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
            this.Reg,
            this.Note});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Font = new System.Drawing.Font("굴림", 12F);
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Margin = new System.Windows.Forms.Padding(2);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1325, 891);
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
            this.label4.Location = new System.Drawing.Point(783, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(153, 20);
            this.label4.TabIndex = 12;
            this.label4.Text = "BLE Connect ID:";
            // 
            // modeLabel
            // 
            this.modeLabel.AutoSize = true;
            this.modeLabel.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.modeLabel.Location = new System.Drawing.Point(3, 10);
            this.modeLabel.Name = "modeLabel";
            this.modeLabel.Size = new System.Drawing.Size(241, 20);
            this.modeLabel.TabIndex = 13;
            this.modeLabel.Text = "Carrot Plug v3.7(BG770)";
            // 
            // BtnMode
            // 
            this.BtnMode.Font = new System.Drawing.Font("굴림", 12F);
            this.BtnMode.Location = new System.Drawing.Point(412, 6);
            this.BtnMode.Name = "BtnMode";
            this.BtnMode.Size = new System.Drawing.Size(144, 28);
            this.BtnMode.TabIndex = 14;
            this.BtnMode.Text = "Mode Change";
            this.BtnMode.UseVisualStyleBackColor = true;
            this.BtnMode.Click += new System.EventHandler(this.BtnMode_Click);
            // 
            // PassCount
            // 
            this.PassCount.AutoSize = true;
            this.PassCount.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.PassCount.Location = new System.Drawing.Point(408, 48);
            this.PassCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.PassCount.Name = "PassCount";
            this.PassCount.Size = new System.Drawing.Size(21, 20);
            this.PassCount.TabIndex = 11;
            this.PassCount.Text = "-";
            // 
            // Count
            // 
            this.Count.AutoSize = true;
            this.Count.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.Count.Location = new System.Drawing.Point(301, 48);
            this.Count.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Count.Name = "Count";
            this.Count.Size = new System.Drawing.Size(21, 20);
            this.Count.TabIndex = 10;
            this.Count.Text = "-";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("굴림", 12F);
            this.label2.Location = new System.Drawing.Point(141, 48);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(156, 20);
            this.label2.TabIndex = 8;
            this.label2.Text = "Number of Tag : ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("굴림", 12F);
            this.label3.Location = new System.Drawing.Point(339, 48);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 20);
            this.label3.TabIndex = 9;
            this.label3.Text = "Pass :";
            // 
            // Scan
            // 
            this.Scan.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.Scan.Location = new System.Drawing.Point(7, 41);
            this.Scan.Margin = new System.Windows.Forms.Padding(2);
            this.Scan.Name = "Scan";
            this.Scan.Size = new System.Drawing.Size(107, 35);
            this.Scan.TabIndex = 17;
            this.Scan.Text = "QR Scan";
            this.Scan.UseVisualStyleBackColor = true;
            this.Scan.Click += new System.EventHandler(this.Scan_Click);
            // 
            // ble_label
            // 
            this.ble_label.AutoSize = true;
            this.ble_label.Font = new System.Drawing.Font("굴림", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ble_label.Location = new System.Drawing.Point(1150, 14);
            this.ble_label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ble_label.Name = "ble_label";
            this.ble_label.Size = new System.Drawing.Size(67, 20);
            this.ble_label.TabIndex = 18;
            this.ble_label.Text = "Ready";
            // 
            // mode_label
            // 
            this.mode_label.AutoSize = true;
            this.mode_label.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.mode_label.Location = new System.Drawing.Point(256, 10);
            this.mode_label.Name = "mode_label";
            this.mode_label.Size = new System.Drawing.Size(148, 20);
            this.mode_label.TabIndex = 19;
            this.mode_label.Text = "QA Test Mode";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.modeLabel);
            this.splitContainer1.Panel1.Controls.Add(this.btnSave);
            this.splitContainer1.Panel1.Controls.Add(this.ble_label);
            this.splitContainer1.Panel1.Controls.Add(this.btnClearBle);
            this.splitContainer1.Panel1.Controls.Add(this.mode_label);
            this.splitContainer1.Panel1.Controls.Add(this.btnStartBle);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.BtnMode);
            this.splitContainer1.Panel1.Controls.Add(this.Scan);
            this.splitContainer1.Panel1.Controls.Add(this.txtSearch);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.Count);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.PassCount);
            this.splitContainer1.Panel1MinSize = 80;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView1);
            this.splitContainer1.Panel2MinSize = 100;
            this.splitContainer1.Size = new System.Drawing.Size(1325, 975);
            this.splitContainer1.SplitterDistance = 80;
            this.splitContainer1.SplitterIncrement = 4;
            this.splitContainer1.TabIndex = 20;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1325, 975);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Carrot QA Program";
            ((System.ComponentModel.ISupportInitialize)(this.listTimer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbTimer)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        int bleState = 0;

        private void DbTimeUp(object source, ElapsedEventArgs e)
        {
            try
            {
               /* if (ServerVersion == 0 )
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
                }*/
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
                    고정 IP : 112.169.63.43
                */
                if (!( ip== "112.169.63.43" || ip == "175.209.190.173" || ip == "112.216.238.122" || ip == "106.245.254.26" || ip == "112.216.234.42" || ip == "112.216.234.43" || ip == "112.216.234.44"))
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
                            result = mydb.UpdateQuery_qa2(imei, icc_id, tag.passFlag, tag.TagFlagString, tag.TagBleID, tag);
                        else
                            result = mydb.UpdateQuery_qa3(imei, icc_id, tag.passFlag, tag.TagFlagString, tag.TagBleID);

                        if (result == 1)
                        {
                            tag.dbString = "OK";
                        }
                        else
                        {
                            tag.dbString = "Fail";
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


                var sendingAdv = from tag in tagColl
                                    where tag.SendedImei == true
                                    select tag.SendedImei;

                Debug.WriteLine($"Count : {sendingAdv.Count()}");
                if(sendingAdv.Count() == 0)
                {
                    //db 전송이 OK, 검사결과가 OK인 목록을 오름차순 으로 추출
                    var tags = from tag in tagColl
                               where tag.dbString == "OK" && tag.passFlag == "OK"
                               orderby tag.updateTime ascending
                               select tag;
                    Debug.WriteLine($"Tags : {tags.Count()}");
                    if (tags.Count() > 0)
                    {
                        Taginfo item = tags.First();
                        item.SendedImei = true;
                        sleepDevImei = Convert.ToUInt32(item.TagIMEI.Substring(7), 16);
                        bleSender.SetImei(sleepDevImei);
                        Debug.WriteLine($"Sleep {item.TagIMEI}");
                    }
                }


                if (tagTimeout != null)
                {
                    tagList.Remove(tagTimeout.TagMac);
                    tagColl.Remove(tagTimeout);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
                            LVI.SubItems.Add(Convert.ToString(tag.TagFlagString));

                            LVI.UseItemStyleForSubItems = false;
                            if(!tag.passFlag.Contains("OK"))
                            {
                                LVI.SubItems[4].BackColor = System.Drawing.Color.Red;
                            }

                            if( !tag.dbString.Contains("OK") )
                            {
                                LVI.SubItems[5].BackColor = System.Drawing.Color.Red;
                            }
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
                this.mode_label.Text = "개통 Test Mode";
            }
            else
            {
                this.modeFlag = 0;
                this.mode_label.Text = " QA Test Mode";
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


        private void Scan_Click(object sender, EventArgs e)
        {
            Form2 dlg = new Form2(ServerVersion.ToString());
            dlg.Show();
        }

        private async void Tag_Received(BluetoothLEAdvertisementWatcher received, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            string macTemp = args.BluetoothAddress.ToString("X");

            //show only connectable tags
            if (args.Advertisement.LocalName == "O" || args.Advertisement.LocalName == "Q" || this.tagSearch.ContainsKey(macTemp))
            {

                //get tag infos
                Taginfo taginfo = new Taginfo();
                taginfo.TagMac = macTemp;

                if (this.tagSearch.ContainsKey(taginfo.TagMac) == false)
                {
                    this.tagSearch.Add(taginfo.TagMac, taginfo);
                }

                //update existing tag infos
                if (this.tagList.ContainsKey(macTemp) == true)
                {
                    taginfo = this.tagList[macTemp];
                }

                if (args.Advertisement.LocalName != "")
                    taginfo.TagName = args.Advertisement.LocalName;
                else
                {
                    taginfo.TagName = this.tagSearch[macTemp].TagName;
                }
                taginfo.updateTime = DateTime.Now;
                taginfo.CarrotPlugFlag = false;
                //ulong blAddress = args.BluetoothAddress;
                //BluetoothDevice blDevice = await Windows.Devices.Bluetooth.BluetoothDevice.FromBluetoothAddressAsync(blAddress);
                Debug.WriteLine("ble device :" + taginfo.TagName + "\tmac :" + taginfo.TagMac + "\ttype :" + args.AdvertisementType);

                //get tag datas
                string datasection = String.Empty;
                //if (taginfo.TagName.Length == 1 && (taginfo.TagName == "O" || taginfo.TagName == "Q"))
               
                foreach (BluetoothLEAdvertisementDataSection section in args.Advertisement.DataSections)
                {
                    var data = new byte[section.Data.Length];
                    using (var reader = DataReader.FromBuffer(section.Data))
                    {
                        reader.ReadBytes(data);
                        datasection = String.Format("{0}", BitConverter.ToString(data));
                        
                        //Complete Local Name패킷은 아래 로직에서 처리 안함
                        if (section.DataType == 0x09)
                        {
                            continue;
                        }

                        try
                        {
                            UInt16 crcRaw = (UInt16)((byte)data[data.Length - 2]<<8 | (byte)data[data.Length - 1]);
                            ushort check = CRC16.ComputeChecksum(data, data.Length - 2);
                            if (crcRaw != check)
                            {
                                continue; 
                            }
                            if(args.RawSignalStrengthInDBm < -80)
                            {
                                //Console.WriteLine($"Fail Rssi {args.RawSignalStrengthInDBm}");
                                continue;
                            }
                        }
                        catch { continue; }

                        taginfo.TagDataRaw.Clear();
                        taginfo.TagDataRaw.Add(datasection);
                        taginfo.TagRssi = args.RawSignalStrengthInDBm;

                        
                        if (section.Data.Length != 21 && section.Data.Length != 24)
                        {
                            Debug.WriteLine($"DataType {section.DataType}");
                            Debug.WriteLine($"DataLegnth {section.Data.Length}");
                            Debug.WriteLine($"Datas {datasection}");
                            Debug.WriteLine("");
                        }

                        try
                        {
                            if (args.AdvertisementType == BluetoothLEAdvertisementType.ScanResponse && modeFlag == 0 && taginfo.TagName == "Q")
                            {
                                if(this.tagList.ContainsKey(taginfo.TagMac) == true)
                                {
                                    int Index = 5;
                                    string tempStr = taginfo.TagDataRaw[0].Replace("-", "");
                                    
                                    taginfo.TagFlag = (uint)Convert.ToInt32(tempStr.Substring(Index, 1), 16);
                                    Index += 1;

                                    taginfo.ng2_gpsSnr = Convert.ToInt32(tempStr.Substring(Index, 2), 10);
                                    Index += 2;

                                    taginfo.ng2_temp = Convert.ToInt32(tempStr.Substring(Index, 2), 10);
                                    Index += 2;

                                    taginfo.ng2_cap = (Convert.ToInt32(tempStr.Substring(Index, 2), 16) * 2);
                                    Index += 2;

                                    taginfo.ng2_b3_min = (Convert.ToInt32(tempStr.Substring(Index, 2), 10) * -1);
                                    Index += 2;

                                    taginfo.ng2_b3_avg = (Convert.ToInt32(tempStr.Substring(Index, 2), 10) * -1);
                                    Index += 2;

                                    taginfo.ng2_b3_max = (Convert.ToInt32(tempStr.Substring(Index, 2), 10) * -1);
                                    Index += 2;

                                    taginfo.ng2_b5_min = (Convert.ToInt32(tempStr.Substring(Index, 2), 10) * -1);
                                    Index += 2;

                                    taginfo.ng2_b5_avg = (Convert.ToInt32(tempStr.Substring(Index, 2), 10) * -1);
                                    Index += 2;

                                    taginfo.ng2_b5_max = (Convert.ToInt32(tempStr.Substring(Index, 2), 10) * -1);
                                    Index += 2;

                                    /*Gold*/
                                    taginfo.ng2_Gold_GPS = Convert.ToInt32(tempStr.Substring(Index, 2), 10);
                                    Index += 2;

                                    taginfo.ng2_Gold_GPS_Margin = Convert.ToInt32(tempStr.Substring(Index, 2), 10);
                                    Index += 2;

                                    taginfo.ng2_Gold_Temp = Convert.ToInt32(tempStr.Substring(Index, 2), 10);
                                    Index += 2;

                                    taginfo.ng2_Gold_Temp_Margin = Convert.ToInt32(tempStr.Substring(Index, 2), 10);
                                    Index += 2;

                                    taginfo.ng2_Gold_Cap_Max = (Convert.ToInt32(tempStr.Substring(Index, 2), 16) * 2);
                                    Index += 2;

                                    taginfo.ng2_Gold_Cap_Min = (Convert.ToInt32(tempStr.Substring(Index, 2), 16) * 2);
                                    Index += 2;

                                    taginfo.ng2_Gold_b3 = (Convert.ToInt32(tempStr.Substring(Index, 2), 10) * -1);
                                    Index += 2;

                                    taginfo.ng2_Gold_b3_Margin = (Convert.ToInt32(tempStr.Substring(Index, 2), 10) * -1);
                                    Index += 2;

                                    taginfo.ng2_Gold_b5 = (Convert.ToInt32(tempStr.Substring(Index, 2), 10) * -1);
                                    Index += 2;

                                    taginfo.ng2_Gold_b5_Margin = (Convert.ToInt32(tempStr.Substring(Index, 2), 10) * -1);
                                    Index += 2;

                                    taginfo.ng2_ble_rssi = taginfo.TagRssi;
                                    taginfo.ng2_rawdata_flag = true;
                                    


                                    if (modeFlag == 0 && taginfo.TagName == "Q")
                                    {
                                        taginfo.TagFlagString = taginfo.TagVersion + " ";

                                        if ((taginfo.TagFlag & 0x01) == 0x01)
                                            taginfo.TagFlagString += "GPS Fail";
                                        else
                                            taginfo.TagFlagString += "GPS OK";
                                        if (taginfo.ng2_rawdata_flag)
                                            taginfo.TagFlagString += $"({taginfo.ng2_gpsSnr}:{taginfo.ng2_Gold_GPS},{taginfo.ng2_Gold_GPS_Margin})";
                                        taginfo.TagFlagString += ", ";

                                        if ((taginfo.TagFlag & 0x02) == 0x02)
                                            taginfo.TagFlagString += "TEMP Fail";
                                        else
                                            taginfo.TagFlagString += "TEMP OK";
                                        if (taginfo.ng2_rawdata_flag)
                                            taginfo.TagFlagString += $"({taginfo.ng2_temp}:{taginfo.ng2_Gold_Temp},{taginfo.ng2_Gold_Temp_Margin})";
                                        taginfo.TagFlagString += ", ";

                                        if ((taginfo.TagFlag & 0x04) == 0x04)
                                            taginfo.TagFlagString += "CAP Fail";
                                        else
                                            taginfo.TagFlagString += "CAP OK";
                                        if (taginfo.ng2_rawdata_flag)
                                            taginfo.TagFlagString += $"({taginfo.ng2_cap}:{taginfo.ng2_Gold_Cap_Min},{taginfo.ng2_Gold_Cap_Max})";
                                        taginfo.TagFlagString += ", ";


                                        if ((taginfo.TagFlag & 0x08) == 0x08)
                                            taginfo.TagFlagString += "LTE Fail";
                                        else
                                            taginfo.TagFlagString += "LTE OK";
                                        if (taginfo.ng2_rawdata_flag)
                                        {
                                            taginfo.TagFlagString += $"({taginfo.ng2_b3_avg},{taginfo.ng2_b3_max}:{taginfo.ng2_Gold_b3},{taginfo.ng2_Gold_b3_Margin}:" +
                                                $"{taginfo.ng2_b5_avg},{taginfo.ng2_b5_max}:{taginfo.ng2_Gold_b5},{taginfo.ng2_Gold_b5_Margin})";
                                        }

                                        if ((taginfo.TagFlag & 0xFF) == 0)
                                        {
                                            taginfo.passFlag = "OK";
                                        }
                                        else
                                        {
                                            taginfo.passFlag = "NG";
                                        }
                                    }
                                    //Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " SR :" + taginfo.TagName +"("+ taginfo.TagMac +"), Data : " + datasection);
                                }
                            }
                            else if (section.Data.Length > 6)
                            {
                                // 
                                taginfo.btAddress = args.BluetoothAddress;
                                taginfo.TagMenu = taginfo.TagDataRaw[0].Replace("-", "");
                                string majorVersion = ((uint)Convert.ToInt32(taginfo.TagMenu.Substring(2, 1), 16)).ToString();
                                string minorVersion = ((uint)Convert.ToInt32(taginfo.TagMenu.Substring(3, 1), 16)).ToString();
                                string patchVersion = ((uint)Convert.ToInt32(taginfo.TagMenu.Substring(0, 2), 16)).ToString();
                                taginfo.TagVersionNumber = Convert.ToUInt32(majorVersion) * FIRMWARE_MAJOR_MASK;
                                taginfo.TagVersionNumber += Convert.ToUInt32(minorVersion) * FIRMWARE_MINOR_MASK;
                                taginfo.TagVersionNumber += Convert.ToUInt32(patchVersion) * FIRMWARE_PATCH_MASK;
                                taginfo.TagVersion = 'v' + majorVersion + '.' + minorVersion + '.' + patchVersion;                  // Firmware Version
                                taginfo.TagIccID = "898205" + taginfo.TagMenu.Substring(4, 13);                                     // ICC ID
                                
                                if (modeFlag == 1 && taginfo.TagName == "O")
                                    taginfo.TagFlag = (uint)Convert.ToInt32(taginfo.TagMenu.Substring(17, 1), 16);                      // QA Flag    

                                taginfo.TagBleID = "4C520000-E25D-11EB-BA80-" + taginfo.TagMenu.Substring(18, 12);                  // BLE UUID
                                //taginfo.TagIMEI = "3596271" + taginfo.TagMenu.Substring(30, 8);                                   // IMEI
                                taginfo.TagIMEI = "8635930" + taginfo.TagMenu.Substring(30, 8);                                     // IMEI

                                if (modeFlag == 1 && taginfo.TagName == "O")
                                    taginfo.TagFlagString = taginfo.TagVersion + " ";
                                
                                
                                if (modeFlag == 0 && taginfo.TagName == "Q")
                                {
                                    /*
                                    if ((taginfo.TagFlag & 0x01) == 0x01)
                                        taginfo.TagFlagString += "GPS Fail";
                                    else
                                        taginfo.TagFlagString += "GPS OK";
                                    if (taginfo.ng2_rawdata_flag)
                                        taginfo.TagFlagString += $"({taginfo.ng2_gpsSnr}:{taginfo.ng2_Gold_GPS},{taginfo.ng2_Gold_GPS_Margin})";
                                    taginfo.TagFlagString += ", ";

                                    if ((taginfo.TagFlag & 0x02) == 0x02)
                                        taginfo.TagFlagString += "TEMP Fail";
                                    else
                                        taginfo.TagFlagString += "TEMP OK";
                                    if (taginfo.ng2_rawdata_flag)
                                        taginfo.TagFlagString += $"({taginfo.ng2_temp}:{taginfo.ng2_Gold_Temp},{taginfo.ng2_Gold_Temp_Margin})";
                                    taginfo.TagFlagString += ", ";

                                    if ((taginfo.TagFlag & 0x04) == 0x04)
                                        taginfo.TagFlagString += "CAP Fail";
                                    else
                                        taginfo.TagFlagString += "CAP OK";
                                    if (taginfo.ng2_rawdata_flag)
                                        taginfo.TagFlagString += $"({taginfo.ng2_cap}:{taginfo.ng2_Gold_Cap_Min},{taginfo.ng2_Gold_Cap_Max})";
                                    taginfo.TagFlagString += ", ";


                                    if ((taginfo.TagFlag & 0x08) == 0x08)
                                        taginfo.TagFlagString += "LTE Fail";
                                    else
                                        taginfo.TagFlagString += "LTE OK";
                                    if (taginfo.ng2_rawdata_flag)
                                    {
                                        taginfo.TagFlagString += $"({taginfo.ng2_b3_avg},{taginfo.ng2_b3_max}:{taginfo.ng2_Gold_b3},{taginfo.ng2_Gold_b3_Margin}:" +
                                            $"{taginfo.ng2_b5_avg},{taginfo.ng2_b5_max}:{taginfo.ng2_Gold_b5},{taginfo.ng2_Gold_b5_Margin})";
                                    }

                                    if ((taginfo.TagFlag & 0xFF) == 0)
                                    {
                                        taginfo.passFlag = "OK";
                                    }
                                    else
                                    {
                                        taginfo.passFlag = "NG";
                                    }
                                    */
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
                                            taginfo.passFlag = "OK";
                                            break;
                                    }
                                    taginfo.TagFlagString += " " + progressString;
                                    taginfo.CarrotPlugFlag = true;

                                }
                                // Debug.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " FD :" + taginfo.TagName + "(" + taginfo.TagMac + " / " + args.AdvertisementType.ToString() + ") L:" + section.Data.Length + " Data: " + datasection);

                            }
                            else
                            {
                                taginfo.TagMenu = "";
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
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
