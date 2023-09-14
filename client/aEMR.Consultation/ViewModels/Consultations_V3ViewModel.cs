﻿using System.Windows.Controls;
using Caliburn.Micro;
using System.ComponentModel.Composition;
using aEMR.ViewContracts;
using aEMR.Infrastructure;
using aEMR.Infrastructure.Events;
using System.Windows;
using DataEntities;
using aEMR.Common;
using Castle.Windsor;
using System.Collections.ObjectModel;
using aEMR.Common.ViewModels;
using aEMR.Common.BaseModel;
using System.Linq;

/*
 * 20180920 #001 TBL: Added IsDiagTrmentChanged
 * 20200415 #002 TTM: BM 0030117: Fix lỗi chết chương trình khi nhập ICD10.
 * 20230330 #003 QTD: Thêm ICD9 cho ngoại trú
 */
namespace aEMR.ConsultantEPrescription.ViewModels
{
    [Export(typeof(IConsultations_V3)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class Consultations_V3ViewModel : ViewModelBase, IConsultations_V3
        , IHandle<ConsultationDoubleClickEvent>
        , IHandle<DiagnosisTreatmentSelectedAndCloseEvent<DiagnosisTreatment>>
    {
        public ConsultationOldIsEditingEnable IsEditingEnable { get; set; }
        private bool _IsPopUp = false;
        public bool IsPopUp
        {
            get
            {
                return _IsPopUp;
            }
            set
            {
                if (_IsPopUp == value)
                    return;
                _IsPopUp = value;
                mChanDoan_tabSuaKhamBenh_ThongTin = mChanDoan_tabSuaKhamBenh_ThongTin && !IsPopUp;
            }
        }
        public IsInTreatmentProgramChanged IsInTreatmentProgramChangedCallback { get; set; }
        [ImportingConstructor]
        public Consultations_V3ViewModel(IWindsorContainer container, INavigationService navigationService, IEventAggregator eventArg)
        {
            Globals.EventAggregator.Subscribe(this);
            CreateSubVM();
            authorization();
        }

        // TxD 22/09/2018 Added the following to ALLOW child VM of each Tab to get ACCESS to the CENTRAL Data stored in the MainKB (Main KHAM BENH) Screen
        private ICS_DataStorage _CS_DS = null;
        public ICS_DataStorage CS_DS
        {
            get
            {
                return _CS_DS;
            }
            set
            {
                _CS_DS = value;
                ((IPatientInfo)UCPatientProfileInfo).CS_DS = CS_DS;
                ucOutPMR.CS_DS = CS_DS;
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            ActivateSubVM();
        }

        private void CreateSubVM()
        {
            UCDoctorProfileInfo = Globals.GetViewModel<ILoginInfo>();
            UCPatientProfileInfo = Globals.GetViewModel<IPatientInfo>();
            UCHeaderInfoPMR = Globals.GetViewModel<IPatientMedicalRecords_ByPatientID>();
            ucOutPMR = Globals.GetViewModel<IConsultationOld_V3>();
            ucOutPMR.IsInTreatmentProgramChangedCallback = new IsInTreatmentProgramChanged(() =>
            {
                if (IsInTreatmentProgramChangedCallback != null)
                {
                    IsInTreatmentProgramChangedCallback();
                }
            });
            ucOutPMRs = Globals.GetViewModel<IConsultationList>();
            UCPtRegDetailInfo = Globals.GetViewModel<IPtRegDetailInfo>();
            UCSmallProcedureEdit = Globals.GetViewModel<ISmallProcedureEdit>();
            //▼===== 20200409 TTM: IsFromOutOrInDiag để phân biệt xem màn hình thủ thuật đang xuất phát từ ngoại trú hay nội trú.
            //                     True là nội trú, False là ngoại trú.
            UCSmallProcedureEdit.IsFromOutOrInDiag = false;
            //▲=====
            ucOutPMR.gICD10Changed += (aICD10Collection) =>
            {
                if (aICD10Collection == null || !aICD10Collection.Any(x => x.IsMain) 
                //▼===== #002
                || aICD10Collection.Any(x => x.IsMain && x.DiseasesReference == null))
                //▲===== #002
                {
                    return;
                }
                if (UCSmallProcedureEdit != null && UCSmallProcedureEdit.SmallProcedureObj != null)
                {
                    DiseasesReference CurrentICD = aICD10Collection.First(x => x.IsMain).DiseasesReference.DeepCopy();
                    string aBeforeDiagTreatment = CurrentICD.DiseaseNameVN;
                    //CurrentICD.DiseaseNameVN = ucOutPMR.DiagTrmtItem.DiagnosisFinal;
                    CurrentICD.DiseaseNameVN = CurrentICD.DiagnosisFinal != null ? CurrentICD.DiagnosisFinal : ucOutPMR.DiagTrmtItem.DiagnosisFinal;
                    UCSmallProcedureEdit.SmallProcedureObj.Diagnosis = CurrentICD.DiseaseNameVN;
                    UCSmallProcedureEdit.SmallProcedureObj.BeforeICD10 = CurrentICD;
                    UCSmallProcedureEdit.CallNotifyOfPropertyChange(aBeforeDiagTreatment);
                }
            };
            ucOutPMR.IsEditingEnable += (aIsEnable) =>
            {
                IsEditingEnable?.Invoke(aIsEnable);
            };
            UCSmallProcedureDesc = Globals.GetViewModel<IHtmlEditor>();
        }

        private void ActivateSubVM()
        {
            ActivateItem(UCDoctorProfileInfo);
            ActivateItem(UCPatientProfileInfo);
            ActivateItem(UCHeaderInfoPMR);
            ActivateItem(ucOutPMR);
            ActivateItem(ucOutPMRs);
            ActivateItem(UCPtRegDetailInfo);
            ActivateItem(UCSmallProcedureEdit);
        }

        public IPatientMedicalRecords_ByPatientID UCHeaderInfoPMR
        {
            get;
            set;
        }

        public object UCDoctorProfileInfo
        {
            get;
            set;
        }
        public object UCPatientProfileInfo
        {
            get;
            set;
        }

        public TabControl tabCommon { get; set; }
        public void tabCommon_Loaded(object sender, RoutedEventArgs e)
        {
            tabCommon = (TabControl)sender;
            if (!mChanDoan_KhamBenhMoi || IsPopUp)
                tabCommon.SelectedIndex = 1;
        }

        public void authorization()
        {
            if (!Globals.isAccountCheck)
            {
                return;
            }
            //bucOutPMR = Globals.CheckOperation(Globals.listRefModule
            //    , (int)eModules.mConsultation, (int)eConsultation.mPtPMRConsultationNew, (int)oConsultationEx.mLanKhamCuoi);
            //bucOutPMREditor = Globals.CheckOperation(Globals.listRefModule
            //    , (int)eModules.mConsultation, (int)eConsultation.mPtPMRConsultationNew, (int)oConsultationEx.mKhamBenhMoi);
            //bucOutPMRs = Globals.CheckOperation(Globals.listRefModule
            //    , (int)eModules.mConsultation, (int)eConsultation.mPtPMRConsultationNew, (int)oConsultationEx.mCacLanKhamBenh); ;

            mChanDoan_KhamBenhMoi = Globals.CheckOperation(Globals.listRefModule
                                        , (int)eModules.mConsultation
                                        , (int)eConsultation.mPtPMRConsultationNew
                                        , (int)oConsultationEx.mChanDoan_KhamBenhMoi);

            mChanDoan_tabLanKhamTruoc_ThongTin = Globals.CheckOperation(Globals.listRefModule
                                        , (int)eModules.mConsultation
                                        , (int)eConsultation.mPtPMRConsultationNew
                                        , (int)oConsultationEx.mChanDoan_tabLanKhamTruoc_ThongTin);
            mChanDoan_tabSuaKhamBenh_ThongTin = Globals.CheckOperation(Globals.listRefModule
                                        , (int)eModules.mConsultation
                                        , (int)eConsultation.mPtPMRConsultationNew
                                        , (int)oConsultationEx.mChanDoan_tabSuaKhamBenh_ThongTin);
        }
        #region account checking

        private bool _mChanDoan_KhamBenhMoi = true;
        private bool _mChanDoan_tabLanKhamTruoc_ThongTin = true;
        private bool _mChanDoan_tabSuaKhamBenh_ThongTin = true;
        public bool mChanDoan_KhamBenhMoi
        {
            get
            {
                return _mChanDoan_KhamBenhMoi;
            }
            set
            {
                if (_mChanDoan_KhamBenhMoi == value)
                    return;
                _mChanDoan_KhamBenhMoi = value;
            }
        }
        public bool mChanDoan_tabLanKhamTruoc_ThongTin
        {
            get
            {
                return _mChanDoan_tabLanKhamTruoc_ThongTin;
            }
            set
            {
                if (_mChanDoan_tabLanKhamTruoc_ThongTin == value)
                    return;
                _mChanDoan_tabLanKhamTruoc_ThongTin = value;
            }
        }
        public bool mChanDoan_tabSuaKhamBenh_ThongTin
        {
            get
            {
                return _mChanDoan_tabSuaKhamBenh_ThongTin;
            }
            set
            {
                if (_mChanDoan_tabSuaKhamBenh_ThongTin == value)
                    return;
                _mChanDoan_tabSuaKhamBenh_ThongTin = value;

            }
        }

        private bool _bucOutPMR = true;
        private bool _bucOutPMREditor = true;
        private bool _bucOutPMRs = true;
        public bool bucOutPMR
        {
            get
            {
                return _bucOutPMR;
            }
            set
            {
                if (_bucOutPMR == value)
                    return;
                _bucOutPMR = value;
            }
        }
        public bool bucOutPMREditor
        {
            get
            {
                return _bucOutPMREditor;
            }
            set
            {
                if (_bucOutPMREditor == value)
                    return;
                _bucOutPMREditor = value;
            }
        }
        public bool bucOutPMRs
        {
            get
            {
                return _bucOutPMRs;
            }
            set
            {
                if (_bucOutPMRs == value)
                    return;
                _bucOutPMRs = value;
            }
        }
        #endregion
        public IConsultationOld_V3 ucOutPMR { get; set; }
        public object ucOutPMREditor
        {
            get;
            set;
        }
        public IConsultationList ucOutPMRs { get; set; }


        public IPtRegDetailInfo UCPtRegDetailInfo
        {
            get;
            set;
        }
        public object gTabEdit
        {
            get;
            set;
        }
        public void TabEdit_Loaded(object sender, RoutedEventArgs e)
        {
            gTabEdit = sender;
        }

        #region IHandle<DoubleClickEvent> Members
        public void Handle(ConsultationDoubleClickEvent message)
        {
            //if (message != null && this.IsActive) test01
            if (message != null)
            {
                ((TabItem)gTabEdit).IsSelected = true;
            }
        }
        #endregion


        #region IHandle<ConsultationChooseServiceRecIDEvent> Members

        //public void Handle(ConsultationChooseServiceRecIDEvent message)
        //{
        //    if (message != null && this.IsActive)
        //    {
        //        TryClose();
        //        Globals.EventAggregator.Publish(new ConsultationParentChooseServiceRecIDEvent { DiagTrmtItem = message.DiagTrmtItem });
        //    }
        //}

        #endregion

        #region IHandle<CloseConsultationNewEvent> Members
        //public void Handle(CloseConsultationNewEvent message)
        //{
        //    if (message != null && this.IsActive)
        //    {
        //        TryClose();
        //        Globals.EventAggregator.Publish(new ConsultationParentChooseServiceRecIDEvent { DiagTrmtItem = message.DiagTrmtItem });
        //    }
        //}
        #endregion

        //Chọn 1 chẩn đoán và đóng Window Chẩn Đoán lại
        public void Handle(DiagnosisTreatmentSelectedAndCloseEvent<DiagnosisTreatment> message)
        {
            if (this.GetView() != null)
            {
                if (message != null)
                {
                    //Phát chọn 1 chẩn đoán và đóng window

                    TryClose();
                    Globals.EventAggregator.Publish(new DiagnosisTreatmentSelectedEvent<DiagnosisTreatment> { DiagnosisTreatment = message.DiagnosisTreatment.DeepCopy() });
                    Globals.EventAggregator.Unsubscribe(this);

                }
            }
        }
        public void InitPatientInfo(PatientRegistrationDetail PtRegDetail)
        {
            ucOutPMR.InitPatientInfo(PtRegDetail);
            ucOutPMRs.InitPatientInfo();
        }
        public bool CheckValidDiagnosis()
        {
            return ucOutPMR.CheckValidDiagnosis();
        }
        public ObservableCollection<DiagnosisIcd10Items> refIDC10List
        {
            get
            {
                return ucOutPMR.refIDC10List;
            }
            set
            {
                ucOutPMR.refIDC10List = value;
            }
        }
        public long Compare2Object()
        {
            return ucOutPMR.Compare2Object();
        }
        public DiagnosisTreatment DiagTrmtItem
        {
            get
            {
                return ucOutPMR.DiagTrmtItem;
            }
            set
            {
                ucOutPMR.DiagTrmtItem = value;
            }
        }
        public void ChangeStatesAfterUpdated(bool IsUpdate = false)
        {
            ucOutPMR.ChangeStatesAfterUpdated(IsUpdate);
            //Khi lưu thành công chẩn đoán thì set cho ObjDoctorStaffID để hiển thị trên màn hình khỏi load lại
            ucOutPMR.DiagTrmtItem.ObjDoctorStaffID = ucOutPMR.DiagTrmtItem.PatientServiceRecord.Staff;
        }
        public bool IsShowSummaryContent
        {
            get
            {
                return ucOutPMR.IsShowSummaryContent;
            }
            set
            {
                ucOutPMR.IsShowSummaryContent = value;
            }
        }
        public bool btUpdateIsEnabled
        {
            get { return ucOutPMR.btUpdateIsEnabled; }
        }
        public bool btSaveCreateNewIsEnabled
        {
            get { return ucOutPMR.btSaveCreateNewIsEnabled; }
        }
        /*▼====: #001*/
        public bool IsDiagTrmentChanged
        {
            get
            {
                return ucOutPMR.IsDiagTrmentChanged;
            }
            set
            {
                ucOutPMR.IsDiagTrmentChanged = value;
            }
        }
        /*▲====: #001*/
        public bool IsShowEditTinhTrangTheChat
        {
            get
            {
                return ucOutPMR.IsShowEditTinhTrangTheChat;
            }
            set
            {
                ucOutPMR.IsShowEditTinhTrangTheChat = value;
            }
        }
        public ISmallProcedureEdit UCSmallProcedureEdit { get; set; }
        public void ApplySmallProcedure(SmallProcedure aSmallProcedureObj)
        {
            if (UCSmallProcedureEdit != null)
            {
                UCSmallProcedureEdit.ApplySmallProcedure(aSmallProcedureObj);
            }
            if (UCSmallProcedureDesc != null)
            {
                UCSmallProcedureDesc.LoadBaseSection(aSmallProcedureObj.ProcedureDescription);
            }
        }
        public SmallProcedure UpdatedSmallProcedure
        {
            get
            {
                return UCSmallProcedureEdit != null ? UCSmallProcedureEdit.UpdatedSmallProcedure : null;
            }
        }
        public SmallProcedure SmallProcedureObj
        {
            get
            {
                return UCSmallProcedureEdit != null ? UCSmallProcedureEdit.SmallProcedureObj : null;
            }
        }
        public bool IsVisibility
        {
            get
            {
                return UCSmallProcedureEdit.IsVisibility;
            }
            set
            {
                UCSmallProcedureEdit.IsVisibility = value;
            }
        }
        public bool IsVisibilitySkip
        {
            get
            {
                return UCSmallProcedureEdit.IsVisibilitySkip;
            }
            set
            {
                UCSmallProcedureEdit.IsVisibilitySkip = value;
            }
        }
        public bool FormEditorIsEnabled
        {
            get
            {
                return UCSmallProcedureEdit.FormEditorIsEnabled;
            }
            set
            {
                UCSmallProcedureEdit.FormEditorIsEnabled = value;
            }
        }
        private IHtmlEditor _UCSmallProcedureDesc;
        public IHtmlEditor UCSmallProcedureDesc
        {
            get
            {
                return _UCSmallProcedureDesc;
            }
            set
            {
                if (_UCSmallProcedureDesc == value)
                {
                    return;
                }
                _UCSmallProcedureDesc = value;
                NotifyOfPropertyChange(() => UCSmallProcedureDesc);
            }
        }
        public string ProcedureDescription
        {
            get
            {
                return UCSmallProcedureDesc == null ? null : UCSmallProcedureDesc.BodyContent;
            }
        }
        public string ProcedureDescriptionContent
        {
            get
            {
                return UCSmallProcedureDesc?.BodyContentText;
            }
        }
        private IRegistration_DataStorage _Registration_DataStorage;
        public IRegistration_DataStorage Registration_DataStorage
        {
            get
            {
                return _Registration_DataStorage;
            }
            set
            {
                if (_Registration_DataStorage == value)
                {
                    return;
                }
                _Registration_DataStorage = value;
                NotifyOfPropertyChange(() => Registration_DataStorage);
                ucOutPMR.Registration_DataStorage = Registration_DataStorage;
                ucOutPMRs.Registration_DataStorage = Registration_DataStorage;
                UCHeaderInfoPMR.Registration_DataStorage = Registration_DataStorage;
                UCPtRegDetailInfo.Registration_DataStorage = Registration_DataStorage;
                UCSmallProcedureDesc.Registration_DataStorage = Registration_DataStorage;
                UCSmallProcedureEdit.Registration_DataStorage = Registration_DataStorage;
            }
        }
        public void NotifyViewDataChanged()
        {
            ucOutPMR.NotifyViewDataChanged();
        }
        public bool IsOutPtTreatmentProgram
        {
            get
            {
                return ucOutPMR != null && ucOutPMR.IsOutPtTreatmentProgram;
            }
            set
            {
                if (ucOutPMR == null)
                {
                    return;
                }
                ucOutPMR.IsOutPtTreatmentProgram = value;
            }
        }
        public void ICD10Changed(ObservableCollection<DiagnosisIcd10Items> ICD10List)
        {
            ucOutPMR.ICD10Changed(ICD10List);
        }

        //▼===== #003
        public ObservableCollection<DiagnosisICD9Items> refICD9List
        {
            get
            {
                return UCSmallProcedureEdit.refICD9List;
            }
            set
            {
                UCSmallProcedureEdit.refICD9List = value;
            }
        }
        public long Compare2ICD9List()
        {
            return UCSmallProcedureEdit.Compare2ICD9List();
        }

        public ObservableCollection<Resources> SelectedResourceList
        {
            get
            {
                return UCSmallProcedureEdit.SelectedResourceList;
            }
            set
            {
                UCSmallProcedureEdit.SelectedResourceList = value;
            }
        }
        //▲===== #003
    }
}