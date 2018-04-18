using CPFrameWork.Global;
using NVelocity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CPFrameWork.UIInterface.Tree
{
    [CPName("树工具", 3)]
    public class CPTreeExpression
    {
        public const string DataRowKey = "CPDR_4092415F-0918-43C5-B6C5-EED77F90FE01";
        private VelocityContext _vltContext;
        public CPTreeExpression(VelocityContext context)
        {
            this._vltContext = context;
        }
        [CPName("获取字段值")]
        public string Field([CPName("字段名")]string fieldName)
        {
            object obj = null;
            DataRow dr = this._vltContext.Get(DataRowKey) as DataRow;
            obj = dr[fieldName];
            if (obj == null)
                return "";
            else
            {

                return obj.ToString().Trim();
            }
        }



    }
}
