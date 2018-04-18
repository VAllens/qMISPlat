using CPFrameWork.Global;
using CPFrameWork.Organ.Domain;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.Organ.Infrastructure
{ 
    public interface ICODbContext : IDbContext
    {

    }
    public class CODbContext : DbContext, ICODbContext
    {
        public CODbContext()
        {
        }
        public CODbContext( DbContextOptions<CODbContext> options):base(options)
        {

        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    //if (CPAppContext.CurDbType() == DbHelper.DbTypeEnum.SqlServer)
        //    //{
        //    //    optionsBuilder.UseSqlServer(CPAppContext.Configuration.GetSection("ConnectionStrings")["CPOrganIns"]);
        //    //}
        //}
        public DbSet<CODep> CODepCol { get; set; }
        public DbSet<COUser> COUserCol { get; set; }
        public DbSet<CORole> CORoleCol { get; set; }
        public DbSet<COUserIdentity> COUserIdentityCol { get; set; }
        public DbSet<CODepUserRelate> CODepUserRelateCol { get; set; }
        public DbSet<CORoleUserRelate> CORoleUserRelateCol { get; set; }



        protected override void OnModelCreating( ModelBuilder modelBuilder)
        {

            //CODep
            modelBuilder.Entity< CODep>().ToTable("V_CP_Dep");
            modelBuilder.Entity<CODep>().HasKey(t => t.Id);
            modelBuilder.Entity<CODep>().Property(t => t.Id).HasColumnName("DepId");
            modelBuilder.Entity<CODep>().Ignore(t => t.UserCol);
            //配置关联关系,并配置级联删除
            //modelBuilder.Entity<CODep>()..HasMany<COUser>(c => c.UserCol).WithMany(t => t.DepCol).Map(
            //    m =>
            //    {
            //        m.ToTable("V_CP_DepUser");//关联表名
            //        m.MapLeftKey("UserId");
            //        m.MapRightKey("DepId");

            //    }
            //    );
            //CODep
            //CODepUserRelate 
            modelBuilder.Entity<CODepUserRelate>().ToTable("V_CP_DepUser");
            modelBuilder.Entity<CODepUserRelate>().HasKey(t => new { t.Id  });
            modelBuilder.Entity<CODepUserRelate>().HasOne(t => t.Dep).WithMany(p => p.DepUserCol).HasForeignKey(c => c.DepId);
            modelBuilder.Entity<CODepUserRelate>().HasOne(t => t.User).WithMany(p => p.DepUserCol).HasForeignKey(c => c.UserId);
            //CODepUserRelate 

            //COUser 
            modelBuilder.Entity<COUser>().ToTable("V_CP_User");
            modelBuilder.Entity<COUser>().HasKey(t => t.Id);
            modelBuilder.Entity<COUser>().Property(t => t.Id).HasColumnName("UserId");
            modelBuilder.Entity<COUser>().Ignore(t => t.DepCol);
            modelBuilder.Entity<COUser>().Ignore(t => t.RoleCol);
            //COUser 

            //CORole 
            modelBuilder.Entity<CORole>().ToTable("V_CP_Role");
            modelBuilder.Entity<CORole>().HasKey(t => t.Id);
            modelBuilder.Entity<CORole>().Property(t => t.Id).HasColumnName("RoleId");
            modelBuilder.Entity<CORole>().Ignore(t => t.UserCol);
            //CORole 

            //CORoleUserRelate 
            modelBuilder.Entity<CORoleUserRelate>().ToTable("V_CP_RoleUser");
            modelBuilder.Entity<CORoleUserRelate>().HasKey(t => new { t.Id });
            modelBuilder.Entity<CORoleUserRelate>().HasOne(t => t.Role).WithMany(p => p.RoleUserCol).HasForeignKey(c => c.RoleId);
            modelBuilder.Entity<CORoleUserRelate>().HasOne(t => t.User).WithMany(p => p.RoleUserCol).HasForeignKey(c => c.UserId);
            //CORoleUserRelate 
            //COUserIdentity 
            modelBuilder.Entity<COUserIdentity>().ToTable("CP_UserIdentity");
            modelBuilder.Entity<COUserIdentity>().HasKey(t => t.Id);
            //COUserIdentity 


            //在此设置数据库对应关系 
            base.OnModelCreating(modelBuilder);
        }
    }
}
