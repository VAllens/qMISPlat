using System;
using System.Collections.Generic;
using System.Text;

namespace CPFrameWork.Flow
{
   public  class CPFlowEnum
    { 

        public enum LinkTypeEnum
        {
            /// <summary>
            /// 提交
            /// </summary>
            Submit = 1,
            /// <summary>
            /// 回退
            /// </summary>
            Fallback = 2
        }
        public enum RebackTaskSubmitTypeEnum
        {
            /// <summary>
            /// 直接提交给回退节点
            /// </summary>
            ToRebackPhase = 1,
            /// <summary>
            /// 根据流程配置，提交给阶段的后续节点
            /// </summary>
            ToNextPhase = 2
        }
        public enum TaskTypeEnum
        {
            /// <summary>
            /// 普通任务
            /// </summary>
            NormalTask = 1,
            /// <summary>
            /// 传阅任务,无需办理
            /// </summary>
            MissionTask = 2
        }
        public enum TaskMakeTypeEnum
        {
            /// <summary>
            /// 提交
            /// </summary>
            Submit = 1,
            /// <summary>
            /// 回退
            /// </summary>
            Reback = 2,
            /// <summary>
            /// 取回
            /// </summary>
            Retrieve = 3
        }
        public enum TaskStateEnum
        {
            /// <summary>
            /// 已激活
            /// </summary>
            AlreadyActivated = 1,
            /// <summary>
            /// 未激活
            /// </summary>
            NotActive = 0,
            /// <summary>
            /// 已暂停
            /// </summary>
            Paused = 2
        }
        public static string GetInsStateString(InsStateEnum insState)
        {
            if (insState == InsStateEnum.Start)
                return "启动中";
            else if (insState == InsStateEnum.Normal)
                return "办理中";
            else if (insState == InsStateEnum.Pause)
                return "暂停";
            else if (insState == InsStateEnum.End)
                return "正常结束";
            else if (insState == InsStateEnum.Stop)
                return "终止";
            else
                return "";
        }
        public enum InsStateEnum
        {
            /// <summary>
            /// 启动中
            /// </summary>
            Start = 1,
            /// <summary>
            /// 办理中
            /// </summary>
            Normal = 2,
            /// <summary>
            /// 暂停
            /// </summary>
            Pause = 3,
            /// <summary>
            /// 正常结束
            /// </summary>
            End = 4,
            /// <summary>
            /// 终止
            /// </summary>
            Stop = 5
        }
        public enum FlowTypeEnum
        {
            /// <summary>
            /// 任意式工作流
            /// </summary>
            FreeFlow = 1,
            /// <summary>
            /// 顺序式工作流
            /// </summary>
            OrderFlow = 2
        }
        public enum FlowVerStateEnum
        {
            /// <summary>
            /// 修改中
            /// </summary>
            Editing =0,
            /// <summary>
            /// 已发布
            /// </summary>
            Release = 1
        }
        public enum FlowRebackModelEnum
        {
            /// <summary>
            /// 任意节点回退均回退到发起节点
            /// </summary>
            ToFirst = 1,
            /// <summary>
            /// 回退到上一节点
            /// </summary>
            ToPre = 2
        }

        public enum PhaseTypeEnum
        {
            /// <summary>
            /// 开始节点
            /// </summary>
            Start = 1,
            /// <summary>
            /// 普通节点
            /// </summary>
            Normal = 2,
            /// <summary>
            /// 会签节点
            /// </summary>
            Meet = 3,
            /// <summary>
            /// 传阅节点
            /// </summary>
            Circulation = 4,
            /// <summary>
            /// 结束节点
            /// </summary>
            End = 5
        }
        public enum UserSelScaleEnum
        {
            /// <summary>
            /// 所有用户
            /// </summary>
            AllUser = 1,
            /// <summary>
            /// 指定 用户
            /// </summary>
            AppointUser = 2
        }

        public enum LinkConditionTypeEnum
        {
            /// <summary>
            /// 没有条件
            /// </summary>
            NoCondition = 0,
            /// <summary>
            /// 根据表达式判断
            /// </summary>
            ByExpression = 1,
            /// <summary>
            /// SQL语句判断，支持表达式替换
            /// </summary>
            BySql = 2,
            /// <summary>
            /// 3：扩展类
            /// </summary>
            ByClass = 3
        }


        public enum RuleTypeEnum
        {
            /// <summary>
            /// 提交任务
            /// </summary>
            Submit = 1,
            /// <summary>
            /// 回退任务
            /// </summary>
            Reback = 2
        }

        public enum RuleCompareTypeEnum {
            BySql = 1,
            ByClass = 2
        }

        public enum CompareTypeEnum
        {
            /// <summary>
            /// 等于
            /// </summary>
            Equal = 1,
            /// <summary>
            /// 不等于
            /// </summary>
            NotEqual = 2,
            /// <summary>
            /// 大于
            /// </summary>
            Greater = 3,
            /// <summary>
            /// 大于等于
            /// </summary>
            GreaterAndEqual = 4,
            /// <summary>
            /// 小于
            /// </summary>
            Less = 5,
            /// <summary>
            /// 小于等于
            /// </summary>
            LessAndEqual = 6,
            /// <summary>
            /// 包含
            /// </summary>
            Contain = 7
        }


        public enum HandleApplyTypeEnum
        {
            /// <summary>
            /// 规则满足时
            /// </summary>
            RuleSatisfaction = 1,
            /// <summary>
            /// 规则不满足时
            /// </summary>
            RuleNotSatisfaction = 2
        }

        public enum HandleTypeEnum
        {
            /// <summary>
            /// 直接自动启动流程
            /// </summary>
            StartFlow = 1,
            /// <summary>
            /// 执行自定义扩展类
            /// </summary>
            ExecuteClass = 2,
            /// <summary>
            /// 给出提示信息，不允许用户提交或回退流程
            /// </summary>
            Alert = 3,
            /// <summary>
            /// 询问用户是否提交或回退流程
            /// </summary>
            Confirm = 4,
            /// <summary>
            /// 询问用户是否启动某个流程
            /// </summary>
            AskStartFlow = 5
        }

        public enum InitFormMethodTypeEnum
        {
            /// <summary>
            /// 通过SQL语句初始化
            /// </summary>
            BySql = 1,
            /// <summary>
            /// 通过扩展类初始化
            /// </summary>
            ByClass = 2
        }
    }
}
