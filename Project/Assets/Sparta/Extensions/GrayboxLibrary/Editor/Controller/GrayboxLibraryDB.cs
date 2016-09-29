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
        private MySqlConnection _con = null;
        // reader object
        private MySqlDataReader _rdr = null;

        private static GrayboxLibraryDB _instance;

        private GrayboxLibraryDB()
        {
        }

        public static GrayboxLibraryDB GetInstance()
        {
            if(_instance == null)
                _instance = new GrayboxLibraryDB();

            return _instance;
        }


        //Opens the connection to the DB
        public void Connect()
        {
            try
            {
                _con = new MySqlConnection(GrayboxLibraryConfig.DbConfig);
                _con.Open();
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }


        //Execute a query and returns the result
        public ArrayList ExecuteQuery(MySqlCommand cmd)
        {
            ArrayList result = new ArrayList();

            try
            {
                if(_con.State.ToString() != "Open")
                    _con.Open();

                using(_con)
                {
                    using(cmd)
                    {
                        cmd.Connection = _con;
                        _rdr = cmd.ExecuteReader();
                        if(_rdr.HasRows)
                        {
                            while(_rdr.Read())
                            {
                                if(_rdr[0].ToString().Length > 0)
                                {
                                    Dictionary<string, string> row = new Dictionary<string, string>();

                                    for(int column = 0; column < _rdr.FieldCount; column++)
                                        row.Add(_rdr.GetName(column), _rdr[column].ToString());

                                    result.Add(row);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.ToString());
            }

            return result;

        }


        //Execute a query and returns the result
        public void ExecuteSQL(MySqlCommand cmd)
        {
            try
            {
                if(_con.State.ToString() != "Open")
                    _con.Open();

                using(_con)
                {
                    using (cmd)
                    {
                        cmd.Connection = _con;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }


        //Closes the connection to the DB
        public void Disconnect()
        {
            if(_con != null)
            {
                MySqlConnection.ClearPool(_con);
                if(_con.State.ToString() != "Closed")
                    _con.Close();
                _con.Dispose();
            }
        }
    }
}