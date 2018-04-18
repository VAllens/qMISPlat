using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPFrameWork.Global;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CPFrameWork.Plat.Tab
{ 
    public class TabController : CPMVCBaseController
    {
        public IActionResult ManaConfig()
        {
            base.SetGlobalViewBag();
            return View();
        }
        // GET: /<controller>/
        public IActionResult TabView()
        {
            base.SetGlobalViewBag();
            return View();
        }
    }
}
