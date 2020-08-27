using Renci.SshNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppSFTPTest
{
    /// <summary>
    /// SFTP操作類
    /// </summary>
    public class SFTPHelper
    {
        #region 欄位或屬性
        private SftpClient _sftp;
        /// <summary>
        /// SFTP連線狀態
        /// </summary>
        public bool Connected { get { return _sftp.IsConnected; } }
        #endregion

        #region 構造
        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">埠</param>
        /// <param name="user">帳號</param>
        /// <param name="pwd">密碼</param>
        public SFTPHelper(string ip, int port, string user, string pwd)
        {
            _sftp = new SftpClient(ip, port, user, pwd);
        }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="user">帳號</param>
        /// <param name="pwd">密碼</param>
        public SFTPHelper(string ip, string user, string pwd)
        {
            _sftp = new SftpClient(ip, user, pwd);
        }

        #endregion

        #region 連線SFTP
        /// <summary>
        /// 連線SFTP
        /// </summary>
        /// <returns>true成功</returns>
        public bool Connect()
        {
            try
            {
                if (!Connected)
                    _sftp.Connect();

                return true;
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("連線SFTP失敗，原因：{0}", ex.Message));
                throw new Exception(string.Format("連線SFTP失敗，原因：{0}", ex.Message));
            }
        }
        #endregion

        #region 斷開SFTP
        /// <summary>
        /// 斷開SFTP
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (_sftp != null && Connected)
                    _sftp.Disconnect();
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("斷開SFTP失敗，原因：{0}", ex.Message));
                throw new Exception(string.Format("斷開SFTP失敗，原因：{0}", ex.Message));
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
                List<string> result = new List<string>();

                var data =_sftp.ListDirectory(ftpFolderPath);

                foreach (var item in data)
                    result.Add(item.FullName);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SFTP檔案上傳失敗，原因：{0}", ex.Message));
            }
        }

        /// <summary>
        /// 獲取SFTP檔案列表
        /// </summary>
        /// <param name="remotePath">遠端目錄</param>
        /// <param name="fileSuffix">檔案字尾</param>
        /// <returns></returns>
        public List<string> GetFileList(string remotePath, string fileSuffix)
        {
            try
            {
                Connect();
                var files = _sftp.ListDirectory(remotePath);
                Disconnect();
                var objList = new List<string>();
                foreach (var file in files)
                {
                    string name = file.Name;
                    if (name.Length > (fileSuffix.Length + 1) && fileSuffix == name.Substring(name.Length - fileSuffix.Length))
                        objList.Add(name);
                }
                return objList;
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("SFTP檔案列表獲取失敗，原因：{0}", ex.Message));
                throw new Exception(string.Format("SFTP檔案列表獲取失敗，原因：{0}", ex.Message));
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
                    return _sftp.GetLastAccessTime(Path.Combine(ftpFolderPath, fileName));
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
                    return _sftp.Get(Path.Combine(ftpFolderPath, fileName)).Attributes.Size;
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
                    _sftp.UploadFile(fileStream, Path.Combine(ftpFolderPath, fileName));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("連線FTP失敗，原因：{0}", ex.Message));
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
                if (IsFileExists(ftpFolderPath, fileName))
                {
                    string serverPath = Path.Combine(ftpFolderPath, fileName);
                    string localPath = Path.Combine(localFilePath, localFileName);

                    var byt = _sftp.ReadAllBytes(serverPath);
                    File.WriteAllBytes(localPath, byt);

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
                        if (!DownloadFile(ftpFolderPath, fileName, localFilePath, fileName))
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
                _sftp.CreateDirectory(Path.Combine(ftpFolderPath, folderName));
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
                    _sftp.Delete(Path.Combine(ftpFolderPath, fileName));
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
                _sftp.DeleteDirectory(Path.Combine(ftpFolderPath, folderName));
          
                return true;
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
            return _sftp.Exists(string.Format("{0}/{1}", ftpFolderPath, fileName));
        }

        #endregion

        #region 移動SFTP檔案
        /// <summary>
        /// 移動SFTP檔案
        /// </summary>
        /// <param name="oldRemotePath">舊遠端路徑</param>
        /// <param name="newRemotePath">新遠端路徑</param>
        public void Move(string oldRemotePath, string newRemotePath)
        {
            try
            {
                Connect();
                _sftp.RenameFile(oldRemotePath, newRemotePath);
                Disconnect();
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("SFTP檔案移動失敗，原因：{0}", ex.Message));
                throw new Exception(string.Format("SFTP檔案移動失敗，原因：{0}", ex.Message));
            }
        }
        #endregion
    }
}
