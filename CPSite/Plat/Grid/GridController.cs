using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPFrameWork.Global;
using CPFrameWork.UIInterface.Grid;
using Microsoft.AspNetCore.Hosting;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CPFrameWork.Plat.Grid
{
    public class GridController : CPMVCBaseController
    {
        public IActionResult ManaConfig()
        {
            base.SetGlobalViewBag();
            return View();
        }
        // GET: /<controller>/
        public IActionResult GridView()
        {
            base.SetGlobalViewBag();
            if(CPAppContext.HostingEnvironment.IsDevelopment())
            {
                ViewBag.GridJS = "GridEngine.js";
            }
            else
            {
                ViewBag.GridJS = "GridEngine.min.js";
            }
            return View();
        }
        public IActionResult GridViewPreview()
        {
            base.SetGlobalViewBag();
            int TargetGridId = CPAppContext.QueryString<int>("TargetGridId");
            string userId = CPExpressionHelper.Instance.RunCompile("${CPUser.UserId()}");
           CPGrid grid =  CPGridEngine.Instance(int.Parse(userId)).GetGrid(TargetGridId, false, false);
            return RedirectToAction("GridView", new { GridCode = grid.GridCode });
             

        }
    }
}
