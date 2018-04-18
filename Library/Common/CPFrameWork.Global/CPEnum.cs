using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.Global
{
   public  class CPEnum
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public enum MsgTypeEnum
        {
            /// <summary>
            /// 提醒消息
            /// </summary>
            Notice = 1,
            /// <summary>
            /// 必须处理的任务消息
            /// </summary>
            Task = 2
        }
        public static CPEnum.MsgTypeEnum ConvertMsgTypeEnum(int n)
        {
            CPEnum.MsgTypeEnum type = CPEnum.MsgTypeEnum.Task;
            foreach (int nIndex in Enum.GetValues(typeof(CPEnum.MsgTypeEnum)))
            {
                if (nIndex.Equals(n))
                {
                    type = (CPEnum.MsgTypeEnum)Enum.Parse(typeof(CPEnum.MsgTypeEnum), n.ToString());
                    break;
                }
            }
            return type;
        }
        public enum FieldValueTypeEnum
        {
            Int = 1,
            Double = 2,
            DateTime = 3,
            String = 4,
            GUID = 5
        }

      

        public static CPEnum.DeviceTypeEnum ConvertDeviceTypeEnum(int n )
        {
            CPEnum.DeviceTypeEnum type = CPEnum.DeviceTypeEnum.PCBrowser;
            foreach (int nIndex in Enum.GetValues(typeof(CPEnum.DeviceTypeEnum)))
            {
                if (nIndex.Equals(n))
                {
                    type = (CPEnum.DeviceTypeEnum)Enum.Parse(typeof(CPEnum.DeviceTypeEnum), n.ToString());
                    break;
                }
            }
            return type;
        }
        public enum DeviceTypeEnum
        {
            /// <summary>
            /// PC浏览器设备
            /// </summary>
            PCBrowser = 1,
            /// <summary>
            /// IOS手机
            /// </summary>
            IOSPhone = 2,
            /// <summary>
            /// 安卓手机
            /// </summary>
            AndroidPhone = 3,
            /// <summary>
            /// IOS平板
            /// </summary>
            IOSPad = 4,
            /// <summary>
            /// 安卓平板
            /// </summary>
            AndroidPad = 5
        }

    }
}
