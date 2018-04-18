using CPFrameWork.Flow.Domain;
using CPFrameWork.Flow.Infrastructure;
using CPFrameWork.Flow.Repository;
using CPFrameWork.Global;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CPFrameWork.Flow
{
   public  class CPFlowTemplate
    {
        #region 获取配置时相关 
        /// <summary>
        /// 获取节点对象服务类
        /// </summary>
        /// <returns></returns>
        public static void StartupInit(IServiceCollection services, IConfigurationRoot Configuration)
        {
            // Add framework services.
            services.AddDbContext<CPFlowDbContext>(options =>//手工高亮
                options.UseSqlServer(Configuration.GetConnectionString("CPCommonIns")));

            services.TryAddTransient<ICPFlowDbContext, CPFlowDbContext>();
            services.TryAddTransient<BaseCPFlowRep, CPFlowRep>();
            services.TryAddTransient<BaseRepository<CPFlowPhase>, CPFlowPhaseRep>();
            services.TryAddTransient<BaseRepository<CPFlowPhaseLink>, CPFlowPhaseLinkRep>();
            services.TryAddTransient<BaseRepository<CPFlowPhaseForm>, CPFlowPhaseFormRep>();
            services.TryAddTransient<BaseRepository<CPFlowPhaseRule>, CPFlowPhaseRuleRep>();
            services.TryAddTransient<BaseRepository<CPFlowPhaseRuleHandle>, CPFlowPhaseRuleHandleRep>();
            services.TryAddTransient<CPFlowTemplate, CPFlowTemplate>();
        }
        public static CPFlowTemplate Instance()
        {
            CPFlowTemplate iObj = CPAppContext.GetService<CPFlowTemplate>();
            return iObj;
        }
        #endregion


        private BaseCPFlowRep _BaseCPFlowRep;
        private BaseRepository<CPFlowPhase> _CPFlowPhaseRep;
        private BaseRepository<CPFlowPhaseLink> _CPFlowPhaseLinkRep;
        private BaseRepository<CPFlowPhaseForm> _CPFlowPhaseFormRep;
        private BaseRepository<CPFlowPhaseRule> _CPFlowPhaseRuleRep;
        private BaseRepository<CPFlowPhaseRuleHandle> _CPFlowPhaseRuleHandleRep;  
        public CPFlowTemplate(
                 BaseCPFlowRep BaseCPFlowRep,
         BaseRepository<CPFlowPhase> CPFlowPhaseRep,
         BaseRepository<CPFlowPhaseLink> CPFlowPhaseLinkRep,
         BaseRepository<CPFlowPhaseForm> CPFlowPhaseFormRep,
         BaseRepository<CPFlowPhaseRule> CPFlowPhaseRuleRep,
         BaseRepository<CPFlowPhaseRuleHandle> CPFlowPhaseRuleHandleRep
            )
        {
            this._BaseCPFlowRep = BaseCPFlowRep;
            this._CPFlowPhaseRep = CPFlowPhaseRep;
            this._CPFlowPhaseLinkRep = CPFlowPhaseLinkRep;
            this._CPFlowPhaseFormRep = CPFlowPhaseFormRep;
            this._CPFlowPhaseRuleRep = CPFlowPhaseRuleRep;
            this._CPFlowPhaseRuleHandleRep = CPFlowPhaseRuleHandleRep;
        }


        #region 获取配置信息

        public List<CPFlow> GetHasStartRightFlow(int curUserId,List<int> userRoleIdCol,List<int> userDepIdCol)
        {
            List<CPFlow> allFlow = this._BaseCPFlowRep.GetAllFlowMaxVer();
            List<CPFlow> returnCol = new List<CPFlow>();
            allFlow.ForEach(t => {
                if(string.IsNullOrEmpty(t.HasRightUserIds) && string.IsNullOrEmpty(t.HasRightRoleIds) && string.IsNullOrEmpty(t.HasRightDepIds))
                {
                    returnCol.Add(t);
                }
                else
                {
                    bool hasRight = false;
                    if(string.IsNullOrEmpty(t.HasRightUserIds)==false)
                    {
                        List<string > tCol = t.HasRightUserIds.Split(',').ToList();
                        if(tCol.Contains(curUserId.ToString()))
                        {
                            hasRight = true;                            
                        }
                    }
                    if(hasRight==false && string.IsNullOrEmpty(t.HasRightRoleIds)==false)
                    {
                        List<string> tCol = t.HasRightRoleIds.Split(',').ToList();
                        tCol.ForEach(tmp => {
                            if(userRoleIdCol.Contains(int.Parse(tmp)))
                            {
                                hasRight = true;
                                return;
                            }
                        });
                    }
                    if (hasRight == false && string.IsNullOrEmpty(t.HasRightDepIds) == false)
                    {
                        List<string> tCol = t.HasRightDepIds.Split(',').ToList();
                        tCol.ForEach(tmp => {
                            if (userDepIdCol.Contains(int.Parse(tmp)))
                            {
                                hasRight = true;
                                return;
                            }
                        });
                    }
                    if (hasRight)
                        returnCol.Add(t);
                }
            });
            return returnCol;
        }

        public List<CPFlow> GetHasMonitorRightFlow(int curUserId, List<int> userRoleIdCol)
        {
            List<CPFlow> allFlow = this._BaseCPFlowRep.Get().ToList();
            List<CPFlow> returnCol = new List<CPFlow>();
            allFlow.ForEach(t => {
                bool hasRight = false;
                if (string.IsNullOrEmpty(t.JGUserIds) == false)
                {
                    List<string> tCol = t.JGUserIds.Split(',').ToList();
                    if (tCol.Contains(curUserId.ToString()))
                    {
                        hasRight = true;
                    }
                }
                if (hasRight == false && string.IsNullOrEmpty(t.JGRoleIds) == false)
                {
                    List<string> tCol = t.JGRoleIds.Split(',').ToList();
                    tCol.ForEach(tmp => {
                        if (userRoleIdCol.Contains(int.Parse(tmp)))
                        {
                            hasRight = true;
                            return;
                        }
                    });
                }
                if (hasRight)
                    returnCol.Add(t);
            });
            return returnCol;

        }
        public CPFlow GetFlowByFlowVerId(int flowVerId,bool isLoadFlowPhaseInfo,bool isLoadPhaseLinkInfo)
        {
            int nCount = 0;
            if (isLoadFlowPhaseInfo)
            {
                nCount++;
            }
            if (isLoadPhaseLinkInfo)
            {
                nCount++;
            }
            Expression<Func<CPFlow, dynamic>>[] eagerLoadingProperties = new Expression<Func<CPFlow, dynamic>>[nCount];
            int nIndex = 0;
            if (isLoadFlowPhaseInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.PhaseCol;
                nIndex++;
            }
            if (isLoadPhaseLinkInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.PhaseLinkCol;
                nIndex++;
            }
            //ISpecification<CPFlow> specification;
            //specification = new ExpressionSpecification<CPFlow>(t => t.TreeCode.Equals(treeCode));
            CPFlow flow = this._BaseCPFlowRep.Get(flowVerId, eagerLoadingProperties);
            return flow;
        }
        #endregion

        #region 从定制器里保存流程配置
        public bool UpdateFlowByDesigner(CPFlowDesigner flowDesign,int flowVerId,string flowName,ref string errorMsg)
        {
            bool b = true;
            #region 先修改流程基本信息配置和节点和路径
            CPFlow flow = this.GetFlowByFlowVerId(flowVerId, true, true);
            if(flow == null)
            {
                b = false;
                errorMsg = "根据FlowVerId[" + flowVerId + "]获取流程配置时，未取到配置信息";
                return b;
            } 
            flow.FlowName = flowName;
            flow.CreateTime = DateTime.Now;
            #region 同步阶段信息
            List<CPFlowPhase> phaseColAdd = new List<CPFlowPhase>();
            List<CPFlowPhase> phaseColUpdate = new List<CPFlowPhase>();
            List<CPFlowPhase> phaseColDel = new List<CPFlowPhase>();
            flow.PhaseCol.ForEach(t => {
                bool bIsDelete = true;
                CPFlowDesignerPhase dPhaseTmp =null;
                foreach (string key in flowDesign.states.Keys)
                {
                    CPFlowDesignerPhase dPhase = flowDesign.states[key];
                    if(dPhase.props.temp1 != null && 
                        dPhase.props.temp1.value!= null
                        && string.IsNullOrEmpty(dPhase.props.temp1.value)==false
                        && int.Parse(dPhase.props.temp1.value).Equals(t.PhaseId))
                    {
                        dPhaseTmp = dPhase;
                        bIsDelete = false;
                        break;
                    }
                }
                //看看是不是删除了
                if(bIsDelete)
                {
                    phaseColDel.Add(t);
                }
                else
                {
                    //修改
                    t.PhaseType = dPhaseTmp.ConvertTypeToPhaseType();
                    t.PhaseName = dPhaseTmp.text.text;
                    t.PositionX = Convert.ToDouble(dPhaseTmp.attr.x);
                    t.PositionY = Convert.ToDouble(dPhaseTmp.attr.y);
                    t.Width = Convert.ToDouble(dPhaseTmp.attr.width);
                    t.Height = Convert.ToDouble(dPhaseTmp.attr.height);
                    
                    phaseColUpdate.Add(t);
                }
            });
            //找出新增加的
            foreach(string key in flowDesign.states.Keys)
            {
              //  string phaseIdTmp = flowDesign.states[key].props.temp1.value;// key.Replace(designerPhasePre, "");
               // if(flow.PhaseCol.Where(t=>t.PhaseId.Equals(int.Parse(phaseIdTmp))).Count()<=0)
               if(flowDesign.states[key].props.temp1 == null || string.IsNullOrEmpty(flowDesign.states[key].props.temp1.value))
                {
                    CPFlowDesignerPhase dPhase = flowDesign.states[key];
                    CPFlowPhase newPhase = new CPFlowPhase();
                    newPhase.FlowVerId = flow.FlowVerId;
                    newPhase.PhaseType = dPhase.ConvertTypeToPhaseType();
                    newPhase.PhaseName = dPhase.text.text;
                    newPhase.PositionX = Convert.ToDouble(dPhase.attr.x);
                    newPhase.PositionY = Convert.ToDouble(dPhase.attr.y);
                    newPhase.Width = Convert.ToDouble(dPhase.attr.width);
                    newPhase.Height = Convert.ToDouble(dPhase.attr.height);
                    phaseColAdd.Add(newPhase);
                }
            }
            phaseColDel.ForEach(t => { flow.PhaseCol.Remove(t); });
            phaseColAdd.ForEach(t => { flow.PhaseCol.Add(t); });
            try
            {
                //先删除下阶段相关信息
                List<int> phaseIdCol = new List<int>();
                phaseColDel.ForEach(t => { phaseIdCol.Add(t.PhaseId); });
                this._BaseCPFlowRep.DeletePhase(phaseIdCol);
                flow.CreateTime = DateTime.Now;//为确保每次都调用保存数据库
                this._BaseCPFlowRep.Update(flow);
              

            }catch(Exception ex)
            {
                b = false;
                errorMsg = "保存阶段信息时出错，详细信息为:" + ex.Message;
                return b;
            }
            #endregion

            #region 同步路径信息
            List<CPFlowPhaseLink> linkColAdd = new List<CPFlowPhaseLink>();
            List<CPFlowPhaseLink> linkColUpdate = new List<CPFlowPhaseLink>();
            List<CPFlowPhaseLink> linkColDel = new List<CPFlowPhaseLink>();
            //修改或删除
            flow.PhaseLinkCol.ForEach(t => {
                bool bExists = false;
                CPFlowDesignerPhaseLink dLink = null;
                foreach (string key in flowDesign.paths.Keys)
                {
                    CPFlowDesignerPhaseLink dLinkTmp = flowDesign.paths[key];
                    //if(dLink.from.Equals(designerPhasePre + t.StartPhaseId.ToString(),StringComparison.CurrentCultureIgnoreCase)
                    //&& dLink.to.Equals(designerPhasePre + t.EndPhaseId.ToString(), StringComparison.CurrentCultureIgnoreCase)
                    //)
                    if(dLinkTmp.props.temp1!=null && dLinkTmp.props.temp1.value != null 
                        && string.IsNullOrEmpty(dLinkTmp.props.temp1.value)==false
                        && int.Parse(dLinkTmp.props.temp1.value).Equals(t.LinkId)
                    )
                    {
                        bExists = true;
                        dLink = dLinkTmp;
                        break;
                    }
                }
                if(bExists==false)
                {
                    linkColDel.Add(t);
                }
                else
                {
                    t.TipX = Convert.ToDouble(dLink.text.textPos.x);
                    t.TipY = Convert.ToDouble(dLink.text.textPos.y);
                    t.LinkDots = "";
                    dLink.dots.ForEach(dot => {
                        if(string.IsNullOrEmpty(t.LinkDots))
                        {
                            t.LinkDots = dot.x + "," + dot.y;
                        }
                        else
                        {
                            t.LinkDots += ";" +  dot.x + "," + dot.y;
                        }
                    });
                }
            });
            //新加的
            foreach(string key in flowDesign.paths.Keys)
            {
                CPFlowDesignerPhaseLink dLink = flowDesign.paths[key];
                CPFlowDesignerPhase startPhaseDesigner = flowDesign.states[dLink.from];
                CPFlowDesignerPhase endPhaseDesigner = flowDesign.states[dLink.to];
                int startPhaseIdDesigner = 0;
                int endPhaseIdDesigner = 0;
                //看下这个节点是不是新加的
                if(startPhaseDesigner.props.temp1!=null && startPhaseDesigner.props.temp1.value !=null
                    && string.IsNullOrEmpty(startPhaseDesigner.props.temp1.value)==false
                    )
                {
                    startPhaseIdDesigner = int.Parse(startPhaseDesigner.props.temp1.value);
                }
                else
                {
                    //新加的节点
                    List<CPFlowPhase> tColTmp = flow.PhaseCol.Where(t => t.PositionX.Equals(Convert.ToDouble(startPhaseDesigner.attr.x))
                    && t.PositionY.Equals(Convert.ToDouble(startPhaseDesigner.attr.y))).ToList();
                    startPhaseIdDesigner = tColTmp[0].PhaseId;
                }
                if (endPhaseDesigner.props.temp1!=null && endPhaseDesigner.props.temp1.value != null
                     && string.IsNullOrEmpty(endPhaseDesigner.props.temp1.value) == false
                     )
                {
                    endPhaseIdDesigner = int.Parse(endPhaseDesigner.props.temp1.value);
                }
                else
                {
                    //说明开始节点是新加的，那就要根据定制器中新节点的X和Y去找对应数据库中的ID 
                    List<CPFlowPhase> tColTmp = flow.PhaseCol.Where(t => t.PositionX.Equals(Convert.ToDouble(endPhaseDesigner.attr.x))
                    && t.PositionY.Equals(Convert.ToDouble(endPhaseDesigner.attr.y))).ToList();
                    endPhaseIdDesigner = tColTmp[0].PhaseId;
                }
                if(flow.PhaseLinkCol.Where(t=>t.StartPhaseId.Equals(startPhaseIdDesigner) && t.EndPhaseId.Equals(endPhaseIdDesigner)).Count()<=0)
                {
                    CPFlowPhaseLink linkNew = new CPFlowPhaseLink();
                    linkNew.FlowVerId = flow.FlowVerId;
                    linkNew.StartPhaseId = startPhaseIdDesigner;
                    linkNew.EndPhaseId = endPhaseIdDesigner;
                    linkNew.TipX = Convert.ToDouble(dLink.text.textPos.x);
                    linkNew.TipY = Convert.ToDouble(dLink.text.textPos.y);
                    linkNew.LinkDots = "";
                    dLink.dots.ForEach(dot => {
                        if (string.IsNullOrEmpty(linkNew.LinkDots))
                        {
                            linkNew.LinkDots = dot.x + "," + dot.y;
                        }
                        else
                        {
                            linkNew.LinkDots += ";" + dot.x + "," + dot.y;
                        }
                    });
                    if(dLink.props == null || dLink.props.PathType == null || 
                        string.IsNullOrEmpty(dLink.props.PathType.value))
                    {
                        linkNew.LinkType = CPFlowEnum.LinkTypeEnum.Submit;
                    }
                    else
                    {
                        if (dLink.props.PathType.value.Trim().Equals("1"))
                            linkNew.LinkType = CPFlowEnum.LinkTypeEnum.Submit;
                        else if (dLink.props.PathType.value.Trim().Equals("2"))
                            linkNew.LinkType = CPFlowEnum.LinkTypeEnum.Fallback;
                        else
                            linkNew.LinkType = CPFlowEnum.LinkTypeEnum.Submit;
                    }
                    linkColAdd.Add(linkNew);
                }
            }

            linkColDel.ForEach(t => { flow.PhaseLinkCol.Remove(t); });
            linkColAdd.ForEach(t => { flow.PhaseLinkCol.Add(t); });
            try
            {
                flow.CreateTime = DateTime.Now;//为确保每次都调用保存数据库
                this._BaseCPFlowRep.Update(flow);
            }
            catch (Exception ex)
            {
                b = false;
                errorMsg = "保存阶段路径信息时出错，详细信息为:" + ex.Message;
                return b;
            }
            #endregion

            #endregion
            return b;
        }
        #endregion

        #region 修改流程相关配置
        public bool UpdateFlow(CPFlow flow)
        {
            this._BaseCPFlowRep.Update(flow);
            return true;
        }
        public bool CreateFlowNewVer(int FlowVerId)
        {
            DataSet ds = this._BaseCPFlowRep.GetConfigByFlowVerId(FlowVerId);
            bool b = this._BaseCPFlowRep.SyncConfigFromDataSet(ds, int.Parse(ds.Tables["Flow_Template"].Rows[0]["FlowClassId"].ToString()),false);
            return b;
        }
        /// <summary>
        /// 获取流程最大版本定制信息
        /// </summary>
        /// <param name="flowId"></param>
        /// <returns></returns>
        public   CPFlow GetFlowMaxVer(int flowId)
        {
            return this._BaseCPFlowRep.GetFlowMaxVer(flowId);
        }
        /// <summary>
        /// 获取流程最新两个版本的配置信息
        /// </summary>
        /// <param name="flowId"></param>
        /// <returns></returns>
        public List<CPFlow> GetFlowMaxTwoVer(int flowId)
        {
            return this._BaseCPFlowRep.GetFlowMaxTwoVer(flowId);
        }
        #endregion

        #region 根据配置信息获取首节点
        /// <summary>
        /// 根据配置信息获取流程首节点
        /// </summary>
        /// <param name="allPhaseCol">流程所有节点</param>
        /// <param name="allPhaseLinkCol">流程所有的路径</param>
        /// <returns>返回首节点，未找到返回空</returns>
        public CPFlowPhase GetFirstPhase(List<CPFlowPhase> allPhaseCol,List<CPFlowPhaseLink> allPhaseLinkCol)
        {
            if (allPhaseCol == null || allPhaseCol.Count <= 0)
                return null;
            CPFlowPhase phase = null;
            allPhaseCol.ForEach(t => {
                if(allPhaseLinkCol.Where(c=>c.EndPhaseId.Equals(t.PhaseId)).Count()<=0)
                {
                    phase = t;
                    return;
                }
            });
            return phase;
        } 
        /// <summary>
        /// 根据配置信息获取流程首节点
        /// </summary>
        /// <param name="flowVerId">流程版本ID</param>
        /// <returns>返回首节点，未找到返回空</returns>
        public CPFlowPhase GetFirstPhase(int flowVerId)
        {
            CPFlow flow = this.GetFlowByFlowVerId(flowVerId, true, true);
            if (flow.PhaseCol == null || flow.PhaseLinkCol.Count <= 0)
                return null;
            CPFlowPhase phase = null;
            flow.PhaseCol.ForEach(t => {
                if (flow.PhaseLinkColSubmit.Where(c => c.EndPhaseId.Equals(t.PhaseId)).Count() <= 0)
                {
                    phase = t;
                    return;
                }
            });
            return phase;
        }
        public List<CPFlowPhase> GetFlowPhase(int flowVerId,bool isLoadFormInfo,bool isLoadRuleInfo)
        {
            int nCount = 0;
            if (isLoadFormInfo)
            {
                nCount++;
            }
            if (isLoadRuleInfo)
            {
                nCount++;
            }

            Expression<Func<CPFlowPhase, dynamic>>[] eagerLoadingProperties = new Expression<Func<CPFlowPhase, dynamic>>[nCount];
            int nIndex = 0;
            if (isLoadFormInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.FormCol;
                nIndex++;
            }
            if (isLoadRuleInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.RuleCol;
                nIndex++;
            }
            ISpecification<CPFlowPhase> specification;
            specification = new ExpressionSpecification<CPFlowPhase>(t => t.FlowVerId.Equals(flowVerId));
            return this._CPFlowPhaseRep.GetByCondition(specification, eagerLoadingProperties).ToList();
        }
        public CPFlowPhase GetFlowPhaseByPhaseId(int phaseId, bool isLoadFormInfo, bool isLoadRuleInfo)
        {
            int nCount = 0;
            if (isLoadFormInfo)
            {
                nCount++;
            }
            if (isLoadRuleInfo)
            {
                nCount++;
            }

            Expression<Func<CPFlowPhase, dynamic>>[] eagerLoadingProperties = new Expression<Func<CPFlowPhase, dynamic>>[nCount];
            int nIndex = 0;
            if (isLoadFormInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.FormCol;
                nIndex++;
            }
            if (isLoadRuleInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.RuleCol;
                nIndex++;
            }
            ISpecification<CPFlowPhase> specification;
            specification = new ExpressionSpecification<CPFlowPhase>(t => t.Id.Equals(phaseId));
            List<CPFlowPhase> pCol = this._CPFlowPhaseRep.GetByCondition(specification,eagerLoadingProperties).ToList();
            if (pCol.Count > 0)
                return pCol[0];
            else
                return null;
        }

        public List<CPFlowPhase> GetFlowPhaseByPhaseId(List<int> phaseIdCol, bool isLoadFormInfo, bool isLoadRuleInfo)
        {
            int nCount = 0;
            if (isLoadFormInfo)
            {
                nCount++;
            }
            if (isLoadRuleInfo)
            {
                nCount++;
            }

            Expression<Func<CPFlowPhase, dynamic>>[] eagerLoadingProperties = new Expression<Func<CPFlowPhase, dynamic>>[nCount];
            int nIndex = 0;
            if (isLoadFormInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.FormCol;
                nIndex++;
            }
            if (isLoadRuleInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.RuleCol;
                nIndex++;
            }
            ISpecification<CPFlowPhase> specification;
            specification = new ExpressionSpecification<CPFlowPhase>(t => phaseIdCol.Contains(t.Id));
            List<CPFlowPhase> pCol = this._CPFlowPhaseRep.GetByCondition(specification, eagerLoadingProperties).ToList();
            return pCol;
        }
        public List<CPFlowPhaseLink> GetFlowPhaseLink(int flowVerId)
        {
            ISpecification<CPFlowPhaseLink> specification;
            specification = new ExpressionSpecification<CPFlowPhaseLink>(t => t.FlowVerId.Equals(flowVerId));
            return this._CPFlowPhaseLinkRep.GetByCondition(specification).ToList();
        }
        #endregion

        #region 获取节点表单配置
        public List<CPFlowPhaseForm> GetPhaseAllForm(int flowVerId, bool isLoadPhaseInfo)
        {
            int nCount = 0;
            if (isLoadPhaseInfo)
            {
                nCount++;
            }

            Expression<Func<CPFlowPhaseForm, dynamic>>[] eagerLoadingProperties = new Expression<Func<CPFlowPhaseForm, dynamic>>[nCount];
            int nIndex = 0;
            if (isLoadPhaseInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.FlowPhase;
                nIndex++;
            }

            ISpecification<CPFlowPhaseForm> specification;
            specification = new ExpressionSpecification<CPFlowPhaseForm>(t => t.FlowVerId.Equals(flowVerId));
            return this._CPFlowPhaseFormRep.GetByCondition(specification, eagerLoadingProperties).ToList();
        }
        public List<CPFlowPhaseForm> GetPhaseForm(int phaseId,bool isLoadPhaseInfo)
        {
            int nCount = 0;
            if (isLoadPhaseInfo)
            {
                nCount++;
            }
          
            Expression<Func<CPFlowPhaseForm, dynamic>>[] eagerLoadingProperties = new Expression<Func<CPFlowPhaseForm, dynamic>>[nCount];
            int nIndex = 0;
            if (isLoadPhaseInfo)
            { 
                    eagerLoadingProperties[nIndex] = t => t.FlowPhase;
                nIndex++;
            }
           
            ISpecification<CPFlowPhaseForm> specification;
            specification = new ExpressionSpecification<CPFlowPhaseForm>(t => t.PhaseId.Equals(phaseId));
            return this._CPFlowPhaseFormRep.GetByCondition(specification, eagerLoadingProperties).ToList();
        }
        #endregion
    }
}
