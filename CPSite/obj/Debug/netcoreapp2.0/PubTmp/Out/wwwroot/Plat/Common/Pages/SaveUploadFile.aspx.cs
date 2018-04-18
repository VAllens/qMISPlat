using CPFrameWork.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CPFrameWorkV1.Plat.Common.Pages
{
    public partial class SaveUploadFile : System.Web.UI.Page
    {
        protected string _FilePath = "";
        protected void Page_Load(object sender, EventArgs e)
        {
                this._FilePath = CPAppContext.QueryString<string>("FilePath");
            this._FilePath = CPAppContext.CPFiles() + this._FilePath;
                this._FilePath = Server.MapPath(this._FilePath);
                if (System.IO.Directory.Exists(this._FilePath) == false)
                    System.IO.Directory.CreateDirectory(_FilePath);
                this._FilePath += HttpContext.Current.Request.Files[0].FileName;
                byte[] bData = new byte[HttpContext.Current.Request.InputStream.Length];
                HttpContext.Current.Request.InputStream.Read(bData, 0, bData.Length);
                FileStream fs = new FileStream(_FilePath, FileMode.OpenOrCreate);//可以是其他重载方法 
                byte[] byData = new byte[fs.Length];
                fs.Read(byData, 0, byData.Length);
                fs.Close();
                Response.Write("{\"ReturnCode\":" + 1 + "}");
                Response.End();
           
            
        }
    }
}