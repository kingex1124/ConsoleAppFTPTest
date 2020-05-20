using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleAppFTPTest
{
    public class FTPExtensions
    {
        private static string _sFTPServerIP, _sUserName, _sPassWord, _sDirName, _sFileOrDir, _sFromFileName, _sToFileName, _sFileName;
        private static int _iFTPReTry;

        /// <summary>
        /// IP
        /// </summary>
        public static string sFTPServerIP { get { return _sFTPServerIP; } set { _sFTPServerIP = value; } }
        /// <summary>
        /// 帳號
        /// </summary>
        public static string sUserName { get { return _sUserName; } set { _sUserName = value; } }
        /// <summary>
        /// 密碼
        /// </summary>
        public static string sPassWord { get { return _sPassWord; } set { _sPassWord = value; } }
        /// <summary>
        /// FTP 檔案路徑
        /// </summary>
        public static string sDirName { get { return _sDirName; } set { _sDirName = value; } }
        public static string sFileOrDir { get { return _sFileOrDir; } set { _sFileOrDir = value; } }
        /// <summary>
        /// 檔名
        /// </summary>
        public static string sFromFileName { get { return _sFromFileName; } set { _sFromFileName = value; } }
        /// <summary>
        /// 下載路徑
        /// </summary>
        public static string sToFileName { get { return _sToFileName; } set { _sToFileName = value; } }
        public static string sFileName { get { return _sFileName; } set { _sFileName = value; } }
        public static int iFTPReTry { get { return _iFTPReTry; } set { _iFTPReTry = value; } }


        public static Array FTPQuery()
        {
            try
            {
                sDirName = sDirName.Replace("\\", "/");
                string sURI = "FTP://" + sFTPServerIP + "/" + sDirName;
                System.Net.FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                //設定連線模式及相關參數
                myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                myFTP.Timeout = 2000; //等待時間
                myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
                myFTP.Method = System.Net.WebRequestMethods.Ftp.ListDirectory; //取得檔案清單

                StreamReader myReadStream = new StreamReader(myFTP.GetResponse().GetResponseStream(), Encoding.Default); //取得FTP請求回應

                //檔案清單
                string sFTPFile; StringBuilder sbResult = new StringBuilder(); //,string[] sDownloadFiles;
                while (!(myReadStream.EndOfStream))
                {
                    sFTPFile = myReadStream.ReadLine();
                    sbResult.Append(sFTPFile + "\n");
                    //Console.WriteLine("{0}", FTPFile);
                }
                myReadStream.Close();
                myReadStream.Dispose();
                sFTPFile = null;
                sbResult.Remove(sbResult.ToString().LastIndexOf("\n"), 1); //檔案清單查詢結果
                //Console.WriteLine("Result:" + "\n" + "{0}", sResult);
                return sbResult.ToString().Split('\n'); //回傳至字串陣列
            }
            catch (Exception ex)
            {
                //Console.WriteLine("FTP File Query Fail" + "\n" + "{0}", ex.Message);
                //MessageBox.Show(ex.Message , null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
                iFTPReTry--;
                if (iFTPReTry >= 0)
                {
                    return FTPQuery();
                }
                else
                {
                    return null;
                }
            }
        }

        public static List<string> FTPDetailQuery()
        {
            try
            {
                sDirName = sDirName.Replace("\\", "/");
                string sURI = "FTP://" + sFTPServerIP + "/" + sDirName;
                System.Net.FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                //設定連線模式及相關參數
                myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                myFTP.Timeout = 2000; //等待時間
                myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
                myFTP.Method = System.Net.WebRequestMethods.Ftp.ListDirectoryDetails; //取得詳細檔案清單

                StreamReader myReadStream = new StreamReader(myFTP.GetResponse().GetResponseStream(), Encoding.Default); //取得FTP請求回應
                //目錄清單
                string sFileQuery;
                string[] sFileList;
                StringBuilder sbResult = new StringBuilder();
                List<string> lFileResult = new List<string>();
                while (!(myReadStream.EndOfStream))
                {
                    sFileQuery = myReadStream.ReadLine();
                    sbResult.Append(sFileQuery + "\n");
                    //Console.WriteLine("{0}", sFTPFile);
                }
                myReadStream.Close();
                myReadStream.Dispose();
                sFileQuery = null;
                sbResult.Remove(sbResult.ToString().LastIndexOf("\n"), 1); //檔案清單查詢結果
                //Console.WriteLine("Result:" & vbNewLine & "{0}", sResult);
                sFileList = sbResult.ToString().Split('\n'); //檔案清單轉換為字串陣列
                //判斷是否為資料夾
                if (sFileOrDir.ToLower() == "file")
                {
                    sFileOrDir = "-rw-r--r--";
                    //sFileOrDir = "-r--r--r--";
                }
                else
                {
                    sFileOrDir = "drwxr-xr-x";
                }
                //解析資料夾
                foreach (string myFileInfo in sFileList)
                {
                    if (myFileInfo.IndexOf(sFileOrDir) >= 0)
                    {
                        string[] sInfoStr = myFileInfo.Split(' ');
                        string sDirStr = null;
                        int iFileStr = 0;
                        //解析字元陣列
                        for (int myFileChar = 0; myFileChar < sInfoStr.Length; myFileChar++)
                        {
                            //字元陣列非空項則設為字串
                            if (sInfoStr[myFileChar] != null)
                                iFileStr++;

                            //字串陣列第9個為FTP資料夾名稱
                            if (iFileStr > 8)
                                sDirStr = sInfoStr[myFileChar] + " ";

                        }
                        sDirStr = sDirStr.Trim(); //去除字元空格
                        if (sDirStr != "." && sDirStr != "..")
                        {
                            lFileResult.Add(sDirStr);
                            //Console.WriteLine("sDownloadFiles:{0}", DownloadFiles[DownloadFiles.Count-1] );
                        }
                    }
                }
                return lFileResult;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("FTP Dictionar Query Fail" + "\n" + "{0}", ex.Message);
                //MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
                iFTPReTry--;
                if (iFTPReTry >= 0)
                {
                    return FTPDetailQuery();
                }
                else
                {
                    return null;
                }
            }
        }

        public static DateTime FTPGetFileDate()
        {
            try
            {
                sDirName = sDirName.Replace("\\", "/");
                string sURI = "FTP://" + sFTPServerIP + "/" + sDirName + "/" + sFileName;

                FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                //設定連線模式及相關參數
                myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                myFTP.Timeout = 2000; //等待時間
                myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
                myFTP.Method = WebRequestMethods.Ftp.GetDateTimestamp; //取得資料修改日期

                System.Net.FtpWebResponse myFTPFileDate = (System.Net.FtpWebResponse)myFTP.GetResponse(); //取得FTP請求回應

                return myFTPFileDate.LastModified;
            }
            catch (Exception ex)
            {
                Console.WriteLine("FTP Dictionar Query Fail" + "\n" + "{0}", ex.Message);
                //MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
                iFTPReTry--;
                if (iFTPReTry >= 0)
                {
                    return FTPGetFileDate();
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        public static int FTPGetFileSize()
        {
            try
            {
                sDirName = sDirName.Replace("\\", "/");
                string sURI = "FTP://" + sFTPServerIP + "/" + sDirName + "/" + sFileName;
                FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                //設定連線模式及相關參數
                myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                myFTP.Timeout = 2000; //等待時間
                myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
                myFTP.Method = System.Net.WebRequestMethods.Ftp.GetFileSize; //取得資料容量大小

                System.Net.FtpWebResponse myFTPFileSize = (System.Net.FtpWebResponse)myFTP.GetResponse(); //取得FTP請求回應
                return (int)myFTPFileSize.ContentLength;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("FTP File Size Query Fail" + "\n" + "{0}", ex.Message)
                //MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
                iFTPReTry--;
                if (iFTPReTry >= 0)
                {
                    return FTPGetFileSize();
                }
                else
                {
                    return 0;
                }
            }
        }

        public static Boolean FTPUploadFile()
        {
            try
            {
                sDirName = sDirName.Replace("\\", "/");
                string sURI = "FTP://" + sFTPServerIP + "/" + sDirName + "/" + sToFileName;
                System.Net.FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                //設定連線模式及相關參數
                myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                myFTP.KeepAlive = false; //關閉/保持 連線
                myFTP.Timeout = 2000; //等待時間
                myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
                myFTP.UsePassive = false; //通訊埠接聽並等待連接
                myFTP.Method = System.Net.WebRequestMethods.Ftp.UploadFile; //下傳檔案
                /* proxy setting (不使用proxy) */
                myFTP.Proxy = GlobalProxySelection.GetEmptyWebProxy();
                myFTP.Proxy = null;

                //上傳檔案
                System.IO.FileStream myReadStream = new System.IO.FileStream(sFromFileName, FileMode.Open, FileAccess.Read); //檔案設為讀取模式
                System.IO.Stream myWriteStream = myFTP.GetRequestStream(); //資料串流設為上傳至FTP
                byte[] bBuffer = new byte[2047]; int iRead = 0; //傳輸位元初始化
                do
                {
                    iRead = myReadStream.Read(bBuffer, 0, bBuffer.Length); //讀取上傳檔案
                    myWriteStream.Write(bBuffer, 0, iRead); //傳送資料串流
                    //Console.WriteLine("Buffer: {0} Byte", iRead);
                } while (!(iRead == 0));

                myReadStream.Flush();
                myReadStream.Close();
                myReadStream.Dispose();
                myWriteStream.Flush();
                myWriteStream.Close();
                myWriteStream.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("FTP Upload Fail" + "\n" + "{0}", ex.Message);
                //MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
                iFTPReTry--;
                if (iFTPReTry >= 0)
                {
                    return FTPUploadFile();
                }
                else
                {
                    return false;
                }
            }
        }

        public static Boolean FTPDownloadFile()
        {
            try
            {
                sDirName = sDirName.Replace("\\", "/");
                string sURI = "FTP://" + sFTPServerIP + "/" + sDirName + "/" + sFromFileName;
                System.Net.FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                //設定連線模式及相關參數
                myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                myFTP.Timeout = 2000; //等待時間
                myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
                myFTP.UsePassive = false; //通訊埠接聽並等待連接
                myFTP.Method = System.Net.WebRequestMethods.Ftp.DownloadFile; //下傳檔案

                System.Net.FtpWebResponse myFTPResponse = (System.Net.FtpWebResponse)myFTP.GetResponse(); //取得FTP回應
                //下載檔案
                System.IO.FileStream myWriteStream = new System.IO.FileStream(sToFileName, FileMode.Create, FileAccess.Write); //檔案設為寫入模式
                System.IO.Stream myReadStream = myFTPResponse.GetResponseStream(); //資料串流設為接收FTP回應下載
                byte[] bBuffer = new byte[2047]; int iRead = 0; //傳輸位元初始化
                do
                {
                    iRead = myReadStream.Read(bBuffer, 0, bBuffer.Length); //接收資料串流
                    myWriteStream.Write(bBuffer, 0, iRead); //寫入下載檔案
                    //Console.WriteLine("bBuffer: {0} Byte", iRead);
                } while (!(iRead == 0));

                myReadStream.Flush();
                myReadStream.Close();
                myReadStream.Dispose();
                myWriteStream.Flush();
                myWriteStream.Close();
                myWriteStream.Dispose();
                myFTPResponse.Close();
                return true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("FTP Download Fail" & vbNewLine & "{0}", ex.Message)
                //MessageBox.Show(ex.Message , null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, False)
                iFTPReTry--;
                if (iFTPReTry >= 0)
                {
                    return FTPDownloadFile();
                }
                else
                {
                    return false;
                }
            }
        }

        public static Boolean FTPCreateDir()
        {
            try
            {
                sDirName = sDirName.Replace("\\", "/");
                string sURI = "FTP://" + sFTPServerIP + "/" + sDirName;
                System.Net.FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                //設定連線模式及相關參數
                myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                myFTP.KeepAlive = false; //關閉/保持 連線
                myFTP.Timeout = 2000; //等待時間
                myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
                myFTP.Method = System.Net.WebRequestMethods.Ftp.MakeDirectory; //建立目錄模式

                System.Net.FtpWebResponse myFtpResponse = (System.Net.FtpWebResponse)myFTP.GetResponse(); //創建目錄
                myFtpResponse.Close();
                return true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("FTP Directory Create Fail" + "\n" + "{0}", ex.Message);
                //MessageBox.Show(ex.Message , null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
                iFTPReTry--;
                if (iFTPReTry >= 0)
                {
                    return FTPCreateDir();
                }
                else
                {
                    return false;
                }
            }
        }

        public static Boolean FTPDeleteFile()
        {
            try
            {
                sDirName = sDirName.Replace("\\", "/");
                string sURI = "FTP://" + sFTPServerIP + "/" + sDirName + sFileName;
                System.Net.FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                //設定連線模式及相關參數
                myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                myFTP.KeepAlive = false; //關閉/保持 連線
                myFTP.Timeout = 2000; //等待時間
                myFTP.Method = System.Net.WebRequestMethods.Ftp.DeleteFile; //刪除檔案

                System.Net.FtpWebResponse myFtpResponse = (System.Net.FtpWebResponse)myFTP.GetResponse(); //刪除檔案/資料夾
                myFtpResponse.Close();
                return true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("FTP File Seach Fail" + "\n" + "{0}", ex.Message);
                //MessageBox.Show(ex.Message , null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
                iFTPReTry--;
                if (iFTPReTry >= 0)
                {
                    return FTPDeleteFile();
                }
                else
                {
                    return false;
                }
            }
        }

        public static Boolean FTPRemoveDirectory()
        {
            try
            {
                sDirName = sDirName.Replace("\\", "/");
                string sURI = "FTP://" + sFTPServerIP + "/" + sDirName;
                System.Net.FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                //設定連線模式及相關參數
                myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                myFTP.KeepAlive = false; //關閉/保持 連線
                myFTP.Timeout = 2000; //等待時間
                myFTP.Method = System.Net.WebRequestMethods.Ftp.RemoveDirectory; //移除資料夾

                System.Net.FtpWebResponse myFtpResponse = (System.Net.FtpWebResponse)myFTP.GetResponse(); //刪除檔案/資料夾
                myFtpResponse.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("FTP File Seach Fail" + "\n" + "{0}", ex.Message);
                MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
                iFTPReTry--;
                if (iFTPReTry >= 0)
                {
                    return FTPRemoveDirectory();
                }
                else
                {
                    return false;
                }
            }
        }

        public static void cmdFTPDownloadFile()
        {
            try
            {
                sDirName = sDirName.Replace("\\", "/");
                System.IO.FileStream myFTPCommand = new System.IO.FileStream("D:\\FTPCommand.txt", FileMode.Create, FileAccess.ReadWrite);
                StreamWriter myCommand = new StreamWriter(myFTPCommand);
                myCommand.BaseStream.Seek(0, SeekOrigin.Begin);
                myCommand.WriteLine("open" + " " + sFTPServerIP + " ");
                myCommand.WriteLine(sUserName);
                myCommand.WriteLine(sPassWord);
                myCommand.WriteLine("get" + " " + sDirName + "\"" + sFromFileName + "\"" + " " + "\"" + sToFileName + "\"");
                myCommand.WriteLine("bye");
                myCommand.Flush();
                myCommand.Close();
                myCommand.Dispose();
                Process.Start(System.Environment.GetEnvironmentVariable("SystemRoot") + "\\system32\\ftp.exe", "-s:\"D:\\FTPCommand.txt\"").WaitForExit();
                File.Delete("D:\\FTPCommand.txt");
            }
            catch (Exception ex)
            {
                //Console.WriteLine("FTP File Seach Fail" + "\n" + "{0}", ex.Message);
                //MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
            }
        }

        public static List<string> cmdFTPQuery()
        {
            try
            {
                sDirName = sDirName.Replace("\\", "/");
                System.IO.FileStream myFTPCommand = new System.IO.FileStream("D:\\FTPCommand.txt", FileMode.Create, FileAccess.ReadWrite);
                StreamWriter myCommand = new StreamWriter(myFTPCommand);
                myCommand.BaseStream.Seek(0, SeekOrigin.Begin);
                myCommand.WriteLine("open" + " " + sFTPServerIP + "\t");
                myCommand.WriteLine(sUserName);
                myCommand.WriteLine(sPassWord);
                myCommand.WriteLine("cd" + " " + "\"" + sDirName + "\"");
                myCommand.WriteLine("ls" + " " + "*" + sFileName + "*");
                myCommand.WriteLine("bye");
                myCommand.Flush();
                myCommand.Close();
                myCommand.Dispose();
                //建立cmd執行緒
                Process myProcess = new Process();
                myProcess.StartInfo.FileName = System.Environment.GetEnvironmentVariable("SystemRoot") + "\\system32\\cmd.exe";
                //myProcess.StartInfo.Arguments = "/c " + Command(); //設定程式執行參數
                myProcess.StartInfo.UseShellExecute = false; //關閉Shell的使用
                myProcess.StartInfo.RedirectStandardInput = true; //重新導向標準輸入
                myProcess.StartInfo.RedirectStandardOutput = true; //重新導向標準輸出
                myProcess.StartInfo.RedirectStandardError = true; //重新導向錯誤輸出
                myProcess.StartInfo.CreateNoWindow = true; //設定不顯示視窗
                myProcess.Start();
                myProcess.StandardInput.WriteLine("ftp -s:\"D:\\FTPCommand.txt\"");
                myProcess.StandardInput.WriteLine("exit");
                //解析檔案清單
                string sFileQuery;
                string[] sFileList;
                List<string> lFileResult = new List<string>();
                //sFileQuery = UrlEncode(myProcess.StandardOutput.ReadToEnd()); //從輸出流取得命令執行結果，解決中文亂碼的問題
                //Application.DoEvents();
                sFileQuery = myProcess.StandardOutput.ReadToEnd();
                sFileQuery = sFileQuery.Replace("\n", null);
                sFileList = sFileQuery.Split('\n');
                foreach (string myFile in sFileList)
                {
                    if (myFile.IndexOf(sFileName) >= 0 && myFile.IndexOf(sDirName) == 0)
                    {
                        string myStr = null;
                        if (myFile.IndexOf("版本") >= 0 && myFile.IndexOf(sFTPServerIP) > 0)
                        {
                            char[] myChar = myFile.ToCharArray();
                            Array.Reverse(myChar);
                            myStr = new string(myChar);
                            myStr = myFile.Substring(myFile.Length - myStr.IndexOf("\t"), myStr.IndexOf("\t"));
                        }
                        lFileResult.Add(myStr);
                    }
                }
                File.Delete("D:\\FTPCommand.txt");
                return lFileResult;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("FTP File Seach Fail" & vbNewLine & "{0}", ex.Message)
                //MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, False)
                return null;
            }
        }

        public static String UrlEncode(String Str)
        {
            if (Str == null)
                return null;

            return Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(Str));
            //return Encoding.GetEncoding("utf-8").GetString(Encoding.GetEncoding("utf-8").GetBytes(Str));
        }

    }
}
