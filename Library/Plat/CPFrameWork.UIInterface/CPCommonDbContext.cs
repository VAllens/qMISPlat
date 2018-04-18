using CPFrameWork.UIInterface.Form;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using CPFrameWork.UIInterface.Grid;
using CPFrameWork.UIInterface.Tab;
using CPFrameWork.UIInterface.Tree;

namespace CPFrameWork.UIInterface
{
   
    public interface ICPCommonDbContext : IDbContext
    {

    } 
    internal class CPCommonDbContext : DbContext, ICPCommonDbContext
    {
        public CPCommonDbContext(DbContextOptions<CPCommonDbContext> options) : base(options)
        {

        }
        #region 表单
        public DbSet<CPForm> CPFormCol { get; set; }

        public DbSet<CPFormChildTable> CPFormChildTableCol { get; set; }
        public DbSet<CPFormField> CPFormFieldCol { get; set; }
        public DbSet<CPFormView> CPFormViewCol { get; set; }

        public DbSet<CPFormViewField> CPFormViewFieldCol { get; set; }

        public DbSet<CPFormUseScene> CPFormUseSceneCol { get; set; }
        public DbSet<CPFormUseSceneFunc> CPFormUseSceneFuncCol { get; set; }

        public DbSet<CPFormGroup> CPFormGroupCol { get; set; }
        public DbSet<CPFormFieldRight> CPFormFieldRightCol { get; set; }

        public DbSet<CPFormFieldInit> CPFormFieldInitCol { get; set; }

        public DbSet<CPFormFieldRule> CPFormFieldRuleCol { get; set; }

        #endregion

        #region 列表
        public DbSet<CPGrid> CPGridCol { get; set; }
        public DbSet<CPGridColumn> CPGridColumnCol { get; set; }
        public DbSet<CPGridFunc> CPGridFuncCol { get; set; }
        #endregion

        #region 标签 
        public DbSet<CPTab> CPTabCol { get; set; }
        public DbSet<CPTabItem> CPTabItemCol { get; set; }
        #endregion



        #region 树 
        public DbSet<CPTree> CPTreeCol { get; set; }
        public DbSet<CPTreeDataSource> CPTreeDataSourceCol { get; set; }
        public DbSet<CPTreeFunc> CPTreeFuncCol { get; set; }
        #endregion


        protected override void OnModelCreating( ModelBuilder modelBuilder)
        {

            #region 表单

            //CPForm 
            modelBuilder.Entity<CPForm>().ToTable("Form_Main");
            modelBuilder.Entity<CPForm>().HasKey(t => t.Id);
            modelBuilder.Entity<CPForm>().Property(t => t.Id).HasColumnName("FormId");
            modelBuilder.Entity<CPForm>().HasMany(t => t.ChildTableCol).WithOne(t => t.Form).HasForeignKey(t => t.FormId).OnDelete( DeleteBehavior.Cascade);
            modelBuilder.Entity<CPForm>().HasMany(t => t.FieldCol).WithOne(t => t.Form).HasForeignKey(t => t.FormId) .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CPForm>().HasMany(t => t.ViewCol).WithOne(t => t.Form).HasForeignKey(t => t.FormId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CPForm>().HasMany(t => t.UseSceneCol).WithOne(t => t.Form).HasForeignKey(t => t.FormId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CPForm>().HasMany(t => t.GroupCol).WithOne(t => t.Form).HasForeignKey(t => t.FormId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CPForm>().HasMany(t => t.FieldRuleCol).WithOne(t => t.Form).HasForeignKey(t => t.FormId).OnDelete(DeleteBehavior.Cascade);

            //CPForm

            //CPFormChildTable
            modelBuilder.Entity<CPFormChildTable>().ToTable("Form_ChildTable");
            modelBuilder.Entity<CPFormChildTable>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFormChildTable>().Property(t => t.Id).HasColumnName("RelateId");
            //CPFormChildTable

            //CPFormField
            modelBuilder.Entity<CPFormField>().ToTable("Form_Field");
            modelBuilder.Entity<CPFormField>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFormField>().Property(t => t.Id).HasColumnName("FieldId");
            //CPFormField

            //CPFormView
            modelBuilder.Entity<CPFormView>().ToTable("Form_View");
            modelBuilder.Entity<CPFormView>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFormView>().Property(t => t.Id).HasColumnName("ViewId");
            modelBuilder.Entity<CPFormView>().HasMany(t => t.ViewFieldCol).WithOne(t => t.FormView).HasForeignKey(t => t.ViewId).OnDelete(DeleteBehavior.Cascade);
            //CPFormView

            //CPFormViewField
            modelBuilder.Entity<CPFormViewField>().ToTable("Form_View_Inner");
            modelBuilder.Entity<CPFormViewField>().HasKey(t => t.Id);
            //CPFormViewField

            //CPFormUseScene
            modelBuilder.Entity<CPFormUseScene>().ToTable("Form_UseScene");
            modelBuilder.Entity<CPFormUseScene>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFormUseScene>().Property(t => t.Id).HasColumnName("SceneID");
            modelBuilder.Entity<CPFormUseScene>().HasMany(t => t.FuncCol).WithOne(t => t.UseScene).HasForeignKey(t => t.SceneID).OnDelete(DeleteBehavior.Cascade);
            //CPFormUseScene


            //CPFormUseSceneFunc
            modelBuilder.Entity<CPFormUseSceneFunc>().ToTable("Form_UseSceneFunc");
            modelBuilder.Entity<CPFormUseSceneFunc>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFormUseSceneFunc>().Property(t => t.Id).HasColumnName("FuncId");
            modelBuilder.Entity<CPFormUseSceneFunc>().Property(t => t.ShowOrder).HasColumnName("FuncOrder");
            //CPFormUseSceneFunc

            //CPFormGroup
            modelBuilder.Entity<CPFormGroup>().ToTable("Form_Group");
            modelBuilder.Entity<CPFormGroup>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFormGroup>().Property(t => t.Id).HasColumnName("GroupID");
            modelBuilder.Entity<CPFormGroup>().HasMany(t => t.FieldRightCol).WithOne(t => t.RightGroup).HasForeignKey(t => t.GroupID).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CPFormGroup>().HasMany(t => t.FieldInitCol).WithOne(t => t.InitGroup).HasForeignKey(t => t.GroupID).OnDelete(DeleteBehavior.Cascade);
            //CPFormGroup


            //CPFormFieldRight
            modelBuilder.Entity<CPFormFieldRight>().ToTable("Form_FieldRight");
            modelBuilder.Entity<CPFormFieldRight>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFormFieldRight>().Property(t => t.Id).HasColumnName("RightID");
            //CPFormFieldRight

            //CPFormFieldInit
            modelBuilder.Entity<CPFormFieldInit>().ToTable("Form_FieldInitValue");
            modelBuilder.Entity<CPFormFieldInit>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFormFieldInit>().Property(t => t.Id).HasColumnName("InitId");
            //CPFormFieldInit

            //CPFormFieldRule
            modelBuilder.Entity<CPFormFieldRule>().ToTable("Form_Rule");
            modelBuilder.Entity<CPFormFieldRule>().HasKey(t => t.Id);
            modelBuilder.Entity<CPFormFieldRule>().Property(t => t.Id).HasColumnName("RuleId");
            //CPFormFieldRule
            #endregion


            #region 列表
            modelBuilder.Entity<CPGrid>().ToTable("Grid_Main");
            modelBuilder.Entity<CPGrid>().HasKey(t => t.Id);
            modelBuilder.Entity<CPGrid>().Property(t => t.Id).HasColumnName("GridId");
            modelBuilder.Entity<CPGrid>().HasMany(t => t.ColumnCol).WithOne(t => t.Grid).HasForeignKey(t => t.GridId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CPGrid>().HasMany(t => t.FuncCol).WithOne(t => t.Grid).HasForeignKey(t => t.GridId).OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<CPGridColumn>().ToTable("Grid_Column");
            modelBuilder.Entity<CPGridColumn>().HasKey(t => t.Id);
            modelBuilder.Entity<CPGridColumn>().Property(t => t.Id).HasColumnName("ColumnId");
            modelBuilder.Entity<CPGridColumn>().Ignore(t => t.TempSumValue);

            modelBuilder.Entity<CPGridFunc>().ToTable("Grid_Func");
            modelBuilder.Entity<CPGridFunc>().HasKey(t => t.Id);
            modelBuilder.Entity<CPGridFunc>().Property(t => t.Id).HasColumnName("FuncId");
            #endregion


            #region 标签
            modelBuilder.Entity<CPTab>().ToTable("Tab_Main");
            modelBuilder.Entity<CPTab>().HasKey(t => t.Id);
            modelBuilder.Entity<CPTab>().Property(t => t.Id).HasColumnName("TabId");
            modelBuilder.Entity<CPTab>().HasMany(t => t.ItemCol).WithOne(t => t.Tab).HasForeignKey(t => t.TabId).OnDelete(DeleteBehavior.Cascade); 


            modelBuilder.Entity<CPTabItem>().ToTable("Tab_Element");
            modelBuilder.Entity<CPTabItem>().HasKey(t => t.Id);
            modelBuilder.Entity<CPTabItem>().Property(t => t.Id).HasColumnName("EleId");

            #endregion


            #region 树
            modelBuilder.Entity<CPTree>().ToTable("Tree_Main");
            modelBuilder.Entity<CPTree>().HasKey(t => t.Id);
            modelBuilder.Entity<CPTree>().Property(t => t.Id).HasColumnName("TreeId");
            modelBuilder.Entity<CPTree>().HasMany(t => t.FuncCol).WithOne(t => t.Tree).HasForeignKey(t => t.TreeId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CPTree>().HasMany(t => t.DataSourceCol).WithOne(t => t.Tree).HasForeignKey(t => t.TreeId).OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<CPTreeFunc>().ToTable("Tree_Func");
            modelBuilder.Entity<CPTreeFunc>().HasKey(t => t.Id);
            modelBuilder.Entity<CPTreeFunc>().Property(t => t.Id).HasColumnName("FuncId");
            modelBuilder.Entity<CPTreeFunc>().Property(t => t.ShowOrder).HasColumnName("ShowOrder");

            modelBuilder.Entity<CPTreeDataSource>().ToTable("Tree_DataSource");
            modelBuilder.Entity<CPTreeDataSource>().HasKey(t => t.Id);
            modelBuilder.Entity<CPTreeDataSource>().Property(t => t.Id).HasColumnName("SourceId"); 

            #endregion
            //在此设置数据库对应关系 
            base.OnModelCreating(modelBuilder);
        }
    }
}
