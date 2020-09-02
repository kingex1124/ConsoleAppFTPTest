using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppFTPTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //using (PowerShell powershell = PowerShell.Create())
            //{
            //    powershell.AddScript("Test-NetConnection -ComputerName 128.110.5.135 -Port 21");

            //    var powerResult = powershell.Invoke();

            //    foreach (PSObject result in powerResult)
            //    {
            //        Console.WriteLine(result.Members["TcpTestSucceeded"].Value);
            //    }
            //}

            //FTPHelp ftp3 = new FTPHelp("128.110.138.11", "test", "011684");

            //var re = ftp3.UploadFile("", "123.txt", @"C:\Users\011714\Desktop\down", "TEST1.txt");

            string path = Path.Combine("FTP://", "123");

            // 測試取得資料大小
            //var data = new FileInfo(@"C:\Users\011714\Desktop\down\TEST2.txt").Length;

            FTPParameter param = new FTPParameter("128.110.5.134", "006788", "ftp006788");

            FTPHelp ftp = new FTPHelp(param);

            var reFileDataList = ftp.GetFileList("");

            var reFolderList = ftp.GetFolderList("");
            // ftp.RemoveFolder("", "測試");

            //FTPParameter param2 = new FTPParameter("128.110.5.135", "006788", "ftp006788");

            //FTPHelp ftp2 = new FTPHelp(param2);

            //var reUpFolder = ftp.UploadFolder("測試", @"C:\Users\011714\Desktop\down");

            var reFileList = ftp.GetFileAndFolderList("");

            //bool reDowFolder = ftp.DownloadFolder("TEST", @"C:\Users\011714\Desktop\down");

            //DateTime reDateTime = ftp.GetFileModifiedDate("", "chase_upload.txt");

            //bool reDowFile = ftp.DownloadFile("", "TEST", @"C:\Users\011714\Desktop\down", @"C:\Users\011714\Desktop\down");

            //var reUpFile = ftp.UploadFile("", "TEST.txt", @"C:\Users\011714\Desktop\down", "TEST.txt");

            //var reFileList2 = ftp.GetFileList("");

            var reUpFolder = ftp.UploadFolder("測試", @"C:\Users\011714\Desktop\down");

            FTPHelp ftp2 = new FTPHelp("128.110.5.135", "006788", "ftp006788");

            var reUpFolder2 = ftp2.UploadFolder("測試", @"C:\Users\011714\Desktop\down");

        }

    }
}
