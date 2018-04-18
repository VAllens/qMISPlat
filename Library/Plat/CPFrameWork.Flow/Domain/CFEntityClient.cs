using System;
using System.Collections.Generic;
using System.Text;

namespace CPFrameWork.Flow.Domain
{
    public class CPFlowPhaseClient 
    {
        /// <summary>
        /// 阶段ID
        /// </summary>
        public int PhaseId { get; set; } 
        

        #region 存储用户真正的办理用户
        public List<CPFlowPhaseTaskRevUser> TaskRevUser { get; set; }
    
        #endregion

    
    }
}
