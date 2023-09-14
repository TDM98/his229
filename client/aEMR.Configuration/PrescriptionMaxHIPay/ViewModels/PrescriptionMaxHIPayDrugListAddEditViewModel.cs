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
using System.Windows.Controls;
using System.ComponentModel;

namespace aEMR.Configuration.PrescriptionMaxHIPay.ViewModels
{
    [Export(typeof(IPrescriptionMaxHIPayDrugListAddEdit)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class PrescriptionMaxHIPayDrugListAddEditViewModel : Conductor<object>, IPrescriptionMaxHIPayDrugListAddEdit
    {
        private readonly INavigationService _navigationService;
        private readonly ISalePosCaching _salePosCaching;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public PrescriptionMaxHIPayDrugListAddEditViewModel(IWindsorContainer container, INavigationService navigationService, ISalePosCaching salePosCaching)
        {
            _navigationService = navigationService;
            _logger = container.Resolve<ILogger>();
            _salePosCaching = salePosCaching;

            GetAllLookupVRegistrationType();
            RefGenMedProductDetailss = new Common.Collections.PagedSortableCollectionView<RefGenMedProductSimple>();
            RefGenMedProductDetailss.PageSize = Globals.PageSize;
        }

        void RefGenMedProductDetailss_OnRefresh(object sender, RefreshEventArgs e)
        {
            SearchRefDrugGenericDetails_AutoPaging(null, BrandName, RefGenMedProductDetailss.PageIndex, RefGenMedProductDetailss.PageSize);
        }

        public void InitializeNewItem()
        {
            ObjPrescriptionMaxHIPayDrugList_Current = new PrescriptionMaxHIPayDrugList();
            ObjPrescriptionMaxHIPayDrugList_Current.ValidDate = Globals.GetCurServerDateTime();
            ObjPrescriptionMaxHIPayDrugList_Current.PrescriptionMaxHIPayGroup = new PrescriptionMaxHIPayGroup();
            ObjPrescriptionMaxHIPayDrugList_Current.DrugLists = new ObservableCollection<PrescriptionMaxHIPayDrugListLink>();
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
        
        private bool _IsEdit;
        public bool IsEdit
        {
            get { return _IsEdit; }
            set
            {
                if (_IsEdit != value)
                {
                    _IsEdit = value;
                    NotifyOfPropertyChange(() => IsEdit);
                }
            }
        }

        private ObservableCollection<PrescriptionMaxHIPayGroup> _PrescriptionMaxHIPayGroup;
        public ObservableCollection<PrescriptionMaxHIPayGroup> PrescriptionMaxHIPayGroup
        {
            get { return _PrescriptionMaxHIPayGroup; }
            set
            {
                _PrescriptionMaxHIPayGroup = value;
                NotifyOfPropertyChange(() => PrescriptionMaxHIPayGroup);
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

        private Lookup _VRegistrationTypeSelected;
        public Lookup VRegistrationTypeSelected
        {
            get { return _VRegistrationTypeSelected; }
            set
            {
                _VRegistrationTypeSelected = value;
                NotifyOfPropertyChange(() => VRegistrationTypeSelected);
            }
        }

        private long _PrescriptionMaxHIPayGroupID = 0;
        public long PrescriptionMaxHIPayGroupID
        {
            get { return _PrescriptionMaxHIPayGroupID; }
            set
            {
                _PrescriptionMaxHIPayGroupID = value;
                NotifyOfPropertyChange(() => PrescriptionMaxHIPayGroupID);
            }
        }
        
        private void GetAllLookupVRegistrationType()
        {
            ObservableCollection<Lookup> tmp = Globals.AllLookupValueList.Where(x => x.ObjectTypeID == (long)LookupValues.V_RegistrationType
                && (x.LookupID == (long)AllLookupValues.RegistrationType.NOI_TRU || x.LookupID == (long)AllLookupValues.RegistrationType.NGOAI_TRU
                    || x.LookupID == (long)AllLookupValues.RegistrationType.DIEU_TRI_NGOAI_TRU)).ToObservableCollection();

            VRegistrationType = tmp;
            VRegistrationTypeSelected = VRegistrationType.FirstOrDefault();
        }

        public void cboVRegistrationTypeSelectedItemChanged(object selectedItem)
        {
            if (selectedItem != null)
            {
                VRegistrationTypeSelected = (selectedItem as Lookup);
            }

            PrescriptionMaxHIPayGroupID = 0;
            PrescriptionMaxHIPayGroup_GetAll(VRegistrationTypeSelected.LookupID);
        }
        
        public void cboPrescriptionMaxHIPayGroupSelectedItemChanged(object selectedItem)
        {
            if (selectedItem != null)
            {
                PrescriptionMaxHIPayGroupID = (selectedItem as PrescriptionMaxHIPayGroup).PrescriptionMaxHIPayGroupID;
                if (ObjPrescriptionMaxHIPayDrugList_Current != null && ObjPrescriptionMaxHIPayDrugList_Current.PrescriptionMaxHIPayGroup != null)
                {
                    ObjPrescriptionMaxHIPayDrugList_Current.PrescriptionMaxHIPayGroup.PrescriptionMaxHIPayGroupID = PrescriptionMaxHIPayGroupID;
                }
            }
        }

        public void PrescriptionMaxHIPayGroup_GetAll(long V_RegistrationType)
        {
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new ConfigurationManagerServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginPrescriptionMaxHIPayGroup_GetAll(V_RegistrationType, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                var items = contract.EndPrescriptionMaxHIPayGroup_GetAll(asyncResult);
                                if (items != null)
                                {
                                    PrescriptionMaxHIPayGroup = new ObservableCollection<PrescriptionMaxHIPayGroup>(items);
                                }
                                else
                                {
                                    PrescriptionMaxHIPayGroup = new ObservableCollection<PrescriptionMaxHIPayGroup>();
                                }

                                if (IsEdit)
                                {
                                    if (ObjPrescriptionMaxHIPayDrugList_Current != null && ObjPrescriptionMaxHIPayDrugList_Current.PrescriptionMaxHIPayGroup != null && ObjPrescriptionMaxHIPayDrugList_Current.PrescriptionMaxHIPayGroup.PrescriptionMaxHIPayGroupID > 0)
                                    {
                                        PrescriptionMaxHIPayGroupID = ObjPrescriptionMaxHIPayDrugList_Current.PrescriptionMaxHIPayGroup.PrescriptionMaxHIPayGroupID;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
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
                    Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                    this.HideBusyIndicator();
                }
            });
            t.Start();
        }

        public void PrescriptionMaxHIPayDrugListLink_ByID(long PrescriptionMaxHIPayDrugListID)
        {
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new ConfigurationManagerServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginPrescriptionMaxHIPayDrugListLink_ByID(PrescriptionMaxHIPayDrugListID, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                var items = contract.EndPrescriptionMaxHIPayDrugListLink_ByID(asyncResult);
                                if (items != null)
                                {
                                    ObjPrescriptionMaxHIPayDrugListLink = new ObservableCollection<PrescriptionMaxHIPayDrugListLink>(items);
                                }
                                else
                                {
                                    ObjPrescriptionMaxHIPayDrugListLink = new ObservableCollection<PrescriptionMaxHIPayDrugListLink>();
                                }

                                if (ObjPrescriptionMaxHIPayDrugList_Current != null)
                                {
                                    ObjPrescriptionMaxHIPayDrugList_Current.DrugLists = ObjPrescriptionMaxHIPayDrugListLink;
                                }
                            }
                            catch (Exception ex)
                            {
                                Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
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
                    Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                    this.HideBusyIndicator();
                }
            });
            t.Start();
        }

        private PrescriptionMaxHIPayDrugList _ObjPrescriptionMaxHIPayDrugList_Current;
        public PrescriptionMaxHIPayDrugList ObjPrescriptionMaxHIPayDrugList_Current
        {
            get { return _ObjPrescriptionMaxHIPayDrugList_Current; }
            set
            {
                _ObjPrescriptionMaxHIPayDrugList_Current = value;
                NotifyOfPropertyChange(() => ObjPrescriptionMaxHIPayDrugList_Current);
                if (ObjPrescriptionMaxHIPayDrugList_Current != null && ObjPrescriptionMaxHIPayDrugList_Current.PrescriptionMaxHIPayDrugListID > 0)
                {
                    if (ObjPrescriptionMaxHIPayDrugList_Current.PrescriptionMaxHIPayDrugListID > 0)
                    {
                        PrescriptionMaxHIPayDrugListLink_ByID(ObjPrescriptionMaxHIPayDrugList_Current.PrescriptionMaxHIPayDrugListID);
                    }
                    if (ObjPrescriptionMaxHIPayDrugList_Current.PrescriptionMaxHIPayGroup != null && ObjPrescriptionMaxHIPayDrugList_Current.V_RegistrationType != null && ObjPrescriptionMaxHIPayDrugList_Current.V_RegistrationType.LookupID > 0)
                    {
                        PrescriptionMaxHIPayGroup_GetAll(ObjPrescriptionMaxHIPayDrugList_Current.V_RegistrationType.LookupID);
                    }
                }
            }
        }


        #region Auto for Drug Member

        public long V_MedProductType { get; set; } = 11001;

        private string BrandName;

        private Common.Collections.PagedSortableCollectionView<RefGenMedProductSimple> _RefGenMedProductDetailss;
        public Common.Collections.PagedSortableCollectionView<RefGenMedProductSimple> RefGenMedProductDetailss
        {
            get
            {
                return _RefGenMedProductDetailss;
            }
            set
            {
                if (_RefGenMedProductDetailss != value)
                {
                    _RefGenMedProductDetailss = value;
                }
                NotifyOfPropertyChange(() => RefGenMedProductDetailss);
            }
        }
        
        private RefGenMedProductSimple _CurrentRefGenMedProductDetails;
        public RefGenMedProductSimple CurrentRefGenMedProductDetails
        {
            get
            {
                return _CurrentRefGenMedProductDetails;
            }
            set
            {
                if (_CurrentRefGenMedProductDetails != value)
                {
                    _CurrentRefGenMedProductDetails = value;
                }
                NotifyOfPropertyChange(() => CurrentRefGenMedProductDetails);
            }
        }

        private void SearchRefDrugGenericDetails_AutoPaging(bool? IsCode, string Name, int PageIndex, int PageSize)
        {
            int totalCount = 0;
            //Ghi chu: autocomplete khong nen dung indicator vi mat focus 
            //IsLoading = true;
            var t = new Thread(() =>
            {
                using (var serviceFactory = new PharmacyGenMedDeptDetailsServiceClient())
                {
                    var contract = serviceFactory.ServiceInstance;
                    contract.BeginRefGenMedProductDetails_SimpleAutoPaging(IsCode, Name, V_MedProductType, null, PageSize, PageIndex, Globals.DispatchCallback((asyncResult) =>
                    {

                        try
                        {
                            var ListUnits = contract.EndRefGenMedProductDetails_SimpleAutoPaging(out totalCount, asyncResult);
                            if (IsCode.GetValueOrDefault())
                            {
                                if (ListUnits != null && ListUnits.Count > 0)
                                {
                                    CurrentRefGenMedProductDetails = ListUnits.FirstOrDefault();
                                    AddChoose();
                                }
                                else
                                {
                                    CurrentRefGenMedProductDetails = null;

                                    if (au != null)
                                    {
                                        au.Text = "";
                                    }
                                    MessageBox.Show(eHCMSResources.A0752_G1_Msg_InfoKhTimThay);
                                }
                            }
                            else
                            {
                                if (ListUnits != null)
                                {
                                    RefGenMedProductDetailss.Clear();
                                    RefGenMedProductDetailss.TotalItemCount = totalCount;
                                    RefGenMedProductDetailss.ItemCount = totalCount;

                                    //foreach (RefGenMedProductSimple p in ListUnits)
                                    //{
                                    //    RefGenMedProductDetailss.Add(p);
                                    //}
                                    RefGenMedProductDetailss.SourceCollection = ListUnits;
                                    NotifyOfPropertyChange(() => RefGenMedProductDetailss);
                                }
                                au.ItemsSource = RefGenMedProductDetailss;
                                au.PopulateComplete();
                            }
                        }
                        catch (Exception ex)
                        {
                            Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                        }
                        finally
                        {
                            //IsLoading = false;
                        }

                    }), null);

                }

            });

            t.Start();
        }

        AutoCompleteBox au;
        public void AutoCompleteBox_Populating(object sender, PopulatingEventArgs e)
        {
            au = sender as AutoCompleteBox;
            BrandName = e.Parameter;
            RefGenMedProductDetailss.PageIndex = 0;
            SearchRefDrugGenericDetails_AutoPaging(null, e.Parameter, 0, RefGenMedProductDetailss.PageSize);
        }
        #endregion

        public void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            AxTextBox obj = sender as AxTextBox;

            if (obj == null || string.IsNullOrWhiteSpace(obj.Text))
            {
                return;
            }

            string Code = Globals.FormatCode(V_MedProductType, obj.Text);

            if (CurrentRefGenMedProductDetails != null)
            {
                if (CurrentRefGenMedProductDetails.Code.ToLower() != obj.Text.ToLower())
                {
                    SearchRefDrugGenericDetails_AutoPaging(true, Code, 0, RefGenMedProductDetailss.PageSize);
                }
            }
            else
            {
                SearchRefDrugGenericDetails_AutoPaging(true, Code, 0, RefGenMedProductDetailss.PageSize);
            }
        }

        public void AutoDrug_Text_DropDownClosed(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            RefGenMedProductSimple obj = (sender as AutoCompleteBox).SelectedItem as RefGenMedProductSimple;
            if (CurrentRefGenMedProductDetails != null)
            {
                if (CurrentRefGenMedProductDetails.BrandName != obj.BrandName)
                {
                    CurrentRefGenMedProductDetails = obj;
                }
            }
            else
            {
                CurrentRefGenMedProductDetails = obj;
            }
            AddChoose();
        }
        
        private ObservableCollection<PrescriptionMaxHIPayDrugListLink> _ObjPrescriptionMaxHIPayDrugListLink;
        public ObservableCollection<PrescriptionMaxHIPayDrugListLink> ObjPrescriptionMaxHIPayDrugListLink
        {
            get { return _ObjPrescriptionMaxHIPayDrugListLink; }
            set
            {
                _ObjPrescriptionMaxHIPayDrugListLink = value;
                NotifyOfPropertyChange(() => ObjPrescriptionMaxHIPayDrugListLink);
            }
        }
        
        public void hplDelete_Click(object selectedItem)
        {
            if (selectedItem != null)
            {
                ObjPrescriptionMaxHIPayDrugListLink.Remove((selectedItem as PrescriptionMaxHIPayDrugListLink));
            }
        }
                
        public void AddChoose()
        {
            if (CurrentRefGenMedProductDetails == null)
            {
                return;
            }
            if (ObjPrescriptionMaxHIPayDrugListLink == null)
            {
                ObjPrescriptionMaxHIPayDrugListLink = new ObservableCollection<PrescriptionMaxHIPayDrugListLink>();
            }
            foreach (var item in ObjPrescriptionMaxHIPayDrugListLink)
            {
                if (item.Code == CurrentRefGenMedProductDetails.Code)
                {
                    return;
                }
            }
            if (ObjPrescriptionMaxHIPayDrugList_Current == null)
            {
                ObjPrescriptionMaxHIPayDrugList_Current = new PrescriptionMaxHIPayDrugList();
            }

            ObjPrescriptionMaxHIPayDrugListLink.Add(new PrescriptionMaxHIPayDrugListLink
            {
                PrescriptionMaxHIPayDrugListID = ObjPrescriptionMaxHIPayDrugList_Current.PrescriptionMaxHIPayDrugListID,
                GenMedProductID = CurrentRefGenMedProductDetails.GenMedProductID,
                Code = CurrentRefGenMedProductDetails.Code,
                BrandName = CurrentRefGenMedProductDetails.BrandName
            });

            ObjPrescriptionMaxHIPayDrugList_Current.DrugLists = ObjPrescriptionMaxHIPayDrugListLink;
        }

        public void btSave()
        {
            bool Result = false;
            //Globals.EventAggregator.Publish(new BusyEvent { IsBusy = true, Message = "Đang Lưu..." });
            this.ShowBusyIndicator(eHCMSResources.Z0172_G1_DangLuuDLieu);
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new ConfigurationManagerServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;

                        contract.BeginEditPrescriptionMaxHIPayDrugList(ObjPrescriptionMaxHIPayDrugList_Current, Globals.LoggedUserAccount.Staff.StaffID, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                Result = contract.EndEditPrescriptionMaxHIPayDrugList(asyncResult);
                                if (Result)
                                {
                                    Globals.EventAggregator.Publish(new SaveEvent<PrescriptionMaxHIPayDrugList> { Result = ObjPrescriptionMaxHIPayDrugList_Current });
                                    TryClose();
                                    MessageBox.Show(eHCMSResources.Z0655_G1_DaGhi, eHCMSResources.A0464_G1_Msg_Ghi, MessageBoxButton.OK);
                                }
                                else
                                {
                                    MessageBox.Show(eHCMSResources.Z0654_G1_GhiKgThCong, eHCMSResources.A0464_G1_Msg_Ghi, MessageBoxButton.OK);
                                }
                            }
                            catch (Exception ex)
                            {
                                Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
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
                    Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                    this.HideBusyIndicator();
                }
            });
            t.Start();
        }
        
        public bool CheckValidDiseases(object temp)
        {
            Diseases p = temp as Diseases;
            if (p == null)
            {
                return false;
            }
            return p.Validate();
        }
        public bool CheckValidChapter(object temp)
        {
            DiseaseChapters p = temp as DiseaseChapters;
            if (p == null)
            {
                return false;
            }
            return p.Validate();
        }
        public bool CheckValidICD(object temp)
        {
            ICD p = temp as ICD;
            if (p == null)
            {
                return false;
            }
            return p.Validate();
        }

        public void btClose()
        {
            TryClose();
        }
    }
}
