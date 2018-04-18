using CPFrameWork.Flow.Domain;
using CPFrameWork.Flow.Infrastructure;
using CPFrameWork.Flow.Repository;
using CPFrameWork.Global;
using CPFrameWork.Global.Msg;
using CPFrameWork.Organ.Application;
using CPFrameWork.Organ.Domain;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPFrameWork.Flow
{
   public  class CPFlowEngine
    {
        #region 获取配置时相关 
        /// <summary>
        /// 获取节点对象服务类
        /// </summary>
        /// <returns></returns>
        public static void StartupInit(IServiceCollection services, IConfigurationRoot Configuration)
        {
            // Add framework services.
            services.AddDbContext<CPFlowInsDbContext>(options =>//手工高亮
                options.UseSqlServer(Configuration.GetConnectionString("CPFlowIns")));

            services.TryAddTransient<ICPFlowInsDbContext, CPFlowInsDbContext>();
            services.TryAddTransient<BaseCPFlowInstanceRep, CPFlowInstanceRep>();
            services.TryAddTransient<BaseRepository<CPFlowInstanceTask>, CPFlowInstanceTaskRep>();
            services.TryAddTransient<BaseRepository<CPFlowInstanceLog>, CPFlowInstanceLogRep>();
            services.TryAddTransient<BaseRepository<CPFlowInstanceLogUnique>, CPFlowInstanceLogUniqueRep>();
            services.TryAddTransient<BaseRepository<CPFlowInstanceForm>, CPFlowInstanceFormRep>();
            services.TryAddTransient<CPFlowEngine, CPFlowEngine>();
        }
        public static CPFlowEngine Instance(int curUserId)
        {
            CPFlowEngine iObj = CPAppContext.GetService<CPFlowEngine>();
            iObj.CurUserId = curUserId;
            return iObj;
        }
        #endregion


        private BaseCPFlowInstanceRep _CPFlowInstanceRep;
        private BaseRepository<CPFlowInstanceTask> _CPFlowInstanceTaskRep;
        private BaseRepository<CPFlowInstanceLog> _CPFlowInstanceLogRep;
        private BaseRepository<CPFlowInstanceLogUnique> _CPFlowInstanceLogUniqueRep;
        private BaseRepository<CPFlowInstanceForm> _CPFlowInstanceFormRep;
        public CPFlowEngine(
                 BaseCPFlowInstanceRep CPFlowInstanceRep,
         BaseRepository<CPFlowInstanceTask> CPFlowInstanceTaskRep,
         BaseRepository<CPFlowInstanceLog> CPFlowInstanceLogRep,
         BaseRepository<CPFlowInstanceLogUnique> CPFlowInstanceLogUniqueRep,
         BaseRepository<CPFlowInstanceForm> CPFlowInstanceFormRep
            )
        {
            this._CPFlowInstanceRep = CPFlowInstanceRep;
            this._CPFlowInstanceTaskRep = CPFlowInstanceTaskRep;
            this._CPFlowInstanceLogRep = CPFlowInstanceLogRep;
            this._CPFlowInstanceLogUniqueRep = CPFlowInstanceLogUniqueRep;
            this._CPFlowInstanceFormRep = CPFlowInstanceFormRep;
        }
        /// <summary>
        /// 当前用户ID
        /// </summary>
        public int CurUserId { get; set; }

        #region 启动流程
        /// <summary>
        /// 启动流程,并自动给首节点创建任务实例
        /// </summary>
        /// <param name="flowVerId">流程版本Id</param>
        /// <param name="insTitle">流程标题</param>
        /// <param name="mainFormPK">主表单主键</param>
        /// <param name="newInsId">返回新的流程实例ID</param>
        /// <param name="newTaskId">返回新的流程任务ID</param>
        /// <param name="errorMsg">如出错时，返回出错信息</param>
        /// <returns></returns>
        public bool StartFlow(int flowVerId,string insTitle,int mainFormPK,
            out int newInsId,
            out int newTaskId,
            out string errorMsg)
        {
            bool b = true;
            newInsId = 0;
            newTaskId = 0;
            errorMsg = "";
            CPFlowTemplate template = CPFlowTemplate.Instance();
            CPFlow flow = template.GetFlowByFlowVerId(flowVerId,true, true);
            if(flow == null)
            {
                b = false;
                errorMsg = "根据流程版本ID【" + flowVerId + "】未获取到流程信息！";
                return b;
            }
            COUser curUser = COOrgans.Instance().GetUserById(this.CurUserId, false, false);
            if(curUser == null)
            {
                b = false;
                errorMsg = "根据用户ID【" + this.CurUserId + "】未获取到用户信息！";
                return b;
            }
            CPFlowPhase firstPhase = template.GetFirstPhase(flow.PhaseCol, flow.PhaseLinkColSubmit);
            if (firstPhase == null)
            {
                b = false;
                errorMsg = "流程启动失败，未获取到流程首节点！";
                return b;
            }
            #region 创建新的流程实例 
            CPFlowInstance instance = new CPFlowInstance();
            instance.FlowVerId = flow.FlowVerId;
            instance.FlowId = flow.FlowId;
            instance.FlowName = flow.FlowName;
            instance.InsTitle = insTitle;
            instance.CreateUserId = curUser.Id;
            instance.CreateUserName = curUser.UserName;
            instance.CreateTime = DateTime.Now;
            instance.InsState = CPFlowEnum.InsStateEnum.Start;
            instance.InsStateTitle = CPFlowEnum.GetInsStateString(instance.InsState);
            instance.MainFormPk = mainFormPK;
            int n = this._CPFlowInstanceRep.Add(instance);
            if (n > 0)
            {
                b = true;
                newInsId = instance.InsId;
            }
            else
                b = false;
            if (b == false)
            {
                errorMsg = "将流程实例保存到数据库中时出错！";
                return b;
            }
            #endregion

            #region 创建首节点任务
            CPFlowInstanceTask task = new CPFlowInstanceTask();
            task.InsId = instance.InsId;
            task.FlowVerId = instance.FlowVerId;
            task.FlowId = instance.FlowId;
            task.FlowName = instance.FlowName;
            task.InsTitle = instance.InsTitle;
            task.TaskState = CPFlowEnum.TaskStateEnum.AlreadyActivated;
            task.TaskMakeType = CPFlowEnum.TaskMakeTypeEnum.Submit;
            task.TaskType = CPFlowEnum.TaskTypeEnum.NormalTask;
            task.RevPhaseId = firstPhase.PhaseId;
            task.RevPhaseName = firstPhase.PhaseName;
            task.RevUserId = curUser.Id;
            task.RevUserName = curUser.UserName;
            task.RevSourceUserId = curUser.Id;
            task.RevSourceUserName = curUser.UserName;
            task.RevTime = DateTime.Now;
            task.RevUserIsView = false;
            n = this._CPFlowInstanceTaskRep.Add(task);
            if (n > 0)
            {
                b = true;
                newTaskId = task.TaskId;
                //首节点提交节点和任务弄成本身
                task.SubmitPhaseIds = task.RevPhaseId.ToString();
                task.SubmitPhaseNames = task.RevPhaseName;
                task.SubmitUserIds = task.RevUserId.ToString();
                task.SubmitUserNames = task.RevUserName;
                task.SubmitTaskIds = task.TaskId.ToString();
                this._CPFlowInstanceTaskRep.Update(task);
            }
            else
                b = false;
            if (b == false)
            {
                errorMsg = "将任务实例保存到数据库中时出错！";
                return b;
            }
            #endregion

            #region 创建主表单实例
            CPFlowInstanceForm form = new CPFlowInstanceForm();
            form.InsId = instance.InsId;
            form.FlowVerId = instance.FlowVerId;
            form.FlowId = instance.FlowId;
            form.FormPK = instance.MainFormPk;
            List<CPFlowPhaseForm> formCol = template.GetPhaseForm(firstPhase.PhaseId, false);
            form.FormCode = formCol.Where(t => t.IsMainForm.Value == true).ToList()[0].FormCode;
            n = this._CPFlowInstanceFormRep.Add(form);
            if (n > 0)
            {
                b = true;
              
            }
            else
                b = false;
            if (b == false)
            {
                errorMsg = "将表单实例保存到数据库中时出错！";
                return b;
            }
            #endregion

            #region 修改流程状态
            instance.InsState = CPFlowEnum.InsStateEnum.Normal;
            instance.InsStateTitle = CPFlowEnum.GetInsStateString(instance.InsState);
            this._CPFlowInstanceRep.Update(instance);
            #endregion

            #region 发送消息提醒
            b = this.SendMsg(instance, new List<CPFlowInstanceTask>() { task }, flow, curUser, out errorMsg);
            #endregion
            return b;
        }
        #endregion

        #region 发送消息提醒
        public bool SendMsg(CPFlowInstance curIns, List<CPFlowInstanceTask> curTaskCol, CPFlow curFlow, COUser curUser, out string errorMsg)
        {
            errorMsg = "";
            if (curTaskCol.Count <= 0)
                return true;
            bool b = true;
            List<CPMsgEntity> eCol = new List<CPMsgEntity>();
            curTaskCol.ForEach(t =>
            {
                if (t.TaskState == CPFlowEnum.TaskStateEnum.AlreadyActivated)
                {                  
                    string msgTitle = curFlow.MsgTitleTemplate;
                    string msgContext = curFlow.MsgContentTemplate;
                    string msgUrl = curFlow.MsgTaskManaTemplate;
                    CPExpressionHelper.Instance.Add(CPFlowExpression.InsKey, curIns);
                    CPExpressionHelper.Instance.Add(CPFlowExpression.TaskKey, t);
                    msgTitle = CPExpressionHelper.Instance.RunCompile(msgTitle);
                    msgContext = CPExpressionHelper.Instance.RunCompile(msgContext);
                    msgUrl = CPExpressionHelper.Instance.RunCompile(msgUrl);
                    CPExpressionHelper.Instance.Remove(CPFlowExpression.InsKey);
                    CPExpressionHelper.Instance.Remove(CPFlowExpression.TaskKey);
                    CPMsgEntity item = new CPMsgEntity();
                    item.MsgSourceModule = "WF";
                    item.MsgSourceModulePK = t.TaskId.ToString();
                    item.MsgType = CPEnum.MsgTypeEnum.Task;
                    item.MsgTitle = msgTitle;
                    item.MsgContenxt = msgContext;
                    item.MsgSendUserId = curUser.Id;
                    item.MsgSendUserName = curUser.UserName;
                    item.ReciveUserId = t.RevUserId.Value;
                    item.ReciveUserName = t.RevUserName;
                    item.ReciveTime = DateTime.Now;
                    item.ManaPageUrl = msgUrl;
                    item.IsRead = false;
                    eCol.Add(item);
                }
            });
            b = CPMsgs.Instance().SendMsg(eCol, out errorMsg);
            if(b)
            {
                eCol.ForEach(t => {
                    List<CPFlowInstanceTask> tmpT = curTaskCol.Where(c => c.TaskId.Equals(int.Parse(t.MsgSourceModulePK))).ToList();
                    if(tmpT.Count >0)
                    {
                        if (string.IsNullOrEmpty(tmpT[0].MsgIds))
                            tmpT[0].MsgIds = t.Id.ToString();
                        else
                            tmpT[0].MsgIds += "," +  t.Id.ToString();
                    }
                });
                this._CPFlowInstanceTaskRep.Update(curTaskCol);
            }
            return b;
        }
        #endregion

        #region 提交流程到下一个阶段
        /// <summary>
        /// 将流程提交到下一个节点
        /// </summary>
        /// <param name="insId">流程实例ID</param>
        /// <param name="taskId">任务ID</param>
        /// <param name="inputPhaseCol">待提交到的节点集合，如果传入null或集合为空，则默认根据流程配置获取</param>
        /// <param name="errorMsg">出错信息</param>
        /// <returns>是否提交成功</returns>
        public bool SubmitTask(int insId,int taskId,List<CPFlowPhaseClient> inputPhaseCol,out List<CPFlowInstanceTask> newTaskCol, out string errorMsg)
        {
            bool b = true;
            errorMsg = "";
            newTaskCol = new List<CPFlowInstanceTask>();
            
            #region 前期参数准备
            CPFlowInstance curIns = this.GetInstance(insId);
            if(curIns == null)
            {
                errorMsg = "未找到流程ID为【" + insId + "】的流程实例！";
                b = false; return b;
            }
            CPFlowInstanceTask curTask = this.GetInstanceTask(taskId);
            if (curTask == null)
            {
                errorMsg = "未找到任务ID为【" + taskId + "】的任务实例！";
                b = false; return b;
            }
            if(curTask.TaskState == CPFlowEnum.TaskStateEnum.NotActive)
            {
                errorMsg = "当前任务由于他人回退任务导致任务暂停或未激活，请稍候再办理！";
                b = false; return b;
            }
            if (curTask.TaskState == CPFlowEnum.TaskStateEnum.Paused)
            {
                errorMsg = "当前任务处于暂停状态，请先恢复办理状态！";
                b = false; return b;
            }
            CPFlowTemplate template = CPFlowTemplate.Instance();
            CPFlow curFlow = template.GetFlowByFlowVerId(curTask.FlowVerId, true, true);
            List<CPFlowPhase> tmpPhaseCol = curFlow.PhaseCol.Where(c => c.PhaseId.Equals(curTask.RevPhaseId)).ToList();
            if(tmpPhaseCol.Count <=0)
            {
                errorMsg = "根据当前任务实例未找到ID为【" + curTask.RevPhaseId + "】的流程阶段对象";
                b = false;return b;
            }           
            CPFlowPhase curPhase = tmpPhaseCol[0];
            if (curPhase.PhaseType == CPFlowEnum.PhaseTypeEnum.Meet)
            {
                #region 如果当前节点是会签节点，则看下会签节点中有没有其它人是回退的，如果有，当前是会签最后一个人的，则也是回退
                bool isReback = false;
                isReback = this.CheckMeetNodeIsNeedFallback( curTask);
                #endregion
            }
            COUser curUser = COOrgans.Instance().GetUserById(this.CurUserId, false, false);
            if (curUser == null)
            {
                b = false;
                errorMsg = "根据用户ID【" + this.CurUserId + "】未获取到用户信息！";
                return b;
            }
            //判断当前流程任务对应的节点是不是最后一个节点
            bool curTaskPhaseIsLast = false;
            List<CPFlowPhase> nextPhaseCol = null;
            if (inputPhaseCol == null || inputPhaseCol.Count <= 0)
            {
                b = this.GetNextPhaseByTask(curTask, curFlow, out nextPhaseCol, out errorMsg);
                if(b==false)
                {
                    return false;
                }
                #region 判断当前流程任务对应的节点是不是最后一个节点
                if(nextPhaseCol.Count <=0)
                {
                    curTaskPhaseIsLast = true;
                }
                else
                {
                    //看下后一个阶段是不是类型为结束的阶段，如果是，也是表示当前阶段是最后一个阶段了
                    if(nextPhaseCol.Count ==1 && nextPhaseCol[0].PhaseType == CPFlowEnum.PhaseTypeEnum.End)
                    {
                        curTaskPhaseIsLast = true;
                    }
                }
                #endregion
            }
            else
            {
                nextPhaseCol = new List<CPFlowPhase>();
                inputPhaseCol.ForEach(t => {
                    List<CPFlowPhase> tmpCol = curFlow.PhaseCol.Where(c => c.PhaseId.Equals(t.PhaseId)).ToList();
                    if(tmpCol.Count >0)
                    {
                        tmpCol[0].TaskRevUser = t.TaskRevUser;
                        nextPhaseCol.Add(tmpCol[0]);
                    }
                });
            }
            #endregion
            //临时变量，记录要不要创建新的流程任务
            bool isNeedCreateNewTask = true;
            #region 先结束当前任务及关联任务
            if(curPhase.PhaseType == CPFlowEnum.PhaseTypeEnum.Start
                || curPhase.PhaseType == CPFlowEnum.PhaseTypeEnum.Normal)
            {
                #region 普通节点，看看有没有提交给多个人，如果有，则自动将其他人任务给删除了
                CPFlowInstanceLog log = CPFlowInstanceLog.InitFromTask(curTask);
                b = this.AddInstanceLog(log,curIns);
                if(!b)
                {
                    errorMsg = "普通节点或首节点，将当前任务添加到日志数据表中时出错！";
                    return b;
                }
                //删除这个节点上的所有任务
                b = this.DeleteInstanceTask(curTask.InsId, curTask.RevPhaseId.Value);
                if(!b)
                {
                    errorMsg = "普通节点或首节点，删除流程任务时出错，流程ID为【" + curTask.InsId + "】，阶段ID为【" + curTask.RevPhaseId.Value + "】！";
                    return b;
                }
                #endregion
            }
            else if(curPhase.PhaseType == CPFlowEnum.PhaseTypeEnum.Meet)
            {
                #region 会签节点
                CPFlowInstanceLog log = CPFlowInstanceLog.InitFromTask(curTask);
                b = this.AddInstanceLog(log, curIns);
                if (!b)
                {
                    errorMsg = "会签节点，将当前任务添加到日志数据表中时出错！";
                    return b;
                }
                //删除任务
                b = this.DeleteInstanceTask(curTask);
                if (!b)
                {
                    errorMsg = "会签节点，删除流程任务时出错，任务ID为【" + curTask.TaskId + "】！";
                    return b;
                }
                //看看本会签节点有没有其他任务，如果有，则不需要 创建新的任务
                List<CPFlowInstanceTask> meetTaskCol = this.GetInstanceTask(curTask.InsId, curTask.RevPhaseId.Value);
                if(meetTaskCol.Count >0)
                {
                    isNeedCreateNewTask = false;
                }
                #endregion
            }
            else if (curPhase.PhaseType == CPFlowEnum.PhaseTypeEnum.Circulation)
            {
                #region 传阅节点
                CPFlowInstanceLog log = CPFlowInstanceLog.InitFromTask(curTask);
                b = this.AddInstanceLog(log, curIns);
                if (!b)
                {
                    errorMsg = "传阅节点，将当前任务添加到日志数据表中时出错！";
                    return b;
                }
                //删除这个节点上的所有任务
                b = this.DeleteInstanceTask(curTask);
                if (!b)
                {
                    errorMsg = "传阅节点，删除流程任务时出错，任务ID为【" + curTask.TaskId + "】！";
                    return b;
                }
                isNeedCreateNewTask = false;
                #endregion
            }
            #endregion

            #region 判断是不是需要创建新的任务
            if(isNeedCreateNewTask)
            {
                if (curTaskPhaseIsLast)
                    isNeedCreateNewTask = false;
            }
            #endregion

            #region 产生新的任务实例
            if(isNeedCreateNewTask)
            {
                List<CPFlowInstanceTask> addedTaskCol = new List<CPFlowInstanceTask>();
                nextPhaseCol.ForEach(nextPhase => {
                    nextPhase.InitTaskDefaultRevUser(curIns,CurUserId);
                    nextPhase.TaskRevUser.ForEach(revUser => {
                        //看看这个用户在这个节点上是否有任务了
                        CPFlowInstanceTask revUserTaskTmp = this.GetInstanceTask(
                            curIns.InsId,
                            nextPhase.PhaseId,
                            revUser.RevUserId
                            );
                        if(revUserTaskTmp == null)
                        {
                            revUserTaskTmp = CPFlowInstanceTask.InitFromInstanceAndPhase(
                                curIns,
                                curTask,
                                nextPhase,
                                revUser,
                                curFlow.PhaseLinkColSubmit
                                );
                            addedTaskCol.Add(revUserTaskTmp);
                        }
                    });
                    //要看看新产生的任务能不能激活
                });
                b = this.AddInstanceTask(addedTaskCol);
                if(!b)
                {
                    errorMsg = "写入新任务到数据库中时出错！" ;
                    b = false; return b;
                }
                #region 发送消息提醒
                b = this.SendMsg(curIns, addedTaskCol, curFlow, curUser, out errorMsg);
                if (!b)
                {
                    return b;
                }
                #endregion

                newTaskCol = addedTaskCol;
                if(curTask.TaskMakeType == CPFlowEnum.TaskMakeTypeEnum.Reback)
                {
                    #region 如果当前任务是回退产生的，则把当前临时改成未激活的任务改成正常的状态
                    List<CPFlowInstanceTask> taskCol= this.GetInstanceTaskByInsId(curIns.InsId);
                    List<CPFlowInstanceTask> editedCol = new List<CPFlowInstanceTask>();
                    taskCol.ForEach(t => {
                        if(t.TaskState == CPFlowEnum.TaskStateEnum.NotActive
                         && t.RebackPreTaskState.HasValue)
                        {
                            t.TaskState = t.RebackPreTaskState.Value;
                            t.RebackPreTaskState = null;
                            editedCol.Add(t);
                        }
                    });
                    if(editedCol.Count >0)
                    {
                        this._CPFlowInstanceTaskRep.Update(editedCol);
                    }
                    #endregion
                }
                //批量设置流程状态为激活或不激活
                this.UpdateTaskToActiveStatePrivate(curIns, curFlow);
            }
            #endregion

            #region 修改主流程状态
            if(curTaskPhaseIsLast)
            {
                //最后一个节点，并且没有新的流程任务了
                if(this.GetInstanceTaskCount(curIns.InsId)<=0)
                {
                    b = this.EndInstance(curIns, curUser,out errorMsg);
                    if(!b)
                    {
                        errorMsg = "结束流程实例时出错！详细信息如下：" + errorMsg;
                        b = false; return b;
                    }
                }
            }
            #endregion
            return b;
        }


        #endregion

        #region 回退流程
        public bool CheckMeetNodeIsNeedFallback(CPFlowInstanceTask curTask)
        {
            #region 如果当前节点是会签节点，则看下会签节点中有没有其它人是回退的，如果有，当前是会签最后一个人的，则也是回退
            int nTaskCount = this.GetInstanceTaskCount(curTask.InsId, curTask.RevPhaseId.Value);
            if (nTaskCount <= 1)
            {
                bool isReback = false;
                List<CPFlowInstanceLog> logCol = this.GetInstanceLog(curTask.InsId, curTask.RevPhaseId.Value);
                logCol.ForEach(t => {
                    if (t.SubmitTaskIds.Equals(curTask.SubmitTaskIds))
                    {
                        if (t.TaskManaType == CPFlowEnum.TaskMakeTypeEnum.Reback)
                        {
                            isReback = true;
                            return;
                        }
                    }
                });
                if (isReback)
                {
                    return true;
                }
            }
            return false;
            #endregion
        }
        /// <summary>
        /// 检测会签节点是否需要 回退
        /// </summary>
        /// <param name="insId"></param>
        /// <param name="taskId"></param>
        /// <returns>true :需要 </returns>
        public bool CheckMeetNodeIsNeedFallback(int taskId)
        {
            CPFlowInstanceTask curTask = this.GetInstanceTask(taskId);
            return this.CheckMeetNodeIsNeedFallback( curTask);
        }
        /// <summary>
        /// 回退流程任务
        /// </summary>
        /// <param name="insId">流程ID</param>
        /// <param name="taskId">任务ID</param>
        /// <param name="newTaskCol">返回新的任务实例</param>
        /// <param name="errorMsg">返回错误信息</param>
        /// <returns>是否操作成功</returns>
        private bool FallbackTask(CPFlowInstance curIns, CPFlowInstanceTask curTask, List<CPFlowPhase> fallbackPhaseCol, CPFlow curFlow,CPFlowPhase curPhase, out List<CPFlowInstanceTask> newTaskCol, out string errorMsg)
        {
            bool b = true;
            errorMsg = "";
            newTaskCol = new List<CPFlowInstanceTask>(); 
          

            //临时变量，记录要不要创建新的流程任务
            bool isNeedCreateNewTask = true;
            #region 先结束当前任务及关联任务
            if (curPhase.PhaseType == CPFlowEnum.PhaseTypeEnum.Normal)
            {
                #region 普通节点，看看有没有提交给多个人，如果有，则自动将其他人任务给删除了
                CPFlowInstanceLog log = CPFlowInstanceLog.InitFromTask(curTask);
                log.TaskManaType = CPFlowEnum.TaskMakeTypeEnum.Reback;
                b = this.AddInstanceLog(log, curIns);
                if (!b)
                {
                    errorMsg = "普通节点或首节点，将当前任务添加到日志数据表中时出错！";
                    return b;
                }
                //删除这个节点上的所有任务
                b = this.DeleteInstanceTask(curTask.InsId, curTask.RevPhaseId.Value);
                if (!b)
                {
                    errorMsg = "普通节点或首节点，删除流程任务时出错，流程ID为【" + curTask.InsId + "】，阶段ID为【" + curTask.RevPhaseId.Value + "】！";
                    return b;
                }
                #endregion
            }
            else if (curPhase.PhaseType == CPFlowEnum.PhaseTypeEnum.Meet)
            {
                #region 会签节点
                CPFlowInstanceLog log = CPFlowInstanceLog.InitFromTask(curTask);
                log.TaskManaType = CPFlowEnum.TaskMakeTypeEnum.Reback;
                b = this.AddInstanceLog(log, curIns);
                if (!b)
                {
                    errorMsg = "会签节点，将当前任务添加到日志数据表中时出错！";
                    return b;
                }
                //删除任务
                b = this.DeleteInstanceTask(curTask);
                if (!b)
                {
                    errorMsg = "会签节点，删除流程任务时出错，任务ID为【" + curTask.TaskId + "】！";
                    return b;
                }
                //看看本会签节点有没有其他任务，如果有，则不需要 创建新的任务
                List<CPFlowInstanceTask> meetTaskCol = this.GetInstanceTask(curTask.InsId, curTask.RevPhaseId.Value);
                if (meetTaskCol.Count > 0)
                {
                    isNeedCreateNewTask = false;
                }
                #endregion
            }
            #endregion



            #region 产生新的任务实例
            if (isNeedCreateNewTask)
            {
                List<CPFlowInstanceTask> addedTaskCol = new List<CPFlowInstanceTask>();
                fallbackPhaseCol.ForEach(nextPhase => {
                    nextPhase.InitTaskDefaultRevUser(curIns, CurUserId);
                    nextPhase.TaskRevUser.ForEach(revUser => {
                        //看看这个用户在这个节点上是否有任务了
                        CPFlowInstanceTask revUserTaskTmp = this.GetInstanceTask(
                            curIns.InsId,
                            nextPhase.PhaseId,
                            revUser.RevUserId
                            );
                        if (revUserTaskTmp == null)
                        {
                            revUserTaskTmp = CPFlowInstanceTask.InitFromInstanceAndPhase(
                                curIns,
                                curTask,
                                nextPhase,
                                revUser,
                                curFlow.PhaseLinkColFallback
                                );
                            revUserTaskTmp.TaskState = CPFlowEnum.TaskStateEnum.AlreadyActivated;
                            revUserTaskTmp.TaskMakeType = CPFlowEnum.TaskMakeTypeEnum.Reback;
                            addedTaskCol.Add(revUserTaskTmp);
                        }
                    });
                    //要看看新产生的任务能不能激活
                });
                b = this.AddInstanceTask(addedTaskCol);
                if (!b)
                {
                    errorMsg = "写入新任务到数据库中时出错！";
                    b = false; return b;
                }
                newTaskCol = addedTaskCol;
                #region 发送消息提醒
                COUser curUser = COOrgans.Instance().GetUserById(this.CurUserId, false, false);
                 b = this.SendMsg(curIns, addedTaskCol, curFlow, curUser, out errorMsg);
                if (!b)
                {
                    return b;
                }
                #endregion
                #region 自动把其它任务改成未激活，这样回退的任务再次提交时，直接激活这些任务
                List<CPFlowInstanceTask> taskCol = this.GetInstanceTaskByInsId(curTask.InsId);
                taskCol.ForEach(t => {
                    if (addedTaskCol.Where(c => c.TaskId.Equals(t.TaskId)).ToList().Count <= 0)
                    {
                        t.RebackPreTaskState = t.TaskState;
                        t.TaskState = CPFlowEnum.TaskStateEnum.NotActive;
                    }
                });
                if (taskCol.Count > 0)
                {
                    this._CPFlowInstanceTaskRep.Update(taskCol);
                }
                #endregion
            }
            #endregion



            return b;
        }
        /// <summary>
        /// 回退流程任务
        /// </summary>
        /// <param name="insId">流程ID</param>
        /// <param name="taskId">任务ID</param>
        /// <param name="newTaskCol">返回新的任务实例</param>
        /// <param name="errorMsg">返回错误信息</param>
        /// <returns>是否操作成功</returns>
        public bool FallbackTask(int insId, int taskId, int fallbackPhaseId, out List<CPFlowInstanceTask> newTaskCol, out string errorMsg)
        {
            bool b = true;
            errorMsg = "";
            newTaskCol = new List<CPFlowInstanceTask>();

            #region 前期参数准备
            CPFlowInstance curIns = this.GetInstance(insId);
            if (curIns == null)
            {
                errorMsg = "未找到流程ID为【" + insId + "】的流程实例！";
                b = false; return b;
            }
            CPFlowInstanceTask curTask = this.GetInstanceTask(taskId);
            if (curTask == null)
            {
                errorMsg = "未找到任务ID为【" + taskId + "】的任务实例！";
                b = false; return b;
            }
            if (curTask.TaskState == CPFlowEnum.TaskStateEnum.NotActive)
            {
                errorMsg = "当前任务由于他人回退任务导致任务暂停或未激活，请稍候再办理！";
                b = false; return b;
            }
            if (curTask.TaskState == CPFlowEnum.TaskStateEnum.Paused)
            {
                errorMsg = "当前任务处于暂停状态，请先恢复办理状态！";
                b = false; return b;
            }
            CPFlowTemplate template = CPFlowTemplate.Instance();
            CPFlow curFlow = template.GetFlowByFlowVerId(curTask.FlowVerId, true, true);
            List<CPFlowPhase> tmpPhaseCol = curFlow.PhaseCol.Where(c => c.PhaseId.Equals(curTask.RevPhaseId)).ToList();
            if (tmpPhaseCol.Count <= 0)
            {
                errorMsg = "根据当前任务实例未找到ID为【" + curTask.RevPhaseId + "】的流程阶段对象";
                b = false; return b;
            }
            CPFlowPhase curPhase = tmpPhaseCol[0];
            COUser curUser = COOrgans.Instance().GetUserById(this.CurUserId, false, false);
            if (curUser == null)
            {
                b = false;
                errorMsg = "根据用户ID【" + this.CurUserId + "】未获取到用户信息！";
                return b;
            }

            #endregion

            List<CPFlowPhase> fallbackPhaseCol =  curFlow.PhaseCol.Where(c => c.PhaseId.Equals(fallbackPhaseId)).ToList();
            if (fallbackPhaseCol.Count <= 0)
            {
                errorMsg = "根据传入的驳回目标阶段ID【" + fallbackPhaseId + "】未找到对应的流程阶段对象";
                b = false; return b;
            }
            b = this.FallbackTask(curIns, curTask, fallbackPhaseCol, curFlow, curPhase, out newTaskCol, out errorMsg);

             

            return b;
        }
        /// <summary>
        /// 回退流程任务
        /// </summary>
        /// <param name="insId">流程ID</param>
        /// <param name="taskId">任务ID</param>
        /// <param name="newTaskCol">返回新的任务实例</param>
        /// <param name="errorMsg">返回错误信息</param>
        /// <returns>是否操作成功</returns>
        public bool FallbackTask(int insId, int taskId,   out List<CPFlowInstanceTask> newTaskCol, out string errorMsg)
        {
            bool b = true;
            errorMsg = "";
            newTaskCol = new List<CPFlowInstanceTask>();

            #region 前期参数准备
            CPFlowInstance curIns = this.GetInstance(insId);
            if (curIns == null)
            {
                errorMsg = "未找到流程ID为【" + insId + "】的流程实例！";
                b = false; return b;
            }
            CPFlowInstanceTask curTask = this.GetInstanceTask(taskId);
            if (curTask == null)
            {
                errorMsg = "未找到任务ID为【" + taskId + "】的任务实例！";
                b = false; return b;
            }
            if (curTask.TaskState == CPFlowEnum.TaskStateEnum.NotActive)
            {
                errorMsg = "当前任务由于他人回退任务导致任务暂停或未激活，请稍候再办理！";
                b = false; return b;
            }
            if (curTask.TaskState == CPFlowEnum.TaskStateEnum.Paused)
            {
                errorMsg = "当前任务处于暂停状态，请先恢复办理状态！";
                b = false; return b;
            }
            CPFlowTemplate template = CPFlowTemplate.Instance();
            CPFlow curFlow = template.GetFlowByFlowVerId(curTask.FlowVerId, true, true);
            List<CPFlowPhase> tmpPhaseCol = curFlow.PhaseCol.Where(c => c.PhaseId.Equals(curTask.RevPhaseId)).ToList();
            if (tmpPhaseCol.Count <= 0)
            {
                errorMsg = "根据当前任务实例未找到ID为【" + curTask.RevPhaseId + "】的流程阶段对象";
                b = false; return b;
            }
            CPFlowPhase curPhase = tmpPhaseCol[0];
            COUser curUser = COOrgans.Instance().GetUserById(this.CurUserId, false, false);
            if (curUser == null)
            {
                b = false;
                errorMsg = "根据用户ID【" + this.CurUserId + "】未获取到用户信息！";
                return b;
            }

            #endregion

            List<CPFlowPhase> fallbackPhaseCol = null;
            string tmpError = "";
            b = this.GetCanFallbackPhase(curIns, curTask, curFlow, out fallbackPhaseCol, out tmpError);
            if (!b)
            {
                errorMsg = tmpError;
                return b;
            }

            b = this.FallbackTask(curIns, curTask, fallbackPhaseCol, curFlow, curPhase, out newTaskCol, out errorMsg);
            return b;
        }
        /// <summary>
        /// 获取可以回退的节点集合
        /// </summary>
        /// <param name="curTask"></param>
        /// <param name="flow"></param>
        /// <param name="nextPhaseCol"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool GetCanFallbackPhase(CPFlowInstance curIns, CPFlowInstanceTask curTask,
            CPFlow curFlow,
            out List<CPFlowPhase> fallbackPhaseCol,
          out string errorMsg)
        {
            errorMsg = "";
            fallbackPhaseCol = new List<CPFlowPhase>();
            List<CPFlowPhase> outputPhaseCol = new List<CPFlowPhase>();
            bool b = true;
            #region 参数检测
            if (curFlow.PhaseCol == null || curFlow.PhaseCol.Count <= 0)
            {
                errorMsg = "传入的参数flow对象，属性PhaseCol未包含任务流程阶段相关信息";
                b = false;
                return b;
            }
            if (curFlow.PhaseLinkCol == null || curFlow.PhaseLinkCol.Count <= 0)
            {
                errorMsg = "传入的参数flow对象，属性PhaseLinkCol未包含任务流程阶段路由相关信息";
                b = false;
                return b;
            }
            #endregion

            if(curFlow.FlowRebackModel == CPFlowEnum.FlowRebackModelEnum.ToFirst)
            {
                #region 回退到首节点
                CPFlowPhase firstPhase = CPFlowTemplate.Instance().GetFirstPhase(curFlow.PhaseCol,
                    curFlow.PhaseLinkColSubmit);
                firstPhase.InitTaskDefaultRevUser(curIns, this.CurUserId);
                outputPhaseCol.Add(firstPhase);
                #endregion
            }
            else if(curFlow.FlowRebackModel == CPFlowEnum.FlowRebackModelEnum.ToPre)
            {
                #region 回退到上一个节点,如果是分支汇聚节点，则回退到所有进入此任务的分支节点
                curFlow.PhaseLinkColFallback.ForEach(t => {
                    if(t.StartPhaseId.Equals(curTask.RevPhaseId))
                    {
                        outputPhaseCol.AddRange(curFlow.PhaseCol.Where(c=>c.PhaseId.Equals(t.EndPhaseId)).ToList());
                    }
                });
                #endregion
            }
            fallbackPhaseCol = outputPhaseCol;
            return b;
        }
        #endregion

        #region 取回流程
        /// <summary>
        /// 取回流程
        /// </summary>
        /// <param name="insId"></param>
        /// <param name="newTaskCol"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool TakeBackFlow(int insId,out List<CPFlowInstanceTask> newTaskCol,out string errorMsg)
        {
            bool b = true;
            errorMsg = "";
            newTaskCol = new List<CPFlowInstanceTask>();
            CPFlowInstance curIns = this.GetInstance(insId); 
            List<CPFlowInstanceTask> curTaskCol = this.GetInstanceTaskByInsId(insId);
            b =this.CheckUserCanTakeBackFlow(insId, curIns, curTaskCol,  out errorMsg);
            if (!b)
                return b;
            List<CPFlowInstanceLog> logCol =  this.GetInstanceLogBySubmitTaskId(insId, int.Parse(curTaskCol[0].SubmitTaskIds));
            if(logCol.Count <=0)
            {
                errorMsg = "根据提交任务ID【" + curTaskCol[0].SubmitTaskIds+ "】，未获取到办理日志信息";
                return false;
            }
            //先删除当前所有的待办任务信息
            b = this.DeleteInstanceTask(insId);
            //给当前用户产生一条新的信息
            COUser curUser = COOrgans.Instance().GetUserById(this.CurUserId, false, false);
            CPFlowInstanceTask taskNew = logCol[0].ReCreateTask();
            taskNew.TaskState = CPFlowEnum.TaskStateEnum.AlreadyActivated;
            taskNew.RebackPreTaskState = null;
            taskNew.TaskMakeType = CPFlowEnum.TaskMakeTypeEnum.Retrieve;
            taskNew.RevUserId = this.CurUserId;
            taskNew.RevUserName = curUser.UserName;
            taskNew.RevSourceUserId = this.CurUserId;
            taskNew.RevSourceUserName = curUser.UserName;
            taskNew.RevTime = DateTime.Now;
            taskNew.PauseRemark = "";
            taskNew.PauseUserId = null;
            taskNew.PauseUserName = null;
            taskNew.PauseTime = null;
            taskNew.MsgIds = "";
            newTaskCol.Add(taskNew);
            b  =this.AddInstanceTask(newTaskCol);
            return b;
        }
        /// <summary>
        /// 检测当前用户是否可以取回任务
        /// </summary>
        /// <param name="insId">流程实例ID</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool CheckUserCanTakeBackFlow(int insId,out string errorMsg)
        { 
            errorMsg = "";
            CPFlowInstance curIns = this.GetInstance(insId);
            List<CPFlowInstanceTask> curTaskCol = this.GetInstanceTaskByInsId(insId);
            return this.CheckUserCanTakeBackFlow(insId,curIns, curTaskCol, out errorMsg);
        }
        private bool CheckUserCanTakeBackFlow(int insId,CPFlowInstance curIns, List<CPFlowInstanceTask> curTaskCol, out string errorMsg)
        {
            bool b = true;
            errorMsg = ""; 
            if (curIns == null)
            {
                errorMsg = "未找到流程ID为【" + insId + "】的流程实例！";
                b = false; return b;
            }
            if (curIns.InsState == CPFlowEnum.InsStateEnum.End || curIns.InsState == CPFlowEnum.InsStateEnum.Pause
                || curIns.InsState == CPFlowEnum.InsStateEnum.Start
                || curIns.InsState == CPFlowEnum.InsStateEnum.Stop
                )
            {
                return false;
            }
            //获取这个流程实例下的任务
            if (curTaskCol.Count <= 0)
                return false;
            //如果提交的taskId不是同一个，也不允许取回
            List<string> allSubmitTaskCol = new List<string>();
            curTaskCol.ForEach(t => {
                if (allSubmitTaskCol.Contains(t.SubmitTaskIds) == false)
                    allSubmitTaskCol.Add(t.SubmitTaskIds);
            });
            if (allSubmitTaskCol.Count > 1)
                return false;
            List<CPFlowInstanceTask> canTakebackTask = new List<CPFlowInstanceTask>();
            curTaskCol.ForEach(t => {
                if (t.TaskState == CPFlowEnum.TaskStateEnum.AlreadyActivated
                && (t.RevUserIsView.HasValue == false || (t.RevUserIsView.HasValue == true && t.RevUserIsView.Value == false))
                    && t.TaskMakeType == CPFlowEnum.TaskMakeTypeEnum.Submit
                    && t.SubmitTaskIds.IndexOf(",") == -1
                    && t.SubmitPhaseIds.IndexOf(",") == -1
                    && t.RevUserId.Equals(this.CurUserId) == false
                    )
                {
                    canTakebackTask.Add(t);
                }
            });
            if (canTakebackTask.Count <= 0)
                return false;
            //根据这个产生这个任务的taskId，看下是不是当前用户提交的，如果是，则可以取回，如果不是，则不能取回。
            //并且所有的的任务必须都是同一个节点产生的
            List<int> submitUserId = new List<int>();
            canTakebackTask.ForEach(t => {
                List<CPFlowInstanceLog> logCol = this.GetInstanceLogBySubmitTaskId(t.InsId, int.Parse(t.SubmitTaskIds));
                logCol.ForEach(log => {
                    if (submitUserId.Contains(log.RevUserId.Value) == false)
                    {
                        submitUserId.Add(log.RevUserId.Value);
                    }
                });

            });
            if (submitUserId.Count > 1)
                return false;
            else
            {
                if (submitUserId.Contains(this.CurUserId))
                    return true;
                else
                    return false;
            }
        }
        #endregion

        #region 设置某条任务为已阅状态
        public bool UpdateTaskToReadState(CPFlowInstanceTask curTask)
        {
            curTask.RevUserIsView = true;
            this._CPFlowInstanceTaskRep.Update(curTask);
            //更改状态为已读状态
            string[] sArray = curTask.MsgIds.Split(',');
            List<int> idCol = new List<int>();
            for (int i = 0; i < sArray.Length; i++)
            {
                if (string.IsNullOrEmpty(sArray[i]) == false)
                {
                    idCol.Add(int.Parse(sArray[i]));
                }
            }
            string errorMsg = "";
            CPMsgs.Instance().UpdateMsgReadState(idCol, out errorMsg);
            return true;
        }
        #endregion

        #region 设置流程待办任务为激活或不激活状态
        private bool UpdateTaskToActiveStatePrivate(CPFlowInstance curIns ,CPFlow curFlow)
        {
            List<int> updateedTaskIdCol    = new List<int>();
            string errorMsg = "";
            //为了解决分去流程，有时候算待办任务不对的问题，在这里再算一次，其思路就是看当前待办任务,除状态为-1的节点待办外，还有没有其它的，如果有，则不管，如果没有，则将0改成1           
            ISpecification<CPFlowInstanceTask> specificationNotActive;
            specificationNotActive = new ExpressionSpecification<CPFlowInstanceTask>(t => t.TaskState == CPFlowEnum.TaskStateEnum.NotActive
            && t.InsId.Equals(curIns.InsId)
            );
            List<CPFlowInstanceTask> taskNotActive = this._CPFlowInstanceTaskRep.GetByCondition(specificationNotActive).ToList();
            ISpecification<CPFlowInstanceTask> specificationActived;
            specificationActived = new ExpressionSpecification<CPFlowInstanceTask>(t => t.TaskState != CPFlowEnum.TaskStateEnum.NotActive
            && t.InsId.Equals(curIns.InsId)
            
            );
            List<CPFlowInstanceTask> taskActived = this._CPFlowInstanceTaskRep.GetByCondition(specificationActived).ToList();
            if (taskNotActive.Count() <= 0)
                return true;
            else
            {
                #region 如果出现未激活的待办任务有多个流程节点的情况，则要看下，有没有节点是某个节点的后续节点，如果是，则此节点暂时不应该激活。

                //string notUpdateNodeIds = "";
                List<int> notUpdateNodeIds = new List<int>();
                if (taskNotActive.Count() > 1)
                {
                    //存储所有的节点ID和及后续节点
                    Dictionary<int, List<int>> nodeIdAndAllNextNodeIds = new Dictionary<int, List<int>>();
                    taskNotActive.ForEach(notActiveTask => {
                        #region 获取当前节点的后续节点
                        CPFlowPhase curNode = curFlow.PhaseCol.Where(c => c.PhaseId.Equals(notActiveTask.RevPhaseId)).ToList()[0];
                        List<CPFlowPhase> nextNodeCol = new List<CPFlowPhase>();
                        nextNodeCol.Add(curNode);
                        List<int> idCol = new List<int>();
                        while (nextNodeCol.Count > 0)
                        {
                            List<CPFlowPhase> tempNexNodeCol = new List<CPFlowPhase>();
                            nextNodeCol.ForEach(node => {
                                List<CPFlowPhaseLink> nextlinkCol = curFlow.PhaseLinkColSubmit.Where(c => c.StartPhaseId.Equals(node.PhaseId)).ToList();//
                                foreach (CPFlowPhaseLink link in nextlinkCol)
                                {

                                    if (link.EndPhaseId.Equals(curNode.PhaseId) == false && idCol.Contains(link.EndPhaseId) == false)
                                    {
                                        idCol.Add(link.EndPhaseId);
                                    }
                                    if (link.StartPhaseId.Equals(curNode.PhaseId) == false && idCol.Contains(link.StartPhaseId) == false)
                                    {
                                        idCol.Add(link.StartPhaseId);
                                    }
                                    tempNexNodeCol.Add(curFlow.PhaseCol.Where(c=>c.PhaseId.Equals(link.EndPhaseId)).ToList()[0]);
                                }
                            });
                            
                            nextNodeCol = tempNexNodeCol;
                        }
                        if (nodeIdAndAllNextNodeIds.Keys.Contains(curNode.PhaseId) == false)
                        {
                            nodeIdAndAllNextNodeIds.Add(curNode.PhaseId, idCol);
                        }
                        #endregion
                    });
                    

                    foreach (int key in nodeIdAndAllNextNodeIds.Keys)
                    {
                        foreach (int keySmall in nodeIdAndAllNextNodeIds.Keys)
                        {
                            if (key.Equals(keySmall))
                                continue;
                            if (nodeIdAndAllNextNodeIds[keySmall].Contains(key))
                            {
                                //if (string.IsNullOrEmpty(notUpdateNodeIds))
                                //    notUpdateNodeIds = key.ToString();
                                //else
                                //    notUpdateNodeIds += "," + key.ToString();
                                notUpdateNodeIds.Add(key);
                            }
                        }
                    }
                }
                #endregion
                int nodeId = taskNotActive[0].RevPhaseId.Value;
                if (taskActived.Count() <= 0)
                {
                    ISpecification<CPFlowInstanceTask> specificationTmp;
                    specificationTmp = new ExpressionSpecification<CPFlowInstanceTask>(t => t.TaskState == CPFlowEnum.TaskStateEnum.NotActive
                    && t.InsId.Equals(curIns.InsId)
                    && notUpdateNodeIds.Contains(t.RevPhaseId.Value)==false
                    );
                    List<CPFlowInstanceTask> taskTmp = this._CPFlowInstanceTaskRep.GetByCondition(specificationTmp).ToList();
                    //修改状态
                    taskTmp.ForEach(t => { t.TaskState = CPFlowEnum.TaskStateEnum.AlreadyActivated; });
                    if (taskTmp.Count > 0)
                    {
                        this._CPFlowInstanceTaskRep.Update(taskTmp);
                        #region 发送消息提醒
                        COUser curUser = COOrgans.Instance().GetUserById(this.CurUserId, false, false);
                        bool b = this.SendMsg(curIns, taskTmp, curFlow, curUser, out errorMsg);
                        if (!b)
                        {
                            return b;
                        }
                        #endregion
                    }
                }
                else
                {
                    bool canUpdate = true;
                    taskActived.ForEach(t => {
                        if (t.RevPhaseId.Equals(nodeId) == false)
                        {
                            //传阅节点不算在里面
                            List<CPFlowPhase> pCol = curFlow.PhaseCol.Where(c => c.PhaseId.Equals(t.RevPhaseId.Value)).ToList();
                            if (pCol.Count() > 0 && pCol[0].PhaseType != CPFlowEnum.PhaseTypeEnum.Circulation)
                            {
                                canUpdate = false;
                                return;
                            }
                        }
                    });
                    if (canUpdate)
                    { 
                        ISpecification<CPFlowInstanceTask> specificationTmp;
                        specificationTmp = new ExpressionSpecification<CPFlowInstanceTask>(t => t.TaskState == CPFlowEnum.TaskStateEnum.NotActive
                        && t.InsId.Equals(curIns.InsId)
                        && notUpdateNodeIds.Contains(t.RevPhaseId.Value) == false
                        );
                        List<CPFlowInstanceTask> taskTmp = this._CPFlowInstanceTaskRep.GetByCondition(specificationTmp).ToList();
                        //修改状态
                        taskTmp.ForEach(t => { t.TaskState = CPFlowEnum.TaskStateEnum.AlreadyActivated; });
                        if (taskTmp.Count > 0)
                        {
                            this._CPFlowInstanceTaskRep.Update(taskTmp);
                            #region 发送消息提醒
                            COUser curUser = COOrgans.Instance().GetUserById(this.CurUserId, false, false);
                            bool b = this.SendMsg(curIns, taskTmp, curFlow, curUser, out errorMsg);
                            if (!b)
                            {
                                return b;
                            }
                            #endregion
                        }
                    }
                }
                return true;
            }
        }
        #endregion

        #region 根据流程配置和任务实例，获取当前任务实例对应流程节点的所有后续节点，如果当前任务时回退来的，则直接提交到回退的那个节点和对应的用户
        public bool GetNextPhaseByTask(CPFlowInstanceTask curTask,
            CPFlow flow,
            out List<CPFlowPhase> nextPhaseCol,
          out string errorMsg)
        {
            errorMsg = "";
            nextPhaseCol = new List<CPFlowPhase>();
            List<CPFlowPhase> outputPhaseCol = new List<CPFlowPhase>();
            bool b = true;
            #region 参数检测
            if (flow.PhaseCol == null || flow.PhaseCol.Count <= 0)
            {
                errorMsg = "传入的参数flow对象，属性PhaseCol未包含任务流程阶段相关信息";
                b = false;
                return b;
            }
            if (flow.PhaseLinkCol == null || flow.PhaseLinkCol.Count <= 0)
            {
                errorMsg = "传入的参数flow对象，属性PhaseLinkCol未包含任务流程阶段路由相关信息";
                b = false;
                return b;
            }
            #endregion

            if (
                (curTask.TaskMakeType == CPFlowEnum.TaskMakeTypeEnum.Reback
                && flow.RebackTaskSubmitType == CPFlowEnum.RebackTaskSubmitTypeEnum.ToRebackPhase
                )
               
                )
            {
                #region 任务是回退的，并且是直接提交给回退节点 
                List<string> tPhaseIdCol = curTask.SubmitPhaseIds.Split(',').ToList();
                if (tPhaseIdCol.Count <= 0)
                {
                    errorMsg = "当前任务是回退产生的，但任务里未存储回退节点ID";
                    b = false;
                    return b;
                }
                tPhaseIdCol.ForEach(t =>
                {
                    List<CPFlowPhase> pColTmp = flow.PhaseCol.Where(c => c.PhaseId.Equals(int.Parse(t))).ToList();
                    if (pColTmp.Count > 0)
                    {
                        if (outputPhaseCol.Contains(pColTmp[0]) == false)
                        {
                            outputPhaseCol.Add(pColTmp[0]);
                        }
                    }
                });
                #endregion
            }
            else if(
                (curTask.TaskMakeType == CPFlowEnum.TaskMakeTypeEnum.Submit
                 || curTask.TaskMakeType == CPFlowEnum.TaskMakeTypeEnum.Retrieve
                 )
                ||
                (curTask.TaskMakeType == CPFlowEnum.TaskMakeTypeEnum.Reback
                && flow.RebackTaskSubmitType == CPFlowEnum.RebackTaskSubmitTypeEnum.ToNextPhase)
                )
            {
                #region 任务是提交的或取回的或回退的，但回退任务再次提交时，提交到后续节点
                flow.PhaseLinkColSubmit.ForEach(link => {
                    if(link.StartPhaseId.Equals(curTask.RevPhaseId))
                    {
                        if(this.CheckLinkCanPass(link,flow))
                        {
                            List<CPFlowPhase> pColTmp = flow.PhaseCol.Where(c => c.PhaseId.Equals(link.EndPhaseId)).ToList();
                            if (pColTmp.Count > 0 
                            && pColTmp[0].PhaseType != CPFlowEnum.PhaseTypeEnum.End
                            )
                            {
                                if (outputPhaseCol.Contains(pColTmp[0]) == false)
                                {
                                    outputPhaseCol.Add(pColTmp[0]);
                                }
                            }
                        }
                    }
                });
                #endregion
            }
            nextPhaseCol = outputPhaseCol;
            return b;
        }
        #endregion

        #region 检测某条路由是否可以进入
        /// <summary>
        /// 检测某条路由是否可以进入
        /// </summary>
        /// <param name="link"></param>
        /// <param name="flow"></param>
        /// <returns>true:可以进入 false:不可以进入</returns>
        public bool CheckLinkCanPass(CPFlowPhaseLink link,CPFlow flow)
        {
            return true;
        }
        #endregion

        #region 获取流程实例 
        private bool EndInstance(CPFlowInstance curIns,COUser curUser,out string  errorMsg)
        {
            errorMsg = "";
            curIns.InsState =  CPFlowEnum.InsStateEnum.End;
            curIns.InsStateTitle = CPFlowEnum.GetInsStateString(curIns.InsState);
            curIns.InsEndTime = DateTime.Now;
            curIns.InsEndUserId = curUser.Id;
            curIns.InsEndUserName = curUser.UserName;
            this._CPFlowInstanceRep.Update(curIns);
            //获取所有单条日志并修改状态
            List<CPFlowInstanceLogUnique> logCol =  this.GetInstanceLogUnique(curIns.InsId);
            logCol.ForEach(t => {
                t.InsState = curIns.InsState;
                t.InsStateTitle = curIns.InsStateTitle;
                t.InsEndTime = curIns.InsEndTime;

            });
            bool b = true;
            this._CPFlowInstanceLogUniqueRep.Update(logCol);
            //将所有的传阅节点任务改成日志表里。
            string tmpError = "";
            List<CPFlowInstanceTask> taskCol =  this.GetInstanceTaskByInsId(curIns.InsId);
            taskCol.ForEach(curTask => {
                CPFlowInstanceLog log = CPFlowInstanceLog.InitFromTask(curTask);
                b = this.AddInstanceLog(log, curIns);
                if (!b)
                {
                    tmpError = "普通节点或首节点，将当前任务添加到日志数据表中时出错！";
                  //  return b;
                }
                //删除这个节点上的所有任务
                b = this.DeleteInstanceTask(curTask.InsId, curTask.RevPhaseId.Value);
                if (!b)
                {
                    tmpError = "普通节点或首节点，删除流程任务时出错，流程ID为【" + curTask.InsId + "】，阶段ID为【" + curTask.RevPhaseId.Value + "】！";
                  
                }
            });
           if(!b)
            {
                errorMsg = tmpError;
            }
            return b;
        }
        public CPFlowInstance GetInstance(int insId)
        {
            return this._CPFlowInstanceRep.Get(insId);
        }
        #endregion

        #region 流程任务相关
        public bool AddInstanceTask(List<CPFlowInstanceTask> taskCol)
        {
            if (taskCol.Count <= 0)
                return true;
            int n = this._CPFlowInstanceTaskRep.Add(taskCol);
            if (n > 0)
                return true;
            else
                return false;
        }
        public CPFlowInstanceTask GetInstanceTask(int insId,int phaseId,int userId)
        {
            ISpecification<CPFlowInstanceTask> specification;
            specification = new ExpressionSpecification<CPFlowInstanceTask>(t => t.RevPhaseId.Equals(phaseId)
            && t.InsId.Equals(insId)
            && t.RevUserId.Equals(userId)
            );
            List < CPFlowInstanceTask > col = this._CPFlowInstanceTaskRep.GetByCondition(specification).ToList();
            if (col.Count <= 0)
                return null;
            else
                return col[0];
        }
        public List<CPFlowInstanceTask> GetInstanceTaskByInsId(int insId)
        {
            ISpecification<CPFlowInstanceTask> specification;
            specification = new ExpressionSpecification<CPFlowInstanceTask>(t =>  
            t.InsId.Equals(insId) 
            );
            List<CPFlowInstanceTask> col = this._CPFlowInstanceTaskRep.GetByCondition(specification).ToList();
            return col;
        }
        public int GetInstanceTaskCount(int insId)
        {
            ISpecification<CPFlowInstanceTask> specification;
            specification = new ExpressionSpecification<CPFlowInstanceTask>(t =>
            t.InsId.Equals(insId)
            );
            return this._CPFlowInstanceTaskRep.Count(specification);
        }
        public int GetInstanceTaskCount(int insId,int phaseId)
        {
            ISpecification<CPFlowInstanceTask> specification;
            specification = new ExpressionSpecification<CPFlowInstanceTask>(t =>
            t.InsId.Equals(insId)
            && t.RevPhaseId.Value.Equals(phaseId)
            );
            return this._CPFlowInstanceTaskRep.Count(specification);
        }
        public List<CPFlowInstanceTask> GetInstanceTask(int insId,int phaseId)
        {
            ISpecification<CPFlowInstanceTask> specification;
            specification = new ExpressionSpecification<CPFlowInstanceTask>(t => t.RevPhaseId.Equals(phaseId) && t.InsId.Equals(insId));
            return this._CPFlowInstanceTaskRep.GetByCondition(specification).ToList();
        }
        public bool DeleteInstanceTask(int insId,int phaseId)
        {
            List<CPFlowInstanceTask> taskCol = this.GetInstanceTask(insId, phaseId);
            List<int> idCol = new List<int>();
            taskCol.ForEach(t => {
                string[] sArray = t.MsgIds.Split(',');
                for (int i = 0; i < sArray.Length; i++)
                {
                    if (string.IsNullOrEmpty(sArray[i]) == false)
                    {
                        idCol.Add(int.Parse(sArray[i]));
                    }
                }
            });
            bool b = this._CPFlowInstanceRep.DeleteInstanceTask(insId, phaseId);
            if (b)
            {
                string errorMsg = "";
                CPMsgs.Instance().DeleteMsg(idCol, out errorMsg);

            }
            return b;
        }
        public bool DeleteInstanceTask(int insId)
        {
            List<CPFlowInstanceTask> taskCol =  this.GetInstanceTaskByInsId(insId);
            List<int> idCol = new List<int>();
            taskCol.ForEach(t =>{
                string[] sArray = t.MsgIds.Split(','); 
                for (int i = 0; i < sArray.Length; i++)
                {
                    if (string.IsNullOrEmpty(sArray[i]) == false)
                    {
                        idCol.Add(int.Parse(sArray[i]));
                    }
                }
            });
            bool b= this._CPFlowInstanceRep.DeleteInstanceTask(insId);
            if(b)
            {
                string errorMsg = "";
                CPMsgs.Instance().DeleteMsg(idCol, out errorMsg);

            }
            return b;
        }
        public bool DeleteInstanceTask(CPFlowInstanceTask task)
        {
             this._CPFlowInstanceTaskRep.Delete(task);
            string[] sArray = task.MsgIds.Split(',');
            List<int> idCol = new List<int>();
            for(int i =0;i<sArray.Length;i++)
            {
                if(string.IsNullOrEmpty(sArray[i])==false)
                {
                    idCol.Add(int.Parse(sArray[i]));
                }
            }
            string errorMsg = "";
            CPMsgs.Instance().DeleteMsg(idCol, out errorMsg);
            return true;
        }
        public CPFlowInstanceTask GetInstanceTask(int taskId)
        {
            return this._CPFlowInstanceTaskRep.Get(taskId);
        }
        #endregion

        #region 流程任务日志
        /// <summary>
        /// 根据流程ID和用户ID，获取用户唯一的日志记录，没有则返回null
        /// </summary>
        /// <param name="insId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public CPFlowInstanceLogUnique GetInstanceLogUnique(int insId,int userId)
        {
            ISpecification<CPFlowInstanceLogUnique> specification;
            specification = new ExpressionSpecification<CPFlowInstanceLogUnique>(t => t.ManaUserId.Equals(userId) && t.InsId.Equals(insId));
            List< CPFlowInstanceLogUnique> col =  this._CPFlowInstanceLogUniqueRep.GetByCondition(specification).ToList();
            if (col.Count <= 0)
                return null;
            else
                return col[0];
        }
        public List<CPFlowInstanceLogUnique> GetInstanceLogUnique(int insId)
        {
            ISpecification<CPFlowInstanceLogUnique> specification;
            specification = new ExpressionSpecification<CPFlowInstanceLogUnique>(t =>  t.InsId.Equals(insId));
            List<CPFlowInstanceLogUnique> col = this._CPFlowInstanceLogUniqueRep.GetByCondition(specification).ToList();
            return col;
        }
        public List<CPFlowInstanceLog> GetInstanceLog(int insId,int phaseId)
        {
            ISpecification<CPFlowInstanceLog> specification;
            specification = new ExpressionSpecification<CPFlowInstanceLog>(t => t.InsId.Equals(insId) && t.RevPhaseId.Equals(phaseId));
            List<CPFlowInstanceLog> col = this._CPFlowInstanceLogRep.GetByCondition(specification).ToList();
            return col;
        }
        public List<CPFlowInstanceLog> GetInstanceLog(int insId)
        {
            ISpecification<CPFlowInstanceLog> specification;
            specification = new ExpressionSpecification<CPFlowInstanceLog>(t => t.InsId.Equals(insId) );
            List<CPFlowInstanceLog> col = this._CPFlowInstanceLogRep.GetByCondition(specification).ToList();
            return col;
        }
        public List<CPFlowInstanceLog> GetInstanceLogBySubmitTaskId(int insId, int SubmitTaskId)
        {
            ISpecification<CPFlowInstanceLog> specification;
            specification = new ExpressionSpecification<CPFlowInstanceLog>(t => t.InsId.Equals(insId) );
            List<CPFlowInstanceLog> col = this._CPFlowInstanceLogRep.GetByCondition(specification).ToList();
            List<CPFlowInstanceLog> colNew = new List<CPFlowInstanceLog>();
            col.ForEach(t => {
                if(string.IsNullOrEmpty(t.SubmitTaskIds)==false && t.SubmitTaskIds.Split(',').Contains(SubmitTaskId.ToString()))
                {
                    colNew.Add(t);
                }
            });
            return colNew;
        }
        public List<CPFlowInstanceLog> GetInstanceLogByUser(int insId)
        {
            ISpecification<CPFlowInstanceLog> specification;
            specification = new ExpressionSpecification<CPFlowInstanceLog>(t => t.InsId.Equals(insId) && t.RevUserId.Value.Equals(this.CurUserId));
            List<CPFlowInstanceLog> col = this._CPFlowInstanceLogRep.GetByCondition(specification).ToList();
            return col;
        }
        /// <summary>
        /// 添加日志到数据库
        /// </summary>
        /// <param name="log"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool AddInstanceLog(CPFlowInstanceLog log, CPFlowInstance instance)
        {
            int n = this._CPFlowInstanceLogRep.Add(log);
            if (n <= 0)
                return false;
            else
            {
                //再添加到单一日志表里
                CPFlowInstanceLogUnique logUnique = this.GetInstanceLogUnique(log.InsId, log.RevUserId.Value);
                if (logUnique == null)
                {
                    logUnique = CPFlowInstanceLogUnique.InitFromLogAndInstance(log, instance);
                    n = this._CPFlowInstanceLogUniqueRep.Add(logUnique);
                    if (n <= 0)
                        return false;
                }
                else
                {
                    List<string> idCol = logUnique.ManaPhaseIds.Split(',').ToList();
                    if(idCol.Contains(log.RevPhaseId.Value.ToString())==false)
                    {
                        if(string.IsNullOrEmpty(logUnique.ManaPhaseIds))
                        {
                            logUnique.ManaPhaseIds = log.RevPhaseId.Value.ToString();
                            logUnique.ManaPhaseNames = log.RevPhaseName;
                        }
                        else
                        {
                            logUnique.ManaPhaseIds += "," +  log.RevPhaseId.Value.ToString();
                            logUnique.ManaPhaseNames += "," + log.RevPhaseName;
                        }
                        this._CPFlowInstanceLogUniqueRep.Update(logUnique);
                    }
                }
                
                return true;
            }
        }
        #endregion

        #region 流程表单相关
        public bool AddInstanceForm(CPFlowInstanceForm form)
        {
            this._CPFlowInstanceFormRep.Add(form);
            return true;
        }
        public List<CPFlowInstanceForm> GetInstanceForm(int insId)
        {
            ISpecification<CPFlowInstanceForm> specification;
            specification = new ExpressionSpecification<CPFlowInstanceForm>(t => t.InsId.Equals(insId)
            );
            List<CPFlowInstanceForm> col = this._CPFlowInstanceFormRep.GetByCondition(specification).ToList();
            return col;
        }
        public CPFlowPhase GetCurPhaseAndFormByTask(CPFlowInstanceTask curTask)
        {
            CPFlowPhase phase = CPFlowTemplate.Instance() .GetFlowPhaseByPhaseId(curTask.RevPhaseId.Value, true, false);
            List<CPFlowInstanceForm> formCol = this.GetInstanceForm(curTask.InsId);
            phase.FormCol.ForEach(t => {
                List<CPFlowInstanceForm> fTmp = formCol.Where(c => c.FormCode.Equals(t.FormCode)).ToList();
                if(fTmp.Count >0)
                {
                    t.FormPageUrl += "&PKValues=" + fTmp[0].FormPK;
                }
            });
            return phase;
        }
        #endregion
    }
}
