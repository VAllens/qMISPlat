using CPFrameWork.Flow.Domain;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPFrameWork.Flow.Infrastructure
{
   

    public interface ICPFlowDbContext : IDbContext
    {

    }
    public interface ICPFlowInsDbContext : IDbContext
    {

    }
    public class CPFlowDbContext : DbContext, ICPFlowDbContext
    {
        public CPFlowDbContext()
        {
        }
        public CPFlowDbContext(DbContextOptions<CPFlowDbContext> options) : base(options)
        {

        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    //if (CPAppContext.CurDbType() == DbHelper.DbTypeEnum.SqlServer)
        //    //{
        //    //    optionsBuilder.UseSqlServer(CPAppContext.Configuration.GetSection("ConnectionStrings")["CPOrganIns"]);
        //    //}
        //}
        public DbSet<CPFlow> CPFlowCol { get; set; }
        public DbSet<CPFlowPhase> CPFlowPhaseCol { get; set; }
        public DbSet<CPFlowPhaseLink> CPFlowPhaseLinkCol { get; set; }
        public DbSet<CPFlowPhaseForm> CPFlowPhaseFormCol { get; set; }

        public DbSet<CPFlowPhaseRule> CPFlowPhaseRuleCol { get; set; }
        public DbSet<CPFlowPhaseRuleHandle> CPFlowPhaseRuleHandleCol { get; set; }

     
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //CPFlow
            modelBuilder.Entity<CPFlow>().ToTable("Flow_Template");
            modelBuilder.Entity<CPFlow>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFlow>().Property(t => t.Id).HasColumnName("FlowVerId");
            modelBuilder.Entity<CPFlow>().Ignore(t => t.FlowVerId);
            modelBuilder.Entity<CPFlow>().Ignore(t => t.PhaseLinkColSubmit);
            modelBuilder.Entity<CPFlow>().Ignore(t => t.PhaseLinkColFallback);
            modelBuilder.Entity<CPFlow>().HasMany(t => t.PhaseCol).WithOne(t => t.Flow).HasForeignKey(t => t.FlowVerId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CPFlow>().HasMany(t => t.PhaseLinkCol).WithOne(t => t.Flow).HasForeignKey(t => t.FlowVerId).OnDelete(DeleteBehavior.Cascade);
            //CPFlow

            //Flow_TemplatePhase
            modelBuilder.Entity<CPFlowPhase>().ToTable("Flow_TemplatePhase");
            modelBuilder.Entity<CPFlowPhase>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFlowPhase>().Property(t => t.Id).HasColumnName("PhaseId");
            modelBuilder.Entity<CPFlowPhase>().Ignore(t => t.PhaseId);
            modelBuilder.Entity<CPFlowPhase>().Ignore(t => t.TaskRevUser);
            modelBuilder.Entity<CPFlowPhase>().HasMany(t => t.FormCol).WithOne(t => t.FlowPhase).HasForeignKey(t => t.PhaseId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CPFlowPhase>().HasMany(t => t.RuleCol).WithOne(t => t.FlowPhase).HasForeignKey(t => t.PhaseId).OnDelete(DeleteBehavior.Cascade);
            //Flow_TemplatePhase

            //CPFlowPhaseLink
            modelBuilder.Entity<CPFlowPhaseLink>().ToTable("Flow_TemplatePhaseLink");
            modelBuilder.Entity<CPFlowPhaseLink>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFlowPhaseLink>().Property(t => t.Id).HasColumnName("LinkId");
            modelBuilder.Entity<CPFlowPhaseLink>().Ignore(t => t.LinkId);
            //CPFlowPhaseLink

            //Flow_TemplatePhaseForm
            modelBuilder.Entity<CPFlowPhaseForm>().ToTable("Flow_TemplatePhaseForm");
            modelBuilder.Entity<CPFlowPhaseForm>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFlowPhaseForm>().Property(t => t.Id).HasColumnName("FormId");
            modelBuilder.Entity<CPFlowPhaseForm>().Ignore(t => t.FormId);
            //Flow_TemplatePhaseForm

            //Flow_TemplatePhaseRule
            modelBuilder.Entity<CPFlowPhaseRule>().ToTable("Flow_TemplatePhaseRule");
            modelBuilder.Entity<CPFlowPhaseRule>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFlowPhaseRule>().Property(t => t.Id).HasColumnName("RuleId");
            modelBuilder.Entity<CPFlowPhaseRule>().Ignore(t => t.RuleId);
            modelBuilder.Entity<CPFlowPhaseRule>().HasMany(t => t.RuleHandleCol).WithOne(t => t.FlowPhaseRule).HasForeignKey(t => t.RuleHandleId).OnDelete(DeleteBehavior.Cascade);
            //Flow_TemplatePhaseRule

            //Flow_TemplatePhaseRuleHandle
            modelBuilder.Entity<CPFlowPhaseRuleHandle>().ToTable("Flow_TemplatePhaseRuleHandle");
            modelBuilder.Entity<CPFlowPhaseRuleHandle>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFlowPhaseRuleHandle>().Property(t => t.Id).HasColumnName("RuleHandleId");
            modelBuilder.Entity<CPFlowPhaseRuleHandle>().Ignore(t => t.RuleHandleId);
            //Flow_TemplatePhaseRuleHandle
             

            //在此设置数据库对应关系 
            base.OnModelCreating(modelBuilder);
        }
    }

    public class CPFlowInsDbContext : DbContext, ICPFlowInsDbContext
    {
        public CPFlowInsDbContext()
        {
        }
        public CPFlowInsDbContext(DbContextOptions<CPFlowInsDbContext> options) : base(options)
        {

        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    //if (CPAppContext.CurDbType() == DbHelper.DbTypeEnum.SqlServer)
        //    //{
        //    //    optionsBuilder.UseSqlServer(CPAppContext.Configuration.GetSection("ConnectionStrings")["CPOrganIns"]);
        //    //}
      

        public DbSet<CPFlowInstance> CPFlowInstanceCol { get; set; }
        public DbSet<CPFlowInstanceTask> CPFlowInstanceTaskCol { get; set; }
        public DbSet<CPFlowInstanceLog> CPFlowInstanceLogCol { get; set; }
        public DbSet<CPFlowInstanceLogUnique> CPFlowInstanceLogUniqueCol { get; set; }
        public DbSet<CPFlowInstanceForm> CPFlowInstanceFormCol { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
 

            //CPFlowInstance
            modelBuilder.Entity<CPFlowInstance>().ToTable("Flow_Instance");
            modelBuilder.Entity<CPFlowInstance>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFlowInstance>().Property(t => t.Id).HasColumnName("InsId");
            modelBuilder.Entity<CPFlowInstance>().Ignore(t => t.InsId);
            //CPFlowInstance

            //CPFlowInstanceTask
            modelBuilder.Entity<CPFlowInstanceTask>().ToTable("Flow_InstanceTask");
            modelBuilder.Entity<CPFlowInstanceTask>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFlowInstanceTask>().Property(t => t.Id).HasColumnName("TaskId");
            modelBuilder.Entity<CPFlowInstanceTask>().Ignore(t => t.TaskId);
            //CPFlowInstanceTask

            //CPFlowInstanceLog
            modelBuilder.Entity<CPFlowInstanceLog>().ToTable("Flow_InstanceLog");
            modelBuilder.Entity<CPFlowInstanceLog>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFlowInstanceLog>().Property(t => t.Id).HasColumnName("LogId");
            modelBuilder.Entity<CPFlowInstanceLog>().Ignore(t => t.LogId);
            //CPFlowInstanceLog

            //CPFlowInstanceLogUnique
            modelBuilder.Entity<CPFlowInstanceLogUnique>().ToTable("Flow_InstanceLogUnique");
            modelBuilder.Entity<CPFlowInstanceLogUnique>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFlowInstanceLogUnique>().Property(t => t.Id).HasColumnName("UniqueLogId");
            modelBuilder.Entity<CPFlowInstanceLogUnique>().Ignore(t => t.UniqueLogId);
            //CPFlowInstanceLogUnique

            //CPFlowInstanceForm
            modelBuilder.Entity<CPFlowInstanceForm>().ToTable("Flow_InstanceForm");
            modelBuilder.Entity<CPFlowInstanceForm>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFlowInstanceForm>().Property(t => t.Id).HasColumnName("InsFormId");
            modelBuilder.Entity<CPFlowInstanceForm>().Ignore(t => t.InsFormId);
            //CPFlowInstanceForm

            //在此设置数据库对应关系 
            base.OnModelCreating(modelBuilder);
        }
    }
}
