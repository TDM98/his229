using System.Windows.Controls;
using Caliburn.Micro;
using System.ComponentModel.Composition;
using aEMR.ViewContracts;
using aEMR.Infrastructure;
using aEMR.Infrastructure.Events;
using System.Windows;
using DataEntities;
using Castle.Windsor;
using aEMR.Common.BaseModel;
using aEMR.Common;
using eHCMSLanguage;
using System;
using System.Linq;
using System.Collections.Generic;
using aEMR.Common.ViewModels;
using aEMR.CommonTasks;
using System.Threading;
using System.Collections.ObjectModel;
using aEMR.ServiceClient;
using aEMR.ServiceClient.Consultation_PCLs;
using System.Text;
using aEMR.ViewContracts.MedicalRecordCover;
using aEMR.Infrastructure.CachingUtils;
using Castle.Core.Logging;

namespace aEMR.Consultation.ViewModels.MedicalRecordCover
{
    [Export(typeof(IMedicalRecordCoverSample1)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class MedicalRecordCoverSample1ViewModel : ViewModelBase, IMedicalRecordCoverSample1
    {
        private readonly INavigationService _navigationService;
        private readonly ISalePosCaching _salePosCaching;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public MedicalRecordCoverSample1ViewModel(IWindsorContainer container, INavigationService navigationService, IEventAggregator eventAggregator, ISalePosCaching salePosCaching)
        {
            _navigationService = navigationService;
            _logger = container.Resolve<ILogger>();
            _salePosCaching = salePosCaching;

            Globals.EventAggregator.Subscribe(this);
            authorization();
        }

        public void InitPatientInfo(PatientRegistration CurPatientRegistration = null)
        {
            CurrentPatientRegistration = new PatientRegistration();
            FRS_Father = null;
            FRS_Mother = null;
            if (CurPatientRegistration != null)
            {
                CurrentPatientRegistration = CurPatientRegistration;
            }
            GetFamilyRelationships_ByPatientID();
            GetMedicalRecordCoverSampleFront();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            InitPatientInfo();
        }

        public void authorization()
        {
            if (!Globals.isAccountCheck)
            {
                return;
            }
        }

        private string _EditTitle = "Lưu";
        public string EditTitle
        {
            get
            {
                return _EditTitle;
            }
            set
            {
                if (_EditTitle == value)
                {
                    return;
                }
                _EditTitle = value;
                NotifyOfPropertyChange(() => EditTitle);
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
            }
        }

        private PatientRegistration _CurrentPatientRegistration;
        public PatientRegistration CurrentPatientRegistration
        {
            get
            {
                return _CurrentPatientRegistration;
            }
            set
            {
                if (_CurrentPatientRegistration == value)
                {
                    return;
                }
                _CurrentPatientRegistration = value;
                NotifyOfPropertyChange(() => CurrentPatientRegistration);
            }
        }

        private FamilyRelationships _FRS_Father;
        public FamilyRelationships FRS_Father
        {
            get
            {
                return _FRS_Father;
            }
            set
            {
                if (_FRS_Father == value)
                {
                    return;
                }
                _FRS_Father = value;
                NotifyOfPropertyChange(() => FRS_Father);
            }
        }

        private FamilyRelationships _FRS_Mother;
        public FamilyRelationships FRS_Mother
        {
            get
            {
                return _FRS_Mother;
            }
            set
            {
                if (_FRS_Mother == value)
                {
                    return;
                }
                _FRS_Mother = value;
                NotifyOfPropertyChange(() => FRS_Mother);
            }
        }

        private ObservableCollection<FamilyRelationships> _CurFamilyRelationships;
        public ObservableCollection<FamilyRelationships> CurFamilyRelationships
        {
            get
            {
                return _CurFamilyRelationships;
            }
            set
            {
                if (_CurFamilyRelationships == value)
                {
                    return;
                }
                _CurFamilyRelationships = value;
                NotifyOfPropertyChange(() => CurFamilyRelationships);
            }
        }

        public void GetFamilyRelationships_ByPatientID()
        {
            if (CurrentPatientRegistration == null || CurrentPatientRegistration.Patient == null || CurrentPatientRegistration.Patient.PatientID <= 0)
            {
                return;
            }
            this.ShowBusyIndicator(eHCMSResources.K2887_G1_DangXuLy);
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new PatientRegistrationServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginGetFamilyRelationships_ByPatientID(CurrentPatientRegistration.Patient.PatientID, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                var result = contract.EndGetFamilyRelationships_ByPatientID(asyncResult);
                                if (result != null && result.Count > 0)
                                {
                                    if (CurrentPatientRegistration.Patient.FRelationships == null)
                                    {
                                        CurrentPatientRegistration.Patient.FRelationships = new ObservableCollection<FamilyRelationships>();
                                    }
                                    foreach (var item in result)
                                    {
                                        CurrentPatientRegistration.Patient.FRelationships.Add(item);
                                    }

                                    FRS_Father = result.Where(x => x.V_FamilyRelationship.LookupID == (long)AllLookupValues.FamilyRelationship.CHA).FirstOrDefault();
                                    FRS_Mother = result.Where(x => x.V_FamilyRelationship.LookupID == (long)AllLookupValues.FamilyRelationship.ME).FirstOrDefault();
                                }

                                if (FRS_Mother == null)
                                {
                                    FRS_Mother = new FamilyRelationships()
                                    {
                                        PatientID = CurrentPatientRegistration.Patient.PatientID,
                                        V_FamilyRelationship = new Lookup()
                                        {
                                            LookupID = (long)AllLookupValues.FamilyRelationship.ME
                                        }
                                    };
                                }
                                if (FRS_Father == null)
                                {
                                    FRS_Father = new FamilyRelationships()
                                    {
                                        PatientID = CurrentPatientRegistration.Patient.PatientID,
                                        V_FamilyRelationship = new Lookup()
                                        {
                                            LookupID = (long)AllLookupValues.FamilyRelationship.CHA
                                        }
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, eHCMSResources.T0432_G1_Error, MessageBoxButton.OK);
                            }
                            finally
                            {
                                this.HideBusyIndicator();
                            }
                        }), null);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, eHCMSResources.T0432_G1_Error, MessageBoxButton.OK);
                    this.HideBusyIndicator();
                }
            });

            t.Start();
        }

        private void GetMedicalRecordCoverSampleFront(int group = 0)
        {
            if (CurrentPatientRegistration == null || CurrentPatientRegistration.AdmissionInfo == null || CurrentPatientRegistration.AdmissionInfo.InPatientAdmDisDetailID <= 0)
            {
                return;
            }
            this.ShowBusyIndicator(eHCMSResources.K2887_G1_DangXuLy);
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new PatientRegistrationServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginGetMedicalRecordCoverSampleFront_ByADDetailID(CurrentPatientRegistration.AdmissionInfo.InPatientAdmDisDetailID, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                var result = contract.EndGetMedicalRecordCoverSampleFront_ByADDetailID(asyncResult);
                                if (result != null)
                                {
                                    switch (group)
                                    {
                                        case 1:
                                            CurrentPatientRegistration.MedRecordCoverSampleFront = result;
                                            break;
                                        default:
                                            CurrentPatientRegistration.MedRecordCoverSampleFront = result;
                                            break;
                                    }
                                }
                                else
                                {
                                    CurrentPatientRegistration.MedRecordCoverSampleFront = new MedicalRecordCoverSampleFront()
                                    {
                                        InPatientAdmDisDetailID = CurrentPatientRegistration.AdmissionInfo.InPatientAdmDisDetailID,
                                        PtRegistrationID = CurrentPatientRegistration.PtRegistrationID,
                                        ServiceRecID = (long)(CurrentPatientRegistration.DiagnosisTreatment != null ? CurrentPatientRegistration.DiagnosisTreatment.ServiceRecID : 0),
                                        V_HospitalTransfer = new Lookup(),
                                        V_Pathology = new Lookup(),
                                        V_ReferralType = new Lookup()
                                    };
                                }

                                if (CurrentPatientRegistration != null && CurrentPatientRegistration.MedRecordCoverSampleFront != null && CurrentPatientRegistration.MedRecordCoverSampleFront.MedicalRecordCoverSampleFrontID > 0)
                                {
                                    EditTitle = "Cập nhật";
                                }
                                else
                                {
                                    EditTitle = "Lưu";
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, eHCMSResources.T0432_G1_Error, MessageBoxButton.OK);
                            }
                            finally
                            {
                                this.HideBusyIndicator();
                            }
                        }), null);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, eHCMSResources.T0432_G1_Error, MessageBoxButton.OK);
                    this.HideBusyIndicator();
                }
            });

            t.Start();
        }

        public void btnSave_Click()
        {
            FRS_Mother.CreatedStaff = Globals.LoggedUserAccount.Staff;
            FRS_Father.CreatedStaff = Globals.LoggedUserAccount.Staff;
            CurrentPatientRegistration.MedRecordCoverSampleFront.CreatedStaff = Globals.LoggedUserAccount.Staff;

            CurFamilyRelationships = new ObservableCollection<FamilyRelationships>();
            CurFamilyRelationships.Add(FRS_Mother);
            CurFamilyRelationships.Add(FRS_Father);
            if (CheckValid())
            {
                EditFamilyRelationshipsXML(CurFamilyRelationships);
                EditMedRecordCoverSampleFront();
            }
        }

        public void btnUpdate_Click()
        {
            btnSave_Click();
        }

        private void EditFamilyRelationshipsXML(ObservableCollection<FamilyRelationships> FRelationships)
        {
            if (CurFamilyRelationships == null)
            {
                return;
            }
            this.ShowBusyIndicator(eHCMSResources.K2887_G1_DangXuLy);
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new PatientRegistrationServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginEditFamilyRelationshipsXML(FRelationships, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                var result = contract.EndEditFamilyRelationshipsXML(asyncResult);
                                //if (result)
                                //{
                                //    MessageBox.Show(eHCMSResources.A0468_G1_Msg_InfoLuuOK, eHCMSResources.T2937_G1_Luu, MessageBoxButton.OK);
                                //}
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, eHCMSResources.T0432_G1_Error, MessageBoxButton.OK);
                            }
                            finally
                            {
                                this.HideBusyIndicator();
                            }
                        }), null);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, eHCMSResources.T0432_G1_Error, MessageBoxButton.OK);
                    this.HideBusyIndicator();
                }
            });

            t.Start();
        }

        private void EditMedRecordCoverSampleFront()
        {
            if (CurrentPatientRegistration == null || CurrentPatientRegistration.MedRecordCoverSampleFront == null)
            {
                return;
            }
            this.ShowBusyIndicator(eHCMSResources.K2887_G1_DangXuLy);
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new PatientRegistrationServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginEditMedRecordCoverSampleFront(CurrentPatientRegistration.MedRecordCoverSampleFront, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                long MedicalRecordCoverSampleFrontID = 0;
                                var result = contract.EndEditMedRecordCoverSampleFront(out MedicalRecordCoverSampleFrontID, asyncResult);
                                if (result)
                                {
                                    EditTitle = "Cập nhật";
                                    CurrentPatientRegistration.MedRecordCoverSampleFront.MedicalRecordCoverSampleFrontID = MedicalRecordCoverSampleFrontID;
                                    GetMedicalRecordCoverSampleFront();
                                    MessageBox.Show(eHCMSResources.A0468_G1_Msg_InfoLuuOK, eHCMSResources.T2937_G1_Luu, MessageBoxButton.OK);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, eHCMSResources.T0432_G1_Error, MessageBoxButton.OK);
                            }
                            finally
                            {
                                this.HideBusyIndicator();
                            }
                        }), null);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, eHCMSResources.T0432_G1_Error, MessageBoxButton.OK);
                    this.HideBusyIndicator();
                }
            });

            t.Start();
        }

        public bool CheckValid()
        {
            string Errors = "";

            if (CurrentPatientRegistration == null || CurrentPatientRegistration.MedRecordCoverSampleFront == null)
            {
                MessageBox.Show("Không có thông tin bệnh án!", eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                return false;
            }

            if (CurrentPatientRegistration.MedRecordCoverSampleFront.V_ReferralType == null || CurrentPatientRegistration.MedRecordCoverSampleFront.V_ReferralType.LookupID <= 0)
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa chọn nơi giới thiệu.";
            }
            if (CurrentPatientRegistration.MedRecordCoverSampleFront.HospitalizedForThisDisease <= 0)
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Vào viện do bệnh này lần thứ.";
            }
            if ((CurrentPatientRegistration.MedRecordCoverSampleFront.V_HospitalTransfer == null || CurrentPatientRegistration.MedRecordCoverSampleFront.V_HospitalTransfer.LookupID <= 0)
                && (CurrentPatientRegistration.AdmissionInfo.V_DischargeType == AllLookupValues.V_DischargeType.CHUYEN_TUYEN_CHUYEN_MON || CurrentPatientRegistration.AdmissionInfo.V_DischargeType == AllLookupValues.V_DischargeType.CHUYEN_VIEN_NGUOI_BENH))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Vào viện do bệnh này lần thứ.";
            }

            if (Errors != "")
            {
                MessageBox.Show(Errors, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                return false;
            }

            return true;
        }

        public string CheckStrNull(string str_check, string out_if_null, string out_if_not_null)
        {
            return string.IsNullOrEmpty(str_check) ? out_if_null : out_if_not_null;
        }
    }
}