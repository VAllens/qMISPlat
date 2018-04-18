using CPFrameWork.Global.Msg;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.Global
{
    public interface ICPFrameDbContext : IDbContext
    {

    }
    public class CPFrameDbContext : DbContext, ICPFrameDbContext
    {
        public CPFrameDbContext(DbContextOptions<CPFrameDbContext> options):base(options)
        {

        }
        public DbSet<CPLog> CODepCol { get; set; }

        public DbSet<CPMsgEntity> CPMsgEntityCol { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //CPLog start
            modelBuilder.Entity<CPLog>().ToTable("CP_Log");
            modelBuilder.Entity<CPLog>().HasKey(t => t.Id);
            //CPLog end
            //CPMsgDbEntity start
            modelBuilder.Entity<CPMsgEntity>().ToTable("CP_Msg");
            modelBuilder.Entity<CPMsgEntity>().HasKey(t => t.Id);
            //CPMsgDbEntity end
            //在此设置数据库对应关系 
            base.OnModelCreating(modelBuilder);
        }
    }
}
