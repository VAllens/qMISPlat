using CPFrameWork.Organ;
using CPFrameWork.Organ.Application;
using CPFrameWork.Organ.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPFrameWork.UIInterface.Form
{

    /// <summary>
    /// 用户修改密码表单
    /// </summary>

    public class CPUserFormInitEx : ICPFormBeforeLoad
    {
        public void BeforeLoad(ICPFormBeforeLoadEventArgs e)
        {
            if (e.IsEdit)
            {
                e.FormData.Tables[0].Rows[0]["UserPwd"] = "";

            }
        }
    }
    public class COUserFormEx : ICPFormAfterSave
    {
        public void AfterSave(ICPFormAfterSaveEventArgs e)
        {

            string UserPwd = e.GetFieldValue("CP_User", "UserPwd", 0);
            COOrgans organs = COOrgans.Instance();
            UserPwd = organs.UserPwdEncrypt(UserPwd);
            COUser user = organs.GetUserById(int.Parse(e.PKValue), false, false);
            user.UserPwd = UserPwd;
            organs.UpdateUser(user);

        }
    }

    public class CORoleFormEx : ICPFormAfterSave
    {
        public void AfterSave(ICPFormAfterSaveEventArgs e)
        {
            string RoleId = e.GetFieldValue("CP_Role", "RoleId", 0);
            string RoleUserIds = e.GetFieldValue("CP_Role", "RoleUserIds", 0);
            List<string> col = RoleUserIds.Split(',').ToList();
            List<CORoleUserRelate> relateCol = new List<CORoleUserRelate>();
            col.ForEach(t => {
                relateCol.Add(new CORoleUserRelate() { RoleId = int.Parse(RoleId),UserId= int.Parse(t) });
            });
            COOrgans.Instance().InitRoleUsers(int.Parse(RoleId), relateCol);
        }
    }
    public class CODepFormEx : ICPFormAfterSave
    {
        public void AfterSave(ICPFormAfterSaveEventArgs e)
        {
            string DepId = e.GetFieldValue("CP_Dep", "DepId", 0);
            string UserIds = e.GetFieldValue("CP_Dep", "UserIds", 0);
            string DepMainLeaderId = e.GetFieldValue("CP_Dep", "DepMainLeaderId", 0);
            string DepViceLeaderIds = e.GetFieldValue("CP_Dep", "DepViceLeaderIds", 0);
            string DepSupervisorId = e.GetFieldValue("CP_Dep", "DepSupervisorId", 0);
            UserIds += "," + DepMainLeaderId + "," + DepViceLeaderIds + "," + DepSupervisorId;
            List<string> col = UserIds.Split(',').ToList();
            List<CODepUserRelate> relateCol = new List<CODepUserRelate>();
            List<CODepUserRelate> oldRelateCol =  COOrgans.Instance().GetDepUserRelate(int.Parse(DepId));
            int showOrder = 10;
            if(oldRelateCol.Count >0)
            {
                oldRelateCol=  oldRelateCol.OrderByDescending(t => t.ShowOrder).ToList();
                showOrder = oldRelateCol[0].ShowOrder + 10;
            }
            col.ForEach(t => {
                if (string.IsNullOrEmpty(t) == false)
                {

                    List<CODepUserRelate> tmpCol = oldRelateCol.Where(c => c.UserId.Equals(int.Parse(t))).ToList();
                    if(tmpCol.Count >0)
                    {
                        relateCol.Add(new CODepUserRelate() { DepId = int.Parse(DepId), UserId = int.Parse(t), ShowOrder = tmpCol[0].ShowOrder });
                    }
                    else
                    {
                        relateCol.Add(new CODepUserRelate() { DepId = int.Parse(DepId), UserId = int.Parse(t), ShowOrder = showOrder });
                    }
                    
                }
            });
            COOrgans.Instance().InitDepUsers(int.Parse(DepId), relateCol);
        }
    }
}
