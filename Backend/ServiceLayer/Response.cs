using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace KanBan_2024.ServiceLayer
{
    public class Response
    {
        public string ErrorMessage { get; set; }
        public Object ReturnValue {  get; set; }

        public Response(Object returnValue, string errorMessage)
        {
            ErrorMessage = errorMessage;
            ReturnValue = returnValue;
        }
    }

}
