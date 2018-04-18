using CPFrameWork.Global;
using CPFrameWork.UIInterface.Tab;
using CPFrameWork.Utility;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace CPFrameWork.UIInterface
{
    public class CPCommonEngineController: CPWebApiBaseController
    {
        [HttpPost]
        public IActionResult UploadFilesAjax()
        {
            string _FilePath = CPAppContext.QueryString<string>("FilePath");
            _FilePath = CPAppContext.CPFilesPath() + _FilePath;
            if (System.IO.Directory.Exists(_FilePath) == false)
                System.IO.Directory.CreateDirectory(_FilePath);

            long size = 0;
            var files = Request.Form.Files;
            foreach (var file in files)
            {
                var filename = ContentDispositionHeaderValue
                                 .Parse(file.ContentDisposition)
                              .FileName
                              .Trim('"');
                filename = _FilePath + $@"\{filename}";
                size += file.Length;
                using (FileStream fs = System.IO.File.Create(filename))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
            }
            string message = "{\"ReturnCode\":" + 1 + "}";
            return Json(message);
        }
        [HttpGet]
        public FileResult DownloadFile()
        {

            string FilePath = CPAppContext.QueryString<string>("FilePath");

            FilePath = CPAppContext.CPFilesPath() + FilePath;

            byte[] fileBytes = System.IO.File.ReadAllBytes(FilePath);
            return File(fileBytes, "application/x-msdownload", System.IO.Path.GetFileName(FilePath));
        }

        [HttpGet]
        public FileResult ShowPicture()
        {

            string FilePath = CPAppContext.QueryString<string>("FilePath");
            FilePath = CPAppContext.CPFilesPath() + FilePath;
            int lastIndex = FilePath.LastIndexOf(".");
            string fileEx = FilePath.Substring(lastIndex + 1, FilePath.Length - lastIndex - 1);
            // return new FileStreamResult(new FileStream(FilePath, FileMode.Open), "image/" + fileEx);
            byte[] fileBytes = System.IO.File.ReadAllBytes(FilePath);
            return File(fileBytes, "image/" + fileEx, System.IO.Path.GetFileName(FilePath));
        }

    }
}
