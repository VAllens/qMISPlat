using CPFrameWork.Global;
using CPFrameWork.UIInterface.Form.Controls;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CPFrameWork.UIInterface.Form
{
    #region 表单保存后，自动同步字段信息，创建视图和应用场景 
    public class CPFormAfterAdd : ICPFormAfterSave
    {
        public void AfterSave(ICPFormAfterSaveEventArgs e)
        {
            if(e.IsAdd)
            {
                int formId = Convert.ToInt32(e.GetFieldValue("Form_Main", "FormId",0));
                int curUserId = Convert.ToInt32(CPExpressionHelper.Instance.RunCompile("${CPUser.UserId()}"));
                bool b = CPFormTemplate.Instance(curUserId).SynFieldFromDb(formId);
                if(b)
                {
                    b= CPFormTemplate.Instance(curUserId).InitFormDefaultView(formId);
                }
                if(b)
                {
                    b = CPFormTemplate.Instance(curUserId).AddDefaultScene(formId);
                }
            }
        }
    }
    #endregion
    public   class CPFormTemplate
    {
        #region 获取实例 

        /// <summary>
        /// 获取节点对象服务类
        /// </summary>
        /// <returns></returns>
        public static void StartupInit(IServiceCollection services, IConfigurationRoot Configuration)
        {
            // Add framework services.
        services.AddDbContext<CPCommonDbContext>(options =>//手工高亮
                options.UseSqlServer(Configuration.GetConnectionString("CPCommonIns")));

            services.TryAddTransient<ICPCommonDbContext, CPCommonDbContext>();
            services.TryAddTransient<BaseCPFormRep, CPFormRep>();

            services.TryAddTransient<IRepository<CPForm>, CPFormRep>();
            services.TryAddTransient<IRepository<CPFormChildTable>, CPFormChildTableRep>();
            services.TryAddTransient<IRepository<CPFormField>, CPFormFieldRep>();
            services.TryAddTransient<IRepository<CPFormView>, CPFormViewRep>();

            services.TryAddTransient<IRepository<CPFormViewField>, CPFormViewFieldRep>();
            services.TryAddTransient<IRepository<CPFormUseScene>, CPFormUseSceneRep>();
            services.TryAddTransient<IRepository<CPFormUseSceneFunc>, CPFormUseSceneFuncRep>();
            services.TryAddTransient<IRepository<CPFormGroup>, CPFormGroupRep>();
            services.TryAddTransient<IRepository<CPFormFieldRight>, CPFormFieldRightRep>();
            services.TryAddTransient<IRepository<CPFormFieldInit>, CPFormFieldInitRep>();
            services.TryAddTransient<IRepository<CPFormFieldRule>, CPFormFieldRuleRep>();

            services.TryAddTransient<CPFormTemplate, CPFormTemplate>();
        }
        public static CPFormTemplate Instance(int curUserId)
        {
            CPFormTemplate iObj = CPAppContext.GetService<CPFormTemplate>();
            iObj.CurUserId = curUserId;
            return iObj;
        }

        　
        #endregion
        public int CurUserId { get; set; }


        private BaseCPFormRep _CPFormRep;
        private IRepository<CPFormChildTable> _CPFormChildTableRep;
        private IRepository<CPFormField> _CPFormFieldRep;
        private IRepository<CPFormView> _CPFormViewRep;
        private IRepository<CPFormViewField> _CPFormViewFieldRep;
        private IRepository<CPFormUseScene> _CPFormUseSceneRep;
        private IRepository<CPFormUseSceneFunc> _CPFormUseSceneFuncRep;
        private IRepository<CPFormGroup> _CPFormGroupRep;
        private IRepository<CPFormFieldRight> _CPFormFieldRightRep;
        private IRepository<CPFormFieldInit> _CPFormFieldInitRep;
        private IRepository<CPFormFieldRule> _CPFormFieldRuleRep;
        public CPFormTemplate(
            BaseCPFormRep CPFormRep,
            IRepository<CPFormChildTable> CPFormChildTableRep,
            IRepository<CPFormField> CPFormFieldRep,
            IRepository<CPFormView> CPFormViewRep,
            IRepository<CPFormViewField> CPFormViewFieldRep,
            IRepository<CPFormUseScene> CPFormUseSceneRep,
            IRepository<CPFormUseSceneFunc> CPFormUseSceneFuncRep,
            IRepository<CPFormGroup> CPFormGroupRep,
            IRepository<CPFormFieldRight> CPFormFieldRightRep,
            IRepository<CPFormFieldInit> CPFormFieldInitRep,
            IRepository<CPFormFieldRule> CPFormFieldRuleRep
            )
        {
            this._CPFormRep = CPFormRep;
            this._CPFormChildTableRep = CPFormChildTableRep;
            this._CPFormFieldRep = CPFormFieldRep;
            this._CPFormViewRep = CPFormViewRep;
            this._CPFormViewFieldRep = CPFormViewFieldRep;
            this._CPFormUseSceneRep = CPFormUseSceneRep;
            this._CPFormUseSceneFuncRep = CPFormUseSceneFuncRep;
            this._CPFormGroupRep = CPFormGroupRep;
            this._CPFormFieldRightRep = CPFormFieldRightRep;
            this._CPFormFieldInitRep = CPFormFieldInitRep;
            this._CPFormFieldRuleRep = CPFormFieldRuleRep;
        }
        #region 主表单相关
        /// <summary>
        /// 获取表单配置信息
        /// </summary>
        /// <param name="formCode">表单Code</param>
        /// <param name="isLoadChildTableInfo">是否获取子表信息</param>
        /// <param name="isLoadFieldInfo">是否获取字段信息</param>
        /// <param name="isLoadViewInfo">是否获取视图信息</param>
        /// <param name="isLoadUseSceneInfo">是否获取使用场景信息</param>
        /// <param name="isLoadGroupInfo">是否获取权限组或初始化组信息</param>
        /// <param name="isLoadFieldRuleInfo">是否获取规则信息</param>
        /// <returns></returns>
        public CPForm GetForm(string formCode,bool isLoadChildTableInfo,bool isLoadFieldInfo,bool isLoadViewInfo,bool isLoadUseSceneInfo,bool isLoadGroupInfo,bool isLoadFieldRuleInfo)
        {
            int nCount = 0;
            if (isLoadChildTableInfo)
            { 
                nCount++;
            }
            if (isLoadFieldInfo)
            { 
                nCount++;
            }
            if (isLoadViewInfo)
            { 
                nCount++;
            }
            if (isLoadUseSceneInfo)
            { 
                nCount++;
            }
            if (isLoadGroupInfo)
            { 
                nCount++;
            }
            if (isLoadFieldRuleInfo) {
                nCount++;
            }
            Expression<Func<CPForm, dynamic>>[] eagerLoadingProperties = new Expression<Func<CPForm, dynamic>>[nCount]  ;
            int nIndex = 0;
            if(isLoadChildTableInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.ChildTableCol;
                nIndex++;
            }
            if(isLoadFieldInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.FieldCol;
                nIndex++;
            }
            if (isLoadViewInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.ViewCol;
                nIndex++;
            }
            if (isLoadUseSceneInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.UseSceneCol;
                nIndex++;
            }
            if (isLoadGroupInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.GroupCol;
                nIndex++;
            }
            if (isLoadFieldRuleInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.FieldRuleCol;
                nIndex++;
            }
            ISpecification<CPForm> specification;
            specification = new ExpressionSpecification<CPForm>(t => t.FormCode.Equals(formCode));
            IList<CPForm> col = this._CPFormRep.GetByCondition(specification, eagerLoadingProperties);
            if (col.Count <= 0)
                return null;
            else
                return col[0];
        }
        public CPForm GetForm(int formId, bool isLoadChildTableInfo, bool isLoadFieldInfo, bool isLoadViewInfo, bool isLoadUseSceneInfo, bool isLoadGroupInfo, bool isLoadFieldRuleInfo)
        {
            int nCount = 0;
            if (isLoadChildTableInfo)
            {
                nCount++;
            }
            if (isLoadFieldInfo)
            {
                nCount++;
            }
            if (isLoadViewInfo)
            {
                nCount++;
            }
            if (isLoadUseSceneInfo)
            {
                nCount++;
            }
            if (isLoadGroupInfo)
            {
                nCount++;
            }
            if (isLoadFieldRuleInfo)
            {
                nCount++;
            }
            Expression<Func<CPForm, dynamic>>[] eagerLoadingProperties = new Expression<Func<CPForm, dynamic>>[nCount];
            int nIndex = 0;
            if (isLoadChildTableInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.ChildTableCol;
                nIndex++;
            }
            if (isLoadFieldInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.FieldCol;
                nIndex++;
            }
            if (isLoadViewInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.ViewCol;
                nIndex++;
            }
            if (isLoadUseSceneInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.UseSceneCol;
                nIndex++;
            }
            if (isLoadGroupInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.GroupCol;
                nIndex++;
            }
            if (isLoadFieldRuleInfo)
            {
                eagerLoadingProperties[nIndex] = t => t.FieldRuleCol;
                nIndex++;
            }
            ISpecification<CPForm> specification;
            specification = new ExpressionSpecification<CPForm>(t => t.Id.Equals(formId));
            IList<CPForm> col = this._CPFormRep.GetByCondition(specification, eagerLoadingProperties);
            if (col.Count <= 0)
                return null;
            else
                return col[0];
        }
        public CPFormView GetFormViewByViewId(int  viewId, bool isLoadViewFieldInfo)
        {
            Expression<Func<CPFormView, dynamic>>[] eagerLoadingProperties = null;
            if (isLoadViewFieldInfo)
            {
                eagerLoadingProperties = new Expression<Func<CPFormView, dynamic>>[] { t => t.ViewFieldCol };
            }
            ISpecification<CPFormView> specification;
            specification = new ExpressionSpecification<CPFormView>(t => t.Id.Equals(viewId));
            IList<CPFormView> col = this._CPFormViewRep.GetByCondition(specification, eagerLoadingProperties);
            if (col.Count <= 0)
                return null;
            else
                return col[0];
        }
        public CPFormView GetFormView(string viewCode,bool isLoadViewFieldInfo)
        {
            Expression<Func<CPFormView, dynamic>>[] eagerLoadingProperties = null;
            if(isLoadViewFieldInfo)
            {
                eagerLoadingProperties =new Expression<Func<CPFormView, dynamic>>[] {t=>t.ViewFieldCol };
            } 
            ISpecification<CPFormView> specification;
            specification = new ExpressionSpecification<CPFormView>(t => t.ViewCode.Equals(viewCode));
            IList<CPFormView> col = this._CPFormViewRep.GetByCondition(specification, eagerLoadingProperties);
            if (col.Count <= 0)
                return null;
            else
                return col[0];
        }
        public List<CPFormView> GetFormView( int formId,bool isLoadViewFieldInfo)
        {
            Expression<Func<CPFormView, dynamic>>[] eagerLoadingProperties = null;
            if (isLoadViewFieldInfo)
            {
                eagerLoadingProperties = new Expression<Func<CPFormView, dynamic>>[] { t => t.ViewFieldCol };
            }
            ISpecification<CPFormView> specification;
            specification = new ExpressionSpecification<CPFormView>(t => t.FormId.Equals(formId));
            IList<CPFormView> col = this._CPFormViewRep.GetByCondition(specification,eagerLoadingProperties);
            return col.ToList();
        }
        public CPFormUseScene GetFormUseScene(string sceneCode,bool isLoadFuncInfo)
        {
            Expression<Func<CPFormUseScene, dynamic>>[] eagerLoadingProperties = null;
            if (isLoadFuncInfo)
            {
                eagerLoadingProperties = new Expression<Func<CPFormUseScene, dynamic>>[] { t => t.FuncCol };
            }
            ISpecification<CPFormUseScene> specification;
            specification = new ExpressionSpecification<CPFormUseScene>(t => t.SceneCode.Equals(sceneCode));
            IList<CPFormUseScene> col = this._CPFormUseSceneRep.GetByCondition(specification, eagerLoadingProperties);
            if (col.Count <= 0)
                return null;
            else
                return col[0];
        }
        #endregion

        public List<CPFormChildTable> GetFormChildTable(int formId)
        {
            ISpecification<CPFormChildTable> specification;
            specification = new ExpressionSpecification<CPFormChildTable>(t => t.FormId.Equals(formId));
            IList<CPFormChildTable> col = this._CPFormChildTableRep.GetByCondition(specification);
            return col.ToList();
        }
        public List<CPFormField> GetFormFieldCol(int formId)
        {
            ISpecification<CPFormField> specification;
            specification = new ExpressionSpecification<CPFormField>(t => t.FormId.Equals(formId));
            IList<CPFormField> col = this._CPFormFieldRep.GetByCondition(specification);
            return col.ToList();
        }
        public CPFormGroup GetFormRightGroup(string rightGroupCode,bool isLoadFieldRightInfo)
        {
            Expression<Func<CPFormGroup, dynamic>>[] eagerLoadingProperties = null;
            if (isLoadFieldRightInfo)
            {
                eagerLoadingProperties = new Expression<Func<CPFormGroup, dynamic>>[] { t => t.FieldRightCol };
            }
            ISpecification<CPFormGroup> specification;
            specification = new ExpressionSpecification<CPFormGroup>(t => t.GroupCode.Equals(rightGroupCode));
            IList<CPFormGroup> col = this._CPFormGroupRep.GetByCondition(specification, eagerLoadingProperties);
            if (col.Count > 0)
                return col[0];
            else
                return null;
        }
        public CPFormGroup GetFormInitGroup(string initGroupCode, bool isLoadFieldInitInfo)
        {
            Expression<Func<CPFormGroup, dynamic>>[] eagerLoadingProperties = null;
            if (isLoadFieldInitInfo)
            {
                eagerLoadingProperties = new Expression<Func<CPFormGroup, dynamic>>[] { t => t.FieldInitCol };
            }
            ISpecification<CPFormGroup> specification;
            specification = new ExpressionSpecification<CPFormGroup>(t => t.GroupCode.Equals(initGroupCode));
            IList<CPFormGroup> col = this._CPFormGroupRep.GetByCondition(specification, eagerLoadingProperties);
            if (col.Count > 0)
                return col[0];
            else
                return null;
        }
        #region 根据表单配置，自动初始化字段信息
        public bool SynFieldFromDb(int formId)
        {
            CPForm form = this._CPFormRep.Get(formId);
            if (form == null)
                return false;
            List<CPFormChildTable> tableCol = this.GetFormChildTable(formId);
            List<CPFormField> fieldCol = this.GetFormFieldCol(formId);
            List<CPFormField> add = new List<Form.CPFormField>();
            List<CPFormField> edit = new List<Form.CPFormField>();
            List<CPFormField> del = new List<Form.CPFormField>();

            List<CPDbField> tmpAllDbField = new List<CPDbField>();
            #region 先获取主表的字段
            List<CPDbField> mainTableFieldCol = CPAppContext.GetTableField(form.DbIns, form.MainTableName);
            if (mainTableFieldCol.Count > 0)
                tmpAllDbField.AddRange(mainTableFieldCol);
            mainTableFieldCol.ForEach(t => {
                List<CPFormField> q = fieldCol.Where(c => c.TableName.Equals(t.TableName, StringComparison.CurrentCultureIgnoreCase) && c.FieldName.Equals(t.FieldName, StringComparison.CurrentCultureIgnoreCase) && c.IsChildTable == false).ToList();
                if(q.Count >0)
                {
                    //修改
                   // q[0].IsAllowNull = t.IsAllowNull;
                    q[0].FieldValueType = t.ValueType;
                    q[0].FieldValueLength = t.ValueLength;
                    edit.Add(q[0]);
                }
                else
                {
                    //添加 
                    CPFormField newQ = new CPFormField();
                    newQ.FormId = form.Id;
                    newQ.IsChildTable = false;
                    newQ.TableName = t.TableName;
                    newQ.FieldName = t.FieldName;
                    newQ.FieldTitle = t.FieldName;
                    newQ.ControlType = CPFormEnum.ControlTypeEnum.TextBox;
                    newQ.IsAllowNull = t.IsAllowNull;
                    newQ.FieldValueType = t.ValueType;
                    newQ.FieldValueLength = t.ValueLength;
                    newQ.OrganIsCanMultiSel = true;
                    newQ.FieldStatus = CPFormEnum.FieldStatusEnum.Edit;
                    newQ.FieldTitleShowWidth = 20;
                    newQ.ShowWidth = "98%";
                    newQ.MultiRows = 5;
                    newQ.ShowHeight = 100;

                    add.Add(newQ);
                }

            });
            #endregion

            #region 再找子表的主段
            tableCol.ForEach(childTable => {
                #region 先找出子表本身
                List<CPFormField> qT = fieldCol.Where(c => c.TableName.Equals(childTable.TableName, StringComparison.CurrentCultureIgnoreCase) && c.FieldName.Equals(childTable.TableName, StringComparison.CurrentCultureIgnoreCase) && c.IsChildTable == true).ToList();
                if (qT.Count<= 0)
                {
                    //添加 
                    CPFormField newQ = new CPFormField();
                    newQ.FormId = form.Id;
                    newQ.IsChildTable = true;
                    newQ.TableName = childTable.TableName;
                    newQ.FieldName = childTable.TableName;
                    newQ.FieldTitle = childTable.TableName+"子表";
                    newQ.ControlType = CPFormEnum.ControlTypeEnum.ChildTableExtend;
                    newQ.OrganIsCanMultiSel = true;
                    add.Add(newQ);
                }
                #endregion
                List<CPDbField> cTableFieldCol = CPAppContext.GetTableField(form.DbIns, childTable.TableName);
                if (cTableFieldCol.Count > 0)
                    tmpAllDbField.AddRange(cTableFieldCol);
                cTableFieldCol.ForEach(t => {
                    List<CPFormField> q = fieldCol.Where(c => c.TableName.Equals(t.TableName, StringComparison.CurrentCultureIgnoreCase) && c.FieldName.Equals(t.FieldName, StringComparison.CurrentCultureIgnoreCase) && c.IsChildTable == false).ToList();
                    if (q.Count > 0)
                    {
                        //修改
                      //  q[0].IsAllowNull = t.IsAllowNull;
                        q[0].FieldValueType = t.ValueType;
                        q[0].FieldValueLength = t.ValueLength;
                        edit.Add(q[0]);
                    }
                    else
                    {
                        //添加 
                        CPFormField newQ = new CPFormField();
                        newQ.FormId = form.Id;
                        newQ.IsChildTable = false;
                        newQ.TableName = t.TableName;
                        newQ.FieldName = t.FieldName;
                        newQ.FieldTitle = t.FieldName;
                        newQ.ControlType = CPFormEnum.ControlTypeEnum.TextBox;
                        newQ.IsAllowNull = t.IsAllowNull;
                        newQ.FieldValueType = t.ValueType;
                        newQ.FieldValueLength = t.ValueLength;
                        newQ.OrganIsCanMultiSel = true;

                        newQ.FieldStatus = CPFormEnum.FieldStatusEnum.Edit;
                        newQ.FieldTitleShowWidth = 20;
                        newQ.ShowWidth = "98%";
                        newQ.MultiRows = 5;
                        newQ.ShowHeight = 100;
                        add.Add(newQ);
                    }

                });
            });
            #endregion

            #region 再找出要删除的
            fieldCol.ForEach(t => {
                if (t.IsChildTable == false)
                {
                    List<CPDbField> qQ = tmpAllDbField.Where(c => c.TableName.Equals(t.TableName,StringComparison.CurrentCultureIgnoreCase) && c.FieldName.Equals(t.FieldName, StringComparison.CurrentCultureIgnoreCase)).ToList();
                    if (qQ.Count <= 0)
                        del.Add(t);
                }
                else
                {
                    List<CPFormChildTable> qQ = tableCol.Where(c => c.TableName.Equals(t.TableName, StringComparison.CurrentCultureIgnoreCase)).ToList();
                    if (qQ.Count <= 0)
                        del.Add(t);
                }
            });
            #endregion

            //由于ef core批量写入数据时，采用了批处理方法，但这种方式会导致写入顺序错，暂时没有找到什么方法，所以改成一条条写。
            if (add.Count > 0)
                this._CPFormFieldRep.AddOneByOne(add);
            if (edit.Count > 0)
                this._CPFormFieldRep.Update(edit);
            if (del.Count > 0)
                this._CPFormFieldRep.Delete(del);

            #region 同时再修改视图排版里的字段信息
            List<CPFormView> viewCol = this.GetFormView(form.Id,true);
            if (add.Count >0)
            {
                #region 增加的
                viewCol.ForEach(t => {
                    if(t.ViewType == CPFormEnum.ViewTypeEnum.OneColumn
                    ||
                    t.ViewType == CPFormEnum.ViewTypeEnum.TwoColumn)
                    {
                        int nOrder = 0;
                        if (t.ViewFieldCol.Count > 0)
                        {
                            nOrder = t.ViewFieldCol.OrderByDescending(c => c.ShowOrder).FirstOrDefault().ShowOrder.Value;
                            nOrder = nOrder + 10;
                        }
                        List<CPFormViewField> col = new List<CPFormViewField>();
                        add.ForEach(f => {
                            CPFormViewField vT = new CPFormViewField();
                            vT.FormId = form.Id;
                            vT.ViewId = t.Id;
                            vT.FieldId = f.Id;
                            vT.ShowOrder = nOrder;
                            nOrder = nOrder + 10;
                            vT.ViewBlockIndex = 0;
                           // vT.FieldStatus = CPFormEnum.FieldStatusEnum.Edit;
                            vT.IsSpanAll = false;
                            //vT.FieldTitleShowWidth = 20;
                            //vT.ShowWidth = "98%";
                            //vT.MultiRows = 5;
                            //vT.ShowHeight = 100;

                            col.Add(vT);
                        });
                       if(col.Count >0)
                        {
                            this._CPFormViewFieldRep.Add(col);
                        }
                    }
                });
                #endregion
            }
            if (del.Count > 0)
            {
                #region 删除的
              
                viewCol.ForEach(t => {
                    if (t.ViewType == CPFormEnum.ViewTypeEnum.OneColumn
                    ||
                    t.ViewType == CPFormEnum.ViewTypeEnum.TwoColumn)
                    {
                      
                        List<CPFormViewField> col = new List<CPFormViewField>();
                        t.ViewFieldCol.ForEach(c => {
                            if(del.Where(f=>f.Id.Equals(c.FieldId)).ToList().Count >0)
                            {
                                col.Add(c);
                            }
                        });
                        if (col.Count > 0)
                        {
                            this._CPFormViewFieldRep.Delete(col);
                        }
                    }
                });
                #endregion
            }
            #endregion
            return true;

        }
        #endregion

        #region 添加默认的应用场景
        public bool AddDefaultScene(int formId)
        {
            CPForm form = this._CPFormRep.Get(formId);
            if (form == null)
                return false;
            CPFormUseScene scene = new CPFormUseScene();
            int autoIndex;
            scene.FormId = formId;
            scene.SceneCode = CPAutoNumHelper.Instance().GetNextAutoNum("FormSceneCodeAuto", out autoIndex);
            scene.AutoIndex = autoIndex;
            scene.FormSavedAction = CPFormEnum.FormSavedActionEnum.ReturnEditPage;
            scene.SceneName = "默认";
            scene.FuncCol = new List<CPFormUseSceneFunc>();
            CPFormUseSceneFunc func = new CPFormUseSceneFunc();
            func.FuncTitle = "保存";
            func.ShowOrder = 10;
            func.FuncExeJS = "CPFormSaveFormData()";
            func.FuncIsShowInView = CPFormEnum.FuncIsShowInViewEnum.OnlyWriteShow;
            func.IsControlByRight = false;
            func.FormId = formId;
            scene.FuncCol.Add(func);
            this._CPFormUseSceneRep.Add(scene);
            return true;
        }
        #endregion

        #region 根据表单配置，自动初始化一个默认排版视图
        public bool InitFormDefaultView(int formId)
        {
            CPForm form = this._CPFormRep.Get(formId);
            if (form == null)
                return false;
            CPFormView view = new Form.CPFormView();
            int autoIndex;
            view.ViewCode = CPAutoNumHelper.Instance().GetNextAutoNum("FormViewCodeAuto",out autoIndex);
            view.AutoIndex = autoIndex;
            view.ViewName = "默认一行两列式视图";
            view.FormId = form.Id;
            view.ViewType = CPFormEnum.ViewTypeEnum.TwoColumn;
            view.ViewDeviceType = CPFormEnum.ViewDeviceTypeEnum.PCIphoneIpad;
            view.IsDefault = true;
            view.ViewFieldCol = new List<CPFormViewField>();
            #region 获取所有的字段
            List<CPFormField> fieldCol = this.GetFormFieldCol(formId);
            int nOrder = 10;
            fieldCol.ForEach(t => {
                CPFormViewField vT = new CPFormViewField();
                vT.FormId = form.Id;
                vT.FieldId = t.Id;
                vT.ShowOrder = nOrder;
                nOrder = nOrder + 10;
                vT.ViewBlockIndex = 0;
              //  vT.FieldStatus = CPFormEnum.FieldStatusEnum.Edit;
                vT.IsSpanAll = false;
                //vT.FieldTitleShowWidth = 20;
                //vT.ShowWidth = "98%";
                //vT.MultiRows = 5;
                //vT.ShowHeight = 100;

                view.ViewFieldCol.Add(vT);
            });
            #endregion
            this._CPFormViewRep.Add(view);
            return true;

        }
        #endregion

        #region 根据表单配置，自动初始化一个使用编辑器排版视图
        public bool InitFormDefaultViewForEditor(int formId)
        {
            CPForm form = this.GetForm(formId, true, true, false, false, false, true);
            if (form == null)
                return false;
            CPFormView view = new Form.CPFormView();
            int autoIndex;
            view.ViewCode = CPAutoNumHelper.Instance().GetNextAutoNum("FormViewCodeAuto", out autoIndex);
            view.AutoIndex = autoIndex;
            view.ViewName = "使用编辑器排版视图";
            view.FormId = form.Id;
            view.ViewType = CPFormEnum.ViewTypeEnum.TextEditor;
            view.ViewDeviceType = CPFormEnum.ViewDeviceTypeEnum.PCIphoneIpad;
            view.IsDefault = false;
            view.FormViewHTML = "";
            view.ViewFieldCol = new List<CPFormViewField>();
            #region 默认组一个一行四列的排版
            List<CPFormField> fieldCol = this.GetFormFieldCol(form.Id);
            List<CPFormViewField> viewFieldCol = new List<CPFormViewField>();
            int nIndex = 10;
            fieldCol.ForEach(t => {
                CPFormViewField vF = new CPFormViewField();
                vF.FieldId = t.Id;
                vF.FormId = t.FormId;
                vF.IsSpanAll = false;
                vF.ShowOrder = nIndex;
                nIndex++;
                vF.ViewBlockIndex = 0;
                vF.ViewId = 0;
                viewFieldCol.Add(vF);
            });
            view.ViewFieldCol = viewFieldCol;
            //先取出主表的和包括子拓展表的字段
            List<CPFormField> mainTableField = fieldCol.Where(t => t.TableName.Equals(form.MainTableName) || t.IsChildTable.Value).ToList();
            List<int> fieidCol = new List<int>();
            mainTableField.ForEach(t => { fieidCol.Add(t.Id); });
            //转化成视图中的字段
            List<CPFormViewField> mainTableFormViewField = view.ViewFieldCol.Where(c => fieidCol.Contains(c.FieldId)).ToList();
            string mainHTML = this.FormatOneTableHTML(form, fieldCol, view, mainTableFormViewField, 
                new List<CPFormFieldRight>(),  CPEnum.DeviceTypeEnum.PCBrowser, 4, true,false,false);
            //再看看子表的
            if (form.ChildTableCol != null && form.ChildTableCol.Count > 0)
            {
                form.ChildTableCol.ForEach(cTable => {
                    List<CPFormField> cTableField = fieldCol.Where(t => t.TableName.Equals(cTable.TableName) && t.IsChildTable.Value == false).ToList();
                    fieidCol.Clear();
                    cTableField.ForEach(t => { fieidCol.Add(t.Id); });
                    //转化成视图中的字段
                    List<CPFormViewField> cTableFormViewField = view.ViewFieldCol.Where(c => fieidCol.Contains(c.FieldId)).ToList();
                    string cHTML = this.FormatOneTableHTML(form, fieldCol, view, cTableFormViewField,
                        new List<CPFormFieldRight>(),  CPEnum.DeviceTypeEnum.PCBrowser, 4, false, false,false);
                    mainHTML = mainHTML.Replace("{@" + cTable.TableName + "@}", cHTML);
                });
            }
            // sbHTML.Append(mainHTML);
            view.FormViewHTML = mainHTML;
            #endregion
            this._CPFormViewRep.Add(view);
            return true;

        }
        #endregion

        #region 根据视图，自动组织展现的HTML
        private string FormatOneTableHTML(CPForm form, List<CPFormField> fieldCol, CPFormView view, List<CPFormViewField> viewFieldCol, 
            List<CPFormFieldRight> fieldRightCol,
            CPEnum.DeviceTypeEnum curDeviceType,int oneRowColumn,bool isMainTable,bool isRealCreateControlHtml,bool isView)
        {
            StringBuilder sbHTML = new StringBuilder();
            List<CPFormViewField> tmpFieldCol = viewFieldCol.OrderBy(t => t.ShowOrder).ToList();
            #region 内置布局，按table方式
            if (curDeviceType == CPEnum.DeviceTypeEnum.PCBrowser)
            {
                StringBuilder sbHidden = new StringBuilder();
                sbHTML.Append("<table  border=\"0\" style='width:99%;' align=\"center\" cellpadding=\"0\" cellspacing=\"0\"><tbody>");
                if (isMainTable)
                {
                    //如果没有拓展表，则自动构建一个隐藏的下拉列表，用来解决没有ng-repeat，不能执行ngRepeatFinished事件的问题
                    if (form.ChildTableCol.Count <= 0)
                    {
                        if (isRealCreateControlHtml)
                        {
                            string sHtml = @"<select   id='CPForm_TmpHideSelect'     style='width:0px;display:none;' ";
                            sHtml += @"<option value='{{selectItem.valueEx}}' ng-repeat='selectItem in FormObj.Data.CPFormTmpHideSelectTable track by $index'  on-Repeat-Finished-Render >{{selectItem.textEx}}</option>";
                            sHtml += " />";

                            sbHTML.Append("<tr><td colspan='" + oneRowColumn + "' class='trHeader'>" + form.FormTitle + sHtml + "</td></tr>");
                        }
                    }
                    else
                    {
                        sbHTML.Append("<tr><td colspan='" + oneRowColumn + "' class='trHeader'>" + form.FormTitle + "</td></tr>");
                    }
                }
                int curRowIndex = 0;
                bool lastIsTrEnd = false;
                tmpFieldCol.ForEach(t => {
                    IList<CPFormField> tmpCol = fieldCol.Where(c => c.Id.Equals(t.FieldId)).ToList();
                    if (tmpCol.Count <= 0)
                        return;
                    CPFormFieldRight fieldRight = null;
                    IList<CPFormFieldRight> rCol = fieldRightCol.Where(c => c.FieldId.Equals(t.FieldId)).ToList();
                    if (rCol.Count > 0)
                        fieldRight = rCol[0];
                    if(tmpCol[0].FieldStatus == CPFormEnum.FieldStatusEnum.Hidden)
                    {

                        string controlHtml = "";
                        if (isRealCreateControlHtml)
                        {
                            if(isView)
                            {
                                if (tmpCol[0].FieldStatus == CPFormEnum.FieldStatusEnum.Edit)
                                    tmpCol[0].FieldStatus = CPFormEnum.FieldStatusEnum.Read;

                            }
                            controlHtml = ICPFormControlManager.GetControlInstance(tmpCol[0]).FormatHtml(tmpCol[0], fieldRight, isMainTable ? false : true);
                        }
                        else
                        {
                            controlHtml = "{@" + tmpCol[0].TableName + "." + tmpCol[0].FieldName + "@}";
                        }
                        sbHidden.Append(controlHtml);
                        return;
                    }
                    if (tmpCol[0].ControlType == CPFormEnum.ControlTypeEnum.ChildTableExtend)
                    {
                        #region 子表
                        if (lastIsTrEnd == false)
                        {
                            sbHTML.Append("<td class='tdLeft'></td><td class='tdRight'></td></tr>");
                        }
                        sbHTML.Append("<tr><td colspan='" + oneRowColumn + "' class='trHeader'><div class='ChildTableHeaderLeft'>" + tmpCol[0].FieldTitle + "</div><div  class='ChildTableHeaderRight'><input type='button' id='btnCPFormAddChildRow_" + tmpCol[0].TableName + "'  class='btnExtendTableAdd'  ng-click=\"CPFormAddChildRow('btnCPFormAddChildRow_" + tmpCol[0].TableName + "',true)\"  data-TableName='" + tmpCol[0].TableName + "'  value='添加'/></div></td></tr>");
                        sbHTML.Append("<tr   ng-repeat='item in FormObj.Data." + tmpCol[0].TableName + "' on-Repeat-Finished-Render  id=\"trCPFormExtendTable_" + tmpCol[0].TableName + "\"  ><td colspan='" + oneRowColumn + "' class='CPFormExtendTableTdCss' >");
                        sbHTML.Append("{@" + tmpCol[0].FieldName + "@}");//最后统一替换
                        sbHTML.Append("</td></tr>");
                        curRowIndex = 0;
                        lastIsTrEnd = true;
                        #endregion
                    }
                    else
                    {
                        #region 非子表
                        string notNull = "";
                        if(tmpCol[0].IsAllowNull.Value==false)
                        {
                            notNull = "<span  class='NotAllowNullCss'>*</span>";
                        }
                        if (t.IsSpanAll.Value)
                        {
                            #region 通栏展现
                            if (lastIsTrEnd==false && curRowIndex != 0)
                            {
                                sbHTML.Append("<td class='tdLeft' style='width:" + tmpCol[0].FieldTitleShowWidth + "%'></td>");
                                sbHTML.Append("<td class='tdRight' style='width:" + (50- tmpCol[0].FieldTitleShowWidth ) + "%;' ></td>");
                                sbHTML.Append("</tr>");
                            }
                            sbHTML.Append("<tr>");
                            sbHTML.Append("<td class='tdLeft' style='width:" + tmpCol[0].FieldTitleShowWidth + "%'>" + tmpCol[0].FieldTitle +  ":" + notNull +"</td>");
                            string controlHtml = "";
                            if(isRealCreateControlHtml)
                            {
                                if (isView)
                                {
                                    if (tmpCol[0].FieldStatus == CPFormEnum.FieldStatusEnum.Edit)
                                        tmpCol[0].FieldStatus = CPFormEnum.FieldStatusEnum.Read;

                                }
                                controlHtml = ICPFormControlManager.GetControlInstance(tmpCol[0]).FormatHtml(tmpCol[0], fieldRight, isMainTable ? false : true);
                            }
                            else
                            {
                                controlHtml = "{@" + tmpCol[0].TableName + "." + tmpCol[0].FieldName + "@}";
                            }
                            sbHTML.Append("<td class='tdRight' style='width:" + (100 - tmpCol[0].FieldTitleShowWidth) + "%;'  colspan='" + (oneRowColumn-1) + "'>" +controlHtml+ "</td>");
                            sbHTML.Append("</tr>");
                            curRowIndex = 0;
                            lastIsTrEnd = true;
                            #endregion
                        }
                        else
                        {
                            #region 非通栏展现
                            if (curRowIndex == 0)
                            {
                                sbHTML.Append("<tr>");
                                lastIsTrEnd = false;
                            }

                            sbHTML.Append("<td class='tdLeft' style='width:" + tmpCol[0].FieldTitleShowWidth + "%'>" + tmpCol[0].FieldTitle +":" + notNull + "</td>");
                            string controlHtml = "";
                            if (isRealCreateControlHtml)
                            {
                                if (isView)
                                {
                                    if (tmpCol[0].FieldStatus == CPFormEnum.FieldStatusEnum.Edit)
                                        tmpCol[0].FieldStatus = CPFormEnum.FieldStatusEnum.Read;

                                }
                                controlHtml = ICPFormControlManager.GetControlInstance(tmpCol[0]).FormatHtml(tmpCol[0], fieldRight, isMainTable ? false : true);
                            }
                            else
                            {
                                controlHtml = "{@" + tmpCol[0].TableName + "." + tmpCol[0].FieldName + "@}";
                            }
                            sbHTML.Append("<td class='tdRight' style='width:" + (50- tmpCol[0].FieldTitleShowWidth ) + "%;' >" + controlHtml+ "</td>");
                            curRowIndex++;
                            if (curRowIndex >= 2)
                            {
                                sbHTML.Append("</tr>");
                                curRowIndex = 0;
                                lastIsTrEnd = true;
                            }
                            #endregion
                        }
                        #endregion
                    }
                });
                if (lastIsTrEnd == false)
                {
                    sbHTML.Append("<td class='tdLeft'></td><td class='tdRight'></td></tr>");
                }
                if (sbHidden.Length > 0)
                {
                    sbHTML.Append("<tr ><td colspan='" + oneRowColumn + "' >");
                    sbHTML.Append(sbHidden);
                    sbHTML.Append("</td></tr>");
                }
                if (isMainTable==false)
                {//拓展表，最后加一行空白作为分隔
                    sbHTML.Append("<tr ><td colspan='" + oneRowColumn + "' class='trExtendTableLastRow' >"); 
                    sbHTML.Append("</td></tr>");
                }
               
                sbHTML.Append("</tbody></table>");
            }

            #endregion
            return sbHTML.ToString();
        }
        public string FormatFormViewHtml(CPForm form,List<CPFormField> fieldCol, CPFormView view,
            List<CPFormFieldRight> fieldRightCol, CPEnum.DeviceTypeEnum curDeviceType,bool isRealCreateControlHtml
            ,bool isView)
        {
            StringBuilder sbHTML = new StringBuilder();
            if (fieldRightCol == null)
                fieldRightCol = new List<CPFormFieldRight>();
            if (view.ViewType == CPFormEnum.ViewTypeEnum.TwoColumn ||
                view.ViewType == CPFormEnum.ViewTypeEnum.OneColumn)
            {
                #region 内置布局，一行四列式或一行两列式
                if(view.ViewType == CPFormEnum.ViewTypeEnum.OneColumn)
                {
                    //改成通栏展现，即变成 一行两列式
                    view.ViewFieldCol.ForEach(t => {
                        t.IsSpanAll = true;
                    });
                }
                //先取出主表的和包括子拓展表的字段
                List<CPFormField> mainTableField = fieldCol.Where(t => t.TableName.Equals(form.MainTableName) || t.IsChildTable.Value).ToList();
                List<int> fieidCol = new List<int>();
                mainTableField.ForEach(t => { fieidCol.Add(t.Id); });
                //转化成视图中的字段
                List<CPFormViewField> mainTableFormViewField = view.ViewFieldCol.Where(c => fieidCol.Contains(c.FieldId)).ToList();
                string mainHTML = this.FormatOneTableHTML(form, fieldCol, view, mainTableFormViewField,
                    fieldRightCol, curDeviceType, 4,true, isRealCreateControlHtml,isView);
                //再看看子表的
                if(form.ChildTableCol != null && form.ChildTableCol.Count >0)
                {
                    form.ChildTableCol.ForEach(cTable => {
                        List<CPFormField> cTableField = fieldCol.Where(t => t.TableName.Equals(cTable.TableName) && t.IsChildTable.Value==false).ToList();
                        fieidCol.Clear();
                        cTableField.ForEach(t => { fieidCol.Add(t.Id); });
                        //转化成视图中的字段
                        List<CPFormViewField> cTableFormViewField = view.ViewFieldCol.Where(c => fieidCol.Contains(c.FieldId)).ToList();
                        string cHTML = this.FormatOneTableHTML(form, fieldCol, view, cTableFormViewField, 
                            fieldRightCol, curDeviceType, 4,false, isRealCreateControlHtml,isView);
                        mainHTML = mainHTML.Replace("{@" + cTable.TableName + "@}", cHTML);
                    });
                }
                sbHTML.Append(mainHTML);
                #endregion
            }
           
            else if (view.ViewType == CPFormEnum.ViewTypeEnum.TextEditor)
            {
                #region 编辑器布局
                string sHtml = view.FormViewHTML;
                string regexConst = @"{\@[\s\S]*?\@}";
                Regex re = new Regex(regexConst, RegexOptions.IgnoreCase);
                MatchCollection matches = re.Matches(sHtml);
                foreach (Match match in matches)
                {
                    sHtml = sHtml.Replace(match.Value, this.GetControlHtml( match.Value,form,fieldCol,view,fieldRightCol,
                        curDeviceType,isView));
                }
                sbHTML.Append(sHtml);
                if (form.ChildTableCol.Count <= 0)
                {
                    string sHtml2 = @"<select   id='CPForm_TmpHideSelect'     style='width:0px;display:none;' ";
                    sHtml2 += @"<option value='{{selectItem.valueEx}}' ng-repeat='selectItem in FormObj.Data.CPFormTmpHideSelectTable track by $index'  on-Repeat-Finished-Render >{{selectItem.textEx}}</option>";
                    sHtml2 += " />";

                    sbHTML.Append("<div>" + sHtml2 + "</div>");
                }
                #endregion
            }
            return sbHTML.ToString();
        }
        private string GetControlHtml(string tableField, CPForm form, List<CPFormField> fieldCol, CPFormView view,
            List<CPFormFieldRight> fieldRightCol, CPEnum.DeviceTypeEnum curDeviceType
            ,bool isView)
        {
            tableField = tableField.Replace("{@", "").Replace("@}", "");
            string[] fArray = tableField.Split('.');
            CPFormField curField = null;
            fieldCol.ForEach(t => {
                if(t.TableName.Equals(fArray[0],StringComparison.CurrentCultureIgnoreCase)
                && t.FieldName.Equals(fArray[1],StringComparison.CurrentCultureIgnoreCase))
                {
                    curField = t;
                    return;
                }
            });
            if (curField == null)
                return "未找到字段【" + tableField + "】";
            CPFormFieldRight fieldRight = null;
            IList<CPFormFieldRight> rCol = fieldRightCol.Where(c => c.FieldId.Equals(curField.Id)).ToList();
            if (rCol.Count > 0)
                fieldRight = rCol[0];
            bool isMainTable = true;
            if (fArray[0].Equals(form.MainTableName,StringComparison.CurrentCultureIgnoreCase) == false)
                isMainTable = false;
            if(isView)
            {
                if (curField.FieldStatus == CPFormEnum.FieldStatusEnum.Edit)
                    curField.FieldStatus = CPFormEnum.FieldStatusEnum.Read;
            }
            return ICPFormControlManager.GetControlInstance(curField).FormatHtml(curField, fieldRight, isMainTable ? false : true);
        }
        #endregion

        #region 根据视图ID，获取表单所有的字段信息
        public List<CPFormField> GetFormFieldByViewId(int viewId)
        {
            CPFormView view = this.GetFormViewByViewId(viewId, false);
            return this.GetFormFieldCol(view.FormId);
        }
        #endregion


        #region 保存视图HTML
        public bool SaveViewHTML(int viewId,string viewHTML)
        {
            CPFormView view = this.GetFormViewByViewId(viewId, false);
            view.FormViewHTML = viewHTML;
            this._CPFormViewRep.Update(view);
            return true;
        }
        #endregion

        #region 配置导出，同步相关
        public string GetFormConfigXml(List<int> formIdCol)
        {

            if (formIdCol.Count <= 0)
                return "";
            DataSet ds = this._CPFormRep.GetConfig(formIdCol);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ds.WriteXml(ms);
            byte[] bData = ms.GetBuffer();
            ms.Close();
            return System.Text.UTF8Encoding.UTF8.GetString(bData);
        }
        /// <summary>
        /// 从xml创建新的列表配置实例 
        /// </summary>
        /// <param name="funcId"></param>
        /// <param name="sysId"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public bool InitFormFromConfigXml(int targetSysId, byte[] bData)
        {
            DataSet ds = new DataSet();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(bData, 0, bData.Length);
            ms.Position = 0;
            ds.ReadXml(ms);
            ms.Close();
            bool b = true;
            b = _CPFormRep.SyncConfigFromDataSet(targetSysId, ds, true);
            return b;
        }
        public bool SyncFormFromConfigXml(int targetSysId, byte[] bData)
        {
            DataSet ds = new DataSet();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(bData, 0, bData.Length);
            ms.Position = 0;
            ds.ReadXml(ms);
            ms.Close();
            bool b = true;
            b = _CPFormRep.SyncConfigFromDataSet(targetSysId, ds, false);
            return b;
        }
        #endregion   
    }
}

