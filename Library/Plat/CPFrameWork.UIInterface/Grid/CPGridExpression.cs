using CPFrameWork.Global;
using NVelocity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CPFrameWork.UIInterface.Grid
{
    [CPName("列表工具",1)]
    public class CPGridExpression
    { 
    public const string DataRowKey = "CPDR_8B631857-0A56-4A19-9CAB-B5324E8B5FA5"; 
        private VelocityContext _vltContext;
        public CPGridExpression(VelocityContext context)
        {
            this._vltContext = context;
        }
        [CPName("获取字段值")]
        public string Field([CPName("字段名")]string fieldName)
        {
            object obj = null;
            if (this._vltContext.Get(DataRowKey) is DataRow)
            {
                DataRow dr = this._vltContext.Get(DataRowKey) as DataRow;
                obj = dr[fieldName];
            }
            else
            {
                if (this._vltContext.Get(DataRowKey).GetType().Name == "DataRowView")
                {
                    DataRowView dr = this._vltContext.Get(DataRowKey) as DataRowView;
                    obj = dr[fieldName];
                }
            }
            if (obj == null)
                return "";
            else
            {

                return obj.ToString().Trim();
            }
        }



    }
}
