using CPFrameWork.Utility;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic; 
using System.Linq; 
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.Global
{
    #region CPLog
    public class CPLog : BaseEntity
    {
        /// <summary>
        /// 操作用户ID
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 操作用户姓名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public Nullable<System.DateTime> OperTime { get; set; }
        /// <summary>
        /// IP
        /// </summary>
        public string OperIP { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public Nullable<CPEnum.DeviceTypeEnum> OperDevice { get; set; }
        /// <summary>
        /// 操作url地址
        /// </summary>
        public string OperUrl { get; set; }
        /// <summary>
        /// 日志内容
        /// </summary>
        public string OperRemark { get; set; }
        /// <summary>
        /// 日志类型，根据模块，自行指定中文描述型日志类型
        /// </summary>
        public string OperType { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.OperTime.HasValue == false)
                this.OperTime = DateTime.Now;
            if (this.OperDevice.HasValue == false)
                this.OperDevice = CPEnum.DeviceTypeEnum.PCBrowser;
        }
    }

   

    public class CPLogRep : BaseRepository<CPLog>
    {
        public CPLogRep(ICPFrameDbContext dbContext) : base(dbContext)
        {

        }
    }
  
    #endregion


    public class CPLogHelper
    {
        #region 实例 
        public static void StartupInit(IServiceCollection services, IConfigurationRoot Configuration)
        {
            services.AddDbContext<CPFrameDbContext>(options =>//手工高亮
               options.UseSqlServer(Configuration.GetConnectionString("CPFrameIns")));
            services.TryAddTransient<ICPFrameDbContext, CPFrameDbContext>();
            services.TryAddTransient<BaseRepository<CPLog>, CPLogRep>();
            services.TryAddTransient<CPLogHelper, CPLogHelper>();
        }
        public static CPLogHelper Instance()
        {
            return CPAppContext.GetService<CPLogHelper>();
        } 
        #endregion
         
        private BaseRepository<CPLog> _CPLogRep; 
        public CPLogHelper(
         BaseRepository<CPLog> CPLogRep
            )
        {
            this._CPLogRep = CPLogRep; 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户姓名</param>
        /// <param name="device">设备类型</param>
        /// <param name="operRemark">日志内容</param>
        /// <param name="operType">日志类型</param>
        /// <returns></returns>
        public bool AddLog(int userId,string userName,CPEnum.DeviceTypeEnum device,string operRemark,string operType)
        {
            CPLog log = new CPLog();
            log.UserId = userId;
            log.UserName = userName;
            log.OperTime = DateTime.Now;
            log.OperDevice = device;
            log.OperRemark = operRemark;
            log.OperType = operType;
            log.OperIP = "";
            try
            {
                //获取操作IP
                log.OperIP = CPAppContext.GetClientIP();
            }
            catch( Exception ex)
            {
                ex.ToString();
            }
            log.OperUrl = "";
            try
            {
                //获取办理页面地址
                log.OperUrl = CPAppContext.GetHttpContext().Request.Path;
            }
            catch(Exception ex)
            {

            }
            return this._CPLogRep.Add(log) > 0 ? true : false;
           
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户姓名</param>
        /// <param name="device">设备类型</param>
        /// <param name="operRemark">日志内容</param>
        /// <param name="operType">日志类型</param>
        /// <param name="operIP">用户操作IP</param>
        /// <param name="operUrl">用户操作页面URL</param>
        /// <returns></returns>
        public bool AddLog(int userId, string userName, CPEnum.DeviceTypeEnum device, string operRemark, string operType,string operUrl,string operIP)
        {
            CPLog log = new CPLog();
            log.UserId = userId;
            log.UserName = userName;
            log.OperTime = DateTime.Now;
            log.OperDevice = device;
            log.OperRemark = operRemark;
            log.OperType = operType;
            log.OperIP = operIP; 
            log.OperUrl = operUrl;            
            return this._CPLogRep.Add(log) > 0 ? true : false;

        }
    }
}
