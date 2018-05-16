using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace Fato_Service
{
    public class Logger
    {
        private string scompid = string.Empty;
        private string strSessionID = string.Empty;
        private Boolean _WriteLogs = false;
        public string sCompanyID
        {
            get { return scompid; }
            set { scompid = value; }
        }

        public string sSessionID
        {
            get { return strSessionID; }
            set { strSessionID = value; }
        }

        public Boolean WriteLogs
        {
            get { return _WriteLogs; }
            set { _WriteLogs = value; }
        }

        internal string Log(string MethodName_, string errorMsg_, string Searchtype)
        {
            string logpath = string.Empty;
            try
            {
                if (_WriteLogs)
                {
                    object lockIndex = new object();

                    string filepath = string.Empty;
                    XmlDocument xmlConfigDetails = new XmlDocument();


                    DataAccess_Net.DataAccess objfbDataAcess = new DataAccess_Net.DataAccess();
                    logpath = objfbDataAcess.fnDataAccess("ConfigParams/Log", ",");
                    xmlConfigDetails.LoadXml(logpath);

                    logpath = xmlConfigDetails.DocumentElement.SelectSingleNode("Log").Attributes["Path"].Value;

                    if (Searchtype.ToUpper() == "FATOSERVICE")
                        logpath += "BookingEngine\\" + scompid + "\\" + Searchtype + "\\";
                    


                    FileInfo objFileInfo;
                    lock (lockIndex)
                    {
                        if (!Directory.Exists(logpath))
                        {
                            Directory.CreateDirectory(logpath);
                        }
                        filepath = logpath + "Fato_Service" + DateTime.Today.ToString("dd-MMM-yyyy") + ".log";

                        objFileInfo = new FileInfo(filepath);

                        FileStream fs;
                        if (objFileInfo.Exists)
                        {
                            if (objFileInfo.Length > 5048576)
                            {
                                filepath = logpath + strSessionID + DateTime.Today.ToString("dd-MMM-yyyy") + DateTime.Now.ToString("H-m-s") + ".log";
                                fs = new FileStream(filepath, FileMode.Create, FileAccess.ReadWrite);
                            }
                            else
                            {
                                fs = new FileStream(filepath, FileMode.Append, FileAccess.Write);
                            }
                        }
                        else
                        {
                            fs = new FileStream(filepath, FileMode.Create, FileAccess.ReadWrite);
                        }

                        TextWriter m_streamWriter = new StreamWriter(fs);

                        if (!string.IsNullOrEmpty(errorMsg_))
                        {
                            m_streamWriter.WriteLine("      ============================================================================================================================================");
                            //m_streamWriter.WriteLine("Time           :: " + System.DateTime.Now );
                            m_streamWriter.WriteLine("      Method Name    :: " + MethodName_);
                            m_streamWriter.WriteLine(Environment.NewLine + "      " + errorMsg_);
                            //m_streamWriter.WriteLine("============================================================================================================================================");
                        }
                        else
                        {
                            m_streamWriter.WriteLine("*************************************************************************************************************************************************************************************");

                        }
                        m_streamWriter.Close();
                        m_streamWriter.Dispose();

                    }

                }
            }
            catch (Exception _ex) //if error comes in log writing
            {
                return "Error: Failed to Write Log" + _ex.Message;
                //throw new System.ApplicationException(_ex.ToString());
            }
            return logpath;
        }

        
        public string DBConnect(String CompanyId, string DataBaseType)
        {
            String UId;
            String provider;
            String pass;
            String dsource;
            String incat;
            String mystring;
            string mycon = string.Empty;
            XmlDocument xmDataAccess;
            xmDataAccess = new XmlDocument();
            DataAccess_Net.DataAccess objda = new DataAccess_Net.DataAccess();

            if (!string.IsNullOrEmpty(DataBaseType))
                mystring = "/ConfigParams/Company/" + CompanyId + "/" + DataBaseType;
            else
                mystring = "/ConfigParams/AdminDB,/ConfigParams/subagent,/ConfigParams/Log";

            mystring = objda.fnDataAccess(mystring, ",");
            mystring = mystring.Replace("'", "\"");
            xmDataAccess.LoadXml(mystring);

            try
            {
                if (xmDataAccess.DocumentElement.Name != "Error")
                {
                    if (xmDataAccess.DocumentElement.HasChildNodes)
                    {
                        if (!string.IsNullOrEmpty(DataBaseType))
                        {
                            provider = xmDataAccess.DocumentElement.SelectSingleNode(DataBaseType).Attributes.GetNamedItem("Provider").InnerText;
                            UId = xmDataAccess.DocumentElement.SelectSingleNode(DataBaseType).Attributes.GetNamedItem("UserID").InnerText;
                            pass = xmDataAccess.DocumentElement.SelectSingleNode(DataBaseType).Attributes.GetNamedItem("Password").InnerText;
                            dsource = xmDataAccess.DocumentElement.SelectSingleNode(DataBaseType).Attributes.GetNamedItem("DataSource").InnerText;
                            incat = xmDataAccess.DocumentElement.SelectSingleNode(DataBaseType).Attributes.GetNamedItem("DBName").InnerText;
                        }
                        else
                        {
                            provider = xmDataAccess.DocumentElement.SelectSingleNode("AdminDB").Attributes.GetNamedItem("Provider").InnerText;
                            UId = xmDataAccess.DocumentElement.SelectSingleNode("AdminDB").Attributes.GetNamedItem("UserID").InnerText;
                            pass = xmDataAccess.DocumentElement.SelectSingleNode("AdminDB").Attributes.GetNamedItem("Password").InnerText;
                            dsource = xmDataAccess.DocumentElement.SelectSingleNode("AdminDB").Attributes.GetNamedItem("DataSource").InnerText;
                            incat = xmDataAccess.DocumentElement.SelectSingleNode("AdminDB").Attributes.GetNamedItem("DBName").InnerText;
                        }
                        mycon = "user id='" + UId + "';password='" + pass + "';data source='" + dsource + "'; initial catalog='" + incat + "'";

                    }
                }
            }
            catch (Exception ex)
            {
               // log.Log("Initializeskyvantage()", Environment.NewLine + "Exception occured in DBConnect function :" + ex.StackTrace + Environment.NewLine + ex.InnerException + Environment.NewLine + ex.Message + Environment.NewLine, "Caching");

            }
            return mycon;
        }

        public Boolean FnComp_WriteLog(string compID)
        {
            Boolean WriteLogs = false;
            try
            {

                SqlConnection objSqlCon = new SqlConnection(DBConnect(compID, "CBD"));
                //objSqlCon = DBConnect(compID, "CBD");
                SqlCommand objSqlCmd = new SqlCommand("", objSqlCon);
                objSqlCmd.CommandType = CommandType.StoredProcedure;
                objSqlCmd.CommandText = "Fsp_Write_Logs";
                //adding parameter
                objSqlCmd.Parameters.Add("@CompID", compID);
                if (objSqlCmd.Connection.State == ConnectionState.Closed) objSqlCmd.Connection.Open();
                WriteLogs = Convert.ToBoolean(objSqlCmd.ExecuteScalar());

                objSqlCon.Dispose();


            }
            catch (Exception ex)
            {
                throw ex;

            }
            return WriteLogs;
        }

        public static DataTable ExecuteDatatable(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cn.Open();
                return ExecuteDatatable(cn, commandType, commandText, commandParameters);
            }
        }
        public static DataTable ExecuteDatatable(string connectionString, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(commandParameters, parameterValues);
                return ExecuteDatatable(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDatatable(connectionString, CommandType.StoredProcedure, spName);
            }
        }
        public static DataTable ExecuteDatatable(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            cmd.Parameters.Clear();

            cmd.CommandTimeout = 1800;

            return dt;
        }

        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            command.Connection = connection;
            command.CommandText = commandText;
            if (transaction != null)
            {
                command.Transaction = transaction;
            }
            command.CommandType = commandType;
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
            return;
        }
        private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            foreach (SqlParameter p in commandParameters)
            {
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }
                command.Parameters.Add(p);
            }
        }
        public sealed class SqlHelperParameterCache
        {
            private SqlHelperParameterCache() { }
            private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

            private static SqlParameter[] DiscoverSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
            {
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(spName, cn))
                {
                    cn.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlCommandBuilder.DeriveParameters(cmd);

                    if (!includeReturnValueParameter)
                    {
                        cmd.Parameters.RemoveAt(0);
                    }

                    SqlParameter[] discoveredParameters = new SqlParameter[cmd.Parameters.Count];
                    cmd.Parameters.CopyTo(discoveredParameters, 0);
                    cn.Close();
                    return discoveredParameters;
                }
            }
            private static SqlParameter[] CloneParameters(SqlParameter[] originalParameters)
            {
                SqlParameter[] clonedParameters = new SqlParameter[originalParameters.Length];
                for (int i = 0, j = originalParameters.Length; i < j; i++)
                {
                    clonedParameters[i] = (SqlParameter)((ICloneable)originalParameters[i]).Clone();
                }
                return clonedParameters;
            }
            public static void CacheParameterSet(string connectionString, string commandText, params SqlParameter[] commandParameters)
            {
                string hashKey = connectionString + ":" + commandText;
                paramCache[hashKey] = commandParameters;
            }
            public static SqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
            {
                string hashKey = connectionString + ":" + commandText;
                SqlParameter[] cachedParameters = (SqlParameter[])paramCache[hashKey];
                if (cachedParameters == null)
                {
                    return null;
                }
                else
                {
                    return CloneParameters(cachedParameters);
                }
            }
            public static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
            {
                return GetSpParameterSet(connectionString, spName, false);
            }
            public static SqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
            {
                string hashKey = connectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");
                SqlParameter[] cachedParameters;
                cachedParameters = (SqlParameter[])paramCache[hashKey];
                //if (cachedParameters == null)
                //{			
                cachedParameters = (SqlParameter[])(paramCache[hashKey] = DiscoverSpParameterSet(connectionString, spName, includeReturnValueParameter));
                //}			
                return CloneParameters(cachedParameters);
            }
        }

        private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                return;
            }
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                commandParameters[i].Value = parameterValues[i];
            }
        }              
        
    }
}
