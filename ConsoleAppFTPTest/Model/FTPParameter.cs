﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppFTPTest
{
    public class FTPParameter
    {
        public FTPParameter()
        {

        }

        public FTPParameter(string ftpServerIP,string userName,string password)
        {
            FTPServerIP = ftpServerIP;
            UserName = userName;
            Password = password;
        }
        /// <summary>
        /// IP
        /// </summary>
        public string FTPServerIP { get; set; }
        /// <summary>
        /// 帳號
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密碼
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 失敗時重連的次數
        /// </summary>
        public int FTPReTry { get; set; }
    }
}
