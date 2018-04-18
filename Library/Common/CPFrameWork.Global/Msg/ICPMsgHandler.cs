using CPFrameWork.Utility.DbOper;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPFrameWork.Global.Msg
{
    /// <summary>
    /// 系统发送消息提醒的接口
    /// </summary>
    public  interface ICPMsgHandler
    {
        /// <summary>
        /// 内置消息数据库访问接口
        /// </summary>
        BaseRepository<CPMsgEntity> CPMsgRep { get; set; }
        /// <summary>
        /// 发送消息接口
        /// </summary>
        /// <param name="msgCol"></param>
        /// <returns></returns>
        bool SendMsg(List<CPMsgEntity> msgCol);
        /// <summary>
        /// 删除消息接口
        /// </summary>
        /// <param name="msgIdCol"></param>
        /// <returns></returns>
        bool DeleteMsg(List<int> msgIdCol);
        /// <summary>
        /// 修改消息是否已阅接口
        /// </summary>
        /// <param name="msgIdCol"></param>
        /// <returns></returns>
         bool UpdateMsgReadState(List<int> msgIdCol);
    } 
    public class CPMsgInnerDbHandler : ICPMsgHandler
    {
        public BaseRepository<CPMsgEntity> CPMsgRep
        {
            get;set;
        }
        public bool DeleteMsg(List<int> msgIdCol)
        {
            DbHelper _helper = new DbHelper("CPFrameIns", CPAppContext.CurDbType());
            string ids = "";
            msgIdCol.ForEach(t => {
                if (string.IsNullOrEmpty(ids))
                    ids = t.ToString();
                else
                    ids += "," + t.ToString();
            });
            string strSql = "DELETE FROM CP_Msg WHERE Id IN (" + ids + ")";
            _helper.ExecuteNonQuery(strSql);
            return true;
        }

        public bool SendMsg(List<CPMsgEntity> msgCol)
        {                    
            this.CPMsgRep.Add(msgCol);
            
            return true;
        }

        public bool UpdateMsgReadState(List<int> msgIdCol)
        {
            DbHelper _helper = new DbHelper("CPFrameIns", CPAppContext.CurDbType());
            string ids = "";
            msgIdCol.ForEach(t => {
                if (string.IsNullOrEmpty(ids))
                    ids = t.ToString();
                else
                    ids += "," + t.ToString();
            });
            string strSql = "UPDATE CP_Msg SET IsRead=1 WHERE Id IN (" + ids + ")";
            _helper.ExecuteNonQuery(strSql);
            return true;
        }
    }
    public class CPMsgEntity:BaseEntity
    {
        /// <summary>
        /// 消息发送模块关键字
        /// </summary>
        public string MsgSourceModule { get; set; }
        /// <summary>
        /// 消息发送源模块的唯一标识 符
        /// </summary>
        public string MsgSourceModulePK { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public CPEnum.MsgTypeEnum MsgType { get; set; }
       /// <summary>
       /// 消息标题
       /// </summary>
        public string MsgTitle { get; set; }
        /// <summary>
        /// 消息正文
        /// </summary>
        public string MsgContenxt { get; set; }
        /// <summary>
        /// 发送用户ID
        /// </summary>
        public int MsgSendUserId { get; set; }
        /// <summary>
        /// 发送用户姓名
        /// </summary>
        public string MsgSendUserName { get; set; }
        /// <summary>
        /// 接收用户ID
        /// </summary>
        public int ReciveUserId { get; set; }
        /// <summary>
        /// 接收用户姓名 
        /// </summary>
        public string ReciveUserName { get; set; }
        /// <summary>
        /// 接收赶时间
        /// </summary>
        public DateTime ReciveTime { get; set; }
        /// <summary>
        /// 办理页面地址
        /// </summary>
        public string ManaPageUrl { get; set; }
        /// <summary>
        /// 消息是否已读
        /// </summary>
        public bool IsRead { get; set; }
    }
    public class CPMsgRep : BaseRepository<CPMsgEntity>
    {
        public CPMsgRep(ICPFrameDbContext dbContext) : base(dbContext)
        {

        }
    }
}
