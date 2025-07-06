using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using Windows.System;

namespace Carrot_QA_test
{
    partial class Form1
    {
        
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnStartBle;
        private System.Windows.Forms.Button btnClearBle;
        private System.Windows.Forms.Button BtnUpload;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ListView listView1;
        private System.ComponentModel.IContainer components = null;
        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>

        #endregion

        private ColumnHeader IMEI;
        private ColumnHeader CCID;
        private ColumnHeader RSSI;
        private ColumnHeader Pass;
        private ColumnHeader Note;
        private Label modeLabel;
        private Label label4;
        private Button BtnMode;
        private ColumnHeader DB;
        private ColumnHeader Reg;
        private Label PassCount;
        private Label Count;
        private Label label3;
        private Label label2;
        private CheckBox VersionCheck;
        private Button Scan;
        private ColumnHeader BLE;
        private Label ble_label;
        private Label myIPAddr;
        private Label myIpLabel;
        private Label dbHost;
        private Label dbLebel;
        private Label ServerVersionText;
    }
}


