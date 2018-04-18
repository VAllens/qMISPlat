using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPFrameWork.Global;
using Microsoft.AspNetCore.Hosting;

namespace CPFrameWork.Plat.Tree
{
    public class TreeController : CPMVCBaseController
    {
        public IActionResult ManaConfig()
        {
            base.SetGlobalViewBag();
            return View();
        }
        public IActionResult TreeView()
        {
            base.SetGlobalViewBag();
            if (CPAppContext.HostingEnvironment.IsDevelopment())
            {
                ViewBag.TreeJS = "TreeEngine.js";
            }
            else
            {
                ViewBag.TreeJS = "TreeEngine.min.js";
            }
            return View();
        }
    }
}