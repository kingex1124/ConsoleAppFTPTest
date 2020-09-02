using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppFTPTest
{
    public class FTPHelper
    {
        #region 屬性

        private string _ftpServerIP;

        private string _userName;

        private string _passwoed;

        private FTPMode _ftpMode;

        #endregion

        #region 建構子

        public FTPHelper(FTPParameter param)
        {
            _ftpServerIP = param.FTPServerIP;

            _userName = param.UserName;

            _passwoed = param.Password;

            _ftpMode = param.FTPMode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftpServerIP">FTPService IP</param>
        /// <param name="userName">帳號</param>
        /// <param name="password">密碼</param>
        /// <param name="ftpMode">設定是否走加密的FTP</param>
        public FTPHelper(string ftpServerIP, string userName, string password, FTPMode ftpMode = FTPMode.None)
        {
            _ftpServerIP = ftpServerIP;

            _userName = userName;

            _passwoed = password;

            _ftpMode = ftpMode;

            if (!PingIPByPowerShell(ftpServerIP))
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", "此IP不通"));
        }

        #endregion

        #region 判斷FTP站台是否存在

        /// <summary>
        /// 判斷FTP站台是否存在
        /// </summary>
        /// <param name="ftpServerIP"></param>
        /// <returns></returns>
        private bool PingIPByPowerShell(string ftpServerIP)
        {
            string powStr = string.Empty;

            try
            {
                using (PowerShell powershell = PowerShell.Create())
                {
                    string result = string.Empty;

                    powStr = string.Format("Test-NetConnection -ComputerName {0} -Port 21", ftpServerIP);

                    powershell.AddScript(powStr);

                    var powerResult = powershell.Invoke();

                    foreach (PSObject resultItem in powerResult)
                        result = resultItem.Members["TcpTestSucceeded"].Value.ToString();

                    if (result == "False")
                        return false;
                    else
                        return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 取得表單

        /// <summary>
        /// 取得檔案列表(Service完整路徑，但沒有FTP://跟IP那些)
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <returns></returns>
        public List<string> GetFileAndFolderList(string ftpFolderPath)
        {
            try
            {
                List<string> result = new List<string>();

                string uriPath = string.Format("{0}{1}/{2}", "FTP://", _ftpServerIP, ftpFolderPath);

                //建立FTP連線
                FtpWebRequest ftp = SettingFTP(uriPath);

                //取得檔案清單
                ftp.Method = WebRequestMethods.Ftp.ListDirectory;

                //取得FTP請求回應
                StreamReader streamReader = new StreamReader(ftp.GetResponse().GetResponseStream(), Encoding.UTF8);

                while (!(streamReader.EndOfStream))
                    result.Add(streamReader.ReadLine());

                streamReader.Close();
                streamReader.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        /// <summary>
        /// 取得檔案列表
        /// </summary>
        /// <param name="ftpFolderPath"></param>
        /// <returns></returns>
        public List<string> GetFileList(string ftpFolderPath)
        {
            try
            {
                var fileAndFolder = GetFileAndFolderList(ftpFolderPath);

                List<string> result = new List<string>();
                foreach (var item in fileAndFolder)
                    if (Path.HasExtension(item))
                        result.Add(item);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        /// <summary>
        /// 取得資料夾列表
        /// </summary>
        /// <param name="ftpFolderPath"></param>
        /// <returns></returns>
        public List<string> GetFolderList(string ftpFolderPath)
        {
            try
            {
                var fileAndFolder = GetFileAndFolderList(ftpFolderPath);

                List<string> result = new List<string>();
                foreach (var item in fileAndFolder)
                    if (!Path.HasExtension(item))
                        result.Add(item);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 取得修改日期

        /// <summary>
        /// 取得檔案最後修改日期
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="fileName">檔案完整名稱(含副檔名)</param>
        /// <returns></returns>
        public DateTime GetFileModifiedDate(string ftpFolderPath,string fileName)
        {
            try
            {
                if (IsFileExists(ftpFolderPath, fileName))
                {
                    string uriPath = string.Format("{0}{1}/{2}/{3}", "FTP://", _ftpServerIP, ftpFolderPath, fileName);

                    FtpWebRequest ftp = SettingFTP(uriPath);

                    //取得資料修改日期
                    ftp.Method = WebRequestMethods.Ftp.GetDateTimestamp;

                    //取得FTP請求回應
                    FtpWebResponse ftpWebResponse = (FtpWebResponse)ftp.GetResponse();

                    return ftpWebResponse.LastModified;
                }
                else
                    throw new Exception(string.Format("連線FTP失敗，原因：{0}", "FTP上無此檔案"));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 取得檔案大小

        /// <summary>
        /// 取得檔案大小
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="fileName">檔案完整名稱(含副檔名)</param>
        /// <returns></returns>
        public long GetFileSize(string ftpFolderPath, string fileName)
        {
            try
            {
                if (IsFileExists(ftpFolderPath, fileName))
                {
                    string uriPath = string.Format("{0}{1}/{2}/{3}", "FTP://", _ftpServerIP, ftpFolderPath, fileName);

                    FtpWebRequest ftp = SettingFTP(uriPath);

                    // 設定連線模式及相關參數
                    // 取得資料容量大小
                    ftp.Method = WebRequestMethods.Ftp.GetFileSize;

                    // 取得FTP請求回應
                    FtpWebResponse ftpWebResponse = (FtpWebResponse)ftp.GetResponse();
                    return ftpWebResponse.ContentLength;
                }
                else
                    throw new Exception(string.Format("連線FTP失敗，原因：{0}", "FTP上無此檔案"));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 上傳檔案

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="fileName">檔案名稱(可更改)</param>
        /// <param name="localFilePath">地端資料夾路徑</param>
        /// <param name="localFileName">地端檔案名稱</param>
        /// <returns></returns>
        public bool UploadFile(string ftpFolderPath, string fileName, string localFilePath, string localFileName)
        {
            try
            {
                string uriPath = string.Format("{0}{1}/{2}/{3}", "FTP://", _ftpServerIP, ftpFolderPath, fileName);

                string localPath = Path.Combine(localFilePath, localFileName);

                if(File.Exists(localPath))
                {
                    FtpWebRequest ftp = SettingFTP(uriPath);

                    // 設定連線模式及相關參數

                    // FTPS用設定
                    if (_ftpMode == FTPMode.Explicit)
                    {
                        ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertificatePolicy;
                        ftp.EnableSsl = true;
                    }

                    // 關閉/保持 連線
                    ftp.KeepAlive = false;
                    // 通訊埠接聽並等待連接
                    ftp.UsePassive = false;
                    //下傳檔案
                    ftp.Method = WebRequestMethods.Ftp.UploadFile;
                    /* proxy setting (不使用proxy) */
                    ftp.Proxy = GlobalProxySelection.GetEmptyWebProxy();
                    ftp.Proxy = null;

                    // 上傳檔案 檔案設為讀取模式
                    FileStream fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                    // 資料串流設為上傳至FTP
                    Stream stream = ftp.GetRequestStream();

                    //傳輸位元初始化
                    byte[] byteBuffer = new byte[2047];
                    int iRead = 0;

                    do
                    {
                        // 讀取上傳檔案
                        iRead = fileStream.Read(byteBuffer, 0, byteBuffer.Length);
                        // 傳送資料串流
                        stream.Write(byteBuffer, 0, iRead);
                    } while (!(iRead == 0));

                    fileStream.Flush();
                    fileStream.Close();
                    fileStream.Dispose();
                    stream.Flush();
                    stream.Close();
                    stream.Dispose();

                    return true;
                }
                else
                    throw new Exception(string.Format("連線FTP失敗，原因：{0}", "地端無此檔案"));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        /// <summary>
        /// 憑證認證
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool AcceptAllCertificatePolicy(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        #endregion

        #region 上傳整個資料夾

        /// <summary>
        /// 上傳整個資料夾
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="localFilePath">地端資料夾路徑</param>
        /// <returns></returns>
        public bool UploadFolder(string ftpFolderPath, string localFilePath)
        {
            try
            {
                var dataList = Directory.EnumerateFiles(localFilePath);
                foreach (var item in dataList)
                {

                    string fileName = Path.GetFileName(item);
                    if (!UploadFile(ftpFolderPath, fileName, localFilePath, fileName))
                        return false;
                    else
                    {
                        if (!CheckDownloadData(ftpFolderPath, fileName, localFilePath, fileName))
                            return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 下載檔案

        /// <summary>
        /// 下載檔案
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="fileName">FTP上檔案名稱</param>
        /// <param name="localFilePath">地端資料夾路徑</param>
        /// <param name="localFileName">地端檔案名稱(可更改)</param>
        /// <returns></returns>
        public bool DownloadFile(string ftpFolderPath, string fileName, string localFilePath, string localFileName)
        {
            try
            {
                string uriPath = string.Format("{0}{1}/{2}/{3}", "FTP://", _ftpServerIP, ftpFolderPath, fileName);

                string localPath = Path.Combine(localFilePath, localFileName);

                if (IsFileExists(ftpFolderPath, fileName))
                {
                    FtpWebRequest ftp = SettingFTP(uriPath);

                    // 設定連線模式及相關參數

                    // FTPS用設定
                    if (_ftpMode == FTPMode.Explicit)
                    {
                        ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertificatePolicy;
                        ftp.EnableSsl = true;
                    }

                    // 通訊埠接聽並等待連接
                    ftp.UsePassive = false;
                    // 下傳檔案
                    ftp.Method = WebRequestMethods.Ftp.DownloadFile;

                    // 取得FTP請求回應
                    FtpWebResponse ftpWebResponse = (FtpWebResponse)ftp.GetResponse();

                    // 檔案設為寫入模式
                    FileStream fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);
                    // 資料串流設為上傳至FTP
                    Stream stream = ftpWebResponse.GetResponseStream();

                    //傳輸位元初始化
                    byte[] byteBuffer = new byte[2047];
                    int iRead = 0;

                    do
                    {
                        iRead = stream.Read(byteBuffer, 0, byteBuffer.Length); //接收資料串流
                        fileStream.Write(byteBuffer, 0, iRead); //寫入下載檔案
                                                                //Console.WriteLine("bBuffer: {0} Byte", iRead);
                    } while (!(iRead == 0));

                    stream.Flush();
                    stream.Close();
                    stream.Dispose();
                    fileStream.Flush();
                    fileStream.Close();
                    fileStream.Dispose();
                    ftpWebResponse.Close();

                    return true;
                }
                else
                    throw new Exception(string.Format("連線FTP失敗，原因：{0}", "FTP上無此檔案"));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 下載整個資料夾

        /// <summary>
        /// 下載整個folder的檔案(不包含資料夾)
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="localFilePath">地端資料夾路徑</param>
        /// <returns></returns>
        public bool DownloadFolder(string ftpFolderPath, string localFilePath)
        {
            try
            {
                var dataList = GetFileList(ftpFolderPath);

                foreach (var item in dataList)
                {
                    string fileName = Path.GetFileName(item);
                    if (!DownloadFile(ftpFolderPath, fileName, localFilePath, fileName))
                        return false;
                    else
                    {
                        if (!CheckDownloadData(ftpFolderPath, fileName, localFilePath, fileName))
                            return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 比較伺服端跟地端檔案大小是否一致

        /// <summary>
        /// 比對Service跟地端資料大小是否一致
        /// </summary>
        /// <param name="ftpFolderPath"></param>
        /// <param name="fileName"></param>
        /// <param name="localFilePath"></param>
        /// <param name="localFileName"></param>
        /// <returns></returns>
        public bool CheckDownloadData(string ftpFolderPath, string fileName, string localFilePath, string localFileName)
        {
            try
            {
                long ftpSize = GetFileSize(ftpFolderPath, fileName);

                string localPath = Path.Combine(localFilePath, localFileName);

                if (File.Exists(localPath))
                {
                    long localSize = new FileInfo(localPath).Length;

                    if (ftpSize == localSize)
                        return true;
                    else
                        return false;
                }
                else
                    throw new Exception(string.Format("連線FTP失敗，原因：{0}", "地端無此檔案"));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 在FTP上建立資料夾

        /// <summary>
        /// 在FTP上建立資料夾
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="folderName">資料夾名稱</param>
        /// <returns></returns>
        public bool CreateFolder(string ftpFolderPath,string folderName)
        {
            try
            {
                string uriPath = string.Format("{0}{1}/{2}/{3}", "FTP://", _ftpServerIP, ftpFolderPath, folderName);
                
                FtpWebRequest ftp = SettingFTP(uriPath);
                // 關閉/保持 連線
                ftp.KeepAlive = false;
                // 建立目錄模式
                ftp.Method = WebRequestMethods.Ftp.MakeDirectory;
               
                // 創建目錄
                FtpWebResponse ftpWebResponse = (FtpWebResponse)ftp.GetResponse();

                ftpWebResponse.Close();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 刪除檔案

        /// <summary>
        /// 刪除檔案(資料夾不行)
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="fileName">檔案名稱</param>
        /// <returns></returns>
        public bool DeleteFile(string ftpFolderPath, string fileName)
        {
            try
            {
                string uriPath = string.Format("{0}{1}/{2}/{3}", "FTP://", _ftpServerIP, ftpFolderPath, fileName);

                if (IsFileExists(ftpFolderPath, fileName))
                {
                    FtpWebRequest ftp = SettingFTP(uriPath);

                    // 設定連線模式及相關參數
                    // 關閉/保持 連線
                    ftp.KeepAlive = false;
                    // 刪除檔案
                    ftp.Method = WebRequestMethods.Ftp.DeleteFile;

                    // 刪除檔案
                    FtpWebResponse ftpWebResponse = (FtpWebResponse)ftp.GetResponse();

                    ftpWebResponse.Close();

                    return true;
                }
                else
                    throw new Exception(string.Format("連線FTP失敗，原因：{0}", "FTP上無此檔案"));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 刪除資料夾

        /// <summary>
        /// 刪除資料夾(不可以刪除檔案)
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="folderName">資料夾名稱</param>
        /// <returns></returns>
        public bool RemoveFolder(string ftpFolderPath, string folderName)
        {
            try
            {
                string uriPath = string.Format("{0}{1}/{2}/{3}", "FTP://", _ftpServerIP, ftpFolderPath, folderName);

                if(IsFolderExists(ftpFolderPath, folderName))
                {
                    FtpWebRequest ftp = SettingFTP(uriPath);
                    // 關閉/保持 連線
                    ftp.KeepAlive = false;
                    // 移除資料夾
                    ftp.Method = WebRequestMethods.Ftp.RemoveDirectory;

                    // 刪除資料夾
                    FtpWebResponse ftpWebResponse = (FtpWebResponse)ftp.GetResponse();

                    ftpWebResponse.Close();

                    return true;
                }
                else
                    throw new Exception(string.Format("連線FTP失敗，原因：{0}", "FTP上無此資料夾"));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 判斷檔案是否存在FTP上

        /// <summary>
        /// 判斷檔案是否存在FTP上
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="fileName">檔案名稱</param>
        /// <returns></returns>
        public bool IsFileExists(string ftpFolderPath, string fileName)
        {
            string ftpPath = string.Format("{0}/{1}", ftpFolderPath, fileName);

            List<string> ftpFileList = GetFileList(ftpFolderPath);

            foreach (var item in ftpFileList)
                if (Path.GetFileName(item) == fileName)
                    return true;

            return false;
        }

        #endregion

        #region 判斷資料夾是否存在FTP上

        /// <summary>
        /// 判斷資料夾是否存在FTP上
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="folderName">資料夾名稱</param>
        /// <returns></returns>
        public bool IsFolderExists(string ftpFolderPath, string folderName)
        {
            string ftpPath = string.Format("{0}/{1}", ftpFolderPath, folderName);

            List<string> ftpFolderList = GetFolderList(ftpFolderPath);

            foreach (var item in ftpFolderList)
                if (Path.GetFileNameWithoutExtension(item) == folderName)
                    return true;

            return false;
        }

        #endregion

        #region 設定FTP參數

        /// <summary>
        /// 設定連線模式及相關參數
        /// </summary>
        /// <param name="uriPath"></param>
        /// <returns></returns>
        private FtpWebRequest SettingFTP(string uriPath)
        {
            FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(uriPath);

            //帳密驗證
            ftp.Credentials = new NetworkCredential(_userName, _passwoed);
            //等待時間
            ftp.Timeout = 2000;
            //傳輸資料型別 二進位/文字
            ftp.UseBinary = true;

            return ftp;
        }

        #endregion
    }
}
