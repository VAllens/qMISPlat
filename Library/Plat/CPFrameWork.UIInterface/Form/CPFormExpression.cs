using CPFrameWork.Global;
using NVelocity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPFrameWork.UIInterface.Form
{
    [CPName("表单工具", 2)]
    public class CPFormExpression
    {
        public const string DataRowKey = "CPDR_D6ED5027-744D-4D9F-988D-536242C6DD9E";
        public const string PKValueKey = "CPPKValue_D6ED5027-744D-4D9F-988D-536242C6DD9E";
        public const string MainTableKey = "CPMainTable_D6ED5027-744D-4D9F-988D-536242C6DD9E";
        public const string MainTablePKKey = "CPMainTablePK_D6ED5027-744D-4D9F-988D-536242C6DD9E";
        private VelocityContext _vltContext;
        public CPFormExpression(VelocityContext context)
        {
            this._vltContext = context;
        }
        [CPName("获取字段值")]
        public string Field([CPName("表名")]string tableName,[CPName("字段名")]string fieldName)
        {
            object obj = "";
            dynamic FormData=  this._vltContext.Get(DataRowKey) as dynamic;
            string pkValue = this._vltContext.Get(PKValueKey).ToString();
            string MainTable = this._vltContext.Get(MainTableKey).ToString();
            string MainTablePK = this._vltContext.Get(MainTablePKKey).ToString();
            //如果是主键，则直接返回主键
            if (MainTable.Equals(tableName, StringComparison.CurrentCultureIgnoreCase)
                &&
                fieldName.Equals(MainTablePK, StringComparison.CurrentCultureIgnoreCase))
            {
                return pkValue;
            } 
            if (MainTable.Equals(tableName, StringComparison.CurrentCultureIgnoreCase))
            {
                //主表
                obj = FormData[tableName][fieldName];
            }
            else
            {
                //暂时只支持取拓展表第一行数据
                int nIndex = 0;
                foreach (var cRowValue in FormData[tableName])
                {
                    if (nIndex == 0)
                    {
                        obj = cRowValue[fieldName];
                        break;
                    }
                    nIndex++;
                }
            }
            if (obj == null)
                return "";
            else
                return obj.ToString();

        }
    }
}
