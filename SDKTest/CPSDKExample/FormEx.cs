using System;
using System.Collections.Generic;
using System.Text;
using CPFrameWork.UIInterface.Form;

namespace CPSDKExample
{
    /*
     *  本文件示例表单二次开发
     * */
    public class FormBeforeLoadEx : CPFrameWork.UIInterface.Form.ICPFormBeforeLoad
    {
        /// <summary>
        /// 表单数据加载前扩展开发接口
        /// </summary>
        /// <param name="e"></param>
        public void BeforeLoad(ICPFormBeforeLoadEventArgs e)
        {
            //e.Form获取静音配置对旬
            //e.FormData获取表单数据
            //e.IsAdd表单是否处于新增状态
            //e.IsEdit表单是否处理修改状态
            // e.IsView表单当前是否是只读状态
            //e.PKValue 获取表单主键，新增时为空
            //在这个接口里，你可以根据业务需求更改表单要显示的数据
            e.FormData.Tables[0].Rows[0]["Name"] = "修改后的Name";
        }
    }

    public class FormAfterSaveEx : CPFrameWork.UIInterface.Form.ICPFormAfterSave
    {
        public void AfterSave(ICPFormAfterSaveEventArgs e)
        {
            //e.Form获取静音配置对旬
            //e.FormData获取表单数据
            //e.IsAdd表单是否处于新增状态
            //e.IsEdit表单是否处理修改状态
            // e.IsView表单当前是否是只读状态
            //e.PKValue 获取表单主键，新增时为空
            //在这个接口里，你可以根据业务需求处理表单数据保存成功后执行的相关业务操作

            //获取主表某个字段的值
            string Name = e.GetFieldValue("表名", "字段名", 0);
            //如果需要 获取子表某个字段的值
            int rowIndex = 0;//获取子表第几行数据
            string Name2 = e.GetFieldValue("表名", "字段名", rowIndex);
        }

        
    }
}
