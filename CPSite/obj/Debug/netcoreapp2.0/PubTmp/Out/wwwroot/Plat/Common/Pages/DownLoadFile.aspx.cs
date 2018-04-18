using CPFrameWork.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CPFrameWorkV1.Plat.Common.Pages
{
    public partial class DownLoadFile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string FilePath = CPAppContext.QueryString<string>("FilePath");
            FilePath = CPAppContext.CPFiles() + FilePath;
            FilePath = Server.MapPath(FilePath);
            if(System.IO.File.Exists(FilePath)==false)
            {
                Response.Write("文件【" + CPAppContext.QueryString<string>("FilePath")+ "】不存在");
                Response.End();
            }
            byte[] bData = System.IO.File.ReadAllBytes(FilePath);
            Response.AddHeader("content-type", "application/octet-stream");
            //设置文件大小
            Response.AddHeader("content-length", bData.Length.ToString());
            Response.BinaryWrite(bData);
            Response.Flush();
            Response.End();
            Response.Clear();
        }
    }
}