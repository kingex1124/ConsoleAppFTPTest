using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.FtpClient;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppFtpClientTest
{
    public class FTPHelp
    {
        #region 屬性

        private FtpClient _ftp;

        public bool Connected { get { return _ftp.IsConnected; } }

        #endregion

        #region 建構子

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="user">帳號</param>
        /// <param name="pwd">密碼</param>
        public FTPHelp(string ip, string user, string pwd)
        {
            _ftp = new FtpClient();
            _ftp.Host = ip;
            _ftp.Credentials = new NetworkCredential(user, pwd);
        }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">Port</param>
        /// <param name="user">帳號</param>
        /// <param name="pwd">密碼</param>
        public FTPHelp(string ip, int port, string user, string pwd)
        {
            _ftp = new FtpClient();
            _ftp.Host = ip;
            _ftp.Port = port;
            _ftp.EncryptionMode = FtpEncryptionMode.Implicit;
            _ftp.ValidateCertificate += Ftp_ValidateCertificate;
            _ftp.Credentials = new NetworkCredential(user, pwd);
        }

        /// <summary>
        /// 驗證
        /// </summary>
        /// <param name="control"></param>
        /// <param name="e"></param>
        private void Ftp_ValidateCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            if (e.PolicyErrors != System.Net.Security.SslPolicyErrors.None)
            {
                // invalid cert, do you want to accept it?
                e.Accept = true;
            }
            else
            {
                e.Accept = true;
            }
        }

        #endregion

        #region 連線 FTP or FTPS

        /// <summary>
        /// 連接FTP
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            try
            {
                _ftp.Connect();
                return _ftp.IsConnected;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 段開  FTP or FTPS

        /// <summary>
        /// 段開連線
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (_ftp != null && Connected)
                    _ftp.Disconnect();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 取得表單

        /// <summary>
        /// 取得檔案列表(Service完整路徑)
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <returns></returns>
        public List<string> GetFileList(string ftpFolderPath)
        {
            try
            {
                var dataArr = _ftp.GetListing(ftpFolderPath);

                List<string> result = new List<string>();

                foreach (var item in dataArr)
                    result.Add(item.FullName);

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
        public DateTime GetFileModifiedDate(string ftpFolderPath, string fileName)
        {
            try
            {
                if (IsFileExists(ftpFolderPath, fileName))
                    return _ftp.GetModifiedTime(Path.Combine(ftpFolderPath, fileName));
                else
                    return DateTime.MinValue;
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
                    return _ftp.GetFileSize(Path.Combine(ftpFolderPath, fileName));
                else
                    return -1;
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
                using (var fileStream = File.OpenRead(Path.Combine(localFilePath, localFileName)))
                {
                    using (var ftpStream = _ftp.OpenWrite(Path.Combine(ftpFolderPath, fileName)))
                    {
                        var buffer = new byte[8 * 1024];
                        int count;
                        while ((count = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            ftpStream.Write(buffer, 0, count);
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
                if (IsFileExists(ftpFolderPath, fileName))
                {
                    string serverPath = Path.Combine(ftpFolderPath, fileName);
                    string localPath = Path.Combine(localFilePath, localFileName);

                    using (Stream ftpStream = _ftp.OpenRead(serverPath))
                    {
                        using (FileStream fileStream = File.Create(localPath, (int)ftpStream.Length))
                        {
                            var buffer = new byte[200 * 1024];
                            int count;
                            while ((count = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                                fileStream.Write(buffer, 0, count);
                        }
                    }
                    return true;
                }
                else
                    return false;
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
        /// <param name="ftpFolderPath"></param>
        /// <param name="localFilePath"></param>
        /// <returns></returns>
        public bool DownloadFolder(string ftpFolderPath, string localFilePath)
        {
            try
            {
                var dataList = GetFileList(ftpFolderPath);

                foreach (var item in dataList)
                {
                    if (Path.HasExtension(item))
                    {
                        string fileName = Path.GetFileName(item);
                        if (!DownloadFile(ftpFolderPath, Path.GetFileName(item), localFilePath, fileName))
                            return false;
                        else
                        {
                            if (!CheckDownloadData(ftpFolderPath, fileName, localFilePath, fileName))
                                return false;
                        }
                    };
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
            }
        }

        #endregion

        #region 比較Service跟地端資料大小是否一致

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

                long localSize = new FileInfo(localPath).Length;

                if (ftpSize == localSize)
                    return true;
                else
                    return false;
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
        public bool CreateFolder(string ftpFolderPath, string folderName)
        {
            try
            {
                if (!IsFolderExists(ftpFolderPath, folderName))
                {
                    _ftp.CreateDirectory(Path.Combine(ftpFolderPath, folderName));
                    return true;
                }
                else
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
                if (IsFileExists(ftpFolderPath, fileName))
                {
                    _ftp.DeleteFile(Path.Combine(ftpFolderPath, fileName));
                    return true;
                }
                else
                    return false;
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
                if (IsFolderExists(ftpFolderPath, folderName))
                {
                    _ftp.DeleteDirectory(Path.Combine(ftpFolderPath, folderName));
                    
                    return true;
                }
                else
                    return false;
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
            return _ftp.FileExists(string.Format("/{0}/{1}", ftpFolderPath, fileName));
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
            return _ftp.DirectoryExists(string.Format("/{0}/{1}",ftpFolderPath, folderName));
        }

        #endregion

    }
}
