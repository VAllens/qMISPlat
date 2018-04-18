using CPFameWork.Portal.Module;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPFameWork.Portal
{
    public interface ICPPortalDbContext : IDbContext
    {

    }
    public class CPPortalDbContext: DbContext, ICPPortalDbContext
    {
        public CPPortalDbContext(DbContextOptions<CPPortalDbContext> options) : base(options)
        {

        }

        #region 模块
        public DbSet<CPPortalModule> CPPortalModuleCol { get; set; }
        public DbSet<CPPortalModuleRight> CPPortalModuleRightCol { get; set; }

        #endregion


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            #region 模块
            modelBuilder.Entity<CPPortalModule>().ToTable("CP_Module");
            modelBuilder.Entity<CPPortalModule>().HasKey(t => t.Id);
            modelBuilder.Entity<CPPortalModule>().Property(t => t.Id).HasColumnName("ModuleId");

            modelBuilder.Entity<CPPortalModuleRight>().ToTable("CP_ModuleRight");
            modelBuilder.Entity<CPPortalModuleRight>().HasKey(t => t.Id);
            modelBuilder.Entity<CPPortalModuleRight>().Property(t => t.Id).HasColumnName("RightId");
            #endregion

            //在此设置数据库对应关系 
            base.OnModelCreating(modelBuilder);
        }
    }
}
