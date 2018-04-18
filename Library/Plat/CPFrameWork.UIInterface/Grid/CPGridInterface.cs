using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CPFrameWork.UIInterface.Grid
{
    public interface CPGridInterface
    {
        bool BeforeReadDataFromDb(CPBeforeReadDataFromDbEventArgs e);
        bool AfterReadDataFromDb(CPAfterReadDataFromDbEventArgs e);
    }
    public class CPAfterReadDataFromDbEventArgs
    {
        public CPGrid GridObj { get; set; }
        public DataTable RealData { get; set; }
        public CPAfterReadDataFromDbEventArgs(CPGrid grid, DataTable realData)
        {
            this.GridObj = grid;
            this.RealData = RealData;
        }
    }
    public class CPBeforeReadDataFromDbEventArgs
    {
        public CPGrid GridObj { get; set; }
        public string  StrSql { get; set; } 
        public CPBeforeReadDataFromDbEventArgs(CPGrid grid, string strSql)
        {
            this.GridObj = grid;
            this.StrSql = strSql;
        }

    }
}
