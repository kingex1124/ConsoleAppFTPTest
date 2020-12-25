using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class FTPExecuteResult
    {
        public bool IsSuccessed { get; set; }

        public string Message { get; set; }

        public FTPExecuteResult()
        {

        }

        public FTPExecuteResult(bool isSuccessed, string message)
        {
            IsSuccessed = isSuccessed;
            Message = message;
        }

        public static FTPExecuteResult Ok()
        {
            return new FTPExecuteResult { IsSuccessed = true };
        }

        public static FTPExecuteResult Ok(string message)
        {
            return new FTPExecuteResult { IsSuccessed = true, Message = message };
        }

        public static FTPExecuteResult Fail(string errMsg)
        {
            return new FTPExecuteResult { IsSuccessed = false, Message = errMsg };
        }
    }

    public class FTPExecuteResult<T> : FTPExecuteResult
    {
        public T Data { get; set; }

        public FTPExecuteResult()
        {
            Data = default(T);
        }

        public static FTPExecuteResult<T> Ok(T data)
        {
            return new FTPExecuteResult<T> { IsSuccessed = true, Data = data };
        }

        public static FTPExecuteResult<T> Ok(T data, string msg)
        {
            return new FTPExecuteResult<T> { IsSuccessed = true, Message = msg, Data = data };
        }

        public static FTPExecuteResult<T> Fail(string errMsg)
        {
            return new FTPExecuteResult<T> { IsSuccessed = false, Message = errMsg };
        }
    }
}
