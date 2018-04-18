using AutoMapper;
using CPFrameWork.Utility.DbOper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPFrameWork.Flow.Domain
{
    public class CPFlowPhaseTaskRevUser
    {
        public int RevUserId { get; set; }
        public string  RevUserName{ get; set; }
        public int RevSourceUserId { get; set; }
        public string RevSourceUserName { get; set; }
    }
    public class CPFlow: BaseOrderEntity
    {
        /// <summary>
        /// 流程版本ID
        /// </summary>
        public int FlowVerId
        {
            get
            {
                return this.Id;
            }
        }
        /// <summary>
        /// 流程ID
        /// </summary>
        public int FlowId { get; set; }
        /// <summary>
        /// 流程
        /// </summary>
        public int FlowClassId { get; set; }
        /// <summary>
        /// 流程类型 1：任意式工作流 2：顺序式工作流
        /// </summary>
        public CPFlowEnum.FlowTypeEnum FlowType { get; set; }

        /// <summary>
        /// 流程标题
        /// </summary>
        public string FlowName { get; set; }
        /// <summary>
        /// 此版本创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 流程版本状态 0：修改中 1：已发布
        /// </summary>
        public CPFlowEnum.FlowVerStateEnum? FlowVerState { get; set; }
        /// <summary>
        /// 流程备注
        /// </summary>
        public string FlowRemark { get; set; }
        /// <summary>
        /// 流程回退模式1：任意节点回退均回退到发起节点 2：回退到上一节点
        /// </summary>
        public CPFlowEnum.FlowRebackModelEnum? FlowRebackModel { get; set; }
        /// <summary>
        /// 如果任务是回退的，任务再次提交时，提交方式
        /// </summary>
        public CPFlowEnum.RebackTaskSubmitTypeEnum? RebackTaskSubmitType { get; set; }
        /// <summary>
        /// 具有此流程发起权限的用户ID，多个用，分隔
        /// </summary>
        public string HasRightUserIds { get; set; }
        /// <summary>
        /// 具有此流程发起权限的用户姓名，多个用，分隔
        /// </summary>
        public string HasRightUserNames { get; set; }
        /// <summary>
        /// 具有此流程发起权限的角色ID，多个用，分隔
        /// </summary>
        public string HasRightRoleIds { get; set; }
        /// <summary>
        /// 具有此流程发起权限的角色名称，多个用，分隔
        /// </summary>
        public string HasRightRoleNames { get; set; }
        /// <summary>
        /// 具有此流程发起权限的部门ID，多个用，分隔
        /// </summary>
        public string HasRightDepIds { get; set; }
        /// <summary>
        /// 具有此流程发起权限的部门名称，多个用，分隔
        /// </summary>
        public string HasRightDepNames { get; set; }
        /// <summary>
        /// 具有此流程监控权限的用户ID，多个用，分隔
        /// </summary>
        public string JGUserIds { get; set; }
        /// <summary>
        /// 具有此流程监控权限的用户姓名，多个用，分隔
        /// </summary>
        public string JGUserNames { get; set; }
        /// <summary>
        /// 具有此流程监控权限的角色ID，多个用，分隔
        /// </summary>
        public string JGRoleIds { get; set; }
        /// <summary>
        /// 具有此流程监控权限的角色姓名，多个用，分隔
        /// </summary>
        public string JGRoleNames { get; set; }
        /// <summary>
        /// 消息标题的模板,支持表达式
        /// </summary>
        public string MsgTitleTemplate { get; set; }
        /// <summary>
        /// 消息正文的模板,支持表达式
        /// </summary>
        public string MsgContentTemplate { get; set; }
        /// <summary>
        /// 消息客户端办理业务时页面地址模板        支持表达式
        /// </summary>
        public string MsgTaskManaTemplate { get; set; }
        /// <summary>
        /// 默认内置按钮在未启动流程实例时的权限        配置格式为：按钮ID|权限ID @按钮ID|权限ID 
        /// </summary>
        public string InnerFuncRight { get; set; }
        /// <summary>
        /// 办理页面内置选项卡是否显示的配置
            //        按钮显示的状态
            //0:显示
            //1：不显示
            //内置选项卡分别为：
            //表单：1 
            //流程：2 
            //配置格式为：
            //按钮ID|权限ID @按钮ID|
        /// </summary>
        public string InnerTabShow { get; set; }
        /// <summary>
        /// 办理页面内置选项卡标题配置
          //内置选项卡分别为：
          //  表单：1 
          //  流程：2 
          //  配置格式为：
          //  按钮ID|标题 @按钮ID|标题
        /// </summary>
        public string InnerTabTitle { get; set; }
        /// <summary>
        /// 节点提交页面地址
        /// </summary>
        public string SubmitPageUrl { get; set; }
        /// <summary>
        /// 节点提交页面地址
        /// </summary>
        public string RebackPageUrl { get; set; }
        /// <summary>
        /// 选择用户页面地址
        /// </summary>
        public string SelectUserPageUrl { get; set; }
        /// <summary>
        /// 业务扩展配置
        /// </summary>
        public string FlowConfigEx { get; set; }
        /// <summary>
        /// 扩展JS
        /// </summary>
        public string JSEx { get; set; }
        /// <summary>
        /// 扩展样式表
        /// </summary>
        public string CSSEx { get; set; }
        /// <summary>
        /// 流程实例化后扩展SQL，支持表达式
        /// </summary>
        public string InsInitSqlEx { get; set; }
        /// <summary>
        /// 流程实例化后扩展类，多个用；分隔
        /// </summary>
        public string InsInitClassEx { get; set; }
        /// <summary>
        /// 流程实例删除前扩展SQL，支持表达式
        /// </summary>
        public string InsBeforeDelSqlEx { get; set; }
        /// <summary>
        /// 流程实例删除前扩展类，多个用；分隔
        /// </summary>
        public string InsBeforeDelClassEx { get; set; }
        /// <summary>
        /// 流程实例删除后扩展SQL，支持表达式
        /// </summary>
        public string InsAfterDelSqlEx { get; set; }
        /// <summary>
        /// 流程实例删除后扩展类，多个用；分隔
        /// </summary>
        public string InsAfterDelClassEx { get; set; }
        /// <summary>
        /// 流程实例终止后扩展SQL，支持表达式
        /// </summary>
        public string InsAfterStopSqlEx { get; set; }
        /// <summary>
        /// 流程实例终止后扩展类，多个用；分隔
        /// </summary>
        public string InsAfterStopClassEx { get; set; }
        /// <summary>
        /// 流程实例作废后扩展SQL，支持表达式
        /// </summary>
        public string InsAfterInvalidSqlEx { get; set; }
        /// <summary>
        /// 流程实例作废后扩展类，多个用；分隔
        /// </summary>
        public string InsAfterInvalidClassEx { get; set; }
        /// <summary>
        /// 流程实例取回后扩展SQL，支持表达式
        /// </summary>
        public string InsAfterTakeBackSqlEx { get; set; }
        /// <summary>
        /// 流程实例取回后扩展类，多个用；分隔
        /// </summary>
        public string InsAfterTakeBackClassEx { get; set; }
        /// <summary>
        /// 流程实例暂停后扩展SQL，支持表达式
        /// </summary>
        public string InsAfterPauseSqlEx { get; set; }
        /// <summary>
        /// 流程实例暂停后扩展类，多个用；分隔
        /// </summary>
        public string InsAfterPauseClassEx { get; set; }
        /// <summary>
        /// 流程实例结束后扩展SQL，支持表达式
        /// </summary>
        public string InsAfterEndSqlEx { get; set; }
        /// <summary>
        /// 流程实例结束后扩展类，多个用；分隔
        /// </summary>
        public string InsAfterEndClassEx { get; set; }
        /// <summary>
        /// 流程页面加载扩展类，多个用；分隔
        /// </summary>
        public string InsPageLoadEx { get; set; }

        /// <summary>
        /// 阶段集合
        /// </summary>
        public List<CPFlowPhase> PhaseCol { get; set; }
        /// <summary>
        /// 路由集合，包含提交和回退的
        /// </summary>
        public List<CPFlowPhaseLink> PhaseLinkCol { get; set; }

        private List<CPFlowPhaseLink> _PhaseLinkColSubmit = null;
        /// <summary>
        /// 提交路由集合
        /// </summary>
        public List<CPFlowPhaseLink> PhaseLinkColSubmit
        {
            get
            {
                if (this._PhaseLinkColSubmit == null && this.PhaseLinkCol !=null)
                {
                    this._PhaseLinkColSubmit = this.PhaseLinkCol.Where(t => t.LinkType.Equals(CPFlowEnum.LinkTypeEnum.Submit)).ToList();
                }
                return this._PhaseLinkColSubmit;
            }
        }
        private List<CPFlowPhaseLink> _PhaseLinkColFallback = null;
        /// <summary>
        /// 回退路由集合
        /// </summary>
        public List<CPFlowPhaseLink> PhaseLinkColFallback
        {
            get
            {
                if (this._PhaseLinkColFallback == null && this.PhaseLinkCol != null)
                {
                    this._PhaseLinkColFallback = this.PhaseLinkCol.Where(t => t.LinkType.Equals(CPFlowEnum.LinkTypeEnum.Fallback)).ToList();
                }
                return this._PhaseLinkColFallback;
            }
        }

        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.CreateTime.HasValue == false)
                this.CreateTime = DateTime.Now;
            if (this.FlowVerState.HasValue == false)
                this.FlowVerState = CPFlowEnum.FlowVerStateEnum.Release;
            if (this.FlowRebackModel.HasValue == false)
                this.FlowRebackModel = CPFlowEnum.FlowRebackModelEnum.ToPre;
            if (this.RebackTaskSubmitType.HasValue == false)
                this.RebackTaskSubmitType = CPFlowEnum.RebackTaskSubmitTypeEnum.ToRebackPhase;
        }

    }

    public class CPFlowPhase:BaseEntity
    {
        /// <summary>
        /// 阶段ID
        /// </summary>
        public int PhaseId { get { return this.Id; } }
        /// <summary>
        /// 流程版本ID
        /// </summary>
        public int FlowVerId { get; set; }
        /// <summary>
        /// 阶段名称
        /// </summary>
        public string PhaseName { get; set; }
        /// <summary>
        /// 阶段类型
        /// </summary>
        public  CPFlowEnum.PhaseTypeEnum? PhaseType { get; set; }
        /// <summary>
        /// 默认办理用户所属角色名称，多个用, 分隔
        /// </summary>
        public string DefaultRoleNames { get; set; }
        /// <summary>
        /// 默认办理用户所属角色代码，多个用, 分隔
        /// </summary>
        public string DefaultRoleIds { get; set; }
        /// <summary>
        /// 默认办理用户所属办理用户，多个用,分隔
        /// </summary>
        public string DefaultUserNames { get; set; }
        /// <summary>
        /// 默认办理用户所属办理用户，多个用，分隔
        /// </summary>
        public string DefaultUserIds { get; set; }
        /// <summary>
        /// 是否出现选择用户的按钮
        /// </summary>
        public bool? IsCanSelectUser { get; set; }
        /// <summary>
        /// 如果可以选择用户，选择用户范围
        /// </summary>
        public CPFlowEnum.UserSelScaleEnum? UserSelScale { get; set; }
        /// <summary>
        /// 指定用户IDs，多个用，分隔
        /// </summary>
        public string SelUserIds { get; set; }
        /// <summary>
        /// 指定用户姓名s，多个用，分隔
        /// </summary>
        public string SelUserNames { get; set; }
        /// <summary>
        /// 指定角色IDs，多个用，分隔
        /// </summary>
        public string SelRoleIds { get; set; }
        //指定角色名称，多个用，分隔
        public string SelRoleNames { get; set; }
        /// <summary>
        /// 默认内置按钮的权限        配置格式为：按钮ID|权限ID @按钮ID|权限ID
        /// </summary>
        public string InnerFuncRight { get; set; }
        /// <summary>
        /// 内置功能按钮显示标题        配置格式 ：按钮ID|标题 @按钮ID|标题
        /// </summary>
        public string InnerFuncTitle { get; set; }
        /// <summary>
        /// 在设计器中x位置
        /// </summary>
        public double? PositionX { get; set; }
        /// <summary>
        /// 在设计器中y位置
        /// </summary>
        public double? PositionY { get; set; }
        /// <summary>
        /// 在设计器中宽
        /// </summary>
        public double? Width { get; set; }
        /// <summary>
        /// 在设计 器中高
        /// </summary>
        public double? Height { get; set; }
        /// <summary>
        /// 阶段提交前扩展SQL，支持表达式
        /// </summary>
        public string BeforeSubmitSqlEx { get; set; }
        /// <summary>
        /// 阶段提交前扩展类，多个用；分隔
        /// </summary>
        public string BeforeSubmitClassEx { get; set; }
        /// <summary>
        /// 阶段提交后扩展SQL，支持表达式
        /// </summary>
        public string AfterSubmitSqlEx { get; set; }
        /// <summary>
        /// 阶段提交后扩展类，多个用；分隔
        /// </summary>
        public string AfterSubmitClassEx { get; set; }
        /// <summary>
        /// 阶段回退前扩展SQL，支持表达式
        /// </summary>
        public string BeforeRebackSqlEx { get; set; }
        /// <summary>
        /// 阶段回退前扩展类，多个用；分隔
        /// </summary>
        public string BeforeRebackClassEx { get; set; }
        /// <summary>
        /// 阶段回退后扩展SQL，支持表达式
        /// </summary>
        public string AfterRebackSqlEx { get; set; }
        /// <summary>
        /// 阶段回退后扩展类，多个用；分隔
        /// </summary>
        public string AfterRebackClassEx { get; set; }
        /// <summary>
        /// 阶段业务扩展配置
        /// </summary>
        public string PhaseConfigEx { get; set; }

        #region 存储用户真正的办理用户
        public List<CPFlowPhaseTaskRevUser> TaskRevUser { get; set; }
        /// <summary>
        /// 初始化用户信息
        /// </summary>
        public void InitTaskDefaultRevUser(CPFlowInstance curIns,int CurUserId)
        {
            if(this.TaskRevUser == null || this.TaskRevUser.Count <=0)
            {
                if (this.TaskRevUser == null)
                    this.TaskRevUser = new List<CPFlowPhaseTaskRevUser>();
                bool bInit = false;
                #region 针对分支汇聚节点，当前面有办理用户选择了人时，后面的办理用户就直接默认选择了的用户，或者针对回退后再提交到这个节点时，默认也是取上次办理的用户
                List<CPFlowInstanceTask> taskCol = CPFlowEngine.Instance(CurUserId).GetInstanceTask(curIns.InsId, this.PhaseId);
                taskCol.ForEach(t => {
                    if (this.TaskRevUser.Where(c => c.RevUserId.Equals(t.RevUserId.Value)).ToList().Count <= 0)
                    {
                        CPFlowPhaseTaskRevUser u = new CPFlowPhaseTaskRevUser();
                        u.RevUserId = t.RevUserId.Value;
                        u.RevUserName = t.RevUserName;
                        u.RevSourceUserId = u.RevUserId;
                        u.RevSourceUserName = u.RevUserName;
                        this.TaskRevUser.Add(u);
                        bInit = true;
                    }
                });
                if(!bInit)
                {
                    List<CPFlowInstanceLog> logCol =  CPFlowEngine.Instance(CurUserId).GetInstanceLog(curIns.InsId, this.PhaseId);
                    logCol.ForEach(t => {
                        if (this.TaskRevUser.Where(c => c.RevUserId.Equals(t.RevUserId.Value)).ToList().Count <= 0)
                        {
                            CPFlowPhaseTaskRevUser u = new CPFlowPhaseTaskRevUser();
                            u.RevUserId = t.RevUserId.Value;
                            u.RevUserName = t.RevUserName;
                            u.RevSourceUserId = u.RevUserId;
                            u.RevSourceUserName = u.RevUserName;
                            this.TaskRevUser.Add(u);
                            bInit = true;
                        }
                    });
                }
                #endregion
                //如果没有传入指定的用户，则取默认办理用户
                if (bInit ==false && string.IsNullOrEmpty(this.DefaultUserIds)==false)
                {
                    string[] idCol = this.DefaultUserIds.Split(',');
                    string[] nameCol = this.DefaultUserNames.Split(',');
                    List<int> addedIdCol = new List<int>();
                    for(int i =0;i<idCol.Length;i++)
                    {
                        if (addedIdCol.Contains(int.Parse(idCol[i])) == false)
                        {
                            addedIdCol.Add(int.Parse(idCol[i]));
                            if (this.TaskRevUser.Where(c => c.RevUserId.Equals(int.Parse(idCol[i]))).ToList().Count <= 0)
                            {
                                this.TaskRevUser.Add(new CPFlowPhaseTaskRevUser()
                                {
                                    RevUserId = int.Parse(idCol[i]),
                                    RevUserName = nameCol[i],
                                    RevSourceUserId = int.Parse(idCol[i]),
                                    RevSourceUserName = nameCol[i]
                                });
                            }
                        }
                    }
                }
                if (bInit == false && string.IsNullOrEmpty(this.DefaultRoleIds) == false)
                {
                    throw new Exception("未开发完");
                }
            }
        }
        #endregion

        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPFlow Flow { get; set; }

        /// <summary>
        /// 表单集合
        /// </summary>
        public List<CPFlowPhaseForm> FormCol { get; set; }

        /// <summary>
        /// 规则集合
        /// </summary>
        public List<CPFlowPhaseRule> RuleCol { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.PhaseType.HasValue == false)
                this.PhaseType = CPFlowEnum.PhaseTypeEnum.Normal;
            if (this.IsCanSelectUser.HasValue == false)
                this.IsCanSelectUser = true;
            if (this.UserSelScale.HasValue == false)
                this.UserSelScale = CPFlowEnum.UserSelScaleEnum.AllUser;
            if (this.PositionX.HasValue == false)
                this.PositionX = 100;
            if (this.PositionY.HasValue == false)
                this.PositionY = 100;
        }
    }

    public class CPFlowPhaseLink : BaseEntity
    {
        /// <summary>
        /// 路由ID
        /// </summary>
        public int LinkId { get { return this.Id; } }
        /// <summary>
        /// 流程版本ID
        /// </summary>
        public int FlowVerId { get; set; }
        /// <summary>
        /// 路由类型        提交：1 回退：2 
        /// </summary>
        public CPFlowEnum.LinkTypeEnum LinkType { get; set; }
        /// <summary>
        /// 开始阶段ID
        /// </summary>
        public int StartPhaseId { get; set; }
        /// <summary>
        /// 结束阶段ID
        /// </summary>
        public int EndPhaseId { get; set; }
        /// <summary>
        /// 条件路由类型
        /// </summary>
        public CPFlowEnum.LinkConditionTypeEnum? LinkConditionType { get; set; }
        /// <summary> 
        /// 条件路由详细配置 说明：当以上条件返回1时，表示允许进入此路由，否则不允许
        /// </summary>
        public string LinkCondition { get; set; }
        /// <summary>
        /// 提示信息X坐标
        /// </summary>
        public double? TipX { get; set; }
        /// <summary>
        /// 提示信息Y坐标
        /// </summary>
        public double? TipY { get; set; }
        /// <summary>
        /// 如果路由类型SQL语句，则选择执行此SQL语句的数据库实例
        /// </summary>
        public string DbIns { get; set; }
        /// <summary>
        /// 定制器用 存储格式为：        X,y;x,y
        /// </summary>
        public string LinkDots { get; set; }
        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPFlow Flow { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.LinkConditionType.HasValue == false)
                this.LinkConditionType = CPFlowEnum.LinkConditionTypeEnum.NoCondition;
        }
    }

    public class CPFlowPhaseRule:BaseEntity
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        public int RuleId { get { return this.Id; } }
        /// <summary>
        /// 流程版本ID
        /// </summary>
        public int FlowVerId { get; set; }
        /// <summary>
        /// 阶段ID
        /// </summary>
        public int PhaseId { get; set; }
        /// <summary>
        /// 规则适用类型 1：提交任务 2：回退任务
        /// </summary>
        public CPFlowEnum.RuleTypeEnum? RuleType { get; set; }
        /// <summary>
        /// 规则比较类型1：通过SQL语句比较2：自定义扩展类
        /// </summary>
        public CPFlowEnum.RuleCompareTypeEnum? RuleCompareType { get; set; }
        /// <summary>
        /// 比较源数据库实例
        /// </summary>
        public string SourceDbIns { get; set; }
        /// <summary>
        /// 比较源SQL语句，支持表达式
        /// </summary>
        public string SourceSql { get; set; }
        /// <summary>
        /// 比较类型
        /// </summary>
        public CPFlowEnum.CompareTypeEnum? CompareType { get; set; }
        /// <summary>
        /// 目标值数据库实例
        /// </summary>
        public string TargetDbIns { get; set; }
        /// <summary>
        /// 1、	目标值SQL语句，支持表达式2、	如果规则类型为扩展类，则此处存储类信息 
        /// </summary>
        public string TargetSqlOrCustomClass { get; set; }

        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPFlowPhase FlowPhase { get; set; }

        public List<CPFlowPhaseRuleHandle> RuleHandleCol { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.RuleType.HasValue == false)
                this.RuleType = CPFlowEnum.RuleTypeEnum.Submit;

            if (this.RuleCompareType.HasValue == false)
                this.RuleCompareType = CPFlowEnum.RuleCompareTypeEnum.BySql;

            if (this.CompareType.HasValue == false)
                this.CompareType = CPFlowEnum.CompareTypeEnum.Contain;

        }
    }

    public class CPFlowPhaseRuleHandle : BaseEntity
    {
        /// <summary>
        /// 规则操作ID
        /// </summary>
        public int RuleHandleId { get { return this.Id; } }
        /// <summary>
        /// 流程版本ID
        /// </summary>
        public int FlowVerId { get; set; }
        /// <summary>
        /// 阶段ID
        /// </summary>
        public int PhaseId { get; set; }
        /// <summary>
        /// 规则ID
        /// </summary>
        public int RuleId { get; set; }
        /// <summary>
        /// 操作适用情况
        /// </summary>
        public CPFlowEnum.HandleApplyTypeEnum HandleApplyType { get; set; }
        /// <summary>
        /// 操作类型
        /// </summary>
        public CPFlowEnum.HandleTypeEnum HandleType { get; set; }
        /// <summary>
        /// 提示信息
        /// </summary>
        public string AlertInfo { get; set; }
        /// <summary>
        /// 需要启动的流程ID
        /// </summary>
        public int? StartFlowId { get; set; }
        /// <summary>
        /// 需要启动的流程名称
        /// </summary>
        public string StartFlowName { get; set; }
        /// <summary>
        /// 初始化表单数据的方法
        /// </summary>
        public CPFlowEnum.InitFormMethodTypeEnum? InitFormMethodType { get; set; }
        /// <summary>
        /// 初始化表单数据SQL语句或扩展类
        /// 支持表达式替换
        /// SQL语句，需要返回新表单数据的主键（第一个字段）和表单访问URL地址（第二个字段）和流程实例名称(第三个字段)，，扩展类也一样
        /// </summary>
        public string InitFormContext { get; set; }
        /// <summary>
        /// 流程默认办理用户角色
        /// </summary>
        public int? TaskManaUserRoleId { get; set; }
        /// <summary>
        /// 流程默认办理用户角色名称
        /// </summary>
        public string TaskManaUserRoleName { get; set; }

        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPFlowPhaseRule FlowPhaseRule { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.InitFormMethodType.HasValue == false)
                this.InitFormMethodType = CPFlowEnum.InitFormMethodTypeEnum.BySql;
        }
    }

    public class CPFlowPhaseForm : BaseOrderEntity {
        /// <summary>
        /// 表单ID
        /// </summary>
        public int FormId { get { return this.Id; } }
        /// <summary>
        /// 流程版本ID
        /// </summary>
        public int FlowVerId { get; set; }
        /// <summary>
        /// 阶段ID
        /// </summary>
        public int PhaseId { get; set; }
        /// <summary>
        /// 表单显示标题
        /// </summary>
        public string FormTitle { get; set; }
        /// <summary>
        /// 表单页面地址
        /// </summary>
        public string FormPageUrl { get; set; }
        /// <summary>
        /// 唯一标识表单的表单Code，不能为空，且同一表单在不同的阶段得配置成一样
        /// </summary>
        public string FormCode { get; set; }
        /// <summary>
        /// 表单初始化组号
        /// </summary>
        public string FormInitGroupCode { get; set; }
        /// <summary>
        /// 表单权限组号
        /// </summary>
        public string FormRightGroupCode { get; set; }
        /// <summary>
        /// 是否是主表单
        /// </summary>
        public bool? IsMainForm { get; set; }


        [Newtonsoft.Json.JsonIgnore]//webapi时，json序列化时不返回客户端
        public CPFlowPhase FlowPhase { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.IsMainForm.HasValue == false)
                this.IsMainForm = true;
        }
    }


    public class CPFlowInstance:BaseEntity
    {
        /// <summary>
        /// 实例ID
        /// </summary>
        public int InsId { get { return this.Id; } }
        /// <summary>
        /// 流程版本ID
        /// </summary>

        public int FlowVerId { get; set; }
        /// <summary>
        /// 流程主ID
        /// </summary>

        public int FlowId { get; set; }

        /// <summary>
        /// 流程模板标题
        /// </summary>
        public string FlowName { get; set; }

        /// <summary>
        /// 流程标题
        /// </summary>
        public string InsTitle { get; set; }
        /// <summary>
        /// 创建用户ID
        /// </summary>
        public int? CreateUserId { get; set; }

        /// <summary>
        /// 创建用户姓名
        /// </summary>
        public string CreateUserName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        public CPFlowEnum.InsStateEnum InsState { get; set; }

        /// <summary>
        /// 状态标题
        /// </summary>
        public string InsStateTitle { get; set; }
        /// <summary>
        /// 流程作废、终止、或正常结束时间
        /// </summary>
        public DateTime? InsEndTime { get; set; }
        /// <summary>
        /// 流程作废、终止、或正常结束用户ID
        /// </summary>
        public int? InsEndUserId { get; set; }

        /// <summary>
        /// 流程作废、终止、或正常结束用户姓名
        /// </summary>
        public string InsEndUserName { get; set; }

        /// <summary>
        /// 流程作废、终止原因描述
        /// </summary>
        public string InsEndRemark { get; set; }
        /// <summary>
        /// 主表单ID，暂时只支持int型主键
        /// </summary>
        public int MainFormPk { get; set; }

        /// <summary>
        /// 扩展字段，由二次开发写入
        /// </summary>
        public string InsEx1 { get; set; }

        /// <summary>
        /// 扩展字段，由二次开发写入
        /// </summary>
        public string InsEx2 { get; set; }

        /// <summary>
        /// 扩展字段，由二次开发写入
        /// </summary>
        public string InsEx3 { get; set; }

        /// <summary>
        /// 扩展字段，由二次开发写入
        /// </summary>
        public string InsEx4 { get; set; }

        /// <summary>
        /// 扩展字段，由二次开发写入
        /// </summary>
        public string InsEx5 { get; set; }

        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.CreateUserId.HasValue == false)
                this.CreateUserId = 0;
            if (this.CreateTime.HasValue == false)
                this.CreateTime = DateTime.Now;
        }
    }

    public class CPFlowInstanceTask : BaseEntity
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int TaskId { get { return this.Id; } }
        /// <summary>
        /// 流程实例ID
        /// </summary>
        public int InsId { get; set; }
        /// <summary>
        /// 流程版本ID
        /// </summary>
        public int FlowVerId { get; set; }
        /// <summary>
        /// 流程主ID
        /// </summary>
        public int FlowId { get; set; }

        /// <summary>
        /// 流程模板标题
        /// </summary>
        public string FlowName { get; set; }

        /// <summary>
        /// 流程标题
        /// </summary>
        public string InsTitle { get; set; }
        /// <summary>
        /// 任务状态
        /// </summary>
        public CPFlowEnum.TaskStateEnum TaskState { get; set; }
        /// <summary>
        /// 由于其它任务回退时，会自动把并行的任务暂时改成未激活状态，这里记录的是修改前的状态
        /// </summary>
        public CPFlowEnum.TaskStateEnum? RebackPreTaskState { get; set; }
        /// <summary>
        /// 任务产生方式
        /// </summary>
        public CPFlowEnum.TaskMakeTypeEnum TaskMakeType { get; set; }
        /// <summary>
        /// 任务类型
        /// </summary>
        public CPFlowEnum.TaskTypeEnum TaskType { get; set; }

        /// <summary>
        /// 提交或回退阶段ID，多个用，分隔
        /// </summary>
        public string SubmitPhaseIds { get; set; }

        /// <summary>
        /// 提交或回退阶段名称，多个用，分隔
        /// </summary>
        public string SubmitPhaseNames { get; set; }

        /// <summary>
        /// 提交或回退任务ID，多个用，分隔
        /// </summary>
        public string SubmitTaskIds { get; set; }

        /// <summary>
        /// /提交或回退用户ID，多个用，分隔
        /// </summary>
        public string SubmitUserIds { get; set; }

        /// <summary>
        /// 提交或回退用户姓名，多个用，分隔
        /// </summary>
        public string SubmitUserNames { get; set; }
        /// <summary>
        /// 接收任务阶段ID
        /// </summary>
        public int? RevPhaseId { get; set; }

        /// <summary>
        /// 接收任务阶段名称
        /// </summary>
        public string RevPhaseName { get; set; }
        /// <summary>
        /// 接收任务用户ID
        /// </summary>
        public int? RevUserId { get; set; }

        /// <summary>
        /// 接收任务用户姓名
        /// </summary>
        public string RevUserName { get; set; }
        /// <summary>
        /// 源任务接收用户ID，主要用来处理委托的情况，这里存储委托人ID
        /// </summary>
        public int? RevSourceUserId { get; set; }

        /// <summary>
        /// 源任务接收用户ID，主要用来处理委托的情况，这里存储委托人姓名
        /// </summary>
        public string RevSourceUserName { get; set; }
        /// <summary>
        /// 接收任务时间
        /// </summary>
        public DateTime? RevTime { get; set; }
        /// <summary>
        /// 接收用户是否已看
        /// </summary>
        public bool? RevUserIsView { get; set; }
        /// <summary>
        /// 接收用户查看时间
        /// </summary>
        public DateTime? RevUserViewTime { get; set; }
        /// <summary>
        /// 任务暂停原因描述
        /// </summary>
        public string PauseRemark { get; set; }
        /// <summary>
        /// 任务暂停用户ID
        /// </summary>
        public int? PauseUserId { get; set; }

        /// <summary>
        /// 任务暂停用户姓名
        /// </summary>
        public string PauseUserName { get; set; }
        /// <summary>
        /// 任务暂停时间
        /// </summary>
        public DateTime? PauseTime { get; set; }

        /// <summary>
        /// 任务消息提醒ID，多个用，分隔，则消息扩展接口负责解析
        /// </summary>
        public string MsgIds { get; set; }

        public override void FormatInitValue()
        {
            base.FormatInitValue();
        }
        public static CPFlowInstanceTask InitFromInstanceAndPhase(CPFlowInstance ins,
            CPFlowInstanceTask curTask,
            CPFlowPhase phase,
            CPFlowPhaseTaskRevUser revUser,
             List<CPFlowPhaseLink> phaseAllLinkCol)
        {
            CPFlowInstanceTask newTask = new CPFlowInstanceTask();
            newTask.InsId = ins.InsId;
            newTask.FlowVerId = ins.FlowVerId;
            newTask.FlowId = ins.FlowId;
            newTask.FlowName = ins.FlowName;
            newTask.InsTitle = ins.InsTitle;
            //有多条路由进入，则默认设置成不激活，否则设置成激活
            if (phaseAllLinkCol.Where(c => c.EndPhaseId.Equals(phase.PhaseId)).ToList().Count > 1)
            {
                newTask.TaskState = CPFlowEnum.TaskStateEnum.NotActive;
            }
            else
            {
                newTask.TaskState = CPFlowEnum.TaskStateEnum.AlreadyActivated;
            }
            newTask.TaskMakeType = CPFlowEnum.TaskMakeTypeEnum.Submit;
            if (phase.PhaseType == CPFlowEnum.PhaseTypeEnum.Circulation)
                newTask.TaskType = CPFlowEnum.TaskTypeEnum.MissionTask;
            else
                newTask.TaskType = CPFlowEnum.TaskTypeEnum.NormalTask;
            newTask.SubmitPhaseIds = curTask.RevPhaseId.ToString();
            newTask.SubmitPhaseNames = curTask.RevPhaseName;
            newTask.SubmitTaskIds = curTask.TaskId.ToString();
            newTask.SubmitUserIds = curTask.RevUserId.ToString() ;
            newTask.SubmitUserNames = curTask.RevUserName;
            newTask.RevPhaseId = phase.PhaseId;
            newTask.RevPhaseName = phase.PhaseName;
            newTask.RevUserId = revUser.RevUserId;
            newTask.RevUserName = revUser.RevUserName;
            newTask.RevSourceUserId = revUser.RevSourceUserId;
            newTask.RevSourceUserName = revUser.RevSourceUserName;
            newTask.RevTime = DateTime.Now;
            newTask.RevUserIsView = false;
            return newTask;

        }
    }

    public class CPFlowInstanceLog:BaseEntity
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        public int LogId { get { return this.Id;} }
        /// <summary>
        /// 任务ID
        /// </summary>
        public int TaskId { get; set; }
        /// <summary>
        /// 流程实例ID
        /// </summary>
        public int InsId { get; set; }
        /// <summary>
        /// 流程版本ID
        /// </summary>
        public int FlowVerId { get; set; }
        /// <summary>
        /// 流程主ID
        /// </summary>
        public int FlowId { get; set; }

        /// <summary>
        /// 流程模板标题
        /// </summary>
        public string FlowName { get; set; }

        /// <summary>
        /// 流程标题
        /// </summary>
        public string InsTitle { get; set; }
        /// <summary>
        /// 任务产生方式
        /// </summary>
        public CPFlowEnum.TaskMakeTypeEnum? TaskMakeType { get; set; }
        /// <summary>
        /// 任务类型
        /// </summary>
        public CPFlowEnum.TaskTypeEnum? TaskType { get; set; }

        /// <summary>
        /// 提交或回退阶段ID，多个用，分隔
        /// </summary>
        public string SubmitPhaseIds { get; set; }

        /// <summary>
        /// 提交或回退阶段名称，多个用，分隔
        /// </summary>
        public string SubmitPhaseNames { get; set; }

        /// <summary>
        /// /提交或回退任务ID，多个用，分隔
        /// </summary>
        public string SubmitTaskIds { get; set; }

        /// <summary>
        /// 提交或回退用户ID，多个用，分隔
        /// </summary>
        public string SubmitUserIds { get; set; }

        /// <summary>
        /// 提交或回退用户姓名，多个用，分隔
        /// </summary>
        public string SubmitUserNames { get; set; }
        /// <summary>
        /// 接收任务阶段ID
        /// </summary>
        public int? RevPhaseId { get; set; }

        /// <summary>
        /// 接收任务阶段名称 
        /// </summary>
        public string RevPhaseName { get; set; }
        /// <summary>
        /// 接收任务用户ID
        /// </summary>
        public int? RevUserId { get; set; }

        /// <summary>
        /// 接收任务用户姓名
        /// </summary>
        public string RevUserName { get; set; }
        /// <summary>
        /// 源任务接收用户ID，主要用来处理委托的情况，这里存储委托人ID
        /// </summary>
        public int? RevSourceUserId { get; set; }
        /// <summary>
        /// 源任务接收用户姓名，主要用来处理委托的情况，这里存储委托人姓名
        /// </summary>
        public string RevSourceUserName { get; set; }
        /// <summary>
        /// 接收任务时间
        /// </summary>
        public DateTime? RevTime { get; set; }
        /// <summary>
        /// 接收用户查看时间
        /// </summary>
        public DateTime? RevUserViewTime { get; set; }

        /// <summary>
        /// 任务消息提醒ID，多个用，分隔，则消息扩展接口负责解析
        /// </summary>
        public string MsgIds { get; set; }
        /// <summary>
        /// /任务办理时间
        /// </summary>
        public DateTime? TaskManaTime { get; set; }
        /// <summary>
        /// 任务办理方式
        /// </summary>
        public CPFlowEnum.TaskMakeTypeEnum TaskManaType { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
        }

        public static CPFlowInstanceLog InitFromTask(CPFlowInstanceTask task)
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<CPFlowInstanceTask, CPFlowInstanceLog>().ForMember(dest => dest.Id, opt =>
                {
                    opt.Ignore();
                }); ;
            });

            CPFlowInstanceLog log = AutoMapper.Mapper.Map<CPFlowInstanceLog>(task);
         
            if (task.RevUserViewTime.HasValue == false)
                log.RevUserViewTime = DateTime.Now;
            log.TaskManaTime = DateTime.Now;
            //默认是提交
            log.TaskManaType = CPFlowEnum.TaskMakeTypeEnum.Submit;
            return log;
        }
        public  CPFlowInstanceTask ReCreateTask( )
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<CPFlowInstanceLog, CPFlowInstanceTask>().ForMember(dest => dest.Id, opt =>
                {
                    opt.Ignore();
                }); ;
            });

            CPFlowInstanceTask task = AutoMapper.Mapper.Map<CPFlowInstanceTask>(this);
            task.RevTime = DateTime.Now;
            task.RevUserIsView = false;
            task.RevUserViewTime = null;
            return task;
        }
    }

    public class CPFlowInstanceLogUnique : BaseEntity
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        public int UniqueLogId { get { return this.Id; } }
        /// <summary>
        /// 流程实例ID
        /// </summary>
        public int InsId { get; set; }
        /// <summary>
        /// 流程版本ID
        /// </summary>
        public int FlowVerId { get; set; }
        /// <summary>
        /// 流程主ID
        /// </summary>
        public int FlowId { get; set; }

        /// <summary>
        /// 流程模板标题
        /// </summary>
        public string FlowName { get; set; }

        /// <summary>
        /// 流程标题
        /// </summary>
        public string InsTitle { get; set; }
        /// <summary>
        /// 创建用户ID
        /// </summary>
        public int? CreateUserId { get; set; }

        /// <summary>
        /// 创建用户姓名
        /// </summary>
        public string CreateUserName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 当前流程状态
        /// </summary>
        public CPFlowEnum.InsStateEnum? InsState { get; set; }

        /// <summary>
        /// 状态标题
        /// </summary>
        public string InsStateTitle { get; set; }
        /// <summary>
        /// 流程作废、终止、或正常结束时间
        /// </summary>
        public DateTime? InsEndTime { get; set; }

        /// <summary>
        /// 办理过节点id，多个用，分隔
        /// </summary>
        public string ManaPhaseIds { get; set; }

        /// <summary>
        /// 办理过节点名称，多个用，分隔
        /// </summary>
        public string ManaPhaseNames { get; set; }
        /// <summary>
        /// 已经参与办理过的用户ID，如果一个用户多次办理，只存储一条记录。
        /// </summary>
        public int? ManaUserId { get; set; }
        /// <summary>
        /// 主表单ID，暂时只支持int型主键
        /// </summary>
        public int? MainFormPk { get; set; }

        public override void FormatInitValue()
        {
            base.FormatInitValue();
        }
        public static CPFlowInstanceLogUnique InitFromLogAndInstance(CPFlowInstanceLog log,CPFlowInstance instance)
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<CPFlowInstanceLog, CPFlowInstanceLogUnique>().ForMember(dest => dest.Id, opt =>
                {
                    opt.Ignore();
                }); ;
            });

            CPFlowInstanceLogUnique logUnique = AutoMapper.Mapper.Map<CPFlowInstanceLogUnique>(log);
            logUnique.CreateUserId = instance.CreateUserId;
            logUnique.CreateUserName = instance.CreateUserName;
            logUnique.CreateTime = instance.CreateTime;
            logUnique.InsState = instance.InsState;
            logUnique.InsStateTitle = instance.InsStateTitle;
            logUnique.InsEndTime = instance.InsEndTime;
            logUnique.ManaPhaseIds = log.RevUserId.ToString();
            logUnique.ManaPhaseNames = log.RevUserName.ToString();
            logUnique.ManaUserId = log.RevUserId;
            logUnique.MainFormPk = instance.MainFormPk;
            return logUnique;
        }
    }

    public class CPFlowInstanceForm : BaseEntity
    {
        /// <summary>
        /// 表单实例ID
        /// </summary>
        public int InsFormId { get { return this.Id; } }
        /// <summary>
        /// 流程实例ID
        /// </summary>
        public int InsId { get; set; }
        /// <summary>
        /// 流程版本ID
        /// </summary>
        public int FlowVerId { get; set; }
        /// <summary>
        /// 流程主ID
        /// </summary>
        public int FlowId { get; set; }

        /// <summary>
        /// 唯一标识表单的表单Code
        /// </summary>
        public string FormCode { get; set; }
        /// <summary>
        /// 表单主键值
        /// </summary>
        public int FormPK { get; set; }
    }

}
