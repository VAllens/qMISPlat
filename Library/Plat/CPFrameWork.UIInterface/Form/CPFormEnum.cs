using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.UIInterface.Form
{
    public class CPFormEnum
    {
        public enum PKValueTypeEnum
        {
            IntSelfIncreasing = 1,
            IntNotSelfIncreasing =2,
            GUID = 3

        }
        public enum RelateTypeEnum
        {
            /// <summary>
            /// 1对一
            /// </summary>
            OneToOne = 1,
            /// <summary>
            /// 1对多
            /// </summary>
            OneToMore = 2
        }
        public enum ControlTypeEnum
        {
            TextBox = 1,
            TextArea = 2,
            DropDownList = 3,
            Combox = 4,
            Radio = 5,
            CheckBox = 6,
            DateTimeSelect = 7,
            TextEditor= 8,
            FileUpload = 9,
            PicUploadAndShow = 10,
            Sign = 11,
            UserSelect = 12,
            DepSelect = 13,
            RoleSelect = 14,
            /// <summary>
            /// 子表拓展表
            /// </summary>
            ChildTableExtend = 20

        }
      
        public enum ViewTypeEnum
        {
            TwoColumn = 1,
            OneColumn = 2,
            TextEditor=3
        }
        public enum ViewDeviceTypeEnum
        {
            /// <summary>
            /// :PC、手机、PAD通用
            /// </summary>
            PCIphoneIpad = 2,
            /// <summary>
            /// :PC、手机、PAD通用
            /// </summary>
            PC = 1,
            /// <summary>
            /// 3、手机专用
            /// </summary>
            Iphone = 3,
            /// <summary>
            /// 4、PAD专用
            /// </summary>
            Ipad = 4,
            /// <summary>
            /// 5、PC、PAD通用
            /// </summary>
            PCIpad = 5,
            /// <summary>
            /// 6、PC、手机专用
            /// </summary>
            PCIphone = 6,
            /// <summary>
            /// 7、手机、PAD专用
            /// </summary>
            IphoneIpad = 6
        }

        public enum FieldStatusEnum
        {
            Edit = 1,
            Read = 2,
            Hidden = 3
        }
        public enum FormSavedActionEnum
        {
            /// <summary>
            /// 关闭window窗口
            /// </summary>
            CloseWindow = 1,
            /// <summary>
            /// 关闭弹出层
            /// </summary>
            CloseDiv = 2,
            /// <summary>
            /// 返回到表单修改页面
            /// </summary>
            ReturnEditPage = 3,
            /// <summary>
            /// 跳转到指定页面
            /// </summary>
            ReturnOtherPage = 4,
            /// <summary>
            /// 自定义脚本
            /// </summary>
            CustomJS = 5,
            /// <summary>
            /// 关闭在top中打开的弹出层
            /// </summary>
            CloseDivInTop = 6

        }


        public enum FuncIsShowInViewEnum
        {
            /// <summary>
            /// 只读可写均显示
            /// </summary>
            ShowAll = 1,
            /// <summary>
            /// 仅可写时显示
            /// </summary>
            OnlyWriteShow = 0,
            /// <summary>
            /// 仅只读时显示
            /// </summary>
            OnlyReadShow = 2
        }
        public enum GroupTypeEnum
        {
            Init = 1,
            Right = 2
        }
        public enum AccessTypeEnum
        {
            Edit = 1,
            Read = 2,
            Hidden = 3
        }
        public enum AccessScoreTypeEnum
        {
            AllUser = 1,
            PartUser = 2
        }
        /// <summary>
        /// 初始化条件类型
        /// </summary>
        public enum InitTimeTypeEnum
        {
            /// <summary>
            /// 新增时初始化
            /// </summary>
            Add = 1,
            /// <summary>
            /// 无值时初始化
            /// </summary>
            NoValue = 2,
            /// <summary>
            /// 总是初始化
            /// </summary>
            All = 3
        }
        /// <summary>
        /// 初始化类型
        /// </summary>
        public enum InitTypeEnum
        {
            /// <summary>
            /// 固定值
            /// </summary>
            StaticValue = 1,
            /// <summary>
            /// 自动编号
            /// </summary>
            Auto = 2,
            /// <summary>
            /// 表达式
            /// </summary>
            Expression = 3,
            /// <summary>
            /// Sql语句
            /// </summary>
            Sql = 4
           
        }

        public enum RuleTypeEnum
        {
            DataCheck= 1, 
            SetFieldDisable = 2,
            SetFieldValue = 3
        }
    }
}
