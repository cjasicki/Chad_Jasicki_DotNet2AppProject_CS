using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Chad_Jasicki_DotNet2AppProject_CS
{
    class clsDatabase
    {
        //***********************************************************
        //**  Procedure:  AcquireConnection()
        //**    Opens a connection using the default database
        //***********************************************************
        private static SqlConnection AcquireConnection()
        {
            return AcquireConnection("Payroll");
        }

        //***********************************************************
        //**  Procedure:  AcquireConnection()
        //**  Description:
        //**    Opens a connection using the specified connection
        //**  NOTE: Overloaded class to allow SQL connection creation
        //**        by external calls.
        //***********************************************************
        public static SqlConnection AcquireConnection(String strConnName)
        {
            SqlConnection cnSQL = null;
            Boolean blnErrorOccurred = false;

            //** Verify parameter
            if (strConnName.Trim().Length < 1)
            {
                blnErrorOccurred = true;
            }
            else if (ConfigurationManager.ConnectionStrings[strConnName] == null)
            {
                blnErrorOccurred = true;
            }
            else
            {
                cnSQL = new SqlConnection();
                cnSQL.ConnectionString = ConfigurationManager.ConnectionStrings[strConnName].ToString();

                try
                {
                    cnSQL.Open();
                }
                catch (Exception ex)
                {
                    blnErrorOccurred = true;
                    cnSQL.Dispose();
                }
            }

            if (blnErrorOccurred)
            {
                return null;
            }
            else
            {
                return cnSQL;
            }
        }
        //** Procedure: InsertPayroll()
        public static Boolean InsertEmployee(string Lname, string strMinit, string Fname, string SSAN, string PayRate)
        {
            SqlConnection cnSQL;
            SqlCommand cmdSQL;
            cnSQL = AcquireConnection();
            if (cnSQL != null)
            {
                cmdSQL = new SqlCommand();
                cmdSQL.Connection = cnSQL;
                cmdSQL.CommandType = CommandType.StoredProcedure;
                cmdSQL.CommandText = "[InsertEmployee]";
                cmdSQL.Parameters.AddWithValue("@NewEmpID", 0);
                cmdSQL.Parameters.AddWithValue("@LName", Lname);
                cmdSQL.Parameters.AddWithValue("@FName", Fname);
                if (string.IsNullOrWhiteSpace(strMinit))
                {
                    cmdSQL.Parameters.AddWithValue("@Minit", DBNull.Value);
                }
                else
                {
                    cmdSQL.Parameters.AddWithValue("@Minit", strMinit);
                }
                
                cmdSQL.Parameters.AddWithValue("@SSAN", SSAN);
                cmdSQL.Parameters.AddWithValue("@PayRate", Convert.ToDecimal(PayRate));
                try
                {
                    cmdSQL.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally
                {
                    cmdSQL.Parameters.Clear();
                    cmdSQL.Dispose();
                    cnSQL.Close();
                    cnSQL.Dispose();

                }
            }
            return true;
        }
        public static DataSet EmployeeExists(string SSAN)
        {
            SqlConnection cnSQL;
            SqlCommand cmdSQL;
            SqlDataAdapter daSQL;
            DataSet dsSQL = null;
            cnSQL = AcquireConnection();

            if (cnSQL != null)
            {
                cmdSQL = new SqlCommand();
                cmdSQL.Connection = cnSQL;
                cmdSQL.CommandType = CommandType.StoredProcedure;
                cmdSQL.CommandText = "[GetEmployeeByFirstLast]";
                cmdSQL.Parameters.AddWithValue("@SSAN", SqlDbType.NChar);
                cmdSQL.Parameters["@SSAN"].Direction = ParameterDirection.Input;
                cmdSQL.Parameters["@SSAN"].Value = SSAN;

                cmdSQL.Parameters.Add(new SqlParameter("@ErrCode", SqlDbType.Int));
                cmdSQL.Parameters["@ErrCode"].Direction = ParameterDirection.ReturnValue;
                dsSQL = new DataSet();
                try
                {
                    //cmdSQL.ExecuteNonQuery();
                    daSQL = new SqlDataAdapter(cmdSQL);
                    daSQL.Fill(dsSQL);
                    dsSQL.Dispose();
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    cmdSQL.Parameters.Clear();
                    cmdSQL.Dispose();
                    cnSQL.Close();
                    cnSQL.Dispose();
                }
            }
            return dsSQL;
        }
    }
}
