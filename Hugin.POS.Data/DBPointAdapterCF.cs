using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Hugin.POS.Common;
using System.Threading;
using Hugin.POS.Data;
using System.IO;
using System.Data.SqlClient;

namespace Hugin.POS.Data
{

    internal class DBPointAdapter : IPointAdapter
    {
        private static SqlConnection oleDbCnn = null;
        private static Thread thread;
        private bool ControllerStarted = false;

        internal DBPointAdapter(String path)
        {
            try
            {
                if (!System.IO.File.Exists(path))
                    path = path + "\\PuanDB.sdf";

                oleDbCnn = new SqlConnection(String.Format("Data Source=\"{0}\";Persist Security Info=False;", path));
                GetOleDbValue("SELECT Count(CustomerCode) FROM TblDaily");
            }
            catch
            {
                if (HasOfflineRecords)
                    StartDBController();
            }
            finally
            {
                if (oleDbCnn.State == ConnectionState.Open)
                    oleDbCnn.Close();

            }
        }

        ~DBPointAdapter()
        {
            try
            {
                if (oleDbCnn != null) oleDbCnn.Close();
            }
            catch { }

            try
            {
                if (thread != null) thread.Abort();
            }
            catch { }
        }

        #region IPointAdapter Members

        public bool Online
        {
            get
            {
                bool retVal = false;

                try
                {
                    GetOleDbValue("SELECT Count(CustomerCode) FROM TblDaily");
                    retVal = true;
                }
                catch { }

                return retVal;
            }
        }

        private string LogPath
        {
            get
            {
                return PosConfiguration.ArchivePath + Settings.DBRestoreFile;
            }
        }
        public bool HasOfflineRecords
        {
            get
            {
                return File.Exists(this.LogPath);
            }
        }

        public void AddCard(ICustomer customer, string cardSerial)
        {
            if (cardSerial == "")
                cardSerial = String.Format("{0:ddMMyyyyHHmmss}", DateTime.Now);

            string strSelect = "SELECT Count(CardSerial) FROM Cards WHERE CardSerial='" + cardSerial + "'";
            int count = Convert.ToInt32(GetOleDbValue(strSelect));
            if (count > 0)
                throw new CardSerialAlreadyExistsException();

            String strInsert = "INSERT INTO Cards(CustomerId, CardSerial, Valid) "
               + " VALUES('" + customer.Number + "', '" + cardSerial + "', 1 )";

            count = ExecuteNonQuery(strInsert);

            if (count <= 0)
                throw new CardSerialInsertException();
        }

        public void UpdatePoint(PointObject pointObj)
        {
            String strUpdate = String.Empty;
            Int64 point = pointObj.Value;
            Int32 retVal = 0;

            EZLogger.Log.Debug(String.Format("Müþteri puaný kayýt ediliyor. M:{0}  P:{1}", pointObj.Customer.Code, pointObj.Customer.Points));

            try
            {
                point += GetPoint(pointObj.Customer);
                strUpdate = "UPDATE TblDaily " +
                            "SET CustomerPoints = @points " +
                            "WHERE CustomerCode=@customerCode";

            }
            catch (CustomerNotInPointDBException)
            {
                strUpdate = "INSERT INTO TblDaily(CustomerPoints, CustomerCode) " +
                            "VALUES(@points, @customerCode)";
            }
            catch (Exception)
            {
                AppendOfflineQuery(pointObj.ToString());
                StartDBController();
                throw new UpdatePointException();
            }

            SqlCommand command = new SqlCommand(strUpdate, oleDbCnn);

            command.Parameters.AddWithValue("@points", point);
            command.Parameters.AddWithValue("@customerCode", pointObj.Customer.Code);

            retVal = ExecuteCommand(command);

            if (retVal <= 0)
            {
                AppendOfflineQuery(pointObj.ToString());
                StartDBController();
                throw new UpdatePointException();
            }

            EZLogger.Log.Debug(String.Format("Müþteri puaný kayýt edildi. M:{0}  P:{1}", pointObj.Customer.Code, pointObj.Customer.Points));

            strUpdate = "INSERT INTO TblLog(Points, CustomerCode, DocumentDate, OfficeID, RegisterID, RegisterFiscalID, ZNo, DocumentNo, DocumentTotal, DocumentTypeID, Description) " +
                        "VALUES(@points, @customerCode, @documentDate, @officeId, @registerId, @fiscalId, @zno, @documentId, @documentTotal,@documentTypeId, @description)";

            command = new SqlCommand(strUpdate, oleDbCnn);

            command.Parameters.AddWithValue("@points", pointObj.Value);
            command.Parameters.AddWithValue("@customerCode", pointObj.Customer.Code);
            command.Parameters.AddWithValue("@documentDate", pointObj.DocumentDate.ToString());
            command.Parameters.AddWithValue("@officeId", pointObj.OfficeID);
            command.Parameters.AddWithValue("@registerId", pointObj.RegisterID);
            command.Parameters.AddWithValue("@fiscalId", pointObj.RegisterFiscalID);
            command.Parameters.AddWithValue("@zno", pointObj.ZNo);
            command.Parameters.AddWithValue("@documentId", pointObj.DocumentNo);
            command.Parameters.AddWithValue("@documentTotal", pointObj.DocumentTotal);
            command.Parameters.AddWithValue("@documentTypeId", pointObj.DocumentTypeID);
            command.Parameters.AddWithValue("@description", pointObj.Description);

            retVal = ExecuteCommand(command);
        }

        public long GetPoint(ICustomer customer)
        {
            string strSelect = "SELECT Count(CustomerCode) FROM TblDaily WHERE CustomerCode='" + customer.Code + "'";

            int count = Convert.ToInt32(GetOleDbValue(strSelect));
            if (count > 0)
            {
                strSelect = "SELECT CustomerPoints FROM TblDaily WHERE CustomerCode='" + customer.Code + "'";
                return Convert.ToInt64(GetOleDbValue(strSelect));
            }
            else
                throw new CustomerNotInPointDBException();
        }

        public bool Invalid(string cardSerial)
        {
            string strSelect = "SELECT Count(CustomerId) FROM Cards WHERE CardSerial = '" + cardSerial + "'";

            int count = Convert.ToInt32(GetOleDbValue(strSelect));
            if (count > 0)
            {
                strSelect = "SELECT Valid FROM Cards WHERE CardSerial = '" + cardSerial + "'";
                int valid = Convert.ToInt32(GetOleDbValue(strSelect));
                return valid == 1;
            }
            else
                throw new CardSerialNotInPointDBException();
        }

        public int InvalidateSerials(ICustomer customer)
        {
            String strUpdate = "UPDATE Cards SET Valid = 0 WHERE CustomerId = '" + customer.Number + "'";
            return ExecuteNonQuery(strUpdate);
        }

        #endregion

        private int ExecuteNonQuery(string strQuery)
        {
            SqlCommand oleDbCmd = new SqlCommand(strQuery, oleDbCnn);
            int rowsEffected = 0;
            try
            {
                Thread t = new Thread(delegate()
                  {
                      try
                      {
                          if (oleDbCnn.State == ConnectionState.Closed)
                              oleDbCnn.Open();

                          rowsEffected = oleDbCmd.ExecuteNonQuery();
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
                if (oleDbCnn.State == ConnectionState.Open)
                    oleDbCnn.Close();
            }
        }

        private int ExecuteCommand(SqlCommand sqlDbCmd)
        {
            int rowsEffected = 0;
            try
            {
                Thread t = new Thread(delegate()
                {
                    try
                    {
                        if (oleDbCnn.State == ConnectionState.Closed)
                            oleDbCnn.Open();

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
                if (oleDbCnn.State == ConnectionState.Open)
                    oleDbCnn.Close();
            }
        }

        private Object GetOleDbValue(string strSelect)
        {
            SqlCommand oleDbCmd = new SqlCommand(strSelect, oleDbCnn);
            SqlDataReader oleDbReader = null;
            Object objValue = null;
            try
            {
                Exception dbEx = null;
                Thread t = new Thread(delegate()
                {
                    try
                    {
                        if (oleDbCnn.State == ConnectionState.Closed)
                            oleDbCnn.Open();

                        oleDbReader = oleDbCmd.ExecuteReader();
                        if (oleDbReader.Read())
                            objValue = oleDbReader[0];
                        oleDbReader.Close();
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
                if (oleDbCnn.State == ConnectionState.Open)
                    oleDbCnn.Close();
            }
        }
        #region Offline record control

        private void StartDBController()
        {
            if (ControllerStarted)
                return;
            ControllerStarted = true;
            thread = new Thread(new ThreadStart(ControlServerForUpdates));
            thread.Name = "DBControllerForUpdates";
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }

        private void AppendOfflineQuery(String query)
        {
           IOUtil.AppendAllText(LogPath, query + "\r\n");
        }

        private void ControlServerForUpdates()
        {
            bool success = false;
            while (true)
            {
                if (!this.HasOfflineRecords)
                {
                    ControllerStarted = false;
                    thread.Abort();
                    break;
                }

                success = false;

                if (Online) success = true;

                if (!success)
                {
                    Thread.Sleep(5000);
                    continue;
                }

                List<String> lines = new List<string>(IOUtil.ReadAllLines(LogPath));
                if (File.Exists(LogPath))
                    File.Delete(LogPath);

                while (lines.Count > 0)
                {
                    String strQuery = lines[0];
                    try
                    {
                        UpdatePoint(StringToObject(strQuery));
                        lines.RemoveAt(0);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        EZLogger.Log.Error("Hatalý offline puan satýrý : " + strQuery);
                        lines.RemoveAt(0);
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                }

                if (lines.Count > 0)
                {
                    foreach (String strQuery in lines)
                        AppendOfflineQuery(strQuery);
                }
            }
        }

        private PointObject StringToObject(string strQuery)
        {
            try
            {
                String[] splitted = Str.Split(strQuery, '&');

                PointObject po = new PointObject();
                po.Value = Convert.ToInt64(splitted[0]);
                po.Customer = Customer.FindByCode(splitted[1]);
                po.DocumentDate = DateTime.Parse(splitted[2]);
                po.OfficeID = splitted[3];
                po.RegisterID = splitted[4];
                po.RegisterFiscalID = splitted[5];
                po.ZNo = Convert.ToInt32(splitted[6]);
                po.DocumentNo = Convert.ToInt32(splitted[7]);
                po.DocumentTotal = Decimal.Parse(splitted[8]);
                po.DocumentTypeID = Convert.ToInt32(splitted[9]);
                po.Description = splitted[10];

                return po;
            }
            catch (Exception)
            {
                throw new ArgumentOutOfRangeException();
            }
        }
        #endregion

    }
}