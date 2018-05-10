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
           // string Path = Assembly.GetExecutingAssembly().CodeBase.Replace(".EXE", "").Replace(".exe", "") + "//FATOConfig.xml";
           // xComp.Load(Path);
            try
            {
                //foreach (XmlNode compId in xComp.DocumentElement.SelectNodes("//CompanyRegistered/Company"))
                // {
                string strCompId = "BC";
                string ConnectionString = string.Empty;
                objlogger.WriteLogs = true;
                objlogger.sCompanyID = strCompId;
                ConnectionString = objlogger.DBConnect(strCompId, "CBD");
                DataTable dt_FATO_Insert_Update = Logger.ExecuteDatatable(ConnectionString, "SP_FATO_Insert_update", null, null,null, strCompId, 1);
                if (dt_FATO_Insert_Update.Rows.Count > 0)
                {
                    DateTime FATO_Insert_Date = Convert.ToDateTime(dt_FATO_Insert_Update.Rows[0]["Fato_insert_date"]);

                    if (DateTime.Now.Date > FATO_Insert_Date.AddDays(1).Date)
                    {
                        Logger.ExecuteDatatable(ConnectionString, "SP_FATO_Insert_update", DateTime.Now.Date.AddDays(-1).ToString("ddMMMyyyy"), DateTime.Now.Date.AddDays(-1).ToString("ddMMMyyyy"), null, strCompId, 2);
                        strLog.Append(Environment.NewLine + "Insert Records In FATO TABLE : " + System.DateTime.Now + Environment.NewLine + Environment.NewLine);

                    }

                    Logger.ExecuteDatatable(ConnectionString, "SP_FATO_Insert_update", DateTime.Now.Date.AddDays(-1).ToString("ddMMMyyyy"), DateTime.Now.Date.AddDays(-1).ToString("ddMMMyyyy"),null, strCompId, 3);
                    strLog.Append(Environment.NewLine + "Update Records In FATO TABLE : " + System.DateTime.Now + Environment.NewLine + Environment.NewLine);


                }
                // }
            }
            catch (Exception ex)
            {
                objlogger.WriteLogs = true;
                strLog.Append(Environment.NewLine + "Exception in FATO Service : " + System.DateTime.Now + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);

          
            }
            finally
            {
                objlogger.Log("FATO Service", strLog.ToString(), "FATOSERVICE");
                Application.Exit();
            }
        }
    }
}
