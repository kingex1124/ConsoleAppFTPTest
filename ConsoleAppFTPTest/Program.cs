using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppFTPTest
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        /* FTP Download  */
        private Boolean FTP_Download_Color_Computer1(string station, string family, string device)
        {
            Boolean Color_Computer1_check = false;
            string Download_path = "jobfile_path";
            try
            {
                FTPExtensions.sFTPServerIP = "172.0.0.1" /* IP */;
                FTPExtensions.sUserName = "jobfile";     /* 帳號 */
                FTPExtensions.sPassWord = "jobfile";     /* 密碼 */
                FTPExtensions.sDirName = @"/FI-7300/" + station + @"/" + family + @"/Color/" + device + @"/"; /* FTP 檔案路徑 */

                if (FTPExtensions.FTPQuery() != null)
                {
                    Array aFTPList = FTPExtensions.FTPQuery();
                    foreach (string myFTPQueryStr in aFTPList)
                    {
                        FTPExtensions.sFromFileName = myFTPQueryStr; /* myFTPQueryStr 檔名 */
                        FTPExtensions.sToFileName = Download_path + myFTPQueryStr; /* 下載路徑 */
                        Color_Computer1_check = FTPExtensions.FTPDownloadFile(); /* Download */
                    }
                    return Color_Computer1_check;
                }
            }
            catch (Exception ex)
            { }
            return false;
        }

        private Boolean FTP_Upload()
        {
            Boolean Upload_check = false;
            /* 獲得日期 */
            string Date = DateTime.Now.ToString("yyyy/MM/dd");
            Date = Date.Replace('/', '-');
            try
            {
                FTPExtensions.sFTPServerIP = "172.0.0.1";
                FTPExtensions.sUserName = "jobfile";
                FTPExtensions.sPassWord = "jobfile";
                FTPExtensions.sDirName = @"/" + Date + "/"; /* FTP路徑 */
                FTPExtensions.sFromFileName = @"D:\5S_Resume\" + Date + @"\" + "5S_" + Date + ".csv"; /* 上傳檔案路徑 */
                FTPExtensions.sToFileName = "5S_" + Date + ".csv"; /* 上傳檔名 */
                Upload_check = FTPExtensions.FTPUploadFile(); /* 上傳 */
                return Upload_check;
            }
            catch (Exception ex)
            { }
            return false;
        }
    }
}
