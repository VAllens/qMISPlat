using System;
using System.Collections.Generic;
using System.Text;

namespace CPFrameWork.UIInterface.Grid
{
   public  class CPGridEnum
    {
        public enum GridTypeEnum
        {
            PC = 1,
            Mobile=2
        }
        public enum DataSourceTypeEnum
        {
            Sql = 1,
            Class = 2
        }
        public enum PKValueTypeEnum
        {
            IntSelfIncreasing = 1,
            IntNotSelfIncreasing = 2,
            GUID = 3
        }
        public enum GroupSortEnum
        {
            asc = 1,
            desc = 2
        }
        public enum SearchModelEnum
        {
            QuickSearch =1,
            ExpandSearch = 2
        }
        public enum DataSumTypeEnum
        {
            /// <summary>
            /// 当前页
            /// </summary>
            CurPage = 1,
            /// <summary>
            /// 所有数据
            /// </summary>
            AllData = 2
        }
        public enum DataTimeSumTypeEnum
        {
            None = -1,
            /// <summary>
            /// 单一年份
            /// </summary>
            SingleYear = 1,
            /// <summary>
            /// 单一月份
            /// </summary>
            SingleMonth = 2,
            /// <summary>
            /// 月份时间段
            /// </summary>
            MonthBetween = 3,
            /// <summary>
            /// 年月时间段
            /// </summary>
            YearMonthBetween = 4,
            /// <summary>
            /// 年份时间段
            /// </summary>
            YearBetween = 5,
            /// <summary>
            /// 单一季度
            /// </summary>
            SingleQuarter = 6
        }
        /// <summary>
        /// 列类型
        /// </summary>
        public enum ColumnTypeEnum
        {
            /// <summary>
            /// 序号列            /// </summary>
            Number = 1,
            /// <summary>
            /// 复选框 
            /// </summary>
            CheckBox = 2,
            /// <summary>
            /// 单选列
            /// </summary>
            Radio = 3,
            /// <summary>
            /// 普通列
            /// </summary>
            Normal = 4,
            /// <summary>
            /// 日期列            /// </summary>
            DateTime = 5, 
            /// <summary>
            /// 列表内置的删除列
            /// </summary>
            ListInnerDel = 6,
            /// <summary>
            /// 模板列            /// </summary>
            Template = 7,
            /// <summary>
            /// 图片超链列            /// </summary>
            PicHref = 8,
            /// <summary>
            /// 文字超链列            /// </summary>
            TextHref = 9,
             
            /// <summary>
            /// 文本框编辑列
            /// </summary>
            TextBoxEditor = 10,
            /// <summary>
            /// 下拉列表编辑列            /// </summary>
            DropDownListEditor = 11,
            /// <summary>
            /// 时间编辑列            /// </summary>
            TimeSelectEditor = 12,
            
        }
        public enum ShowPositionEnum
        {
            Left=1,
            Middle = 2,
            Right = 3
        }
        public enum SearchShowTypeEnum
        {
            Textbox = 1,
            TimeSel = 2,
            DropDownList = 3
        }

        public enum TargetTypeEnum
        {
            _self = 1,
            blank = 2,
            onclick = 3,
            TopOpenNewModel = 4,
            TopOpenNewModelAndRefresh = 5,
            ParentOpenNewModel = 6,
            ParentOpenNewModelAndRefresh = 7,
            OpenNewModel = 8,
            OpenNewModelAndRefresh = 9
        }
        /// <summary>
        /// 根据条件控制列或按钮是否显示的比较方式        /// </summary>
        public enum ShowMethodEnum
        {

            /// <summary>
            /// 等于
            /// </summary>
            EqualTo = 1,
            /// <summary>
            /// 不等于
            /// </summary>
            NotEqualTo = 2,
            /// <summary>
            /// 大于等于
            /// </summary>
            GreaterThanOrEqualTo = 3,
            /// <summary>
            /// 大于
            /// </summary>
            GreaterThan = 4,
            /// <summary>
            /// 小于等于
            /// </summary>
            LessThanOrEqualTo = 5,
            /// <summary>
            /// 小于
            /// </summary>
            LessThan = 6,
            /// <summary>
            /// 包含
            /// </summary>
            Contains = 7,
            /// <summary>
            /// 不包含
            /// </summary>
            DoesNotContain = 8
        }

        public enum FuncTypeEnum
        {
            /// <summary>
            /// 执行脚本
            /// </summary>
            ExecuteJs = 0,
     
            /// <summary>
            /// 导出excel
            /// </summary>
            ExportExcel = 1, 
            /// <summary>
            /// 导出pdf
            /// </summary>
            ExportPdf = 2,
            
            _self = 3,
            _blank = 4,

           
            Save = 5,
            /// <summary>
            /// 删除
            /// </summary>
            Delete = 6,
            TopOpenNewModel = 7,
            TopOpenNewModelAndRefresh = 8,
            ParentOpenNewModel = 9,
            ParentOpenNewModelAndRefresh = 10,
            OpenNewModel = 11,
                OpenNewModelAndRefresh = 12
        }
    }
}
