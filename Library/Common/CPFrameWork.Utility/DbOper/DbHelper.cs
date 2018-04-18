using System;
using System.Data;
using System.Diagnostics; 
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections;
using Microsoft.Extensions.Configuration;

namespace CPFrameWork.Utility.DbOper
{
   public class DbHelper
    {
        public  enum DbTypeEnum
        {
            SqlServer = 1,
            Oracle =2
        }

        #region  属性变量      
        public string ConntionString
        {
            get;set;
        }
        //数据访问基础类--构造函数  
        public DbHelper(string dbIns, DbTypeEnum dbType)
        {
            //var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            //var Configuration = builder.Build();          
            //this.ConntionString = Configuration.GetConnectionString(dbIns);
            this.ConntionString = CPUtils.Configuration.GetSection("ConnectionStrings")[dbIns];
            this.DbType = dbType;
            
            
        } 
        /// <summary>  
        /// 数据库类型   
        /// </summary>   
        public DbTypeEnum DbType
        {
            get;set;
        }
        #endregion

       
        #region 转换参数  
        private System.Data.IDbDataParameter iDbPara(string ParaName, string DataType)
        {
            switch (this.DbType)
            {
                case  DbTypeEnum.SqlServer:
                    return GetSqlPara(ParaName, DataType);
                //case  DbTypeEnum.Oracle:
                //    return GetOleDbPara(ParaName, DataType); 
                default:
                    return GetSqlPara(ParaName, DataType);
            }
        }

        private SqlParameter GetSqlPara(string ParaName, string DataType)
        {
            switch (DataType)
            {
                case "Decimal":
                    return new SqlParameter(ParaName, SqlDbType.Decimal);
                case "Varchar":
                    return new SqlParameter(ParaName, SqlDbType.VarChar);
                case "DateTime":
                    return new SqlParameter(ParaName, SqlDbType.DateTime);
                case "Iamge":
                    return new SqlParameter(ParaName, SqlDbType.Image);
                case "Int":
                    return new SqlParameter(ParaName, SqlDbType.Int);
                case "Text":
                    return new SqlParameter(ParaName, SqlDbType.NText);
                default:
                    return new SqlParameter(ParaName, SqlDbType.VarChar);
            }
        }
        //private OracleParameter GetOraclePara(string ParaName, string DataType)
        //{
        //    switch (DataType)
        //    {
        //        case "Decimal":
        //            return new OracleParameter(ParaName, OracleType.Double);
        //        case "Varchar":
        //            return new OracleParameter(ParaName, OracleType.VarChar);
        //        case "DateTime":
        //            return new OracleParameter(ParaName, OracleType.DateTime);
        //        case "Iamge":
        //            return new OracleParameter(ParaName, OracleType.BFile);
        //        case "Int":
        //            return new OracleParameter(ParaName, OracleType.Int32);
        //        case "Text":
        //            return new OracleParameter(ParaName, OracleType.LongVarChar);
        //        default:
        //            return new OracleParameter(ParaName, OracleType.VarChar);

        //    }
        //}
        //private OleDbParameter GetOleDbPara(string ParaName, string DataType)
        //{
        //    switch (DataType)
        //    {
        //        case "Decimal":
        //            return new OleDbParameter(ParaName, System.Data.DbType.Decimal);
        //        case "Varchar":
        //            return new OleDbParameter(ParaName, System.Data.DbType.String);
        //        case "DateTime":
        //            return new OleDbParameter(ParaName, System.Data.DbType.DateTime);
        //        case "Iamge":
        //            return new OleDbParameter(ParaName, System.Data.DbType.Binary);
        //        case "Int":
        //            return new OleDbParameter(ParaName, System.Data.DbType.Int32);
        //        case "Text":
        //            return new OleDbParameter(ParaName, System.Data.DbType.String);
        //        default:
        //            return new OleDbParameter(ParaName, System.Data.DbType.String);
        //    }
        //}
        #endregion
        #region 创建 Connection 和 Command  
        public IDbConnection GetConnection()
        {
            switch (this.DbType)
            {
                case   DbTypeEnum.SqlServer:
                    return new SqlConnection(this.ConntionString);
                //case  DbTypeEnum.Oracle:
                //    return new OracleConnection(this.ConntionString); 
                default:
                    return new SqlConnection(this.ConntionString);
            }
        }
        private IDbCommand GetCommand(string Sql, IDbConnection iConn)
        {
            switch (this.DbType)
            {
                case DbTypeEnum.SqlServer:
                    return new SqlCommand(Sql, (SqlConnection)iConn);
                //case "Oracle":
                //    return new OracleCommand(Sql, (OracleConnection)iConn); 
                default:
                    return new SqlCommand(Sql, (SqlConnection)iConn);
            }
        }
        private IDbCommand GetCommand()
        {
            switch (this.DbType)
            {
                case DbTypeEnum.SqlServer:
                    return new SqlCommand();
                //case "Oracle":
                //    return new OracleCommand(); 
                default:
                    return new SqlCommand();
            }
        }
        private IDataAdapter GetAdapater(string Sql, IDbConnection iConn)
        {
            switch (this.DbType)
            {
                case DbTypeEnum.SqlServer:
                    return new SqlDataAdapter(Sql, (SqlConnection)iConn);
                //case "Oracle":
                //    return new OracleDataAdapter(Sql, (OracleConnection)iConn); 
                default:
                    return new SqlDataAdapter(Sql, (SqlConnection)iConn); ;
            }
        }
        private IDataAdapter GetAdapater(IDbCommand cmd, IDbConnection iConn)
        {
            switch (this.DbType)
            {
                case DbTypeEnum.SqlServer:
                    return new SqlDataAdapter(cmd as SqlCommand);
                //case "Oracle":
                //    return new OracleDataAdapter(Sql, (OracleConnection)iConn); 
                default:
                    return new SqlDataAdapter(cmd as SqlCommand);
            }
        }
        private IDataAdapter GetAdapater()
        {
            switch (this.DbType)
            {
                case DbTypeEnum.SqlServer:
                    return new SqlDataAdapter();
                //case "Oracle":
                //    return new OracleDataAdapter(); 
                default:
                    return new SqlDataAdapter();
            }
        }
        private IDataAdapter GetAdapater(IDbCommand iCmd)
        {
            switch (this.DbType)
            {
                case DbTypeEnum.SqlServer:
                    return new SqlDataAdapter((SqlCommand)iCmd);
                //case "Oracle":
                //    return new OracleDataAdapter((OracleCommand)iCmd); 
                default:
                    return new SqlDataAdapter((SqlCommand)iCmd);
            }
        }
        #endregion
        #region  执行简单SQL语句  
        public int ExecuteNonQuery(IDbCommand cmd)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                iConn.Open();
                try
                {
                    cmd.Connection = iConn;
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Exception E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    if (iConn.State != ConnectionState.Closed)
                    {
                        iConn.Close();
                    }
                }
            }
        }
        /// <summary>  
        /// 执行SQL语句，返回影响的记录数  
        /// </summary>  
        /// <param name="SQLString">SQL语句</param>  
        /// <returns>影响的记录数</returns>  
        public int ExecuteNonQuery(string SqlString)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                using (IDbCommand iCmd = GetCommand(SqlString, iConn))
                {
                    iConn.Open();
                    try
                    {
                        int rows = iCmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Exception E)
                    {
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                    }
                }
            }
        }
        /// <summary>  
        /// 执行多条SQL语句，实现数据库事务。  
        /// </summary>  
        /// <param name="SQLStringList">多条SQL语句</param>          
        public void ExecuteNonQuery(ArrayList SQLStringList)
        {
            //using作为语句，用于定义一个范围，在此范围的末尾将释放对象  
            using (IDbConnection iConn = this.GetConnection())
            {
                iConn.Open();
                using (IDbCommand iCmd = GetCommand())
                {
                    iCmd.Connection = iConn;
                    using (System.Data.IDbTransaction iDbTran = iConn.BeginTransaction())
                    {
                        iCmd.Transaction = iDbTran;
                        try
                        {
                            for (int n = 0; n < SQLStringList.Count; n++)
                            {
                                string strsql = SQLStringList[n].ToString();
                                if (strsql.Trim().Length > 1)
                                {
                                    iCmd.CommandText = strsql;
                                    iCmd.ExecuteNonQuery();
                                }
                            }
                            iDbTran.Commit();
                        }
                        catch (System.Exception E)
                        {
                            iDbTran.Rollback();
                            throw new Exception(E.Message);
                        }
                        finally
                        {
                            if (iConn.State != ConnectionState.Closed)
                            {
                                iConn.Close();
                            }
                        }
                    }
                }
            }
        }
        /// <summary>  
        /// 执行带一个存储过程参数的的SQL语句。  
        /// </summary>  
        /// <param name="SQLString">SQL语句</param>  
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>  
        /// <returns>影响的记录数</returns>  
        public int ExecuteNonQuery(string SqlString, string content)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                using (IDbCommand iCmd = GetCommand(SqlString, iConn))
                {
                    IDataParameter myParameter = this.iDbPara("@content", "Text");
                    myParameter.Value = content;
                    iCmd.Parameters.Add(myParameter);
                    iConn.Open();
                    try
                    {
                        int rows = iCmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                    }
                }
            }
        }
        /// <summary>  
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)  
        /// </summary>  
        /// <param name="strSQL">SQL语句</param>  
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>  
        /// <returns>影响的记录数</returns>  
        public int ExecuteNonQueryInsertImg(string SqlString, byte[] fs)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                using (IDbCommand iCmd = GetCommand(SqlString, iConn))
                {
                    IDataParameter myParameter = this.iDbPara("@content", "Image");
                    myParameter.Value = fs;
                    iCmd.Parameters.Add(myParameter);
                    iConn.Open();
                    try
                    {
                        int rows = iCmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                    }
                }
            }
        }
        public object ExecuteScalar(IDbCommand cmd)
        {
            using (IDbConnection iConn = GetConnection())
            {
                iConn.Open();
                try
                {
                    cmd.Connection = iConn;
                    object obj = cmd.ExecuteScalar();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (System.Exception e)
                {
                    throw new Exception(e.Message);
                }
                finally
                {
                    if (iConn.State != ConnectionState.Closed)
                    {
                        iConn.Close();
                    }
                }
            }
        }
        /// <summary>  
        /// 执行一条计算查询结果语句，返回查询结果（object）。  
        /// </summary>  
        /// <param name="SQLString">计算查询结果语句</param>  
        /// <returns>查询结果（object）</returns>  
        public object ExecuteScalar(string SqlString)
        {
            using (IDbConnection iConn = GetConnection())
            {
                using (IDbCommand iCmd = GetCommand(SqlString, iConn))
                {
                    iConn.Open();
                    try
                    {
                        object obj = iCmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                    }
                }
            }
        }
       
        /// <summary>  
        /// 执行查询语句，返回DataSet  
        /// </summary>  
        /// <param name="SQLString">查询语句</param>  
        /// <returns>DataSet</returns>  
        public DataSet ExecuteDataSet(string sqlString)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                using (IDbCommand iCmd = GetCommand(sqlString, iConn))
                {
                    DataSet ds = new DataSet();
                    iConn.Open();
                    try
                    {
                        IDataAdapter iAdapter = this.GetAdapater(sqlString, iConn);
                        iAdapter.Fill(ds);
                        return ds;
                    }
                    catch (System.Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                    }
                }
            }
        }
        /// <summary>  
        /// 执行查询语句，返回DataSet  
        /// </summary>  
        /// <param name="sqlString">查询语句</param>  
        /// <param name="dataSet">要填充的DataSet</param>  
        /// <param name="tableName">要填充的表名</param>  
        /// <returns>DataSet</returns>  
        public DataSet ExecuteDataSet(string sqlString, DataSet dataSet, string tableName)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                using (IDbCommand iCmd = GetCommand(sqlString, iConn))
                {
                    iConn.Open();
                    try
                    {
                        IDataAdapter iAdapter = this.GetAdapater(sqlString, iConn);
                        
                        ((SqlDataAdapter)iAdapter).Fill(dataSet, tableName);
                        return dataSet;
                    }
                    catch (System.Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                    }
                }
            }
        }
        /// <summary>  
        /// 执行SQL语句 返回存储过程  
        /// </summary>  
        /// <param name="sqlString">Sql语句</param>  
        /// <param name="dataSet">要填充的DataSet</param>  
        /// <param name="startIndex">开始记录</param>  
        /// <param name="pageSize">页面记录大小</param>  
        /// <param name="tableName">表名称</param>  
        /// <returns>DataSet</returns>  
        public DataSet ExecuteDataSet(string sqlString, DataSet dataSet, int startIndex, int pageSize, string tableName)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                iConn.Open();
                try
                {
                    IDataAdapter iAdapter = this.GetAdapater(sqlString, iConn);

                    ((SqlDataAdapter)iAdapter).Fill(dataSet, startIndex, pageSize, tableName);

                    return dataSet;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    if (iConn.State != ConnectionState.Closed)
                    {
                        iConn.Close();
                    }
                }
            }
        }
        /// <summary>  
        /// 执行查询语句，向XML文件写入数据  
        /// </summary>  
        /// <param name="sqlString">查询语句</param>  
        /// <param name="xmlPath">XML文件路径</param>  
        public void WriteToXml(string sqlString, string xmlPath)
        {
            ExecuteDataSet(sqlString).WriteXml(xmlPath);
        }
        /// <summary>  
        /// 执行查询语句  
        /// </summary>  
        /// <param name="SqlString">查询语句</param>  
        /// <returns>DataTable </returns>  
        public DataTable ExecuteDataTable(string sqlString)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                //IDbCommand iCmd  =  GetCommand(sqlString,iConn);  
                DataSet ds = new DataSet();
                try
                {
                    IDataAdapter iAdapter = this.GetAdapater(sqlString, iConn);
                    iAdapter.Fill(ds);
                }
                catch (System.Exception e)
                {
                    throw new Exception(e.Message);
                }
                finally
                {
                    if (iConn.State != ConnectionState.Closed)
                    {
                        iConn.Close();
                    }
                }
                return ds.Tables[0];
            }
        }
        /// <summary>  
        /// 执行查询语句  
        /// </summary>  
        /// <param name="SqlString">查询语句</param>  
        /// <returns>DataTable </returns>  
        public DataTable ExecuteDataTable(string SqlString, string Proc)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                using (IDbCommand iCmd = GetCommand(SqlString, iConn))
                {
                    iCmd.CommandType = CommandType.StoredProcedure;
                    DataSet ds = new DataSet();
                    try
                    {
                        IDataAdapter iDataAdapter = this.GetAdapater(SqlString, iConn);
                        iDataAdapter.Fill(ds);
                    }
                    catch (System.Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                    }
                    return ds.Tables[0];
                }
            }
        }
        /// <summary>  
        /// 执行查询，并以DataView返回结果集   
        /// </summary>  
        /// <param name="Sql">SQL语句</param>  
        /// <returns>DataView</returns>  
        public DataView ExecuteDataView(string Sql)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                using (IDbCommand iCmd = GetCommand(Sql, iConn))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        IDataAdapter iDataAdapter = this.GetAdapater(Sql, iConn);
                        iDataAdapter.Fill(ds);
                        return ds.Tables[0].DefaultView;
                    }
                    catch (System.Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                    }
                }
            }
        }
        #endregion
        #region 执行带参数的SQL语句  
        /// <summary>  
        /// 执行SQL语句，返回影响的记录数  
        /// </summary>  
        /// <param name="SQLString">SQL语句</param>  
        /// <returns>影响的记录数</returns>  
        public int ExecuteNonQuery(string SQLString, params IDataParameter[] iParms)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                IDbCommand iCmd = GetCommand();
                {
                    try
                    {
                        PrepareCommand(out iCmd, iConn, null, SQLString, iParms);
                        int rows = iCmd.ExecuteNonQuery();
                        iCmd.Parameters.Clear();
                        return rows;
                    }
                    catch (System.Exception E)
                    {
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        iCmd.Dispose();
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                    }
                }
            }
        }
        /// <summary>  
        /// 执行多条SQL语句，实现数据库事务。  
        /// </summary>  
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>  
        public void ExecuteNonQueryTran(Hashtable SQLStringList)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                iConn.Open();
                using (IDbTransaction iTrans = iConn.BeginTransaction())
                {
                    IDbCommand iCmd = GetCommand();
                    try
                    {
                        //循环  
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            IDataParameter[] iParms = (IDataParameter[])myDE.Value;
                            PrepareCommand(out iCmd, iConn, iTrans, cmdText, iParms);
                            int val = iCmd.ExecuteNonQuery();
                            iCmd.Parameters.Clear();
                        }
                        iTrans.Commit();
                    }
                    catch
                    {
                        iTrans.Rollback();
                        throw;
                    }
                    finally
                    {
                        iCmd.Dispose();
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                    }
                }
            }
        }
        /// <summary>  
        /// 执行一条计算查询结果语句，返回查询结果（object）。  
        /// </summary>  
        /// <param name="SQLString">计算查询结果语句</param>  
        /// <returns>查询结果（object）</returns>  
        public object ExecuteScalar(string SQLString, params IDataParameter[] iParms)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                IDbCommand iCmd = GetCommand();
                {
                    try
                    {
                        PrepareCommand(out iCmd, iConn, null, SQLString, iParms);
                        object obj = iCmd.ExecuteScalar();
                        iCmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        iCmd.Dispose();
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                    }
                }
            }
        }
        /// <summary>  
        /// 执行查询语句，返回IDataAdapter  
        /// </summary>  
        /// <param name="strSQL">查询语句</param>  
        /// <returns>IDataAdapter</returns>  
        public IDataReader ExecuteReader(string strSQL)
        {
            IDbConnection iConn = this.GetConnection();
            {
                IDbCommand iCmd = GetCommand();
                {
                    try
                    {
                        PrepareCommand(out iCmd, iConn, null, strSQL,null);
                        System.Data.IDataReader iReader = iCmd.ExecuteReader();
                        iCmd.Parameters.Clear();
                        return iReader;
                    }
                    catch (System.Exception e)
                    {
                        iCmd.Dispose();
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                      
                        
                    }
                }
            }
        }
        /// <summary>  
        /// 执行查询语句，返回IDataReader  
        /// </summary>  
        /// <param name="strSQL">查询语句</param>  
        /// <returns> IDataReader </returns>  
        public IDataReader ExecuteReader(string SQLString, params IDataParameter[] iParms)
        {
            IDbConnection iConn = this.GetConnection();
            {
                IDbCommand iCmd = GetCommand();
                {
                    try
                    {
                        PrepareCommand(out iCmd, iConn, null, SQLString, iParms);
                        System.Data.IDataReader iReader = iCmd.ExecuteReader();
                        iCmd.Parameters.Clear();
                        return iReader;
                    }
                    catch (System.Exception e)
                    {
                        iCmd.Dispose();
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                      
                    }
                }
            }
        }
        /// <summary>  
        /// 执行查询语句，返回DataSet  
        /// </summary>  
        /// <param name="SQLString">查询语句</param>  
        /// <returns>DataSet</returns>  
        public DataSet ExecuteDataSet(string sqlString, CommandType cmdType, params IDataParameter[] iParms)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                IDbCommand iCmd = GetCommand();
                {
                    PrepareCommand(out iCmd, iConn, null, sqlString, iParms);
                    iCmd.CommandType = cmdType;
                    try
                    {
                        IDataAdapter iAdapter = this.GetAdapater(iCmd, iConn);
                        DataSet ds = new DataSet();
                        iAdapter.Fill(ds);
                        iCmd.Parameters.Clear();
                        return ds;
                    }
                    catch (System.Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        iCmd.Dispose();
                        if (iConn.State != ConnectionState.Closed)
                        {
                            iConn.Close();
                        }
                    }
                }
            }
        }
        /// <summary>  
        /// 初始化Command  
        /// </summary>  
        /// <param name="iCmd"></param>  
        /// <param name="iConn"></param>  
        /// <param name="iTrans"></param>  
        /// <param name="cmdText"></param>  
        /// <param name="iParms"></param>  
        private void PrepareCommand(out IDbCommand iCmd, IDbConnection iConn, System.Data.IDbTransaction iTrans, string cmdText, IDataParameter[] iParms)
        {
            if (iConn.State != ConnectionState.Open)
                iConn.Open();
            iCmd = this.GetCommand();
            iCmd.Connection = iConn;
            iCmd.CommandText = cmdText;
            if (iTrans != null)
                iCmd.Transaction = iTrans;
            iCmd.CommandType = CommandType.Text;//cmdType;  
            if (iParms != null)
            {
                foreach (IDataParameter parm in iParms)
                    iCmd.Parameters.Add(parm);
            }
        }
        #endregion
        #region 存储过程操作  
        /// <summary>  
        /// 执行存储过程  
        /// </summary>  
        /// <param name="storedProcName">存储过程名</param>  
        /// <param name="parameters">存储过程参数</param>  
        /// <returns>SqlDataReader</returns>  
        public SqlDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            IDbConnection iConn = this.GetConnection();
            {
                iConn.Open();

                using (SqlCommand sqlCmd = BuildQueryCommand(iConn, storedProcName, parameters))
                {
                    return sqlCmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
            }
        }
        /// <summary>  
        /// 执行存储过程  
        /// </summary>  
        /// <param name="storedProcName">存储过程名</param>  
        /// <param name="parameters">存储过程参数</param>  
        /// <param name="tableName">DataSet结果中的表名</param>  
        /// <returns>DataSet</returns>  
        public DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                DataSet dataSet = new DataSet();
                iConn.Open();
                IDataAdapter iDA = this.GetAdapater();
                iDA = this.GetAdapater(BuildQueryCommand(iConn, storedProcName, parameters));
                ((SqlDataAdapter)iDA).Fill(dataSet, tableName);
                if (iConn.State != ConnectionState.Closed)
                {
                    iConn.Close();
                }
                return dataSet;
            }
        }
        /// <summary>  
        /// 执行存储过程  
        /// </summary>  
        /// <param name="storedProcName">存储过程名</param>  
        /// <param name="parameters">存储过程参数</param>  
        /// <param name="tableName">DataSet结果中的表名</param>  
        /// <param name="startIndex">开始记录索引</param>  
        /// <param name="pageSize">页面记录大小</param>  
        /// <returns>DataSet</returns>  
        public DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, int startIndex, int pageSize, string tableName)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                DataSet dataSet = new DataSet();
                iConn.Open();
                IDataAdapter iDA = this.GetAdapater();
                iDA = this.GetAdapater(BuildQueryCommand(iConn, storedProcName, parameters));

                ((SqlDataAdapter)iDA).Fill(dataSet, startIndex, pageSize, tableName);
                if (iConn.State != ConnectionState.Closed)
                {
                    iConn.Close();
                }
                return dataSet;
            }
        }
        /// <summary>  
        /// 执行存储过程 填充已经存在的DataSet数据集   
        /// </summary>  
        /// <param name="storeProcName">存储过程名称</param>  
        /// <param name="parameters">存储过程参数</param>  
        /// <param name="dataSet">要填充的数据集</param>  
        /// <param name="tablename">要填充的表名</param>  
        /// <returns></returns>  
        public DataSet RunProcedure(string storeProcName, IDataParameter[] parameters, DataSet dataSet, string tableName)
        {
            using (IDbConnection iConn = this.GetConnection())
            {
                iConn.Open();
                IDataAdapter iDA = this.GetAdapater();
                iDA = this.GetAdapater(BuildQueryCommand(iConn, storeProcName, parameters));

                ((SqlDataAdapter)iDA).Fill(dataSet, tableName);

                if (iConn.State != ConnectionState.Closed)
                {
                    iConn.Close();
                }
                return dataSet;
            }
        }
        /// <summary>  
        /// 执行存储过程并返回受影响的行数  
        /// </summary>  
        /// <param name="storedProcName"></param>  
        /// <param name="parameters"></param>  
        /// <returns></returns>  
        public int RunProcedureNoQuery(string storedProcName, IDataParameter[] parameters)
        {
            int result = 0;
            using (IDbConnection iConn = this.GetConnection())
            {
                iConn.Open();
                using (SqlCommand scmd = BuildQueryCommand(iConn, storedProcName, parameters))
                {
                    result = scmd.ExecuteNonQuery();
                }

                if (iConn.State != ConnectionState.Closed)
                {
                    iConn.Close();
                }
            }
            return result;
        }
        public string RunProcedureExecuteScalar(string storeProcName, IDataParameter[] parameters)
        {
            string result = string.Empty;
            using (IDbConnection iConn = this.GetConnection())
            {
                iConn.Open();
                using (SqlCommand scmd = BuildQueryCommand(iConn, storeProcName, parameters))
                {
                    object obj = scmd.ExecuteScalar();
                    if (obj == null)
                        result = null;
                    else
                        result = obj.ToString();
                }
                if (iConn.State != ConnectionState.Closed)
                {
                    iConn.Close();
                }
            }
            return result;
        }
        /// <summary>  
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)  
        /// </summary>  
        /// <param name="connection">数据库连接</param>  
        /// <param name="storedProcName">存储过程名</param>  
        /// <param name="parameters">存储过程参数</param>  
        /// <returns>SqlCommand</returns>  
        private SqlCommand BuildQueryCommand(IDbConnection iConn, string storedProcName, IDataParameter[] parameters)
        {
            IDbCommand iCmd = GetCommand(storedProcName, iConn);
            iCmd.CommandType = CommandType.StoredProcedure;
            if (parameters == null)
            {
                return (SqlCommand)iCmd;
            }
            foreach (IDataParameter parameter in parameters)
            {
                iCmd.Parameters.Add(parameter);
            }
            return (SqlCommand)iCmd;
        }
 
        #endregion
    }
}
