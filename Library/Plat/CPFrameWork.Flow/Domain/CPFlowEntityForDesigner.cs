using System;
using System.Collections.Generic;
using System.Text;

namespace CPFrameWork.Flow.Domain
{
    public class CPFlowDesigner
    {
        public Dictionary<string, CPFlowDesignerPhase> states { get; set; }
        public Dictionary<string, CPFlowDesignerPhaseLink> paths { get; set; }
    }
    #region 阶段
    public class CPFlowDesignerPhaseText
    {
        public string text { get; set; }
    }
    public class CPFlowDesignerPhaseAttr
    {
        public double x { get; set; }
        public double y { get; set; }
        public double width { get; set; }
        public double height { get; set; }
        
    }
    public class CPFlowDesignerPhaseprops
    {
        public CPFlowDesignerPhasepropstemp1 temp1 { get; set; }
      
    }
    public class CPFlowDesignerPhasepropstemp1
    {
        public string value { get; set; }
    }
    public  class CPFlowDesignerPhase
    {
        public CPFlowDesignerPhase()
        {
            this.IsCurActive = false;
        }
        public string type { get; set; }
        public static string ConvertToDesignerType(CPFlowEnum.PhaseTypeEnum phaseType)
        {
            string stype = "";
            if(phaseType == CPFlowEnum.PhaseTypeEnum.Start)
            {
                stype = "start";
            }
            else if (phaseType == CPFlowEnum.PhaseTypeEnum.Normal)
            {
                stype = "task";

            }
            else if (phaseType == CPFlowEnum.PhaseTypeEnum.Circulation)
            {
                stype = "state";
            }
            else if (phaseType == CPFlowEnum.PhaseTypeEnum.Meet)
            {
                stype = "fork";
            }
            else if (phaseType == CPFlowEnum.PhaseTypeEnum.End)
            {
                stype = "end";
            }
            else
            {
                stype = "task";
            }
            return stype;
        }
        public CPFlowEnum.PhaseTypeEnum ConvertTypeToPhaseType()
        {
            if (type.Equals("start"))
                return CPFlowEnum.PhaseTypeEnum.Start;
            else if (type.Equals("task"))
            {
                return CPFlowEnum.PhaseTypeEnum.Normal;
            }
            else if (type.Equals("state"))
            {
                return CPFlowEnum.PhaseTypeEnum.Circulation;
            }
            else if (type.Equals("fork"))
            {
                return CPFlowEnum.PhaseTypeEnum.Meet;
            }
            else if (type.Equals("end"))
            {
                return CPFlowEnum.PhaseTypeEnum.End;
            }
            else
            {
                return CPFlowEnum.PhaseTypeEnum.Normal;
            }

        }
        public CPFlowDesignerPhaseText text { get; set; }
        public CPFlowDesignerPhaseAttr attr { get; set; }
        public CPFlowDesignerPhaseprops props { get; set; }
       /// <summary>
       /// 流程实例是否处于当前节点，主要为流程监控用
       /// </summary>
        public bool IsCurActive { get; set; }
       
    }
    #endregion

    #region 路由
    public class CPFlowDesignerPhaseLinkText
    {
        public string text { get; set; }
        public CPFlowDesignerPhaseLinkTextPos textPos{get;set;}

    }
    public class CPFlowDesignerPhaseLinkTextPos
    {
        public double x { get; set; }
        public double y { get; set; }
    }
    public class CPFlowDesignerPhaseLinkDot
    {
        public double x { get; set; }
        public double y { get; set; }
    }
    public class CPFlowDesignerPhaseLinkprops
    {
        public CPFlowDesignerPhasepropstemp1 temp1 { get; set; }
        public CPFlowDesignerPhasepropstemp1 PathType { get; set; }

    }
    public class CPFlowDesignerPhaseLink
    {
        public CPFlowDesignerPhaseLink()
        {
            this.IsOverOrActive = false;
        }
        public string from { get; set; }
        public string to { get; set; }
        public List<CPFlowDesignerPhaseLinkDot> dots { get; set; }
        public CPFlowDesignerPhaseLinkText text { get; set; }
        public CPFlowDesignerPhaseLinkprops props
        {
            get; set;
        }
        /// <summary>
        /// 路径是否经过，主要为流程监控用
        /// </summary>
        public bool IsOverOrActive { get; set; }

    }
    #endregion
}
