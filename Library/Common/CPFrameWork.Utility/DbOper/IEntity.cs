using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.Utility.DbOper
{
    /// <summary>
    /// 所有领域对象的基础接口
    /// </summary>
    public interface IEntity
    {
        int Id { get; set; }
        /// <summary>
        /// 处理默认值，此方法用来处理从数据库里读取值之后，某些数据为空是，统一给予默认值
        /// </summary>
        void FormatInitValue(); 

    }
    public abstract class  BaseEntity: IEntity
    {
      public   int Id { get; set; }
        /// <summary>
        /// 处理默认值，此方法用来处理从数据库里读取值之后，某些数据为空是，统一给予默认值
        /// </summary>
      public virtual  void FormatInitValue()
        {

        }
       

    }
    /// <summary>
    /// 带排序字段的实体类
    /// </summary>
    public  abstract class BaseOrderEntity : BaseEntity
    {
         
        /// <summary>
        /// 显示顺序
        /// </summary>
        public int? ShowOrder
        {
            get;set;
        }

    }
    public  class IBaseList<TEntity>: List<TEntity>
        where TEntity : class,IEntity
    {
        public TEntity GetById(int id)
        {
            TEntity entity = null;
            foreach(TEntity item in this)
            {
                if(item.Id.Equals(id))
                {
                    entity = item;
                    break;
                }
            }
            return entity;
        }
        public static IBaseList<TEntity> InitFromIQueryable(IQueryable<TEntity>  col)
        {
            IBaseList<TEntity> colNew = new IBaseList<TEntity>();
            colNew.AddRange(col);
            return colNew;
        }
        public static T InitFromIList<T>(IList<TEntity> col)
            where T :IBaseList<TEntity>,new()
        {
            T colNew = new T();
            colNew.AddRange(col);
            return colNew;
        }
    }
    public class IBaseOrderList<TEntity> : IBaseList<TEntity> 
        where TEntity :BaseOrderEntity
    {
        /// <summary>
        /// 根据节点顺序进行排序
        /// </summary>
        public void SortByShowIndex()
        {
            this.Sort(CompareDinosByDispOrder);
        }
        private static int CompareDinosByDispOrder(TEntity x, TEntity y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    int retval = x.ShowOrder.Value.CompareTo(y.ShowOrder.Value);

                    if (retval != 0)
                    {
                        // If the strings are not of equal length,
                        // the longer string is greater.
                        //
                        return retval;
                    }
                    else
                    {
                        // If the strings are of equal length,
                        // sort them with ordinary string comparison.
                        //
                        return x.ShowOrder.Value.CompareTo(y.ShowOrder.Value);
                    }
                }
            }

        }
    }

}
