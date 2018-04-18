using CPFrameWork.Flow.Domain;
using CPFrameWork.Global;
using CPFrameWork.Organ.Application;
using CPFrameWork.Organ.Domain;
using CPFrameWork.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CPFrameWork.Flow
{
    public class CPFlowEngineController : CPWebApiBaseController
    {

        #region 保存流程定制
        public class SaveFlowDesignInput
        {
            public string CurUserIden { get; set; }
            public int CurUserId { get; set; }
            public int FlowVerId { get; set; }
            public string FlowName { get; set; }
            public string FlowJSON { get; set; }
        }
        [HttpPost]
        public CPWebApiBaseReturnEntity SaveFlowDesign([FromBody] SaveFlowDesignInput input)
        {
            base.SetHeader();
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(input.CurUserId, input.CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            re.Result = true;
            try
            {
                CPFlowDesigner flowDesign = Newtonsoft.Json.JsonConvert.DeserializeObject<CPFlowDesigner>(input.FlowJSON);
                CPFlowTemplate template = CPFlowTemplate.Instance();
                string errorMsg = "";
                re.Result = template.UpdateFlowByDesigner(flowDesign, input.FlowVerId, input.FlowName, ref errorMsg);
                if (re.Result == false)
                    re.ErrorMsg = errorMsg;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 获取流程配置信息，为定制器用
        public class GetFlowInfoForDesignerReturn : CPWebApiBaseReturnEntity
        {
            public CPFlowDesigner FlowDesigner { get; set; }
            public string FlowName { get; set; }
            public CPFlowEnum.FlowVerStateEnum FlowVerState { get; set; }
        }
        [HttpGet]
        public GetFlowInfoForDesignerReturn GetFlowInfoForDesigner(int FlowVerId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            GetFlowInfoForDesignerReturn re = new GetFlowInfoForDesignerReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                CPFlow flow = CPFlowTemplate.Instance().GetFlowByFlowVerId(FlowVerId, true, true);
                CPFlowDesigner designer = new CPFlowDesigner();
                designer.states = new Dictionary<string, CPFlowDesignerPhase>();
                designer.paths = new Dictionary<string, CPFlowDesignerPhaseLink>();
                flow.PhaseCol.ForEach(t =>
                {
                    CPFlowDesignerPhase dPhase = new CPFlowDesignerPhase();
                    dPhase.type = CPFlowDesignerPhase.ConvertToDesignerType(t.PhaseType.Value);
                    dPhase.text = new CPFlowDesignerPhaseText();
                    dPhase.text.text = t.PhaseName;
                    dPhase.attr = new CPFlowDesignerPhaseAttr();
                    dPhase.attr.x = t.PositionX.Value;
                    dPhase.attr.y = t.PositionY.Value;
                    dPhase.attr.width = t.Width.Value;
                    dPhase.attr.height = t.Height.Value;
                    dPhase.props = new CPFlowDesignerPhaseprops();
                    dPhase.props.temp1 = new CPFlowDesignerPhasepropstemp1();
                    dPhase.props.temp1.value = t.PhaseId.ToString();                    
                    designer.states.Add("rect" + t.PhaseId, dPhase);
                });
                int nIndex = 1;
                flow.PhaseLinkCol.ForEach(t =>
                {
                    CPFlowDesignerPhaseLink dLink = new CPFlowDesignerPhaseLink();
                    dLink.from = "rect" + t.StartPhaseId.ToString();
                    dLink.to = "rect" + t.EndPhaseId.ToString();
                    dLink.dots = new List<CPFlowDesignerPhaseLinkDot>();
                    if (string.IsNullOrEmpty(t.LinkDots) == false)
                    {
                        List<string> t1Col = t.LinkDots.Split(';').ToList();
                        t1Col.ForEach(d1 =>
                        {
                            if (string.IsNullOrEmpty(d1))
                                return;
                            string[] sArray = d1.Split(',');
                            CPFlowDesignerPhaseLinkDot dot = new CPFlowDesignerPhaseLinkDot();
                            dot.x = double.Parse(sArray[0]);
                            dot.y = double.Parse(sArray[1]);
                            dLink.dots.Add(dot);
                        });
                    }
                    dLink.text = new CPFlowDesignerPhaseLinkText();
                    dLink.text.text = "";
                    dLink.text.textPos = new CPFlowDesignerPhaseLinkTextPos();
                    dLink.text.textPos.x = t.TipX.Value;
                    dLink.text.textPos.y = t.TipY.Value;
                    dLink.props = new CPFlowDesignerPhaseLinkprops();
                    dLink.props.temp1 = new CPFlowDesignerPhasepropstemp1();
                    dLink.props.temp1.value = t.LinkId.ToString();
                    dLink.props.PathType = new CPFlowDesignerPhasepropstemp1();
                    dLink.props.PathType.value = Convert.ToString((int)t.LinkType);
                    designer.paths.Add("path" + nIndex, dLink);
                    nIndex++;
                });
                re.Result = true;
                re.FlowDesigner = designer;
                re.FlowName = flow.FlowName;
                re.FlowVerState = flow.FlowVerState.Value;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 获取流程配置信息，为浏览监控用
        public class GetFlowInfoForMonitorReturn : GetFlowInfoForDesignerReturn
        {
         
            public List<GetFlowInfoForMonitorReturnLogItem> LogCol { get; set; }
        }
       public class GetFlowInfoForMonitorReturnLogItem
        {
            public string SubmitPhaseNames { get; set; }
            public string RevTime { get; set; }
            public DateTime RevTimeD { get; set; }
            public string SubmitUserNames { get; set; }
            public string RevUserName { get; set; }
            public string TaskState { get; set; }
            public string TaskManaTime { get; set; }
            public int PhaseId { get; set; }
        }
        [HttpGet]
        public GetFlowInfoForMonitorReturn GetFlowInfoForMonitor(string InsId,string FlowId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            GetFlowInfoForMonitorReturn re = new GetFlowInfoForMonitorReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                CPFlowTemplate template =  CPFlowTemplate.Instance();
                CPFlowEngine engine = CPFlowEngine.Instance(CurUserId);
                int flowVerId =-1;
                re.LogCol = new List<GetFlowInfoForMonitorReturnLogItem>();
                if (string.IsNullOrEmpty(InsId))
                {
                    //先检测这个流程有没有发布的版本
                    List<CPFlow> flowCol = template.GetFlowMaxTwoVer(int.Parse(FlowId));
                    if (flowCol.Count == 2)
                    {
                        if (flowCol[0].FlowVerState == CPFlowEnum.FlowVerStateEnum.Editing)
                            flowVerId = flowCol[1].FlowVerId;
                        else
                        {
                            flowVerId = flowCol[0].FlowVerId;
                        }
                    }
                    else if (flowCol.Count == 1)
                    {
                        if (flowCol[0].FlowVerState == CPFlowEnum.FlowVerStateEnum.Editing)
                        {
                            re.Result = false;
                            re.ErrorMsg = "当前流程只有一个版本且未正式发布，不能启动流程！";
                            return re;
                        }
                        else
                            flowVerId = flowCol[0].FlowVerId;
                    }
                }
                else
                {
                    CPFlowInstance ins = engine.GetInstance(int.Parse(InsId));
                    flowVerId = ins.FlowVerId;
                 
                }
                if (flowVerId == -1)
                {
                    re.Result = false;
                    re.ErrorMsg = "根据流程模板ID【" + FlowId + "】未找到流程模板信息！";
                    return re;
                }
                GetFlowInfoForDesignerReturn designer = this.GetFlowInfoForDesigner(flowVerId, CurUserId, CurUserIden);
                re.FlowDesigner = designer.FlowDesigner;
                re.FlowName = designer.FlowName;
                re.FlowVerState = designer.FlowVerState;                
                re.Result = true;
                List<CPFlowInstanceTask> curTaskCol = new List<CPFlowInstanceTask>();
                List<CPFlowInstanceLog> logCol = new List<CPFlowInstanceLog>();
                if(string.IsNullOrEmpty(InsId)==false)
                {
                    curTaskCol = engine.GetInstanceTaskByInsId(int.Parse(InsId));
                    logCol = engine.GetInstanceLog(int.Parse(InsId));
                }
                foreach(string key in re.FlowDesigner.states.Keys)
                {
                    int phaseId = int.Parse(key.Replace("rect",""));
                    if(curTaskCol.Where(c=>c.RevPhaseId.Value.Equals(phaseId)).Count() >0)
                    {
                        re.FlowDesigner.states[key].IsCurActive = true;
                    }                   
                }
                foreach(string key in re.FlowDesigner.paths.Keys)
                {
                    CPFlowDesignerPhaseLink link =  re.FlowDesigner.paths[key];
                    int startPhaseId = int.Parse(link.from.Replace("rect", ""));
                    int endPhaseId = int.Parse(link.to.Replace("rect", ""));
                    if(logCol.Where(c=>c.SubmitPhaseIds.Contains(startPhaseId.ToString())
                        && c.RevPhaseId.Value.Equals(endPhaseId)).Count()>0)
                    {
                        link.IsOverOrActive = true;
                    }
                    else
                    {
                        if (curTaskCol.Where(c => c.SubmitPhaseIds.Contains(startPhaseId.ToString())
                          && c.RevPhaseId.Value.Equals(endPhaseId)).Count() > 0)
                        {
                            link.IsOverOrActive = true;
                        }

                    }
                }
              
                logCol.ForEach(t => {
                    GetFlowInfoForMonitorReturnLogItem item = new GetFlowInfoForMonitorReturnLogItem();
                    item.SubmitPhaseNames = t.SubmitPhaseNames;
                    item.RevTime = t.RevTime.Value.ToString();
                    item.RevTimeD = t.RevTime.Value;
                    item.PhaseId = t.RevPhaseId.Value;
                    item.SubmitUserNames = t.SubmitUserNames;
                    item.RevUserName = t.RevUserName;
                    if (t.TaskManaType == CPFlowEnum.TaskMakeTypeEnum.Reback)
                        item.TaskState = "驳回";
                    else if (t.TaskManaType == CPFlowEnum.TaskMakeTypeEnum.Submit)
                        item.TaskState = "同意";
                    else if (t.TaskManaType == CPFlowEnum.TaskMakeTypeEnum.Retrieve)
                        item.TaskState = "取回";
                    item.TaskManaTime = t.TaskManaTime.Value.ToString() ;
                    re.LogCol.Add(item);
                });
                if (string.IsNullOrEmpty(InsId) == false)
                {
                    List<CPFlowInstanceTask> taskCol = engine.GetInstanceTaskByInsId(int.Parse(InsId));
                    taskCol.ForEach(t => {
                        GetFlowInfoForMonitorReturnLogItem item = new GetFlowInfoForMonitorReturnLogItem();
                        item.SubmitPhaseNames = t.SubmitPhaseNames;
                        item.RevTime = t.RevTime.Value.ToString();
                        item.RevTimeD = t.RevTime.Value;
                        item.PhaseId = t.RevPhaseId.Value;
                        item.SubmitUserNames = t.SubmitUserNames;
                        item.RevUserName = t.RevUserName;
                        item.TaskState = "待处理";
                        item.TaskManaTime = "";
                        re.LogCol.Add(item);
                    });
                }
                re.LogCol = re.LogCol.OrderBy(c => c.RevTimeD).ToList();

                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 发布流程
        [HttpGet]
        public CPWebApiBaseReturnEntity ReleaseFlow(int FlowVerId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                CPFlowTemplate template = CPFlowTemplate.Instance();
                CPFlow flow = template.GetFlowByFlowVerId(FlowVerId, false, false);
                flow.FlowVerState = CPFlowEnum.FlowVerStateEnum.Release;
                template.UpdateFlow(flow);
                re.Result = true;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 生成新版本
        public class CreateFlowNewVerReturn : CPWebApiBaseReturnEntity
        {
            public int NewFlowVerId { get; set; }
        }
        [HttpGet]
        public CreateFlowNewVerReturn CreateFlowNewVer(int FlowVerId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CreateFlowNewVerReturn re = new CreateFlowNewVerReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                CPFlowTemplate template = CPFlowTemplate.Instance();
                CPFlow flow = template.GetFlowByFlowVerId(FlowVerId, false, false);
                re.Result = template.CreateFlowNewVer(FlowVerId);
                if (re.Result)
                {
                    CPFlow flowNew = template.GetFlowMaxVer(flow.FlowId);
                    flowNew.FlowVerState = CPFlowEnum.FlowVerStateEnum.Editing;
                    template.UpdateFlow(flowNew);
                    re.NewFlowVerId = flowNew.FlowVerId;
                }
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 还原流程到某个版本
        [HttpGet]
        public CPWebApiBaseReturnEntity RestoreFlowVer(int RestoreFlowVerId,int FlowId, int CurUserId, string CurUserIden)
        {
            //RestoreFlowVerId 需要还原的流程版本ID
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                CPFlowTemplate template = CPFlowTemplate.Instance();
                //先发布版本
                CPFlow flow = template.GetFlowMaxVer(FlowId);
                if (flow.FlowVerState != CPFlowEnum.FlowVerStateEnum.Release)
                {
                    flow.FlowVerState = CPFlowEnum.FlowVerStateEnum.Release;
                    re.Result = template.UpdateFlow(flow);
                }
                //创建新版本
                if (re.Result)
                {
                    re.Result = template.CreateFlowNewVer(RestoreFlowVerId);
                }
                re.Result = true;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 发起流程
        public class StartFlowInput
        {
            public string CurUserIden { get; set; }
            public int CurUserId { get; set; }
            public int FlowVerId { get; set; }
            public string InsTitle { get; set; }
            public int MainFormPK { get; set; }
        }
        public class StartFlowReturn : CPWebApiBaseReturnEntity
        {
            public int InsId { get; set; }
            public int TaskId { get; set; }
        }
        [HttpPost]
        public StartFlowReturn StartFlow([FromBody] StartFlowInput input)
        {
            base.SetHeader();
            StartFlowReturn re = new StartFlowReturn();
            if (this.CheckUserIden(input.CurUserId, input.CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.Result = true;
                int newInsId;
                int newTaskId;
                string errorMsg;
                re.Result = CPFlowEngine.Instance(input.CurUserId).StartFlow(input.FlowVerId,
                    input.InsTitle,
                    input.MainFormPK,
                    out newInsId,
                    out newTaskId,
                    out errorMsg);
                if (re.Result == false)
                {
                    re.ErrorMsg = errorMsg;
                }
                else
                {
                    re.InsId = newInsId;
                    re.TaskId = newTaskId;
                }
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 提交流程
        public class SubmitFlowInput
        {
            public string CurUserIden { get; set; }
            public int CurUserId { get; set; }
            public int InsId { get; set; }
            public int TaskId { get; set; }
            public List<CPFlowPhaseClient> PhaseCol { get; set; }
        }
        public class SubmitFlowReturn : CPWebApiBaseReturnEntity
        {
            public string NewTaskUserNames { get; set; }
        }
        [HttpPost]
        public SubmitFlowReturn SubmitFlow([FromBody] SubmitFlowInput input)
        {
            base.SetHeader();
            SubmitFlowReturn re = new SubmitFlowReturn();
            if (this.CheckUserIden(input.CurUserId, input.CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.Result = true;
                string errorMsg;
                List<CPFlowInstanceTask> newTaskCol;
                re.Result = CPFlowEngine.Instance(input.CurUserId).SubmitTask(input.InsId,
                    input.TaskId,
                    input.PhaseCol,
                    out newTaskCol,
                    out errorMsg);
                re.NewTaskUserNames = "";
                if (re.Result == false)
                {
                    re.ErrorMsg = errorMsg;
                }
                else
                {
                    if (newTaskCol != null && newTaskCol.Count > 0)
                    {
                        List<int> addedId = new List<int>();
                        newTaskCol.ForEach(t =>
                        {
                            if (addedId.Contains(t.RevUserId.Value) == false)
                            {
                                addedId.Add(t.RevUserId.Value);
                                if (string.IsNullOrEmpty(re.NewTaskUserNames))
                                {
                                    re.NewTaskUserNames = t.RevUserName;
                                }
                                else
                                {
                                    re.NewTaskUserNames += "," + t.RevUserName;
                                }
                            }
                        });
                    }
                }
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 检测流程提交时，是否需要弹出提交选人或选择阶段界面
        public class CheckTaskIsCanDirectSubmitReturn : CPWebApiBaseReturnEntity
        {
            /// <summary>
            /// 是否可以直接提交，true:是
            /// </summary>
            public bool IsCanDirectSubmit { get; set; }
            /// <summary>
            /// 是否需要回退，主要是针对会签节点，如果有一个人回退了，则最后一个节点的人也需要 回退
            /// </summary>
            public bool IsNeedFallback { get; set; }
        }
        [HttpGet]
        public CheckTaskIsCanDirectSubmitReturn CheckTaskIsCanDirectSubmit(int TaskId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CheckTaskIsCanDirectSubmitReturn re = new CheckTaskIsCanDirectSubmitReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            re.Result = true;
            try
            {
                CPFlowTemplate template = CPFlowTemplate.Instance();
                CPFlowEngine engine = CPFlowEngine.Instance(CurUserId);
                CPFlowInstanceTask curTask = engine.GetInstanceTask(TaskId);
                CPFlow curFlow = template.GetFlowByFlowVerId(curTask.FlowVerId, true, true);
                CPFlowPhase curPhase = curFlow.PhaseCol.Where(c => c.PhaseId.Equals(curTask.RevPhaseId.Value)).ToList()[0];
                re.IsNeedFallback = false;
                if(curPhase.PhaseType == CPFlowEnum.PhaseTypeEnum.Meet)
                {
                    bool bIsNeed = engine.CheckMeetNodeIsNeedFallback(curTask);
                    if(bIsNeed)
                    {
                        re.Result = true;
                        re.IsNeedFallback = true;
                        return re;
                    }
                }
                List<CPFlowPhase> nextPhaseCol;
                string errorMsg;
                re.Result = engine.GetNextPhaseByTask(curTask, curFlow, out nextPhaseCol, out errorMsg);
                if (re.Result == false)
                {
                    re.ErrorMsg = errorMsg;
                    return re;
                }
                else
                {
                    if (nextPhaseCol.Count <= 0)
                    {
                        re.IsCanDirectSubmit = true;
                    }
                    else if (nextPhaseCol.Count == 1)
                    {
                        if (nextPhaseCol[0].IsCanSelectUser == false)
                            re.IsCanDirectSubmit = true;
                        else
                            re.IsCanDirectSubmit = false;
                    }
                    else
                    {
                        re.IsCanDirectSubmit = false;
                    }
                    #region 如果需要转向提交界面，则看下当前任务是不是会签节点的任务，如果是，则只有最后一个人办理才需要转向
                    if(re.IsCanDirectSubmit==false)
                    {
                        if(curPhase.PhaseType == CPFlowEnum.PhaseTypeEnum.Meet)
                        {
                            if(engine.GetInstanceTaskCount(curTask.InsId,curTask.RevPhaseId.Value)>1)
                            {
                                re.IsCanDirectSubmit = true;
                            }
                        }
                    }
                    #endregion
                    return re;
                }
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 获取下一步可提交的节点和默认办理用户
        public class GetNextPhaseByTaskReturn : CPWebApiBaseReturnEntity
        {
            public List<CPFlowPhase> PhaseCol { get; set; }
        }
        [HttpGet]
        public GetNextPhaseByTaskReturn GetNextPhaseByTask(int TaskId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            GetNextPhaseByTaskReturn re = new GetNextPhaseByTaskReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            re.Result = true;
            try
            {
                CPFlowTemplate template = CPFlowTemplate.Instance();
                CPFlowEngine engine = CPFlowEngine.Instance(CurUserId);
                CPFlowInstanceTask curTask = engine.GetInstanceTask(TaskId);
                CPFlow curFlow = template.GetFlowByFlowVerId(curTask.FlowVerId, true, true);
                List<CPFlowPhase> nextPhaseCol;
                string errorMsg;
                re.Result = engine.GetNextPhaseByTask(curTask, curFlow, out nextPhaseCol, out errorMsg);
                if (re.Result == false)
                {
                    re.ErrorMsg = errorMsg;
                    return re;
                }
                else
                {
                    CPFlowInstance curIns = engine.GetInstance(curTask.InsId);
                    nextPhaseCol.ForEach(t => { t.InitTaskDefaultRevUser(curIns,CurUserId); });
                    re.PhaseCol = nextPhaseCol;
                    return re;
                }
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 流程办理界面用，获取流程定制信息
        public class GetFlowInfoReturn : CPWebApiBaseReturnEntity
        {
            public CPFlow Flow { get; set; }
            public CPFlowPhase CurPhase { get; set; }
            //默认给流程起一个标题
            public string InsTitle { get; set; } 
            /// <summary>
            /// 记录当前流程任务可以回退的节点个数
            /// </summary>
            public int TaskCanFallbackCount { get; set; }

            /// <summary>
            /// 浏览状态下，看下当前用户是否可以取回流程任务
            /// </summary>
            public bool CheckUserCanTakeBackFlow { get; set; }
        }
        [HttpGet]
        public GetFlowInfoReturn GetFlowInfo(string InsId,string TaskId,string FlowId, int CurUserId, string CurUserIden)
        {
            //流程发起时，InsId和TaskId传入空,
            //流程办理时，FlowId传入空,
            //流程查看时，FlowId和TaskId传入空,
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            InsId = CPAppContext.FormatSqlPara(InsId);
            FlowId = CPAppContext.FormatSqlPara(FlowId);
            GetFlowInfoReturn re = new GetFlowInfoReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            re.Result = true;
            try
            {
                CPFlowTemplate template = CPFlowTemplate.Instance();
                if(string.IsNullOrEmpty(InsId) && string.IsNullOrEmpty(TaskId))
                {
                    #region 流程发起时
                    //先检测这个流程有没有发布的版本
                    List<CPFlow> flowCol= template.GetFlowMaxTwoVer(int.Parse(FlowId));
                    if(flowCol.Count  ==2)
                    {
                        if (flowCol[0].FlowVerState == CPFlowEnum.FlowVerStateEnum.Editing)
                            re.Flow = flowCol[1];
                        else
                        {
                            re.Flow = flowCol[0];
                        }
                    }
                    else if(flowCol.Count ==1)
                    {
                        if (flowCol[0].FlowVerState == CPFlowEnum.FlowVerStateEnum.Editing)
                        {
                            re.Result = false;
                            re.ErrorMsg = "当前流程只有一个版本且未正式发布，不能启动流程！";
                            return re;
                        }
                        else
                            re.Flow = flowCol[0];
                    }
                    if(re.Flow == null)
                    {
                        re.Result = false;
                        re.ErrorMsg = "根据流程模板ID【" + FlowId + "】未找到流程模板信息！";
                        return re;
                    }
                    re.Flow.FormatInitValue();
                    List<CPFlowPhase> allPhaseCol = template.GetFlowPhase(re.Flow.FlowVerId, true, false);
                    List<CPFlowPhaseLink> allLinkCol = template.GetFlowPhaseLink(re.Flow.FlowVerId);
                    allLinkCol = allLinkCol.Where(t => t.LinkType.Equals(CPFlowEnum.LinkTypeEnum.Submit)).ToList();
                    re.CurPhase = template.GetFirstPhase(allPhaseCol, allLinkCol);
                    re.InsTitle = re.Flow.FlowName + "_" + new CPRuntimeContext().CurTimeLongString();
                    re.TaskCanFallbackCount = 0;
                    re.CheckUserCanTakeBackFlow = false;
                    #endregion
                }
                else if(  string.IsNullOrEmpty(TaskId) && string.IsNullOrEmpty(InsId)==false)
                {
                    #region 流程查看时
                    CPFlowEngine engine = CPFlowEngine.Instance(CurUserId);
                    CPFlowInstance curIns = engine.GetInstance(int.Parse(InsId));
                    if (curIns == null)
                    {
                        re.ErrorMsg = "未找到流程ID为【" + InsId + "】的流程实例！";
                        re.Result = false; return re;
                    }
                    re.Flow = template.GetFlowByFlowVerId(curIns.FlowVerId, false, false);
                    if (re.Flow == null)
                    {
                        re.Result = false;
                        re.ErrorMsg = "根据流程模板ID【" + curIns.FlowVerId + "】未找到流程模板信息！";
                        return re;
                    }
                    re.InsTitle = curIns.InsTitle;
                    //获取用户所有办理过的节点对应的表单实例 
                    List<CPFlowInstanceLog> logCol = engine.GetInstanceLogByUser(curIns.InsId);
                    if(logCol.Count <=0)
                    {
                        #region 用户没有办理过流程,则自动获取所有表单实例 
                        re.CurPhase = template.GetFirstPhase(re.Flow.FlowVerId);
                        if (re.CurPhase.FormCol != null)
                        {
                            re.CurPhase.FormCol.Clear();
                        }
                        else
                            re.CurPhase.FormCol = new List<CPFlowPhaseForm>();
                        List<CPFlowPhaseForm> allFormCol = template.GetPhaseAllForm(re.Flow.FlowVerId, false);
                        List<CPFlowInstanceForm> insFormCol = engine.GetInstanceForm(curIns.InsId);
                        allFormCol.ForEach(t => {
                            List<CPFlowInstanceForm> tCol = insFormCol.Where(c => c.FormCode.Equals(t.FormCode)).ToList();
                            if(tCol.Count >0)
                            {
                                t.PhaseId = re.CurPhase.PhaseId;
                                t.FormPageUrl += "&IsView=true&PKValues=" + tCol[0].FormPK;
                                re.CurPhase.FormCol.Add(t);
                            }
                        });
                        re.CurPhase.FormCol = re.CurPhase.FormCol.OrderBy(c => c.ShowOrder).ToList();
                        re.InsTitle = curIns.InsTitle;
                        re.TaskCanFallbackCount = 0;
                        re.CheckUserCanTakeBackFlow = false;
                        #endregion
                    }
                    else
                    {
                        #region 办理过，则只取办理过对应的表单
                        List<int> pIdCol = new List<int>();
                        logCol.ForEach(t => {
                            if (pIdCol.Contains(t.RevPhaseId.Value) == false)
                                pIdCol.Add(t.RevPhaseId.Value);
                        });
                        List<CPFlowPhase> phaseCol =  template.GetFlowPhaseByPhaseId(pIdCol, true, false);
                        re.CurPhase = phaseCol[0];
                     //   re.CurPhase.FormCol.Clear();
                        List<CPFlowInstanceForm> insFormCol = engine.GetInstanceForm(curIns.InsId);
                        List<CPFlowPhaseForm> rFormCol = new List<CPFlowPhaseForm>();
                        List<string> addedForm = new List<string>();
                        phaseCol.ForEach(t => {
                            t.FormCol.ForEach(form => {
                                if(addedForm.Contains(form.FormCode)==false)
                                {
                                    List<CPFlowInstanceForm> tCol = insFormCol.Where(c => c.FormCode.Equals(form.FormCode)).ToList();
                                    if (tCol.Count > 0)
                                    {
                                        form.PhaseId = re.CurPhase.PhaseId;
                                        form.FormPageUrl += "&IsView=true&PKValues=" + tCol[0].FormPK;
                                        rFormCol.Add(form);
                                        addedForm.Add(form.FormCode);
                                    }
                                }
                            });
                        });
                      
                        re.CurPhase.FormCol = rFormCol;
                        re.CurPhase.FormCol = re.CurPhase.FormCol.OrderBy(c => c.ShowOrder).ToList();
                        re.InsTitle = curIns.InsTitle;
                        re.TaskCanFallbackCount = 0;
                        string tmpError;
                        re.CheckUserCanTakeBackFlow = engine.CheckUserCanTakeBackFlow(curIns.InsId, out tmpError);
                        if(re.CheckUserCanTakeBackFlow==false && string.IsNullOrEmpty(tmpError)==false)
                        {
                            re.Result = false;
                            re.ErrorMsg = tmpError;
                            return re;
                        }
                        #endregion
                    }
                    #endregion
                }
                else if (string.IsNullOrEmpty(TaskId) == false && string.IsNullOrEmpty(InsId) == false)
                {
                    #region 流程办理时
                    CPFlowEngine engine = CPFlowEngine.Instance(CurUserId);
                    CPFlowInstanceTask curTask = engine.GetInstanceTask(int.Parse(TaskId));
                    if (curTask == null)
                    {
                        re.ErrorMsg = "未找到任务ID为【" + InsId + "】的流程任务！";
                        re.Result = false; return re;
                    }
                    re.Flow = template.GetFlowByFlowVerId(curTask.FlowVerId, false, false);
                    if (re.Flow == null)
                    {
                        re.Result = false;
                        re.ErrorMsg = "根据流程模板ID【" + curTask.FlowVerId + "】未找到流程模板信息！";
                        return re;
                    }
                    re.CurPhase = engine.GetCurPhaseAndFormByTask(curTask);
                    re.InsTitle = curTask.InsTitle;
                    CPFlowInstance curIns = engine.GetInstance(curTask.InsId);
                    if (curIns == null)
                    {
                        re.ErrorMsg = "未找到流程ID为【" + InsId + "】的流程实例！";
                        re.Result = false; return re;
                    }
                    CPFlow curFlow = template.GetFlowByFlowVerId(curTask.FlowVerId,true,true);
                    List<CPFlowPhase> fallbackPhaseCol;
                    string tmpError;
                    re.Result = engine.GetCanFallbackPhase(curIns, curTask, curFlow, out fallbackPhaseCol, out tmpError);
                    if(re.Result==false)
                    {
                        re.ErrorMsg = tmpError;
                        re.TaskCanFallbackCount = 0;
                        return re;
                    }
                    re.TaskCanFallbackCount = fallbackPhaseCol.Count;
                    re.CheckUserCanTakeBackFlow = false;
                    //设置任务为已阅状态
                    re.Result = engine.UpdateTaskToReadState(curTask);
                    if (re.Result == false)
                    {
                        re.ErrorMsg = "设置任务为已阅状态时出错";
                        return re;
                    }
                    #endregion
                }

                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 保存表单与流程的关联信息
        [HttpGet]
        public CPWebApiBaseReturnEntity SaveFlowFormInfo(int InsId,string FormCode,int FormPK,int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden); 
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            re.Result = true;
            try
            {
                CPFlowInstance instance = CPFlowEngine.Instance(CurUserId).GetInstance(InsId);
                CPFlowInstanceForm form = new CPFlowInstanceForm();
                form.InsId = InsId;
                form.FlowId = instance.FlowId;
                form.FlowVerId = instance.FlowVerId;
                form.FormCode = FormCode;
                form.FormPK = FormPK;
                re.Result = CPFlowEngine.Instance(CurUserId).AddInstanceForm(form);
                if(re.Result==false)
                {
                    re.ErrorMsg = "保存表单与流程关系数据时出错。";
                }
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 驳回流程
        public class FallbackFlowReturn:CPWebApiBaseReturnEntity
        {
            public string NewTaskUserNames { get; set; }
        }
        [HttpGet]
        public FallbackFlowReturn FallbackFlow(int InsId, int TaskId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            FallbackFlowReturn re = new FallbackFlowReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            re.Result = true;
            try
            {
                CPFlowEngine engine = CPFlowEngine.Instance(CurUserId);
                string errorMsg;
                List<CPFlowInstanceTask> newTaskCol;
                re.NewTaskUserNames = "";
                re.Result=engine.FallbackTask(InsId, TaskId, out newTaskCol, out errorMsg);
                if (re.Result == false)
                {
                    re.ErrorMsg = errorMsg;
                }
                else
                {
                    if (newTaskCol != null && newTaskCol.Count > 0)
                    {
                        List<int> addedId = new List<int>();
                        newTaskCol.ForEach(t =>
                        {
                            if (addedId.Contains(t.RevUserId.Value) == false)
                            {
                                addedId.Add(t.RevUserId.Value);
                                if (string.IsNullOrEmpty(re.NewTaskUserNames))
                                {
                                    re.NewTaskUserNames = t.RevUserName;
                                }
                                else
                                {
                                    re.NewTaskUserNames += "," + t.RevUserName;
                                }
                            }
                        });
                    }
                }
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }

      
        [HttpGet]
        public FallbackFlowReturn FallbackFlowToAppointPhase(int InsId, int TaskId, int FallbackTargetPhaseId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            FallbackFlowReturn re = new FallbackFlowReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            re.Result = true;
            try
            {
                CPFlowEngine engine = CPFlowEngine.Instance(CurUserId);
                string errorMsg;
                List<CPFlowInstanceTask> newTaskCol;
                re.NewTaskUserNames = "";
                re.Result = engine.FallbackTask(InsId, TaskId, FallbackTargetPhaseId, out newTaskCol, out errorMsg);
                if (re.Result == false)
                {
                    re.ErrorMsg = errorMsg;
                }
                else
                {
                    if (newTaskCol != null && newTaskCol.Count > 0)
                    {
                        List<int> addedId = new List<int>();
                        newTaskCol.ForEach(t =>
                        {
                            if (addedId.Contains(t.RevUserId.Value) == false)
                            {
                                addedId.Add(t.RevUserId.Value);
                                if (string.IsNullOrEmpty(re.NewTaskUserNames))
                                {
                                    re.NewTaskUserNames = t.RevUserName;
                                }
                                else
                                {
                                    re.NewTaskUserNames += "," + t.RevUserName;
                                }
                            }
                        });
                    }
                }
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 获取可驳回的流程节点信息
        public class GetCanFallbackPhaseReturn : CPWebApiBaseReturnEntity
        {
            public List<CPFlowPhase> PhaseCol { get; set; }
        }
        [HttpGet]
        public GetCanFallbackPhaseReturn GetCanFallbackPhase(int TaskId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            GetCanFallbackPhaseReturn re = new GetCanFallbackPhaseReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            re.Result = true;
            try
            {
                CPFlowTemplate template = CPFlowTemplate.Instance();
                CPFlowEngine engine = CPFlowEngine.Instance(CurUserId);
                CPFlowInstanceTask curTask = engine.GetInstanceTask(TaskId);
                if (curTask == null)
                {
                    re.ErrorMsg = "未找到任务ID为【" + TaskId + "】的任务实例！";
                    re.Result = false;
                    return re;
                }
                if (curTask.TaskState == CPFlowEnum.TaskStateEnum.NotActive)
                {
                    re.ErrorMsg = "当前任务由于他人回退任务导致任务暂停或未激活，请稍候再办理！";
                    re.Result = false;
                    return re;
                }
                if (curTask.TaskState == CPFlowEnum.TaskStateEnum.Paused)
                {
                    re.ErrorMsg = "当前任务处于暂停状态，请先恢复办理状态！";
                    re.Result = false;
                    return re;
                }
                CPFlowInstance curIns = engine.GetInstance(curTask.InsId);
                CPFlow curFlow = template.GetFlowByFlowVerId(curTask.FlowVerId, true, true);
                List<CPFlowPhase> fallbackPhaseCol;
                string errorMsg;
                re.Result = engine.GetCanFallbackPhase(curIns,curTask, curFlow, out fallbackPhaseCol, out errorMsg);
                if (re.Result == false)
                {
                    re.ErrorMsg = errorMsg;
                    return re;
                }
                else
                {
                    fallbackPhaseCol.ForEach(t => { t.InitTaskDefaultRevUser(curIns, CurUserId); });
                    re.PhaseCol = fallbackPhaseCol;
                    return re;
                }
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 取回流程
        public class TakeBackFlowReturn : CPWebApiBaseReturnEntity
        {
            public string NewTaskUserNames { get; set; }
        }
        [HttpGet]
        public TakeBackFlowReturn TakeBackFlow(int InsId,  int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            TakeBackFlowReturn re = new TakeBackFlowReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            re.Result = true;
            try
            {
                CPFlowEngine engine = CPFlowEngine.Instance(CurUserId);
                string errorMsg;
                List<CPFlowInstanceTask> newTaskCol;
                re.NewTaskUserNames = "";
                re.Result = engine.TakeBackFlow(InsId,   out newTaskCol, out errorMsg);
                if (re.Result == false)
                {
                    re.ErrorMsg = errorMsg;
                }
                else
                {
                    if (newTaskCol != null && newTaskCol.Count > 0)
                    {
                        List<int> addedId = new List<int>();
                        newTaskCol.ForEach(t =>
                        {
                            if (addedId.Contains(t.RevUserId.Value) == false)
                            {
                                addedId.Add(t.RevUserId.Value);
                                if (string.IsNullOrEmpty(re.NewTaskUserNames))
                                {
                                    re.NewTaskUserNames = t.RevUserName;
                                }
                                else
                                {
                                    re.NewTaskUserNames += "," + t.RevUserName;
                                }
                            }
                        });
                    }
                }
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }


     
        #endregion
    }
}
