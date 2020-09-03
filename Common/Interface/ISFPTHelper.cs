using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interface
{
    public interface ISFPTHelper: IFTPCommon
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
    }
}
