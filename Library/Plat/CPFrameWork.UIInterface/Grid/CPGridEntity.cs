using CPFrameWork.Utility.DbOper;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPFrameWork.UIInterface.Grid
{
    #region CPGrid
    public class CPGrid : BaseEntity
    {
        /// <summary>
        /// 列表唯一code，页面地址里使用这个
        /// </summary>
        public string GridCode { get; set; }
        /// <summary>
        /// 列表编号流水号
        /// </summary>
        public int? AutoIndex { get; set; }
        /// <summary>
        /// 列表标题
        /// </summary>
        public string GridTitle { get; set; }
        /// <summary>
        /// 所属子系统
        /// </summary>
        public int? SysId { get; set; }
        /// <summary>
        /// 所属业务功能，用户自行输入或选择，主要是用来将某一个功能的相关配置都组合到一起，便于后面的查找或修改
        /// </summary>
        public string FuncTitle { get; set; }
        /// <summary>
        /// 列表类型
        /// </summary>
        public CPGridEnum.GridTypeEnum? GridType { get; set; }
        /// <summary>
        /// 表数据链接实例
        /// </summary>
        public string DbIns { get; set; }
        /// <summary>
        /// 数据源类型
        /// </summary>
        public CPGridEnum.DataSourceTypeEnum? DataSourceType { get; set; }
        /// <summary>
        /// 数据源内容，支持表达式
        /// </summary>
        public string DataSource { get; set; }
        /// <summary>
        /// 主表表名
        /// </summary>
        public string MainTableName { get; set; }
        /// <summary>
        /// 主键字段名，多个用，分隔
        /// </summary>
        public string PKFieldName { get; set; }
        /// <summary>
        /// 主键值类型
        /// </summary>
        public CPGridEnum.PKValueTypeEnum? PKValueType { get; set; }
        /// <summary>
        /// 删除数据的sql语句，用于需要删除多表时用
        /// </summary>
        public string DelDataSql { get; set; }
        /// <summary>
        /// 默认排序，多个用,分隔 ，如f1 asc,f2 desc
        /// </summary>
        public string DataOrder { get; set; }
        /// <summary>
        /// 是否分页展现数据
        /// </summary>
        public bool? IsPage { get; set; }
        /// <summary>
        /// 默认每页显示条数
        /// </summary>
        public int? PageSize { get; set; }
        /// <summary>
        /// 数据是否分组显示
        /// </summary>
        public bool? IsGroup { get; set; }
        /// <summary>
        /// 默认分组字段，多个使用,分隔
        /// </summary>
        public string GroupField { get; set; }
        /// <summary>
        /// 分组描述
        /// </summary>
        public string GroupAlias { get; set; }
        /// <summary>
        /// 默认分组排序方式
        /// </summary>
        public CPGridEnum.GroupSortEnum? GroupSort { get; set; }
        /// <summary>
        /// 是否显示自定义分组区域
        /// </summary>
        public bool? IsShowGroupArea { get; set; }
        /// <summary>
        /// 锁定列的数量，从左开始算
        /// </summary>
        public int? LockColumnCount { get; set; }
        /// <summary>
        /// 默认是否加载数据
        /// </summary>
        public bool? IsLoadData { get; set; }
        /// <summary>
        /// 是否启用缓存
        /// </summary>
        public bool? IsCache { get; set; }
        /// <summary>
        /// Cache是否关联用户
        /// </summary>
        public bool? IsCacheRelateUser { get; set; }
        /// <summary>
        /// 扩展脚本
        /// </summary>
        public string JsEx { get; set; }
        /// <summary>
        /// 扩展样式
        /// </summary>
        public string CSSEx { get; set; }
        /// <summary>
        /// 查询功能展现模式 1：快速查询 2:展开排列式查询
        /// </summary>
        public CPGridEnum.SearchModelEnum? SearchModel { get; set; }
        /// <summary>
        /// 数据统计方式
        /// </summary>
        public CPGridEnum.DataSumTypeEnum? DataSumType { get; set; }
        /// <summary>
        /// 数据统计期间：
        /// </summary>
        public CPGridEnum.DataTimeSumTypeEnum? DataTimeSumType { get; set; }
        public string DataTimeSumField { get; set; }
        public string BeforeGridLoad { get; set; }

        public List<CPGridColumn> ColumnCol { get; set; }
        public List<CPGridFunc> FuncCol { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.SysId.HasValue == false)
                this.SysId = 1;
            if (this.GridType.HasValue == false)
                this.GridType = CPGridEnum.GridTypeEnum.PC;
            if (this.DataSourceType.HasValue == false)
                this.DataSourceType = CPGridEnum.DataSourceTypeEnum.Sql;
            if (this.PKValueType.HasValue == false)
                this.PKValueType = CPGridEnum.PKValueTypeEnum.IntSelfIncreasing;
            if (this.IsPage.HasValue == false)
                this.IsPage = true;
            if (this.PageSize.HasValue == false)
                this.PageSize = 20;
            if (this.IsGroup.HasValue == false)
                this.IsGroup = false;
            if (this.GroupSort.HasValue == false)
                this.GroupSort = CPGridEnum.GroupSortEnum.asc;
            if (this.IsShowGroupArea.HasValue == false)
                this.IsShowGroupArea = false;
            if (this.LockColumnCount.HasValue == false)
                this.LockColumnCount = 0;
            if (this.IsLoadData.HasValue == false)
                this.IsLoadData = true;
            if (this.IsCache.HasValue == false)
                this.IsCache = false;
            if (this.IsCacheRelateUser.HasValue == false)
                this.IsCacheRelateUser = false;
            if (this.SearchModel.HasValue == false)
                this.SearchModel = CPGridEnum.SearchModelEnum.QuickSearch;

            if (this.DataSumType.HasValue == false)
                this.DataSumType = CPGridEnum.DataSumTypeEnum.AllData;
            if (this.DataTimeSumType.HasValue == false)
                this.DataTimeSumType = CPGridEnum.DataTimeSumTypeEnum.None;
        }
    }



    #endregion

    #region CPGridColumn
    public class CPGridColumn : BaseOrderEntity
    {

        /// <summary>
        /// 所属列表ID
        /// </summary>
        public int GridId { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string ColumnTitle { get; set; }
        /// <summary>
        /// 第一级复杂表头标题
        /// </summary>
        public string ColumnTitleGroup1 { get; set; }
        /// <summary>
        /// 第二级复杂表头标题，最多运行两级复杂表头
        /// </summary>
        public string ColumnTitleGroup2 { get; set; }
        /// <summary>
        /// 对应数据库字段
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// 当列修改类型为下拉列表时，则配置下拉列表值字段，FieldName则是显示字段
        /// </summary>
        public string FieldValueName { get; set; }
        /// <summary>
        /// 列类型
        /// </summary>
        public CPGridEnum.ColumnTypeEnum? ColumnType { get; set; }
        /// <summary>
        /// ColumnTypeEnum
        /// </summary>
        public bool? IsShow { get; set; }
        /// <summary>
        /// 显示宽度
        /// </summary>
        public int? ShowWidth { get; set; }
        /// <summary>
        /// 最多显示字符数
        /// </summary>
        public int? MaxString { get; set; }
        /// <summary>
        /// 当列类型为模板列时，配置模板html
        /// </summary>
        public string TemplateContent { get; set; }
        /// <summary>
        ///  是否可以排序
        /// </summary>
        public bool? IsCanOrder { get; set; }
        /// <summary>
        /// 数据相同时，是否合并行
        /// </summary>
        public bool? IsMergeRow { get; set; }
        /// <summary>
        /// 显示位置
        /// </summary>
        public CPGridEnum.ShowPositionEnum? ShowPosition { get; set; }
        /// <summary>
        /// 时间字段类型的格式化方式，参考.net下方式做
        /// </summary>
        public string TimeFormat { get; set; }
        /// <summary>
        /// 数字类型格式化方式，参考.net下方式做
        /// </summary>
        public string NumberFormat { get; set; }
        /// <summary>
        /// 数字类型格式化方式，参考.net下方式做
        /// </summary>
        public bool? IsSearchShow { get; set; }
        /// <summary>
        /// 查询显示顺序
        /// </summary>
        public int? SearchShowOrder { get; set; }
        /// <summary>
        /// 查询显示类型,主要针对排列式查询时起作用
        /// </summary>
        public CPGridEnum.SearchShowTypeEnum? SearchShowType { get; set; }
        /// <summary>
        /// 当查询类型为下拉式查询或编辑类型为下拉框时，配置数据源数据库链实例
        /// </summary>
        public string FieldEnumDataIns { get; set; }
        /// <summary>
        /// 当查询类型为下拉式查询或编辑类型为下拉框时，配置数据源SQL语句，支持表达式
        /// </summary>
        public string FieldEnumDataSource { get; set; }
        /// <summary>
        /// 是否可以导出
        /// </summary>
        public bool? IsCanExport { get; set; }
        /// <summary>
        /// 是否显示统计值 
        /// </summary>
        public bool? IsShowSum { get; set; }
        /// <summary>
        /// 字段统计方式，为空表示不统计，存储以下值：
        /// </summary>
        public string SumType { get; set; }
        /// <summary>
        /// 当列类型为图片超链接或文字超链接时，配置图片地址或文字，采用矢量图标
        /// </summary>
        public string ColumnIconOrText { get; set; }
        /// <summary>
        /// 当列类型为图片超链接或文字超链接时，配置显示颜色
        /// </summary>
        public string ColumnIconOrTextColor { get; set; }
        /// <summary>
        /// 链接列跳转方式
        /// </summary>
        public CPGridEnum.TargetTypeEnum? TargetType { get; set; }
        public string TargetContent { get; set; }
        public int? OpenWinWidth { get; set; }
        public int? OpenWinHeight { get; set; }
        public bool? IsUseExpressionShow { get; set; }
        public string LeftExpression { get; set; }
        public CPGridEnum.ShowMethodEnum? ShowMethod { get; set; }
        public string RightExpression { get; set; }
        /// <summary>
        /// 根据条件控制列是否显示
        /// </summary>
        public bool? IsShowByExpression{get;set;}
        /// <summary>
        /// 扩展事件名称，输入类似于如下值：        Onclick        Onchange   ondblclick
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// 事件执行的方法
        /// </summary>
        public string EventMethod { get; set; }

       /// <summary>
       /// 为列表列统计值定义的临时参数
       /// </summary>
        public string  TempSumValue { get; set; }

        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPGrid Grid { get; set; }

        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.ColumnType.HasValue == false)
                this.ColumnType = CPGridEnum.ColumnTypeEnum.Normal;
            if (this.IsShow.HasValue == false)
                this.IsShow = true;
            if (this.ShowOrder.HasValue == false)
                this.ShowOrder = 10;
            if (this.ShowWidth.HasValue == false)
                this.ShowWidth = 10;
            if (this.MaxString.HasValue == false) this.MaxString = 0;
            if (this.IsCanOrder.HasValue == false)
                this.IsCanOrder = true;
            if (this.IsMergeRow.HasValue == false)
                this.IsMergeRow = false;
            if (this.ShowPosition.HasValue == false)
                this.ShowPosition = CPGridEnum.ShowPositionEnum.Left;
            if (this.IsSearchShow.HasValue == false)
                this.IsSearchShow = true;
            if (this.SearchShowOrder.HasValue == false)
                this.SearchShowOrder = 10;
            if (this.SearchShowType.HasValue == false)
                this.SearchShowType = CPGridEnum.SearchShowTypeEnum.Textbox;
            if (this.IsCanExport.HasValue == false)
                this.IsCanExport = true;
            if (this.IsShowSum.HasValue == false)
                this.IsShowSum = false;
            if (this.TargetType.HasValue == false)
                this.TargetType = CPGridEnum.TargetTypeEnum.OpenNewModel;
            if (this.OpenWinWidth.HasValue == false)
                this.OpenWinWidth = 1024;
            if (this.OpenWinHeight.HasValue == false)
                this.OpenWinHeight = 760;
            if (this.IsUseExpressionShow.HasValue == false)
                this.IsUseExpressionShow = false;
            if (this.ShowMethod.HasValue == false)
                this.ShowMethod = CPGridEnum.ShowMethodEnum.Contains;
            if (string.IsNullOrEmpty(this.ColumnTitleGroup1))
                this.ColumnTitleGroup1 = "";
            if (string.IsNullOrEmpty(this.ColumnTitleGroup2))
                this.ColumnTitleGroup2 = "";
            if (this.IsShowByExpression.HasValue == false)
                this.IsShowByExpression = true;
        }

       
    }



    #endregion

    #region CPGridFunc
    public class CPGridFunc : BaseEntity
    {
        /// <summary>
        /// 所属列表ID
        /// </summary>
        public int GridId { get; set; }
        /// <summary>
        /// 功能标题
        /// </summary>
        public string FuncTitle { get; set; }
        /// <summary>
        /// 显示顺序
        /// </summary>
        public int? FuncOrder { get; set; }
        /// <summary>
        /// 功能矢量图标
        /// </summary>
        public string FuncIcon { get; set; }
        /// <summary>
        /// 如果是列表内置按钮，则存储按钮类型
        /// </summary>
        public CPGridEnum.FuncTypeEnum? FuncType { get; set; }
        /// <summary>
        /// 目标页面或自定义方法，支持表达式
        /// </summary>
        public string FuncContext { get; set; }
        /// <summary>
        /// 新窗口的宽度
        /// </summary>
        public int? OpenWinWidth { get; set; }
        /// <summary>
        /// 新窗口的高度
        /// </summary>
        public int? OpenWinHeight { get; set; }
        /// <summary>
        /// 链接弹出窗口的标题
        /// </summary>
        public string FuncOpenWinTitle { get; set; }
        /// <summary>
        /// 是否启用根据条件来判断列是否显示\
        /// </summary>
        public bool? IsUseExpressionShow { get; set; }
        /// <summary>
        /// 用来比较是否显示的原值表达式
        /// </summary>
        public string LeftExpression { get; set; }
        /// <summary>
        /// 比较方法
        /// </summary>
        public CPGridEnum.ShowMethodEnum? ShowMethod { get; set; }
        /// <summary>
        /// /用来比较是否显示的目标值表达式
        /// </summary>
        public string RightExpression { get; set; }
        /// <summary>
        /// 条件为真时是否显示
        /// </summary>
        public bool? IsShowByExpression { get; set; }

        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPGrid Grid { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.FuncOrder.HasValue == false)
                this.FuncOrder = 10;
            if (this.FuncType.HasValue == false)
                this.FuncType = CPGridEnum.FuncTypeEnum.ExecuteJs;
            if (this.OpenWinWidth.HasValue == false)
                this.OpenWinWidth = 1024;
            if (this.OpenWinHeight.HasValue == false)
                this.OpenWinHeight = 760;
            if (this.IsUseExpressionShow.HasValue == false)
                this.IsUseExpressionShow = false;
            if (this.ShowMethod.HasValue == false)
                this.ShowMethod = CPGridEnum.ShowMethodEnum.Contains;
            if (this.IsShowByExpression.HasValue == false)
                this.IsShowByExpression = true;
        }
    }



    #endregion
}
