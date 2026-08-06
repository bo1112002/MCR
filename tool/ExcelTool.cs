using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using Tools.Config;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using Tools;

namespace MCR.tool
{
    public class ExcelTool
    {
        public static DataSet LoadDataFromExcel(string filePath)
        {
            try
            {
                string strConn;
                strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=False;IMEX=1'"; 
                //strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'"; 
                strConn = string.Format(strConn , filePath);
                OleDbConnection OleConn = new OleDbConnection(strConn);
                OleConn.Open();
                String sql = "SELECT * FROM  [Sheet1$]";//可是更改Sheet名称，比如sheet2，等等  

                OleDbDataAdapter OleDaExcel = new OleDbDataAdapter(sql, OleConn);
                DataSet OleDsExcle = new DataSet();
                OleDaExcel.Fill(OleDsExcle, "Sheet1");
                OleConn.Close();
                return OleDsExcle;
            }
            catch (Exception err)
            {
                Loger.Log("ExcelTool->DataSet-> " + err.Message);
                return null;
            }
        }

        /// <summary> 
        /// DataTable直接导出Excel,此方法会把DataTable的数据用Excel打开,再自己手动去保存到确切的位置 
        /// </summary> 
        /// <param name="dt">要导出Excel的DataTable</param> 
        public static bool DoExport(System.Data.DataTable dt , string outExcelFile )
        {
            dt.TableName = "DataTable";
            string outExcelFileXML = outExcelFile + ".XML";
            if (File.Exists(outExcelFileXML))
            {
                File.Delete(outExcelFileXML);
            }
            dt.WriteXml(outExcelFileXML , XmlWriteMode.WriteSchema);

            if (File.Exists(outExcelFileXML) == false) 
                return false;

            byte[] bs  = Encoding.UTF8.GetBytes( "ExcelExport:" +  outExcelFileXML) ;
            //string strPs = Convert.ToBase64String(bs);
            UdpClient udp = new UdpClient();
            int rs = udp.Send(bs, bs.Length, AppSettings.GetServicePoint());

            /*
            ProcessStartInfo startInfo = new ProcessStartInfo(AppSettings.ConvertPDF_EXE, "ExcelExport:" + strPs);
            startInfo.UseShellExecute = true  ;
            startInfo.CreateNoWindow = true;

            Process pp = Process.Start(startInfo);
            bool rs = pp.WaitForExit(1000 * 30);
*/
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(3000);
                File.Exists(outExcelFile);
                {
                    return true;
                }
            }
            return false;

            /*
            try
            {
                Microsoft.Office.Interop.Excel.Application app = new ApplicationClass();
                if (app == null)
                {
                    throw new Exception("Excel无法启动");
                }
                app.Visible = true  ;
                Workbooks wbs = app.Workbooks;
                Workbook wb = wbs.Add(Missing.Value);
                Worksheet ws = (Worksheet)wb.Worksheets[1];

                int cnt = dt.Rows.Count;
                int columncnt = dt.Columns.Count;

                // *****************获取数据******************** 
                object[,] objData = new Object[cnt + 1, columncnt];  // 创建缓存数据 
                // 获取列标题 
                for (int i = 0; i < columncnt; i++)
                {
                    objData[0, i] = dt.Columns[i].ColumnName;
                }
                // 获取具体数据 
                for (int i = 0; i < cnt; i++)
                {
                    System.Data.DataRow dr = dt.Rows[i];
                    for (int j = 0; j < columncnt; j++)
                    {
                        objData[i + 1, j] = dr[j];
                    }
                }

                //********************* 写入Excel****************** 
                Range r = ws.get_Range(app.Cells[1, 1], app.Cells[cnt + 1, columncnt]);
                r.NumberFormat = "@";
                //r = r.get_Resize(cnt+1, columncnt); 
                r.Value2 = objData;
                r.EntireColumn.AutoFit();

                ws.SaveAs(outExcelFile);
                wb.SaveAs( outExcelFile );
                wbs.Close();
                app.Quit();
                app = null;
                return true;
            }
            catch (Exception err)
            {
                MyConfig.Log(err.Message + "-->" +err.StackTrace );
                return false;
            }*/
        }
    }
}
