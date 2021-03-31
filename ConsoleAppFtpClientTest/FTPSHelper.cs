using Common.Interface;
using Common.Model;
using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppFtpClientTest
{
    public class FTPSHelper: IFTPSHelper
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
        public FTPSHelper(string ip, string user, string pwd)
        {
            if (!PingIPByTcpClient(ip))
                if (!PingIPByPowerShell(ip))
                    throw new Exception(string.Format("連線FTP失敗，原因：{0}", "此IP不通"));

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
        public FTPSHelper(string ip, int port, string user, string pwd)
        {
            if (!PingIPByTcpClient(ip, port))
                if (!PingIPByPowerShell(ip, port))
                    throw new Exception(string.Format("連線FTP失敗，原因：{0}", "此IP不通"));

            _ftp = new FtpClient();
            _ftp.Host = ip;
            _ftp.Port = port; 
            _ftp.EncryptionMode = FtpEncryptionMode.Implicit;
            _ftp.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            _ftp.ValidateCertificate += Ftp_ValidateCertificate;
            _ftp.Credentials = new NetworkCredential(user, pwd);
        }

        #region 判斷FTP站台是否存在

        /// <summary>
        /// 透過TcpClient判斷FTP站台是否存在
        /// </summary>
        /// <param name="ftpServerIP"></param>
        /// <returns></returns>
        private bool PingIPByTcpClient(string ftpServerIP, int port = -1)
        {
            using (TcpClient tcpClient = new TcpClient(ftpServerIP, port == -1 ? 21 : port))
            {
                if (tcpClient.Connected)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 判斷FTP站台是否存在
        /// </summary>
        /// <param name="ftpServerIP"></param>
        /// <returns></returns>
        private bool PingIPByPowerShell(string ftpServerIP, int port = -1)
        {
            string powStr = string.Empty;

            try
            {
                using (PowerShell powershell = PowerShell.Create())
                {
                    string result = string.Empty;

                    // 沒port 走一般FTP 21 port
                    if (port == -1)
                        powStr = string.Format("Test-NetConnection -ComputerName {0} -Port 21", ftpServerIP);
                    else
                        powStr = string.Format("Test-NetConnection -ComputerName {0} -Port {1}", ftpServerIP, port);

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
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.ToString()));
            }
        }

        #endregion

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
                e.Accept = true;
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
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.ToString()));
            }
        }

        #endregion

        #region 斷開  FTP or FTPS

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
                throw new Exception(string.Format("斷線FTP失敗，原因：{0}", ex.ToString()));
            }
        }

        #endregion

        #region 取得表單

        /// <summary>
        /// 取得檔案資料夾完整資訊
        /// </summary>
        /// <param name="ftpFolderPath"></param>
        /// <returns></returns>
        public List<FTPFileInfo> GetFileAndFolderListInfo(string ftpFolderPath)
        {
            try
            {
                var dataArr = _ftp.GetListing(ftpFolderPath);

                List<FTPFileInfo> result = new List<FTPFileInfo>();

                foreach (var item in dataArr)
                {
                    result.Add(new FTPFileInfo()
                    {
                        FullFileName = item.FullName,
                        FileName = Path.GetFileName(item.FullName),
                        ModifiedDate = _ftp.GetModifiedTime(Path.Combine(ftpFolderPath, Path.GetFileName(item.FullName))).ToLocalTime(),
                        FileSize = _ftp.GetFileSize(item.FullName)
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("取得FTP表單失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 取得檔案完整資訊
        /// </summary>
        /// <param name="ftpFolderPath"></param>
        /// <returns></returns>
        public List<FTPFileInfo> GetFileListInfo(string ftpFolderPath)
        {
            try
            {
                var fullList = GetFileAndFolderListInfo(ftpFolderPath);

                List<FTPFileInfo> result = new List<FTPFileInfo>();
                foreach (var item in fullList)
                    if (Path.HasExtension(item.FullFileName))
                        result.Add(item);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("取得FTP表單失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 取得檔案列表(Service完整路徑)
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <returns></returns>
        public List<string> GetFileAndFolderFullNameList(string ftpFolderPath)
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
                throw new Exception(string.Format("取得FTP表單失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 取得檔案列表(非完整路徑)
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <returns></returns>
        public List<string> GetFileAndFolderList(string ftpFolderPath)
        {
            try
            {
                var dataArr = _ftp.GetListing(ftpFolderPath);

                List<string> result = new List<string>();

                foreach (var item in dataArr)
                    result.Add(Path.GetFileName(item.FullName));

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("取得FTP表單失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 取得檔案列表
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <returns></returns>
        public List<string> GetFileFullNameList(string ftpFolderPath)
        {
            try
            {
                var fileAndFolder = GetFileAndFolderFullNameList(ftpFolderPath);

                List<string> result = new List<string>();
                foreach (var item in fileAndFolder)
                    if (Path.HasExtension(item))
                        result.Add(item);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("取得FTP檔案列表失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 取得檔案列表(非完整路徑)
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
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
                throw new Exception(string.Format("取得FTP檔案列表失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 取得資料夾列表
        /// </summary>
        /// <param name="ftpFolderPath"></param>
        /// <returns></returns>
        public List<string> GetFolderFullNameList(string ftpFolderPath)
        {
            try
            {
                var fileAndFolder = GetFileAndFolderFullNameList(ftpFolderPath);

                List<string> result = new List<string>();
                foreach (var item in fileAndFolder)
                    if (!Path.HasExtension(item))
                        result.Add(item);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("取得FTP資料夾列表失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 取得資料夾列表(非完整路徑)
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
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
                throw new Exception(string.Format("取得FTP資料夾列表失敗，原因：{0}", ex.ToString()));
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
                    return _ftp.GetModifiedTime(Path.Combine(ftpFolderPath, fileName)).ToLocalTime();
                else
                    throw new Exception(string.Format("取得FTP檔案修改日其失敗，原因：{0}", "FTP上無此檔案"));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("取得FTP檔案修改日其失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 取得資料夾最後修改日期
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="folderName">資料夾名稱</param>
        /// <returns></returns>
        public DateTime GetFolderModifiedDate(string ftpFolderPath, string folderName)
        {
            try
            {
                if (IsFolderExists(ftpFolderPath, folderName))
                    return _ftp.GetModifiedTime(Path.Combine(ftpFolderPath, folderName)).ToLocalTime();
                else
                    throw new Exception(string.Format("取得FTP資料夾修改日其失敗，原因：{0}", "FTP上無此檔案"));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("取得FTP資料夾修改日其失敗，原因：{0}", ex.ToString()));
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
                    throw new Exception(string.Format("取得FTP檔案大小失敗，原因：{0}", "FTP上無此檔案"));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("取得FTP檔案大小失敗，原因：{0}", ex.ToString()));
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
        public FTPExecuteResult UploadFile(string ftpFolderPath, string fileName, string localFilePath, string localFileName)
        {
            try
            {
                string ftpPath = Path.Combine(ftpFolderPath, fileName);

                string localPath = Path.Combine(localFilePath, localFileName);

                if (File.Exists(localPath))
                {
                    using (var fileStream = File.OpenRead(localPath))
                    {
                        using (var ftpStream = _ftp.OpenWrite(ftpPath))
                        {
                            var buffer = new byte[8 * 1024];
                            int count;
                            while ((count = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                                ftpStream.Write(buffer, 0, count);
                        }
                    }

                    return FTPExecuteResult.Ok("上傳成功");
                }
                else
                    return FTPExecuteResult.Fail(string.Format("上傳FTP檔案失敗，原因：{0}", "地端無此檔案"));
            }
            catch (Exception ex)
            {
                return FTPExecuteResult.Fail(string.Format("上傳FTP檔案失敗，原因：{0}", ex.ToString()));
            }
        }

        #endregion

        #region 上傳整個資料夾

        /// <summary>
        /// 上傳整個資料夾
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="localFilePath">地端資料夾路徑</param>
        /// <returns></returns>
        public FTPExecuteResult UploadFolder(string ftpFolderPath, string localFilePath)
        {
            try
            {
                var dataList = Directory.EnumerateFiles(localFilePath);
                foreach (var item in dataList)
                {
                    string fileName = Path.GetFileName(item);

                    FTPExecuteResult uploadResult = UploadFile(ftpFolderPath, fileName, localFilePath, fileName);

                    if (!uploadResult.IsSuccessed)
                        return uploadResult;
                    else
                    {
                        FTPExecuteResult checkDataConsistentResult = CheckDataConsistent(ftpFolderPath, fileName, localFilePath, fileName);

                        if (!checkDataConsistentResult.IsSuccessed)
                            return checkDataConsistentResult;
                    }
                }

                return FTPExecuteResult.Ok("上傳資料夾檔案成功。");
            }
            catch (Exception ex)
            {
                return FTPExecuteResult.Fail(string.Format("FTP上傳資料夾檔案失敗，原因：{0}", ex.ToString()));
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
        public FTPExecuteResult DownloadFile(string ftpFolderPath, string fileName, string localFilePath, string localFileName)
        {
            try
            {
                if (IsFileExists(ftpFolderPath, fileName))
                {
                    string ftpPath = Path.Combine(ftpFolderPath, fileName);
                    string localPath = Path.Combine(localFilePath, localFileName);


                    using (Stream ftpStream = _ftp.OpenRead(ftpPath))
                    {
                        if (ftpStream.Length != 0)
                        {
                            using (FileStream fileStream = File.Create(localPath, (int)ftpStream.Length))
                            {
                                var buffer = new byte[200 * 1024];
                                int count;
                                while ((count = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                                    fileStream.Write(buffer, 0, count);
                            }
                        }
                        else
                            using (FileStream fs = File.Create(localPath)) { };
                    }

                    return FTPExecuteResult.Ok("檔案下載成功。");
                }
                else
                    return FTPExecuteResult.Fail(string.Format("下載FTP檔案失敗，原因：{0}", "FTP上無此檔案"));
            }
            catch (Exception ex)
            {
                return FTPExecuteResult.Fail(string.Format("下載FTP檔案失敗，原因：{0}", ex.ToString()));
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
        public FTPExecuteResult DownloadFolder(string ftpFolderPath, string localFilePath)
        {
            try
            {
                var dataList = GetFileFullNameList(ftpFolderPath);

                foreach (var item in dataList)
                {
                    string fileName = Path.GetFileName(item);

                    FTPExecuteResult downloadResult = DownloadFile(ftpFolderPath, fileName, localFilePath, fileName);

                    if (!downloadResult.IsSuccessed)
                        return downloadResult;
                    else
                    {
                        FTPExecuteResult checkDataConsistentResult = CheckDataConsistent(ftpFolderPath, fileName, localFilePath, fileName);

                        if (!checkDataConsistentResult.IsSuccessed)
                            return checkDataConsistentResult;
                    }
                }

                return FTPExecuteResult.Ok("下載資料夾檔案成功。");
            }
            catch (Exception ex)
            {
                return FTPExecuteResult.Fail(string.Format("下載FTP資料夾檔案失敗，原因：{0}", ex.ToString()));
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
        public FTPExecuteResult CheckDataConsistent(string ftpFolderPath, string fileName, string localFilePath, string localFileName)
        {
            try
            {
                long ftpSize = GetFileSize(ftpFolderPath, fileName);

                string localPath = Path.Combine(localFilePath, localFileName);

                if (File.Exists(localPath))
                {
                    long localSize = new FileInfo(localPath).Length;

                    if (ftpSize == localSize)
                        return FTPExecuteResult.Ok("FTP上與地端檔案一致。");
                    else
                        return FTPExecuteResult.Fail("地端檔案與上傳檔案大小不一致。");
                }
                else
                    return FTPExecuteResult.Fail(string.Format("FTP檔案與地端檔案比較失敗，原因：{0}", "地端無此檔案"));
            }
            catch (Exception ex)
            {
                return FTPExecuteResult.Fail(string.Format("FTP檔案與地端檔案比較失敗，原因：{0}", ex.ToString()));
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
        public FTPExecuteResult CreateFolder(string ftpFolderPath, string folderName)
        {
            try
            {
                if (!IsFolderExists(ftpFolderPath, folderName))
                {
                    if (_ftp.CreateDirectory(Path.Combine(ftpFolderPath, folderName)))
                        return FTPExecuteResult.Ok("資料夾建立成功。");
                    else
                        return FTPExecuteResult.Fail("FTP建立資料夾失敗。");
                }
                else
                    return FTPExecuteResult.Fail(string.Format("FTP建立資料夾失敗，原因：{0}", "FTP上資料夾已存在"));
            }
            catch (Exception ex)
            {
                return FTPExecuteResult.Fail(string.Format("FTP建立資料夾失敗，原因：{0}", ex.ToString()));
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
        public FTPExecuteResult DeleteFile(string ftpFolderPath, string fileName)
        {
            try
            {
                if (IsFileExists(ftpFolderPath, fileName))
                {
                    _ftp.DeleteFile(Path.Combine(ftpFolderPath, fileName));

                    return FTPExecuteResult.Ok("刪除檔案成功。");
                }
                else
                    return FTPExecuteResult.Fail(string.Format("FTP刪除檔案失敗，原因：{0}", "FTP上無此檔案"));
            }
            catch (Exception ex)
            {
                return FTPExecuteResult.Fail(string.Format("FTP刪除檔案失敗，原因：{0}", ex.ToString()));
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
        public FTPExecuteResult RemoveFolder(string ftpFolderPath, string folderName)
        {
            try
            {
                if (IsFolderExists(ftpFolderPath, folderName))
                {
                    _ftp.DeleteDirectory(Path.Combine(ftpFolderPath, folderName));

                    return FTPExecuteResult.Ok("成功刪除資料夾");
                }
                else
                    return FTPExecuteResult.Fail(string.Format("FTP刪除資料夾失敗，原因：{0}", "FTP上無此資料夾"));
            }
            catch (Exception ex)
            {
                return FTPExecuteResult.Fail(string.Format("FTP刪除資料夾失敗，原因：{0}", ex.ToString()));
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
            return _ftp.DirectoryExists(string.Format("/{0}/{1}", ftpFolderPath, folderName));
        }

        #endregion

        #region 檔案、資料夾改名稱、搬移

        /// <summary>
        /// 修改FTP上檔案名稱
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="oldFileName">原本檔案名稱(含附檔名)</param>
        /// <param name="newFileName">新的檔案名稱(含附檔名)</param>
        /// <returns></returns>
        public FTPExecuteResult ReNameFile(string ftpFolderPath, string oldFileName, string newFileName)
        {
            return MoveFile(ftpFolderPath, oldFileName, ftpFolderPath, newFileName);
        }

        /// <summary>
        /// FTP上移動檔案
        /// </summary>
        /// <param name="oldFtpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="fileName">檔案名稱(含附檔名)</param>
        /// <param name="newFtpFolderPath">新的資料夾路徑，根目錄請代空字串</param>
        /// <returns></returns>
        public FTPExecuteResult MoveFile(string oldFtpFolderPath, string fileName, string newFtpFolderPath)
        {
            return MoveFile(oldFtpFolderPath, fileName, newFtpFolderPath, fileName);
        }

        /// <summary>
        /// FTP上移動檔案，並修改檔案名稱
        /// </summary>
        /// <param name="oldFtpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="oldFileName">原本檔案名稱(含附檔名)</param>
        /// <param name="newFtpFolderPath">新的檔案名稱(含附檔名)</param>
        /// <param name="newFileName">新的資料夾路徑，根目錄請代空字串</param>
        /// <returns></returns>
        public FTPExecuteResult MoveFile(string oldFtpFolderPath, string oldFileName, string newFtpFolderPath, string newFileName)
        {
            try
            {
                if (IsFileExists(oldFtpFolderPath, oldFileName))
                {
                    if (_ftp.MoveFile(Path.Combine(oldFtpFolderPath, oldFileName), Path.Combine(newFtpFolderPath, newFileName)))
                        return FTPExecuteResult.Ok("異動成功。");
                    else
                        return FTPExecuteResult.Fail("異動FTP檔案失敗。");
                }
                else
                    return FTPExecuteResult.Fail(string.Format("異動FTP檔案失敗，原因：{0}", "FTP上無此檔案"));
            }
            catch (Exception ex)
            {
                return FTPExecuteResult.Fail(string.Format("異動FTP檔案失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 修改FTP上資料夾名稱
        /// </summary>
        /// <param name="ftpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="oldFolderName">原本資料夾名稱</param>
        /// <param name="newFolderName">新的資料夾名稱</param>
        /// <returns></returns>
        public FTPExecuteResult ReNameFolder(string ftpFolderPath, string oldFolderName, string newFolderName)
        {
            return MoveFolder(ftpFolderPath, oldFolderName, ftpFolderPath, newFolderName);
        }

        /// <summary>
        /// FTP上移動資料夾
        /// </summary>
        /// <param name="oldFtpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="folderName">資料夾名稱</param>
        /// <param name="newFtpFolderPath">新的資料夾路徑，根目錄請代空字串</param>
        /// <returns></returns>
        public FTPExecuteResult MoveFolder(string oldFtpFolderPath, string folderName, string newFtpFolderPath)
        {
            return MoveFolder(oldFtpFolderPath, folderName, newFtpFolderPath, folderName);
        }

        /// <summary>
        /// FTP上移動資料夾，並修改資料夾名稱
        /// </summary>
        /// <param name="oldFtpFolderPath">資料夾路徑，根目錄請代空字串</param>
        /// <param name="oldFolderName">原本資料夾名稱</param>
        /// <param name="newFtpFolderPath">新的資料夾名稱</param>
        /// <param name="newFolderName">新的資料夾路徑，根目錄請代空字串</param>
        /// <returns></returns>
        public FTPExecuteResult MoveFolder(string oldFtpFolderPath, string oldFolderName, string newFtpFolderPath, string newFolderName)
        {
            try
            {
                if (IsFolderExists(oldFtpFolderPath, oldFolderName))
                {
                    if (_ftp.MoveDirectory(Path.Combine(oldFtpFolderPath, oldFolderName), Path.Combine(newFtpFolderPath, newFolderName)))
                        return FTPExecuteResult.Ok("異動成功。");
                    else
                        return FTPExecuteResult.Fail("異動FTP資料夾失敗。");
                }
                else
                    return FTPExecuteResult.Fail(string.Format("異動FTP資料夾失敗，原因：{0}", "FTP上無此檔案"));
            }
            catch (Exception ex)
            {
                return FTPExecuteResult.Fail(string.Format("異動FTP資料夾失敗，原因：{0}", ex.ToString()));
            }
        }

        #endregion
    }
}
