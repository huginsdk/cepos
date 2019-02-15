using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.Data;
using System.Data.SQLite;

namespace Hugin.POS.Data
{
    public class DBAdapter
    {
        private static SQLiteConnection sqlConn = null;
        private static SQLiteDataAdapter sda;
        private static DBAdapter dbAdapter = null;

        internal static DBAdapter Instance()
        {
            if (dbAdapter == null)
                dbAdapter = new DBAdapter();
            return dbAdapter;
        }

        private DBAdapter()
        {
            try
            {
                string path = PosConfiguration.DataPath + "HuginDB.db3";
                string password = "87654321";
                string cnnstr = "";

                cnnstr = String.Format("data source={0};", path);
                sqlConn = new SQLiteConnection(cnnstr);

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (sqlConn.State == ConnectionState.Open)
                    sqlConn.Close();

            }
        }

        public  DataSet GetDataSet(string strQery)
        {
            DataSet ds = new DataSet();
            sda = new SQLiteDataAdapter(strQery, sqlConn);
            sda.Fill(ds);
            return ds;
        }

        public  int UpdateDataSet(DataSet ds)
        {
            SQLiteCommandBuilder cmdBuilder = new SQLiteCommandBuilder(sda);
            
            return sda.Update(ds);
        }

        public int ExecuteNonQuery(string strQery)
        {
            SQLiteCommand cmd = new SQLiteCommand(strQery, sqlConn);
            int rowExecuted = 0;

            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }

            try
            {
                rowExecuted = cmd.ExecuteNonQuery();
            }
            catch
            {

            }
            finally
            {
                sqlConn.Close();
            }

            return rowExecuted;

        }


        public Object ExecuteQuery(string strQery)
        {
            SQLiteCommand sqlDbCmd;
            lock (sqlDbCmd = new SQLiteCommand(strQery, sqlConn))
            {
                SQLiteDataReader sqlDbReader = null;
                Object objValue = null;
                try
                {
                    Exception dbEx = null;
                    System.Threading.Thread t = new System.Threading.Thread(delegate()
                    {
                        try
                        {
                            if (sqlConn.State == ConnectionState.Closed)
                                sqlConn.Open();

                            sqlDbReader = sqlDbCmd.ExecuteReader();
                            if (sqlDbReader.Read())
                                objValue = sqlDbReader[0];
                            sqlDbReader.Close();
                        }
                        catch (Exception ex) { dbEx = ex; }
                    });
                    t.Start();
                    t.Join(1000);

                    if (dbEx != null)
                        throw dbEx;

                    return objValue;

                }
                finally
                {
                    if (sqlConn.State == ConnectionState.Open)
                        sqlConn.Close();
                }
            }
        }

        public int ExecuteCommand(SQLiteCommand sqlDbCmd)
        {
            sqlDbCmd.Connection = sqlConn;
            int rowsEffected = 0;
            try
            {
                System.Threading.Thread t = new System.Threading.Thread(delegate()
                {
                    try
                    {
                        if (sqlConn.State == ConnectionState.Closed)
                            sqlConn.Open();

                        rowsEffected = sqlDbCmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) { }
                });
                t.Start();
                t.Join(1000);

                return rowsEffected;
            }
            catch (Exception ex)
            {
                return rowsEffected;
            }
            finally
            {
                if (sqlConn.State == ConnectionState.Open)
                    sqlConn.Close();
            }
        }
    }
}
