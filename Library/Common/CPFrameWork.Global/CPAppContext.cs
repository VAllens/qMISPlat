using CPFrameWork.Utility.DbOper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static CPFrameWork.Utility.DbOper.DbHelper;

namespace CPFrameWork.Global
{
   public   class CPAppContext
    {
        #region 依赖注入全局，获取运行时相关信息
        public static IHttpContextAccessor HttpContextAccessor{ get; set; }
    public static IHostingEnvironment HostingEnvironment { get; set; }
    public static IConfigurationRoot Configuration { get; set; }
        public static IServiceCollection ServiceCollection { get; set; }
        public static T GetService<T>()
        {
            return HttpContextAccessor.HttpContext.RequestServices.GetService<T>();
        }
        public static IEnumerable<T> GetServices<T>()
        {
            return HttpContextAccessor.HttpContext.RequestServices.GetServices<T>();
        }
        public static object GetService(Type type)
        {
            return HttpContextAccessor.HttpContext.RequestServices.GetService(type);
        }
        public static IEnumerable<object> GetServices(Type type)
        {
            return HttpContextAccessor.HttpContext.RequestServices.GetServices(type);
        }
        #endregion

        public static int RootParentId = -1;
        public static int InnerSysId = 1;

        public static string CPWebRootPath()
        {
            if (HostingEnvironment.IsDevelopment())
            {
                return "";
            }
            else if (HostingEnvironment.IsProduction())
            {
                return "/CPSite";
            }
            else if (HostingEnvironment.IsStaging())
            {
                return "/CPSite";
            }
            else
                return "";
        }
         
        #region  获取ip与MAC地址  

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        public static string GetClientIP()
        {
            try
            {
                object factory = CPAppContext.GetService(typeof(Microsoft.AspNetCore.Http.IHttpContextAccessor));

                Microsoft.AspNetCore.Http.HttpContext context = ((Microsoft.AspNetCore.Http.HttpContextAccessor)factory).HttpContext;
             
                var ip = context.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
                return ip;
            }
            catch (Exception)
            {
                return "未获取用户IP";
            }

        } 
        #endregion
        public static DbTypeEnum CurDbType()
        {
            return DbTypeEnum.SqlServer;
        }
        public static HttpContext GetHttpContext()
        {
            object factory = CPAppContext.GetService(typeof(Microsoft.AspNetCore.Http.IHttpContextAccessor));

            Microsoft.AspNetCore.Http.HttpContext context = ((Microsoft.AspNetCore.Http.HttpContextAccessor)factory).HttpContext;
            return context;
        }
        public static bool ConvertIntToBool(object n)
        {
            if (Convert.ToInt32(n) == 1)
                return true;
            else
                return false;
        }
        public static int ConvertBoolToInt(bool b)
        {
            if (b)
                return 1;
            else
                return 0;
        }
        #region 根据数据库实例，和表名，自动获取表的所有字段名
        public static List<CPDbField> GetTableField(string dbInstance,string tableName)
        {
            List<CPDbField> col = new List<Global.CPDbField>();
            if (CurDbType() == DbTypeEnum.SqlServer)
            {
                #region sql
                string strSql = @"SELECT  表名 = d.name ,
                            字段序号 = a.colorder ,
                            字段名 = a.name ,
                            标识 = CASE WHEN COLUMNPROPERTY(a.id, a.name, 'IsIdentity') = 1
                                      THEN '1'
                                      ELSE '0'
                                 END ,
                            主键 = CASE WHEN EXISTS ( SELECT  1
                                                    FROM    sysobjects
                                                    WHERE   xtype = 'PK '
                                                            AND name IN (
                                                            SELECT  name
                                                            FROM    sysindexes
                                                            WHERE   indid IN (
                                                                    SELECT  indid
                                                                    FROM    sysindexkeys
                                                                    WHERE   id = a.id
                                                                            AND colid = a.colid ) ) )
                                      THEN '1'
                                      ELSE '0'
                                 END ,
                            类型 = b.name ,
                            占用字节数 = a.length ,
                            长度 = COLUMNPROPERTY(a.id, a.name, 'PRECISION ') ,
                            小数位数 = ISNULL(COLUMNPROPERTY(a.id, a.name, 'Scale '), 0) ,
                            允许空 = CASE WHEN a.isnullable = 1 THEN '1'
                                       ELSE '0'
                                  END ,
                            默认值 = ISNULL(e.text, ' ')
                    FROM    syscolumns a
                            LEFT   JOIN systypes b ON a.xtype = b.xusertype
                            INNER   JOIN sysobjects d ON a.id = d.id
                                                         AND d.xtype = 'U '
                                                         AND d.name <> 'dtproperties '
                            LEFT   JOIN syscomments e ON a.cdefault = e.id
                    WHERE d.name = '" + tableName + @"'
                    ORDER BY a.id ,
                            a.colorder";
                DbHelper _db = new DbHelper(dbInstance, DbTypeEnum.SqlServer);
                DataTable dt = _db.ExecuteDataSet(strSql).Tables[0];
               
                foreach (DataRow dr in dt.Rows)
                {
                    CPDbField f = new CPDbField();
                    f.TableName = Convert.IsDBNull(dr["表名"]) ? "" : dr["表名"].ToString();
                    f.FieldName = Convert.IsDBNull(dr["字段名"]) ? "" : dr["字段名"].ToString();
                    f.IsIdentity = Convert.IsDBNull(dr["标识"]) ? false : ConvertIntToBool(int.Parse(dr["标识"].ToString()));
                    f.IsPK = Convert.IsDBNull(dr["主键"]) ? false : ConvertIntToBool(int.Parse(dr["主键"].ToString()));
                    f.IsAllowNull = Convert.IsDBNull(dr["允许空"]) ? true : ConvertIntToBool(int.Parse(dr["允许空"].ToString()));
                    string sValueType = Convert.IsDBNull(dr["类型"]) ? "" : dr["类型"].ToString();
                    if (sValueType.Equals("int", StringComparison.CurrentCultureIgnoreCase)
                        || sValueType.Equals("bigint", StringComparison.CurrentCultureIgnoreCase)
                        )
                    {
                        f.ValueType = CPEnum.FieldValueTypeEnum.Int;
                    }
                    else if (sValueType.Equals("nvarchar", StringComparison.CurrentCultureIgnoreCase)
                        || sValueType.Equals("char", StringComparison.CurrentCultureIgnoreCase)
                        || sValueType.Equals("nchar", StringComparison.CurrentCultureIgnoreCase)
                        || sValueType.Equals("ntext", StringComparison.CurrentCultureIgnoreCase)
                        || sValueType.Equals("varchar", StringComparison.CurrentCultureIgnoreCase)
                        )
                    {
                        f.ValueType = CPEnum.FieldValueTypeEnum.String;
                    }
                    else if (sValueType.Equals("decimal", StringComparison.CurrentCultureIgnoreCase)
                        || sValueType.Equals("float", StringComparison.CurrentCultureIgnoreCase)
                        || sValueType.Equals("numeric", StringComparison.CurrentCultureIgnoreCase)

                        )
                    {
                        f.ValueType = CPEnum.FieldValueTypeEnum.Double;
                    }
                    else if (sValueType.Equals("date", StringComparison.CurrentCultureIgnoreCase)
                        || sValueType.Equals("datetime", StringComparison.CurrentCultureIgnoreCase)
                        || sValueType.Equals("datetime2", StringComparison.CurrentCultureIgnoreCase)
                        )
                    {
                        f.ValueType = CPEnum.FieldValueTypeEnum.DateTime;
                    }
                    else if (sValueType.Equals("uniqueidentifier", StringComparison.CurrentCultureIgnoreCase)
                       )
                    {
                        f.ValueType = CPEnum.FieldValueTypeEnum.GUID;
                    }
                    else
                    {
                        f.ValueType = CPEnum.FieldValueTypeEnum.String;
                    }
                    f.ValueLength = Convert.IsDBNull(dr["长度"]) ? 0 : int.Parse(dr["长度"].ToString());
                    col.Add(f);
                }
                #endregion
            }
            return col;

        }
        #endregion

        #region 根据数据库实例，获取所有的表名和主键名
        public static List<CPDbTable> GetTable(string dbInstance)
        {
            List<CPDbTable> col = new List<Global.CPDbTable>();
            if (CurDbType() == DbTypeEnum.SqlServer)
            {
                #region sql
                string strSql = @"SELECT * FROM (
	                    SELECT  表名 = d.name ,
								                    字段序号 = a.colorder ,
								                    字段名 = a.name ,
								                    标识 = CASE WHEN COLUMNPROPERTY(a.id, a.name, 'IsIdentity') = 1
										                      THEN '1'
										                      ELSE '0'
									                     END ,
								                    主键 = CASE WHEN EXISTS ( SELECT  1
														                    FROM    sysobjects
														                    WHERE   xtype = 'PK '
																                    AND name IN (
																                    SELECT  name
																                    FROM    sysindexes
																                    WHERE   indid IN (
																		                    SELECT  indid
																		                    FROM    sysindexkeys
																		                    WHERE   id = a.id
																				                    AND colid = a.colid ) ) )
										                      THEN '1'
										                      ELSE '0'
									                     END ,
								                    类型 = b.name ,
								                    占用字节数 = a.length ,
								                    长度 = COLUMNPROPERTY(a.id, a.name, 'PRECISION ') ,
								                    小数位数 = ISNULL(COLUMNPROPERTY(a.id, a.name, 'Scale '), 0) ,
								                    允许空 = CASE WHEN a.isnullable = 1 THEN '1'
										                       ELSE '0'
									                      END ,
								                    默认值 = ISNULL(e.text, ' ')
						                    FROM    syscolumns a
								                    LEFT   JOIN systypes b ON a.xtype = b.xusertype
								                    INNER   JOIN sysobjects d ON a.id = d.id
															                     AND d.xtype = 'U '
															                     AND d.name <> 'dtproperties '
								                    LEFT   JOIN syscomments e ON a.cdefault = e.id
                                    ) AS ccc WHERE ccc.主键=1 ORDER BY ccc.表名 
			                                     ";
                DbHelper _db = new DbHelper(dbInstance, DbTypeEnum.SqlServer);
                DataTable dt = _db.ExecuteDataSet(strSql).Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    
                    
                    string TableName = Convert.IsDBNull(dr["表名"]) ? "" : dr["表名"].ToString();
                    List<CPDbTable> tmpCol = col.Where(t => t.TableName.Equals(TableName)).ToList();
                    if (tmpCol.Count > 0)
                    {
                        tmpCol[0].PKNames += "," + (Convert.IsDBNull(dr["字段名"]) ? "" : dr["字段名"].ToString());
                    }
                    else
                    {
                        CPDbTable f = new CPDbTable();
                        f.TableName = TableName;
                        f.PKNames = Convert.IsDBNull(dr["字段名"]) ? "" : dr["字段名"].ToString();
                        col.Add(f);
                    }
                }
                #endregion
            }
            return col;

        }
        #endregion

        #region 根据数据库实例，获取所有的视图
        public static List<CPDbTable> GetView(string dbInstance)
        {
            List<CPDbTable> col = new List<Global.CPDbTable>();
            if (CurDbType() == DbTypeEnum.SqlServer)
            {
                #region sql
                string strSql = @"select name from sysobjects where xtype='V' 
			                                     ";
                DbHelper _db = new DbHelper(dbInstance, DbTypeEnum.SqlServer);
                DataTable dt = _db.ExecuteDataSet(strSql).Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    string TableName = Convert.IsDBNull(dr["name"]) ? "" : dr["name"].ToString();
                    CPDbTable f = new CPDbTable();
                    f.TableName = TableName;
                    f.PKNames = "";
                    col.Add(f);

                }
                #endregion
            }
            return col;

        }
        #endregion

        #region 根据数据库实例，获取数据名
        public static string GetDbName(string dbInstance)
        {
            DbHelper _db = new DbHelper(dbInstance, DbTypeEnum.SqlServer);
            string db = _db.GetConnection().Database;
            _db = null;
            return db;
        }
        #endregion

        /// <summary>
        /// 存储文件的真实目录
        /// </summary>
        /// <returns></returns>

        public static string CPFilesPath()
        {
            string StorePath = Configuration.GetSection("File")["StorePath"];
            if (string.IsNullOrEmpty(StorePath))
            {
                return HostingEnvironment.WebRootPath + System.IO.Path.DirectorySeparatorChar +
                    "CPFiles" + System.IO.Path.DirectorySeparatorChar;
            }
            else
            {
                if (StorePath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()) == false)
                    StorePath += System.IO.Path.DirectorySeparatorChar;
                return StorePath;
            }
        }
        public static string GetPara(string key)
        {
            DbHelper _db = new DbHelper("CPCommonIns",CurDbType());
            string strSql = "SELECT ParaValue FROM CP_Para  WHERE ParaKey='" + key+ "'";
            object obj = _db.ExecuteScalar(strSql);
            if (Convert.IsDBNull(obj) || obj == null)
                return "";
            else
                return obj.ToString();
        }
        public static string FormatSqlPara(string sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return sValue;
            sValue = System.Web.HttpUtility.UrlDecode(sValue);
            //增加处理SQL注入的代码
            return sValue;
        }
        public static bool CheckHasQueryStringKeyAndValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            if (CPAppContext.GetHttpContext().Request.Query.Keys.Contains(key) == false || CPAppContext.GetHttpContext().Request.Query[key] == "")
                return false;
            else
                return true;
        }
        public static bool CheckHasQueryStringKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            
            if (CPAppContext.GetHttpContext().Request.Query.Keys.Contains(key)==false)
                return false;
            else
                return true;
        }
        public static T QueryString<T>(string key)
        {
            if (CheckHasQueryStringKey(key) == false)
                return default(T);
            string s = GetHttpContext().Request.Query[key].ToString();
            s = System.Web.HttpUtility.UrlDecode(s);
            return ConvertTo<T>(s);
        }
        /// <summary>
        /// 将数据转换为指定类型
        /// </summary>
        /// <typeparam name="T">转换的目标类型</typeparam>
        /// <param name="data">转换的数据</param>
        private static T ConvertTo<T>(object data)
        {
            //如果数据为空，则返回
            if (data == null)
            {
                return default(T);
            }

            try
            {
                //如果数据是T类型，则直接转换
                if (data is T)
                {
                    return (T)data;
                }

                //如果目标类型是枚举
                if (typeof(T).BaseType == typeof(Enum))
                {
                    return GetInstance<T>(data.ToString());
                }

                //如果数据实现了IConvertible接口，则转换类型
                if (data is IConvertible)
                {
                    return (T)Convert.ChangeType(data, typeof(T));
                }
                else
                {
                    return default(T);
                }
            }
            catch
            {
                return default(T);
            }
        }
        private static T GetInstance<T>(string member)
        {
            return (T)Enum.Parse(typeof(T), member, true);
        }
        public static string  GetInsertSql(string tableName,DataColumnCollection columnCol,DataRow dr)
        {
            string sql = "insert into " + tableName;
            string field = "";
            string fieldValue = "";
            foreach (DataColumn dc  in columnCol)
            {
                if (dc.AutoIncrement)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(field))
                    field = dc.ColumnName;
                else
                    field += " , " + dc.ColumnName;
                if (string.IsNullOrEmpty(fieldValue))
                    fieldValue = "@" + dc.ColumnName;
                else
                    fieldValue += " , @" + dc.ColumnName;

            }
            sql += " ( " + field + ") VALUES ( " + fieldValue + ")";
            //SqlCommand cmd = new SqlCommand(sql, conn);
            //foreach (DataColumn dc in columnCol)
            //{
            //    if (dc.AutoIncrement)
            //    {
            //        continue;
            //    }
            //    cmd.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
            //}

            //  return cmd;
            return sql;
        }
    }
}
