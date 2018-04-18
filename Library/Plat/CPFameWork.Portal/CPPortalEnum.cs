using System;
using System.Collections.Generic;
using System.Text;

namespace CPFameWork.Portal
{
   public  class CPPortalEnum
    {
        public enum ModuleOpenTypeEnum
        {
            /// <summary>
            /// 内置框架中打开
            /// </summary>
            InnerFrame = 1,
            /// <summary>
            /// 弹出新页面打开
            /// </summary>
            _blank = 2
        }
    }
}
