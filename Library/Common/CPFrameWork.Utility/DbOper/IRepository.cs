
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq.Parsing.Structure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query;

namespace CPFrameWork.Utility.DbOper
{ 

    public interface IRepository<TEntity>
        where TEntity : class,IEntity,new()
    {
        int Add(TEntity entity);
        int Add(IList<TEntity> entityCol);
        int AddOneByOne(IList<TEntity> entityCol);
        TEntity Get(int id, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);
        T GetMax<T>(Expression<Func<TEntity, T>> maxExpression, ISpecification<TEntity> specification);
        IList<TEntity> Get(params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);
        IList<TEntity> GetByCondition(ISpecification<TEntity> specification, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);
        IList<TEntity> GetByCondition(ISpecification<TEntity> specification, Expression<Func<TEntity, dynamic>> orderBy,
            CPEFEnum.SortOrder orderType, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);
        CPPagedResult<TEntity> GetByCondition(ISpecification<TEntity> specification,
            Expression<Func<TEntity, dynamic>> orderBy,
            CPEFEnum.SortOrder orderType,
            int pageSize, int currentPage, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);
        int Count(ISpecification<TEntity> specification);
        IList<TEntity> SqlQuery(string sql);
        int Update(TEntity entity);
        int Update(IList<TEntity> entityCol);
        int Delete(TEntity entity);
        int Delete(IList<TEntity> entityCol);
        int Delete(int id);
        int Delete(List<int> idCol);
        int DeleteByCondition(Expression<Func<TEntity, bool>> whereCondition);
        


    }

    public abstract class BaseRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity,new()
    {
        private static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();
        private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");
        private static readonly PropertyInfo NodeTypeProviderField = QueryCompilerTypeInfo.DeclaredProperties.Single(x => x.Name == "NodeTypeProvider");
        private static readonly MethodInfo CreateQueryParserMethod = QueryCompilerTypeInfo.DeclaredMethods.First(x => x.Name == "CreateQueryParser");
        private static readonly FieldInfo DataBaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");
        private static readonly PropertyInfo DatabaseDependenciesProperty = typeof(Microsoft.EntityFrameworkCore.Storage.Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");

        /// <summary>
        /// 用于监控生成的sql语句
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public string GetTraceString<T>(IQueryable<T> query)
        {
            var str = query.ToString();
            var provider = query.Provider;
            var fields = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields;
            var properties = typeof(EntityQueryProvider).GetTypeInfo().DeclaredProperties;
            try
            {
                var queryCompiler = QueryCompilerField.GetValue(query.Provider);
                var nodeTypeProvider = (INodeTypeProvider)NodeTypeProviderField.GetValue(queryCompiler);
                var parser = (IQueryParser)CreateQueryParserMethod.Invoke(queryCompiler, new object[] { nodeTypeProvider });
                var queryModel = parser.GetParsedQuery(query.Expression);
                var database = DataBaseField.GetValue(queryCompiler);
                var databaseDependencies = (DatabaseDependencies)DatabaseDependenciesProperty.GetValue(database);
                var queryCompilationContextFactory = databaseDependencies.QueryCompilationContextFactory;
                var queryCompilationContext = queryCompilationContextFactory.Create(false);
                var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
                modelVisitor.CreateQueryExecutor<T>(queryModel);
                var sql = modelVisitor.Queries.First().ToString();

                return sql;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }


        protected IDbContext _dbContext;
        public BaseRepository(IDbContext dbContext)
        {
            this._dbContext = dbContext;
        }
        private MemberExpression GetMemberInfo(LambdaExpression lambda)
        {
            if (lambda == null)
                throw new ArgumentNullException("method");

            MemberExpression memberExpr = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr =
                    ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null)
                throw new ArgumentException("method");

            return memberExpr;
        }

        protected string GetEagerLoadingPath(Expression<Func<TEntity, dynamic>> eagerLoadingProperty)
        {
            MemberExpression memberExpression = this.GetMemberInfo(eagerLoadingProperty);
            var parameterName = eagerLoadingProperty.Parameters.First().Name;
            var memberExpressionStr = memberExpression.ToString();
            var path = memberExpressionStr.Replace(parameterName + ".", "");
            return path;
        }
        public virtual int Add(TEntity entity)
        {
            this._dbContext.Set<TEntity>().Add(entity);
            this._dbContext.Entry<TEntity>(entity).State = EntityState.Added;
            return this._dbContext.SaveChanges();
        }
        public virtual int Add(IList<TEntity> entityCol)
        {
            entityCol.ToList().ForEach(t => {
                this._dbContext.Set<TEntity>().Add(t);
                this._dbContext.Entry<TEntity>(t).State = EntityState.Added;
            });            
            return this._dbContext.SaveChanges();
        }
        public virtual int AddOneByOne(IList<TEntity> entityCol)
        {
            //由于ef core批量写入数据时，采用了批处理方法，但这种方式会导致写入顺序错，暂时没有找到什么方法，所以改成一条条写。
            entityCol.ToList().ForEach(t => {
                this._dbContext.Set<TEntity>().Add(t);
                this._dbContext.Entry<TEntity>(t).State = EntityState.Added;
                this._dbContext.SaveChanges();
            });
            return entityCol.Count;
        }
        public virtual IList<TEntity> Get(params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        { 
            
            DbSet<TEntity> dbSet = this._dbContext.Set<TEntity>();
            IList<TEntity> col = null;
            if (eagerLoadingProperties != null &&
               eagerLoadingProperties.Length > 0)
            {
                var eagerLoadingProperty = eagerLoadingProperties[0];
                var eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                var dbquery = dbSet.Include(eagerLoadingPath);
                for (int i = 1; i < eagerLoadingProperties.Length; i++)
                {
                    eagerLoadingProperty = eagerLoadingProperties[i];
                    eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                    dbquery = dbquery.Include(eagerLoadingPath);
                }
                col = dbquery.ToList();

            }
            else
            {
                col = dbSet.ToList();
            }
          
            col.ToList().ForEach(t => { 
                t.FormatInitValue(); });
            return col;
        }
        public virtual T GetMax<T>(Expression<Func<TEntity, T>> maxExpression, ISpecification<TEntity> specification)
        {
            DbSet<TEntity> dbSet = this._dbContext.Set<TEntity>();
            T t = dbSet.Where(specification.GetExpression()).Max(maxExpression);
            return t;
        }

        public virtual TEntity Get(int id,params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            TEntity entity = null;
            if (eagerLoadingProperties != null &&
              eagerLoadingProperties.Length > 0)
            {
                DbSet<TEntity> dbSet = this._dbContext.Set<TEntity>();
                var eagerLoadingProperty = eagerLoadingProperties[0];
                var eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                var dbquery = dbSet.Include(eagerLoadingPath);
                for (int i = 1; i < eagerLoadingProperties.Length; i++)
                {
                    eagerLoadingProperty = eagerLoadingProperties[i];
                    eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                    dbquery = dbquery.Include(eagerLoadingPath);
                }
                  entity =  dbquery.Where(c => c.Id.Equals(id)).First();


            }
            else
            {
                  entity = this._dbContext.Set<TEntity>().Find(id);
                
            }
            if (entity != null)
            {
                entity.FormatInitValue();
            }
            return entity;
        }
         
        public virtual IList<TEntity> GetByCondition(ISpecification<TEntity> specification, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {

            return this.GetByCondition(specification, null, CPEFEnum.SortOrder.Asc,eagerLoadingProperties);
        }
       public virtual IList<TEntity> GetByCondition(ISpecification<TEntity> specification, 
           Expression<Func<TEntity, dynamic>> orderBy,
         CPEFEnum.SortOrder orderType
           , params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            
            DbSet<TEntity> dbSet = this._dbContext.Set<TEntity>();
            IQueryable<TEntity> queryable = null; //this._dbContext.Set<TEntity>().Where(specification.GetExpression());
            if (eagerLoadingProperties != null &&
              eagerLoadingProperties.Length > 0)
            { 
                var eagerLoadingProperty = eagerLoadingProperties[0];
                var eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                var dbquery = dbSet.Include(eagerLoadingPath);
                for (int i = 1; i < eagerLoadingProperties.Length; i++)
                {
                    eagerLoadingProperty = eagerLoadingProperties[i];
                    eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                    dbquery = dbquery.Include(eagerLoadingPath);
                }
                queryable = dbquery.Where(specification.GetExpression()); 
            }
            else
            {
                queryable = dbSet.Where(specification.GetExpression());
            }
            if (orderBy != null)
            {
                if(orderType == CPEFEnum.SortOrder.Asc)
                {
                    queryable = queryable.SortBy(orderBy);
                }
                else
                    queryable = queryable.SortByDescending(orderBy);
            }

 
             //string Sql = this.GetTraceString<TEntity>(queryable);
 
            //
            List<TEntity> col = queryable.ToList();
             
            col.ToList().ForEach(t => { t.FormatInitValue(); });
            return col;
        }
        public virtual CPPagedResult<TEntity> GetByCondition(
            ISpecification<TEntity> specification,
            Expression<Func<TEntity, dynamic>> orderBy,
               CPEFEnum.SortOrder orderType,
            int pageSize, int currentPage
            , params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            DbSet<TEntity> dbSet = this._dbContext.Set<TEntity>();
            IQueryable<TEntity> queryable = null;// this._dbContext.Set<TEntity>().Where(specification.GetExpression());
            if (eagerLoadingProperties != null &&
             eagerLoadingProperties.Length > 0)
            {
                var eagerLoadingProperty = eagerLoadingProperties[0];
                var eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                var dbquery = dbSet.Include(eagerLoadingPath);
                for (int i = 1; i < eagerLoadingProperties.Length; i++)
                {
                    eagerLoadingProperty = eagerLoadingProperties[i];
                    eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                    dbquery = dbquery.Include(eagerLoadingPath);
                }
                queryable = dbquery.Where(specification.GetExpression());
            }
            else
            {
                queryable = dbSet.Where(specification.GetExpression());
            }
            IQueryable<TEntity> queryableData = this._dbContext.Set<TEntity>().Where(specification.GetExpression());
             if (orderType == CPEFEnum.SortOrder.Asc)
            {
                queryableData = queryable.SortBy(orderBy);
              
            }
            else
            {
                queryableData = queryable.SortByDescending(orderBy);
            }
            queryableData = queryableData.Skip(currentPage * (currentPage - 1))//跳过行数，最终生成的sql语句是Top(n)
                .Take(pageSize).AsQueryable();//返回指定数量的行 
            int nCount = queryable.Count();//获取总记录数

            List<TEntity> col = queryableData.ToList();
            
            return new CPPagedResult<TEntity>(nCount,
                (nCount + pageSize - 1) / pageSize,
                pageSize,
                currentPage, col);
        }
        public int Count(ISpecification<TEntity> specification)
        {
            DbSet<TEntity> dbSet = this._dbContext.Set<TEntity>();
            return dbSet.Where(specification.GetExpression()).Count();
        }
        public IList<TEntity> SqlQuery(string sql)
        {

            //  List<TEntity> col = this._dbContext.Database.SqlQuery<TEntity>(sql).ToList();
            List<TEntity> col = this._dbContext.Set<TEntity>().FromSql<TEntity>(sql).ToList();
           col.ForEach(t => { 
                t.FormatInitValue();
            });
            return col;
        }
        public virtual int Update(TEntity entity)
        {
            var dbEntity = this._dbContext.Entry<TEntity>(entity); 
            if (dbEntity.State ==EntityState.Modified)
            {
                var entityToUpdate = this._dbContext.Set<TEntity>().Find(entity.Id);
                //EmitMapper.ObjectMapperManager.DefaultInstance.GetMapper<TEntity, TEntity>()
                //                              .Map(entity, entityToUpdate);
                entityToUpdate= AutoMapper.Mapper.Map<TEntity>(entity);
               return   this._dbContext.SaveChanges();

            }
            return 1;
           
        }
        public virtual int Update(IList<TEntity> entityCol)
        {
            entityCol.ToList().ForEach(t => {
                var dbEntity = this._dbContext.Entry<TEntity>(t); 
                if (dbEntity.State == EntityState.Modified)
                {
                    var entityToUpdate = this._dbContext.Set<TEntity>().Find(t.Id);
                    //EmitMapper.ObjectMapperManager.DefaultInstance.GetMapper<TEntity, TEntity>()
                    //                              .Map(t, entityToUpdate);
                    entityToUpdate = AutoMapper.Mapper.Map<TEntity>(t);


                }
            });
            return this._dbContext.SaveChanges();
            
        }
        public virtual int Delete(TEntity entity)
        {
            this._dbContext.Set<TEntity>().Remove(entity);
            this._dbContext.Entry<TEntity>(entity).State = EntityState.Deleted;
            return this._dbContext.SaveChanges();
        }
        public virtual int Delete(IList<TEntity> entityCol)
        {
            entityCol.ToList().ForEach(t => {
                this._dbContext.Set<TEntity>().Remove(t);
                this._dbContext.Entry<TEntity>(t).State = EntityState.Deleted;
            });
            return  this._dbContext.SaveChanges();
        }
        public virtual int Delete(int id)
        {
            TEntity entity = this.Get(id);
            this._dbContext.Set<TEntity>().Remove(entity);
            this._dbContext.Entry<TEntity>(entity).State = EntityState.Deleted;
            return this._dbContext.SaveChanges();
        }
        public int Delete(List<int> idCol)
        {
            idCol.ForEach(t => {
                TEntity entity = this.Get(t);
                this._dbContext.Set<TEntity>().Remove(entity);
                this._dbContext.Entry<TEntity>(entity).State =EntityState.Deleted;
            });
            
            return this._dbContext.SaveChanges();
        }
        public virtual int DeleteByCondition(Expression<Func<TEntity, bool>> whereCondition)
        {
            Specification<TEntity> specification = ExpressionSpecification<TEntity>.Eval(whereCondition);
            IList<TEntity> col =  this.GetByCondition(specification);
            this._dbContext.Set<TEntity>().RemoveRange(col);
            col.ToList().ForEach(t => {
                this._dbContext.Entry<TEntity>(t).State = EntityState.Deleted;
            });
            return this._dbContext.SaveChanges();
        }
    }
}
