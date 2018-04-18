using CPFrameWork.Global;
using CPFrameWork.UIInterface.Grid;
using CPFrameWork.Utility;
using CPFrameWork.Utility.DbOper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace CPFrameWork.UIInterface.Grid
{
    public class GridEngineController : CPWebApiBaseController
    {

        #region 获取配置信息
        public class GetGridInfoReturn : CPWebApiBaseReturnEntity
        {
            public CPGridClient Grid { get; set; }
           
            /// <summary>
            /// 编辑数据下拉列表数据源
            /// </summary>
            public string ListDataJson { get; set; }
        }
        public class CPGridClient: CPGrid
        {
            /// <summary>
            /// 多表头集合
            /// </summary>
            public List<GetGridInfoHeaderGroup> HeaderGroup { get; set; }
        }
        public class GetGridInfoHeaderGroup
        {
            /// <summary>
            /// 标题
            /// </summary>
            public string GroupTitle { get; set; }
            /// <summary>
            /// 列集合ID
            /// </summary>
            public List<int> ChildColumnId { get; set; }
            public List<GetGridInfoHeaderGroup> ChildGroupCol { get; set; }
        }
        [HttpGet]
        public GetGridInfoReturn GetGridInfo(string  GridCode, string OtherCondition, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            GridCode = CPAppContext.FormatSqlPara(GridCode);
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            if (OtherCondition != null)
            {
                OtherCondition = System.Web.HttpUtility.UrlDecode(OtherCondition);
                OtherCondition = OtherCondition.Replace("@", "%");
            }
            GetGridInfoReturn re = new GetGridInfoReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                CPGrid Grid = CPGridEngine.Instance(CurUserId).GetGrid(GridCode, true, true);

                #region 处理列表列有统计值时，计算统计值结果
                CPGridEngine.Instance(CurUserId).StatisticsColumnSum(Grid, OtherCondition);
                #endregion

                #region 处理列是否显示
                Grid.ColumnCol.ForEach(t =>
                {
                    if (t.IsUseExpressionShow.Value)
                    {
                        string leftValue = CPExpressionHelper.Instance.RunCompile(t.LeftExpression);
                        string rightValue = CPExpressionHelper.Instance.RunCompile(t.RightExpression);
                        bool bResult = false;
                        if (t.ShowMethod == CPGridEnum.ShowMethodEnum.EqualTo)
                        {
                            if (leftValue.Equals(rightValue, StringComparison.CurrentCultureIgnoreCase))
                                bResult = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    bResult = false;
                                else
                                    bResult = true;
                            }
                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.NotEqualTo)
                        {
                            if (leftValue.Equals(rightValue, StringComparison.CurrentCultureIgnoreCase) == false)
                                bResult = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    bResult = false;
                                else
                                    bResult = true;
                            }
                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.GreaterThanOrEqualTo)
                        {
                            double dLeft = 0;
                            double dRight = 0;
                            try
                            {
                                dLeft = Convert.ToDouble(leftValue);
                            }
                            catch (Exception ex) { }
                            try
                            {
                                dRight = Convert.ToDouble(rightValue);
                            }
                            catch (Exception ex) { }
                            if (dLeft >= dRight)
                                bResult = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    bResult = false;
                                else
                                    bResult = true;
                            }
                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.GreaterThan)
                        {
                            double dLeft = 0;
                            double dRight = 0;
                            try
                            {
                                dLeft = Convert.ToDouble(leftValue);
                            }
                            catch (Exception ex) { }
                            try
                            {
                                dRight = Convert.ToDouble(rightValue);
                            }
                            catch (Exception ex) { }
                            if (dLeft > dRight)
                                bResult = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    bResult = false;
                                else
                                    bResult = true;
                            }
                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.LessThanOrEqualTo)
                        {
                            double dLeft = 0;
                            double dRight = 0;
                            try
                            {
                                dLeft = Convert.ToDouble(leftValue);
                            }
                            catch (Exception ex) { }
                            try
                            {
                                dRight = Convert.ToDouble(rightValue);
                            }
                            catch (Exception ex) { }
                            if (dLeft <= dRight)
                                bResult = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    bResult = false;
                                else
                                    bResult = true;
                            }
                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.LessThan)
                        {
                            double dLeft = 0;
                            double dRight = 0;
                            try
                            {
                                dLeft = Convert.ToDouble(leftValue);
                            }
                            catch (Exception ex) { }
                            try
                            {
                                dRight = Convert.ToDouble(rightValue);
                            }
                            catch (Exception ex) { }
                            if (dLeft < dRight)
                                bResult = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    bResult = false;
                                else
                                    bResult = true;
                            }

                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.Contains)
                        {
                            if (leftValue.IndexOf(rightValue, StringComparison.CurrentCultureIgnoreCase) != -1)
                                bResult = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    bResult = false;
                                else
                                    bResult = true;
                            }
                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.DoesNotContain)
                        {
                            if (leftValue.IndexOf(rightValue, StringComparison.CurrentCultureIgnoreCase) == -1)
                                bResult = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    bResult = false;
                                else
                                    bResult = true;
                            }
                        }
                        if (bResult == false)
                            t.IsShow = false;
                    }
                });
                #endregion
                AutoMapper.Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<CPGrid, CPGridClient>();
                });
                re.Grid = AutoMapper.Mapper.Map<CPGridClient>(Grid);

                #region 处理按钮
                List<CPGridFunc> tCol = new List<CPGridFunc>();
                re.Grid.FuncCol.ForEach(t =>
                {
                    bool isShow = true;
                    if (t.IsUseExpressionShow.Value)
                    {
                        string leftValue = CPExpressionHelper.Instance.RunCompile(t.LeftExpression);
                        string rightValue = CPExpressionHelper.Instance.RunCompile(t.RightExpression);

                        if (t.ShowMethod == CPGridEnum.ShowMethodEnum.EqualTo)
                        {
                            if (leftValue.Equals(rightValue, StringComparison.CurrentCultureIgnoreCase))
                                isShow = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    isShow = false;
                                else
                                    isShow = true;
                            }
                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.NotEqualTo)
                        {
                            if (leftValue.Equals(rightValue, StringComparison.CurrentCultureIgnoreCase) == false)
                                isShow = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    isShow = false;
                                else
                                    isShow = true;
                            }
                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.GreaterThanOrEqualTo)
                        {
                            double dLeft = 0;
                            double dRight = 0;
                            try
                            {
                                dLeft = Convert.ToDouble(leftValue);
                            }
                            catch (Exception ex) { }
                            try
                            {
                                dRight = Convert.ToDouble(rightValue);
                            }
                            catch (Exception ex) { }
                            if (dLeft >= dRight)
                                isShow = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    isShow = false;
                                else
                                    isShow = true;
                            }
                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.GreaterThan)
                        {
                            double dLeft = 0;
                            double dRight = 0;
                            try
                            {
                                dLeft = Convert.ToDouble(leftValue);
                            }
                            catch (Exception ex) { }
                            try
                            {
                                dRight = Convert.ToDouble(rightValue);
                            }
                            catch (Exception ex) { }
                            if (dLeft > dRight)
                                isShow = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    isShow = false;
                                else
                                    isShow = true;
                            }
                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.LessThanOrEqualTo)
                        {
                            double dLeft = 0;
                            double dRight = 0;
                            try
                            {
                                dLeft = Convert.ToDouble(leftValue);
                            }
                            catch (Exception ex) { }
                            try
                            {
                                dRight = Convert.ToDouble(rightValue);
                            }
                            catch (Exception ex) { }
                            if (dLeft <= dRight)
                                isShow = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    isShow = false;
                                else
                                    isShow = true;
                            }
                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.LessThan)
                        {
                            double dLeft = 0;
                            double dRight = 0;
                            try
                            {
                                dLeft = Convert.ToDouble(leftValue);
                            }
                            catch (Exception ex) { }
                            try
                            {
                                dRight = Convert.ToDouble(rightValue);
                            }
                            catch (Exception ex) { }
                            if (dLeft < dRight)
                                isShow = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    isShow = false;
                                else
                                    isShow = true;
                            }

                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.Contains)
                        {
                            if (leftValue.IndexOf(rightValue, StringComparison.CurrentCultureIgnoreCase) != -1)
                                isShow = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    isShow = false;
                                else
                                    isShow = true;
                            }
                        }
                        else if (t.ShowMethod == CPGridEnum.ShowMethodEnum.DoesNotContain)
                        {
                            if (leftValue.IndexOf(rightValue, StringComparison.CurrentCultureIgnoreCase) == -1)
                                isShow = t.IsShowByExpression.Value;
                            else
                            {
                                if (t.IsShowByExpression.Value)
                                    isShow = false;
                                else
                                    isShow = true;
                            }
                        }
                    }
                    else
                        isShow = true;
                    if (isShow)
                    {
                        t.FuncContext = CPExpressionHelper.Instance.RunCompile(t.FuncContext);
                        tCol.Add(t);
                    }
                });
                #region 添加修改配置按钮
                if (re.Grid.SysId.HasValue && re.Grid.SysId.Value.Equals(CPAppContext.InnerSysId) == false)
                {
                    string UserAdminSysIds = CPExpressionHelper.Instance.RunCompile("${CPUser.UserAdminSysIds()}");
                    if (UserAdminSysIds.Split(',').ToList().Contains(re.Grid.SysId.ToString()))
                    {
                        CPGridFunc func1 = new CPGridFunc();
                        func1.FuncTitle = "修改配置";
                        func1.FuncOrder = 999999;
                        func1.FuncType = CPGridEnum.FuncTypeEnum.ExecuteJs;
                        func1.FuncOpenWinTitle = func1.FuncTitle;
                        func1.FuncIcon = "icon-shezhi1";
                        func1.IsUseExpressionShow = false;
                        func1.OpenWinHeight = 768;
                        func1.OpenWinWidth = 1024;
                        func1.FuncContext = "CPGridUpdateConfig(" + re.Grid.Id + ")";
                        tCol.Add(func1);
                    }
                }
                #endregion
                re.Grid.FuncCol = tCol;
                #endregion

                #region 处理多表头
                re.Grid.HeaderGroup = new List<GetGridInfoHeaderGroup>();
                List<string> addedHeader = new List<string>();
                re.Grid.ColumnCol.ForEach(t =>
                {
                    if (addedHeader.Contains(t.ColumnTitleGroup1) == false)
                    {
                        GetGridInfoHeaderGroup group1 = new GetGridInfoHeaderGroup();
                        group1.GroupTitle = t.ColumnTitleGroup1;
                        group1.ChildColumnId = new List<int>();
                        group1.ChildGroupCol = new List<GetGridInfoHeaderGroup>();
                        #region 找出所有的子列
                        List<CPGridColumn> tmpCol = re.Grid.ColumnCol.Where(c => c.ColumnTitleGroup1.Equals(t.ColumnTitleGroup1)).ToList();
                        if (string.IsNullOrEmpty(t.ColumnTitleGroup1))
                        {
                            tmpCol.ForEach(c =>
                            {
                                group1.ChildColumnId.Add(c.Id);
                            });
                        }
                        else
                        {
                            List<string> added2 = new List<string>();
                            tmpCol.ForEach(c =>
                            {
                            //如果一级不为空，则看看有没有二级子表头
                            if (added2.Contains(c.ColumnTitleGroup2) == false)
                                {
                                    List<CPGridColumn> tmpCol2 = re.Grid.ColumnCol.Where(f => f.ColumnTitleGroup1.Equals(t.ColumnTitleGroup1) && f.ColumnTitleGroup2.Equals(c.ColumnTitleGroup2)).ToList();
                                    GetGridInfoHeaderGroup groupChild = new GetGridInfoHeaderGroup();
                                    groupChild.GroupTitle = c.ColumnTitleGroup2;
                                    groupChild.ChildColumnId = new List<int>();
                                    groupChild.ChildGroupCol = new List<GetGridInfoHeaderGroup>();
                                    tmpCol2.ForEach(f =>
                                    {
                                        groupChild.ChildColumnId.Add(f.Id);
                                    });
                                    added2.Add(c.ColumnTitleGroup2);
                                    group1.ChildGroupCol.Add(groupChild);
                                }
                            });
                        }
                        #endregion
                        re.Grid.HeaderGroup.Add(group1);
                        addedHeader.Add(t.ColumnTitleGroup1);
                    }
                });
                #endregion
                re.Result = true;
                #region 处理编辑列表，下拉 列表数据源
                re.ListDataJson = "";
                try
                {
                    re.ListDataJson = JsonConvert.SerializeObject(CPGridEngine.Instance(CurUserId).ReadListDataAndFormat(re.Grid));
                }
                catch (Exception ex)
                {
                    re.Result = false;
                    re.ErrorMsg = ex.Message;
                }
                #endregion

                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 获取实际数据
        public class GetGridDataReturn : CPWebApiBaseReturnEntity
        { 

            /// <summary>
            /// 真实数据JSON
            /// </summary>
            public string DataJson { get; set; }
           
            /// <summary>
            /// 总数据条数
            /// </summary>
            public int RecordSize { get; set; }
        }
      
        [HttpGet]
        public GetGridDataReturn GetGridData(string GridCode, int CurUserId, string CurUserIden,
            int page, int pageSize,string OtherCondition)
        {
            base.SetHeader();
            GridCode = CPAppContext.FormatSqlPara(GridCode);
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            //OtherCondition = CPAppContext.FormatSqlPara(OtherCondition);
            if (OtherCondition != null)
            {
                OtherCondition = System.Web.HttpUtility.UrlDecode(OtherCondition);
                OtherCondition = OtherCondition.Replace("@", "%");
            }
             GetGridDataReturn re = new GetGridDataReturn();
            try
            {
                CPGrid Grid = CPGridEngine.Instance(CurUserId).GetGrid(GridCode, true, true);
                string orderBy = "";
                #region 获取排序字段
                List<string> querys = CPAppContext.GetHttpContext().Request.Query.Keys.Where(t => t.StartsWith("sort[")).ToList();
                var queryCount = querys.Count(m => m.EndsWith("[field]"));
                for (int i = 0; i < queryCount; i++)
                {
                    //请查询字段和对应的值存储在一个字典中
                    string field = CPAppContext.QueryString<string>("sort[" + i + "][field]");
                    field = field.Replace("Column", "");
                    List<CPGridColumn> colTmp = Grid.ColumnCol.Where(c => c.Id.Equals(int.Parse(field))).ToList();
                    if (colTmp.Count > 0)
                        field = colTmp[0].FieldName;
                    string fieldValue = CPAppContext.QueryString<string>("sort[" + i + "][dir]");
                    if (string.IsNullOrEmpty(orderBy))
                    {
                        orderBy = field + " " + fieldValue;
                    }
                    else
                        orderBy += "," + field + " " + fieldValue;
                }
                #endregion


                if (this.CheckUserIden(CurUserId, CurUserIden) == false)
                {
                    re.Result = false;
                    re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                    return re;
                }

                try
                {
                    int recordSize = 0;
                    DataTable dt = CPGridEngine.Instance(CurUserId).ReadDataAndFormat(Grid, page, pageSize, OtherCondition, orderBy, out recordSize);
                    re.RecordSize = recordSize;
                    re.DataJson = CPUtils.DataTable2Json(dt);
                    re.Result = true;
                }
                catch (Exception ex)
                {
                    re.Result = false;
                    re.ErrorMsg = ex.Message.ToString();
                }

                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 自动同步 列表列信息
        [HttpGet]

        //http://localhost:9000/CPSite/api/GridEngine/SynGridFieldInfo?GridId=1&CurUserId=1&CurUserIden=E0A04F94-90D5-4C2A-8C9D-836813F73DC4
        public CPWebApiBaseReturnEntity SynGridFieldInfo(int GridId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.Result = CPGridEngine.Instance(CurUserId).SyncFieldInfo(GridId);
                if (re.Result == false)
                {
                    re.ErrorMsg = "从数据库同步字段信息时出错了！";
                }
            }
            catch(Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
            }
            return re;

        }
        #endregion


        #region 列表数据修改
        public class UpdateGridDataInput
        {
          public int CurUserId { get; set; }
          public string CurUserIden { get; set; }
            public string GridCode { get; set; }
            public List<UpdateGridDataInputItem> Items { get; set; }
        }
        public class UpdateGridDataInputItem
        {
            public string DataPK { get; set; }
            public  List<string> FieldNamCol{ get; set; }
            public List<string> FieldValueCol { get; set; }
        }
        [HttpPost]
        public CPWebApiBaseReturnEntity UpdateGridData([FromBody] UpdateGridDataInput input )
        {
            base.SetHeader();

            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(input.CurUserId, input.CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                CPGrid grid = CPGridEngine.Instance(input.CurUserId).GetGrid(input.GridCode, false, false);
                DbHelper dbHelper = new DbHelper(grid.DbIns, CPAppContext.CurDbType());
                StringBuilder sb = new StringBuilder();
                string[] pkArray = grid.PKFieldName.Split(',');
                input.Items.ForEach(t =>
                {
                    string strSql = "";
                    strSql = "UPDATE " + grid.MainTableName + " SET ";
                    for (int i = 0; i < t.FieldNamCol.Count; i++)
                    {
                        string sValue = t.FieldValueCol[i];
                        if (string.IsNullOrEmpty(sValue) == false)
                            sValue = sValue.Replace("'", "''");
                        else
                            sValue = "";
                        if (i == 0)
                        {
                            strSql += t.FieldNamCol[i] + "='" + sValue + "'";
                        }
                        else
                        {
                            strSql += "," + t.FieldNamCol[i] + "='" + sValue + "'";
                        }
                    }
                    string[] dataPKArray = t.DataPK.Split(',');
                    for (int i = 0; i < pkArray.Length; i++)
                    {
                        if (i == 0)
                        {
                            strSql += " WHERE " + pkArray[i] + "='" + dataPKArray[i] + "'";
                        }
                        else
                        {
                            strSql += " AND " + pkArray[i] + "='" + dataPKArray[i] + "'";
                        }
                    }
                    if (sb.Length > 0)
                        sb.Append(";");
                    sb.Append(strSql);

                });
                if (sb.Length > 0)
                {
                    dbHelper.ExecuteNonQuery(sb.ToString());
                }
                re.Result = true;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 列表数据删除
      
        [HttpGet]
        public CPWebApiBaseReturnEntity DeleteGridData(int CurUserId, string CurUserIden,string GridCode,string DataPks)
        {
            base.SetHeader();
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            try
            {
                if (this.CheckUserIden(CurUserId, CurUserIden) == false)
                {
                    re.Result = false;
                    re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                    return re;
                }
                CPGrid grid = CPGridEngine.Instance(CurUserId).GetGrid(GridCode, false, false);
                DbHelper dbHelper = new DbHelper(grid.DbIns, CPAppContext.CurDbType());
                StringBuilder sb = new StringBuilder();

                string[] dataPKValueArray = DataPks.Split('@');
                if (string.IsNullOrEmpty(grid.DelDataSql))
                {
                    string[] pkArray = grid.PKFieldName.Split(',');
                    for (int m = 0; m < dataPKValueArray.Length; m++)
                    {
                        string strSql = "";
                        strSql = "DELETE FROM  " + grid.MainTableName + " WHERE  ";
                        string[] vArray = dataPKValueArray[m].Split(',');
                        for (int i = 0; i < pkArray.Length; i++)
                        {
                            if (i == 0)
                            {
                                strSql += pkArray[i] + "='" + vArray[i] + "'";
                            }
                            else
                            {
                                strSql += " AND " + pkArray[i] + "='" + vArray[i] + "'";
                            }
                        }
                        sb.Append(strSql);
                    }
                }
                else
                {
                    //自己配置了查询条件
                    string strSql = grid.DelDataSql;
                    DataPks = "'" + DataPks.Replace("@", "','") + "'";
                    strSql = strSql.Replace("{@PKValues@}", DataPks);
                    strSql = CPExpressionHelper.Instance.RunCompile(strSql);
                    sb.Append(strSql);
                }
                if (sb.Length > 0)
                {
                    dbHelper.ExecuteNonQuery(sb.ToString());
                }
                re.Result = true;
                return re;
            }
            catch(Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 导入列表内置功能
        [HttpGet]
        public CPWebApiBaseReturnEntity ImportGridInnerFunc(int GridId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                CPGridEngine.Instance(CurUserId).ImportInnerFunc(GridId);
                re.Result = true;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 导出配置
        [HttpGet]
        public FileResult DownloadGridConfig(string GridIds, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                throw new Exception("系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！");
            }
            GridIds = GridIds.Replace("@", ",");
            GridIds = CPAppContext.FormatSqlPara(GridIds);
            List<int> col = new List<int>();
            GridIds.Split(',').ToList().ForEach(t => {
                if (string.IsNullOrEmpty(t) == false)
                    col.Add(int.Parse(t));
            });
            string sXml = CPGridEngine.Instance(CurUserId).GetGridConfigXml(col);
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(sXml);
            return File(byteArray, "application/x-msdownload", "列表配置.CPXml");
        }
        #endregion

        #region 根据配置文件新增或修改配置
        [HttpPost]
        public CPWebApiBaseReturnEntity SynGridConfig(int TargetSysId,bool IsCreateNew,int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                var files = Request.Form.Files;
                foreach (var file in files)
                {
                    //  var filename = ContentDispositionHeaderValue
                    //                   .Parse(file.ContentDisposition)
                    //                .FileName
                    //                .Trim('"');
                    ////  filename = _FilePath + $@"\{filename}";
                    //  size += file.Length;
                    byte[] bData = null;
                    using (var fileStream = file.OpenReadStream())
                    using (var ms = new MemoryStream())
                    {
                        fileStream.CopyTo(ms);
                        bData = ms.ToArray();
                        //var fileBytes = ms.ToArray();
                        //string s = Convert.ToBase64String(fileBytes);
                        //// act on the Base64 data
                    }
                    if (bData != null)
                    {
                        if (IsCreateNew)
                        {
                            re.Result = CPGridEngine.Instance(CurUserId).InitGridFromConfigXml(TargetSysId, bData);
                        }
                        else
                        {
                            re.Result = CPGridEngine.Instance(CurUserId).SyncGridFromConfigXml(TargetSysId, bData);
                        }
                    }
                }
                re.Result = true;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion
    }
}
