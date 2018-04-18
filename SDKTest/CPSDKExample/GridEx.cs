using System;
using System.Collections.Generic;
using System.Text;
using CPFrameWork.UIInterface.Grid;

namespace CPSDKExample
{
    /*
     *  本文件示例列表二次开发
     * */
    public class GridEx : CPFrameWork.UIInterface.Grid.CPGridInterface
    {
        /// <summary>
        /// 列表数据，从数据库里读取出来后的扩展开发接口，在这个接口里，可以更改显示到列表界面上的数据
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool AfterReadDataFromDb(CPAfterReadDataFromDbEventArgs e)
        {
            //获取列表配置对旬 e.GridObj;
            //获取列表数据e.RealData;
            //修改某一行数据的值
            e.RealData.Rows[0]["Name"] = "修改后的数据";
            return true;
        }

        public bool BeforeReadDataFromDb(CPBeforeReadDataFromDbEventArgs e)
        {
            //获取列表配置对旬 e.GridObj;
            //获取或设置列表取数据的SQL语句 ：e.StrSql
            e.StrSql = "SELECT * FROM Table where Id=9";
            return true;
        }
    }
}
