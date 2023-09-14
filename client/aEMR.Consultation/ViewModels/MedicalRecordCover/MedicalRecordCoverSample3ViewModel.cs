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
    [Export(typeof(IMedicalRecordCoverSample3)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class MedicalRecordCoverSample3ViewModel : ViewModelBase, IMedicalRecordCoverSample3
    {
        private readonly INavigationService _navigationService;
        private readonly ISalePosCaching _salePosCaching;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public MedicalRecordCoverSample3ViewModel(IWindsorContainer container, INavigationService navigationService, IEventAggregator eventAggregator, ISalePosCaching salePosCaching)
        {
            _navigationService = navigationService;
            _logger = container.Resolve<ILogger>();
            _salePosCaching = salePosCaching;

            Globals.EventAggregator.Subscribe(this);
            authorization();

            aucHoldConsultDoctor = Globals.GetViewModel<IAucHoldConsultDoctor>();
            aucHoldConsultDoctor.StaffCatType = (long)V_StaffCatType.BacSi;
        }
        
        public void InitPatientInfo(PatientRegistration CurPatientRegistration = null)
        {
            aucHoldConsultDoctor.setDefault();
            CurrentPatientRegistration = new PatientRegistration();
            if (CurPatientRegistration != null)
            {
                CurrentPatientRegistration = CurPatientRegistration;
            }
            GetMedicalRecordCoverSample3();
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

        private IAucHoldConsultDoctor _aucHoldConsultDoctor;
        public IAucHoldConsultDoctor aucHoldConsultDoctor
        {
            get
            {
                return _aucHoldConsultDoctor;
            }
            set
            {
                if (_aucHoldConsultDoctor != value)
                {
                    _aucHoldConsultDoctor = value;
                    NotifyOfPropertyChange(() => aucHoldConsultDoctor);
                }
            }
        }

        private void GetMedicalRecordCoverSample3(int group = 0)
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
                        contract.BeginGetMedicalRecordCoverSample3_ByADDetailID(CurrentPatientRegistration.AdmissionInfo.InPatientAdmDisDetailID, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                var result = contract.EndGetMedicalRecordCoverSample3_ByADDetailID(asyncResult);
                                if (result != null)
                                {
                                    switch (group)
                                    {
                                        case 1:
                                            CurrentPatientRegistration.MedRecordCoverSample3 = result;
                                            break;
                                        default:
                                            CurrentPatientRegistration.MedRecordCoverSample3 = result;
                                            break;
                                    }
                                }
                                else
                                {
                                    CurrentPatientRegistration.MedRecordCoverSample3 = new MedicalRecordCoverSample3()
                                    {
                                        InPatientAdmDisDetailID = CurrentPatientRegistration.AdmissionInfo.InPatientAdmDisDetailID,
                                        PtRegistrationID = CurrentPatientRegistration.PtRegistrationID,
                                        ServiceRecID = (long)(CurrentPatientRegistration.DiagnosisTreatment != null ? CurrentPatientRegistration.DiagnosisTreatment.ServiceRecID : 0),
                                        DoctorStaff = new Staff()
                                    };
                                }

                                if (CurrentPatientRegistration != null && CurrentPatientRegistration.MedRecordCoverSample3 != null)
                                {
                                    if (CurrentPatientRegistration.MedRecordCoverSample3.DoctorStaff != null)
                                    {
                                        aucHoldConsultDoctor.setDefault(CurrentPatientRegistration.MedRecordCoverSample3.DoctorStaff.FullName);
                                        aucHoldConsultDoctor.StaffID = CurrentPatientRegistration.MedRecordCoverSample3.DoctorStaff.StaffID;
                                        aucHoldConsultDoctor.StaffName = CurrentPatientRegistration.MedRecordCoverSample3.DoctorStaff.FullName;
                                    }

                                    if (CurrentPatientRegistration.MedRecordCoverSample3.MedicalRecordCoverSample3ID > 0)
                                    {
                                        EditTitle = "Cập nhật";
                                    }
                                    else
                                    {
                                        EditTitle = "Lưu";
                                    }
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
            CurrentPatientRegistration.MedRecordCoverSample3.CreatedStaff = Globals.LoggedUserAccount.Staff;
            if (CurrentPatientRegistration != null && CurrentPatientRegistration.MedRecordCoverSample3 != null && CurrentPatientRegistration.MedRecordCoverSample3.DoctorStaff != null)
            {
                CurrentPatientRegistration.MedRecordCoverSample3.DoctorStaff.StaffID = aucHoldConsultDoctor.StaffID;
                CurrentPatientRegistration.MedRecordCoverSample3.DoctorStaff.FullName = aucHoldConsultDoctor.StaffName;
            }
            if (CheckValid())
            {
                EditMedRecordCoverSample3();
            }
        }

        public void btnUpdate_Click()
        {
            btnSave_Click();
        }

        private void EditMedRecordCoverSample3()
        {
            if (CurrentPatientRegistration == null || CurrentPatientRegistration.MedRecordCoverSample3 == null)
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
                        contract.BeginEditMedRecordCoverSample3(CurrentPatientRegistration.MedRecordCoverSample3, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                long MedicalRecordCoverSample3ID = 0;
                                var result = contract.EndEditMedRecordCoverSample3(out MedicalRecordCoverSample3ID, asyncResult);
                                if (result)
                                {
                                    EditTitle = "Cập nhật";
                                    CurrentPatientRegistration.MedRecordCoverSample3.MedicalRecordCoverSample3ID = MedicalRecordCoverSample3ID;
                                    GetMedicalRecordCoverSample3();
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

            if (CurrentPatientRegistration == null || CurrentPatientRegistration.MedRecordCoverSample3 == null)
            {
                MessageBox.Show("Không có thông tin bệnh án!", eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                return false;
            }

            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample3.RespiratoryTestResult))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Hô hấp.";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample3.DigestionTestResult))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Tiêu hoá.";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample3.UrologyTestResult))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Thận- Tiết niệu- Sinh dục.";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample3.NeurologyTestResult))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Thần Kinh.";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample3.OrthopaedicsTestResult))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Cơ- Xương- Khớp.";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample3.OtherDiseases))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Tai- Mũi- Họng, Răng-Hàm-Mặt, Mắt, Dinh dưỡng và các bệnh lý khác.";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample3.SummaryOfMedicalRecords))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Tóm tắt bệnh án của bìa hồ sơ.";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample3.Prognosis))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Tiên lượng.";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample3.TreatmentDirection))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Hướng điều trị.";
            }
            if (CurrentPatientRegistration.MedRecordCoverSample3.DoctorStaff == null || CurrentPatientRegistration.MedRecordCoverSample3.DoctorStaff.StaffID <= 0)
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa chọn Bác sĩ làm bệnh án.";
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