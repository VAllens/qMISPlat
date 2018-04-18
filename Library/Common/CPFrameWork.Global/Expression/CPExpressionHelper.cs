using CacheManager.Core;
using CPFrameWork.Utility.DbOper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NVelocity;
using NVelocity.App;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq; 
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.Global
{
    [AttributeUsage(AttributeTargets.All)]
     public sealed class CPNameAttribute : Attribute
   {
         private readonly string _name;
        private readonly int _toolType;
  
          public string Name
          {
              get { return _name; }
         }
      public int ToolType
        {
            get { return _toolType; }
        }
        public CPNameAttribute(string name,params int[] toolType)
        {
            _name = name;
            if (toolType.Length > 0)
                this._toolType = toolType[0];
        }
    }
  
    public class CPExpressionHelper
    {
        private VelocityEngine _vltEngine;
        public VelocityContext _vltContext;
        private static CPExpressionHelper _instance = null;
        public static CPExpressionHelper Instance
        {

            get
            {
                //var manager = CacheFactory.Build("CPExpressionHelperBuilder", settings =>
                //{
                //    settings.WithMicrosoftMemoryCacheHandle("CPExpressionHelperHandle");
                //});
                //CPExpressionHelper _helper = manager.Get<CPExpressionHelper>("CPExpressionHelperInstance");
                //if (_helper == null)
                //{
                //    _helper = new CPExpressionHelper();
                //    manager.Add("CPExpressionHelperInstance", _helper);
                //}
                //return _helper;
                if (_instance == null)
                    _instance = new CPExpressionHelper();
                return _instance;

            }
        }
        public CPExpressionHelper()
        {
            _vltEngine = new VelocityEngine();
            _vltEngine.Init();

            _vltContext = new VelocityContext();
            _vltContext.Put("CPContext", new CPRuntimeContext());
            _vltContext.Put("CPUser", new CPUserIden());
            _vltContext.Put("CPGrid", Activator.CreateInstance(Type.GetType("CPFrameWork.UIInterface.Grid.CPGridExpression,CPFrameWork.UIInterface"),new object[] { _vltContext}));
            _vltContext.Put("CPForm", Activator.CreateInstance(Type.GetType("CPFrameWork.UIInterface.Form.CPFormExpression,CPFrameWork.UIInterface"), new object[] { _vltContext }));
            _vltContext.Put("CPTree", Activator.CreateInstance(Type.GetType("CPFrameWork.UIInterface.Tree.CPTreeExpression,CPFrameWork.UIInterface"), new object[] { _vltContext }));
            _vltContext.Put("CPFlow", Activator.CreateInstance(Type.GetType("CPFrameWork.Flow.CPFlowExpression,CPFrameWork.Flow"), new object[] { _vltContext }));
        }
        public void Add(string key, object obj)
        {
            this._vltContext.Put(key, obj);
        }
        public void Remove(string key)
        {
            this._vltContext.Remove(key);
        }
        public string RunCompile(string template)
        {
            try
            {
               
               //注意，新版本表达式引擎，不支持获取类的属性，得用方法代替
                if (string.IsNullOrEmpty(template))
                    return "";
                //由于不知道为什么这个模板解析在.netcore 下运行有点慢，所以暂时如果没有表达式，则不运行了
                if (template.IndexOf("${") == -1)
                    return template;
                System.IO.StringWriter vltWriter = new System.IO.StringWriter();
              
                _vltEngine.Evaluate(_vltContext, vltWriter, null, template.ToString());

                string s = vltWriter.GetStringBuilder().ToString();
                vltWriter.Close();
                return s;
            }
            catch (NullReferenceException)
            {
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }


    }  
    /// <summary>  
    /// 运行时上下文  
    /// </summary>  
    [CPName("运行时参数",0)]
    public class CPRuntimeContext
    {
        [CPName("从session中获取值")]
        public string Session([CPName("Key")] string key)
        {
            string s = CPAppContext.GetHttpContext().Session.GetString(key);
            if (string.IsNullOrEmpty(s))
                return "";
            else
                return s;
        }
        [CPName("字符串转换成大写")]
        public string ToUpper([CPName("待转换字符串")]string sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return sValue;
            return sValue.ToUpper();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ins"></param>
        /// <returns></returns>
        [CPName("获取数据库名称")]
        public string GetDbName([CPName("数据库链接实例")]string ins)
        {
            DbHelper _helper = new DbHelper(ins,CPAppContext.CurDbType());
            string db = _helper.GetConnection().Database;
            _helper = null;
            return db;
        }
        [CPName("获取页面地址参数值")]
        public string QueryString([CPName("参数key")]string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";

            object obj = CPAppContext.GetHttpContext().Request.Query[key];
            if (obj == null)
                return "";
            else
                return obj.ToString();
        }
        [CPName("取当前年")]
        public string CurYear()
        {
            
                return DateTime.Now.Year.ToString();
             
        }
        [CPName("取当前年，返回后两位")]
        public string CurYearShort()
        {
          
                string sYear = DateTime.Now.Year.ToString();
                sYear = sYear.Substring(2, 2);
                return sYear;
            
        }
        [CPName("取当前月")]
        public string CurMonth()
        {
          
                return DateTime.Now.Month.ToString("00");
             
        }
        [CPName("取年月字符串")]
        public string CurMonthWithYear()
        {
           
                DateTime d = DateTime.Now;
                return d.Year + "-" + d.Month.ToString();
          
        }
        [CPName("取前一个月")]
        public string PreMonth()
        {
            
                DateTime d = DateTime.Now.AddMonths(-1);
                return d.Year + "-" + d.Month.ToString();
           
        }
        [CPName("取后一个月")]
        public string NextMonth()
        {
             
                DateTime d = DateTime.Now.AddMonths(1);
                return d.Year + "-" + d.Month.ToString();
           
        }
        [CPName("取当天")]
        public string CurDay()
        {
           
                return DateTime.Now.Day.ToString("00");
            
        }
        [CPName("取当前时间")]
        public string CurTime()
        {
             
                return DateTime.Now.ToString();
           
        }
        [CPName("取当前时间，只返回天")]
        public string CurTimeShortDate()
        {
            
                //需要按2010-08-09格式组成，
                int year = DateTime.Now.Year;
                int month = DateTime.Now.Month;
                int day = DateTime.Now.Day;
                string time = year + "-";
                if (month < 10)
                    time += "0" + month;
                else
                    time += month;
                if (day < 10)
                    time += "-0" + day;
                else
                    time += "-" + day;
                return time;
            
        }
        [CPName("取当前时间字符串，包括时分秒")]
        public string CurTimeLongString()
        {
           
                DateTime curTime = DateTime.Now;
                string month = curTime.Month.ToString();
                string day = curTime.Day.ToString();
                string hour = curTime.Hour.ToString();
                string minute = curTime.Minute.ToString();
                string second = curTime.Second.ToString();
                if (curTime.Month < 10)
                    month = "0" + month;
                if (curTime.Day < 10)
                    day = "0" + day;
                if (curTime.Hour < 10)
                    hour = "0" + hour;
                if (curTime.Minute < 10)
                    minute = "0" + minute;
                if (curTime.Second < 10)
                    second = "0" + second;
                string s = curTime.Year.ToString() + month
                    + day + hour + minute + second;
                return s;
          
        }
        [CPName("取当前时分秒字符串")]
        public string HHMMSSLongString()
        {
           
                DateTime curTime = DateTime.Now;
               
                string hour = curTime.Hour.ToString();
                string minute = curTime.Minute.ToString();
                string second = curTime.Second.ToString();
                
                if (curTime.Hour < 10)
                    hour = "0" + hour;
                if (curTime.Minute < 10)
                    minute = "0" + minute;
                if (curTime.Second < 10)
                    second = "0" + second;
                string s =   hour + minute + second;
                return s;
           
        }
        [CPName("取GUID")]
        public string NewGUID()
        {
            
                return Guid.NewGuid().ToString();
          
        }
        [CPName("取网站根路径")]
        public string CPWebRootPath()
        {
            return CPAppContext.CPWebRootPath();
        }
    }

    /// <summary>
    /// 用户登录session
    /// </summary>
    [CPName("用户登录Session相关",0)]
    public class CPUserIden
    {
        [CPName("当前用户Id")]
        public int UserId()
        {
            string UserId = CPAppContext.GetHttpContext().Session.GetString("UserId");
            if (string.IsNullOrEmpty(UserId))
                return 0;
            else
                return int.Parse(UserId);
        }
        [CPName("用户SessionIden")]
        public string UserIden()
        { 
            string UserKey = CPAppContext.GetHttpContext().Session.GetString("UserKey");
            if (string.IsNullOrEmpty(UserKey))
                return "";
            else
                return UserKey;
        }
        [CPName("用户姓名")]
        public string UserName()
        {
            string UserName = CPAppContext.GetHttpContext().Session.GetString("UserName");
            if (string.IsNullOrEmpty(UserName))
                return "";
            else
                return UserName;
        }
        [CPName("用户登录名")]
        public string UserLoginName()
        {
            string UserLoginName = CPAppContext.GetHttpContext().Session.GetString("UserLoginName");
            if (string.IsNullOrEmpty(UserLoginName))
                return "";
            else
                return UserLoginName;
        }
        [CPName("用户照片地址")]
        public string UserPhotoPath()
        {
            string UserPhotoPath = CPAppContext.GetHttpContext().Session.GetString("UserPhotoPath");
            if (string.IsNullOrEmpty(UserPhotoPath))
                return "";
            else
                return UserPhotoPath;
        }
        [CPName("当前用户所属角色ID，多个用，分隔")]
        public string UserRoleIds()
        {
            string RoleIds = CPAppContext.GetHttpContext().Session.GetString("RoleIds");
            if (string.IsNullOrEmpty(RoleIds))
                return "";
            else
                return RoleIds;
        }
        [CPName("当前用户所属角色姓名，多个用，分隔")]
        public string UserRoleNames()
        {
            string RoleNames = CPAppContext.GetHttpContext().Session.GetString("RoleNames");
            if (string.IsNullOrEmpty(RoleNames))
                return "";
            else
                return RoleNames;
        }
        [CPName("当前用户所属部门ID，多个用，分隔")]
        public string DepIds()
        {
            string DepIds = CPAppContext.GetHttpContext().Session.GetString("DepIds");
            if (string.IsNullOrEmpty(DepIds))
                return "";
            else
                return DepIds;
        }
        [CPName("当前用户所属部门姓名，多个用，分隔")]
        public string DepNames()
        {
            string DepNames = CPAppContext.GetHttpContext().Session.GetString("DepNames");
            if (string.IsNullOrEmpty(DepNames))
                return "";
            else
                return DepNames;
        }
        [CPName("获取当前用户拥有管理员权限的子系统ID，多个用，分隔")]
        public string UserAdminSysIds()
        {
            string UserAdminSysIds = CPAppContext.GetHttpContext().Session.GetString("UserAdminSysIds");
            if (string.IsNullOrEmpty(UserAdminSysIds))
                return "";
            else
                return UserAdminSysIds;
        }
    }
   
}
