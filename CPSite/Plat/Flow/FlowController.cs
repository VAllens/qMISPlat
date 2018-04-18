using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPFrameWork.Global;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using CPFrameWork.Flow;
using CPFrameWork.Flow.Domain;

namespace CPFrameWork.Plat.Flow
{
    public class FlowController : CPMVCBaseController
    {
        public IActionResult FlowDesign()
        {
            base.SetGlobalViewBag();
            return View();
        }
        public IActionResult FlowStartFrame()
        {
            base.SetGlobalViewBag();
            string UserId = CPExpressionHelper.Instance.RunCompile("${CPUser.UserId()}");
            string UserRoleIds = CPExpressionHelper.Instance.RunCompile("${CPUser.UserRoleIds()}");
            string DepIds = CPExpressionHelper.Instance.RunCompile("${CPUser.DepIds()}");
            CPFlowTemplate template = CPFlowTemplate.Instance();
            List<int> roleIdCol = new List<int>();
            List<int> depIdCol = new List<int>();
            UserRoleIds.Split(',').ToList().ForEach(t => {
                if(string.IsNullOrEmpty(t)==false)
                {
                    roleIdCol.Add(int.Parse(t));
                }
            });
            DepIds.Split(',').ToList().ForEach(t => {
                if (string.IsNullOrEmpty(t) == false)
                {
                    depIdCol.Add(int.Parse(t));
                }
            });
            List<CPFlow>  col  = template.GetHasStartRightFlow(int.Parse(UserId), roleIdCol, depIdCol);
            string flowVerIds = "";
            col.ForEach(t => {
                if (string.IsNullOrEmpty(flowVerIds))
                    flowVerIds = t.FlowVerId.ToString();
                else
                    flowVerIds += "," +  t.FlowVerId.ToString();
            });
            if (string.IsNullOrEmpty(flowVerIds))
                flowVerIds = "-1";
            CPAppContext.GetHttpContext().Session.SetString("UserHasRightFlowVerIds", flowVerIds);
            return View();
        }
        public IActionResult FlowMonitorFrame()
        {
            base.SetGlobalViewBag();
            string UserId = CPExpressionHelper.Instance.RunCompile("${CPUser.UserId()}");
            string UserRoleIds = CPExpressionHelper.Instance.RunCompile("${CPUser.UserRoleIds()}"); 
            CPFlowTemplate template = CPFlowTemplate.Instance();
            List<int> roleIdCol = new List<int>(); 
            UserRoleIds.Split(',').ToList().ForEach(t => {
                if (string.IsNullOrEmpty(t) == false)
                {
                    roleIdCol.Add(int.Parse(t));
                }
            }); 
            List<CPFlow> col = template.GetHasMonitorRightFlow(int.Parse(UserId), roleIdCol);
            string flowVerIds = "";
            col.ForEach(t => {
                if (string.IsNullOrEmpty(flowVerIds))
                    flowVerIds = t.FlowVerId.ToString();
                else
                    flowVerIds += "," + t.FlowVerId.ToString();
            });
            if (string.IsNullOrEmpty(flowVerIds))
                flowVerIds = "-1";
            CPAppContext.GetHttpContext().Session.SetString("UserHasMonitorRightFlowVerIds", flowVerIds); 
            return Redirect(CPAppContext.CPWebRootPath() + "/Plat/Grid/GridView?GridCode="  + CPAppContext.QueryString<string>("GridCode"));
        }
        public IActionResult FlowMonitor()
        {
            base.SetGlobalViewBag();
            return View();
        }
        public IActionResult FlowEngine()
        {
            base.SetGlobalViewBag();
            if (CPAppContext.HostingEnvironment.IsDevelopment())
            {
                ViewBag.FlowJS = "FlowEngine.js";
            }
            else
            {
                ViewBag.FlowJS = "FlowEngine.min.js";
            }
            return View();
        }
        public IActionResult FlowSubmit()
        {
            base.SetGlobalViewBag();
           
            return View();
        }
        public IActionResult FlowFallback()
        {
            base.SetGlobalViewBag();

            return View();
        }
    }
}