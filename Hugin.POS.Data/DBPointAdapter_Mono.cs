using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.Data;
using System.Threading;
using System.IO;

namespace Hugin.POS.Data
{
    internal class DBPointAdapter :IPointAdapter
    {
       
        internal DBPointAdapter(String path)
        {
        }

        #region IPointAdapter Members

        public bool Online
        {
            get
            {
                bool retVal = false;

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
			/*
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
                throw new CardSerialInsertException();*/
        }

        public void UpdatePoint(PointObject pointObj)
        {
			/*
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

            OleDbCommand command = new OleDbCommand(strUpdate, oleDbCnn);

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

            command = new OleDbCommand(strUpdate, oleDbCnn);

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

            retVal = ExecuteCommand(command);*/
        }

        public long GetPoint(ICustomer customer)
        {
			return 0;
			/*
            string strSelect = "SELECT Count(CustomerCode) FROM TblDaily WHERE CustomerCode='" + customer.Code + "'";

            int count = Convert.ToInt32(GetOleDbValue(strSelect));
            if (count > 0)
            {
                strSelect = "SELECT CustomerPoints FROM TblDaily WHERE CustomerCode='" + customer.Code + "'";
                return Convert.ToInt64(GetOleDbValue(strSelect));
            }
            else
                throw new CustomerNotInPointDBException();*/
        }
       
        public bool Invalid(string cardSerial)
        {
			return false;
			/*
            string strSelect = "SELECT Count(CustomerId) FROM Cards WHERE CardSerial = '" + cardSerial + "'";

            int count = Convert.ToInt32(GetOleDbValue(strSelect));
            if (count > 0)
            {
                strSelect = "SELECT Valid FROM Cards WHERE CardSerial = '" + cardSerial + "'";
                int valid = Convert.ToInt32(GetOleDbValue(strSelect));
                return valid == 1;
            }
            else
                throw new CardSerialNotInPointDBException();*/
        }

        public int InvalidateSerials(ICustomer customer)
        {
			return 0;
			/*
            String strUpdate = "UPDATE Cards SET Valid = 0 WHERE CustomerId = '" + customer.Number + "'";
            return ExecuteNonQuery(strUpdate);*/
        }

        #endregion
    }
}
