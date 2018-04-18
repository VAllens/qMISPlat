using CPFrameWork.Global;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CPFrameWork.UIInterface.Tab
{
  public  class CPTabEngine
    {
        #region 获取实例 

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
            services.TryAddTransient<BaseCPTabRep, CPTabRep>();
            services.TryAddTransient<BaseRepository<CPTabItem>, CPTabItemRep>();
            services.TryAddTransient<CPTabEngine, CPTabEngine>();
        }
        public static CPTabEngine Instance()
        {
            CPTabEngine iObj = CPAppContext.GetService<CPTabEngine>(); 
            return iObj;
        }


        #endregion  
        private BaseCPTabRep _CPTabRep;
        private BaseRepository<CPTabItem> _CPTabItemRep;

        public CPTabEngine(
            BaseCPTabRep CPTabRep,
            BaseRepository<CPTabItem> CPTabItemRep
            )
        {
            this._CPTabRep = CPTabRep;
            this._CPTabItemRep = CPTabItemRep;

        }

        public CPTab GetTab(string tabCode,bool isLoadItemInfo)
        {
            int nCount = 0;
            if (isLoadItemInfo)
            {
                nCount++;
            }
              

            Expression<Func<CPTab, dynamic>>[] eagerLoadingProperties = new Expression<Func<CPTab, dynamic>>[nCount];
            int nIndex = 0;
            if (isLoadItemInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.ItemCol;
                nIndex++;
            }
            

            ISpecification<CPTab> specification;
            specification = new ExpressionSpecification<CPTab>(t => t.TabCode.Equals(tabCode));
            IList<CPTab> col = this._CPTabRep.GetByCondition(specification, eagerLoadingProperties);
            if (col.Count <= 0)
                return null;
            else
            {
                col[0].ItemCol = col[0].ItemCol.OrderBy(c => c.ShowOrder).ToList(); 
                return col[0];
            }
        }



        #region 配置导出，同步相关
        public string GetTabConfigXml(List<int> tabIdCol)
        {

            if (tabIdCol.Count <= 0)
                return "";
            DataSet ds = this._CPTabRep.GetConfig(tabIdCol);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ds.WriteXml(ms);
            byte[] bData = ms.GetBuffer();
            ms.Close();
            return System.Text.UTF8Encoding.UTF8.GetString(bData);
        }
        /// <summary>
        /// 从xml创建新的列表配置实例 
        /// </summary>
        /// <param name="funcId"></param>
        /// <param name="sysId"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public bool InitTabFromConfigXml(int targetSysId, byte[] bData)
        {
            DataSet ds = new DataSet();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(bData, 0, bData.Length);
            ms.Position = 0;
            ds.ReadXml(ms);
            ms.Close();
            bool b = true;
            b = _CPTabRep.SyncConfigFromDataSet(targetSysId, ds, true);
            return b;
        }
        public bool SyncTabFromConfigXml(int targetSysId, byte[] bData)
        {
            DataSet ds = new DataSet();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(bData, 0, bData.Length);
            ms.Position = 0;
            ds.ReadXml(ms);
            ms.Close();
            bool b = true;
            b = _CPTabRep.SyncConfigFromDataSet(targetSysId, ds, false);
            return b;
        }
        #endregion   
    }
}
