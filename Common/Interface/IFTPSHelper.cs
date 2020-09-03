using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interface
{
    public interface IFTPSHelper: IFTPCommon
    {
        /// <summary>
        /// 連接FTP
        /// </summary>
        /// <returns></returns>
        bool Connect();

        /// <summary>
        /// 段開連線
        /// </summary>
        void Disconnect();
    }
}
