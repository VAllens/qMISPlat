using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPFrameWork.Global;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CPFrameWork.Plat.Auto
{ 
    public class AutoController : CPMVCBaseController
    {
        public IActionResult ManaConfig()
        {
            base.SetGlobalViewBag();
            return View();
        }
     
    }
}
