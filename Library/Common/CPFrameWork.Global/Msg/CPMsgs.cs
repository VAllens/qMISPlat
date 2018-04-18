using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPFrameWork.Global.Msg
{
   public  class CPMsgs
    {

        #region 实例 
        public static void StartupInit(IServiceCollection services, IConfigurationRoot Configuration)
        {
            services.AddDbContext<CPFrameDbContext>(options =>//手工高亮
               options.UseSqlServer(Configuration.GetConnectionString("CPFrameIns")));
            services.TryAddTransient<ICPFrameDbContext, CPFrameDbContext>();
            services.TryAddTransient<BaseRepository<CPMsgEntity>, CPMsgRep>();
            services.TryAddTransient<CPMsgs, CPMsgs>();
        }
        public static CPMsgs Instance()
        {
            return CPAppContext.GetService<CPMsgs>();
        }
        #endregion
        private BaseRepository<CPMsgEntity> _CPMsgRep;
        public CPMsgs(
         BaseRepository<CPMsgEntity> CPMsgRep
            )
        {
            this._CPMsgRep = CPMsgRep;
        }
        public bool SendMsg(List<CPMsgEntity> msgCol,out string errorMsg)
        {
            errorMsg = "";
            if (msgCol == null   || msgCol. Count <= 0)
                return true;
            string MsgSendHandler = CPAppContext.GetPara("MsgSendHandler");
            if (string.IsNullOrEmpty(MsgSendHandler))
                return true;
            bool b = true;
            string[] sArray = MsgSendHandler.Split(';');
            for(int i =0;i<sArray.Length;i++)
            {
                if (string.IsNullOrEmpty(sArray[i]))
                    continue;
                try
                {
                    ICPMsgHandler inter = null;
                    if (sArray[i].Equals("CPFrameWork.Global.Msg.CPMsgInnerDbHandler,CPFrameWork.Global", StringComparison.CurrentCultureIgnoreCase))
                    {
                        inter = new CPMsgInnerDbHandler();
                    }
                    else
                    {
                       inter = Activator.CreateInstance(Type.GetType(sArray[i])) as ICPMsgHandler;
                    }
                    inter.CPMsgRep = this._CPMsgRep;
                    b =inter.SendMsg(msgCol);
                    if(!b)
                    {
                        errorMsg = "调用消息接口【" + sArray[i] + "】发送消息时出错。";
                        return b;
                    }
                }
                catch(Exception ex)
                {
                    errorMsg = "调用消息接口【" + sArray[i] + "】发送消息时出错，详细信息如下：" + ex.Message;
                    return false;
                }
            }
            return true;

        }
        public bool DeleteMsg(List<int> msgIdCol, out string errorMsg)
        {
            errorMsg = "";
            if (msgIdCol.Count <= 0)
                return true;
            string MsgSendHandler = CPAppContext.GetPara("MsgSendHandler");
            if (string.IsNullOrEmpty(MsgSendHandler))
                return true;
            bool b = true;
            string[] sArray = MsgSendHandler.Split(';');
            for (int i = 0; i < sArray.Length; i++)
            {
                if (string.IsNullOrEmpty(sArray[i]))
                    continue;
                try
                {
                    ICPMsgHandler inter = null;
                    if (sArray[i].Equals("CPFrameWork.Global.Msg.CPMsgInnerDbHandler,CPFrameWork.Global", StringComparison.CurrentCultureIgnoreCase))
                    {
                        inter = new CPMsgInnerDbHandler();
                    }
                    else
                    {
                        inter = Activator.CreateInstance(Type.GetType(sArray[i])) as ICPMsgHandler;
                    }
                    inter.CPMsgRep = this._CPMsgRep;
                    b = inter.DeleteMsg(msgIdCol);
                    if (!b)
                    {
                        errorMsg = "调用消息接口【" + sArray[i] + "】删除消息时出错。";
                        return b;
                    }
                }
                catch (Exception ex)
                {
                    errorMsg = "调用消息接口【" + sArray[i] + "】删除消息时出错，详细信息如下：" + ex.Message;
                    return false;
                }
            }
            return true;
        }
        public bool UpdateMsgReadState(List<int> msgIdCol, out string errorMsg)
        {
            errorMsg = "";
            if (msgIdCol.Count <= 0)
                return true;
            string MsgSendHandler = CPAppContext.GetPara("MsgSendHandler");
            if (string.IsNullOrEmpty(MsgSendHandler))
                return true;
            bool b = true;
            string[] sArray = MsgSendHandler.Split(';');
            for (int i = 0; i < sArray.Length; i++)
            {
                if (string.IsNullOrEmpty(sArray[i]))
                    continue;
                try
                {
                    ICPMsgHandler inter = null;
                    if (sArray[i].Equals("CPFrameWork.Global.Msg.CPMsgInnerDbHandler,CPFrameWork.Global", StringComparison.CurrentCultureIgnoreCase))
                    {
                        inter = new CPMsgInnerDbHandler();
                    }
                    else
                    {
                        inter = Activator.CreateInstance(Type.GetType(sArray[i])) as ICPMsgHandler;
                    }
                    inter.CPMsgRep = this._CPMsgRep;
                    b = inter.UpdateMsgReadState(msgIdCol);
                    if (!b)
                    {
                        errorMsg = "调用消息接口【" + sArray[i] + "】更改消息状态已读状态时出错。";
                        return b;
                    }
                }
                catch (Exception ex)
                {
                    errorMsg = "调用消息接口【" + sArray[i] + "】更改消息状态已读状态时出错，详细信息如下：" + ex.Message;
                    return false;
                }
            }
            return true;
        }

        public List<CPMsgEntity> GetMsg(int ReciveUserId,CPEnum.MsgTypeEnum msgType,int MsgCount)
        {
            //MsgCount 《=0，则获取所有的，否则获取指定 的条数
            ISpecification<CPMsgEntity> specification;
            specification = new ExpressionSpecification<CPMsgEntity>(t => t.ReciveUserId.Equals(ReciveUserId) && t.MsgType.Equals((int)msgType));
            List<CPMsgEntity> pCol = null;
            if (MsgCount <= 0)
            {
                pCol = this._CPMsgRep.GetByCondition(specification).ToList();
            }
            else
            {
                pCol =this._CPMsgRep.GetByCondition(specification).Take(MsgCount).ToList();
            }
            return pCol;

        }
    }
}
