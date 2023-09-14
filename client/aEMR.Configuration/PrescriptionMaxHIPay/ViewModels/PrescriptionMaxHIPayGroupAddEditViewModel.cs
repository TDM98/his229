using eHCMSLanguage;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using aEMR.Infrastructure;
using aEMR.Infrastructure.Events;
using aEMR.ServiceClient;
using aEMR.ViewContracts.Configuration;
using aEMR.Infrastructure.CachingUtils;
using Castle.Windsor;
using Castle.Core.Logging;
using Caliburn.Micro;
using DataEntities;
using aEMR.ViewContracts;
using eHCMS.Services.Core;
using System.Collections.Generic;
using aEMR.Common;
using System.Linq;
using aEMR.Controls;

namespace aEMR.Configuration.PrescriptionMaxHIPay.ViewModels
{
    [Export(typeof(IPrescriptionMaxHIPayGroupAddEdit)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class PrescriptionMaxHIPayGroupAddEditViewModel : Conductor<object>, IPrescriptionMaxHIPayGroupAddEdit
    {
        private readonly INavigationService _navigationService;
        private readonly ISalePosCaching _salePosCaching;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public PrescriptionMaxHIPayGroupAddEditViewModel(IWindsorContainer container, INavigationService navigationService, ISalePosCaching salePosCaching)
        {
            _navigationService = navigationService;
            _logger = container.Resolve<ILogger>();
            _salePosCaching = salePosCaching;
            GetAllLookupVRegistrationType();
        }
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    NotifyOfPropertyChange(() => IsLoading);
                }
            }
        }

        private ObservableCollection<Lookup> _VRegistrationType;
        public ObservableCollection<Lookup> VRegistrationType
        {
            get { return _VRegistrationType; }
            set
            {
                _VRegistrationType = value;
                NotifyOfPropertyChange(() => VRegistrationType);
            }
        }

        private PrescriptionMaxHIPayGroup _ObjPrescriptionMaxHIPayGroup_Current;
        public PrescriptionMaxHIPayGroup ObjPrescriptionMaxHIPayGroup_Current
        {
            get { return _ObjPrescriptionMaxHIPayGroup_Current; }
            set
            {
                _ObjPrescriptionMaxHIPayGroup_Current = value;
                NotifyOfPropertyChange(() => ObjPrescriptionMaxHIPayGroup_Current);
            }
        }     
       
        private string _TitleForm;
        public string TitleForm
        {
            get { return _TitleForm; }
            set
            {
                _TitleForm = value;
                NotifyOfPropertyChange(() => TitleForm);
            }
        }

        private bool _IsEdit;
        public bool IsEdit
        {
            get { return _IsEdit; }
            set
            {
                _IsEdit = value;
                NotifyOfPropertyChange(() => IsEdit);
            }
        }

        public AxTextBoxFilter.TextBoxFilterType IntNumberFilter
        {
            get
            {
                return AxTextBoxFilter.TextBoxFilterType.Integer;
            }
        }
  
        public void InitializeNewItem()
        {
            GetAllLookupVRegistrationType();
            ObjPrescriptionMaxHIPayGroup_Current = new PrescriptionMaxHIPayGroup();
            ObjPrescriptionMaxHIPayGroup_Current.V_RegistrationType = VRegistrationType.FirstOrDefault();
        }

        private void GetAllLookupVRegistrationType()
        {
            ObservableCollection<Lookup> tmp = Globals.AllLookupValueList.Where(x => x.ObjectTypeID == (long)LookupValues.V_RegistrationType
                && (x.LookupID == (long)AllLookupValues.RegistrationType.NOI_TRU || x.LookupID == (long)AllLookupValues.RegistrationType.NGOAI_TRU
                    || x.LookupID == (long)AllLookupValues.RegistrationType.DIEU_TRI_NGOAI_TRU)).ToObservableCollection();
            
            VRegistrationType = tmp;
        }

        public void btSave()
        {
            PrescriptionMaxHIPayGroupSave();
        }
      
        private void PrescriptionMaxHIPayGroupSave()
        {
            if (ObjPrescriptionMaxHIPayGroup_Current == null)
            {
                return;
            }
            if (ObjPrescriptionMaxHIPayGroup_Current.V_RegistrationType == null || ObjPrescriptionMaxHIPayGroup_Current.V_RegistrationType.LookupID < 1)
            {
                MessageBox.Show(string.Format(eHCMSResources.Z0579_G1_VuiLongChon, eHCMSResources.Z3327_G1_LoaiHinh));
                return;
            }
            if (string.IsNullOrEmpty(ObjPrescriptionMaxHIPayGroup_Current.GroupCode))
            {
                MessageBox.Show(string.Format(eHCMSResources.Z0580_G1_VuiLongNhap, eHCMSResources.Z2114_G1_MaNhom));
                return;
            }
            if (string.IsNullOrEmpty(ObjPrescriptionMaxHIPayGroup_Current.GroupCode))
            {
                MessageBox.Show(string.Format(eHCMSResources.Z0580_G1_VuiLongNhap, eHCMSResources.T0883_G1_TenNhom));
                return;
            }
            PrescriptionMaxHIPayGroup_InsertUpdate(ObjPrescriptionMaxHIPayGroup_Current, true);
        }

        public void btClose()
        {
            TryClose();
        }        
     
        private void PrescriptionMaxHIPayGroup_InsertUpdate(PrescriptionMaxHIPayGroup Obj, bool SaveToDB)
        {
            var t = new Thread(() =>
            {
                IsLoading = true;
                using (var serviceFactory = new ConfigurationManagerServiceClient())
                {
                    var contract = serviceFactory.ServiceInstance;

                    Obj.CreatedStaff = Globals.LoggedUserAccount.Staff;
                    Obj.LastUpdateStaff = Globals.LoggedUserAccount.Staff;

                    contract.BeginPrescriptionMaxHIPayGroup_Save(Obj, Globals.DispatchCallback((asyncResult) =>
                    {
                        try
                        {
                            string Result = "";
                            contract.EndPrescriptionMaxHIPayGroup_Save(out Result, asyncResult);
                            switch (Result)
                            {
                                case "Duplex-Code":
                                    {
                                        MessageBox.Show(string.Format("{0} {1}!", "Đã tồn tại Mã nhóm cùng loại hình", eHCMSResources.A1006_G1_Msg_DungTenKhac), eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                                        break;
                                    }
                                case "Duplex-Name":
                                    {
                                        MessageBox.Show(string.Format("{0} {1}!", "Đã tồn tại Tên nhóm cùng loại hình", eHCMSResources.A1006_G1_Msg_DungTenKhac), eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                                        break;
                                    }
                                case "Update-0":
                                    {
                                        MessageBox.Show(eHCMSResources.A0608_G1_Msg_InfoHChinhFail, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                                        break;
                                    }
                                case "Update-1":
                                    {
                                        Globals.EventAggregator.Publish(new SaveEvent<PrescriptionMaxHIPayGroup> { Result = ObjPrescriptionMaxHIPayGroup_Current });
                                        MessageBox.Show(eHCMSResources.A0468_G1_Msg_InfoLuuOK, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                                        TryClose();
                                        break;
                                    }
                                case "Insert-0":
                                    {
                                        MessageBox.Show(eHCMSResources.A1026_G1_Msg_InfoThemFail, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                                        break;
                                    }
                                case "Insert-1":
                                    {
                                        Globals.EventAggregator.Publish(new SaveEvent<PrescriptionMaxHIPayGroup> { Result = ObjPrescriptionMaxHIPayGroup_Current });
                                        MessageBox.Show(eHCMSResources.A0468_G1_Msg_InfoLuuOK, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                                        TryClose();
                                        break;
                                    }
                            }
                        }
                        catch (Exception ex)
                        {
                            Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                        }
                        finally
                        {
                            //Globals.IsBusy = false;
                            IsLoading = false;
                        }
                    }), null);
                }
            });
            t.Start();
        }
    
    }
}
