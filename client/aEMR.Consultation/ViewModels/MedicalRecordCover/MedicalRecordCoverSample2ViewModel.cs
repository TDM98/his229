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
    [Export(typeof(IMedicalRecordCoverSample2)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class MedicalRecordCoverSample2ViewModel : ViewModelBase, IMedicalRecordCoverSample2
    {
        private readonly INavigationService _navigationService;
        private readonly ISalePosCaching _salePosCaching;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public MedicalRecordCoverSample2ViewModel(IWindsorContainer container, INavigationService navigationService, IEventAggregator eventAggregator, ISalePosCaching salePosCaching)
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
            if (CurPatientRegistration != null)
            {
                CurrentPatientRegistration = CurPatientRegistration;
            }
            GetMedicalRecordCoverSample2();
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
        
        private void GetMedicalRecordCoverSample2(int group = 0)
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
                        contract.BeginGetMedicalRecordCoverSample2_ByADDetailID(CurrentPatientRegistration.AdmissionInfo.InPatientAdmDisDetailID, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                var result = contract.EndGetMedicalRecordCoverSample2_ByADDetailID(asyncResult);
                                if (result != null)
                                {
                                    switch (group)
                                    {
                                        case 1:
                                            CurrentPatientRegistration.MedRecordCoverSample2 = result;
                                            break;
                                        default:
                                            CurrentPatientRegistration.MedRecordCoverSample2 = result;
                                            break;
                                    }
                                }
                                else
                                {
                                    CurrentPatientRegistration.MedRecordCoverSample2 = new MedicalRecordCoverSample2()
                                    {
                                        InPatientAdmDisDetailID = CurrentPatientRegistration.AdmissionInfo.InPatientAdmDisDetailID,
                                        PtRegistrationID = CurrentPatientRegistration.PtRegistrationID,
                                        ServiceRecID = (long)(CurrentPatientRegistration.DiagnosisTreatment != null ? CurrentPatientRegistration.DiagnosisTreatment.ServiceRecID : 0),
                                        V_ConditionAtBirth = new Lookup(),
                                        V_Alimentation = new Lookup(),
                                        V_TakeCare = new Lookup()
                                    };
                                }

                                if (CurrentPatientRegistration != null && CurrentPatientRegistration.MedRecordCoverSample2 != null && CurrentPatientRegistration.MedRecordCoverSample2.MedicalRecordCoverSample2ID > 0)
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
            CurrentPatientRegistration.MedRecordCoverSample2.CreatedStaff = Globals.LoggedUserAccount.Staff;
            if (CheckValid())
            {
                EditMedRecordCoverSample2();
            }
        }

        public void btnUpdate_Click()
        {
            btnSave_Click();
        }
        
        private void EditMedRecordCoverSample2()
        {
            if (CurrentPatientRegistration == null || CurrentPatientRegistration.MedRecordCoverSample2 == null)
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
                        contract.BeginEditMedRecordCoverSample2(CurrentPatientRegistration.MedRecordCoverSample2, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                long MedicalRecordCoverSample2ID = 0;
                                var result = contract.EndEditMedRecordCoverSample2(out MedicalRecordCoverSample2ID, asyncResult);
                                if (result)
                                {
                                    EditTitle = "Cập nhật";
                                    CurrentPatientRegistration.MedRecordCoverSample2.MedicalRecordCoverSample2ID = MedicalRecordCoverSample2ID;
                                    GetMedicalRecordCoverSample2();
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

            if (CurrentPatientRegistration == null || CurrentPatientRegistration.MedRecordCoverSample2 == null)
            {
                MessageBox.Show("Không có thông tin bệnh án!", eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                return false;
            }

            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample2.ReasonHospitalStay))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập lý do vào viện.";
            }
            if (CurrentPatientRegistration.MedRecordCoverSample2.V_ConditionAtBirth == null || CurrentPatientRegistration.MedRecordCoverSample2.V_ConditionAtBirth.LookupID <= 0)
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa chọn Tình trạng khi sinh.";
            }
            if (CurrentPatientRegistration.MedRecordCoverSample2.Weight <= 0)
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập cân nặng.";
            }
            if (CurrentPatientRegistration.MedRecordCoverSample2.IsBirthDefects && string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample2.NoteBirthDefects))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Cụ thể tật bẩm sinh.";
            }
            if (CurrentPatientRegistration.MedRecordCoverSample2.IsVaccinated_Other && string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample2.Vaccinated_Other))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập cụ thể những bệnh khác được tiêm chủng khác khi đã chọn tiêm chủng khác.";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample2.FullBodyExamination))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Toàn thân.";
            }
            if (string.IsNullOrEmpty(CurrentPatientRegistration.MedRecordCoverSample2.CirculatoryExamination))
            {
                Errors += CheckStrNull(Errors, " - ", "\n - ") + "Chưa nhập Tuần hoàn.";
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