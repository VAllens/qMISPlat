using CPFrameWork.Utility.DbOper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace CPFrameWork.Global
{
   
    public class CPWebApiBaseReturnEntity
    {
        public bool Result { get; set; }
        public string ErrorMsg { get; set; }
    }
    [Route("Plat/[controller]/[action]/{id?}")]
    public class CPMVCBaseController : Controller
    {
        
        public void SetGlobalViewBag()
        {
            ViewBag.CPSkin = "Default";
            ViewBag.CPWebRootPath = "<script>var CPWebRootPath = \"" + CPAppContext.CPWebRootPath()+ "\";</script>";
            ViewBag.CPCurUserId = "<script>var CPCurUserId = \"" + CPExpressionHelper.Instance.RunCompile("${CPUser.UserId()}") + "\";</script>";
            ViewBag.CPCurUserIden = "<script>var CPCurUserIden = \"" + CPExpressionHelper.Instance.RunCompile("${CPUser.UserIden()}") + "\";</script>";
            ViewBag.CPWebRootPath2 = CPAppContext.CPWebRootPath();
        }

    }
   [Route("api/[controller]/[action]/{id?}")]
    //[Route("api/[controller]")]
    public class CPWebApiBaseController : Controller
    {
        public void SetHeader()
        {
            try
            {
                //增加设置允许跨域访问，否则会导致如果直接把HTML放入手机里时，会导致不能访问数据的问题
                CPAppContext.GetHttpContext().Response.Headers.Add("Access-Control-Allow-Origin", "*"); //允许哪些url可以跨域请求到本域
                CPAppContext.GetHttpContext().Response.Headers.Add("Access-Control-Allow-Methods", "POST"); //允许的请求方法，一般是GET,POST,PUT,DELETE,OPTIONS
                CPAppContext.GetHttpContext().Response.Headers.Add("Access-Control-Allow-Headers", "x-requested-with,content-type"); //允许哪些请求头可以跨域
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }
        public void InitUserIden(String sessionKey)
        {
            //自动创建SESSion
        }
        public bool CheckUserIden(int UserId, String SessionKey)
        {
            try
            {
                new Guid(SessionKey);
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
            DbHelper _helper = new DbHelper("CPOrganIns", CPAppContext.CurDbType());
            string strSql = " SELECT UserId FROM CP_UserIdentity WHERE UserKey='" + SessionKey + "'";
            object userIdDb = _helper.ExecuteScalar(strSql);
            if (Convert.IsDBNull(userIdDb) || userIdDb == null)
                return false;
            else
            {
                if (int.Parse(userIdDb.ToString()).Equals(UserId))
                    return true;
                else
                    return false;
            } 
        }

       
    }
}
