using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPFrameWork.Global;
using Microsoft.AspNetCore.Hosting;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CPFrameWork.Plat.Form
{
    public class FormController : CPMVCBaseController
    {
        public IActionResult ManaConfig()
        {
            base.SetGlobalViewBag();
            return View();
        }
        // GET: /<controller>/
        public IActionResult FormView()
        {
            base.SetGlobalViewBag();
            if (CPAppContext.HostingEnvironment.IsDevelopment())
            {
                ViewBag.FormJS = "FormEngine.js";
            }
            else
            {
                ViewBag.FormJS = "FormEngine.min.js";
            }
            return View();
        }
        public IActionResult FormDesign()
        {
            base.SetGlobalViewBag();
            return View();
        }
    }
}
