using System;
using System.Data;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace Fato_Service
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Logger objlogger = new Logger();
            StringBuilder strLog = new StringBuilder();
            XmlDocument xComp = new XmlDocument();
            try
            {
                string strCompId = "BC";
                string ConnectionString = string.Empty;
                objlogger.WriteLogs = true;
                objlogger.sCompanyID = strCompId;

#if DEBUG
                ConnectionString = "Server=XXXX;Initial Catalog=XXXX;Integrated Security=False;User ID=XXXX;Password=XXXX;";
#else
                ConnectionString = objlogger.DBConnect(strCompId, "CBD");
#endif

                DataTable dt_FATO_Insert_Update = Logger.ExecuteDatatable(ConnectionString, "SP_FATO_Insert_update", null, null,null, strCompId, 1, 1);
                if (dt_FATO_Insert_Update.Rows.Count > 0)
                {
                    DateTime FATO_Insert_Date = Convert.ToDateTime(dt_FATO_Insert_Update.Rows[0]["Fato_insert_date"]);
                    DateTime CurrentDate = DateTime.Parse(DateTime.Now.ToString("dd/MM/yyyy HH:00:00"));

                    if (CurrentDate > FATO_Insert_Date)
                    {
                        Logger.ExecuteDatatable(ConnectionString, "SP_FATO_Insert_update", CurrentDate.AddHours(-1), CurrentDate, null, strCompId, 2, 1);
                        strLog.Append(Environment.NewLine + "Insert Records In FATO TABLE : " + DateTime.Now + Environment.NewLine + Environment.NewLine);
                    }

                    Logger.ExecuteDatatable(ConnectionString, "SP_FATO_Insert_update", null, null, null, strCompId, 3, 1);
                    strLog.Append(Environment.NewLine + "Update Records In FATO TABLE : " + DateTime.Now + Environment.NewLine + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                objlogger.WriteLogs = true;
                strLog.Append(Environment.NewLine + "Exception in FATO Service : " + DateTime.Now + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
            }
            finally
            {
                objlogger.Log("FATO Service", strLog.ToString(), "FATOSERVICE");
                Application.Exit();
            }
        }
    }
}
