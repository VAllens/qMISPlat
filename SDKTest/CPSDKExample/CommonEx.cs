using System;
using System.Collections.Generic;
using System.Text;

namespace CPSDKExample
{
    public class CommonEx
    {
        /// <summary>
        /// 获取当前登录用户的用户ID
        /// </summary>
        /// <returns></returns>
        public static int GetCurUserId()
        {
            return int.Parse(CPFrameWork.Global.CPExpressionHelper.Instance.RunCompile("${CPUser.UserId()}"));
        }
        /// <summary>
        /// 获取当前登录用户的用户Key
        /// </summary>
        /// <returns></returns>
        public static string  GetCurUserIden()
        {
            return  CPFrameWork.Global.CPExpressionHelper.Instance.RunCompile("${CPUser.UserIden()}") ;
        }
        /// <summary>
        /// 获取当前登录用户的用户姓名 
        /// </summary>
        /// <returns></returns>
        public static string GetCurUserName()
        {
            return CPFrameWork.Global.CPExpressionHelper.Instance.RunCompile("${CPUser.UserName()}");
        }
        /// <summary>
        /// 获取当前登录用户的用户登录名
        /// </summary>
        /// <returns></returns>
        public static string GetCurUserLoginName()
        {
            return CPFrameWork.Global.CPExpressionHelper.Instance.RunCompile("${CPUser.UserLoginName()}");
        }
        /// <summary>
        /// 获取当前登录用户的用户所属角色ID
        /// </summary>
        /// <returns></returns>
        public static string GetCurUserRoleIds()
        {
            return CPFrameWork.Global.CPExpressionHelper.Instance.RunCompile("${CPUser.UserRoleIds()}");
        }
        /// <summary>
        /// 获取当前登录用户的用户所属部门ID
        /// </summary>
        /// <returns></returns>
        public static string GetCurDepIds()
        {
            return CPFrameWork.Global.CPExpressionHelper.Instance.RunCompile("${CPUser.DepIds()}");
        }
    }
}
