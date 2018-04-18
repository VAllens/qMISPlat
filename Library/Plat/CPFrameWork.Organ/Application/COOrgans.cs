using CPFrameWork.Global;
using CPFrameWork.Global.Systems;
using CPFrameWork.Organ.Domain;
using CPFrameWork.Organ.Infrastructure;
using CPFrameWork.Organ.Repository;
using CPFrameWork.Utility.DbOper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.Organ.Application
{
    public  class COOrgans
    {

        #region 获取配置时相关 
        /// <summary>
        /// 获取节点对象服务类
        /// </summary>
        /// <returns></returns>
        public static void StartupInit(IServiceCollection services, IConfigurationRoot Configuration)
        {
            // Add framework services.
            services.AddDbContext<CODbContext>(options =>//手工高亮
                options.UseSqlServer(Configuration.GetConnectionString("CPOrganIns")));

            services.TryAddTransient<ICODbContext, CODbContext>();
            services.TryAddTransient<IRepository<COUser>, COUserRep>();
            services.TryAddTransient<IRepository<CORoleUserRelate>, CORoleUserRelateRep>();
            services.TryAddTransient<IRepository<CODepUserRelate>, CODepUserRelateRep>();
            services.TryAddTransient<BaseCODepRep, CODepRep>();
            services.TryAddTransient<BaseCOUserIdentityRep, COUserIdentityRep>();
            services.TryAddTransient<BaseCORoleRep, CORoleRep>();
            services.TryAddTransient<COOrgans, COOrgans>();
        }
        public static COOrgans Instance()
        {
            COOrgans iObj = CPAppContext.GetService<COOrgans>();
            return iObj;
        }
        #endregion


        private BaseCODepRep _BaseCODepRep;
        private IRepository<COUser> _COUserRep;
        private IRepository<CORoleUserRelate> _CORoleUserRelateRep;
        private IRepository<CODepUserRelate> _CODepUserRelateRep;
        private BaseCORoleRep _CORoleRep;
        private BaseCOUserIdentityRep _COUserIdentityRep;
        public COOrgans(
                 BaseCODepRep BaseCODepRep,
         IRepository<COUser> COUserRep,
         IRepository<CORoleUserRelate> CORoleUserRelateRep,
         IRepository<CODepUserRelate> CODepUserRelateRep,
         BaseCORoleRep CORoleRep,
         BaseCOUserIdentityRep COUserIdentityRep
            )
        {
            this._BaseCODepRep = BaseCODepRep;
            this._COUserIdentityRep = COUserIdentityRep;
            this._CORoleRep = CORoleRep;
            this._COUserRep = COUserRep;
            this._CORoleUserRelateRep = CORoleUserRelateRep;
            this._CODepUserRelateRep = CODepUserRelateRep;
        } 

        #region 用户相关
        /// <summary>
        /// 用户密码加密
        /// </summary>
        /// <param name="userPwd"></param>
        /// <returns></returns>
        public string UserPwdEncrypt(string userPwd)
        {
            if (string.IsNullOrEmpty(userPwd)) userPwd = "";
             MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(userPwd));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();

        }
        /// <summary>
        /// 登录方法
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="userPwd"></param>
        /// <returns></returns>
        public COUserIdentity Login(string loginName,string userPwd, CPEnum.DeviceTypeEnum device, ref string errorMsg)
        {
            COUser user = this.GetUserByLoginName(loginName, false, false);
            if(user == null)
            {
                errorMsg = "不存在登录名为[" + loginName + "]的用户，请重新输入！";
                return null;
            }
            if(user.UserPwd.Equals(this.UserPwdEncrypt(userPwd),StringComparison.CurrentCultureIgnoreCase)==false)
            {
                errorMsg = "登录名或密码不对，请重新输入！";
                return null;
            }
            Guid gId = Guid.NewGuid();
            bool b  = this.AddUserIdentity(user.Id, gId, device);
            if(b)
            {
                //记录登录日志
                CPLogHelper.Instance().AddLog(user.Id, user.UserName, device, user.UserName  + "登录成功！", "用户登录");
                COUserIdentity userIden =  this.GetUserIdentity(gId);
                this.AddUserSession(userIden, user);
                return userIden;
            }
            else
            {
                errorMsg = "写入登录标识时出错！";
                return null;
            }
        }
        private void AddUserSession(COUserIdentity userIden, COUser user)
        {
            CPAppContext.GetHttpContext().Session.SetString("UserId", userIden.UserId.ToString());
            CPAppContext.GetHttpContext().Session.SetString("UserKey", userIden.UserKey.ToString());
            CPAppContext.GetHttpContext().Session.SetString("UserName", user.UserName);
            CPAppContext.GetHttpContext().Session.SetString("UserLoginName", user.LoginName.ToString());
            if (string.IsNullOrEmpty(user.UserPhotoPath))
            {
                CPAppContext.GetHttpContext().Session.SetString("UserPhotoPath", "");
            }
            else
            {
                CPAppContext.GetHttpContext().Session.SetString("UserPhotoPath", user.UserPhotoPath.ToString());
            }
            //获取用户所在的角色
            List<CORole> userRole = this.GetUserStaticRoles(user.Id);
            string RoleIds = "";
            string RoleNames = "";
            userRole.ForEach(t => {
                if (string.IsNullOrEmpty(RoleIds))
                {
                    RoleIds = t.Id.ToString();
                    RoleNames = t.RoleName;
                }
                else
                {
                    RoleIds += "," +  t.Id.ToString();
                    RoleNames += "," + t.RoleName;
                }
            });
            CPAppContext.GetHttpContext().Session.SetString("RoleIds", RoleIds.ToString());
            CPAppContext.GetHttpContext().Session.SetString("RoleNames", RoleNames.ToString());
            //获取部门
            List<CODep> depCol = this.GetDepByUser(user.Id);
            string DepIds = "";
            string DepNames = "";
            depCol.ForEach(t => {
                if (string.IsNullOrEmpty(DepIds))
                {
                    DepIds = t.Id.ToString();
                    DepNames = t.DepName;
                }
                else
                {
                    DepIds += "," + t.Id.ToString();
                    DepNames += "," + t.DepName;
                }
            });
            CPAppContext.GetHttpContext().Session.SetString("DepIds", DepIds.ToString());
            CPAppContext.GetHttpContext().Session.SetString("DepNames", DepNames.ToString());
            //获取当前用户拥有管理员权限的子系统
            List<CPSystem> sysCol = CPSystemHelper.Instance().GetSystems();
            string sysIds = "";
            sysCol.ForEach(t => {
                if (string.IsNullOrEmpty(t.AdminUserIds))
                    return;
                if(t.AdminUserIds.Split(',').Contains(user.Id.ToString()))
                {
                    if (string.IsNullOrEmpty(sysIds)) sysIds = t.Id.ToString();
                    else
                        sysIds += "," + t.Id.ToString();
                }
            });
            CPAppContext.GetHttpContext().Session.SetString("UserAdminSysIds", sysIds.ToString());

        }

        public COUser GetUserById(int userId,bool isLoadUserDepInfo,bool isLoadUserRoleInfo)
        {
            Expression<Func<COUser, dynamic>>[] eagerLoadingProperties = null;
            if (isLoadUserDepInfo)
            {
                if (isLoadUserRoleInfo)
                {
                    eagerLoadingProperties = new Expression<Func<COUser, dynamic>>[] { t => t.DepCol ,t=>t.RoleCol};
                }
                else
                {
                    eagerLoadingProperties = new Expression<Func<COUser, dynamic>>[] { t => t.DepCol };
                }
            }
            else
            {
                if (isLoadUserRoleInfo)
                {
                    eagerLoadingProperties = new Expression<Func<COUser, dynamic>>[] {  t => t.RoleCol };
                }

            }
            return this._COUserRep.Get(userId,eagerLoadingProperties);
        }
        public COUser GetUserByLoginName(string loginName, bool isLoadUserDepInfo, bool isLoadUserRoleInfo)
        {
            Expression<Func<COUser, dynamic>>[] eagerLoadingProperties = null;
            if (isLoadUserDepInfo)
            {
                if (isLoadUserRoleInfo)
                {
                    eagerLoadingProperties = new Expression<Func<COUser, dynamic>>[] { t => t.DepCol, t => t.RoleCol };
                }
                else
                {
                    eagerLoadingProperties = new Expression<Func<COUser, dynamic>>[] { t => t.DepCol };
                }
            }
            else
            {
                if (isLoadUserRoleInfo)
                {
                    eagerLoadingProperties = new Expression<Func<COUser, dynamic>>[] { t => t.RoleCol };
                }

            }
            ISpecification<COUser> specification;
            specification = new ExpressionSpecification<COUser>(t => t.LoginName.Equals(loginName));
            IList<COUser> col =  this._COUserRep.GetByCondition(specification, eagerLoadingProperties);
            if (col.Count <= 0)
                return null;
            else
                return col[0];
        }
        public List<COUser> GetUserByDepId(int depId)
        {
            return this._BaseCODepRep.GetUserInDep(depId);
        }
        public List<COUser> GetUserByUserNameOrLoginName(string nameEx)
        {
            ISpecification<COUser> specification;
            specification = new ExpressionSpecification<COUser>(t => t.IsCanLogin.Value.Equals(true) && t.IsOutUser.Value.Equals(false)
            && ( t.UserName.Contains(nameEx)  || t.LoginName.Contains(nameEx))
            );
            IList<COUser> col = this._COUserRep.GetByCondition(specification);
            return col.ToList();
        }
        public List<COUser> GetUserByUserIds(string userIds)
        {
            List<string> col = new List<string>();
            col = userIds.Split(',').ToList();
            ISpecification<COUser> specification;
            specification = new ExpressionSpecification<COUser>(t => t.IsCanLogin.Value.Equals(true) && t.IsOutUser.Value.Equals(false)
            && col.Contains(t.Id.ToString())
            );
            IList<COUser> colNew = this._COUserRep.GetByCondition(specification);
            return colNew.ToList();
        }
        public List<COUser> GetAllUser()
        {
            ISpecification<COUser> specification;
            specification = new ExpressionSpecification<COUser>(t => t.IsCanLogin.Value.Equals(true) && t.IsOutUser.Value.Equals(false));
            IList <COUser> col = this._COUserRep.GetByCondition(specification);
            return col.ToList();
        }
        public List<COUser> GetUserByRoleId(int roleId)
        {
            return this.GetRoleById(roleId, true).UserCol;
        }
        public bool UpdateUser(COUser user)
        {
            this._COUserRep.Update(user);
            return true;
        } 
        #endregion

        #region 部门相关
        public List<CODepUserRelate> GetDepUserRelate(int depId)
        {
            ISpecification<CODepUserRelate> specification;
            specification = new ExpressionSpecification<CODepUserRelate>(t => t.DepId.Equals(depId));
            IList<CODepUserRelate> col = this._CODepUserRelateRep.GetByCondition(specification);
            return col.ToList();
        }
        public bool InitDepUsers(int depId, List<CODepUserRelate> depUserCol)
        {
            this._CODepUserRelateRep.DeleteByCondition(t => t.DepId.Equals(depId));
            this._CODepUserRelateRep.Add(depUserCol);
            return true;
        }
        public List<CODep> GetDepByUser(int userId)
        {
           return  this._BaseCODepRep.GetDepByUser(userId);
        }
        public List<CODep> GetAllDep()
        {
            // return this._BaseCODepRep.Get().ToList();
            ISpecification<CODep> specification;
            specification = new ExpressionSpecification<CODep>(t => t.DepState == COEnum.DepStateEnum.Normal);
            IList<CODep> col = this._BaseCODepRep.GetByCondition(specification);
            return col.ToList();
        }
        public List<CODep> GetDepByParentId(int parentId, bool isLoadUserInfo)
        {
            ISpecification<CODep> specification;
            specification = new ExpressionSpecification<CODep>(t => t.ParentId.Equals(parentId));
            List<CODep> depCol =  this._BaseCODepRep.GetByCondition(specification).ToList();
            if(isLoadUserInfo)
            {
                depCol.ForEach(t => {
                    t.UserCol = this._BaseCODepRep.GetUserInDep(t.Id);
                });
            }
            return depCol;
        }
        /// <summary>
        /// 将部门设置成假删除状态
        /// </summary>
        /// <param name="depId"></param>
        /// <returns></returns>
        public bool SetDepDeleteState(int depId)
        {
            CODep dep = this.GetDepById(depId, false);
            dep.DepState = COEnum.DepStateEnum.Delete;
            this._BaseCODepRep.Update(dep);
            this.InitDepUsers(depId, new List<CODepUserRelate>());
            return true;
        }
        public CODep GetDepById(int depId,bool isLoadUserInfo)
        { 

            CODep dep =  this._BaseCODepRep.Get(depId);
            if (isLoadUserInfo)
            {
                dep.UserCol = this._BaseCODepRep.GetUserInDep(depId);
            }
            return dep;
        }
        #endregion

        #region 角色相关
        public bool InitRoleUsers(int roleId,List<CORoleUserRelate> roleUserCol)
        {
            this._CORoleUserRelateRep.DeleteByCondition(t => t.RoleId.Equals(roleId));
            this._CORoleUserRelateRep.Add(roleUserCol);
            return true;
        }
        public CORole GetRoleById(int roleId,bool isLoadUserInfo)
        {
            Expression<Func<CORole, dynamic>>[] eagerLoadingProperties = null;
            if (isLoadUserInfo)
            {
                eagerLoadingProperties = new Expression<Func<CORole, dynamic>>[] { t => t.UserCol };
            }
            return this._CORoleRep.Get(roleId, eagerLoadingProperties);
        }
        /// <summary>
        /// 根据用户获取所有的角色
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<CORole> GetUserStaticRoles(int userId)
        {

            return this._CORoleRep.GetUserStaticRoles(userId);
        }
        #endregion

        #region 登录key相关
        public COUserIdentity GetUserIdentity(Guid UserKey)
        {
            ISpecification<COUserIdentity> specification;
            specification = new ExpressionSpecification<COUserIdentity>(t => t.UserKey.Equals(UserKey));
            List<COUserIdentity> col =  this._COUserIdentityRep.GetByCondition(specification).ToList();
            if (col.Count > 0)
                return col[0];
            else
                return null;
        }
        public bool AddUserIdentity(int userId,Guid userKey, CPEnum.DeviceTypeEnum deviceType)
        {
            //删除过期的数据
            this. _COUserIdentityRep.DeleteOverdueKey();
            COUserIdentity iden = new Domain.COUserIdentity() {
                 UserId = userId,
                 UserKey = userKey,
                 LoginTime = DateTime.Now,
                 LoginDevice = deviceType
            };
            return this._COUserIdentityRep.Add(iden) > 0 ? true : false;
        }
        #endregion
    }
}
