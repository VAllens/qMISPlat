using System;
using System.Collections.Generic;
using CPFrameWork.Utility.DbOper; 
using CPFrameWork.Global;

namespace CPFrameWork.Organ.Domain
{
    #region CODep
    public class CODep:BaseOrderEntity
    {
         public CODep()
        {
            
        }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepName { get; set; }
        /// <summary>
        /// 部门简称
        /// </summary>
        public string DepShortName { get; set; }
        /// <summary>
        /// 部门Code，客户的Code
        /// </summary>
        public string DepShortCode { get; set; }
        /// <summary>
        /// 父部门ID，根部门的父部门ID为-1
        /// </summary>
        public Nullable<int> ParentId { get; set; }
        /// <summary>
        /// 部门类型         单位：1 部门：2
        /// </summary>
        public Nullable<COEnum.DepTypeEnum> DepType { get; set; }
        /// <summary>
        /// 部门状态
        /// </summary>
        public Nullable<COEnum.DepStateEnum> DepState { get; set; }
        /// <summary>
        /// 部门领导用户ID
        /// </summary>
        public Nullable<int> DepMainLeaderId { get; set; }
        /// <summary>
        /// 部门领导名称
        /// </summary>
        public string DepMainLeaderName { get; set; }
        /// <summary>
        /// 部门副领导用户ID，多个用，分隔
        /// </summary>
        public string DepViceLeaderIds { get; set; }
        /// <summary>
        /// 部门领导名称，多个用，分隔
        /// </summary>
        public string DepViceLeaderNames { get; set; }
        /// <summary>
        /// 部门主管领导用户ID
        /// </summary>
        public Nullable<int> DepSupervisorId { get; set; }
        /// <summary>
        /// 部门主管领导用户姓名
        /// </summary>
        public string DepSupervisorName { get; set; }
        private List<COUser> _userCol = null;
        /// <summary>
        /// 部门下用户
        /// </summary>
        public List<COUser> UserCol
        {
            get
            {
                if(this._userCol == null)
                {
                    this._userCol = new List<COUser>();
                    if(this.DepUserCol != null)
                    {
                        this.DepUserCol.ForEach(t => {
                            this._userCol.Add(t.User);
                        });
                    }
                }
                return this._userCol;
            }
            set
            {
                this._userCol = value;
            }
        }
        internal List<CODepUserRelate> DepUserCol { get; set; }

        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.ParentId.HasValue == false)
                this.ParentId = CPAppContext.RootParentId;
            if (this.DepType.HasValue == false)
                this.DepType = COEnum.DepTypeEnum.Dep;
            if (this.DepState.HasValue == false)
                this.DepState = COEnum.DepStateEnum.Normal;

        }
    }





    #endregion

    #region CODepUserRelate
    public class CODepUserRelate : BaseEntity
    {
        public CODepUserRelate()
        {

        }
        public int DepId { get; set; }

        public CODep Dep { get; set; }
        public int UserId { get; set; }
        public COUser User { get; set; }
        public int ShowOrder { get; set; }

        public override void FormatInitValue()
        {
            base.FormatInitValue(); 

        }
    }

    



    #endregion
    #region COUser
    public class COUser:BaseEntity
    {
        /// <summary>
        /// 用户登录名
        /// </summary>
        public string LoginName { get; set; }
        /// <summary>
        /// 用户密码 MD5
        /// </summary>
        public string UserPwd { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 是否可登录
        /// </summary>
        public Nullable<bool> IsCanLogin { get; set; }
        /// <summary>
        /// 用户手机号，多个用，分隔
        /// </summary>
        public string MobilePhone { get; set; }
        /// <summary>
        /// 办公室电话s
        /// </summary>
        public string OfficePhone { get; set; }
        /// <summary>
        /// 是否是外单位用户
        /// </summary>
        public Nullable<bool> IsOutUser { get; set; }
        /// <summary>
        /// 用户微信ID
        /// </summary>
        public string UserWXId { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public Nullable<COEnum.UserSexEnum> UserSex { get; set; }
        /// <summary>
        /// 用户描述
        /// </summary>
        public string UserDescrip { get; set; }

        public string UserPhotoName { get; set; }
        public string UserPhotoPath { get; set; }
        public string UserSignName { get; set; }
        public string UserSignPath { get; set; }

        private List<CODep> _depCol = null;
        /// <summary>
        /// 所属部门集合
        /// </summary>
        public List<CODep> DepCol {
            get
            {
                if (this._depCol == null)
                {
                    this._depCol = new List<CODep>();
                    if (this.DepUserCol != null)
                    {
                        this.DepUserCol.ForEach(t => {
                            this._depCol.Add(t.Dep);
                        });
                    }
                }
                return this._depCol;
            }
            set
            {
                this._depCol = value;
            }
        }
        internal List<CODepUserRelate> DepUserCol { get; set; }
        private List<CORole> _roleCol = null;
        /// <summary>
        /// 所属角色
        /// </summary>
        public List<CORole> RoleCol { get {
                if (this._roleCol == null)
                {
                    this._roleCol = new List<CORole>();
                    if (this.RoleUserCol != null)
                    {
                        this.RoleUserCol.ForEach(t => {
                            this._roleCol.Add(t.Role);
                        });
                    }
                }
                return this._roleCol;
            } set {
                this._roleCol = value;
            } }
        internal List<CORoleUserRelate> RoleUserCol { get; set; }

        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.IsCanLogin.HasValue == false)
                this.IsCanLogin = true;
            if (this.IsOutUser.HasValue == false)
                this.IsOutUser = false;
            if (this.UserSex.HasValue == false)
                this.UserSex = COEnum.UserSexEnum.Man;
            //if (this.ShowOrder.HasValue == false)
            //    this.ShowOrder = 10;
        }
    }

    

    #endregion


    #region CORole
    public class CORole : BaseEntity
    { 
        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }
        /// <summary>
        /// 动态角色 SQL
        /// </summary>
        public string RoleUserSql { get; set; }
        /// <summary>
        /// 角色描述
        /// </summary>
        public string RoleRemark { get; set; }

        private List<COUser> _userCol = null;
        /// <summary>
        /// 包含用户
        /// </summary>
        public List<COUser> UserCol
        {
            get
            {
                if (this._userCol == null)
                {
                    this._userCol = new List<COUser>();
                    if (this.RoleUserCol != null)
                    {
                        this.RoleUserCol.ForEach(t =>
                        {
                            this._userCol.Add(t.User);
                        });
                    }
                }
                return this._userCol;
            }
            set
            {
                this._userCol = value;
            }
        }

        public List<CORoleUserRelate> RoleUserCol { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue(); 
        }
    }

    public class CORoleUserRelate : BaseEntity
    {
         
        public int RoleId { get; set; }

        public CORole Role { get; set; }

        public int UserId { get; set; }

        public COUser User { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
        }
    }

    #endregion

    #region COUserIdentity
    public class COUserIdentity : BaseEntity
    {
        
        public System.Guid UserKey { get; set; }
        public int UserId { get; set; }
        public Nullable<System.DateTime> LoginTime { get; set; }
        public Nullable<CPEnum.DeviceTypeEnum> LoginDevice { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.LoginTime.HasValue == false)
                this.LoginTime = DateTime.Now;
            if (this.LoginDevice.HasValue == false)
                this.LoginDevice = CPEnum.DeviceTypeEnum.PCBrowser;
        }
    }

    


    #endregion
}
