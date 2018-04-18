using CPFrameWork.Global;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPFameWork.Portal.Module
{
   public   class CPModuleEngine
    {
        #region 获取实例 

        /// <summary>
        /// 获取节点对象服务类
        /// </summary>
        /// <returns></returns>
        public static void StartupInit(IServiceCollection services, IConfigurationRoot Configuration)
        {
            // Add framework services.
            services.AddDbContext<CPPortalDbContext>(options =>//手工高亮
                options.UseSqlServer(Configuration.GetConnectionString("CPFrameIns")));

            services.TryAddTransient<ICPPortalDbContext, CPPortalDbContext>();
            services.TryAddTransient<BaseCPPortalModuleRep, CPPortalModuleRep>();
            services.TryAddTransient<BaseRepository<CPPortalModuleRight>, CPPortalModuleRightRep>();
            services.TryAddTransient<CPModuleEngine, CPModuleEngine>();
        }
        public static CPModuleEngine Instance(int curUserId)
        {
            CPModuleEngine iObj = CPAppContext.GetService<CPModuleEngine>();
            iObj.CurUserId = curUserId;
            return iObj;
        }


        #endregion
        public int CurUserId { get; set; }
        private BaseCPPortalModuleRep _CPPortalModuleRep;
        private BaseRepository<CPPortalModuleRight> _CPPortalModuleRightRep;

        public CPModuleEngine(
            BaseCPPortalModuleRep CPPortalModuleRep,
             BaseRepository<CPPortalModuleRight> CPPortalModuleRightRep
            )
        {
            this._CPPortalModuleRep = CPPortalModuleRep;
            this._CPPortalModuleRightRep = CPPortalModuleRightRep;  

        }
        public List<CPPortalModule> GetModulesByParent( int parentModuleId = -1, int sysId = 1)
        {
            ISpecification<CPPortalModule> specification;
            specification = new ExpressionSpecification<CPPortalModule>(t => t.ParentId.Equals(parentModuleId) && t.IsShow==true && t.SysId == sysId);
            IList<CPPortalModule> col = this._CPPortalModuleRep.GetByCondition(specification);
            return col.OrderBy(t=>t.ShowOrder).ToList();
        }
        public   List<CPPortalModule> GetModulesWithRight(List<int> RoleIdCol, int sysId)
        {
            return this._CPPortalModuleRep.GetModulesWithRight(RoleIdCol, sysId);
        }
        public List<CPPortalModule> GetModules(int userId,int sysId = 1,int parentModuleId=-1)
        {
            string curUserRoleIds = CPExpressionHelper.Instance.RunCompile("${CPUser.UserRoleIds()}");
            string UserAdminSysIds = CPExpressionHelper.Instance.RunCompile("${CPUser.UserAdminSysIds()}");
            string[] UserAdminSysIdCol = UserAdminSysIds.Split(',');
            if(UserAdminSysIdCol.Contains(sysId.ToString()))
            {
                //超级管理员获取所有的
                return this.GetModulesByParent(parentModuleId,sysId);
            }
            else
            {
                if (string.IsNullOrEmpty(curUserRoleIds))
                    return new List<CPPortalModule>();
                List<int> roleIdCol = new List<int>();
                curUserRoleIds.Split(',').ToList().ForEach(t => {
                    roleIdCol.Add(int.Parse(t));
                });
                return this._CPPortalModuleRep.GetModulesWithRight(roleIdCol, sysId, parentModuleId);
            }

        }

        public bool InitRoleModuleRight(int roleId,List<CPPortalModuleRight> rightCol)
        {
            this._CPPortalModuleRightRep.DeleteByCondition(t => t.RoleId.Equals(roleId));
            this._CPPortalModuleRightRep.Add(rightCol);
            return true;
        }
    }
}
