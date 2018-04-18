using AutoMapper;
using CPFameWork.Portal.Module;
using CPFrameWork.Global;
using CPFrameWork.Global.Msg;
using CPFrameWork.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CPFrameWork.UIInterface.Tree
{
    public class CPModuleEngineController : CPWebApiBaseController
    {

        #region 获取子模块
        public class GetChildModuleReturn : CPWebApiBaseReturnEntity
        {
            public List<CPPortalModuleClient> ModuleCol { get; set; }
        }
        public class CPPortalModuleClient: CPPortalModule
        {
            public List<CPPortalModuleClient> ChildModule { get; set; }
        }
        [HttpGet]
        public GetChildModuleReturn GetChildModule(int SysId,int ParentModuleId, bool IsLoadChildModule, int CurUserId, string CurUserIden)
        {
            base.SetHeader(); 
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            GetChildModuleReturn re = new GetChildModuleReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                List<CPPortalModule> moduleCol = CPModuleEngine.Instance(CurUserId).GetModules(CurUserId, SysId, ParentModuleId);
                re.ModuleCol = new List<CPPortalModuleClient>();
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<CPPortalModule, CPPortalModuleClient>();
                });
                moduleCol.ForEach(t =>
                {
                    CPPortalModuleClient c = AutoMapper.Mapper.Map<CPPortalModuleClient>(t);
                    c.ChildModule = new List<CPPortalModuleClient>();
                    if (IsLoadChildModule)
                    {
                        List<CPPortalModule> cModuleCol = CPModuleEngine.Instance(CurUserId).GetModules(CurUserId, SysId, t.Id);
                        cModuleCol.ForEach(f =>
                        {
                            CPPortalModuleClient tmp = AutoMapper.Mapper.Map<CPPortalModuleClient>(f);
                            c.ChildModule.Add(tmp);
                        });
                    }
                    re.ModuleCol.Add(c);
                });
                re.Result = true;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion


        #region 将某个角色授权的模块ID存储session中
        [HttpGet]
        public CPWebApiBaseReturnEntity SetRoleModuleIdsToSession(int RoleId,int SysId , int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                List<CPPortalModule> col = CPModuleEngine.Instance(CurUserId).GetModulesWithRight(new List<int>() { RoleId }, SysId);
                string ids = "";
                col.ForEach(t =>
                {
                    if (string.IsNullOrEmpty(ids))
                        ids = t.Id.ToString();
                    else
                        ids += "," + t.Id.ToString();
                });
                CPAppContext.GetHttpContext().Session.SetString("CurUserModuleRightInRole", ids);
                re.Result = true;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion


        #region 给某个角色授权
        public class SetRoleModuleRightInput
        {
            public int CurUserId { get; set; }
            public string CurUserIden { get; set; }
            public int RoleId { get; set; }
            public string ModuleIds { get; set; }
        }
        [HttpPost]
        public CPWebApiBaseReturnEntity SetRoleModuleRight([FromBody] SetRoleModuleRightInput input)
        {
            base.SetHeader(); 
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(input.CurUserId, input.CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                List<CPPortalModuleRight> col = new List<CPPortalModuleRight>();
                string[] sArray = input.ModuleIds.Split(',');
                sArray.ToList().ForEach(t =>
                {
                    if (string.IsNullOrEmpty(t))
                        return;
                    CPPortalModuleRight item = new CPPortalModuleRight();
                    item.RoleId = input.RoleId;
                    item.ModuleId = int.Parse(t);
                    col.Add(item);
                });
                re.Result = CPModuleEngine.Instance(input.CurUserId).InitRoleModuleRight(input.RoleId, col);

                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 获取消息提醒接口
        public class GetMsgReturn : CPWebApiBaseReturnEntity
        {
            public List<CPMsgEntity> MsgCol { get; set; }
        }
        [HttpGet]
        public GetMsgReturn GetMsg( int MsgType,int MsgCount,int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            GetMsgReturn re = new GetMsgReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.MsgCol =   CPMsgs.Instance().GetMsg(CurUserId, CPEnum.ConvertMsgTypeEnum(MsgType), MsgCount);
                re.Result = true;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

    }
}
