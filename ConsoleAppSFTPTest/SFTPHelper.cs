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
        private SftpClient sftp;
        /// <summary>
        /// SFTP連線狀態
        /// </summary>
        public bool Connected { get { return sftp.IsConnected; } }
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
            sftp = new SftpClient(ip, port, user, pwd);
        }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="user">帳號</param>
        /// <param name="pwd">密碼</param>
        public SFTPHelper(string ip, string user, string pwd)
        {
            sftp = new SftpClient(ip, user, pwd);
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
                    sftp.Connect();

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
                if (sftp != null && Connected)
                    sftp.Disconnect();
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("斷開SFTP失敗，原因：{0}", ex.Message));
                throw new Exception(string.Format("斷開SFTP失敗，原因：{0}", ex.Message));
            }
        }
        #endregion

        #region SFTP上傳檔案
        /// <summary>
        /// SFTP上傳檔案
        /// </summary>
        /// <param name="localPath">本地路徑</param>
        /// <param name="remotePath">遠端路徑</param>
        public void Put(string localPath, string remotePath)
        {
            try
            {
                using (var file = File.OpenRead(localPath))
                {
                    Connect();
                    sftp.UploadFile(file, remotePath);
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("SFTP檔案上傳失敗，原因：{0}", ex.Message));
                throw new Exception(string.Format("SFTP檔案上傳失敗，原因：{0}", ex.Message));
            }
        }
        #endregion

        #region SFTP獲取檔案
        /// <summary>
        /// SFTP獲取檔案
        /// </summary>
        /// <param name="remotePath">遠端路徑</param>
        /// <param name="localPath">本地路徑</param>
        public void Get(string remotePath, string localPath)
        {
            try
            {
                Connect();
                var byt = sftp.ReadAllBytes(remotePath);
                Disconnect();
                File.WriteAllBytes(localPath, byt);
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("SFTP檔案獲取失敗，原因：{0}", ex.Message));
                throw new Exception(string.Format("SFTP檔案獲取失敗，原因：{0}", ex.Message));
            }

        }
        #endregion

        #region 刪除SFTP檔案
        /// <summary>
        /// 刪除SFTP檔案
        /// </summary>
        /// <param name="remoteFile">遠端路徑</param>
        public void Delete(string remoteFile)
        {
            try
            {
                Connect();
                sftp.Delete(remoteFile);
                Disconnect();
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("SFTP檔案刪除失敗，原因：{0}", ex.Message));
                throw new Exception(string.Format("SFTP檔案刪除失敗，原因：{0}", ex.Message));
            }
        }
        #endregion

        #region 獲取SFTP檔案列表
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
                var files = sftp.ListDirectory(remotePath);
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

        #region 取得SFTP檔案大小資訊

        /// <summary>
        /// 取得SFTP檔案大小資訊
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public long GetSFTPFileSize(string path)
        {
            Connect();
            var dateSize = sftp.Get(path).Attributes.Size;
            Disconnect();

            return dateSize;
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
                sftp.RenameFile(oldRemotePath, newRemotePath);
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
