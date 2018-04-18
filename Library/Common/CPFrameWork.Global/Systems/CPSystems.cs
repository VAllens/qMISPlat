using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPFrameWork.Global.Systems
{
    public class CPSystem: BaseEntity
    {
        public string SysTitle { get; set; }
        /// <summary>
        /// 管理员用户ID，多个用，分隔
        /// </summary>
        public string AdminUserIds { get; set; }
        /// <summary>
        /// 管理员用户姓名，多个用，分隔
        /// </summary>
        public string AdminUserNames { get; set; }
    }
    public   class CPSystemRep : BaseRepository<CPSystem>
    {
        public CPSystemRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        } 
    }
    public  class CPSystemHelper
    {
        #region 实例 
        /// <summary>
        /// 获取节点对象服务类
        /// </summary>
        /// <returns></returns>
        public static void StartupInit(IServiceCollection services, IConfigurationRoot Configuration)
        {
            // Add framework services.
            services.AddDbContext<CPCommonDbContext>(options =>//手工高亮
                options.UseSqlServer(Configuration.GetConnectionString("CPCommonIns")));
            services.TryAddTransient<ICPCommonDbContext, CPCommonDbContext>();
            services.TryAddTransient<IRepository<CPSystem>, CPSystemRep>();
            services.TryAddTransient<CPSystemHelper, CPSystemHelper>();
        }
        public static CPSystemHelper Instance()
        {
            return CPAppContext.GetService<CPSystemHelper>();
        }
        #endregion

        private IRepository<CPSystem> _CPSystemRep;
        public CPSystemHelper(
         IRepository<CPSystem> CPSystemRep
            )
        {
            this._CPSystemRep = CPSystemRep;
        }
       public List<CPSystem> GetSystems()
        {
            return _CPSystemRep.Get().ToList();
        }
    }
}
