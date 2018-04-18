using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPFrameWork.Global;
using System.Net.Http.Headers;
using System.IO;
using CPFrameWork.Global.Systems;
using System.Reflection;
using System.Net.Http;
using System.Net;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CPFrameWork.Plat.Common
{
    public class SelectExpClass
    {
        public string Name { get; set; }
        public string ExpKey { get; set; }
        public List<SelectExpMethod> MethodCol { get; set; }
    }
    public class SelectExpMethod
    {
        public string Name { get; set; }
        public string MethodName { get; set; }
        public List<string> ParaCol { get; set; }
        public List<string> ParaTitleCol { get; set; }
        public string ParaNames
        {
            get
            {
                if (this.ParaCol.Count <= 0)
                    return "";
                string s = "";
                this.ParaCol.ForEach(t => {
                    if (s == "") s = t;
                    else
                        s += "," + t;
                });
                return s;
            }
        }
        public string ParaNameTitles
        {
            get
            {
                if (this.ParaTitleCol.Count <= 0)
                    return "";
                string s = "";
                this.ParaTitleCol.ForEach(t => {
                    if (s == "") s = t;
                    else
                        s += "," + t;
                });
                return s;
            }
        }
    }
    public class CommonController : CPMVCBaseController
    {
        public IActionResult PlatAdmin()
        {
            base.SetGlobalViewBag();
            List<CPSystem> sysCol = CPSystemHelper.Instance().GetSystems();
            ViewBag.SystemCol = sysCol;
            ViewBag.DefaultUrl = CPAppContext.CPWebRootPath() + "/Plat/Tab/TabView?TabCode=Tab0002&SysId=2";
            return View();
        }
        public IActionResult Help()
        {
            base.SetGlobalViewBag();
          
            return View();
        }

        public ActionResult SelectExp()
        {
            base.SetGlobalViewBag();
            //nType: 0通用表达式  1列表  2表单   3树 4流程
            int ToolType = CPAppContext.QueryString<int>("Type");
            List<SelectExpClass> cCol = new List<SelectExpClass>();
            for (int i = 0; i < CPExpressionHelper.Instance._vltContext.Keys.Length; i++)
            {
                Type type = CPExpressionHelper.Instance._vltContext.InternalGet(CPExpressionHelper.Instance._vltContext.Keys[i].ToString()).GetType();
                SelectExpClass c = new SelectExpClass();
                var attribute = type.GetCustomAttributes(typeof(CPNameAttribute), false).FirstOrDefault(); 
                c.Name = type.Name;
                c.ExpKey = CPExpressionHelper.Instance._vltContext.Keys[i].ToString();
                if (attribute != null)
                {
                    c.Name = ((CPNameAttribute)attribute).Name + c.Name;
                    if (ToolType.Equals(0))
                    {
                        if (((CPNameAttribute)attribute).ToolType.Equals(1)
                            || ((CPNameAttribute)attribute).ToolType.Equals(2)
                            || ((CPNameAttribute)attribute).ToolType.Equals(3)
                            || ((CPNameAttribute)attribute).ToolType.Equals(4)
                            )
                            continue;
                    }
                    else if (ToolType.Equals(1))
                    {
                        if (((CPNameAttribute)attribute).ToolType.Equals(2)
                            ||
                            ((CPNameAttribute)attribute).ToolType.Equals(3)
                               ||
                            ((CPNameAttribute)attribute).ToolType.Equals(4)
                            )
                            continue;
                    }
                    else if (ToolType.Equals(2))
                    {
                        if (((CPNameAttribute)attribute).ToolType.Equals(1)
                            ||
                            ((CPNameAttribute)attribute).ToolType.Equals(3)
                             ||
                            ((CPNameAttribute)attribute).ToolType.Equals(4)
                            )
                            continue;
                    }
                    else if (ToolType.Equals(3))
                    {
                        if (((CPNameAttribute)attribute).ToolType.Equals(1)
                            ||
                            ((CPNameAttribute)attribute).ToolType.Equals(2)
                             ||
                            ((CPNameAttribute)attribute).ToolType.Equals(4)
                            )
                            continue;
                    }
                    else if (ToolType.Equals(4))
                    {
                        if (((CPNameAttribute)attribute).ToolType.Equals(1)
                            ||
                            ((CPNameAttribute)attribute).ToolType.Equals(2)
                             ||
                            ((CPNameAttribute)attribute).ToolType.Equals(3)
                            )
                            continue;
                    }
                }
                c.MethodCol = new List<SelectExpMethod>();
                MethodInfo[] mCol = type.GetMethods();
                mCol.ToList().ForEach(t =>
                {
                    if (t.Attributes.ToString().ToLower().IndexOf("public") == -1)
                        return;
                    if (t.Name.Equals("ToString", StringComparison.CurrentCultureIgnoreCase)
                    || t.Name.Equals("Equals", StringComparison.CurrentCultureIgnoreCase)
                    || t.Name.Equals("GetHashCode", StringComparison.CurrentCultureIgnoreCase)
                    || t.Name.Equals("GetType", StringComparison.CurrentCultureIgnoreCase)
                    )
                        return;
                    SelectExpMethod m = new SelectExpMethod();
                    m.Name = t.Name;
                    m.MethodName = t.Name;
                    var attribute2 = t.GetCustomAttributes(typeof(CPNameAttribute), false).FirstOrDefault();
                    if (attribute2 != null)
                    {
                         m.Name = ((CPNameAttribute)attribute2).Name +  m.Name;
                    }
                    m.ParaCol = new List<string>();
                    m.ParaTitleCol = new List<string>();
                    t.GetParameters().ToList().ForEach(p =>
                    {
                        var attribute3 = p.GetCustomAttributes(typeof(CPNameAttribute), false).FirstOrDefault();
                        if (attribute3 != null)
                        {
                            m.ParaTitleCol.Add(((CPNameAttribute)attribute3).Name);
                        }
                        else
                            m.ParaTitleCol.Add("");
                        m.ParaCol.Add(p.Name);
                    });
                    c.MethodCol.Add(m);
                });
                cCol.Add(c);
            }
            ViewBag.ExpCol = cCol;
            return View();
        }
        // GET: /<controller>/
        public IActionResult FileUpload()
        {
            base.SetGlobalViewBag();
            string BusinessCode = CPAppContext.QueryString<string>("BusinessCode");
            BusinessCode = CPAppContext.FormatSqlPara(BusinessCode);
            string ReturnMethod = CPAppContext.QueryString<string>("ReturnMethod");
            string AllowFileTypes = CPAppContext.QueryString<string>("AllowFileTypes");
            string AllowFileCount = CPAppContext.QueryString<string>("AllowFileCount");
            string FilePath = BusinessCode + "/" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/" + new CPRuntimeContext().HHMMSSLongString() + "/";
            ViewBag.BusinessCode = BusinessCode;
            ViewBag.ReturnMethod = ReturnMethod;
            ViewBag.FilePath = FilePath;
            ViewBag.AllowFileTypes = AllowFileTypes;
            ViewBag.AllowFileCount = AllowFileCount;
            return View();
        }
      
    }
}
