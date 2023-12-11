using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Diagnostics;

public class Plug {
    public string qa1;
    public string ng1_type;
    public string qa2;
    public string ng2_type;
    public string qa3;
    public string ng3_type;
    public string icc_id;
    public string device_id;
    public string prod_date;
    public string lot_no;
    public string sn;
    public string tdtag;
    public string dtag;
    public string ble_id;
    public bool testServerFlag { get; set; } = false;
    public bool mainServerFlag { get; set; } = false;
};

public class Mydb
{
    private MySqlConnection conn;
    private readonly string ConnUrl = "Server=release-carrot-cluster.cluster-cb10can9foe2.ap-northeast-2.rds.amazonaws.com;Database=carrotPlugList;Uid=luxrobo;Pwd=fjrtmfhqh123$;";
    public MySqlDataReader rdr;

    private Dictionary<string, Plug> pluglist = new Dictionary<string, Plug>();

    // Test Server
    //string serverUrl = "https://t-dtag.carrotins.com:8080/api/v1/dtag/registries";
    //string serverBearer = "Bearer KXKQNQ64380880304TLRQQ";
    //string serverHost = "t-dtag.carrotins.com";

    // Main Server
    string serverUrl = "https://dtag.carrotins.com:8080/api/v1/dtag/registries";
    string serverBearer = "Bearer EJH6NE0851819521SSSA7M";
    string serverHost = "dtag.carrotins.com";

    //MainServer "https://dtag.carrotins.com:8080/api/v1/dtag/registries" / "Bearer EJH6NE0851819521SSSA7M" / "dtag.carrotins.com";
    //TestServer "https://t-dtag.carrotins.com:8080/api/v1/dtag/registries" / "Bearer KXKQNQ64380880304TLRQQ" / "t-dtag.carrotins.com";
    JObject regPayload = new JObject()
        {
            { "deviceId", "LUX1_359627100041471"},
            { "cmBzpsRgtDscno", "3007022" },
            { "divcSrlNo", "CPA3007022" },
            { "divcNm", "Carrot Plug 1" },
            { "mdnm", "IV-GT1LM1BT" },
            { "mnftrNm","LUXROBO" },
            { "prddt", "2021-01-19" }
        };

    public Mydb()
    {
        //mainServerHeaderCollection[HttpRequestHeader.Host]          = "dtag.carrotins.com";
        conn = new MySqlConnection(ConnUrl);
        if (conn.State == ConnectionState.Closed)
        {
            conn.Open();
            Console.WriteLine("connReader ON");
            new MySqlCommand("set sql_safe_updates=0;", conn);
        }
    }

    public Mydb(string url)
    {

        conn = new MySqlConnection(url);
        if (conn.State == ConnectionState.Closed)
        {
            conn.Open();
            Console.WriteLine("connReader ON");
            new MySqlCommand("set sql_safe_updates=0;", conn);
        }
    }

    ~Mydb()
    {
        conn.Close();
    }


    public int UpdateQuery(string imei, string icc_id)
    {
        if (icc_id == null)
            icc_id = "NULL";
        string str_update = "UPDATE carrotPlugList.tb_product SET icc_id =" + icc_id + " where imei ='" + imei + "';";
        return new MySqlCommand(str_update, conn).ExecuteNonQuery();
    }

    public int UpdateQuery_qa2(string imei, string icc_id, string qa2, string ng2_type, string ble_id, Taginfo taginfo)
    {
        if (icc_id == null)
            icc_id = "NULL";
        DateTime update_date = DateTime.Now;
        string date_str = update_date.ToString("yyyy-MM-dd HH:mm:ss");
        string str_update;
        str_update = "UPDATE carrotPlugList.tb_product SET icc_id =" + icc_id; 
        str_update += ", qa2=\"" + qa2;
        str_update += "\", ng2_type=\"" + ng2_type;
        str_update += "\", ble_id =\"" + ble_id;
        str_update += "\", qa2_update_date =\""+ date_str;
        if(taginfo.ng2_rawdata_flag)
        {
            str_update += "\", qa2_gps_snr =\""+ taginfo.ng2_gpsSnr;
            str_update += "\", qa2_temperature =\"" + taginfo.ng2_temp;
            str_update += "\", qa2_cap =\"" + taginfo.ng2_cap;
            str_update += "\", qa2_ble_rssi =\""+ taginfo.ng2_ble_rssi;
            str_update += "\", qa2_lte_b3_min =\"-"+ taginfo.ng2_b3_min;
            str_update += "\", qa2_lte_b3_avg =\"-"+ taginfo.ng2_b3_avg;
            str_update += "\", qa2_lte_b3_max =\"-"+ taginfo.ng2_b3_max;
            str_update += "\", qa2_lte_b5_min =\"-"+ taginfo.ng2_b5_min;
            str_update += "\", qa2_lte_b5_avg =\"-"+ taginfo.ng2_b5_avg;
            str_update += "\", qa2_lte_b5_max =\"-"+ taginfo.ng2_b5_max;
        }        
        str_update += "\" where imei ='" + imei + "';";
        
        Debug.WriteLine($"{DateTime.Now} : {imei} {ng2_type}");


        return new MySqlCommand(str_update, conn).ExecuteNonQuery();
    }

    public int UpdateQuery_qa3(string imei, string icc_id, string qa3, string ng3_type, string ble_id)
    {
        if (icc_id == null)
            icc_id = "NULL";
        DateTime update_date = DateTime.Now;
        string date_str = update_date.ToString("yyyy-MM-dd HH:mm:ss");
        string str_update = "UPDATE carrotPlugList.tb_product SET icc_id =" + icc_id + ", qa3=\"" + qa3 + "\", ng3_type=\"" + ng3_type + "\", ble_id =\"" + ble_id + "\", qa3_update_date =\""+ date_str + "\" where imei ='" + imei + "';";
        return  new MySqlCommand(str_update, conn).ExecuteNonQuery();
    }

    private int UpdateQuery_dtag(string imei, string dtag)
    {
        string tdtag = "NG";
        DateTime update_date = DateTime.Now;
        string date_str = update_date.ToString("yyyy-MM-dd HH:mm:ss");
        string str_update = "UPDATE carrotPlugList.tb_product SET dtag =\"" + dtag + "\", tdtag= \"" + tdtag + "\", dtag_update_date =\""+ date_str + "\" where imei ='" + imei + "';";
        return new MySqlCommand(str_update, conn).ExecuteNonQuery();
    }

    private int registries_server(string url, string host, string bearer)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        httpWebRequest.Method = "POST";
        httpWebRequest.Host = host;
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Headers.Add("Authorization", bearer);

        try
        {
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = regPayload.ToString();
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
            return (int)httpResponse.StatusCode;
        }
        catch (WebException webExcp)
        {
            // If you reach this point, an exception has been caught.  
            Console.WriteLine("A WebException has been caught.");
            // Write out the WebException message.  
            Console.WriteLine(webExcp.ToString());
            // Get HttpWebResponse so that you can check the HTTP status code.  
            HttpWebResponse httpResponse = (HttpWebResponse)webExcp.Response;
            if(httpResponse == null)
            {
                return -1;
            }
            return (int)httpResponse.StatusCode;
        }
    }

    public int regist_server(string imei)
    {
        int ret, return_ret;
        string dtag_string;
        GetProduct(imei);
        try
        {
            if(pluglist[imei].device_id == "")
            {
                pluglist[imei].device_id = "LUX2_" + imei;
            }
            regPayload["deviceId"] = pluglist[imei].device_id;
            regPayload["cmBzpsRgtDscno"] = pluglist[imei].sn;
            regPayload["divcSrlNo"] = pluglist[imei].lot_no;
            if(pluglist[imei].prod_date == "")
            {
                regPayload["prddt"] = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                regPayload["prddt"] = pluglist[imei].prod_date;
            }
            regPayload["divcNm"] = "Carrot Plug 3";
            regPayload["mdnm"] = "Carrot Plug 3";
            regPayload["mnftrNm"] = "LUXROBO";
            regPayload["bluetId"] = pluglist[imei].ble_id;
            dtag_string = pluglist[imei].dtag;
            if (dtag_string == "OK")
            {
                pluglist[imei].mainServerFlag = true;
            }

        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex);
            return -1;
        }
        return_ret = -1;

        if(!pluglist[imei].mainServerFlag)
        {
            ret = registries_server(serverUrl, serverHost, serverBearer);
            if (ret == 201 || ret == 409)
            {
                pluglist[imei].mainServerFlag = true;
                dtag_string = "OK";
                return_ret = 1;
            }
            else if (ret == 408)
            {
                return_ret = -4;
            }
            else 
            {
                return_ret = -3;
            }
        }else if(dtag_string == "OK")
        {
            return_ret = 1;
        }
        UpdateQuery_dtag(imei, dtag_string);
        return return_ret;
    }

    public Dictionary<string, Plug> GetProduct(string imei)
    {
        MySqlCommand cmd_select = new MySqlCommand("SELECT device_id, prod_date, lot_no, sn, imei, icc_id, ble_id, dtag, tdtag FROM carrotPlugList.tb_product where imei=" + imei+";", conn);
        if(pluglist.ContainsKey(imei))
            pluglist.Remove(imei);
        rdr = cmd_select.ExecuteReader();
        while (rdr.Read())
        {

            //Console.WriteLine("{0}\t{1}\t{2}", rdr["sn"], rdr["imei"], rdr["icc_id"]);
            Plug data = new Plug();
            try
            {
                data.device_id = rdr["device_id"].ToString();
            }
            catch
            {
                data.device_id = "LUX1_"+imei;
            }
            try
            {
                data.sn = rdr["sn"].ToString();
            }
            catch
            {
                data.sn = "NULL";
            }
            try
            {
                data.icc_id = rdr["icc_id"].ToString();
            }
            catch
            {
                data.icc_id = "NULL";
            }
            try
            {
                data.ble_id = rdr["ble_id"].ToString();
            }
            catch
            {
                data.ble_id = "NULL";
            }
            try
            {
                data.prod_date = rdr["prod_date"].ToString();
            }
            catch
            {
                data.prod_date = "";
            }
            try
            {
                data.lot_no = rdr["lot_no"].ToString();
            }
            catch
            {
                data.lot_no = "NULL";
            }
            try
            {
                data.dtag = rdr["dtag"].ToString();
            }
            catch
            {
                data.dtag = "NULL";
            }
            try
            {
                data.tdtag = rdr["tdtag"].ToString();
            }
            catch
            {
                data.tdtag = "NULL";
            }

            pluglist.Add(rdr["imei"].ToString(), data);
        }
        rdr.Dispose();
        return pluglist;
    }
    public Dictionary<string, Plug> getList()
    {
        if (pluglist.Count == 0) ReflashList();
        return pluglist;
    }
    public Dictionary<string, Plug> ReflashList()
    {

        MySqlCommand cmd_select = new MySqlCommand("SELECT device_id, prod_date, lot_no, sn, imei, icc_id, ble_id, dtag, tdtag FROM carrotPlugList.tb_product LIMIT 0,1000; ", conn);
        pluglist.Clear();
        rdr = cmd_select.ExecuteReader();
        while (rdr.Read())
        {
            Plug data = new Plug();
            try
            {
                data.device_id = rdr["device_id"].ToString();
            }
            catch
            {
                data.device_id = "NULL";
            }
            try
            {
                data.sn = rdr["sn"].ToString();
            }
            catch
            {
                data.sn = "NULL";
            }
            try
            {
                data.icc_id = rdr["icc_id"].ToString();
            }
            catch
            {
                data.icc_id = "NULL";
            }
            try
            {
                data.prod_date = rdr["prod_date"].ToString();
            }
            catch
            {
                data.prod_date = "NULL";
            }
            try
            {
                data.lot_no = rdr["lot_no"].ToString();
            }
            catch
            {
                data.lot_no = "NULL";
            }
            try
            {
                data.lot_no = rdr["dtag"].ToString();
            }
            catch
            {
                data.lot_no = "NULL";
            }
            try
            {
                data.lot_no = rdr["tdtag"].ToString();
            }
            catch
            {
                data.lot_no = "NULL";
            }



            //Console.WriteLine("{0}\t{1}\t{2}", rdr["sn"], rdr["imei"], rdr["icc_id"]);
            pluglist.Add(rdr["imei"].ToString(), data);
        }
        rdr.Dispose();
        return pluglist;
    }
}


public class Taginfo : INotifyPropertyChanged
{
    #region const
    private const string captor_T = "T";
    private const string captor_RH = "RH";
    private const string captor_MAG_MOV = "MAG_MOV";
    private const string captor_ANG = "ANG";
    private const string hex_T = "6E-2A-";
    private const string hex_RH = "6F-2A-";
    private const string hex_MAG_MOV = "06-2A-";
    private const string hex_ANG = "A1-2A-";
    #endregion

    /** mac adress */
    private string mac = "";

    /** local name */
    private string name = "";

    /** RSSI / power value */
    private Int16 rssi = 0;

    /** formated data*/
    private string data = "";

    private string menu = "";

    private string flagString = "";

    private uint flag = 0xFF;
    private string _passFlag = "NG";

    private int ng2_gpsSnr_raw = 0;
    private int ng2_temp_raw = 0;
    private int ng2_cap_raw = 0;
    private int ng2_b3_min_raw = 0;
    private int ng2_b3_avg_raw = 0;
    private int ng2_b3_max_raw = 0;
    private int ng2_b5_min_raw = 0;
    private int ng2_b5_avg_raw = 0;
    private int ng2_b5_max_raw = 0;
    private int ng2_ble_rssi_raw = 0;
    
    /*∞ÒµÂª˘«√ µ•¿Ã≈Õ*/
    private int ng2_gold_gps_raw = 0;
    private int ng2_gold_gps_margin_raw = 0;
    private int ng2_gold_temp_raw = 0;
    private int ng2_gold_temp_margin_raw = 0;
    private int ng2_gold_cap_max_raw = 0;
    private int ng2_gold_cap_min_raw = 0;
    private int ng2_gold_b3_raw = 0;
    private int ng2_gold_b3_margin_raw = 0;
    private int ng2_gold_b5_raw = 0;
    private int ng2_gold_b5_margin_raw = 0;
    private bool ng2_rawdata_flag_raw = false;

    public ulong btAddress;
    public string passFlag
    {
        get
        {
            return _passFlag;
        }
        set
        {
            if (!_passFlag.Equals(value))
            {
                if(_passFlag != "OK")
                {
                    _passFlag = value;
                    passFlagUpdate = true;
                }
            }
        }
    }
    public string TagBleID = "4C520000-E25D-11EB-BA80-000000000000";
    public string TagIccID = "";
    public string TagIMEI = "";
    public bool passFlagUpdate = true;
    public string TagVersion = "";
    public string dbString = "NG";
    public bool dbFlag = false;
    public bool serverFlag = false;
    public UInt32 TagVersionNumber = 0;
    public DateTime updateTime;

    public bool SendedImei = false;


    /** existence of data used to hide/show data on display */
    private bool dataExist = false;


    /** used to get updates on data in ObservableCollection */
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(name));
        }
    }

    #region accessors
    public string TagMac { get { return mac; } set { mac = value; OnPropertyChanged("TagMac"); } }
    public string TagName { get { return name; } set { name = value; OnPropertyChanged("TagName"); } }
    public Int16 TagRssi { get { return rssi; } set { rssi = value; OnPropertyChanged("TagRssi"); } }
    public List<string> TagDataRaw = new List<string>();
    public string TagData { get { return data; } set { data = value; OnPropertyChanged("TagData"); } }
    public string TagMenu { get { return menu; } set { menu = value; OnPropertyChanged("TagMenu"); } }
    public string TagFlagString { get { return flagString; }    set { flagString        = value; OnPropertyChanged("TagFlagString"); } }
    public int ng2_gpsSnr   { get { return ng2_gpsSnr_raw; }    set { ng2_gpsSnr_raw    = value; OnPropertyChanged("ng2_gpsSnr"); } }
    public int ng2_temp     { get { return ng2_temp_raw; }      set { ng2_temp_raw      = value; OnPropertyChanged("ng2_temp"); } }
    public int ng2_cap      { get { return ng2_cap_raw; }       set { ng2_cap_raw       = value; OnPropertyChanged("ng2_cap"); } }
    public int ng2_ble_rssi { get { return ng2_ble_rssi_raw; }  set { ng2_ble_rssi_raw  = value; OnPropertyChanged("ng2_ble_rssi"); } }
    public int ng2_b3_min   { get { return ng2_b3_min_raw; }    set { ng2_b3_min_raw    = value; OnPropertyChanged("ng2_b3_min"); } }
    public int ng2_b3_avg   { get { return ng2_b3_avg_raw; }    set { ng2_b3_avg_raw    = value; OnPropertyChanged("ng2_b3_avg"); } }
    public int ng2_b3_max   { get { return ng2_b3_max_raw; }    set { ng2_b3_max_raw    = value; OnPropertyChanged("ng2_b3_max"); } }
    public int ng2_b5_min   { get { return ng2_b5_min_raw; }    set { ng2_b5_min_raw    = value; OnPropertyChanged("ng2_b5_min"); } }
    public int ng2_b5_avg   { get { return ng2_b5_avg_raw; }    set { ng2_b5_avg_raw    = value; OnPropertyChanged("ng2_b5_avg"); } }
    public int ng2_b5_max   { get { return ng2_b5_max_raw; }    set { ng2_b5_max_raw    = value; OnPropertyChanged("ng2_b5_max"); } }

    public int ng2_Gold_GPS { get { return ng2_gold_gps_raw; } set { ng2_gold_gps_raw = value; OnPropertyChanged("ng2_gold_gps"); } }
    public int ng2_Gold_GPS_Margin { get { return ng2_gold_gps_margin_raw; } set { ng2_gold_gps_margin_raw = value; OnPropertyChanged("ng2_gold_gps_margin"); } }
    public int ng2_Gold_Temp { get { return ng2_gold_temp_raw; } set { ng2_gold_temp_raw = value; OnPropertyChanged("ng2_gold_temp"); } }
    public int ng2_Gold_Temp_Margin { get { return ng2_gold_temp_margin_raw; } set { ng2_gold_temp_margin_raw = value; OnPropertyChanged("ng2_gold_temp_margin"); } }

    public int ng2_Gold_Cap_Max { get { return ng2_gold_cap_max_raw; } set { ng2_gold_cap_max_raw = value; OnPropertyChanged("ng2_gold_cap_max"); } }

    public int ng2_Gold_Cap_Min { get { return ng2_gold_cap_min_raw; } set { ng2_gold_cap_min_raw = value; OnPropertyChanged("ng2_gold_cap_min"); } }

    public int ng2_Gold_b3 { get { return ng2_gold_b3_raw; } set { ng2_gold_b3_raw = value; OnPropertyChanged("ng2_gold_b3"); } }
    public int ng2_Gold_b3_Margin { get { return ng2_gold_b3_margin_raw; } set { ng2_gold_b3_margin_raw = value; OnPropertyChanged("ng2_gold_b3_margin"); } }
    public int ng2_Gold_b5 { get { return ng2_gold_b5_raw; } set { ng2_gold_b5_raw = value; OnPropertyChanged("ng2_gold_b5"); } }
    public int ng2_Gold_b5_Margin { get { return ng2_gold_b5_margin_raw; } set { ng2_gold_b5_margin_raw = value; OnPropertyChanged("ng2_gold_b5_margin"); } }
    

    public bool ng2_rawdata_flag { get { return ng2_rawdata_flag_raw; }    set { ng2_rawdata_flag_raw    = value; OnPropertyChanged("ng2_rawdata_flag"); } }
    public uint TagFlag { get { return flag; } set { flag = value; OnPropertyChanged("TagFlag"); } }
    private string TagCaptorType { get; set; }
    public object TagDataVisibility { get { if (dataExist) { return Visibility.Visible; } else { return Visibility.Collapsed; } } }
    #endregion
    public Boolean CarrotPlugFlag { get; set; }

    /** @brief : store captor type in TagCaptorType
      */
    private void getCaptorType()
    {
        TagCaptorType = String.Empty;

        for (int i = 1; i < TagDataRaw.Count - 1; ++i)
        {
            if (TagDataRaw[i].Length > 6)
            {
                string type = TagDataRaw[i].Substring(0, 6);
                if (type.Equals(hex_T))
                {
                    TagCaptorType += captor_T;
                }
                else if (type.Equals(hex_RH))
                {
                    TagCaptorType += captor_RH;
                }
                else if (type.Equals(hex_MAG_MOV))
                {
                    TagCaptorType += captor_MAG_MOV;
                }
                else if (type.Equals(hex_ANG))
                {
                    TagCaptorType += captor_ANG;
                }
            }
        }
    }


    /** @brief : update showned values
     * @param [in] taginfo : Taginfo with updated values
     */
    public void update(Taginfo taginfo)
    {
        this.TagName            = taginfo.TagName;
        this.TagRssi            = taginfo.TagRssi;
        this.TagData            = taginfo.TagData;
        this.TagMenu            = taginfo.TagMenu;
        this.TagVersion         = taginfo.TagVersion;
        this.TagVersionNumber   = taginfo.TagVersionNumber;
        this.TagIccID           = taginfo.TagIccID;
        this.TagIMEI            = taginfo.TagIMEI;
        this.TagBleID           = taginfo.TagBleID;
        this.TagFlagString      = taginfo.TagFlagString;
        this.ng2_b3_avg         = taginfo.ng2_b3_avg;
        this.ng2_b3_min         = taginfo.ng2_b3_min;
        this.ng2_b3_max         = taginfo.ng2_b3_max;
        this.ng2_b5_avg         = taginfo.ng2_b5_avg;
        this.ng2_b5_min         = taginfo.ng2_b5_min;
        this.ng2_b5_max         = taginfo.ng2_b5_max;
        this.ng2_rawdata_flag   = taginfo.ng2_rawdata_flag;
        this.ng2_temp           = taginfo.ng2_temp;
        this.ng2_cap            = taginfo.ng2_cap;
        this.ng2_gpsSnr         = taginfo.ng2_gpsSnr;
        this.passFlag           = taginfo.passFlag;
        this.btAddress          = taginfo.btAddress;
        this.flag               = taginfo.flag;
        this.updateTime         = taginfo.updateTime;
}
}
