using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
public class Plug {
    public string qa1;
    public string ng1_type;
    public string qa2;
    public string ng2_type;
    public string qa3;
    public string ng3_type;
    public string icc_id;
    public string device_id;
    public string prod_data;
    public string lot_no;
    public string sn;
    public bool testServerFlag { get; set; } = false;
    public bool mainServerFlag { get; set; } = true;
};

public class Mydb
{
    private MySqlConnection conn;
    private readonly string ConnUrl = "Server=release-carrot-cluster.cluster-ro-cb10can9foe2.ap-northeast-2.rds.amazonaws.com;Database=carrotPlugList;Uid=luxrobo;Pwd=fjrtmfhqh123$;";
    public MySqlDataReader rdr;

    private Dictionary<string, Plug> pluglist = new Dictionary<string, Plug>();

    string mainServerUrl = "https://dtag.carrotins.com:8080/api/v1/dtag/registries";
    string testServerUrl = "https://t-dtag.carrotins.com:8080/api/v1/dtag/registries";
    JObject regPayload = new JObject()
        {
            { "deviceId", "LUX1_359627100041471"},
            { "cmBzpsRgtDscno", "3007022" },
            { "divcSrlNo", "CPA3007022" },
            { "divcNm", "Carrot Plug" },
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
        }
    }

    public Mydb(string url)
    {

        conn = new MySqlConnection(url);
        if (conn.State == ConnectionState.Closed)
        {
            conn.Open();
            Console.WriteLine("connReader ON");
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
        string str_update = "UPDATE carrotPlugList.tb_product SET icc_id =" + icc_id + " where imei =" + imei + ';';
        return new MySqlCommand(str_update, conn).ExecuteNonQuery();
    }

    public int UpdateQuery_qa2(string imei, string icc_id, string qa2, string ng2_type)
    {
        if (icc_id == null)
            icc_id = "NULL";
        string str_update = "UPDATE carrotPlugList.tb_product SET icc_id =" + icc_id + ", qa2=\"" + qa2 + "\", ng2_type=\"" + ng2_type + "\" where imei =" + imei + ';';
        return new MySqlCommand(str_update, conn).ExecuteNonQuery();
    }

    public int UpdateQuery_qa3(string imei, string icc_id, string qa3, string ng3_type)
    {
        if (icc_id == null)
            icc_id = "NULL";
        string str_update = "UPDATE carrotPlugList.tb_product SET icc_id =" + icc_id + ", qa3=\"" + qa3 + "\", ng3_type=\"" + ng3_type + "\" where imei =" + imei + ';';
        return new MySqlCommand(str_update, conn).ExecuteNonQuery();
    }

    private int registries_server(string url, string host, string bearer)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        httpWebRequest.Method = "POST";
        httpWebRequest.Host = host;
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Headers.Add("Authorization", bearer);
        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = regPayload.ToString();
            streamWriter.Write(json);
            streamWriter.Flush();
            streamWriter.Close();
        }
        try
        {
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
            return (int)httpResponse.StatusCode;
        }
    }

    public int regist_server(string imei)
    {
        string retrun_text = "";
        int ret;
        GetProduct(imei);
        try
        {
            regPayload["deviceId"] = pluglist[imei].device_id;
            regPayload["cmBzpsRgtDscno"] = pluglist[imei].sn;
            regPayload["divcSrlNo"] = pluglist[imei].lot_no;
            regPayload["prddt"] = pluglist[imei].prod_data;
        }
        catch(KeyNotFoundException keyExcp)
        {
            return -1;
        }
        if (!pluglist[imei].testServerFlag)
        {
            ret = registries_server(testServerUrl, "t-dtag.carrotins.com", "Bearer 8PKLPS2623330268GXRAQK");

            if (ret == 200 || ret == 409)
            {
                pluglist[imei].testServerFlag = true;
                return 1;
            }
            else
            {
                return -2;
            }
        }

        if(!pluglist[imei].mainServerFlag)
        {
            ret = registries_server(mainServerUrl, "dtag.carrotins.com", "Bearer EJH6NE0851819521SSSA7M");
            if (ret == 200 || ret == 409)
            {
                pluglist[imei].mainServerFlag = true;
                return 1;
            }
        }
        return -3;
    }

    public Dictionary<string, Plug> GetProduct(string imei)
    {
        MySqlCommand cmd_select = new MySqlCommand("SELECT device_id, prod_date, lot_no, sn, imei, icc_id FROM carrotPlugList.tb_product where imei=" + imei+";", conn);
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
                data.icc_id = rdr["sn"].ToString();
            }
            catch
            {
                data.icc_id = "NULL";
            }
            try
            {
                data.prod_data = rdr["prod_data"].ToString();
            }
            catch
            {
                data.prod_data = "NULL";
            }
            try
            {
                data.lot_no = rdr["lot_no"].ToString();
            }
            catch
            {
                data.lot_no = "NULL";
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

        MySqlCommand cmd_select = new MySqlCommand("SELECT device_id, prod_date, lot_no, sn, imei, icc_id FROM carrotPlugList.tb_product LIMIT 0,1000; ", conn);
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
                data.icc_id = rdr["sn"].ToString();
            }
            catch
            {
                data.icc_id = "NULL";
            }
            try
            {
                data.prod_data = rdr["prod_data"].ToString();
            }
            catch
            {
                data.prod_data = "NULL";
            }
            try
            {
                data.lot_no = rdr["lot_no"].ToString();
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

    private uint flag = 0;
    private string _passFlag = "Fail";
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
                _passFlag = value;
                passFlagUpdate = true;
            }
        }
    }
    public bool passFlagUpdate = true;
    public string TagVersion = "";
    public string dbString = "NG";
    public bool dbFlag = false;
    public string serverString = "NG";
    public bool serverFlag = false;
    public DateTime updateTime;

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
    public string TagFlagString { get { return flagString; } set { flagString = value; OnPropertyChanged("TagFlagString"); } }
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
        this.TagName = taginfo.TagName;
        this.TagRssi = taginfo.TagRssi;
        this.TagData = taginfo.TagData;
        this.TagMenu = taginfo.TagMenu;
        this.TagVersion = taginfo.TagVersion;
        this.TagFlagString = taginfo.TagFlagString;
        this.passFlag = taginfo.passFlag;

        this.flag = taginfo.flag;
        this.updateTime = taginfo.updateTime;
}
}
