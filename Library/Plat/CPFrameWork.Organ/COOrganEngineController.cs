using CPFrameWork.Global;
using CPFrameWork.Organ.Application;
using CPFrameWork.Organ.Domain;
using CPFrameWork.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CPFrameWork.Organ
{
    public class COOrganEngineController : CPWebApiBaseController
    {

        #region 登录方法
        public class LoginReturn:CPWebApiBaseReturnEntity
        {
            public System.Guid UserKey { get; set; }
            public int UserId { get; set; }
        }
        [HttpGet]
        public LoginReturn Login(string LoginName, string UserPwd, int DeviceType)
        {
            base.SetHeader();
            LoginReturn re = new LoginReturn();
            re.Result = true;
            try
            {
                string errorMsg = "";
                COUserIdentity iden = COOrgans.Instance().Login(LoginName, UserPwd, CPEnum.ConvertDeviceTypeEnum(DeviceType), ref errorMsg);
                if (string.IsNullOrEmpty(errorMsg) == false)
                {
                    re.Result = false;
                    re.ErrorMsg = errorMsg;
                    return re;
                }
                re.UserId = iden.UserId;
                re.UserKey = iden.UserKey;
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

        #region  获取所有部门，为组织机构选择用
        public class GetAllDepReturn:CPWebApiBaseReturnEntity
        {
            public List<CODepOrganSel> DepCol { get; set; }
        }
        public class CODepOrganSel
        {
            public int Id { get; set; }
            public string DepName { get; set; }
            public bool HasChildDep { get; set; }
            public List<CODepOrganSel> ChildDep { get; set; }
        }
        public List<CODepOrganSel> GetChildDep(List<CODep> allDep,int parentDepId)
        {
            List<CODepOrganSel> col = new List<CODepOrganSel>();
            allDep.Where(t => t.ParentId.Equals(parentDepId)).ToList().ForEach(t => {
                CODepOrganSel item = new CODepOrganSel();
                item.Id = t.Id;
                item.DepName = t.DepName;
                item.ChildDep = this.GetChildDep(allDep, t.Id);
                if (item.ChildDep.Count > 0)
                    item.HasChildDep = true;
                else
                    item.HasChildDep = false;
                col.Add(item);
            });
            return col;
        }
        public GetAllDepReturn GetAllDep( int CurUserId, string CurUserIden)
        {
            base.SetHeader(); 
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            GetAllDepReturn re = new GetAllDepReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                List<CODep> allDep = COOrgans.Instance().GetAllDep();
                re.DepCol = new List<CODepOrganSel>();
                allDep.Where(t => t.ParentId.Value.Equals(-1)).ToList().ForEach(t =>
                {
                    CODepOrganSel item = new CODepOrganSel();
                    item.Id = t.Id;
                    item.DepName = t.DepName;
                    item.ChildDep = this.GetChildDep(allDep, t.Id);
                    if (item.ChildDep.Count > 0)
                        item.HasChildDep = true;
                    else
                        item.HasChildDep = false;
                    re.DepCol.Add(item);
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

        #region  获取所有用户
        public class GetAllUserReturn : CPWebApiBaseReturnEntity
        {
            public List<COUser> UserCol { get; set; }
        }
        public GetAllUserReturn GetAllUser(int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            GetAllUserReturn re = new GetAllUserReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.UserCol = COOrgans.Instance().GetAllUser();
                re.UserCol = re.UserCol.OrderBy(t => t.UserName).ToList();
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

        #region  根据部门获取用户
        public class GetUserByDepReturn : CPWebApiBaseReturnEntity
        {
            public List<COUser> UserCol { get; set; }
        }
        public GetUserByDepReturn GetUserByDep(int DepId,int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            GetUserByDepReturn re = new GetUserByDepReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.UserCol = COOrgans.Instance().GetUserByDepId(DepId);
                re.UserCol = re.UserCol.OrderBy(t => t.UserName).ToList();
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

        #region  根据用户登录名或用户名查询用户
        public class GetUserByUserNameOrLoginNameReturn : CPWebApiBaseReturnEntity
        {
            public List<COUser> UserCol { get; set; }
        }
        public GetUserByUserNameOrLoginNameReturn GetUserByUserNameOrLoginName(string NameEx, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            NameEx = CPAppContext.FormatSqlPara(NameEx);
            GetUserByUserNameOrLoginNameReturn re = new GetUserByUserNameOrLoginNameReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.UserCol = COOrgans.Instance().GetUserByUserNameOrLoginName(NameEx);
                re.UserCol = re.UserCol.OrderBy(t => t.UserName).ToList();
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

        #region 获取用户信息
        public class GetUserInfoReturn : CPWebApiBaseReturnEntity
        {
            public List<COUser> UserCol { get; set; }
        }
        public class GetUserInfoInput
        {
            public string CurUserIden { get; set; }
            public string UserIds { get; set; }
            public int CurUserId { get; set; }
        }
        [HttpPost]
        public GetUserInfoReturn GetUserInfo([FromBody] GetUserInfoInput input )
        {
            base.SetHeader(); 
            GetUserInfoReturn re = new GetUserInfoReturn();
            if (this.CheckUserIden(input.CurUserId, input.CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.UserCol = COOrgans.Instance().GetUserByUserIds(input.UserIds);
                re.UserCol = re.UserCol.OrderBy(t => t.UserName).ToList();
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


        #region 设置部门为假删除状态
        [HttpGet]
        public CPWebApiBaseReturnEntity SetDepDeleteState(int DepId,int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.Result = COOrgans.Instance().SetDepDeleteState(DepId);
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

        #region 修改密码
        [HttpGet]
        public CPWebApiBaseReturnEntity UpdateUserPwd(string OldPwd,string NewPwd)
        {
            base.SetHeader();
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            try
            {
                string userId = CPExpressionHelper.Instance.RunCompile("${CPUser.UserId()}");
                COOrgans organs = COOrgans.Instance();
                COUser user = organs.GetUserById(int.Parse(userId), false, false);
                OldPwd = organs.UserPwdEncrypt(OldPwd);
                if (user.UserPwd.Equals(OldPwd, StringComparison.CurrentCultureIgnoreCase) == false)
                {
                    re.Result = false;
                    re.ErrorMsg = "原密码输入不对，请重新输入！";
                    return re;
                }
                user.UserPwd = organs.UserPwdEncrypt(NewPwd);
                re.Result = organs.UpdateUser(user);
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

        #region  获取当前用户所在部门的所有用户
        public class GetUserByMyDepReturn : CPWebApiBaseReturnEntity
        {
            public List<COUser> UserCol { get; set; }
        }
        public GetUserByMyDepReturn GetUserByMyDep( int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            GetUserByMyDepReturn re = new GetUserByMyDepReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                string depIds = CPExpressionHelper.Instance.RunCompile("${CPUser.DepIds()}");
                re.UserCol = new List<COUser>();
                depIds.Split(',').ToList().ForEach(t =>
                {
                    if (string.IsNullOrEmpty(t) == false)
                    {
                        List<COUser> col = COOrgans.Instance().GetUserByDepId(int.Parse(t));
                        col.ForEach(u =>
                        {
                            if (re.UserCol.Where(f => f.Id.Equals(u.Id)).Count() <= 0)
                            {
                                re.UserCol.Add(u);
                            }
                        });
                    }
                });

                re.UserCol = re.UserCol.OrderBy(t => t.UserName).ToList();
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
