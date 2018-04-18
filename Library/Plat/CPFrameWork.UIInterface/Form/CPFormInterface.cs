using CPFrameWork.Global;
using CPFrameWork.Organ;
using CPFrameWork.Organ.Application;
using CPFrameWork.Organ.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CPFrameWork.UIInterface.Form
{
     
    public class ICPFormBeforeLoadEventArgs
    {
        /// <summary>
        /// 表单配置对象
        /// </summary>
        public CPForm Form { get; set; }
        /// <summary>
        /// 表单主键值
        /// </summary>
        public string PKValue { get; set; }
        /// <summary>
        /// 表单数据
        /// </summary>
        public DataSet FormData { get; set; }
        /// <summary>
        /// 是否处于只读状态
        /// </summary>
        public bool IsView
        {
            get
            {
                if (CPAppContext.CheckHasQueryStringKeyAndValue("IsView"))
                {
                    if (CPAppContext.QueryString<bool>("IsView"))
                    {
                        return true;
                    }
                    else
                        return false;
                }
                    
                else
                    return false;
            }
        }
        /// <summary>
        /// 是否处于编辑状态
        /// </summary>
        public bool IsEdit
        {
            get
            {
                if (CPAppContext.CheckHasQueryStringKeyAndValue("PKValues"))
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// 是否处于新增状态
        /// </summary>
        public bool IsAdd
        {
            get
            {
                if (CPAppContext.CheckHasQueryStringKeyAndValue("PKValues"))
                    return false;
                else
                    return true;
            }
        }

        public ICPFormBeforeLoadEventArgs(CPForm form,string pkValue,DataSet formData)
        {
            this.Form = form;
            this.PKValue = PKValue;
            this.FormData = formData;
        }
    }
    public interface ICPFormBeforeLoad
    {
        void BeforeLoad(ICPFormBeforeLoadEventArgs e);
    }
    public  interface ICPFormAfterSave
    {
        void AfterSave(ICPFormAfterSaveEventArgs e);
    }

    public class ICPFormAfterSaveEventArgs
    {
        /// <summary>
        /// 表单配置对象
        /// </summary>
        public CPForm Form { get; set; }
        /// <summary>
        /// 表单主键值
        /// </summary>
        public string PKValue { get; set; }
        /// <summary>
        /// 表单数据
        /// </summary>
        public dynamic FormData { get; set; }
        /// <summary>
        /// 获取某个字段的值
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="rowIndex">从0开始</param>
        /// <returns></returns>
        public string GetFieldValue(string tableName,string fieldName,int rowIndex)
        {
            //如果是主键，则直接返回主键
            if(Form.MainTableName.Equals(tableName,StringComparison.CurrentCultureIgnoreCase)
                &&
                fieldName.Equals(Form.PKFieldName,StringComparison.CurrentCultureIgnoreCase))
            {
                return PKValue;
            }
            object obj = null;
            var mainTableObject = JsonConvert.DeserializeObject<JObject>(Convert.ToString(FormData[tableName]));
            if(Form.MainTableName.Equals(tableName,StringComparison.CurrentCultureIgnoreCase))
            {
                //主表
                obj = mainTableObject[fieldName];
            }
            else
            {
                int nIndex = 0;
                foreach (var cRowValue in mainTableObject)
                {
                    if(rowIndex == nIndex)
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
        /// <summary>
        /// 是否处于只读状态
        /// </summary>
        public bool IsView
        {
            get
            {
                if (CPAppContext.CheckHasQueryStringKeyAndValue("IsView"))
                {
                    if (CPAppContext.QueryString<bool>("IsView"))
                    {
                        return true;
                    }
                    else
                        return false;
                }

                else
                    return false;
            }
        }
        /// <summary>
        /// 是否处于编辑状态
        /// </summary>
        public bool IsEdit
        {
            get
            {
                if (CPAppContext.CheckHasQueryStringKeyAndValue("PKValues"))
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// 是否处于新增状态
        /// </summary>
        public bool IsAdd
        {
            get
            {
                if (CPAppContext.CheckHasQueryStringKeyAndValue("PKValues"))
                    return false;
                else
                    return true;
            }
        }

        public ICPFormAfterSaveEventArgs(CPForm form, string pkValue, dynamic formData)
        {
            this.Form = form;
            this.PKValue = pkValue;
            this.FormData = formData;
        }
    }



}
