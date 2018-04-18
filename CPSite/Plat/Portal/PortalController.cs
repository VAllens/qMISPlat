using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPFrameWork.Global;

namespace CPFrameWork.Plat.Portal
{
    public class PortalController : CPMVCBaseController
    {
        public IActionResult SysFrame()
        {
            base.SetGlobalViewBag(); 
            ViewBag.UserName = CPExpressionHelper.Instance.RunCompile("${CPUser.UserName()}");
            ViewBag.UserId = CPExpressionHelper.Instance.RunCompile("${CPUser.UserId()}");
            ViewBag.UserPhotoPath = CPExpressionHelper.Instance.RunCompile("${CPUser.UserPhotoPath()}");
            if(string.IsNullOrEmpty(ViewBag.UserPhotoPath))
            {
                ViewBag.UserPhotoPath = "../../Style/CommonImage/UserNoPic.png";
            }
            else
            {
                ViewBag.UserPhotoPath = CPAppContext.CPWebRootPath() + "/api/CPCommonEngine/ShowPicture?FilePath=" + System.Web.HttpUtility.UrlEncode(ViewBag.UserPhotoPath);
            }
            ViewBag.SystemTitle = CPAppContext.GetPara("SystemTitle");
            return View();
        }
        public IActionResult Login()
        {
            base.SetGlobalViewBag();
            string DefaultUrl = CPAppContext.GetPara("DefaultUrl");
            if (string.IsNullOrEmpty(DefaultUrl))
                DefaultUrl = "/Plat/Portal/SysFrame";
            ViewBag.DefaultUrl = DefaultUrl;
            return View();
        }
        public IActionResult LoginOut()
        {
            CPAppContext.GetHttpContext().Session.Clear();
            string loginUrl = CPAppContext.GetPara("LoginUrl");
            if(string.IsNullOrEmpty(loginUrl))
                return RedirectToAction("Login");
            else
                return Redirect(CPAppContext.CPWebRootPath() + loginUrl);
            //
        }
    }
}