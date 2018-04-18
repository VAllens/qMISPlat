using CPFrameWork.Global;
using CPFrameWork.Utility;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CPFrameWork.UIInterface.Tree
{
    public class CPTreeEngine
    {
        #region 获取实例 

        /// <summary>
        /// 获取节点对象服务类
        /// </summary>
        /// <returns></returns>
        public static void StartupInit(IServiceCollection services, IConfigurationRoot Configuration)
        {
            // Add framework services.
            services.AddDbContext<CPCommonDbContext>(options =>//手工高亮
                options.UseSqlServer(Configuration.GetConnectionString("CPCommonIns")));

            services.TryAddTransient<ICPCommonDbContext, CPCommonDbContext>();
            services.TryAddTransient<BaseCPTreeRep, CPTreeRep>();
            services.TryAddTransient<BaseRepository<CPTreeDataSource>, CPTreeDataSourceRep>();
            services.TryAddTransient<BaseRepository<CPTreeFunc>, CPTreeFuncRep>();
            services.TryAddTransient<CPTreeEngine, CPTreeEngine>();
        }
        public static CPTreeEngine Instance()
        {
            CPTreeEngine iObj = CPAppContext.GetService<CPTreeEngine>();
            return iObj;
        }


        #endregion  
        private BaseCPTreeRep _CPTreeRep;
        private BaseRepository<CPTreeDataSource> _CPTreeDataSourceRep;
        private BaseRepository<CPTreeFunc> _CPTreeFuncRep;

        public CPTreeEngine(
            BaseCPTreeRep CPTreeRep,
            BaseRepository<CPTreeDataSource> CPTreeDataSourceRep,
            BaseRepository<CPTreeFunc> CPTreeFuncRep
            )
        {
            this._CPTreeRep = CPTreeRep;
            this._CPTreeDataSourceRep = CPTreeDataSourceRep;
            this._CPTreeFuncRep = CPTreeFuncRep;

        }

        public CPTree GetTree(string treeCode, bool isLoadFunc,bool isLoadDataSource)
        {
            int nCount = 0;
            if (isLoadFunc)
            {
                nCount++;
            }
            if (isLoadDataSource)
            {
                nCount++;
            }


            Expression<Func<CPTree, dynamic>>[] eagerLoadingProperties = new Expression<Func<CPTree, dynamic>>[nCount];
            int nIndex = 0;
            if (isLoadFunc)
            {
                eagerLoadingProperties[nIndex] = t => t.FuncCol;
                nIndex++;
            }
            if (isLoadDataSource)
            {
                eagerLoadingProperties[nIndex] = t => t.DataSourceCol;
                nIndex++;
            }


            ISpecification<CPTree> specification;
            specification = new ExpressionSpecification<CPTree>(t => t.TreeCode.Equals(treeCode));
            IList<CPTree> col = this._CPTreeRep.GetByCondition(specification, eagerLoadingProperties);
            if (col.Count <= 0)
                return null;
            else
            {
                if (col[0].FuncCol != null)
                {
                    col[0].FuncCol = col[0].FuncCol.OrderBy(c => c.ShowOrder).ToList();
                }
                return col[0];
            }
        }


        #region 获取树真实数据
        private List<CPTreeNode> GetChildData(CPTree tree, int TreeDataSourceId,DataRow parentDR,List<string> chkDefaultSelValue)
        {
            List<CPTreeNode> col = new List<CPTreeNode>();
            List<CPTreeDataSource> SourceCol = tree.DataSourceCol.Where(c => c.Id.Equals(TreeDataSourceId)).ToList();
            SourceCol.ForEach(t => {
                string strSql = t.DataSource;
                CPExpressionHelper.Instance.Add(CPTreeExpression.DataRowKey, parentDR);
                strSql = CPExpressionHelper.Instance.RunCompile(strSql);
                CPExpressionHelper.Instance.Remove(CPTreeExpression.DataRowKey);
                DbHelper _helper = new DbHelper(t.DbIns, CPAppContext.CurDbType());
                DataTable dt = _helper.ExecuteDataSet(strSql).Tables[0];
                if (string.IsNullOrEmpty(t.PKField) == false)
                {
                    if (dt.Columns.Contains(t.PKField) == false)
                    {
                        throw new Exception("数据源SQL中未包括主键字段【" + t.PKField + "】");
                    }
                }
                foreach (DataRow dr in dt.Rows)
                {
                    CPExpressionHelper.Instance.Add(CPTreeExpression.DataRowKey, dr);
                    CPTreeNode node = new CPTreeNode();
                    node.NodeId =  "Node_" + Guid.NewGuid().ToString("N");
                    node.NodeTitle = CPExpressionHelper.Instance.RunCompile(t.NodeTitle); 
                    node.NodeAttrEx = CPExpressionHelper.Instance.RunCompile(t.NodeAttrEx); 
                    node.NodeIcon = t.NodeIcon;
                    node.hasChildren = false;
                    node.TreeDataSourceId = t.Id;
                    if (string.IsNullOrEmpty(t.PKField) == false)
                    {
                        node.DeleteFieldValue = dr[t.PKField].ToString();
                    }
                    else
                    {
                        node.DeleteFieldValue = "";
                    }
                    node.DataRowJSON = "";
                    if (string.IsNullOrEmpty(t.ChkSelFieldName))
                        node.ChkSelFieldName = "";
                    else
                    {
                        node.ChkSelFieldName = t.ChkSelFieldName;
                        if (dt.Columns.Contains(t.ChkSelFieldName) == false)
                        {
                            throw new Exception("数据源SQL中未包括checkbox默认选中字段【" + t.ChkSelFieldName + "】");
                        }
                        node.ChkSelFieldValue = Convert.IsDBNull(dr[t.ChkSelFieldName.Trim()]) ? "" : dr[t.ChkSelFieldName.Trim()].ToString();
                        #region 处理check默认选中
                        if (chkDefaultSelValue.Count > 0)
                        {
                            if (chkDefaultSelValue.Contains(node.ChkSelFieldValue))
                                node.Checked = true;
                            else
                                node.Checked = false;
                        }
                        #endregion
                    }
                    node.items = new List<CPTreeNode>();
                    #region 获取子节点
                    if (t.SourceIsRecursion.Value)
                    {
                        //递归取数据
                        List<CPTreeNode> childCol = this.GetChildData(tree, t.Id, dr, chkDefaultSelValue);
                        if (childCol.Count > 0)
                        {
                            node.hasChildren = true;
                            node.items.AddRange(childCol);
                        }
                    }
                    //取下一级的数据
                    List<CPTreeDataSource> childSourceCol = tree.DataSourceCol.Where(c => c.ParentSourceId.Equals(t.Id)).ToList();
                    childSourceCol.ForEach(child => {
                        List<CPTreeNode> childCol = this.GetChildData(tree, child.Id, dr, chkDefaultSelValue);
                        if (childCol.Count > 0)
                        {
                            node.hasChildren = true;
                            node.items.AddRange(childCol);
                        }
                    });
                    #endregion
                    CPExpressionHelper.Instance.Remove(CPTreeExpression.DataRowKey);
                    col.Add(node);
                }
            });
            return col;
        }

        private int GetChildDataCount(CPTree tree, int TreeDataSourceId, DataRow parentDR)
        {
            int nCount = 0;
            List<CPTreeNode> col = new List<CPTreeNode>();
            List<CPTreeDataSource> SourceCol = tree.DataSourceCol.Where(c => c.Id.Equals(TreeDataSourceId)).ToList();
            SourceCol.ForEach(t => {
                string strSql = t.DataSource;
                CPExpressionHelper.Instance.Add(CPTreeExpression.DataRowKey, parentDR);
                strSql = CPExpressionHelper.Instance.RunCompile(strSql);
                CPExpressionHelper.Instance.Remove(CPTreeExpression.DataRowKey);
                DbHelper _helper = new DbHelper(t.DbIns, CPAppContext.CurDbType());
                DataTable dt = _helper.ExecuteDataSet(strSql).Tables[0];
                if (string.IsNullOrEmpty(t.PKField) == false)
                {
                    if (dt.Columns.Contains(t.PKField) == false)
                    {
                        throw new Exception("数据源SQL中未包括主键字段【" + t.PKField + "】");
                    }
                }
                nCount = dt.Rows.Count;
            });
            return nCount;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeCode"></param>
        /// <param name="TreeDataSourceId"></param>
        /// <param name="parentDataRowJSON">parentDataRowJSON：父节点datarow的JSON,根节点为空，一般是逐级加载数据时用到</param>
        public List<CPTreeNode> GetTreeData(string treeCode,int TreeDataSourceId,string parentDataRowJSON)
        {
            CPTree tree = this.GetTree(treeCode, false, true);
            if (tree == null)
                return   new List<CPTreeNode>();
            if (string.IsNullOrEmpty(tree.ChkDefaultSelValue) == false)
            {
                tree.ChkDefaultSelValue = CPExpressionHelper.Instance.RunCompile(tree.ChkDefaultSelValue);
            }
            List<string> chkDefaultSelValue = null;
            if (string.IsNullOrEmpty(tree.ChkDefaultSelValue))
            {
                chkDefaultSelValue = new List<string>();
            }
            else
            {
                chkDefaultSelValue = tree.ChkDefaultSelValue.Split(',').ToList();
            }
            List<CPTreeNode> col = new List<CPTreeNode>();
            if (tree.DataLoadType == CPTreeEnum.DataLoadTypeEnum.AllLoad)
            {
                #region 全加载,只会调用一次  且不支持局部刷新
                //
                List<CPTreeDataSource> rootSourceCol =  tree.DataSourceCol.Where(c => c.ParentSourceId.Equals(-1)).ToList();
                rootSourceCol.ForEach(t => {
                string strSql = t.DataSource;
                strSql = CPExpressionHelper.Instance.RunCompile(strSql);
                DbHelper _helper = new DbHelper(t.DbIns, CPAppContext.CurDbType());
                DataTable dt = _helper.ExecuteDataSet(strSql).Tables[0];
                if (string.IsNullOrEmpty(t.PKField) == false)
                {
                    if (dt.Columns.Contains(t.PKField) == false)
                    {
                        throw new Exception("数据源SQL中未包括主键字段【" + t.PKField + "】");
                    }
                }
                foreach (DataRow dr in dt.Rows)
                {
                    CPExpressionHelper.Instance.Add(CPTreeExpression.DataRowKey, dr);
                    CPTreeNode node = new CPTreeNode();
                    node.NodeId = "Node_" + Guid.NewGuid().ToString("N");
                    node.NodeTitle = CPExpressionHelper.Instance.RunCompile(t.NodeTitle);
                    node.NodeAttrEx = CPExpressionHelper.Instance.RunCompile(t.NodeAttrEx);
                    node.NodeIcon = t.NodeIcon;
                    node.hasChildren = false;
                    node.TreeDataSourceId = t.Id;
                    node.DataRowJSON = "";
                    if (string.IsNullOrEmpty(t.PKField) == false)
                    {
                        node.DeleteFieldValue = dr[t.PKField].ToString();
                    }
                    else
                    {
                        node.DeleteFieldValue = "";
                    }
                        if (string.IsNullOrEmpty(t.ChkSelFieldName))
                            node.ChkSelFieldName = "";
                        else
                        {
                            node.ChkSelFieldName = t.ChkSelFieldName;
                            if(dt.Columns.Contains(t.ChkSelFieldName)==false)
                            {
                                throw new Exception("数据源SQL中未包括checkbox默认选中字段【" + t.ChkSelFieldName + "】");
                            }
                            node.ChkSelFieldValue =Convert.IsDBNull( dr[t.ChkSelFieldName.Trim()]) ? "" : dr[t.ChkSelFieldName.Trim()].ToString();
                            #region 处理check默认选中
                            if (chkDefaultSelValue.Count>0)
                            {
                                if (chkDefaultSelValue.Contains(node.ChkSelFieldValue))
                                    node.Checked = true;
                                else
                                    node.Checked = false;
                            }
                            #endregion
                        }
                        node.items = new List<CPTreeNode>();
                        #region 获取子节点
                        if (t.SourceIsRecursion.Value)
                        {
                            //递归取数据
                            List<CPTreeNode> childCol = this.GetChildData(tree, t.Id, dr, chkDefaultSelValue);
                            if(childCol.Count >0)
                            {
                                node.hasChildren = true;
                                node.items.AddRange(childCol);
                            }
                        }
                        //取下一级的数据
                        List<CPTreeDataSource> childSourceCol = tree.DataSourceCol.Where(c => c.ParentSourceId.Equals(t.Id)).ToList();
                        childSourceCol.ForEach(child => {
                            List<CPTreeNode> childCol = this.GetChildData(tree, child.Id, dr, chkDefaultSelValue);
                            if (childCol.Count > 0)
                            {
                                node.hasChildren = true;
                                node.items.AddRange(childCol);
                            }
                        });
                        #endregion
                        CPExpressionHelper.Instance.Remove(CPTreeExpression.DataRowKey);
                        col.Add(node);
                    }
                });
                #endregion
            }
            else
            {
                #region 逐级加载 
                List<CPTreeDataSource> rootSourceCol = null;
                if (string.IsNullOrEmpty(parentDataRowJSON))
                {
                    rootSourceCol=  tree.DataSourceCol.Where(c => c.Id.Equals(TreeDataSourceId)).ToList();
                }
                else
                {
                    rootSourceCol = tree.DataSourceCol.Where(c => c.Id.Equals(TreeDataSourceId)).ToList();
                    if(rootSourceCol[0].SourceIsRecursion.Value==false)
                    {
                        rootSourceCol = tree.DataSourceCol.Where(c => c.ParentSourceId.Equals(TreeDataSourceId)).ToList();
                    }
                    else
                    {
                        rootSourceCol.AddRange(
                            tree.DataSourceCol.Where(c => c.ParentSourceId.Equals(TreeDataSourceId)).ToList()
                            );
                    }
                }
                rootSourceCol.ForEach(t => {
                    string strSql = t.DataSource;
                    if (string.IsNullOrEmpty(parentDataRowJSON) == false)
                    {
                        dynamic formData = JsonConvert.DeserializeObject<dynamic>(parentDataRowJSON);
                        DataTable dtTmp = new DataTable();
                        foreach (var key in formData)
                        {
                            Newtonsoft.Json.Linq.JProperty jKey = (Newtonsoft.Json.Linq.JProperty)key;
                            DataColumn dc = new DataColumn();
                            dc.ColumnName = jKey.Name;
                            dtTmp.Columns.Add(dc);
                        }
                        DataRow dr = dtTmp.NewRow();
                        foreach (var key in formData)
                        {
                            Newtonsoft.Json.Linq.JProperty jKey = (Newtonsoft.Json.Linq.JProperty)key;
                            try
                            {
                                dr[jKey.Name] = jKey.Value.ToString();
                            }
                            catch (Exception ex)
                            {
                                dr[jKey.Name] = "";
                            }
                        }
                        dtTmp.Rows.Add(dr);
                        CPExpressionHelper.Instance.Add(CPTreeExpression.DataRowKey, dtTmp.Rows[0]);
                        strSql = CPExpressionHelper.Instance.RunCompile(strSql);
                        CPExpressionHelper.Instance.Remove(CPTreeExpression.DataRowKey);
                    }
                    else
                    {
                        strSql = CPExpressionHelper.Instance.RunCompile(strSql);
                    }
                    DbHelper _helper = new DbHelper(t.DbIns, CPAppContext.CurDbType());
                    DataTable dt = _helper.ExecuteDataSet(strSql).Tables[0];
                    if (string.IsNullOrEmpty(t.PKField) == false)
                    {
                        if (dt.Columns.Contains(t.PKField) == false)
                        {
                            throw new Exception("数据源SQL中未包括主键字段【" + t.PKField + "】");
                        }
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        CPExpressionHelper.Instance.Add(CPTreeExpression.DataRowKey, dr);
                        CPTreeNode node = new CPTreeNode();
                        node.NodeId = "Node_" + Guid.NewGuid().ToString("N");
                        node.NodeTitle = CPExpressionHelper.Instance.RunCompile(t.NodeTitle);
                        node.NodeAttrEx = CPExpressionHelper.Instance.RunCompile(t.NodeAttrEx);
                        node.NodeIcon = t.NodeIcon;
                        node.hasChildren = false;
                        node.TreeDataSourceId = t.Id;
                        node.DataRowJSON = CPUtils.DataRow2Json(dr);
                        if (string.IsNullOrEmpty(t.PKField) == false)
                        {
                            node.DeleteFieldValue = dr[t.PKField].ToString();
                        }
                        else
                        {
                            node.DeleteFieldValue = "";
                        }
                        if (string.IsNullOrEmpty(t.ChkSelFieldName))
                            node.ChkSelFieldName = "";
                        else
                        {
                            node.ChkSelFieldName = t.ChkSelFieldName;
                            if (dt.Columns.Contains(t.ChkSelFieldName) == false)
                            {
                                throw new Exception("数据源SQL中未包括checkbox默认选中字段【" + t.ChkSelFieldName + "】");
                            }
                            node.ChkSelFieldValue = Convert.IsDBNull(dr[t.ChkSelFieldName.Trim()]) ? "" : dr[t.ChkSelFieldName.Trim()].ToString();
                            #region 处理check默认选中
                            if (chkDefaultSelValue.Count > 0)
                            {
                                if (chkDefaultSelValue.Contains(node.ChkSelFieldValue))
                                    node.Checked = true;
                                else
                                    node.Checked = false;
                            }
                            #endregion
                        }
                        node.items = new List<CPTreeNode>();
                        #region 获取子节点
                        if (t.SourceIsRecursion.Value)
                        {
                            //递归取数据
                            int nCount = this.GetChildDataCount(tree, t.Id, dr);
                            if (nCount > 0)
                            {
                                node.hasChildren = true;
                            }
                        }
                        if (node.hasChildren == false)
                        {
                            //看下一级的数据
                            List<CPTreeDataSource> childSourceCol = tree.DataSourceCol.Where(c => c.ParentSourceId.Equals(t.Id)).ToList();
                            childSourceCol.ForEach(child =>
                            {
                                if (node.hasChildren)
                                    return;
                                int nCount = this.GetChildDataCount(tree, child.Id, dr);
                                if (nCount > 0)
                                {
                                    node.hasChildren = true;
                                }
                            });
                        }
                        #endregion
                        CPExpressionHelper.Instance.Remove(CPTreeExpression.DataRowKey);
                        col.Add(node);
                    }
                });
                
          
                #endregion
            }
            return col;
        }
        #endregion


        #region 删除节点数据
        private List<string> GetChildPK(string strSql,string parentId,string dbIns)
        {
            DbHelper _helper = new DbHelper(dbIns, CPAppContext.CurDbType());
            string tempSql = strSql += "='" + parentId + "'";
            DataTable dt = _helper.ExecuteDataSet(strSql).Tables[0];
            List<string> pkCol = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                pkCol.Add(dr[0].ToString());
                //获取子节点
                List<string> tCol = this.GetChildPK(strSql, dr[0].ToString(), dbIns);
                pkCol.AddRange(tCol);
            }
            dt.Dispose();
            return pkCol;
        }
        public bool DeleteTreeData( int TreeDataSourceId,string pkFieldValue,ref string errorMsg)
        {
            CPTreeDataSource ds = this._CPTreeDataSourceRep.Get(TreeDataSourceId);
            if(string.IsNullOrEmpty(ds.MainTableName))
            {
                errorMsg = "数据源未配置数据表名";
                return false;
            }
            if (string.IsNullOrEmpty(ds.PKField))
            {
                errorMsg = "数据源未配置表主键字段";
                return false;
            }
           
            List<string> pkCol = new List<string>();
            pkCol.Add(pkFieldValue);
            DbHelper _helper = new DbHelper(ds.DbIns,CPAppContext.CurDbType());
            if (string.IsNullOrEmpty(ds.ParentIdField)==false)
            {
                //递归删除数据
                string strSql = "SELECT " + ds.PKField + " FROM " + ds.MainTableName + " WHERE " + ds.ParentIdField;
                DataTable dt = _helper.ExecuteDataSet(strSql + "='" + pkFieldValue + "'").Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    pkCol.Add(dr[0].ToString());
                    //获取子节点
                    List<string> tCol = this.GetChildPK(strSql, dr[0].ToString(), ds.DbIns);
                    pkCol.AddRange(tCol);
                }
                dt.Dispose();
            }
           
            string ids = "";
            pkCol.ForEach(t => {
                if (string.IsNullOrEmpty(ids)) ids = "'" + t + "'";
                else ids += ",'" + t + "'";
            });
            string strSql2 = "DELETE FROM " + ds.MainTableName + " WHERE " + ds.PKField + " in (" + ids + ")";
            _helper.ExecuteNonQuery(strSql2);
            return true;
        }
        #endregion

        #region 导入内置按钮
        public bool ImportTreeInnerFunc(int treeId)
        {
            List<CPTreeFunc> col = new List<CPTreeFunc>();
            CPTreeFunc fun0 = new CPTreeFunc();
            fun0.TreeId = treeId;
            fun0.FuncTitle = "全展开";
            fun0.FuncIcon = "icon-unfold";
            fun0.ShowOrder = 20;
            fun0.JSMethod = "CPTreeExpandAllNode()";
            fun0.ShowPosition = CPTreeEnum.ShowPositionEnum.TopAndRight;
            fun0.SourceId = -1;
            fun0.IsUseExpressionShow = false;
            fun0.IsShowByExpression = true;
            col.Add(fun0);
            CPTreeFunc fun1 = new CPTreeFunc();
            fun1.TreeId = treeId;
            fun1.FuncTitle = "全收缩";
            fun1.FuncIcon = "icon-narrow";
            fun1.ShowOrder = 30;
            fun1.JSMethod = "CPTreeCollapseAllNode()";
            fun1.ShowPosition = CPTreeEnum.ShowPositionEnum.TopAndRight;
            fun1.SourceId = -1;
            fun1.IsUseExpressionShow = false;
            fun1.IsShowByExpression = true;
            col.Add(fun1);
            CPTreeFunc fun = new CPTreeFunc();
            fun.TreeId = treeId;
            fun.FuncTitle = "删除";
            fun.FuncIcon = "icon-guanbi1";
            fun.ShowOrder = 40;
            fun.JSMethod = "CPTreeDeleteNode()";
            fun.ShowPosition = CPTreeEnum.ShowPositionEnum.TopAndRight;
            fun.SourceId = -1;
            fun.IsUseExpressionShow = false;
            fun.IsShowByExpression = true;
            col.Add(fun);
            this._CPTreeFuncRep.Add(col);
            return true;
        }
        #endregion

        #region 修改实际数据的父节点ID
        public bool UpdateTreeDataParent(int TreeDataSourceId, string sourcePKValue,string targetPKValue, ref string errorMsg)
        {
            CPTreeDataSource ds = this._CPTreeDataSourceRep.Get(TreeDataSourceId);
            if (string.IsNullOrEmpty(ds.MainTableName))
            {
                errorMsg = "数据源未配置数据表名";
                return false;
            }
            if (string.IsNullOrEmpty(ds.PKField))
            {
                errorMsg = "数据源未配置表主键字段";
                return false;
            }
            if (string.IsNullOrEmpty(ds.ParentIdField))
            {
                errorMsg = "数据源未配置存储父级ID的字段名";
                return false;
            }
            DbHelper _helper = new DbHelper(ds.DbIns, CPAppContext.CurDbType());
            string strSql2 = "UPDATE   " + ds.MainTableName + " SET " +ds.ParentIdField + "='" + targetPKValue+ "'  WHERE " + ds.PKField + " in (" + sourcePKValue + ")";
            _helper.ExecuteNonQuery(strSql2);
            return true;
        }
        #endregion



        #region 配置导出，同步相关
        public string GetTreeConfigXml(List<int> treeIdCol)
        {

            if (treeIdCol.Count <= 0)
                return "";
            DataSet ds = this._CPTreeRep.GetConfig(treeIdCol);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ds.WriteXml(ms);
            byte[] bData = ms.GetBuffer();
            ms.Close();
            return System.Text.UTF8Encoding.UTF8.GetString(bData);
        }
        /// <summary>
        /// 从xml创建新的列表配置实例 
        /// </summary>
        /// <param name="funcId"></param>
        /// <param name="sysId"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public bool InitTreeFromConfigXml(int targetSysId, byte[] bData)
        {
            DataSet ds = new DataSet();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(bData, 0, bData.Length);
            ms.Position = 0;
            ds.ReadXml(ms);
            ms.Close();
            bool b = true;
            b = _CPTreeRep.SyncConfigFromDataSet(targetSysId, ds, true);
            return b;
        }
        public bool SyncTreeFromConfigXml(int targetSysId, byte[] bData)
        {
            DataSet ds = new DataSet();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(bData, 0, bData.Length);
            ms.Position = 0;
            ds.ReadXml(ms);
            ms.Close();
            bool b = true;
            b = _CPTreeRep.SyncConfigFromDataSet(targetSysId, ds, false);
            return b;
        }
        #endregion     
    }
}
