using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interface
{
    public interface ISFTPHelper: IFTPCommon
    {
        /// <summary>
        /// 連線SFTP
        /// </summary>
        /// <returns></returns>
        bool Connect();

        /// <summary>
        /// 斷開SFTP
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 獲取SFTP檔案列表
        /// 透過字尾抓取檔案List
        /// </summary>
        /// <param name="remotePath">遠端目錄</param>
        /// <param name="fileSuffix">檔案字尾</param>
        /// <returns></returns>
        List<string> GetFileAndFolderList(string remotePath, string fileSuffix);
    }
}
