using UnityEngine;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryDB
    {

        // connection object
        MySqlConnection con = null;
        // command object
        MySqlCommand cmd = null;
        // reader object
        MySqlDataReader rdr = null;

        private static GrayboxLibraryDB instance;

        private GrayboxLibraryDB() { }

        public static GrayboxLibraryDB GetInstance()
        {
            if (instance == null)
                instance = new GrayboxLibraryDB();

            return instance;
        }


        //Opens the connection to the DB
        public void Connect()
        {
            try
            {
                con = new MySqlConnection(GrayboxLibraryConfig.DB_CONFIG);
                con.Open();
            }
            catch (Exception ex)
            {
                ex.ToString();
                //Debug.Log(ex.ToString());
            }
        }


        //Execute a query and returns the result
        public ArrayList ExecuteQuery(string sql)
        {
            ArrayList result = new ArrayList();

            try
            {
                if (con.State.ToString() != "Open")
                    con.Open();

                using (con)
                {
                    using (cmd = new MySqlCommand(sql, con))
                    {
                        rdr = cmd.ExecuteReader();
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                if (rdr[0].ToString().Length > 0)
                                {

                                    Dictionary<string, string> row = new Dictionary<string, string>();

                                    for (int column = 0; column < rdr.FieldCount; column++)
                                        row.Add(rdr.GetName(column), rdr[column].ToString());

                                    result.Add(row);
                                }
                            }
                        }
                        rdr.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                Debug.LogError(ex.ToString());
            }

            return result;

        }


        //Execute a query and returns the result
        public void ExecuteSQL(string sql)
        {
            try
            {
                if (con.State.ToString() != "Open")
                    con.Open();

                using (con)
                {
                    using (cmd = new MySqlCommand(sql, con))
                        cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                //Debug.Log(ex.ToString());
            }
        }


        //Closes the connection to the DB
        public void Disconnect()
        {
            if (con != null)
            {
                MySqlConnection.ClearPool(con);
                if (con.State.ToString() != "Closed")
                    con.Close();
                con.Dispose();
            }
        }
    }
}