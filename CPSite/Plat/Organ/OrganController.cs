using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPFrameWork.Global;

namespace CPFrameWork.Plat.Organ
{
    public class OrganController : CPMVCBaseController
    {
        public IActionResult OrganSel()
        {
            base.SetGlobalViewBag();
            return View();
        }
        public IActionResult UserPwdUpdate()
        {
            base.SetGlobalViewBag();
            return View();
        }
    }
}