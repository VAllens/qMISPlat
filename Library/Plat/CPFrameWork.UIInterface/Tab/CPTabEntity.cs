using CPFrameWork.Utility.DbOper;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPFrameWork.UIInterface.Tab
{
    public class CPTabEnum
    {
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

    }

    public   class CPTab: BaseEntity
    {
        /// <summary>
        /// 标签唯一编号
        /// </summary>
        public string TabCode { get; set; }
        public int? AutoIndex { get; set; }
        /// <summary>
        /// 标签标题
        /// </summary>
        public string TabTitle { get; set; }
        /// <summary>
        /// /所属子系统
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

        public List<CPTabItem> ItemCol { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.AutoIndex.HasValue == false)
                this.AutoIndex = 1;
            if (this.SysId.HasValue == false)
                this.SysId = 1;
            if (this.IsControlByRight.HasValue == false)
                this.IsControlByRight = false;
        }
    }


    public class CPTabItem:BaseOrderEntity
    {
        /// <summary>
        /// 所属页签
        /// </summary>
        public int TabId { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string EleTitle { get; set; } 
        /// <summary>
        /// 目标页面地址
        /// </summary>
        public string TargetUrl { get; set; }
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
        public CPTabEnum.ShowMethodEnum? ShowMethod { get; set; }
        /// <summary>
        /// 根据条件控制列是否显示
        /// </summary>
        public bool? IsShowByExpression { get; set; }
        /// <summary>
        /// 用来比较是否显示的目标值表达式
        /// </summary>
        public string RightExpression { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public   CPTab Tab { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.ShowOrder.HasValue == false)
                this.ShowOrder = 10;
            if (this.IsUseExpressionShow.HasValue == false)
                this.IsUseExpressionShow = false;
            if (this.ShowMethod.HasValue == false)
                this.ShowMethod = CPTabEnum.ShowMethodEnum.Contains;
            if (this.IsShowByExpression.HasValue == false)
                this.IsShowByExpression = true;

        }
    }
}
