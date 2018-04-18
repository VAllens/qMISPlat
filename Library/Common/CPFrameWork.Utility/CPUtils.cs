using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CPFrameWork.Utility
{
  public   class CPUtils
    {
        public static IConfigurationRoot Configuration { get; set; }

        #region dataTable转换成Json格式
        //private static string FormatJSONData(string data)
        //{
        //    data = data.Replace("\t", "");
        //   data = data.Replace("\r\n", "\\r\\n");
        //    data = data.Replace("\n", "\\n");
        //    data = data.Replace("\\", "\\\\");
        //    data = data.Replace("\"", "\\\"");
        //    return data;
        //} 
        /// <summary>  
        /// dataTable转换成Json格式 ，这个里面会判断如果数据类型为时间，并且时分秒为0，则只返回到天数据
        /// </summary>  
        /// <param name="dt"></param>  
        /// <returns></returns>  
        public static string DataTable2Json2(DataTable dt)
        {
            if (dt.Rows.Count <= 0)
                return "[]";
            #region  以下是老的代码，不用了，暂时作个备份 吧。

            //StringBuilder jsonBuilder = new StringBuilder(); 
            //jsonBuilder.Append("[");
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    jsonBuilder.Append("{");
            //    for (int j = 0; j < dt.Columns.Count; j++)
            //    {
            //        jsonBuilder.Append("\"");
            //        jsonBuilder.Append(dt.Columns[j].ColumnName);
            //        if (dt.Columns[j].DataType == Type.GetType("System.Boolean"))
            //        {
            //            jsonBuilder.Append("\":\"");
            //            jsonBuilder.Append(dt.Rows[i][j].ToString().Trim().ToLower());
            //            jsonBuilder.Append("\",");
            //        }
            //        else if (dt.Columns[j].DataType == Type.GetType("System.DateTime"))
            //        {
            //            string sValue = dt.Rows[i][j].ToString();
            //            if(string.IsNullOrEmpty(sValue)==false)
            //            {
            //                DateTime dt1 = Convert.ToDateTime(sValue);
            //                DateTime dt2 = Convert.ToDateTime(dt1.Year + "-" + dt1.Month + "-" + dt1.Day + " 00:00:01");
            //                if(dt2 >dt1)
            //                {
            //                    sValue = dt1.ToShortDateString();
            //                }
            //            }
            //            jsonBuilder.Append("\":\"");
            //            jsonBuilder.Append(sValue);
            //            jsonBuilder.Append("\",");
            //        }
            //        else
            //        {
            //            jsonBuilder.Append("\":\"");
            //            jsonBuilder.Append(FormatJSONData(dt.Rows[i][j].ToString()));
            //            jsonBuilder.Append("\",");

            //        }
            //    }
            //    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            //    jsonBuilder.Append("},");
            //}
            //jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            //jsonBuilder.Append("]"); 
            //return jsonBuilder.ToString();
            #endregion

            ArrayList arrayList = new ArrayList();
            foreach (DataRow dataRow in dt.Rows)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();  //实例化一个参数集合
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    //dictionary.Add(dataColumn.ColumnName, dataRow[dataColumn.ColumnName]);
                    if (dataColumn.DataType == Type.GetType("System.Boolean"))
                    {
                        dictionary.Add(dataColumn.ColumnName, dataRow[dataColumn.ColumnName].ToString().Trim().ToLower());
                    }
                    else if (dataColumn.DataType == Type.GetType("System.DateTime"))
                    {
                        string sValue = dataRow[dataColumn.ColumnName].ToString();
                        if (string.IsNullOrEmpty(sValue) == false)
                        {
                            DateTime dt1 = Convert.ToDateTime(sValue);
                            DateTime dt2 = Convert.ToDateTime(dt1.Year + "-" + dt1.Month + "-" + dt1.Day + " 00:00:01");
                            if (dt2 > dt1)
                            {
                                sValue = dt1.ToShortDateString();
                            }
                        }
                        dictionary.Add(dataColumn.ColumnName, sValue);
                    }
                    else
                    {
                        dictionary.Add(dataColumn.ColumnName, dataRow[dataColumn.ColumnName].ToString());

                    }
                }
                arrayList.Add(dictionary); //ArrayList集合中添加键值
            }

            string ss = JsonConvert.SerializeObject(arrayList);
            return ss;
        }
        /// <summary>  
        /// dataTable转换成Json格式  
        /// </summary>  
        /// <param name="dt"></param>  
        /// <returns></returns>  
        public static string DataTable2Json(DataTable dt)
        {
            if (dt.Rows.Count <= 0)
                return "[]";

            #region 以下是老的代码，作个备份 吧。
            //StringBuilder jsonBuilder = new StringBuilder(); 
            //jsonBuilder.Append("[");
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    jsonBuilder.Append("{");
            //    for (int j = 0; j < dt.Columns.Count; j++)
            //    {
            //        jsonBuilder.Append("\"");
            //        jsonBuilder.Append(dt.Columns[j].ColumnName);
            //        if (dt.Columns[j].DataType == Type.GetType("System.Boolean"))
            //        {
            //            jsonBuilder.Append("\":\"");
            //            jsonBuilder.Append(dt.Rows[i][j].ToString().Trim().ToLower());
            //            jsonBuilder.Append("\",");
            //        }
            //        else
            //        {
            //            jsonBuilder.Append("\":\"");
            //            jsonBuilder.Append(FormatJSONData(dt.Rows[i][j].ToString()));
            //            jsonBuilder.Append("\",");

            //        }
            //    }
            //    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            //    jsonBuilder.Append("},");
            //}
            //jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            //jsonBuilder.Append("]"); 
            //return jsonBuilder.ToString();
            #endregion

            ArrayList arrayList = new ArrayList();
            foreach (DataRow dataRow in dt.Rows)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();  //实例化一个参数集合
                foreach (DataColumn dataColumn in dt.Columns)
                { 
                    if (dataColumn.DataType == Type.GetType("System.Boolean"))
                    {
                        dictionary.Add(dataColumn.ColumnName, dataRow[dataColumn.ColumnName].ToString().Trim().ToLower());
                    } 
                    else
                    {
                        dictionary.Add(dataColumn.ColumnName, dataRow[dataColumn.ColumnName].ToString());

                    }
                }
                arrayList.Add(dictionary); //ArrayList集合中添加键值
            }

            string ss = JsonConvert.SerializeObject(arrayList);
            return ss;
        }


        /// <summary>  
        /// 将dataTable结构转成JSON，主要针对表没有数据的情况下使用  
        /// </summary>  
        /// <param name="dt"></param>  
        /// <returns></returns>  
        public static string DataTable2JsonWhenTableNull(DataTable dt)
        {
            if (dt.Columns.Count <= 0)
                return "[]";
            StringBuilder jsonBuilder = new StringBuilder(); 
            jsonBuilder.Append("[");
            jsonBuilder.Append("{");
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                jsonBuilder.Append("\"");
                jsonBuilder.Append(dt.Columns[j].ColumnName);
                jsonBuilder.Append("\":");
                jsonBuilder.Append("\"\",");
            }
            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append("},");
            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append("]"); 
            return jsonBuilder.ToString();
        }

        public static string DataRow2Json(DataRow dr)
        {

            #region 以下是老的代码，作个备份
            //StringBuilder jsonBuilder = new StringBuilder(); 
            //jsonBuilder.Append("{");
            //for (int j = 0; j < dr.Table.Columns.Count; j++)
            //{
            //    jsonBuilder.Append("\"");
            //    jsonBuilder.Append(dr.Table.Columns[j].ColumnName);
            //    if (dr.Table.Columns[j].DataType == Type.GetType("System.Boolean"))
            //    {
            //        jsonBuilder.Append("\":\"");
            //        jsonBuilder.Append(dr[j].ToString().Trim().ToLower());
            //        jsonBuilder.Append("\",");
            //    }
            //    else
            //    {
            //        jsonBuilder.Append("\":\"");
            //        jsonBuilder.Append(FormatJSONData(dr[j].ToString()));
            //        jsonBuilder.Append("\",");

            //    }
            //}
            //jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            //jsonBuilder.Append("}"); 
            //return jsonBuilder.ToString();
            #endregion

            Dictionary<string, object> dictionary = new Dictionary<string, object>();  //实例化一个参数集合
            foreach (DataColumn dataColumn in dr.Table.Columns)
            {
                if (dataColumn.DataType == Type.GetType("System.Boolean"))
                {
                    dictionary.Add(dataColumn.ColumnName, dr[dataColumn.ColumnName].ToString().Trim().ToLower());
                }
                else
                {
                    dictionary.Add(dataColumn.ColumnName, dr[dataColumn.ColumnName].ToString());

                }
            }
            string ss = JsonConvert.SerializeObject(dictionary);
            return ss;
        }
        #endregion dataTable转换成Json格式
    }
}
