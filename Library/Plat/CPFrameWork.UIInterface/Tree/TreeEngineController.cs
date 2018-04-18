using CPFrameWork.Global;
using CPFrameWork.UIInterface.Tab;
using CPFrameWork.Utility;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace CPFrameWork.UIInterface.Tree
{
    public class TreeEngineController: CPWebApiBaseController
    {

        #region 获取配置信息
        public class GetTreeInfoReturn : CPWebApiBaseReturnEntity
        {
            public CPTree Tree { get; set; }
        }
        [HttpGet]
        public GetTreeInfoReturn GetTreeInfo(string TreeCode, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            TreeCode = CPAppContext.FormatSqlPara(TreeCode);
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);

            GetTreeInfoReturn re = new GetTreeInfoReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.Tree = CPTreeEngine.Instance().GetTree(TreeCode, true, true);

                #region 处理
                List<CPTreeFunc> cCol = new List<CPTreeFunc>();
                re.Tree.FuncCol.ForEach(t =>
                {

                    bool isShow = true;
                    if (t.IsUseExpressionShow.Value)
                    {
                        string leftValue = CPExpressionHelper.Instance.RunCompile(t.LeftExpression);
                        string rightValue = CPExpressionHelper.Instance.RunCompile(t.RightExpression);

                        if (t.ShowMethod == CPTreeEnum.ShowMethodEnum.EqualTo)
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
                        else if (t.ShowMethod == CPTreeEnum.ShowMethodEnum.NotEqualTo)
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
                        else if (t.ShowMethod == CPTreeEnum.ShowMethodEnum.GreaterThanOrEqualTo)
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
                        else if (t.ShowMethod == CPTreeEnum.ShowMethodEnum.GreaterThan)
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
                        else if (t.ShowMethod == CPTreeEnum.ShowMethodEnum.LessThanOrEqualTo)
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
                        else if (t.ShowMethod == CPTreeEnum.ShowMethodEnum.LessThan)
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
                        else if (t.ShowMethod == CPTreeEnum.ShowMethodEnum.Contains)
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
                        else if (t.ShowMethod == CPTreeEnum.ShowMethodEnum.DoesNotContain)
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
                        t.JSMethod = CPExpressionHelper.Instance.RunCompile(t.JSMethod);
                        cCol.Add(t);
                    }
                });
                #region 添加修改配置按钮
                if (re.Tree.SysId.HasValue && re.Tree.SysId.Value.Equals(CPAppContext.InnerSysId) == false)
                {
                    string UserAdminSysIds = CPExpressionHelper.Instance.RunCompile("${CPUser.UserAdminSysIds()}");
                    if (UserAdminSysIds.Split(',').ToList().Contains(re.Tree.SysId.ToString()))
                    {
                        CPTreeFunc func1 = new CPTreeFunc();
                        func1.FuncTitle = "修改配置";
                        func1.ShowOrder = 999999;
                        func1.FuncIcon = "icon-shezhi1";
                        func1.IsUseExpressionShow = false;
                        func1.JSMethod = "CPTreeUpdateConfig(" + re.Tree.Id + ")";
                        func1.ShowPosition = CPTreeEnum.ShowPositionEnum.TopAndRight;
                        func1.SourceId = -1;
                        func1.TreeId = re.Tree.Id;
                        cCol.Add(func1);
                    }
                }
                #endregion
                re.Tree.FuncCol = cCol;
                #endregion


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


        #region 获取树真实数据
        public class GetTreeDataReturn : CPWebApiBaseReturnEntity
        {
            public List<CPTreeNode> DataCol { get; set; }
        }
     
        public class GetTreeDataInput
        {
            public string TreeCode { get; set; }
            public int TreeDataSourceId { get; set; }
            public int CurUserId { get; set; }
            public string CurUserIden { get; set; }
            public string DataRowJSON { get; set; }
        }
        [HttpPost]
        public GetTreeDataReturn GetTreeData([FromBody] GetTreeDataInput input)
        {
            //DataLoadType 数据加载模式 1：一次性全加载 2：逐级加载
            base.SetHeader(); 
            GetTreeDataReturn re = new GetTreeDataReturn();
            if (this.CheckUserIden(input.CurUserId, input.CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.Result = true;
                re.DataCol = CPTreeEngine.Instance().GetTreeData(input.TreeCode, input.TreeDataSourceId, input.DataRowJSON);
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


        #region 删除数据
        [HttpGet]
        public CPWebApiBaseReturnEntity DeleteTreeData(int TreeDataSourceId,string pkFieldValue, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            pkFieldValue = CPAppContext.FormatSqlPara(pkFieldValue);
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
                string errorMsg = "";
                re.Result = CPTreeEngine.Instance().DeleteTreeData(TreeDataSourceId, pkFieldValue, ref errorMsg);
                if (re.Result == false)
                {
                    re.ErrorMsg = errorMsg;
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

        #region 导入内置按钮
        [HttpGet]
        public CPWebApiBaseReturnEntity ImportTreeInnerFunc(int treeId, int CurUserId, string CurUserIden)
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
                re.Result = CPTreeEngine.Instance().ImportTreeInnerFunc(treeId);
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

        #region 修改实际数据的父节点ID
        [HttpGet]
        public CPWebApiBaseReturnEntity UpdateTreeDataParent(int TreeDataSourceId, string SourcePKValue, string TargetPKValue, int CurUserId, string CurUserIden)
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
                string errorMsg = "";
                re.Result = CPTreeEngine.Instance().UpdateTreeDataParent(TreeDataSourceId, SourcePKValue, TargetPKValue, ref errorMsg);
                if (re.Result == false)
                {
                    re.ErrorMsg = errorMsg;
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

        #region 导出配置
        [HttpGet]
        public FileResult DownloadTreeConfig(string TreeIds, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                throw new Exception("系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！");
            }
            TreeIds = TreeIds.Replace("@", ",");
            TreeIds = CPAppContext.FormatSqlPara(TreeIds);
            List<int> col = new List<int>();
            TreeIds.Split(',').ToList().ForEach(t => {
                if (string.IsNullOrEmpty(t) == false)
                    col.Add(int.Parse(t));
            });
            string sXml = CPTreeEngine.Instance().GetTreeConfigXml(col);
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(sXml);
            return File(byteArray, "application/x-msdownload", "树配置.CPXml");
        }
        #endregion

        #region 根据配置文件新增或修改配置
        [HttpPost]
        public CPWebApiBaseReturnEntity SynTreeConfig(int TargetSysId, bool IsCreateNew, int CurUserId, string CurUserIden)
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
                            re.Result = CPTreeEngine.Instance().InitTreeFromConfigXml(TargetSysId, bData);
                        }
                        else
                        {
                            re.Result = CPTreeEngine.Instance().SyncTreeFromConfigXml(TargetSysId, bData);
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
