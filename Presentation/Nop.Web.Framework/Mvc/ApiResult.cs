using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Framework.Mvc
{
    public class ApiResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public int StateCode { get; set; }
        /// <summary>
        /// 附带消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 返回对象
        /// </summary>
        public object Result { get; set; }
        
    }
}
