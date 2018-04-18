using CPFrameWork.Utility.DbOper;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPFameWork.Portal.Module
{
   public  class CPPortalModule:BaseEntity
    {
        /// <summary>
        /// 所属子系统，默认是1，目前暂时不实现，为扩展使用
        /// </summary>
        public int SysId { get; set; }
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; }
        /// <summary>
        /// 父模块ID，-1表示根
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        /// 页面地址，支持表达式
        /// </summary>
        public string ModuleUrl { get; set; }
        /// <summary>
        /// 图标地址
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 显示顺序
        /// </summary>
        public int? ShowOrder { get; set; }
        /// <summary>
        /// 是否显示
        /// </summary>
        public bool? IsShow { get; set; }
        /// <summary>
        /// 模块打开方式
        /// </summary>
        public CPPortalEnum.ModuleOpenTypeEnum? OpenType { get; set; }

        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.ParentId.HasValue == false)
                this.ParentId = -1;
            if (this.ShowOrder.HasValue == false)
                this.ShowOrder = 10;
            if (this.IsShow.HasValue == false)
                this.IsShow = true;
            if (this.OpenType.HasValue == false)
                this.OpenType = CPPortalEnum.ModuleOpenTypeEnum.InnerFrame;
        }
    }

    public class CPPortalModuleRight:BaseEntity
    {
        public int ModuleId { get; set; }
        public int RoleId { get; set; }
    }
}
