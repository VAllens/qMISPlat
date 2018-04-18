using AutoMapper;
using CPFrameWork.Global;
using CPFrameWork.Utility.DbOper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CPFrameWork.Global
{

    public class AutoEngineController : CPWebApiBaseController
    {


        

        #region 导出配置
        [HttpGet]
        public FileResult DownloadAutoConfig(string AutoIds, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                throw new Exception("系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！");
            }
            AutoIds = AutoIds.Replace("@", ",");
            AutoIds = CPAppContext.FormatSqlPara(AutoIds);
            List<int> col = new List<int>();
            AutoIds.Split(',').ToList().ForEach(t => {
                if (string.IsNullOrEmpty(t) == false)
                    col.Add(int.Parse(t));
            });
            string sXml = CPAutoNumHelper.Instance().GetAutoConfigXml(col);
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(sXml);
            return File(byteArray, "application/x-msdownload", "自动编号配置.CPXml");
        }
        #endregion

        #region 根据配置文件新增或修改配置
        [HttpPost]
        public CPWebApiBaseReturnEntity SynAutoConfig(int TargetSysId, bool IsCreateNew, int CurUserId, string CurUserIden)
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
                        re.Result = CPAutoNumHelper.Instance().InitAutoFromConfigXml(TargetSysId, bData);
                    }
                    else
                    {
                        re.Result = CPAutoNumHelper.Instance().SyncAutoFromConfigXml(TargetSysId, bData);
                    }
                }
            }
            re.Result = true;
            return re;
        }
        #endregion
    }
}