using CPFrameWork.Global;
using CPFrameWork.Utility.DbOper;
using System;
using System.Collections.Generic; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.UIInterface.Form
{
    #region FormMain
    public class CPForm:BaseEntity
    {
        /// <summary>
        /// 表单唯一code，页面地址里使用这个
        /// </summary>
        public string FormCode { get; set; }
        /// <summary>
        /// 流水号
        /// </summary>
        public int? AutoIndex { get; set; }
        /// <summary>
        /// 表单标题
        /// </summary>
        public string FormTitle { get; set; }
        /// <summary>
        /// 所属子系统
        /// </summary>
        public int? SysId { get; set; }

        /// <summary>
        /// 所属业务功能，用户自行输入或选择，主要是用来将某一个功能的相关配置都组合到一起，便于后面的查找或修改
        /// </summary>
        public string FuncTitle { get; set; }

        /// <summary>
        /// 表数据链接实例
        /// </summary>
        public string DbIns { get; set; }

        /// <summary>
        /// 表所在的库名
        /// </summary>
        public string DbName { get; set; }

        /// <summary>
        /// 主表表名
        /// </summary>
        public string MainTableName { get; set; }

        /// <summary>
        /// 主键字段名，目前只支持单主键
        /// </summary>
        public string PKFieldName { get; set; }

        public CPFormEnum.PKValueTypeEnum? PKValueType { get; set; }

        /// <summary>
        /// 关联子表
        /// </summary>
        public List<CPFormChildTable> ChildTableCol { get; set; }
        /// <summary>
        /// 字段集合
        /// </summary>
        public List<CPFormField> FieldCol { get; set; }
        /// <summary>
        /// 视图集合
        /// </summary>
        public List<CPFormView> ViewCol { get; set; }
        /// <summary>
        /// 表单应用场景集合
        /// </summary>
        public List<CPFormUseScene> UseSceneCol { get; set; }
        /// <summary>
        /// 组集合
        /// </summary>
        public List<CPFormGroup> GroupCol { get; set; }
        /// <summary>
        /// 规则集合
        /// </summary>
        public List<CPFormFieldRule> FieldRuleCol { get; set; }

        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.PKValueType.HasValue == false)
                this.PKValueType = CPFormEnum.PKValueTypeEnum.IntSelfIncreasing;
            if (this.AutoIndex.HasValue == false)
                this.AutoIndex = 0;
        }
    }


    
    #endregion


    #region CPFormChildTable
    public class CPFormChildTable : BaseEntity
    {
        /// <summary>
        /// 表单ID
        /// </summary>
        public int FormId { get; set; }

        /// <summary>
        /// 子表表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 主键字段名，目前只支持单主键
        /// </summary>
        public string PKFieldName { get; set; }
        /// <summary>
        /// 主键值类型
        /// </summary>
        public CPFormEnum.PKValueTypeEnum? PKValueType { get; set; }

        /// <summary>
        /// 与主表关联字段名，目前只支持单 字段关联
        /// </summary>
        public string RelateFieldName { get; set; }
        /// <summary>
        /// 数据关系类型
        /// </summary>
        public CPFormEnum.RelateTypeEnum? RelateType { get; set; }
        /// <summary>
        /// 是否级联删除
        /// </summary>
        public bool? RelateDel { get; set; }
        /// <summary>
        /// 当这几个字段值为空时，则不保存子表的这行数据 多个字段用,分隔
        /// </summary>
        public string FieldNullNotSaveData { get; set; }

        /// <summary>
        /// 主表单
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPForm Form { get; set; }

        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.PKValueType.HasValue == false)
                this.PKValueType = CPFormEnum.PKValueTypeEnum.IntSelfIncreasing;
            if (this.RelateType.HasValue == false)
                this.RelateType = CPFormEnum.RelateTypeEnum.OneToMore;
            if (this.RelateDel.HasValue == false)
                this.RelateDel = true;
        }
    }

     
    #endregion


    #region CPFormField
    public class CPFormField : BaseEntity
    {
        /// <summary>
        /// 表单ID
        /// </summary>
        public int FormId { get; set; }
        /// <summary>
        /// 为了配置展现方便，考虑把子表也作为一个特殊 的字段来处理，如果 是子表，则值为true，否则为false
        /// </summary>
        public bool? IsChildTable { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 字段名  ，如果是子表，则此字段为空
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 存储值的字段名，一般如下拉 列表，组织机构选择控件等会用到
        /// </summary>
        public string FieldValueName { get; set; }

        /// <summary>
        /// 显示标题
        /// </summary>
        public string FieldTitle { get; set; }
        /// <summary>
        /// 控件类型
        /// </summary>
        public CPFormEnum.ControlTypeEnum? ControlType { get; set; }

        /// <summary>
        /// 帮助提示信息
        /// </summary>
        public string FieldTip { get; set; }
        /// <summary>
        /// 控件placeholder提示信息 ，默认为请输入
        /// </summary>
        public string FieldPlaceHolder { get; set; }
        /// <summary>
        /// 是否允许为空
        /// </summary>
        public bool? IsAllowNull { get; set; }

        /// <summary>
        /// 不允许为空时，如果为空的提示信息
        /// </summary>
        public string NotAllowNullTip { get; set; }
        /// <summary>
        /// 值数据类型
        /// </summary>
        public CPEnum.FieldValueTypeEnum? FieldValueType { get; set; }
        /// <summary>
        /// 字段值长度
        /// </summary>
        public int FieldValueLength { get; set; }

        /// <summary>
        /// 控件类型为单选、复选、下拉列表、组合框时，数据源数据库链接实例名称
        /// </summary>
        public string ListDbIns { get; set; }
        /// <summary>
        /// 控件类型为单选、复选、下拉列表、组合框时，数据源SQL语句，必须返回两个以上字段，第一个为 Text,第二个为Value
        /// </summary>
        public string ListSql { get; set; }

        ///// <summary>
        ///// 如果是下拉列表或组合框，联动时，上述SQL语句中使用哪个字段过滤数据，配置字段名
        ///// </summary>
        //public string ListRelateByField { get; set; }

        /// <summary>
        /// 如果是下拉列表或组合框，联动时，要根据哪个字段的值过滤，这里配置字段名
        /// </summary>
        public string ListRelateTargetField { get; set; }

        /// <summary>
        /// 时间选择格式字符串：        YYYY-MM-DD YYYY-MM-DD hh:mm:ss   YYYY年MM月DD日 hh时mm分ss秒      YYYY年MM月   更多可以layer ui控件的使用方法
        /// </summary>
        public string TimeFormat { get; set; }
        /// <summary>
        /// 组织机构选择是否允许多选，
        /// </summary>
        public bool? OrganIsCanMultiSel { get; set; }

        /// <summary>
        /// 可上传附件类型
        /// </summary>
        public string FileAllowType { get; set; }
        /// <summary>
        /// 可以上传文件的个数，0表示不限制
        /// </summary>
        public int? FileAllowCount { get; set; }


        /// <summary>
        /// 字段状态：
        /// </summary>
        public CPFormEnum.FieldStatusEnum? FieldStatus { get; set; }

        /// <summary>
        /// 控件标题列所占宽度百分比，默认20 
        /// </summary>
        public int? FieldTitleShowWidth { get; set; }

        /// <summary>
        /// 控件显示宽度，默认值为98%
        /// </summary>
        public string ShowWidth { get; set; }
        /// <summary>
        /// 多行文本框 行数
        /// </summary>
        public int? MultiRows { get; set; }
        /// <summary>
        /// 单位，像素，像编辑器控件会到
        /// </summary>
        public int? ShowHeight { get; set; }
        /// <summary>
        /// 扩展html，用来显示在控件的后面
        /// </summary>
        public string HtmlEx { get; set; }

        /// <summary>
        /// 控件后面添加的扩展按钮图标样式名称
        /// </summary>
        public string ExButtonIcon { get; set; }

        /// <summary>
        /// 控件后面添加的扩展按钮图标Tip
        /// </summary>
        public string ExButtonTip { get; set; }

        /// <summary>
        /// 控件后面添加的扩展按钮图标，点击执行的扩展方法，支持表达式
        /// </summary>
        public string ExButtonClickMethod { get; set; }

        /// <summary>
        /// 扩展事件名称，输入类似于如下值：        Onclick        Onchange   ondblclick
        /// </summary>
        public string EventName1 { get; set; }

        /// <summary>
        /// 事件执行的方法
        /// </summary>
        public string EventMethod1 { get; set; }

        /// <summary>
        /// 扩展事件名称，输入类似于如下值：        Onclick        Onchange   ondblclick
        /// </summary>
        public string EventName2 { get; set; }

        /// <summary>
        /// 事件执行的方法
        /// </summary>
        public string EventMethod2 { get; set; }

        /// <summary>
        /// 对应表单
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPForm Form { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.IsChildTable.HasValue == false)
                this.IsChildTable = false;
            if (this.ControlType.HasValue == false)
                this.ControlType = CPFormEnum.ControlTypeEnum.TextBox;
            if (this.IsAllowNull.HasValue == false)
                this.IsAllowNull = true;
            if (this.FieldValueType.HasValue == false)
                this.FieldValueType = CPEnum.FieldValueTypeEnum.String;
            if (this.OrganIsCanMultiSel.HasValue == false)
                this.OrganIsCanMultiSel = true;
            if (this.FileAllowCount.HasValue == false)
                this.FileAllowCount = 0;
            if (string.IsNullOrEmpty(this.FieldPlaceHolder))
                this.FieldPlaceHolder = "请输入";
            if (string.IsNullOrEmpty(FileAllowType))
                this.FileAllowType = "";


            if (this.FieldStatus.HasValue == false)
                this.FieldStatus = CPFormEnum.FieldStatusEnum.Edit;
            if (this.MultiRows.HasValue == false)
                this.MultiRows = 5;
            if (this.ShowHeight.HasValue == false)
                this.ShowHeight = 500;
            if (this.FieldTitleShowWidth.HasValue == false)
                this.FieldTitleShowWidth = 20;
        }
    }

     
    #endregion


    #region CPFormView
    public class CPFormView : BaseEntity
    {
        /// <summary>
        /// 表单ID
        /// </summary>
        public int FormId { get; set; }

        /// <summary>
        /// 视图Code
        /// </summary>
        public string ViewCode { get; set; }
        /// <summary>
        /// 流水号
        /// </summary>
        public int? AutoIndex { get; set; }

        /// <summary>
        /// 视图名称
        /// </summary>
        public string ViewName { get; set; }

        /// <summary>
        /// 视图类型  内置视图_一行两列式：1        内置视图_一行一列式:2   使用编辑器排版视图：3
        /// </summary>
        public CPFormEnum.ViewTypeEnum? ViewType { get; set; }
        /// <summary>
        /// 视图适配设备类型：
        /// </summary>
        public CPFormEnum.ViewDeviceTypeEnum? ViewDeviceType { get; set; }

        /// <summary>
        /// 如果是内置视图，考虑实现表单可以分组展现，这里配置各个分组的标题名称，多个用，分隔,形如：        基本信息,权限信息 
        /// </summary>
        public string ViewBlockTitle { get; set; }
        /// <summary>
        /// 使用编辑器排版时，存储模板HTML
        /// </summary>
        public string FormViewHTML { get; set; }
        /// <summary>
        /// 是否是默认视图，有且仅有一个默认视图
        /// </summary>
        public bool? IsDefault { get; set; }

        /// <summary>
        /// 对应表单
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPForm Form { get; set; }

        public List<CPFormViewField> ViewFieldCol { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.ViewType.HasValue == false)
                this.ViewType = CPFormEnum.ViewTypeEnum.TwoColumn;
            if (this.ViewDeviceType.HasValue == false)
                this.ViewDeviceType = CPFormEnum.ViewDeviceTypeEnum.PCIphoneIpad;
            if (this.IsDefault.HasValue == false)
                this.IsDefault = false;
            if (this.AutoIndex.HasValue == false)
                this.AutoIndex = 0;


        }
    }


    
    #endregion

    #region CPFormViewField
    public class CPFormViewField : BaseOrderEntity
    {
        /// <summary>
        /// 表单ID
        /// </summary>
        public int FormId { get; set; }
        /// <summary>
        /// 视图ID
        /// </summary>
        public int ViewId { get; set; }
        /// <summary>
        /// 字段ID
        /// </summary>
        public int FieldId { get; set; }
        /// <summary>
        /// 进行分组展现时，此字段所属组号，默认是0
        /// </summary>
        public int? ViewBlockIndex { get; set; }
        ///// <summary>
        ///// 字段状态：
        ///// </summary>
        //public CPFormEnum.FieldStatusEnum? FieldStatus { get; set; }
        /// <summary>
        /// 是否通栏显示，即一行只显示本控件
        /// </summary>
        public bool? IsSpanAll { get; set; }

        ///// <summary>
        ///// 控件标题列所占宽度百分比，默认20 
        ///// </summary>
        //public int? FieldTitleShowWidth { get; set; }

        ///// <summary>
        ///// 控件显示宽度，默认值为98%
        ///// </summary>
        //public string ShowWidth { get; set; }
        ///// <summary>
        ///// 多行文本框 行数
        ///// </summary>
        //public int? MultiRows { get; set; }
        ///// <summary>
        ///// 单位，像素，像编辑器控件会到
        ///// </summary>
        //public int? ShowHeight { get; set; }
        ///// <summary>
        ///// 扩展html，用来显示在控件的后面
        ///// </summary>
        //public string HtmlEx { get; set; }

        ///// <summary>
        ///// 控件后面添加的扩展按钮图标样式名称
        ///// </summary>
        //public string ExButtonIcon { get; set; }

        ///// <summary>
        ///// 控件后面添加的扩展按钮图标Tip
        ///// </summary>
        //public string ExButtonTip { get; set; }

        ///// <summary>
        ///// 控件后面添加的扩展按钮图标，点击执行的扩展方法，支持表达式
        ///// </summary>
        //public string ExButtonClickMethod { get; set; }

        ///// <summary>
        ///// 扩展事件名称，输入类似于如下值：        Onclick        Onchange   ondblclick
        ///// </summary>
        //public string EventName1 { get; set; }

        ///// <summary>
        ///// 事件执行的方法
        ///// </summary>
        //public string EventMethod1 { get; set; }

        ///// <summary>
        ///// 扩展事件名称，输入类似于如下值：        Onclick        Onchange   ondblclick
        ///// </summary>
        //public string EventName2 { get; set; }

        ///// <summary>
        ///// 事件执行的方法
        ///// </summary>
        //public string EventMethod2 { get; set; }


        /// <summary>
        /// 表单视图
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPFormView FormView { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.ViewBlockIndex.HasValue == false)
                this.ViewBlockIndex = 0; 
            if (this.IsSpanAll.HasValue == false)
                this.IsSpanAll = false; 
        }
    }

     
    #endregion


    #region CPFormUseScene
    public class CPFormUseScene : BaseEntity
    {
        /// <summary>
        /// 表单ID
        /// </summary>
        public int FormId { get; set; }
        /// <summary>
        /// 场景Code
        /// </summary>
        public string SceneCode { get; set; }
        /// <summary>
        /// 流水号
        /// </summary>
        public int? AutoIndex { get; set; }

        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName { get; set; }
         

        /// <summary>
        /// 表单加载扩展事件类
        /// </summary>
        public string FormLoadHandler { get; set; }

        /// <summary>
        /// 表单保存前，客户端扩展方法
        /// </summary>
        public string BeforeFormClientSave { get; set; }

        /// <summary>
        /// 表单保存扩展事件类
        /// </summary>
        public string FormSaveHandler { get; set; }
        /// <summary>
        /// 表单保存后扩展SQL，获取某个字段的值，使用{$表名.字段名$},也支持表达式，先执行SQL，再执行扩展类
        /// </summary>
        public string FormSaveExeSql { get; set; }
        /// <summary>
        /// 表单扩展脚本
        /// </summary>
        public string FormScript { get; set; }
        /// <summary>
        /// 表单保存完后的操作类型：
        /// </summary>
        public CPFormEnum.FormSavedActionEnum? FormSavedAction { get; set; }

        /// <summary>
        /// 表单保存完后，对于4,5，配置相关扩展信息
        /// </summary>
        public string FormSavedInfo { get; set; }

        /// <summary>
        /// 表单对象
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPForm Form { get; set; }
        /// <summary>
        /// 功能集合
        /// </summary>
        public List<CPFormUseSceneFunc> FuncCol { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.FormSavedAction.HasValue == false)
                this.FormSavedAction = CPFormEnum.FormSavedActionEnum.CloseDiv;
            if (this.AutoIndex.HasValue == false)
                this.AutoIndex = 0;
        }
    }

     
    #endregion

    #region CPFormUseSceneFunc
    public class CPFormUseSceneFunc : BaseOrderEntity
    {
        /// <summary>
        /// 表单ID
        /// </summary>
        public int FormId { get; set; }
        /// <summary>
        /// 表单使用场景Id
        /// </summary>
        public int SceneID { get; set; }

        /// <summary>
        /// 按钮名称
        /// </summary>
        public string FuncTitle { get; set; }

        /// <summary>
        /// 按钮图标
        /// </summary>
        public string FuncIcon { get; set; }

        /// <summary>
        /// 执行脚本。
        /// </summary>
        public string FuncExeJS { get; set; }
        /// <summary>
        /// 按钮显示规则
        /// </summary>
        public CPFormEnum.FuncIsShowInViewEnum? FuncIsShowInView { get; set; }
        /// <summary>
        /// 是否受权限控制
        /// </summary>
        public bool? IsControlByRight { get; set; }
        /// <summary>
        ///  拥有此权限角色名，多个，分隔
        /// </summary>
        public string RoleNames { get; set; }
        /// <summary>
        ///  拥有此权限角色名ID，多个，分隔
        /// </summary>
        public string RoleIds { get; set; }

        /// <summary>
        /// 所属应用场景 
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPFormUseScene UseScene { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.FuncIsShowInView.HasValue == false)
                this.FuncIsShowInView = CPFormEnum.FuncIsShowInViewEnum.ShowAll;
            if (this.IsControlByRight.HasValue == false)
                this.IsControlByRight = false;

        }
    }

     
    #endregion

    #region CPFormGroup
    public class CPFormGroup : BaseEntity
    {
        /// <summary>
        /// 表单ID
        /// </summary>
        public int FormId { get; set; }
        /// <summary>
        /// 组code
        /// </summary>
        public string GroupCode { get; set; }
        /// <summary>
        /// 流水号
        /// </summary>
        public int? AutoIndex { get; set; }

        /// <summary>
        /// 组名称
        /// </summary> 
        public string GroupName { get; set; } 
        /// <summary>
        /// 组类型
        /// </summary>
        public CPFormEnum.GroupTypeEnum? GroupType { get; set; }

        /// <summary>
        /// 表单对象
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPForm Form { get; set; } 

        /// <summary>
        /// 字段权限集合
        /// </summary>
        public List<CPFormFieldRight> FieldRightCol { get; set; }
        /// <summary>
        /// 字段初始化集合
        /// </summary>
        public List<CPFormFieldInit> FieldInitCol { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.GroupType.HasValue == false)
                this.GroupType = CPFormEnum.GroupTypeEnum.Init;
            if (this.AutoIndex.HasValue == false)
                this.AutoIndex = 0;
        }
    }

     
    #endregion


    #region CPFormRight
    public class CPFormFieldRight : BaseEntity
    {
        /// <summary>
        /// 表单ID
        /// </summary>
        public int FormId { get; set; }
        /// <summary>
        /// 组Id
        /// </summary>
        public int? GroupID { get; set; }
        /// <summary>
        /// 字段ID
        /// </summary>
        public int? FieldId { get; set; }
        /// <summary>
        /// 权限类别
        /// </summary>
        public CPFormEnum.AccessTypeEnum? AccessType { get; set; }
        /// <summary>
        /// 权限作用范围        所有用户：1 指定部分用户：2
        /// </summary>
        public CPFormEnum.AccessScoreTypeEnum? AccessScoreType { get; set; }
        /// <summary>
        ///  拥有此权限角色名，多个，分隔
        /// </summary>
        public string RoleNames { get; set; }
        /// <summary>
        ///  拥有此权限角色名ID，多个，分隔
        /// </summary>
        public string RoleIds { get; set; }
        /// <summary>
        ///  拥有此权限部门名，多个，分隔
        /// </summary>
        public string DepNames { get; set; }
        /// <summary>
        ///  拥有此权限部门ID，多个，分隔
        /// </summary>
        public string DepIds { get; set; }
        /// <summary>
        ///  拥有此权限用户名，多个，分隔
        /// </summary>
        public string UserNames { get; set; }
        /// <summary>
        ///  拥有此权限用户ID，多个，分隔
        /// </summary>
        public string UserIds { get; set; }
        /// <summary>
        /// 所属权限 组
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPFormGroup RightGroup { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.AccessType.HasValue == false)
                this.AccessType = CPFormEnum.AccessTypeEnum.Edit;
            if (this.AccessScoreType.HasValue == false)
                this.AccessScoreType = CPFormEnum.AccessScoreTypeEnum.AllUser;
        }
    }


   
    #endregion


    #region CPFormInit
    public class CPFormFieldInit : BaseEntity
    {
        /// <summary>
        /// 表单ID
        /// </summary>
        public int FormId { get; set; }
        /// <summary>
        /// 组Id
        /// </summary>
        public int? GroupID { get; set; }
        /// <summary>
        /// 字段ID
        /// </summary>
        public int? FieldId { get; set; }
        /// <summary>
        /// 初始化条件类型
        /// </summary>
        public CPFormEnum.InitTimeTypeEnum? InitTimeType { get; set; }

        /// <summary>
        /// 对于表达式和类库条件初始化，配置表达式和类库信息
        /// </summary>
        public string InitTimeDetail { get; set; }
        /// <summary>
        /// 初始化类型
        /// </summary>
        public CPFormEnum.InitTypeEnum? InitType { get; set; }
        /// <summary>
        /// 上述表达式的详细信息存储于此字段中
        /// </summary>
        public string InitInfo { get; set; }
        /// <summary>
        /// /Sql语句初始化时，配置对应的数据库实例 。
        /// </summary>
        public string InitSqlDbIns { get; set; }
        /// <summary>
        /// 所属初始 组
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPFormGroup InitGroup { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.InitTimeType.HasValue == false)
                this.InitTimeType = CPFormEnum.InitTimeTypeEnum.All;
            if (this.InitType.HasValue == false)
                this.InitType = CPFormEnum.InitTypeEnum.Expression;
        }
    }


    
    #endregion



    #region CPFormFieldRule
    public class CPFormFieldRule : BaseEntity
    {
        /// <summary>
        /// 表单ID
        /// </summary>
        public int FormId { get; set; }
        /// <summary>
        /// 字段ID
        /// </summary>
        public int? FieldId { get; set; }
        /// <summary>
        /// 规则类型        数据校验：1 设置某个控件禁用：2设置某个字段值：3
        /// </summary> 

        public CPFormEnum.RuleTypeEnum? RuleType { get; set; }
        /// <summary>
        /// 条件表达式
        /// </summary>
        public string RuleCondition { get; set; }
        /// <summary>
        /// 如果是数据校验，则存储不满足条件时提示信息        如果是设置某个控件隐藏或禁用，则存储目标字段FieldId        如果是设置某个字段值，则存储设置值的表达式
        /// </summary>
        public string RuleTargetOpertion { get; set; }
        /// <summary>
        /// 所属表单
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPForm Form { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.RuleType.HasValue == false)
                this.RuleType = CPFormEnum.RuleTypeEnum.DataCheck; 
        }
    }

    　
    #endregion
}
