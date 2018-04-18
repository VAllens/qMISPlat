using CPFrameWork.Global;
using CPFrameWork.Utility;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CPFrameWork.UIInterface.Grid
{
   public class CPGridEngine
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
            services.TryAddTransient<BaseCPGridRep, CPGridRep>();
            services.TryAddTransient<BaseRepository<CPGridColumn>, CPGridColumnRep>();
            services.TryAddTransient<BaseRepository<CPGridFunc>, CPGridFuncRep>();
            services.TryAddTransient<CPGridEngine, CPGridEngine>();
        }
        public static CPGridEngine Instance(int curUserId)
        {
            CPGridEngine iObj = CPAppContext.GetService<CPGridEngine>();
            iObj.CurUserId = curUserId;
            return iObj;
        }


        #endregion
        public int CurUserId { get; set; }
        private BaseCPGridRep _CPGridRep;
        private BaseRepository<CPGridColumn> _CPGridColumnRep;
        private BaseRepository<CPGridFunc> _CPGridFuncRep;
     
        public CPGridEngine(
            BaseCPGridRep CPGridRep,
            BaseRepository<CPGridColumn> CPGridColumnRep,
            BaseRepository<CPGridFunc> CPGridFuncRep
            )
        {
            this._CPGridRep = CPGridRep;
            this._CPGridColumnRep = CPGridColumnRep;
            this._CPGridFuncRep = CPGridFuncRep;
            
        }
        #region 获取列表配置信息
        /// <summary>
        /// 获取列表配置信息
        /// </summary>
        /// <param name="gridCode"></param>
        /// <returns></returns>
        public CPGrid GetGrid(string gridCode, bool isLoadColumnInfo, bool isLoadFuncInfo)
        {
            int nCount = 0;
            if (isLoadColumnInfo)
            {
                nCount++;
            }
            if (isLoadFuncInfo)
            {
                nCount++;
            }
           
            Expression<Func<CPGrid, dynamic>>[] eagerLoadingProperties = new Expression<Func<CPGrid, dynamic>>[nCount];
            int nIndex = 0;
            if (isLoadColumnInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.ColumnCol;
                nIndex++;
            }
            if (isLoadFuncInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.FuncCol;
                nIndex++;
            }
          
            ISpecification<CPGrid> specification;
            specification = new ExpressionSpecification<CPGrid>(t => t.GridCode.Equals(gridCode));
            IList<CPGrid> col = this._CPGridRep.GetByCondition(specification, eagerLoadingProperties);
            if (col.Count <= 0)
                return null;
            else
            {
                if (col[0].ColumnCol != null)
                {
                    col[0].ColumnCol.ForEach(t => { t.FormatInitValue(); });
                    col[0].ColumnCol = col[0].ColumnCol.OrderBy(c => c.ShowOrder).ToList();
                }
                if (col[0].FuncCol != null)
                {
                    col[0].FuncCol.ForEach(t => { t.FormatInitValue(); });
                    col[0].FuncCol = col[0].FuncCol.OrderBy(c => c.FuncOrder).ToList();
                }
                return col[0];
            }
        }


        public CPGrid GetGrid(int gridId, bool isLoadColumnInfo, bool isLoadFuncInfo)
        {
            int nCount = 0;
            if (isLoadColumnInfo)
            {
                nCount++;
            }
            if (isLoadFuncInfo)
            {
                nCount++;
            }

            Expression<Func<CPGrid, dynamic>>[] eagerLoadingProperties = new Expression<Func<CPGrid, dynamic>>[nCount];
            int nIndex = 0;
            if (isLoadColumnInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.ColumnCol;
                nIndex++;
            }
            if (isLoadFuncInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.FuncCol;
                nIndex++;
            }
             
            return  this._CPGridRep.Get(gridId, eagerLoadingProperties);
            
        }
        #endregion

        #region 读取真实数据
        /// <summary>
        /// 统计列表列的统计值
        /// </summary>
        /// <param name="gridObj"></param>
        /// <returns></returns>
        public bool StatisticsColumnSum(CPGrid gridObj, string otherCondition)
        {
            gridObj.ColumnCol.ForEach(t => {
                if(t.IsShowSum.Value)
                {
                    t.TempSumValue = this._CPGridRep.StatisticsColumnSum(gridObj, t, otherCondition);
                    if (string.IsNullOrEmpty(t.NumberFormat) == false)
                    {
                        try
                        {
                            t.TempSumValue = Convert.ToDecimal(t.TempSumValue).ToString(t.NumberFormat);
                        }
                        catch (Exception ex) { ex.ToString(); }
                    }
                }
            });
            return true;
        }
        public   DataTable ReadData(CPGrid gridObj, int currentPage,
           int pageSize, string otherCondition, string orderBy, out int recordSize)
        {
            if (string.IsNullOrEmpty(orderBy))
                orderBy = gridObj.DataOrder;
            return this._CPGridRep.ReadData(gridObj, currentPage, pageSize, otherCondition, orderBy, out recordSize);
        }
        public DataTable ReadDataAndFormat(CPGrid gridObj, int currentPage,
          int pageSize, string otherCondition, string orderBy, out int recordSize)
        { 
            DataTable dt = this.ReadData(gridObj, currentPage, pageSize, otherCondition, orderBy, out recordSize);
            #region 重新生成新的DT
            DataTable dtNew = new DataTable();
            DataColumn dcItem0 = new DataColumn();
            dcItem0.ColumnName = "ColumnCPGridPK";
            dcItem0.DataType = Type.GetType("System.String");
            dtNew.Columns.Add(dcItem0);
            foreach (CPGridColumn item in gridObj.ColumnCol)
            {
                if (item.IsShow == false)
                    continue;
                DataColumn dcItem = new DataColumn();
                dcItem.ColumnName = "Column" + item.Id.ToString();
                dcItem.DataType = Type.GetType("System.String");
                dtNew.Columns.Add(dcItem);
                if (item.ColumnType == CPGridEnum.ColumnTypeEnum.TextHref || item.ColumnType == CPGridEnum.ColumnTypeEnum.PicHref)
                {
                    DataColumn dcItem2 = new DataColumn();
                    dcItem2.ColumnName = "ColumnTargetContent" + item.Id.ToString();
                    dcItem2.DataType = Type.GetType("System.String");
                    dtNew.Columns.Add(dcItem2);
                }
                if(item.ColumnType == CPGridEnum.ColumnTypeEnum.TextBoxEditor
                    || item.ColumnType == CPGridEnum.ColumnTypeEnum.DropDownListEditor
                    || item.ColumnType == CPGridEnum.ColumnTypeEnum.TimeSelectEditor
                    )
                {
                    if(string.IsNullOrEmpty(item.EventMethod)==false && string.IsNullOrEmpty(item.EventName)==false)
                    {
                        DataColumn dcItem3 = new DataColumn();
                        dcItem3.ColumnName = "ColumnEventMethod" + item.Id.ToString();
                        dcItem3.DataType = Type.GetType("System.String");
                        dtNew.Columns.Add(dcItem3);

                        DataColumn dcItem4 = new DataColumn();
                        dcItem4.ColumnName = "ColumnEventName" + item.Id.ToString();
                        dcItem4.DataType = Type.GetType("System.String");
                        dtNew.Columns.Add(dcItem4);
                    }
                }
            }
            if (gridObj.IsGroup.Value && string.IsNullOrEmpty(gridObj.GroupField)==false)
            {
                //启用了分组
                string[] gArray = gridObj.GroupField.Split(',');
                for(int i = 0; i < gArray.Length; i++)
                {
                    if (string.IsNullOrEmpty(gArray[i]))
                        continue;
                    DataColumn Group = new DataColumn();
                    Group.ColumnName = gArray[i].Trim() + "_CPGroup";
                    Group.DataType = Type.GetType("System.String");
                    dtNew.Columns.Add(Group);
                }              
            }
            #endregion
            int nIndex = (currentPage-1) *pageSize +1;
            foreach (DataRow dr in dt.Rows)
            {
                DataRow newRow = dtNew.NewRow();
                string dataPK = "";
                #region 主键值
                if (string.IsNullOrEmpty(gridObj.PKFieldName) == false)
                {
                    string[] pkArray = gridObj.PKFieldName.Split(',');
                    string sValue = "";
                    for (int i = 0; i < pkArray.Length; i++)
                    {
                        object obj = dr[pkArray[i].Trim()];
                        if (Convert.IsDBNull(obj) || obj == null)
                        {
                            if (i == 0) sValue = "";
                            else sValue += ",";
                        }
                        else
                        {
                            if (i == 0) sValue = obj.ToString();
                            else sValue += "," + obj.ToString();
                        }
                    }
                    dataPK = sValue;
                }
                else
                {
                    dataPK = "";
                }
                #endregion
                newRow["ColumnCPGridPK"] = dataPK;

                #region 循环列
                foreach (CPGridColumn item in gridObj.ColumnCol)
                {
                    if (item.IsShow == false)
                        continue;
                    object objValue = null;
                    #region 处理列值
                    if (item.ColumnType == CPGridEnum.ColumnTypeEnum.Number)
                    {//序号
                        objValue = nIndex;
                    }
                    else if (item.ColumnType == CPGridEnum.ColumnTypeEnum.CheckBox)
                    {
                        //复选
                        objValue = dataPK;
                    }
                    else if (item.ColumnType == CPGridEnum.ColumnTypeEnum.Radio)
                    {
                        //单选
                        objValue = dataPK;
                    }
                    else if (item.ColumnType == CPGridEnum.ColumnTypeEnum.Normal)
                    {
                        //普通
                        if (string.IsNullOrEmpty(item.FieldName)) objValue = "";
                        else
                        {
                            objValue = dr[item.FieldName.Trim()];
                            if (string.IsNullOrEmpty(item.NumberFormat) == false)
                            {
                                try
                                {
                                    objValue = Convert.ToDecimal(objValue).ToString(item.NumberFormat);
                                }
                                catch(Exception ex) {ex.ToString(); }
                            } 
                               
                           
                        }
                    }
                    else if (item.ColumnType == CPGridEnum.ColumnTypeEnum.DateTime)
                    {
                        #region 日期
                        if (string.IsNullOrEmpty(item.FieldName)) objValue = "";
                        else
                        {
                            objValue = dr[item.FieldName.Trim()];
                            try
                            {

                                DateTime dtTime = Convert.ToDateTime(objValue);
                                objValue = dtTime.ToString(item.TimeFormat);

                            }
                            catch (Exception et) { et.ToString(); }
                        }
                        #endregion

                    }
                    else if (item.ColumnType == CPGridEnum.ColumnTypeEnum.ListInnerDel)
                    {
                        #region 内置删除列
                        if (string.IsNullOrEmpty(item.FieldName)) objValue = "";
                        else
                        {
                            objValue = dr[item.FieldName.Trim()];

                        }
                        #endregion

                    }
                    else if (item.ColumnType == CPGridEnum.ColumnTypeEnum.Template)
                    {
                        #region 模板列
                        if (string.IsNullOrEmpty(item.TemplateContent))
                        {
                            if (string.IsNullOrEmpty(item.FieldName)) objValue = "";
                            else
                            {
                                objValue = dr[item.FieldName.Trim()];

                            }
                        }
                        else
                        {
                            string sTemp = item.TemplateContent;
                            CPExpressionHelper.Instance.Add(CPGridExpression.DataRowKey, dr);
                            sTemp = CPExpressionHelper.Instance.RunCompile(sTemp);
                            CPExpressionHelper.Instance.Remove(CPGridExpression.DataRowKey);
                            objValue = sTemp;
                        }
                        #endregion

                    }
                    else if (item.ColumnType == CPGridEnum.ColumnTypeEnum.PicHref)
                    {
                        #region 图片超链接列
                        string sTemp = item.TargetContent;
                        CPExpressionHelper.Instance.Add(CPGridExpression.DataRowKey, dr);
                        sTemp = CPExpressionHelper.Instance.RunCompile(sTemp);
                        CPExpressionHelper.Instance.Remove(CPGridExpression.DataRowKey);
                        newRow["ColumnTargetContent" + item.Id] = sTemp;
                        if (string.IsNullOrEmpty(item.ColumnIconOrText))
                        {
                            objValue = Convert.IsDBNull(dr[item.FieldName]) ? "" : dr[item.FieldName].ToString();
                        }
                        else
                        {
                            objValue = item.ColumnIconOrText;
                        }
                        #endregion
                    }
                    else if (item.ColumnType == CPGridEnum.ColumnTypeEnum.TextHref)
                    {
                        #region 文字超链接列
                        string sTemp = item.TargetContent;
                        CPExpressionHelper.Instance.Add(CPGridExpression.DataRowKey, dr);
                        sTemp = CPExpressionHelper.Instance.RunCompile(sTemp);
                        CPExpressionHelper.Instance.Remove(CPGridExpression.DataRowKey);
                        newRow["ColumnTargetContent" + item.Id] = sTemp.Replace("\"","'");
                        if (string.IsNullOrEmpty(item.ColumnIconOrText))
                        {
                            objValue = Convert.IsDBNull(dr[item.FieldName]) ? "" : dr[item.FieldName].ToString();
                        }
                        else
                        {
                            objValue = item.ColumnIconOrText;
                        }
                        #endregion
                    }
                    else if (item.ColumnType == CPGridEnum.ColumnTypeEnum.TextBoxEditor
                        || item.ColumnType == CPGridEnum.ColumnTypeEnum.DropDownListEditor
                        )
                    {
                        #region 文本框编辑列，下拉 列表编辑列，时间编辑列
                        if (string.IsNullOrEmpty(item.FieldName)) objValue = "";
                        else
                        {
                            if (dt.Columns[item.FieldName].DataType == Type.GetType("System.Boolean"))
                            {
                                objValue = dr[item.FieldName.Trim()].ToString().ToLower();
                            }
                            else
                            {
                                objValue = dr[item.FieldName.Trim()];
                                if (string.IsNullOrEmpty(item.NumberFormat) == false)
                                {
                                    try
                                    {
                                        objValue = Convert.ToDecimal(objValue).ToString(item.NumberFormat);
                                    }
                                    catch (Exception ex) { ex.ToString(); }
                                }
                            }


                        }
                        if (string.IsNullOrEmpty(item.EventMethod) == false && string.IsNullOrEmpty(item.EventName) == false)
                        {
                            string EventMethod = item.EventMethod;
                            CPExpressionHelper.Instance.Add(CPGridExpression.DataRowKey, dr);
                            EventMethod = CPExpressionHelper.Instance.RunCompile(EventMethod);
                            CPExpressionHelper.Instance.Remove(CPGridExpression.DataRowKey);
                            newRow["ColumnEventMethod" + item.Id] = EventMethod.Replace("\"", "'");
                            newRow["ColumnEventName" + item.Id] = item.EventName;
                        }
                        #endregion

                    }
                    else if (
                       item.ColumnType == CPGridEnum.ColumnTypeEnum.TimeSelectEditor
                        )
                    {
                        #region 时间编辑列
                        if (string.IsNullOrEmpty(item.FieldName)) objValue = "";
                        else
                        {
                            objValue = dr[item.FieldName.Trim()];
                            try
                            {

                                DateTime dtTime = Convert.ToDateTime(objValue);
                                objValue = dtTime.ToString(item.TimeFormat);

                            }
                            catch (Exception et) { et.ToString(); }
                        }
                        if (string.IsNullOrEmpty(item.EventMethod) == false && string.IsNullOrEmpty(item.EventName) == false)
                        {
                            string EventMethod = item.EventMethod;
                            CPExpressionHelper.Instance.Add(CPGridExpression.DataRowKey, dr);
                            EventMethod = CPExpressionHelper.Instance.RunCompile(EventMethod);
                            CPExpressionHelper.Instance.Remove(CPGridExpression.DataRowKey);
                            newRow["ColumnEventMethod" + item.Id] = EventMethod.Replace("\"", "'");
                            newRow["ColumnEventName" + item.Id] = item.EventName;
                        }
                        #endregion

                    }
                    #endregion
                    if (Convert.IsDBNull(objValue) || objValue == null)
                    {
                        newRow["Column" + item.Id.ToString()] = "";
                    }
                    else
                    {
                        newRow["Column" + item.Id.ToString()] = objValue.ToString().Trim();
                    }
                }
                #endregion
                if (gridObj.IsGroup.Value && string.IsNullOrEmpty(gridObj.GroupField) == false)
                {
                    //启用了分组
                    string[] gArray = gridObj.GroupField.Split(',');
                    for (int i = 0; i < gArray.Length; i++)
                    {
                        if (string.IsNullOrEmpty(gArray[i]))
                            continue;
                        newRow[gArray[i].Trim() + "_CPGroup"] = dr[gArray[i].Trim()].ToString(); ;
                    }
                }

                nIndex++;
                dtNew.Rows.Add(newRow);
            }
            return dtNew;
        }

        /// <summary>
        /// 如果列表有编辑列，获取编辑列类型是下拉列表的数据源
        /// </summary>
        /// <param name="gridObj"></param>
        /// <returns></returns>
        public Dictionary<string,string> ReadListDataAndFormat(CPGrid gridObj)
        {
            Dictionary<string, string> col = new Dictionary<string, string>();
            gridObj.ColumnCol.ForEach(t => {
                if(t.ColumnType == CPGridEnum.ColumnTypeEnum.DropDownListEditor)
                {
                    if(string.IsNullOrEmpty(t.FieldEnumDataIns))
                    {
                        throw new Exception("字段" + t.FieldName + "配置成下拉列表编辑列，但未配置下拉列表数据源数据库实例");
                    }
                    if (string.IsNullOrEmpty(t.FieldEnumDataSource))
                    {
                        throw new Exception("字段" + t.FieldName + "配置成下拉列表编辑列，但未配置下拉列表数据源");
                    }
                    DbHelper dbHelper = new DbHelper(t.FieldEnumDataIns, CPAppContext.CurDbType());
                    string strSql = t.FieldEnumDataSource;
                    try
                    {
                       
                        strSql = CPExpressionHelper.Instance.RunCompile(strSql);
                        DataTable dt = dbHelper.ExecuteDataTable(strSql);
                        DataTable dtNew = new DataTable();
                        dtNew.Columns.Add(new DataColumn() { ColumnName = "textEx", DataType = typeof(string) });


                        dtNew.Columns.Add(new DataColumn() { ColumnName = "valueEx", DataType = typeof(string) });
                        foreach (DataRow dr in dt.Rows)
                        {
                            DataRow newDR = dtNew.NewRow();
                            newDR["textEx"] = dr[0];
                            if (dt.Columns[1].DataType == Type.GetType("System.Boolean"))
                            {
                                newDR["valueEx"] = dr[1].ToString().Trim().ToLower();
                            }
                            else
                            {
                                newDR["valueEx"] = dr[1];
                            }
                            dtNew.Rows.Add(newDR);
                        }
                        col.Add("Column" + t.Id.ToString(), CPUtils.DataTable2Json(dtNew));
                    }
                    catch(Exception ex)
                    {
                        throw new Exception("执行sql语句:" + strSql + "出错，详细信息为：" + ex.Message);
                    }
                }
            });
            return col;
        }
        #endregion

        #region 同步列信息
        public bool SyncFieldInfo(int gridId)
        {
            CPGrid grid = this.GetGrid(gridId, true, false);
            int recordSize = 0;
            DataTable dt = this.ReadData(grid, 1, 10, "1=2", "", out recordSize);
            //记录需要删除的字段
            List<CPGridColumn> deleteColumns = new List<CPGridColumn>();
           List<CPGridColumn> addColumns = new List<CPGridColumn>();
            int nIndex = 10;
            if (grid.ColumnCol.Count > 0)
            {
                grid.ColumnCol = grid.ColumnCol.OrderBy(t => t.ShowOrder.Value).ToList();
                nIndex = grid.ColumnCol[grid.ColumnCol.Count - 1].ShowOrder.Value + 10;
            }
             
            foreach (DataColumn dc in dt.Columns)
            {
                if (dc.ColumnName.Equals("ROWSTAT", StringComparison.CurrentCultureIgnoreCase))
                    continue;
                List<CPGridColumn> col = grid.ColumnCol.Where(c => c.FieldName.Equals(dc.ColumnName)).ToList();
                if (col.Count <=0)
                {
                    #region 创建新的列

                    CPGridColumn column = new CPGridColumn();
                    column.GridId = grid.Id;
                    column.ColumnTitle = dc.ColumnName;
                    column.FieldName = dc.ColumnName;
                    if (dc.DataType.Equals(Type.GetType("System.DateTime")))
                    {
                        column.ColumnType = CPGridEnum.ColumnTypeEnum.DateTime;
                    }
                    
                    else
                    {
                        column.ColumnType = CPGridEnum.ColumnTypeEnum.Normal;
                    }
                    column.IsSearchShow = true;
                    column.SearchShowType = CPGridEnum.SearchShowTypeEnum.Textbox;
                    column.SearchShowOrder = nIndex;
                    column.IsShow = true;
                    column.ShowOrder = nIndex;
                    column.ShowPosition = CPGridEnum.ShowPositionEnum.Left;
                    column.ShowWidth = 10; 
                    column.MaxString = 0; 
                    column.IsCanOrder = true;
                    column.IsMergeRow = false;                    
                    column.TimeFormat = "yyyy-MM-dd";                    
                    column.TargetContent = ""; 
                    column.OpenWinWidth = 1000;
                    column.OpenWinHeight = 700;
                    column.IsCanExport = true;
                    column.IsShowSum = false;
                    column.TargetType = CPGridEnum.TargetTypeEnum.TopOpenNewModelAndRefresh;
                    column.ShowMethod = CPGridEnum.ShowMethodEnum.Contains;
                    column.IsUseExpressionShow = false;
                    column.IsShowByExpression = true;
                    #endregion
                    addColumns.Add(column);
                    nIndex = nIndex + 10;
                }
            }
            //记录要删除的列

            foreach (CPGridColumn column in grid.ColumnCol)
            {
                if (dt.Columns.Contains(column.FieldName) == false && column.ColumnType != CPGridEnum.ColumnTypeEnum.Number)
                {
                    deleteColumns.Add(column);
                }
            }
            bool b = true;
            //由于ef core批量写入数据时，采用了批处理方法，但这种方式会导致写入顺序错，暂时没有找到什么方法，所以改成一条条写。
            this._CPGridColumnRep.AddOneByOne(addColumns);
            this._CPGridColumnRep.Delete(deleteColumns);
            return b;
        }
        #endregion


        #region 导入列表内置功能
        public bool ImportInnerFunc(int gridId)
        {
            List<CPGridFunc> col = new List<CPGridFunc>();
            #region 新增
            CPGridFunc f1 = new CPGridFunc();
            f1.GridId = gridId;
            f1.FuncOrder = 10;
            f1.FuncIcon = "icon-add";
            f1.FuncTitle = "新增";
            f1.FuncType = CPGridEnum.FuncTypeEnum.TopOpenNewModelAndRefresh;
            f1.OpenWinWidth = 1024;
            f1.OpenWinHeight = 760;
            f1.FuncOpenWinTitle = "新增";
            f1.IsUseExpressionShow = false;
            f1.ShowMethod = CPGridEnum.ShowMethodEnum.Contains;
            col.Add(f1);
            #endregion

            #region 修改
            CPGridFunc f2 = new CPGridFunc();
            f2.GridId = gridId;
            f2.FuncOrder = 20;
            f2.FuncIcon = "icon-fankui";
            f2.FuncTitle = "修改";
            f2.FuncType = CPGridEnum.FuncTypeEnum.Save;
            f2.OpenWinWidth = 1024;
            f2.OpenWinHeight = 760;
            f2.FuncOpenWinTitle = "修改";
            f2.IsUseExpressionShow = false;
            f2.ShowMethod = CPGridEnum.ShowMethodEnum.Contains;
            col.Add(f2);
            #endregion

            #region 删除
            CPGridFunc f3 = new CPGridFunc();
            f3.GridId = gridId;
            f3.FuncOrder = 30;
            f3.FuncTitle = "删除";
            f3.FuncIcon = "icon-guanbi1";
            f3.FuncType = CPGridEnum.FuncTypeEnum.Delete;
            f3.OpenWinWidth = 1024;
            f3.OpenWinHeight = 760;
            f3.FuncOpenWinTitle = "删除";
            f3.IsUseExpressionShow = false;
            f3.ShowMethod = CPGridEnum.ShowMethodEnum.Contains;
            col.Add(f3);
            #endregion

            #region 导出Excel
            CPGridFunc f4 = new CPGridFunc();
            f4.GridId = gridId;
            f4.FuncOrder = 40;
            f4.FuncTitle = "导出Excel";
            f4.FuncIcon = "icon-activity";
            f4.FuncType = CPGridEnum.FuncTypeEnum.ExportExcel;
            f4.OpenWinWidth = 1024;
            f4.OpenWinHeight = 760;
            f4.FuncOpenWinTitle = "导出Excel";
            f4.IsUseExpressionShow = false;
            f4.ShowMethod = CPGridEnum.ShowMethodEnum.Contains;
            col.Add(f4);
            #endregion
            this._CPGridFuncRep.Add(col);
            return true;
        }
        #endregion



        #region 配置导出，同步相关
        public string GetGridConfigXml(List<int> gridIdCol)
        {

            if (gridIdCol.Count <= 0)
                return "";
            DataSet ds = this._CPGridRep.GetConfig(gridIdCol);
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
        public bool InitGridFromConfigXml( int targetSysId, byte[] bData)
        {
            DataSet ds = new DataSet();
            System.IO.MemoryStream ms = new System.IO.MemoryStream(); 
            ms.Write(bData, 0, bData.Length);
            ms.Position = 0;
            ds.ReadXml(ms);
            ms.Close();
            bool b = true;
            b = _CPGridRep.SyncConfigFromDataSet(targetSysId, ds, true);
            return b;
        }
        public bool SyncGridFromConfigXml(int targetSysId, byte[] bData)
        {
            DataSet ds = new DataSet();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(bData, 0, bData.Length);
            ms.Position = 0;
            ds.ReadXml(ms);
            ms.Close();
            bool b = true;
            b = _CPGridRep.SyncConfigFromDataSet(targetSysId, ds, false);
            return b;
        }
        #endregion      
    }
}
