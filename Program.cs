using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace CarrotDb
{
    class mydb
    {
        private MySqlConnection conn;
        private string ConnUrl = "Server=release-carrot-cluster.cluster-ro-cb10can9foe2.ap-northeast-2.rds.amazonaws.com;Database=carrotPlugList;Uid=luxrobo;Pwd=fjrtmfhqh123$;";
        public MySqlDataReader rdr;
        private Dictionary<string, string> pluglist = new Dictionary<string, string>();

        public mydb()
        {
            conn = new MySqlConnection(ConnUrl);
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
                Console.WriteLine("connReader ON");
            }
        }

        public mydb(string url)
        {

            conn = new MySqlConnection(url);
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
                Console.WriteLine("connReader ON");
            }
        }
        ~mydb()
        {
            conn.Close();
        }

        public void updateQuery(string imei, string icc_id)
        {
            if (icc_id == null)
                icc_id = "NULL";
            string str_update = "UPDATE carrotPlugList.tb_product SET icc_id =" + icc_id + " where imei =" + imei + ';';
            new MySqlCommand(str_update, conn).ExecuteNonQuery();
        }

        public void updateQuery(KeyValuePair<string, string> plug)
        {
            string icc_id = plug.Value;
            if (icc_id == null)
                icc_id = "NULL";
            string str_update = "UPDATE carrotPlugList.tb_product SET icc_id =" + icc_id + " where imei =" + plug.Key + ';';
            new MySqlCommand(str_update, conn).ExecuteNonQuery();
        }
        public Dictionary<string, string> getList()
        {
            if (pluglist.Count == 0) reflashList();
            return pluglist;
        }
        public Dictionary<string, string> reflashList()
        {

            MySqlCommand cmd_select = new MySqlCommand("SELECT sn, imei, icc_id FROM carrotPlugList.tb_product LIMIT 0,1000; ", conn);
            pluglist.Clear();
            rdr = cmd_select.ExecuteReader();
            while (rdr.Read())
            {

                //Console.WriteLine("{0}\t{1}\t{2}", rdr["sn"], rdr["imei"], rdr["icc_id"]);
                pluglist.Add(rdr["imei"].ToString(), rdr["icc_id"].ToString());
            }
            rdr.Dispose();
            return pluglist;
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            // release-carrot-cluster.cluster-ro-cb10can9foe2.ap-northeast-2.rds.amazonaws.com  readonly
            // release-carrot-cluster.cluster-cb10can9foe2.ap-northeast-2.rds.amazonaws.com     read/write
            mydb mydb = new();
            Dictionary<string, string> pluglist = mydb.getList();
            Int16 count = 0;

            Console.WriteLine("\nBefore DB");
            foreach (KeyValuePair<string, string> plug in pluglist)
            {
                Console.WriteLine("{0}\t{1}", plug.Key, plug.Value);
            }

            foreach (KeyValuePair<string, string> plug in pluglist)
            {
                count+= 2;
                mydb.updateQuery(plug.Key, count.ToString());
            }
            Console.WriteLine("\nUpdated DB");
            pluglist = mydb.reflashList();

            foreach (KeyValuePair<string, string> plug in pluglist)
            {
                Console.WriteLine("{0}\t{1}", plug.Key, plug.Value);
            }
        }
    }
}