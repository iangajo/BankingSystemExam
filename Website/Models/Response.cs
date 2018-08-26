using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class Response<T>
    {
        public string ErrorMessage { get; set; }

        public T Data { get; set; }
    }
}
