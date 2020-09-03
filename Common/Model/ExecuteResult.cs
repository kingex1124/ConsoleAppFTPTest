using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class ExecuteResult
    {
        public bool IsSuccessed { get; set; }

        public string Message { get; set; }

        public ExecuteResult()
        {

        }

        public ExecuteResult(bool isSuccessed, string message)
        {
            IsSuccessed = isSuccessed;
            Message = message;
        }
    }
}
