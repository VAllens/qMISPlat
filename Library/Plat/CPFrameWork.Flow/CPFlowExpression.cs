using CPFrameWork.Flow.Domain;
using CPFrameWork.Global;
using NVelocity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPFrameWork.Flow
{
    


    [CPName("流程平台", 4)]
    public class CPFlowExpression
    {
        public const string InsKey = "68DBC0A8-80AC-4739-8B6E-D58FAB7E7B58";
        public const string TaskKey = "3940087B-09D4-411F-A821-D924E3B387A3";
        private VelocityContext _vltContext;
        public CPFlowExpression(VelocityContext context)
        {
            this._vltContext = context;
        }
        [CPName("获取流程实例ID")]
        public string InsId()
        { 
            CPFlowInstance ins = this._vltContext.Get(InsKey) as CPFlowInstance;
            if(ins !=null)
            {
                return ins.InsId.ToString();
            }
            else
            {
                return "未获取到流程实例ID";
            } 
        }
        [CPName("获取流程实例对应的流程ID")]
        public string FlowId()
        {
            CPFlowInstance ins = this._vltContext.Get(InsKey) as CPFlowInstance;
            if (ins != null)
            {
                return ins.FlowId.ToString();
            }
            else
            {
                return "未获取流程实例对应的流程ID";
            }
        }
        [CPName("获取流程实例对应的流程版本ID")]
        public string FlowVerId()
        {
            CPFlowInstance ins = this._vltContext.Get(InsKey) as CPFlowInstance;
            if (ins != null)
            {
                return ins.FlowVerId.ToString();
            }
            else
            {
                return "未获取流程实例对应的流程版本ID";
            }
        }
        [CPName("获取流程实例标题")]
        public string InsTitle()
        {
            CPFlowInstance ins = this._vltContext.Get(InsKey) as CPFlowInstance;
            if (ins != null)
            {
                return ins.InsTitle.ToString();
            }
            else
            {
                return "未获取到流程实例标题";
            }
        }
        [CPName("获取流程实例发起人用户ID")]
        public string CreateUserId()
        {
            CPFlowInstance ins = this._vltContext.Get(InsKey) as CPFlowInstance;
            if (ins != null)
            {
                return ins.CreateUserId.ToString();
            }
            else
            {
                return "未获取流程实例发起人用户ID";
            }
        }
        [CPName("获取流程实例发起人用户姓名")]
        public string CreateUserName()
        {
            CPFlowInstance ins = this._vltContext.Get(InsKey) as CPFlowInstance;
            if (ins != null)
            {
                return ins.CreateUserName.ToString();
            }
            else
            {
                return "未获取流程实例发起人用户姓名";
            }
        }
        [CPName("获取流程实例对应的主表单主键值")]
        public string MainFormPk()
        {
            CPFlowInstance ins = this._vltContext.Get(InsKey) as CPFlowInstance;
            if (ins != null)
            {
                return ins.MainFormPk.ToString();
            }
            else
            {
                return "未获取流程实例对应的主表单主键值";
            }
        }
        [CPName("获取当前待办任务ID")]
        public string TaskId()
        {
            CPFlowInstanceTask task = this._vltContext.Get(TaskKey) as CPFlowInstanceTask;
            if (task != null)
            {
                return task.TaskId.ToString();
            }
            else
            {
                return "未获取当前待办任务ID";
            }
        }
        [CPName("获取当前待办任务提交阶段名称")]
        public string TaskSubmitPhaseNames()
        {
            CPFlowInstanceTask task = this._vltContext.Get(TaskKey) as CPFlowInstanceTask;
            if (task != null)
            {
                return task.SubmitPhaseNames.ToString();
            }
            else
            {
                return "未获取当前待办任务提交阶段名称";
            }
        }
        [CPName("获取当前待办任务提交人姓名")]
        public string TaskSubmitUserNames()
        {
            CPFlowInstanceTask task = this._vltContext.Get(TaskKey) as CPFlowInstanceTask;
            if (task != null)
            {
                return task.SubmitUserNames.ToString();
            }
            else
            {
                return "未获取当前待办任务提交人姓名";
            }
        }
        [CPName("获取当前待办任务接收时间")]
        public string TaskRevTime()
        {
            CPFlowInstanceTask task = this._vltContext.Get(TaskKey) as CPFlowInstanceTask;
            if (task != null)
            {
                return task.RevTime.ToString();
            }
            else
            {
                return "未获取当前待办任务接收时间";
            }
        }
    }
}
