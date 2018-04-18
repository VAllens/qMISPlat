using CPFrameWork.Utility.DbOper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPFrameWork.UIInterface.Tree
{
    public class CPTreeNode
    {
        public CPTreeNode()
        {
            this.Checked = false;
        }
        public string NodeId { get; set; }
        public string NodeTitle { get; set; }
        public string NodeValue { get; set; }
        public string NodeAttrEx { get; set; }
        public string NodeTip { get; set; }
        public string NodeIcon { get; set; }
        public bool hasChildren { get; set; }
        public int TreeDataSourceId { get; set; }
        public string DataRowJSON { get; set; }
        public string DeleteFieldValue { get; set; }

        public string ChkSelFieldName { get; set; }
        public string ChkSelFieldValue { get; set; }
        [JsonProperty(PropertyName = "checked")]
        public bool Checked { get; set; }
        public List<CPTreeNode> items { get; set; }
    }
    public class CPTreeEnum
    {
        public enum ShowTypeEnum
        {
            LeftTreeRightFrame = 1,
            AllTree = 2
        }

        public enum DataLoadTypeEnum
        {
            /// <summary>
            /// 一次性全加载
            /// </summary>
            AllLoad = 1,
            /// <summary>
            /// 逐级加载
            /// </summary>
            GraduallyLoad = 2
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

        public enum ShowPositionEnum
        {
            TopAndRight = 1,
            Top = 2,
            Right = 3
        }
    }


    public class CPTree: BaseEntity
    {

        /// <summary>
        /// 树唯一编号
        /// </summary>
        public string TreeCode { get; set; }

        /// <summary>
        /// 树标题
        /// </summary>
        public string TreeTitle { get; set; }
        /// <summary>
        /// 所属子系统
        /// </summary>
        public int? SysId { get; set; }
        /// <summary>
        /// 所属业务功能，用户自行输入或选择，主要是用来将某一个功能的相关配置都组合到一起，便于后面的查找或修改
        /// </summary>
        public string FuncTitle { get; set; }
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
        ///  拥有此权限用户名，多个，分隔
        /// </summary>
        public string UserNames { get; set; }
        /// <summary>
        ///  拥有此权限用户ID，多个，分隔
        /// </summary>
        public string UserIds { get; set; }
        /// <summary>
        ///  拥有此权限部门名，多个，分隔
        /// </summary>
        public string DepNames { get; set; }
        /// <summary>
        ///  拥有此权限部门ID，多个，分隔
        /// </summary>
        public string DepIds { get; set; }

        /// <summary>
        /// 显示模式
        /// </summary>
        public CPTreeEnum.ShowTypeEnum? ShowType { get; set; }
        /// <summary>
        /// 左侧树占用宽度，单位像素
        /// </summary>
        public int? LeftTreeWidth { get; set; }
        /// <summary>
        /// 扩展JS
        /// </summary>
        public string JSEx { get; set; }
        /// <summary>
        /// 数据加载模式
        /// </summary>
        public CPTreeEnum.DataLoadTypeEnum? DataLoadType { get; set; }

        /// <summary>
        /// 是否显示checkbox
        /// </summary>
        public bool? IsShowCheckBox { get; set; }

        /// <summary>
        /// Checkbox默认选中，支持表达式，多个值之间用，分隔
        /// </summary>
        public string ChkDefaultSelValue { get; set; }
        /// <summary>
        /// 是否级联选择
        /// </summary>
        public bool? IsSelectChild { get; set; }
        /// <summary>
        /// 是否可以拖拽
        /// </summary>
        public bool? IsCanDrag { get; set; }
        /// <summary>
        /// 选中事件执行方法
        /// </summary>
        public string SelectEventMethod { get; set; }
        /// <summary>
        /// 双击事件执行方法
        /// </summary>
        public string DoubleClickEventMethod { get; set; }
        /// <summary>
        /// 加载完成后执行方法
        /// </summary>
        public string OnLoadEventMethod { get; set; }
        /// <summary>
        /// 右键事件执行方法
        /// </summary>
        public string RightClickEventMethod { get; set; }

        /// <summary>
        /// 功能集合
        /// </summary>
        public List<CPTreeFunc> FuncCol { get; set; }

        /// <summary>
        /// 数据源集合
        /// </summary>
        public List<CPTreeDataSource> DataSourceCol { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.SysId.HasValue == false)
                this.SysId = 1;
            if (this.IsControlByRight.HasValue == false)
                this.IsControlByRight = false;
            if (this.ShowType.HasValue == false)
                this.ShowType = CPTreeEnum.ShowTypeEnum.LeftTreeRightFrame;
            if (this.DataLoadType.HasValue == false)
                this.DataLoadType = CPTreeEnum.DataLoadTypeEnum.AllLoad;
            if (this.IsShowCheckBox.HasValue == false)
                this.IsShowCheckBox = false;
            if (this.IsSelectChild.HasValue == false)
                this.IsSelectChild = true;
            if (this.IsCanDrag.HasValue == false)
                this.IsCanDrag = false;
            if (this.LeftTreeWidth.HasValue == false)
                this.LeftTreeWidth = 250;
        }
    }

    public  class CPTreeDataSource:BaseEntity
    { 
        /// <summary>
        /// 树ID
        /// </summary>
        public int TreeId { get; set; }
        /// <summary>
        /// 父级数据源ID，根默认-1
        /// </summary>
        public int ParentSourceId { get; set; }
        /// <summary>
        /// 数据源标题
        /// </summary>
        public string DataTitle { get; set; }
        /// <summary>
        /// 数据链接实例
        /// </summary>
        public string DbIns { get; set; }
        /// <summary>
        /// 数据源内容，支持表达式
        /// </summary>
        public string DataSource { get; set; }
        /// <summary>
        /// 数据源是否递归读取  如果是，则删除时，也是递归删除
        /// </summary>
        public bool? SourceIsRecursion { get; set; }
        /// <summary>
        /// 节点标题，支持表达式
        /// </summary>
        public string NodeTitle { get; set; } 
        /// <summary>
        /// 节点扩展属性，支持表达式
        /// </summary>
        public string NodeAttrEx { get; set; } 
        /// <summary>
        /// 节点未选中时图标
        /// </summary>
        public string NodeIcon { get; set; }
        /// <summary>
        /// 数据表名
        /// </summary>
        public string MainTableName { get; set; }
        /// <summary>
        /// 表主键字段，只支持单主键
        /// </summary>
        public string PKField { get; set; }
        /// <summary>
        /// 存储父级ID的字段名
        /// </summary>
        public string ParentIdField { get; set; }
        /// <summary>
        /// Checkbox根据哪个字段默认选中，配置字段名
        /// </summary>
        public string ChkSelFieldName { get; set; }
        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPTree Tree { get; set; }

        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.SourceIsRecursion.HasValue == false)
                this.SourceIsRecursion = false;
        }
    }


    public class CPTreeFunc : BaseOrderEntity
    {
        /// <summary>
        /// 树ID
        /// </summary>
        public int TreeId { get; set; }
        /// <summary>
        /// 按钮标题
        /// </summary>
        public string FuncTitle { get; set; }
        /// <summary>
        /// 按钮图标
        /// </summary>
        public string FuncIcon { get; set; }
        /// <summary>
        /// 执行JS方法，支持表达式
        /// </summary>

        public string JSMethod { get; set; }

        /// <summary>
        /// 功能显示类型 1：顶部按钮与右键菜单同时显示 2：仅显示顶部按钮 3：仅显示右键菜单
        /// </summary>
        public CPTreeEnum.ShowPositionEnum? ShowPosition { get; set; }
        /// <summary>
        /// 数据源SourceId，-1表示所有层级都出现，选择了某个数据源，则表示只有这个数据源才出现
        /// </summary>
        public int? SourceId { get; set; }

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
        public CPTreeEnum.ShowMethodEnum? ShowMethod { get; set; }
        /// <summary>
        /// /用来比较是否显示的目标值表达式
        /// </summary>
        public string RightExpression { get; set; }
        /// <summary>
        /// 条件为真时是否显示
        /// </summary>
        public bool? IsShowByExpression { get; set; }
        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPTree Tree { get; set; }

        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.ShowPosition.HasValue == false)
                this.ShowPosition = CPTreeEnum.ShowPositionEnum.TopAndRight;
            if (this.SourceId.HasValue == false)
                this.SourceId = -1;
            if (this.IsUseExpressionShow.HasValue == false)
                this.IsUseExpressionShow = false;
            if (this.ShowMethod.HasValue == false)
                this.ShowMethod = CPTreeEnum.ShowMethodEnum.EqualTo;
            if (this.IsShowByExpression.HasValue == false)
                this.IsShowByExpression = true;

        }
    }
}
