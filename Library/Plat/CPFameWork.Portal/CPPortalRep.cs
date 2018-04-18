 

using CPFrameWork.Global;
using CPFameWork.Portal.Module;
using CPFrameWork.Organ.Infrastructure;
using CPFrameWork.Utility.DbOper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPFameWork.Portal
{
    #region 模块
    internal class CPPortalModuleRightRep : BaseRepository<CPPortalModuleRight>
    {
        public CPPortalModuleRightRep(ICPPortalDbContext dbContext) : base(dbContext)
        {

        }
    }
    public abstract class BaseCPPortalModuleRep : BaseRepository<CPPortalModule>
    {
        public BaseCPPortalModuleRep(ICPPortalDbContext dbContext) : base(dbContext)
        {

        }
        public abstract List<CPPortalModule> GetModulesWithRight(List<int> RoleIdCol, int sysId , int parentModuleId);

        public abstract List<CPPortalModule> GetModulesWithRight(List<int> RoleIdCol, int sysId);
    }
    internal class CPPortalModuleRep : BaseCPPortalModuleRep
    {
        public CPPortalModuleRep(ICPPortalDbContext dbContext) : base(dbContext)
        {

        }
        public override List<CPPortalModule> GetModulesWithRight(List<int> RoleIdCol, int sysId)
        {
            CPPortalDbContext _db = this._dbContext as CPPortalDbContext;
            var q = from module in _db.CPPortalModuleCol
                    join right in _db.CPPortalModuleRightCol
                    on module.Id equals right.ModuleId
                    where RoleIdCol.Contains(right.RoleId) && module.SysId.Equals(sysId)
                    orderby module.ShowOrder ascending
                    select module;
            return q.ToList();
        }
        public override List<CPPortalModule> GetModulesWithRight(List<int> RoleIdCol, int sysId, int parentModuleId)
        { 
            CPPortalDbContext _db = this._dbContext as CPPortalDbContext;
            var q = from module in _db.CPPortalModuleCol
                    join right in _db.CPPortalModuleRightCol
                    on module.Id equals right.ModuleId
                    where RoleIdCol.Contains(right.RoleId) && module.SysId.Equals(sysId) && module.ParentId.Value.Equals(parentModuleId)
                    orderby module.ShowOrder ascending
                    select module; 
            return q.ToList();
        }
    }
    #endregion
}
