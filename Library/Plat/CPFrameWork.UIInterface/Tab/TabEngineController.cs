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

namespace CPFrameWork.UIInterface.Grid
{
    public class TabEngineController : CPWebApiBaseController
    {

        #region 获取配置信息
        public class GetTabInfoReturn : CPWebApiBaseReturnEntity
        {
            public CPTab Tab { get; set; }
        }
        [HttpGet]
        public GetTabInfoReturn GetTabInfo(string  TabCode , int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            TabCode = CPAppContext.FormatSqlPara(TabCode);
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);

            GetTabInfoReturn re = new GetTabInfoReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.Tab = CPTabEngine.Instance().GetTab(TabCode, true);
                #region 处理
                List<CPTabItem> cCol = new List<CPTabItem>();
                re.Tab.ItemCol.ForEach(t =>
                {

                    bool isShow = true;
                    if (t.IsUseExpressionShow.Value)
                    {
                        string leftValue = CPExpressionHelper.Instance.RunCompile(t.LeftExpression);
                        string rightValue = CPExpressionHelper.Instance.RunCompile(t.RightExpression);

                        if (t.ShowMethod == CPTabEnum.ShowMethodEnum.EqualTo)
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
                        else if (t.ShowMethod == CPTabEnum.ShowMethodEnum.NotEqualTo)
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
                        else if (t.ShowMethod == CPTabEnum.ShowMethodEnum.GreaterThanOrEqualTo)
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
                        else if (t.ShowMethod == CPTabEnum.ShowMethodEnum.GreaterThan)
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
                        else if (t.ShowMethod == CPTabEnum.ShowMethodEnum.LessThanOrEqualTo)
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
                        else if (t.ShowMethod == CPTabEnum.ShowMethodEnum.LessThan)
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
                        else if (t.ShowMethod == CPTabEnum.ShowMethodEnum.Contains)
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
                        else if (t.ShowMethod == CPTabEnum.ShowMethodEnum.DoesNotContain)
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
                        t.TargetUrl = CPExpressionHelper.Instance.RunCompile(t.TargetUrl);
                        cCol.Add(t);
                    }
                });
                #region 添加修改配置按钮
                if (re.Tab.SysId.HasValue && re.Tab.SysId.Value.Equals(CPAppContext.InnerSysId) == false)
                {
                    string UserAdminSysIds = CPExpressionHelper.Instance.RunCompile("${CPUser.UserAdminSysIds()}");
                    if (UserAdminSysIds.Split(',').ToList().Contains(re.Tab.SysId.ToString()))
                    {
                        CPTabItem func1 = new CPTabItem();
                        func1.EleTitle = "修改配置";
                        func1.IsUseExpressionShow = false;
                        func1.ShowOrder = 99999;
                        func1.TabId = re.Tab.Id;
                        func1.TargetUrl = "/Plat/Form/FormView?FormCode=Form201709231502250010&SceneCode=Scene201709231514430010&ViewCode=View201709231514390010&DeviceType=1&InitGroupCode=Group201709231514470010&SysId=1&PKValues=" + re.Tab.Id;
                        cCol.Add(func1);
                    }
                }
                #endregion
                re.Tab.ItemCol = cCol;
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



        #region 导出配置
        [HttpGet]
        public FileResult DownloadTabConfig(string TabIds, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                throw new Exception("系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！");
            }
            TabIds = TabIds.Replace("@", ",");
            TabIds = CPAppContext.FormatSqlPara(TabIds);
            List<int> col = new List<int>();
            TabIds.Split(',').ToList().ForEach(t => {
                if (string.IsNullOrEmpty(t) == false)
                    col.Add(int.Parse(t));
            });
            string sXml = CPTabEngine.Instance().GetTabConfigXml(col);
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(sXml);
            return File(byteArray, "application/x-msdownload", "标签页配置.CPXml");
        }
        #endregion

        #region 根据配置文件新增或修改配置
        [HttpPost]
        public CPWebApiBaseReturnEntity SynTabConfig(int TargetSysId, bool IsCreateNew, int CurUserId, string CurUserIden)
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
                            re.Result = CPTabEngine.Instance().InitTabFromConfigXml(TargetSysId, bData);
                        }
                        else
                        {
                            re.Result = CPTabEngine.Instance().SyncTabFromConfigXml(TargetSysId, bData);
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
