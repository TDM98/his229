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
    [Export(typeof(IMedicalRecordCoverSample4)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class MedicalRecordCoverSample4ViewModel : ViewModelBase, IMedicalRecordCoverSample4
    {
        private readonly INavigationService _navigationService;
        private readonly ISalePosCaching _salePosCaching;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public MedicalRecordCoverSample4ViewModel(IWindsorContainer container, INavigationService navigationService, IEventAggregator eventAggregator, ISalePosCaching salePosCaching)
        {
            _navigationService = navigationService;
            _logger = container.Resolve<ILogger>();
            _salePosCaching = salePosCaching;

            Globals.EventAggregator.Subscribe(this);
            authorization();

            aucHoldDeliverStaff = Globals.GetViewModel<IAucHoldConsultDoctor>();
            aucHoldDeliverStaff.StaffCatType = (long)V_StaffCatType.BacSi;
            aucHoldReceiverStaff = Globals.GetViewModel<IAucHoldConsultDoctor>();
            aucHoldReceiverStaff.StaffCatType = (long)V_StaffCatType.BacSi;
            aucHoldConsultDoctor = Globals.GetViewModel<IAucHoldConsultDoctor>();
            aucHoldConsultDoctor.StaffCatType = (long)V_StaffCatType.BacSi;
        }

        public void InitPatientInfo(PatientRegistration CurPatientRegistration = null)
        {
            aucHoldDeliverStaff.setDefault();
            aucHoldReceiverStaff.setDefault();
            aucHoldConsultDoctor.setDefault();

            CurrentPatientRegistration = new PatientRegistration();
            if (CurPatientRegistration != null)
            {
                CurrentPatientRegistration = CurPatientRegistration;
            }
            GetMedicalRecordCoverSample4();
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

        private IAucHoldConsultDoctor _aucHoldDeliverStaff;
        public IAucHoldConsultDoctor aucHoldDeliverStaff
        {
            get
            {
                return _aucHoldDeliverStaff;
            }
            set
            {
                if (_aucHoldDeliverStaff != value)
                {
                    _aucHoldDeliverStaff = value;
                    NotifyOfPropertyChange(() => aucHoldDeliverStaff);
                }
            }
        }

        private IAucHoldConsultDoctor _aucHoldReceiverStaff;
        public IAucHoldConsultDoctor aucHoldReceiverStaff
        {
            get
            {
                return _aucHoldReceiverStaff;
            }
            set
            {
                if (_aucHoldReceiverStaff != value)
                {
                    _aucHoldReceiverStaff = value;
                    NotifyOfPropertyChange(() => aucHoldReceiverStaff);
                }
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

        private void GetMedicalRecordCoverSample4(int group = 0)
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
                        contract.BeginGetMedicalRecordCoverSample4_ByADDetailID(CurrentPatientRegistration.AdmissionInfo.InPatientAdmDisDetailID, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                var result = contract.EndGetMedicalRecordCoverSample4_ByADDetailID(asyncResult);
                                if (result != null)
                                {
                                    switch (group)
                                    {
                                        case 1:
                                            CurrentPatientRegistration.MedRecordCoverSample4 = result;
                                            break;
                                        default:
                                            CurrentPatientRegistration.MedRecordCoverSample4 = result;
                                            break;
                                    }
                                }
                                else
                                {
                                    CurrentPatientRegistration.MedRecordCoverSample4 = new MedicalRecordCoverSample4()
                                    {
                                        InPatientAdmDisDetailID = CurrentPatientRegistration.AdmissionInfo.InPatientAdmDisDetailID,
                                        PtRegistrationID = CurrentPatientRegistration.PtRegistrationID,
                                        ServiceRecID = (long)(CurrentPatientRegistration.DiagnosisTreatment != null ? CurrentPatientRegistration.DiagnosisTreatment.ServiceRecID : 0),
                                        DeliverStaff = new Staff(),
                                        ReceiverStaff = new Staff(),
                                        DoctorStaff = new Staff()
                                    };
                                }

                                if (CurrentPatientRegistration != null && CurrentPatientRegistration.MedRecordCoverSample4 != null)
                                {
                                    if (CurrentPatientRegistration.MedRecordCoverSample4.DeliverStaff != null)
                                    {
                                        aucHoldDeliverStaff.setDefault(CurrentPatientRegistration.MedRecordCoverSample4.DeliverStaff.FullName);
                                        aucHoldDeliverStaff.StaffID = CurrentPatientRegistration.MedRecordCoverSample4.DeliverStaff.StaffID;
                                        aucHoldDeliverStaff.StaffName = CurrentPatientRegistration.MedRecordCoverSample4.DeliverStaff.FullName;
                                    }

                                    if (CurrentPatientRegistration.MedRecordCoverSample4.ReceiverStaff != null)
                                    {
                                        aucHoldReceiverStaff.setDefault(CurrentPatientRegistration.MedRecordCoverSample4.ReceiverStaff.FullName);
                                        aucHoldReceiverStaff.StaffID = CurrentPatientRegistration.MedRecordCoverSample4.ReceiverStaff.StaffID;
                                        aucHoldReceiverStaff.StaffName = CurrentPatientRegistration.MedRecordCoverSample4.ReceiverStaff.FullName;
                                    }

                                    if (CurrentPatientRegistration.MedRecordCoverSample4.DoctorStaff != null)
                                    {
                                        aucHoldConsultDoctor.setDefault(CurrentPatientRegistration.MedRecordCoverSample4.DoctorStaff.FullName);
                                        aucHoldConsultDoctor.StaffID = CurrentPatientRegistration.MedRecordCoverSample4.DoctorStaff.StaffID;
                                        aucHoldConsultDoctor.StaffName = CurrentPatientRegistration.MedRecordCoverSample4.DoctorStaff.FullName;
                                    }

                                    if (CurrentPatientRegistration.MedRecordCoverSample4.MedicalRecordCoverSample4ID > 0)
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
            CurrentPatientRegistration.MedRecordCoverSample4.CreatedStaff = Globals.LoggedUserAccount.Staff;
            if (CurrentPatientRegistration != null && CurrentPatientRegistration.MedRecordCoverSample4 != null && CurrentPatientRegistration.MedRecordCoverSample4.DeliverStaff != null)
            {
                CurrentPatientRegistration.MedRecordCoverSample4.DeliverStaff.StaffID = aucHoldDeliverStaff.StaffID;
                CurrentPatientRegistration.MedRecordCoverSample4.DeliverStaff.FullName = aucHoldDeliverStaff.StaffName;
            }
            if (CurrentPatientRegistration != null && CurrentPatientRegistration.MedRecordCoverSample4 != null && CurrentPatientRegistration.MedRecordCoverSample4.ReceiverStaff != null)
            {
                CurrentPatientRegistration.MedRecordCoverSample4.ReceiverStaff.StaffID = aucHoldReceiverStaff.StaffID;
                CurrentPatientRegistration.MedRecordCoverSample4.ReceiverStaff.FullName = aucHoldReceiverStaff.StaffName;
            }
            if (CurrentPatientRegistration != null && CurrentPatientRegistration.MedRecordCoverSample4 != null && CurrentPatientRegistration.MedRecordCoverSample4.DoctorStaff != null)
            {
                CurrentPatientRegistration.MedRecordCoverSample4.DoctorStaff.StaffID = aucHoldConsultDoctor.StaffID;
                CurrentPatientRegistration.MedRecordCoverSample4.DoctorStaff.FullName = aucHoldConsultDoctor.StaffName;
            }
            if (CheckValid())
            {
                EditMedRecordCoverSample4();
            }
        }

        public void btnUpdate_Click()
        {
            btnSave_Click();
        }

        private void EditMedRecordCoverSample4()
        {
            if (CurrentPatientRegistration == null || CurrentPatientRegistration.MedRecordCoverSample4 == null)
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
                        contract.BeginEditMedRecordCoverSample4(CurrentPatientRegistration.MedRecordCoverSample4, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                long MedicalRecordCoverSample4ID = 0;
                                var result = contract.EndEditMedRecordCoverSample4(out MedicalRecordCoverSample4ID, asyncResult);
                                if (result)
                                {
                                    EditTitle = "Cập nhật";
                                    CurrentPatientRegistration.MedRecordCoverSample4.MedicalRecordCoverSample4ID = MedicalRecordCoverSample4ID;
                                    GetMedicalRecordCoverSample4();
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

            if (CurrentPatientRegistration == null || CurrentPatientRegistration.MedRecordCoverSample4 == null)
            {
                MessageBox.Show("Không có thông tin bệnh án!", eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                return false;
            }
            
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample4.TreatmentDirection))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Hướng điều trị.";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample4.XQuangFilmNum))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Số lượng X-Quang, nếu không có nhập số không (0).";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample4.CTFilmNum))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Số lượng CT, nếu không có nhập số không (0).";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample4.UltrasoundFilmNum))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Số lượng Siêu âm, nếu không có nhập số không (0).";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample4.LaboratoryFilmNum))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Số lượng Xét nghiệm, nếu không có nhập số không (0).";
            }
            if (!string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample4.OrderFilmName) && string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample4.OrderFilmNum))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Số lượng Khác, khi đã nhập Trường Khác (tên).";
            }
            if (CurrentPatientRegistration.MedRecordCoverSample4.TotalFilmNum <= 0)
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Số lượng Tổng bộ hồ sơ.";
            }
            if (CurrentPatientRegistration.MedRecordCoverSample4.DeliverStaff == null || CurrentPatientRegistration.MedRecordCoverSample4.DeliverStaff.StaffID <= 0)
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa chọn người giao hồ sơ.";
            }
            if (CurrentPatientRegistration.MedRecordCoverSample4.ReceiverStaff == null || CurrentPatientRegistration.MedRecordCoverSample4.ReceiverStaff.StaffID <= 0)
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa chọn người  nhận hồ sơ.";
            }
            if (CurrentPatientRegistration.MedRecordCoverSample4.DoctorStaff == null || CurrentPatientRegistration.MedRecordCoverSample4.DoctorStaff.StaffID <= 0)
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa chọn Bác sĩ làm điều trị.";
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