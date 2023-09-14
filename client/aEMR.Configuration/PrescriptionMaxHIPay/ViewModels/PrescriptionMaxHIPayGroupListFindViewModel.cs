using eHCMSLanguage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using aEMR.DataContracts;
using aEMR.Infrastructure;
using aEMR.Infrastructure.Events;
using aEMR.ServiceClient;
using aEMR.ViewContracts.Configuration;
using Caliburn.Micro;
using DataEntities;
using aEMR.Common;
using aEMR.Common.Collections;
using aEMR.Infrastructure.CachingUtils;
using Castle.Windsor;
using Castle.Core.Logging;
using aEMR.ViewContracts;
using System.Windows.Controls;
using aEMR.Common.BaseModel;
using System.Linq;
using aEMR.Common.Utilities;
using ObjectCopier = aEMR.Common.ObjectCopier;
using aEMR.Common.ExportExcel;
using Microsoft.Win32;
using System.IO;
//using OfficeOpenXml;
/*
* #001 20180921 TNHX: Apply BusyIndicator, refactor code
* 20230622 #002 DatTB:
* + Thêm filter theo mã ICD đã lưu của nhóm bệnh
* + Thêm function chỉnh sửa ICD của nhóm bệnh
* + Thêm function xuất excel ICD của nhóm bệnh
*/
namespace aEMR.Configuration.PrescriptionMaxHIPay.ViewModels
{
    [Export(typeof(IPrescriptionMaxHIPayGroupListFind)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class PrescriptionMaxHIPayGroupListFindViewModel : ViewModelBase, IPrescriptionMaxHIPayGroupListFind
        , IHandle<SaveEvent<PrescriptionMaxHIPayGroup>>
        , IHandle<SaveEvent<PrescriptionMaxHIPayLinkICD>>
        , IHandle<SaveEvent<PrescriptionMaxHIPayDrugList>>
    {
        protected override void OnActivate()
        {
            authorization();
            base.OnActivate();
        }
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
        }

        private readonly INavigationService _navigationService;
        private readonly ISalePosCaching _salePosCaching;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public PrescriptionMaxHIPayGroupListFindViewModel(IWindsorContainer container, INavigationService navigationService, IEventAggregator eventAggregator, ISalePosCaching salePosCaching)
        {
            _navigationService = navigationService;
            _logger = container.Resolve<ILogger>();
            _salePosCaching = salePosCaching;

            eventAggregator.Subscribe(this);
            authorization();
            Globals.EventAggregator.Subscribe(this);
            InitData();
        }

        #region Properties
        private Visibility _hplAddNewVisible = Visibility.Visible;
        public Visibility hplAddNewVisible
        {
            get { return _hplAddNewVisible; }
            set
            {
                _hplAddNewVisible = value;
                NotifyOfPropertyChange(() => hplAddNewVisible);
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
        
        private ObservableCollection<PrescriptionMaxHIPayGroup> _PrescriptionMaxHIPayDrugListGroup;
        public ObservableCollection<PrescriptionMaxHIPayGroup> PrescriptionMaxHIPayDrugListGroup
        {
            get { return _PrescriptionMaxHIPayDrugListGroup; }
            set
            {
                _PrescriptionMaxHIPayDrugListGroup = value;
                NotifyOfPropertyChange(() => PrescriptionMaxHIPayDrugListGroup);
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

        private Lookup _VRegistrationTypeLinkICDSelected;
        public Lookup VRegistrationTypeLinkICDSelected
        {
            get { return _VRegistrationTypeLinkICDSelected; }
            set
            {
                _VRegistrationTypeLinkICDSelected = value;
                NotifyOfPropertyChange(() => VRegistrationTypeLinkICDSelected);
            }
        }

        private Lookup _VRegistrationTypeDrugListSelected;
        public Lookup VRegistrationTypeDrugListSelected
        {
            get { return _VRegistrationTypeDrugListSelected; }
            set
            {
                _VRegistrationTypeDrugListSelected = value;
                NotifyOfPropertyChange(() => VRegistrationTypeDrugListSelected);
            }
        }

        private string _SearchGroupName;
        public string SearchGroupName
        {
            get
            {
                return _SearchGroupName;
            }
            set
            {
                _SearchGroupName = value;
                NotifyOfPropertyChange(() => SearchGroupName);
            }
        }

        private PagedSortableCollectionView<PrescriptionMaxHIPayGroup> _ObjPrescriptionMaxHIPayGroup_Paging;
        public PagedSortableCollectionView<PrescriptionMaxHIPayGroup> ObjPrescriptionMaxHIPayGroup_Paging
        {
            get { return _ObjPrescriptionMaxHIPayGroup_Paging; }
            set
            {
                _ObjPrescriptionMaxHIPayGroup_Paging = value;
                NotifyOfPropertyChange(() => ObjPrescriptionMaxHIPayGroup_Paging);
            }
        }

        private PagedSortableCollectionView<PrescriptionMaxHIPayDrugList> _ObjPrescriptionMaxHIPayDrugList_Paging;
        public PagedSortableCollectionView<PrescriptionMaxHIPayDrugList> ObjPrescriptionMaxHIPayDrugList_Paging
        {
            get { return _ObjPrescriptionMaxHIPayDrugList_Paging; }
            set
            {
                _ObjPrescriptionMaxHIPayDrugList_Paging = value;
                NotifyOfPropertyChange(() => ObjPrescriptionMaxHIPayDrugList_Paging);
            }
        }

        private void InitData()
        {
            GetAllLookupVRegistrationType();

            ObjPrescriptionMaxHIPayGroup_Paging = new PagedSortableCollectionView<PrescriptionMaxHIPayGroup>();
            ObjPrescriptionMaxHIPayGroup_Paging.OnRefresh += new EventHandler<RefreshEventArgs>(ObjPrescriptionMaxHIPayGroup_Paging_OnRefresh);
            ObjPrescriptionMaxHIPayGroup_Paging.PageIndex = 0;
            GetPrescriptionMaxHIPayGroup_Paging(0, ObjPrescriptionMaxHIPayGroup_Paging.PageSize, true, 2);
            DiseaseChapters = new ObservableCollection<DiseaseChapters>();
            DiseaseChapters_GetAll();
            PrescriptionMaxHIPayGroup = new ObservableCollection<PrescriptionMaxHIPayGroup>();
            PrescriptionMaxHIPayDrugListGroup = new ObservableCollection<PrescriptionMaxHIPayGroup>();

            ObjSearchICD_Paging = new PagedSortableCollectionView<ICD>();
            ObjSearchICD_Paging.OnRefresh += new EventHandler<RefreshEventArgs>(ObjSearchICD_Paging_OnRefresh);
            ObjSearchICD_Paging = new PagedSortableCollectionView<ICD>();
            ObjSearchICD_Paging.OnRefresh += new EventHandler<RefreshEventArgs>(ObjSearchICD_Paging_OnRefresh);
            ObjPrescriptionMaxHIPayLinkICD_ByID_Paging = new PagedSortableCollectionView<PrescriptionMaxHIPayLinkICD>();
            ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.OnRefresh += new EventHandler<RefreshEventArgs>(ObjPrescriptionMaxHIPayLinkICD_ByID_Paging_OnRefresh);

            ObjPrescriptionMaxHIPayDrugList_Paging = new PagedSortableCollectionView<PrescriptionMaxHIPayDrugList>();
            ObjPrescriptionMaxHIPayDrugList_Paging.OnRefresh += new EventHandler<RefreshEventArgs>(ObjPrescriptionMaxHIPayDrugList_Paging_OnRefresh);
            ObjPrescriptionMaxHIPayDrugList_Paging.PageIndex = 0;
            GetPrescriptionMaxHIPayDrugList_Paging(0, ObjPrescriptionMaxHIPayDrugList_Paging.PageSize, true, 2);
        }
        
        void ObjPrescriptionMaxHIPayGroup_Paging_OnRefresh(object sender, RefreshEventArgs e)
        {
            GetPrescriptionMaxHIPayGroup_Paging(ObjPrescriptionMaxHIPayGroup_Paging.PageIndex,
                            ObjPrescriptionMaxHIPayGroup_Paging.PageSize, false, 2);
        }
        void ObjPrescriptionMaxHIPayLinkICD_ByID_Paging_OnRefresh(object sender, RefreshEventArgs e)
        {
            GetPrescriptionMaxHIPayLinkICD_ByID_Paging(ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.PageIndex,
                            ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.PageSize, false, true);
        }
        void ObjSearchICD_Paging_OnRefresh(object sender, RefreshEventArgs e)
        {
            SearchICD_Paging(ObjSearchICD_Paging.PageIndex,
                            ObjSearchICD_Paging.PageSize, false);
        }

        void ObjPrescriptionMaxHIPayDrugList_Paging_OnRefresh(object sender, RefreshEventArgs e)
        {
            GetPrescriptionMaxHIPayDrugList_Paging(ObjPrescriptionMaxHIPayDrugList_Paging.PageIndex,
                            ObjPrescriptionMaxHIPayDrugList_Paging.PageSize, false, 2);
        }

        private void GetAllLookupVRegistrationType()
        {
            ObservableCollection<Lookup> tmp = Globals.AllLookupValueList.Where(x => x.ObjectTypeID == (long)LookupValues.V_RegistrationType
                && (x.LookupID == (long)AllLookupValues.RegistrationType.NOI_TRU || x.LookupID == (long)AllLookupValues.RegistrationType.NGOAI_TRU
                    || x.LookupID == (long)AllLookupValues.RegistrationType.DIEU_TRI_NGOAI_TRU)).ToObservableCollection();

            Lookup all = new Lookup()
            {
                LookupID = 0,
                ObjectValue = "Tất cả"
            };
            tmp.Insert(0, all);
            VRegistrationType = tmp;
            VRegistrationTypeSelected = VRegistrationType.FirstOrDefault();
        }

        public void btSearchPrescriptionMaxHIPayGroup()
        {
            GetPrescriptionMaxHIPayGroup_Paging(ObjPrescriptionMaxHIPayGroup_Paging.PageIndex, ObjPrescriptionMaxHIPayGroup_Paging.PageSize, true, 2);
        }

        public void btSearchPrescriptionMaxHIPayDrugList()
        {
            GetPrescriptionMaxHIPayDrugList_Paging(ObjPrescriptionMaxHIPayDrugList_Paging.PageIndex, ObjPrescriptionMaxHIPayDrugList_Paging.PageSize, true, 2);
        }

        public void hplAddNewPrescriptionMaxHIPayGroup_Click()
        {
            Action<IPrescriptionMaxHIPayGroupAddEdit> onInitDlg = (typeInfo) =>
            {
                typeInfo.TitleForm = "Thêm Mới Nhóm Chi Phí";
                typeInfo.InitializeNewItem();
            };
            GlobalsNAV.ShowDialog(onInitDlg);
        }

        public void hplEditPrescriptionMaxHIPayGroup_Click(object selectedItem)
        {
            if (selectedItem != null)
            {
                Action<IPrescriptionMaxHIPayGroupAddEdit> onInitDlg = (typeInfo) =>
                {
                    typeInfo.ObjPrescriptionMaxHIPayGroup_Current = ObjectCopier.DeepCopy((selectedItem as PrescriptionMaxHIPayGroup));
                    typeInfo.TitleForm = "Hiệu Chỉnh (" + (selectedItem as PrescriptionMaxHIPayGroup).GroupName.Trim() + ")";
                    typeInfo.IsEdit = true;
                };
                GlobalsNAV.ShowDialog<IPrescriptionMaxHIPayGroupAddEdit>(onInitDlg);
            }
        }
        
        public void hplAddNewPrescriptionMaxHIPayDrugList_Click()
        {
            Action<IPrescriptionMaxHIPayDrugListAddEdit> onInitDlg = (typeInfo) =>
            {
                typeInfo.TitleForm = "Thêm Mới Giá Trần Thuốc";
                typeInfo.InitializeNewItem();
            };
            GlobalsNAV.ShowDialog(onInitDlg);
        }
               
        public void hplEditPrescriptionMaxHIPayDrugList_Click(object selectedItem)
        {
            if (selectedItem != null)
            {
                Action<IPrescriptionMaxHIPayDrugListAddEdit> onInitDlg = (typeInfo) =>
                {
                    typeInfo.ObjPrescriptionMaxHIPayDrugList_Current = ObjectCopier.DeepCopy((selectedItem as PrescriptionMaxHIPayDrugList));
                    typeInfo.TitleForm = "Hiệu Chỉnh giá trần thuốc";
                    typeInfo.IsEdit = true;
                };
                GlobalsNAV.ShowDialog<IPrescriptionMaxHIPayDrugListAddEdit>(onInitDlg);
            }
        }

        private void GetPrescriptionMaxHIPayGroup_Paging(int PageIndex, int PageSize, bool CountTotal, int FilterDeleted)
        {
            this.DlgShowBusyIndicator("Danh sách nhóm chi phí");
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new ConfigurationManagerServiceClient())
                    {
                        var client = serviceFactory.ServiceInstance;
                        client.BeginGetPrescriptionMaxHIPayGroup_Paging(VRegistrationTypeSelected.LookupID, SearchGroupName, FilterDeleted, PageIndex, PageSize, "", CountTotal, Globals.DispatchCallback((asyncResult) =>
                        {
                            int Total = 0;
                            IList<PrescriptionMaxHIPayGroup> allItems = null;
                            bool bOK = false;
                            try
                            {
                                allItems = client.EndGetPrescriptionMaxHIPayGroup_Paging(out Total, asyncResult);
                                bOK = true;
                            }
                            catch (FaultException<AxException> fault)
                            {
                                ClientLoggerHelper.LogInfo(fault.ToString());
                            }
                            catch (Exception ex)
                            {
                                ClientLoggerHelper.LogInfo(ex.ToString());
                            }
                            finally
                            {
                                this.DlgHideBusyIndicator();
                            }

                            ObjPrescriptionMaxHIPayGroup_Paging.Clear();

                            if (bOK)
                            {
                                if (CountTotal)
                                {
                                    ObjPrescriptionMaxHIPayGroup_Paging.TotalItemCount = Total;
                                }
                                if (allItems != null)
                                {
                                    foreach (var item in allItems)
                                    {
                                        ObjPrescriptionMaxHIPayGroup_Paging.Add(item);
                                    }
                                }
                            }
                        }), null);
                    }
                }
                catch (Exception ex)
                {
                    ClientLoggerHelper.LogInfo(ex.ToString());
                    this.DlgHideBusyIndicator();
                }
            });
            t.Start();
        }

        private void GetPrescriptionMaxHIPayDrugList_Paging(int PageIndex, int PageSize, bool CountTotal, int FilterDeleted)
        {
            this.DlgShowBusyIndicator("Danh sách giá trần thuốc");
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new ConfigurationManagerServiceClient())
                    {
                        var client = serviceFactory.ServiceInstance;
                        client.BeginGetPrescriptionMaxHIPayDrugList_Paging(VRegistrationTypeSelected.LookupID, PrescriptionMaxHIPayDrugListGroupID, FilterDeleted, PageIndex, PageSize, "", CountTotal, Globals.DispatchCallback((asyncResult) =>
                        {
                            int Total = 0;
                            IList<PrescriptionMaxHIPayDrugList> allItems = null;
                            bool bOK = false;
                            try
                            {
                                allItems = client.EndGetPrescriptionMaxHIPayDrugList_Paging(out Total, asyncResult);
                                bOK = true;
                            }
                            catch (FaultException<AxException> fault)
                            {
                                ClientLoggerHelper.LogInfo(fault.ToString());
                            }
                            catch (Exception ex)
                            {
                                ClientLoggerHelper.LogInfo(ex.ToString());
                            }
                            finally
                            {
                                this.DlgHideBusyIndicator();
                            }

                            ObjPrescriptionMaxHIPayDrugList_Paging.Clear();

                            if (bOK)
                            {
                                if (CountTotal)
                                {
                                    ObjPrescriptionMaxHIPayDrugList_Paging.TotalItemCount = Total;
                                }
                                if (allItems != null)
                                {
                                    foreach (var item in allItems)
                                    {
                                        ObjPrescriptionMaxHIPayDrugList_Paging.Add(item);
                                    }
                                }
                            }
                        }), null);
                    }
                }
                catch (Exception ex)
                {
                    ClientLoggerHelper.LogInfo(ex.ToString());
                    this.DlgHideBusyIndicator();
                }
            });
            t.Start();
        }

        public void cboVRegistrationTypeSelectedItemChanged(object selectedItem)
        {
            if (selectedItem != null)
            {
                VRegistrationTypeSelected = (selectedItem as Lookup);
            }
            GetPrescriptionMaxHIPayGroup_Paging(ObjPrescriptionMaxHIPayGroup_Paging.PageIndex, ObjPrescriptionMaxHIPayGroup_Paging.PageSize, true, 2);
        }

        public void cboVRegistrationTypeLinkICDSelectedItemChanged(object selectedItem)
        {
            if (selectedItem != null)
            {
                VRegistrationTypeLinkICDSelected = (selectedItem as Lookup);
            }
            PrescriptionMaxHIPayGroupID = 0;
            PrescriptionMaxHIPayGroup_GetAll(VRegistrationTypeLinkICDSelected.LookupID);
        }

        public void cboVRegistrationTypeDrugListSelectedItemChanged(object selectedItem)
        {
            if (selectedItem != null)
            {
                VRegistrationTypeDrugListSelected = (selectedItem as Lookup);
            }
            PrescriptionMaxHIPayDrugListGroupID = 0;
            PrescriptionMaxHIPayDrugListGroup_GetAll(VRegistrationTypeDrugListSelected.LookupID);
        }

        #region Handle
        public void Handle(SaveEvent<PrescriptionMaxHIPayGroup> message)
        {
            GetPrescriptionMaxHIPayGroup_Paging(ObjPrescriptionMaxHIPayGroup_Paging.PageIndex, ObjPrescriptionMaxHIPayGroup_Paging.PageSize, true, 2);
        }

        public void Handle(SaveEvent<PrescriptionMaxHIPayDrugList> message)
        {
            GetPrescriptionMaxHIPayDrugList_Paging(0, ObjPrescriptionMaxHIPayDrugList_Paging.PageSize, true, 2);
        }
        #endregion

        private ObservableCollection<DiseaseChapters> _DiseaseChapters;
        public ObservableCollection<DiseaseChapters> DiseaseChapters
        {
            get { return _DiseaseChapters; }
            set
            {
                _DiseaseChapters = value;
                NotifyOfPropertyChange(() => DiseaseChapters);
            }
        }
        private int _DiseaseChapters_ID = 0;
        public int DiseaseChapters_ID
        {
            get { return _DiseaseChapters_ID; }
            set
            {
                _DiseaseChapters_ID = value;
                NotifyOfPropertyChange(() => DiseaseChapters_ID);
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
        private long _PrescriptionMaxHIPayDrugListGroupID = 0;
        public long PrescriptionMaxHIPayDrugListGroupID
        {
            get { return _PrescriptionMaxHIPayDrugListGroupID; }
            set
            {
                _PrescriptionMaxHIPayDrugListGroupID = value;
                NotifyOfPropertyChange(() => PrescriptionMaxHIPayDrugListGroupID);
            }
        }
        private ObservableCollection<Diseases> _Diseases;
        public ObservableCollection<Diseases> Diseases
        {
            get { return _Diseases; }
            set
            {
                _Diseases = value;
                NotifyOfPropertyChange(() => Diseases);
            }
        }
        private ObservableCollection<ICD> _ObjICD_ADD;
        public ObservableCollection<ICD> ObjICD_ADD
        {
            get { return _ObjICD_ADD; }
            set
            {
                _ObjICD_ADD = value;
                NotifyOfPropertyChange(() => ObjICD_ADD);
            }
        }
        
        private long _Disease_ID = 0;
        public long Disease_ID
        {
            get { return _Disease_ID; }
            set
            {
                _Disease_ID = value;
                NotifyOfPropertyChange(() => Disease_ID);
            }
        }
        private string _ICD10Code = "";
        public string ICD10Code
        {
            get { return _ICD10Code; }
            set
            {
                _ICD10Code = value;
                NotifyOfPropertyChange(() => ICD10Code);
            }
        }
     
        public Dictionary<long, ICD> _dicICD;
        public Dictionary<long, ICD> dicICD
        {
            get
            {
                return _dicICD;
            }
            set
            {
                _dicICD = value;
                NotifyOfPropertyChange(() => dicICD);

            }
        }

        private object _ICD_Current;
        public object ICD_Current
        {
            get { return _ICD_Current; }
            set
            {
                _ICD_Current = value;
                NotifyOfPropertyChange(() => ICD_Current);
            }
        }

        private ICD _selectedICD;
        public ICD SelectedICD
        {
            get { return _selectedICD; }
            set
            {
                if (_selectedICD != value)
                {
                    _selectedICD = value;
                    NotifyOfPropertyChange(() => SelectedICD);
                }
            }
        }
        private ICDSearchCriteria _SearchCriteria;
        public ICDSearchCriteria SearchCriteria
        {
            get
            {
                return _SearchCriteria;
            }
            set
            {
                _SearchCriteria = value;
                NotifyOfPropertyChange(() => SearchCriteria);
            }
        }
        private string _SearchName;
        public string SearchName
        {
            get
            {
                return _SearchName;
            }
            set
            {
                _SearchName = value;
                NotifyOfPropertyChange(() => SearchName);
            }
        }
        private bool _IsDeleted = true;
        public bool IsDeleted
        {
            get
            {
                return _IsDeleted;
            }
            set
            {
                _IsDeleted = value;
                NotifyOfPropertyChange(() => IsDeleted);
            }
        }

        private PagedSortableCollectionView<ICD> _ObjSearchICD_Paging;
        public PagedSortableCollectionView<ICD> ObjSearchICD_Paging
        {
            get { return _ObjSearchICD_Paging; }
            set
            {
                _ObjSearchICD_Paging = value;
                NotifyOfPropertyChange(() => ObjSearchICD_Paging);
            }
        }
        private PagedSortableCollectionView<PrescriptionMaxHIPayLinkICD> _ObjPrescriptionMaxHIPayLinkICD_ByID_Paging;
        public PagedSortableCollectionView<PrescriptionMaxHIPayLinkICD> ObjPrescriptionMaxHIPayLinkICD_ByID_Paging
        {
            get { return _ObjPrescriptionMaxHIPayLinkICD_ByID_Paging; }
            set
            {
                _ObjPrescriptionMaxHIPayLinkICD_ByID_Paging = value;
                NotifyOfPropertyChange(() => ObjPrescriptionMaxHIPayLinkICD_ByID_Paging);
            }
        }
        #endregion
       
   
       
        public void authorization()
        {
            if (!Globals.isAccountCheck)
            {
                return;
            }
        }

       
        #region Button
        public void btSearchICD()
        {
            if(DiseaseChapters_ID == 0)
            {
                MessageBox.Show("Chưa chọn chương!");
                return;
            }
            ObjSearchICD_Paging.PageIndex = 0;
            SearchICD_Paging(0, ObjSearchICD_Paging.PageSize, true);
        }

        public void hplDelete_Click(object selectedItem)
        {
            if(selectedItem != null)
            {
                ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.Remove((selectedItem as PrescriptionMaxHIPayLinkICD));
            }
        }
        public void btSaveItems()
        {
            if (PrescriptionMaxHIPayGroupID < 1)
            {
                MessageBox.Show("Chưa chọn Nhóm chi phí", eHCMSResources.T2937_G1_Luu, MessageBoxButton.OK);
                return;
            }
            if (!ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.Any(x=>x.PrescriptionMaxHIPayLinkICDID == 0))
            {
                MessageBox.Show("Không có gì để lưu", eHCMSResources.T2937_G1_Luu, MessageBoxButton.OK);
                return;
            }
            ICD_XMLInsert(Disease_ID, ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.Where(x=>x.PrescriptionMaxHIPayLinkICDID == 0).ToObservableCollection());
        }
        #endregion

        public void cboPrescriptionMaxHIPayGroupSelectedItemChanged(object selectedItem)
        {
            if (selectedItem != null)
            {
                PrescriptionMaxHIPayGroupID = (selectedItem as PrescriptionMaxHIPayGroup).PrescriptionMaxHIPayGroupID;
            }
            GetPrescriptionMaxHIPayLinkICD_ByID_Paging(0, ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.PageSize, true);
        }

        public void cboPrescriptionMaxHIPayDrugListGroupSelectedItemChanged(object selectedItem)
        {
            if (selectedItem != null)
            {
                PrescriptionMaxHIPayDrugListGroupID = (selectedItem as PrescriptionMaxHIPayGroup).PrescriptionMaxHIPayGroupID;
            }
            GetPrescriptionMaxHIPayDrugList_Paging(0, ObjPrescriptionMaxHIPayDrugList_Paging.PageSize, true, 2);
        }

        public void cboDiseaseChaptersSelectedItemChanged(object selectedItem)
        {
            DiseaseChapters_ID = (selectedItem as DiseaseChapters).DiseaseChapterID;
            Disease_ID = 0;
            Diseases_ByChapterID();
            SearchICD_Paging(0, ObjSearchICD_Paging.PageSize, true);
        }
        public void cboDiseasesSelectedItemChanged(object selectedItem)
        {
            if(selectedItem!= null)
            {
                Disease_ID = (selectedItem as Diseases).DiseaseID;
            }
            SearchICD_Paging(0, ObjSearchICD_Paging.PageSize, true);
        }
        public void Diseases_ByChapterID()
        {
            //Globals.EventAggregator.Publish(new BusyEvent { IsBusy = true, Message = "Danh Sách Loại Phòng..." });
            this.ShowBusyIndicator("Danh sách ICD");
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new ConfigurationManagerServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginDiseases_ByChapterID(DiseaseChapters_ID, Globals.DispatchCallback((asyncResult) =>
                         {
                             try
                             {
                                 var items = contract.EndDiseases_ByChapterID(asyncResult);
                                 if (items != null)
                                 {
                                     Diseases = new ObservableCollection<Diseases>(items);
                                     Diseases Default = new Diseases();
                                     Default.DiseaseID = 0;
                                     Default.DiseaseNameVN = string.Format(eHCMSResources.Z0996_G1_Format1, eHCMSResources.T0822_G1_TatCa);
                                     Diseases.Insert(0, Default);
                                 }
                                 else
                                 {
                                     Diseases = new ObservableCollection<Diseases>();
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
        public void GetPrescriptionMaxHIPayLinkICD_ByID_Paging(int PageIndex, int PageSize, bool CountTotal, bool IsRefresh = false)
        {
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new ConfigurationManagerServiceClient())
                    {
                        var client = serviceFactory.ServiceInstance;
                        client.BeginPrescriptionMaxHIPayLinkICD_ByID_Paging(PrescriptionMaxHIPayGroupID, ICD10CodeSaved, PageIndex, PageSize, "", CountTotal, Globals.DispatchCallback((asyncResult) =>
                        {
                            int Total = 0;
                            IList<PrescriptionMaxHIPayLinkICD> allItems = null;
                            bool bOK = false;
                            try
                            {
                                allItems = client.EndPrescriptionMaxHIPayLinkICD_ByID_Paging(out Total, asyncResult);
                                bOK = true;
                            }
                            catch (FaultException<AxException> fault)
                            {
                                ClientLoggerHelper.LogInfo(fault.ToString());
                            }
                            catch (Exception ex)
                            {
                                ClientLoggerHelper.LogInfo(ex.ToString());
                            }
                            finally
                            {
                                this.DlgHideBusyIndicator();
                            }
                            IList<PrescriptionMaxHIPayLinkICD> temp = null;
                            if (IsRefresh && ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.Any(x => x.PrescriptionMaxHIPayLinkICDID == 0))
                            {
                                temp = ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.Where(x => x.PrescriptionMaxHIPayLinkICDID == 0).ToList();
                            }
                            ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.Clear();

                            if (bOK)
                            {
                                if (CountTotal)
                                {
                                    ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.TotalItemCount = Total;
                                }
                                if (allItems != null)
                                {
                                    foreach (var item in allItems)
                                    {
                                        ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.Add(item);
                                    }
                                }
                                if(temp != null && temp.Count> 0)
                                {
                                    foreach (var item in temp)
                                    {
                                        ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.Add(item);
                                    }
                                }
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

        public void DoubleClick(object args)
        {
            EventArgs<object> eventArgs = args as EventArgs<object>;
            ICD_Current = eventArgs.Value as ICD;
            Globals.EventAggregator.Publish(new dgICDListClickSelectionChanged_Event() { ICD_Current = eventArgs.Value });
        }

        public void btAddChoose()
        {
            if (!ObjSearchICD_Paging.Any(x=>x.IsChecked))
            {
                MessageBox.Show("Chưa chọn ICD nào");
            }
            
            foreach (var item in ObjSearchICD_Paging.Where(x => x.IsChecked))
            {
                if (!CheckExistsICD(item))
                {
                    ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.Add(new PrescriptionMaxHIPayLinkICD
                    {
                        PrescriptionMaxHIPayGroupID = PrescriptionMaxHIPayGroupID,
                        IDCode = item.IDCode,
                        ICD10 = item.ICD10Code,
                        DiseaseNameVN = item.DiseaseNameVN,
                        IsActive = item.IsActive,
                        CreatedStaff = Globals.LoggedUserAccount.Staff,
                        CanDelete = true
                    });
                }
            }
        }
        private bool CheckExistsICD(ICD iCD)
        {
            foreach (var item in ObjPrescriptionMaxHIPayLinkICD_ByID_Paging)
            {
                if(item.IDCode == iCD.IDCode)
                {
                    return true;
                }
            }
            return false;
        }
        private void SearchICD_Paging(int PageIndex, int PageSize, bool CountTotal)
        {
            this.ShowBusyIndicator();
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new ConfigurationManagerServiceClient())
                    {
                        var client = serviceFactory.ServiceInstance;
                        client.BeginSearchICD_Paging((long)DiseaseChapters_ID, Disease_ID, ICD10Code, PageIndex, PageSize, "", CountTotal, Globals.DispatchCallback((asyncResult) =>
                        {
                            int Total = 0;
                            IList<ICD> allItems = null;
                            bool bOK = false;
                            try
                            {
                                allItems = client.EndSearchICD_Paging(out Total, asyncResult);
                                bOK = true;
                            }
                            catch (FaultException<AxException> fault)
                            {
                                ClientLoggerHelper.LogInfo(fault.ToString());
                            }
                            catch (Exception ex)
                            {
                                ClientLoggerHelper.LogInfo(ex.ToString());
                            }
                            finally
                            {
                                this.HideBusyIndicator();
                            }

                            ObjSearchICD_Paging.Clear();

                            if (bOK)
                            {
                                if (CountTotal)
                                {
                                    ObjSearchICD_Paging.TotalItemCount = Total;
                                }
                                if (allItems != null)
                                {
                                    foreach (var item in allItems)
                                    {
                                        ObjSearchICD_Paging.Add(item);
                                    }
                                }
                            }
                        }), null);
                    }
                }
                catch (Exception ex)
                {
                    ClientLoggerHelper.LogInfo(ex.ToString());
                    this.HideBusyIndicator();
                }
            });
            t.Start();
        }
        public void DiseaseChapters_GetAll()
        {
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new ConfigurationManagerServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginDiseaseChapters_GetAll(Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                var items = contract.EndDiseaseChapters_GetAll(asyncResult);
                                if (items != null)
                                {
                                    DiseaseChapters = new ObservableCollection<DiseaseChapters>(items);
                                    //ItemDefault
                                    DiseaseChapters Default = new DiseaseChapters();
                                    Default.DiseaseChapterID = -1;
                                    Default.DiseaseChapterNameVN = string.Format(eHCMSResources.Z0996_G1_Format1, eHCMSResources.T0822_G1_TatCa);
                                    DiseaseChapters.Insert(0, Default);
                                    //ItemDefault
                                }
                                else
                                {
                                    DiseaseChapters = null;
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

        public void PrescriptionMaxHIPayDrugListGroup_GetAll(long V_RegistrationType)
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
                                    PrescriptionMaxHIPayDrugListGroup = new ObservableCollection<PrescriptionMaxHIPayGroup>(items);
                                }
                                else
                                {
                                    PrescriptionMaxHIPayDrugListGroup = new ObservableCollection<PrescriptionMaxHIPayGroup>();
                                }

                                PrescriptionMaxHIPayGroup all = new PrescriptionMaxHIPayGroup()
                                {
                                    PrescriptionMaxHIPayGroupID = 0,
                                    GroupName = "Tất cả"
                                };
                                PrescriptionMaxHIPayDrugListGroup.Insert(0, all);
                                PrescriptionMaxHIPayGroupID = PrescriptionMaxHIPayDrugListGroup.FirstOrDefault().PrescriptionMaxHIPayGroupID;
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

        private void ICD_XMLInsert(Int64 Disease_ID, ObservableCollection<PrescriptionMaxHIPayLinkICD> objCollect)
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

                        contract.BeginPrescriptionMaxHIPayLinkICD_XMLInsert(objCollect, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                Result = contract.EndPrescriptionMaxHIPayLinkICD_XMLInsert(asyncResult);
                                if (Result)
                                {
                                    ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.PageIndex = 0;
                                    GetPrescriptionMaxHIPayLinkICD_ByID_Paging(0, ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.PageSize, true);
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

               
        //▼==== #002
        private string _ICD10CodeSaved = "";
        public string ICD10CodeSaved
        {
            get { return _ICD10CodeSaved; }
            set
            {
                _ICD10CodeSaved = value;
                NotifyOfPropertyChange(() => ICD10CodeSaved);
            }
        }

        public void btSearchICDInPrescriptionMaxHIPayLinkICD()
        {
            if (PrescriptionMaxHIPayGroupID == 0)
            {
                MessageBox.Show("Chưa chọn nhóm chi phí!");
                return;
            }
            GetPrescriptionMaxHIPayLinkICD_ByID_Paging(0, ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.PageSize, true);
        }

        public void hplEditICD10Link_Click(object selectedItem)
        {
            if (selectedItem != null)
            {
                //Action<IPrescriptionMaxHIPayLinkICDEdit> onInitDlg = (typeInfo) =>
                //{
                //    typeInfo.ObjICD10Link_Current = ObjectCopier.DeepCopy((selectedItem as PrescriptionMaxHIPayLinkICD));
                //    typeInfo.TitleForm = "Hiệu Chỉnh (" + (selectedItem as GetPrescriptionMaxHIPayGroupLinkICD_ByID_Paging).DiseaseNameVN.Trim() + ")";
                //};
                //GlobalsNAV.ShowDialog<IPrescriptionMaxHIPayLinkICDEdit>(onInitDlg);
            }
        }

        public void Handle(SaveEvent<PrescriptionMaxHIPayLinkICD> message)
        {
            GetPrescriptionMaxHIPayLinkICD_ByID_Paging(0, ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.PageSize, true);
        }

        public void BtnExportExcel()
        {
            this.ShowBusyIndicator(eHCMSResources.Z0669_G1_DangLayDLieu);
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new ConfigurationManagerServiceClient())
                    {
                        ConfigurationReportParams Params = new ConfigurationReportParams()
                        {
                            ConfigurationName = ConfigurationName.PrescriptionMaxHIPayLinkICD,
                            V_RegistrationType = VRegistrationTypeLinkICDSelected != null? VRegistrationTypeLinkICDSelected.LookupID:0,
                            PrescriptionMaxHIPayGroupID = PrescriptionMaxHIPayGroupID
                        };

                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginExportExcelConfigurationManager(Params, Globals.DispatchCallback(asyncResult =>
                        {
                            try
                            {
                                var results = contract.EndExportExcelConfigurationManager(asyncResult);
                                ExportToExcelFileAllData.Export(results, "Shee1");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, eHCMSResources.T0074_G1_I, MessageBoxButton.OK);
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
                    MessageBox.Show(ex.Message, eHCMSResources.T0074_G1_I, MessageBoxButton.OK);
                    this.HideBusyIndicator();
                }
            });
            t.Start();
        }

        public void BtnClearICD()
        {
            if (PrescriptionMaxHIPayGroupID == 0)
            {
                MessageBox.Show("Chưa chọn nhóm chi phí!");
                return;
            }
            if (MessageBox.Show("Bạn có muốn Xóa hết ICD của nhóm chi phí này không?", "Xóa hết ICD", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                this.ShowBusyIndicator(eHCMSResources.Z0492_G1_DangXoa);
                var t = new Thread(() =>
                {
                    try
                    {
                        using (var serviceFactory = new ConfigurationManagerServiceClient())
                        {
                            var contract = serviceFactory.ServiceInstance;
                            contract.BeginPrescriptionMaxHIPayLinkICD_ClearAll(PrescriptionMaxHIPayGroupID, Globals.LoggedUserAccount.Staff.StaffID, Globals.DispatchCallback(asyncResult =>
                            {
                                try
                                {
                                    var results = contract.EndPrescriptionMaxHIPayLinkICD_ClearAll(asyncResult);
                                    if (results)
                                    {
                                        ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.PageIndex = 0;
                                        GetPrescriptionMaxHIPayLinkICD_ByID_Paging(0, ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.PageSize, true);
                                        MessageBox.Show(eHCMSResources.A0478_G1_Msg_InfoXoaOK, eHCMSResources.A0464_G1_Msg_Ghi, MessageBoxButton.OK);
                                    }
                                    else
                                    {
                                        MessageBox.Show(eHCMSResources.Z0654_G1_GhiKgThCong, eHCMSResources.A0464_G1_Msg_Ghi, MessageBoxButton.OK);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message, eHCMSResources.T0074_G1_I, MessageBoxButton.OK);
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
                        MessageBox.Show(ex.Message, eHCMSResources.T0074_G1_I, MessageBoxButton.OK);
                        this.HideBusyIndicator();
                    }
                });
                t.Start();
            }
        }

        public byte[] ReadAllBytes(string fileName)
        {
            byte[] buffer = null;
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, (int)fs.Length);
                }
            }
            catch
            {
                MessageBox.Show("File Excel đang được sử dụng, vui lòng đóng trước khi nhập!");
            }
            return buffer;
        }

        public void btnImportFromExcel()
        {
            MessageBox.Show("Chức năng đang hoàn thiện!");
            return;
            if (PrescriptionMaxHIPayGroupID == 0)
            {
                MessageBox.Show("Chưa chọn nhóm chi phí!");
                return;
            }
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XLS files (*.xls)|*.xls|XLSX files (*.xlsx)|*.xlsx";
            openFileDialog.FilterIndex = 2;
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                byte[] file = ReadAllBytes(filePath);
                if (file != null)
                {
                    ImportFromExcell(file);
                }
                return;
            }
        }

        private void ImportFromExcell(byte[] file)
        {
            this.ShowBusyIndicator(eHCMSResources.Z0125_G1_DangXuLi);
            try
            {
                //using (MemoryStream ms = new MemoryStream(file))
                //{
                //    try
                //    {
                //        //ObservableCollection<PrescriptionMaxHIPayLinkICD> objCollect
                //        List<PrescriptionMaxHIPayLinkICD> ListItemICD = new List<PrescriptionMaxHIPayLinkICD>();
                //        using (ExcelPackage package = new ExcelPackage(ms))
                //        {
                //            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                //            ExcelWorksheet workSheet = package.Workbook.Worksheets.FirstOrDefault();
                //            int startRow = workSheet.Dimension.Start.Row + 1;
                //            int endRow = workSheet.Dimension.End.Row;
                //            if (workSheet.Dimension.Columns != 2)
                //            {
                //                MessageBox.Show("File excel không đúng định dạng");
                //                return;
                //            }
                //            else
                //            {
                //                for (int i = startRow; i <= endRow; i++)
                //                {
                //                    ICD ItemICD = new ICD();
                //                    int j = 1;
                //                    try
                //                    {
                //                        ItemICD.IDCode = Convert.ToInt64(workSheet.Cells[i, j++].Value);
                //                        ItemICD.ICD10Code = workSheet.Cells[i, j++].Value.ToString();
                //                        if (!CheckExistsICD(ItemICD))
                //                        {
                //                            ObjPrescriptionMaxHIPayLinkICD_ByID_Paging.Add(new PrescriptionMaxHIPayLinkICD
                //                            {
                //                                PrescriptionMaxHIPayGroupID = PrescriptionMaxHIPayGroupID,
                //                                IDCode = ItemICD.IDCode,
                //                                ICD10 = ItemICD.ICD10Code,
                //                                DiseaseNameVN = ItemICD.DiseaseNameVN,
                //                                IsActive = ItemICD.IsActive,
                //                                CreatedStaff = Globals.LoggedUserAccount.Staff,
                //                                CanDelete = true
                //                            });
                //                        }
                //                    }
                //                    catch (Exception ex)
                //                    {
                //                        MessageBox.Show("Giá trị tại dòng " + i + ", Cột " + j + " trong file Excel không đúng định dạng!");
                //                        return;
                //                    }
                //                }
                //                if (ObjPrescriptionMaxHIPayLinkICD_ByID_Paging == null)
                //                {
                //                    MessageBox.Show("Chưa có dữ liệu");
                //                }

                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                //    }
                //    finally
                //    {
                //        this.HideBusyIndicator();
                //    }
                //}
            }
            catch (Exception ex)
            {
                this.HideBusyIndicator();
                Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
            }
        }
    }
}
