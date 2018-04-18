using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using CPFrameWork.Global.Systems;

namespace CPFrameWork.Global
{
    public interface ICPCommonDbContext:IDbContext
    {

    }
    public class CPCommonDbContext : DbContext, ICPCommonDbContext
    {
        public CPCommonDbContext(DbContextOptions<CPCommonDbContext> options):base(options)
        {

        }
        public DbSet<CPAutoNum> CPAutoNumCol { get; set; }
        
        public DbSet<CPSystem> CPSystemCol { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //CP_AutoNum start
            modelBuilder.Entity<CPAutoNum>().ToTable("CP_AutoNum");
            modelBuilder.Entity<CPAutoNum>().HasKey(t => t.Id);
            modelBuilder.Entity<CPAutoNum>().Property(t => t.Id).HasColumnName("AutoId");
            //CP_AutoNum end

            //CPSystem start
            modelBuilder.Entity<CPSystem>().ToTable("CP_System");
            modelBuilder.Entity<CPSystem>().HasKey(t => t.Id);
            modelBuilder.Entity<CPSystem>().Property(t => t.Id).HasColumnName("SysId");
            //CPSystem end
            //在此设置数据库对应关系 
            base.OnModelCreating(modelBuilder);
        }
    }
}
