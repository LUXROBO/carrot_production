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
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using Timer = System.Timers.Timer;

namespace Carrot_QA_test
{
    public partial class Form1 : Form
    {
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
            ((System.ComponentModel.ISupportInitialize)(this.listTimer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbTimer)).BeginInit();
            this.SuspendLayout();
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("굴림", 12F);
            this.txtSearch.Location = new System.Drawing.Point(953, 12);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(436, 35);
            this.txtSearch.TabIndex = 0;
            // 
            // btnStartBle
            // 
            this.btnStartBle.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.btnStartBle.Location = new System.Drawing.Point(955, 60);
            this.btnStartBle.Name = "btnStartBle";
            this.btnStartBle.Size = new System.Drawing.Size(110, 42);
            this.btnStartBle.TabIndex = 1;
            this.btnStartBle.Text = "Start";
            this.btnStartBle.UseVisualStyleBackColor = true;
            this.btnStartBle.Click += new System.EventHandler(this.BtnStartBle_Click);
            // 
            // btnClearBle
            // 
            this.btnClearBle.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.btnClearBle.Location = new System.Drawing.Point(1070, 60);
            this.btnClearBle.Name = "btnClearBle";
            this.btnClearBle.Size = new System.Drawing.Size(100, 42);
            this.btnClearBle.TabIndex = 2;
            this.btnClearBle.Text = "Clear";
            this.btnClearBle.UseVisualStyleBackColor = true;
            this.btnClearBle.Click += new System.EventHandler(this.BtnClearBle_Click);
            // 
            // BtnUpload
            // 
            this.BtnUpload.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.BtnUpload.Location = new System.Drawing.Point(1175, 60);
            this.BtnUpload.Name = "BtnUpload";
            this.BtnUpload.Size = new System.Drawing.Size(105, 42);
            this.BtnUpload.TabIndex = 3;
            this.BtnUpload.Text = "Upload";
            this.BtnUpload.UseVisualStyleBackColor = true;
            this.BtnUpload.Click += new System.EventHandler(this.BtnUpload_Click);
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(1285, 60);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(105, 42);
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
            this.RSSI,
            this.Pass,
            this.DB,
            this.Reg,
            this.Note});
            this.listView1.Font = new System.Drawing.Font("굴림", 12F);
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(13, 111);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1376, 1012);
            this.listView1.TabIndex = 5;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
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
            this.label4.Location = new System.Drawing.Point(862, 18);
            this.label4.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 24);
            this.label4.TabIndex = 12;
            this.label4.Text = "Search";
            // 
            // modeLabel
            // 
            this.modeLabel.AutoSize = true;
            this.modeLabel.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.modeLabel.Location = new System.Drawing.Point(12, 18);
            this.modeLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.modeLabel.Name = "modeLabel";
            this.modeLabel.Size = new System.Drawing.Size(308, 24);
            this.modeLabel.TabIndex = 13;
            this.modeLabel.Text = "Carrot Plug QA Test Mode";
            // 
            // BtnMode
            // 
            this.BtnMode.Font = new System.Drawing.Font("굴림", 12F);
            this.BtnMode.Location = new System.Drawing.Point(365, 15);
            this.BtnMode.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.BtnMode.Name = "BtnMode";
            this.BtnMode.Size = new System.Drawing.Size(180, 34);
            this.BtnMode.TabIndex = 14;
            this.BtnMode.Text = "Mode Change";
            this.BtnMode.UseVisualStyleBackColor = true;
            this.BtnMode.Click += new System.EventHandler(this.BtnMode_Click);
            // 
            // PassCount
            // 
            this.PassCount.AutoSize = true;
            this.PassCount.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.PassCount.Location = new System.Drawing.Point(495, 66);
            this.PassCount.Name = "PassCount";
            this.PassCount.Size = new System.Drawing.Size(24, 24);
            this.PassCount.TabIndex = 11;
            this.PassCount.Text = "-";
            // 
            // Count
            // 
            this.Count.AutoSize = true;
            this.Count.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold);
            this.Count.Location = new System.Drawing.Point(360, 66);
            this.Count.Name = "Count";
            this.Count.Size = new System.Drawing.Size(24, 24);
            this.Count.TabIndex = 10;
            this.Count.Text = "-";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("굴림", 12F);
            this.label2.Location = new System.Drawing.Point(157, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(187, 24);
            this.label2.TabIndex = 8;
            this.label2.Text = "Number of Tag : ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("굴림", 12F);
            this.label3.Location = new System.Drawing.Point(400, 66);
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
            this.VersionCheck.Location = new System.Drawing.Point(593, 23);
            this.VersionCheck.Name = "VersionCheck";
            this.VersionCheck.Size = new System.Drawing.Size(154, 22);
            this.VersionCheck.TabIndex = 15;
            this.VersionCheck.Text = "버전 확인 모드";
            this.VersionCheck.UseVisualStyleBackColor = true;
            // 
            // ServerVersionText
            // 
            this.ServerVersionText.AutoSize = true;
            this.ServerVersionText.Font = new System.Drawing.Font("굴림", 12F);
            this.ServerVersionText.Location = new System.Drawing.Point(589, 66);
            this.ServerVersionText.Name = "ServerVersionText";
            this.ServerVersionText.Size = new System.Drawing.Size(201, 24);
            this.ServerVersionText.TabIndex = 16;
            this.ServerVersionText.Text = "Firmware Version :";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1405, 1142);
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
            this.Name = "Form1";
            this.Text = "Carrot QA Program";
            ((System.ComponentModel.ISupportInitialize)(this.listTimer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbTimer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
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
                        ServerVersion %= 65536;
                        this.ServerVersionText.Text = "Firmware Version : v0.1." + ServerVersion.ToString();
                    }
                }
                if (GetExternalIPAddress() != "175.209.190.173")
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
                        string icc_id = tag.TagMenu;
                        string imei = tag.TagName;
                        int result;
                        if (modeFlag == 0)
                            result = mydb.UpdateQuery_qa2(imei, icc_id, tag.passFlag, tag.TagFlagString);
                        else
                            result = mydb.UpdateQuery_qa3(imei, icc_id, tag.passFlag, tag.TagFlagString);

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
                        }
                        tag.passFlagUpdate = false;
                    }

                    TimeSpan timeDiff = DateTime.Now - tag.updateTime;
                    if (timeDiff.Seconds > 10)
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
            try
            {
                listView1.BeginUpdate();
                listView1.Items.Clear();
                try
                {
                    foreach (Taginfo tag in tagColl)
                    {
                        ListViewItem LVI = new ListViewItem(tag.TagName);
                        LVI.SubItems.Add(tag.TagMenu);
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
                Count.Text = _count;
                PassCount.Text = Convert.ToString(_passCount);
            }
            finally
            {
                listView1.EndUpdate();
                Application.DoEvents();
            }

        }

        private void BtnStartBle_Click(object sender, EventArgs e)
        {

            //start scanning
            if (this.watchStarted == false)
            {
                this.watcher.Received += Tag_Received;
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

        private void BtnClearBle_Click(object sender, EventArgs e)
        {
            this.tagList.Clear();
            this.tagColl.Clear();
            Count.Text = "0";
            PassCount.Text = "0";


        }

        /** @brief : update filter for the search
         * update shown tags depending on the search
         */
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            filter = txtSearch.Text;
            this.tagColl.Clear();

            //update shown tags corresponding to search text
            foreach (KeyValuePair<string, Taginfo> pair in this.tagList)
            {
                Taginfo taginfo = new Taginfo();
                taginfo = pair.Value;
                if (Filter(taginfo))
                {
                    this.tagColl.Add(taginfo);
                }
            }
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
        private void Tag_Received(BluetoothLEAdvertisementWatcher received, BluetoothLEAdvertisementReceivedEventArgs args)
        {

            //show only connectable tags
            if (args.AdvertisementType == BluetoothLEAdvertisementType.NonConnectableUndirected || args.AdvertisementType == BluetoothLEAdvertisementType.ConnectableUndirected)
            {
                //get tag infos
                Taginfo taginfo = new Taginfo();
                taginfo.TagMac = args.BluetoothAddress.ToString("X");
                taginfo.TagName = args.Advertisement.LocalName;
                taginfo.updateTime = DateTime.Now;

                //get tag datas
                string datasection = String.Empty;
                foreach (BluetoothLEAdvertisementDataSection section in args.Advertisement.DataSections)
                {
                    var data = new byte[section.Data.Length];
                    using (var reader = DataReader.FromBuffer(section.Data))
                    {
                        reader.ReadBytes(data);
                        datasection = String.Format("{0}", BitConverter.ToString(data));
                        taginfo.TagDataRaw.Add(datasection);
                        taginfo.TagRssi = args.RawSignalStrengthInDBm;
                        try
                        {
                            if (taginfo.TagDataRaw.Count >= 3 && taginfo.TagDataRaw[2].Length > 6)
                            {

                                if (taginfo.TagName.Length == 8)
                                {
                                    taginfo.TagName = "3596271" + taginfo.TagName;
                                    taginfo.CarrotPlugFlag = true;
                                    if (modeFlag == 0)
                                    {
                                        taginfo.TagFlagString = "";
                                        string flag_string = taginfo.TagDataRaw[2].Replace("-", "").Substring(0, 2);
                                        taginfo.TagMenu = taginfo.TagDataRaw[2].Replace("-", "");
                                        if (taginfo.TagMenu.Substring(taginfo.TagMenu.Length - 1, 1) != "0")
                                        {
                                            taginfo.CarrotPlugFlag = false;
                                        }
                                        taginfo.TagMenu = taginfo.TagMenu.Substring(4, taginfo.TagMenu.Length - 5);

                                        taginfo.TagFlag = uint.Parse(flag_string);
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
                                        if((taginfo.TagFlag & 0xFF) == 0)
                                        {
                                            taginfo.passFlag = "OK";
                                        }
                                    }
                                    else
                                    {
                                        taginfo.TagFlagString = "";
                                        taginfo.TagMenu = taginfo.TagDataRaw[2].Replace("-", "");
                                        taginfo.TagFlag = (uint)Convert.ToInt32(taginfo.TagMenu.Substring(taginfo.TagMenu.Length - 1, 1), 16);
                                        if (taginfo.TagFlag == 0)
                                        {
                                            taginfo.CarrotPlugFlag = false;
                                        }
                                        taginfo.TagMenu = taginfo.TagMenu.Substring(4, taginfo.TagMenu.Length - 5);
                                        string verionString = ((uint)Convert.ToInt32(taginfo.TagDataRaw[2].Replace("-", "").Substring(0, 2),16)).ToString();
                                        string progressString = "none";
                                        taginfo.TagFlagString += "v0.1." + verionString;
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
                                                    if (ServerVersion <= int.Parse(verionString))
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
                                    }
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

                if (taginfo.CarrotPlugFlag)
                {
                    if (taginfo.TagName.Contains(filter))
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
                        _count = Convert.ToString(tagColl.Count);
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
                        result = mydb.UpdateQuery_qa2(imei, icc_id, tag.passFlag, tag.TagFlagString);
                    else
                        result = mydb.UpdateQuery_qa3(imei, icc_id, tag.passFlag, tag.TagFlagString);

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
                    msg += imei + " Upload Result :" + result_string +", "+ reg_result + "\n";
                    tag.passFlagUpdate = false;
                }
            }
            MessageBox.Show(msg);
        }

        private void BtnMode_Click(object sender, EventArgs e)
        {
            if (this.modeFlag == 0)
            {
                this.modeFlag = 1;
                this.modeLabel.Text = "Carrot Plug 개통 Test Mode";
                this.tagList.Clear();
                this.tagColl.Clear();
                Count.Text = "0";
                PassCount.Text = "0";
            }
            else
            {
                this.modeFlag = 0;
                this.modeLabel.Text = "Carrot Plug QA Test Mode";
                this.tagList.Clear();
                this.tagColl.Clear();
                Count.Text = "0";
                PassCount.Text = "0";
            }
            Application.DoEvents();
        }

    }
}
