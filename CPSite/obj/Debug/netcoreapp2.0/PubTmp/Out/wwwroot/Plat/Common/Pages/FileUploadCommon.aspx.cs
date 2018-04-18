using CPFrameWork.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CPFrameWorkV1.Plat.Common.Pages
{
    public partial class FileUploadCommon : System.Web.UI.Page
    {
        protected string _BusinessCode = "";
        protected string _ReturnMethod = "";
        protected string _FilePath = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            this._BusinessCode = CPAppContext.QueryString<string>("BusinessCode");
            this._ReturnMethod = CPAppContext.QueryString<string>("ReturnMethod");
            this._FilePath =  this._BusinessCode + "/" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/" + new CPRuntimeContext().HHMMSSLongString + "/";

        }
    }
}