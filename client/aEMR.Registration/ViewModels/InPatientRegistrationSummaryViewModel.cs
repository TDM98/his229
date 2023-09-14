﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using aEMR.CommonTasks;
using aEMR.DataContracts;
using aEMR.Infrastructure;
using aEMR.Infrastructure.Events;
using aEMR.ServiceClient;
using aEMR.ViewContracts;
using Caliburn.Micro;
using DataEntities;
using Service.Core.Common;
using aEMR.Common;
using aEMR.Common.Converters;
using eHCMSLanguage;
using Castle.Windsor;
using System.Text;
using aEMR.Common.Collections;
using aEMR.Controls;
/*
* 20161223 #001 CMN:   Add filter for check exists PCL request with removed item
* 20170218 #002 CMN:   Add Checkbox AllDept for InPtBills
* 20170420 #003 CMN:   Fixed Used date of Issue Bills out of admit date for HT Patient
* 20170522 #004 CMN:   Added variable to check InPt 5 year HI without paid enough
* 20180102 #005 CMN:   Added properties for 4210 file
* 20180329 #006 CMN:   Added CheckValidMedicalInstructionDate Functions
* 20180514 #007 CMN:   Changed Load bills from date to date into server
* 20180525 #008 TBLD:  Kiem tra chi tiet bill tu ngay den ngay
* 20180908 #009 TBL:   Kiem tra chung chi hanh nghe cua bac si chi dinh
* 20181119 #010 TTM:   BM 0005257: Tạo out standing task tìm kiếm bệnh nhân nằm tại khoa và sự kiện chụp lại khi chọn bệnh nhân từ Out standing task.
* 20190802 #011 TTM:   BM 0013054: Chỉ hiển thị khoa nào bệnh nhân đã từng nhập vào trong 1 đợt điều trị.
* 20190820 #012 TTM:   BM 0013042: Bổ sung điều kiện kiểm tra ngày y lệnh của y cụ và DKVT trong cùng 1 bill KTC nếu y cụ nào có ngày y lệnh khác ngày y lệnh của DVKT sẽ không cho lưu.
* 20191025 #013 TBL:   BM 0018467: Thêm IsNotCheckInvalid để khỏi kiểm tra khoảng thời gian giữa 2 lần làm CLS được tính BHYT
* 20191028 #014 TTM:   BM ? BT: #1204: Fix lỗi lấy nhiều lần giá trị khoa nếu bệnh nhân nhập 1 khoa nhiều lần.
* 20191123 #015 TTM:   BM 0019649: Bổ sung điều kiện để màn hình tạo bill viện phí ngoại trú không còn hiện out standing task.
* 20191211 #016 TBL:   BM 0013098: Ekip cho nội trú
* 20200213 #017 TTM:   BM 0023909: Fix lỗi không lưu dv cls đưa vào từ tạo bill nếu trong bill có 1 phiếu chỉ định do người tạo bill lập ra và có cùng ngày với ngày tạo bill.
* 20200429 #018 TTM:   BM 0038153: Fix lỗi double dòng khi lưu bill.
* 20200713 #019 TTM:   BM 0039358: Fix lỗi không hiển thị đúng khi load các dịch vụ ngoại trú lần 2 vào bill nội trú (Sáp nhập).
* 20210308 #020 TNHX:  219 Thêm khoa khám bệnh và bỏ chặn thời gian y lệnh đối với chỉ định từ khoa khám bệnh
* 20210813 #021 TNHX:  436 Ràng buộc tiền giường không được xóa/ không chỉnh sửa ngày giờ y lênh/ kết thúc + Ràng buộc DVKT gây tê cần có thuốc gây tê đi kèm + không có cùng lúc 2 DVKT gây tê
* 20210826 #022 TNHX: Chỉnh lỗi kiểm tra thuốc gây tê khi lưu bill nếu chưa có thuốc + Bỏ kiểm tra ngày sử dụng vs ngày nhập viện đối với DV loadbill từ KhoaPhongKham
* 20210929 #023 TNHX: 681 Trừ đi tiền Nhà nước chi trả đối với BN điều trị COVID
* 20211113 #024 TNHX: Thêm màn hình + event để đưa danh sách + thêm nút lấy danh sách gói dvkt + đưa qua tab dv_cls
* 20220103 #025 TNHX: Tích covid trước khi truyền xuống tính tiền DVKT
* 20220728 #026 QTD:  Thêm đánh dấu tính dịch vụ Bill
*/
namespace aEMR.Registration.ViewModels
{
    [Export(typeof(IInPatientRegistrationSummary)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class InPatientRegistrationSummaryViewModel : Conductor<object>, IInPatientRegistrationSummary
            , IHandle<ResultFound<Patient>>
            , IHandle<ItemSelected<Patient>>
            , IHandle<ItemSelected<PatientRegistration>>
            , IHandle<ResultNotFound<Patient>>
            , IHandle<PayForRegistrationCompleted>
            , IHandle<SaveAndPayForRegistrationCompleted>
            , IHandle<ItemSelected<RefMedicalServiceItem>>
            , IHandle<ItemSelected<PCLExamType>>
            , IHandle<ItemSelected<RefGenMedProductDetails>>
            , IHandle<RemoveItem<MedRegItemBase>>
            , IHandle<EditItem<InPatientBillingInvoice>>
            , IHandle<ItemRemoved<MedRegItemBase, IInPatientBillingInvoiceDetailsListing>>
            , IHandle<ItemChanged<MedRegItemBase, IInPatientBillingInvoiceDetailsListing>>
            , IHandle<DoubleClick>
            , IHandle<DoubleClickAddReqLAB>
            , IHandle<InPatientReturnMedProduct>
            , IHandle<ModifyPriceToInsert_Completed>
            , IHandle<InPatientRegistrationSelectedForInPatientRegistration>
    //, IHandle<LoadBillingDetailsCompleted>
    {
        IEventAggregator _eventArg;
        [ImportingConstructor]
        public InPatientRegistrationSummaryViewModel(IWindsorContainer container, INavigationService navigationService, IEventAggregator eventArg)
        {
            _eventArg = eventArg;
            eventArg.Subscribe(this);
            var searchPatientAndRegVm = Globals.GetViewModel<ISearchPatientAndRegistration>();

            searchPatientAndRegVm.InitButtonVisibility(SearchRegButtonsVisibility.SHOW_SEARCH_REG_BTN);
            searchPatientAndRegVm.SetDefaultButton(SearchRegistrationButtons.SEARCH_REGISTRATION);
            searchPatientAndRegVm.PatientFindByVisibility = false;
            searchPatientAndRegVm.PatientFindBy = AllLookupValues.PatientFindBy.NOITRU;
            searchPatientAndRegVm.mTimBN = mDangKyNoiTru_Patient_TimBN;
            searchPatientAndRegVm.mThemBN = mDangKyNoiTru_Patient_ThemBN;
            searchPatientAndRegVm.mTimDangKy = mDangKyNoiTru_Patient_TimDangKy;

            searchPatientAndRegVm.SearchAdmittedInPtRegOnly = true;

            SearchRegistrationContent = searchPatientAndRegVm;
            ActivateItem(searchPatientAndRegVm);

            var patientInfoVm = Globals.GetViewModel<IPatientSummaryInfoV2>();
            patientInfoVm.mInfo_CapNhatThongTinBN = mDangKyNoiTru_Info_CapNhatThongTinBN;
            patientInfoVm.mInfo_XacNhan = mDangKyNoiTru_Info_XacNhan;
            patientInfoVm.mInfo_XoaThe = mDangKyNoiTru_Info_XoaThe;

            //patientInfoVm.mInfo_XemPhongKham = mDangKyNoiTru_Info_XemPhongKham;
            patientInfoVm.mInfo_XemPhongKham = false;

            PatientSummaryInfoContent = patientInfoVm;
            ActivateItem(patientInfoVm);
            PatientSummaryInfoContent.CanConfirmHi = false;
            PatientSummaryInfoContent.DisplayButtons = false;

            var selectServiceVm = Globals.GetViewModel<IInPatientSelectService>();
            InPatientSelectServiceContent = selectServiceVm;
            ActivateItem(selectServiceVm);

            var selectPclVm = Globals.GetViewModel<IInPatientSelectPcl>();
            SelectPCLContent = selectPclVm;
            ActivateItem(selectPclVm);

            var selectGeneralSugervm = Globals.GetViewModel<IInPatientSelectGeneralSugery>();
            //SelectGeneralSugeryContent.V_PCLMainCategory = (long)AllLookupValues.V_PCLMainCategory.GeneralSugery;
            SelectGeneralSugeryContent = selectGeneralSugervm;
            ActivateItem(selectGeneralSugervm);

            var selectBedvm = Globals.GetViewModel<IInPatientSelectBed>();
            SelectBedContent = selectBedvm;
            ActivateItem(selectBedvm);

            var selectBloodvm = Globals.GetViewModel<IInPatientSelectBlood>();
            SelectBloodContent = selectBloodvm;
            ActivateItem(selectBloodvm);

            //LAB
            var selectPclVmLAB = Globals.GetViewModel<IInPatientSelectPclLAB>();
            SelectPCLContentLAB = selectPclVmLAB;
            ActivateItem(selectPclVmLAB);
            //LAB

            var newBillingVm = Globals.GetViewModel<IInPatientBillingInvoiceDetailsListing>();
            EditingInvoiceDetailsContent = newBillingVm;
            EditingInvoiceDetailsContent.CanEditOnGrid = true;
            ActivateItem(newBillingVm);

            //KMx: Chuyển từ View IInPatientBillingInvoiceListing -> IInPatientBillingInvoiceListingNew (13/09/2014 16:54).
            //var oldBillingVm = Globals.GetViewModel<IInPatientBillingInvoiceListing>();
            var oldBillingVm = Globals.GetViewModel<IInPatientBillingInvoiceListingNew>();
            OldBillingInvoiceContent = oldBillingVm;
            OldBillingInvoiceContent.mDangKyNoiTru_SuaDV = mDangKyNoiTru_SuaDV;
            OldBillingInvoiceContent.mDangKyNoiTru_XemChiTiet = mDangKyNoiTru_XemChiTiet;
            OldBillingInvoiceContent.ShowRecalcHiColumn = false;
            OldBillingInvoiceContent.ShowRecalcHiWithPriceListColumn = false;
            OldBillingInvoiceContent.ShowPrintBillColumn = true;

            ActivateItem(oldBillingVm);

            var drugsVm = Globals.GetViewModel<IDrugListing>();
            SelectDrugContent = drugsVm;
            ActivateItem(drugsVm);

            var medItemVm = Globals.GetViewModel<IMedItemListing>();
            MedItemContent = medItemVm;
            ActivateItem(medItemVm);

            var chemicalVm = Globals.GetViewModel<IChemicalListing>();
            ChemicalItemContent = chemicalVm;
            ActivateItem(chemicalVm);

            DepartmentContent = Globals.GetViewModel<IDepartmentListing>();
            if (Globals.isAccountCheck)
            {
                DepartmentContent.AddSelectOneItem = true;
            }
            else
            {
                DepartmentContent.AddSelectedAllItem = true;
            }
            DepartmentContent.IsShowExaminationDept = true;

            aucHoldConsultDoctor = Globals.GetViewModel<IAucHoldConsultDoctor>();
            aucHoldConsultDoctor.StaffCatType = (long)V_StaffCatType.BacSi;

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                InPatientSelectServiceContent.GetServiceTypes();
                SelectGeneralSugeryContent.GetServiceTypes();
                SelectBedContent.GetServiceTypes();
                SelectBloodContent.GetServiceTypes();
            }
            //KMx: Cấu hình của view này sai rồi, khi nào có thời gian thì làm cấu hình lại.
            //Lưu ý: View này có 2 link (Tạo bill, tạo bill tài vụ), phải check operation ở LeftMenu rồi truyền vào, không phải check trong view này (07/01/2015).
            //authorization();

            // TxD 02/08/2014 Use Global Server Date instead
            //Coroutine.BeginExecute(GetDateTimeFromServer());

            (DepartmentContent as PropertyChangedBase).PropertyChanged += new PropertyChangedEventHandler(InPatientRegistrationViewSummaryModel_PropertyChanged);
            EditingBillingInvoice = new InPatientBillingInvoice();

            // 20191114 TNHX: Thêm giờ vào ngày sử dụng
            //SelectedDate = Globals.GetCurServerDateTime().Date;

            UseAtDateContent = Globals.GetViewModel<IMinHourDateControl>();
            UseAtDateContent.DateTime = new DateTime(Globals.GetCurServerDateTime().Year, Globals.GetCurServerDateTime().Month, Globals.GetCurServerDateTime().Day, Globals.GetCurServerDateTime().Hour, Globals.GetCurServerDateTime().Minute, 0);

            MedicalInstructionDateContent = Globals.GetViewModel<IMinHourDateControl>();
            MedicalInstructionDateContent.DateTime = new DateTime(Globals.GetCurServerDateTime().Year, Globals.GetCurServerDateTime().Month, Globals.GetCurServerDateTime().Day, Globals.GetCurServerDateTime().Hour, Globals.GetCurServerDateTime().Minute, 0);
            ResultDateContent = Globals.GetViewModel<IMinHourDateControl>();
            ResultDateContent.DateTime = new DateTime(Globals.GetCurServerDateTime().Year, Globals.GetCurServerDateTime().Month, Globals.GetCurServerDateTime().Day, Globals.GetCurServerDateTime().Hour, Globals.GetCurServerDateTime().Minute, 0);
            LoadMedicalServiceGroupCollection();
            FromDate = Globals.GetCurServerDateTime();
            ToDate = Globals.GetCurServerDateTime();
        }

        private bool _CheckAllCountCOVID;
        public bool CheckAllCountCOVID
        {
            get
            {
                return _CheckAllCountCOVID;
            }
            set
            {
                if (_CheckAllCountCOVID != value)
                {
                    _CheckAllCountCOVID = value;
                    NotifyOfPropertyChange(() => CheckAllCountCOVID);
                    SetAllCountCOVID();
                }
            }
        }

        public void chkCountPatientCOVID_Click(object source, object sender)
        {
            string error = "";

            if (SelectedRegistrationItem.IsCountPatientCOVID)
            {
                error = CheckCOVIDError(SelectedRegistrationItem);
                if (!string.IsNullOrEmpty(error))
                {
                    SelectedRegistrationItem.IsCountPatientCOVID = false;
                    MessageBox.Show(error, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                    return;
                }
            }

            CalcInvoiceItem(SelectedRegistrationItem, false, false, true);
            ModifiItemCmd(SelectedRegistrationItem);
            if (SelectedRegistrationItem.IsDiscounted)
            {
                CurRegistration.ApplyDiscount(SelectedRegistrationItem, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
            }
            // cập nhật item đang chỉnh vào danh sách gốc AllRegistrationItems
            UpdateDataItemFromGrid(SelectedRegistrationItem);
        }

        public void SetAllCountCOVID()
        {
            if (AllRegistrationItems == null || AllRegistrationItems.Count <= 0)
            {
                return;
            }

            if (CheckAllCountCOVID)
            {
                foreach (MedRegItemBase item in AllRegistrationItems)
                {
                    string error = "";
                    error = CheckCOVIDError(item);

                    if (string.IsNullOrEmpty(error) && !item.IsCountPatientCOVID)
                    {
                        item.IsCountPatientCOVID = true;
                    }

                    if (!IsNewCreateBill)
                    {
                        CalcInvoiceItem(item);
                    }
                    else
                    {
                        CalcInvoiceItem(item, false, false, true);
                    }
                    ModifiItemCmd(item);
                    if (item.IsDiscounted)
                    {
                        CurRegistration.ApplyDiscount(item, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
                    }
                }
            }
            else
            {
                foreach (MedRegItemBase item in AllRegistrationItems)
                {
                    if (item.IsCountPatientCOVID)
                    {
                        item.IsCountPatientCOVID = false;
                    }
                    CalcInvoiceItem(item, false, true);
                    ModifiItemCmd(item);
                    if (item.IsDiscounted)
                    {
                        CurRegistration.ApplyDiscount(item, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
                    }
                }
            }
        }

        private string CheckCOVIDError(MedRegItemBase item)
        {
            string error = "";
            bool IsCountPatientCOVID = false;
            if (CurRegistration.AdmissionInfo != null)
            {
                IsCountPatientCOVID = CurRegistration.AdmissionInfo.IsTreatmentCOVID;
            }
            if (!IsCountPatientCOVID)
            {
                error = "Đăng ký không được xác nhận điều trị COVID";
            }

            if (item is PatientRegistrationDetail)
            {
                PatientRegistrationDetail detail = item as PatientRegistrationDetail;
                if (!detail.RefMedicalServiceItem.InCategoryCOVID)
                {
                    error = "DVKT không nằm trong danh mục COVID. Vui lòng liên hệ KHTH!";
                }
            }
            if (item is PatientPCLRequestDetail)
            {
                PatientPCLRequestDetail detail = item as PatientPCLRequestDetail;
                if (!detail.PCLExamType.InCategoryCOVID)
                {
                    error = "DVKT không nằm trong danh mục COVID. Vui lòng liên hệ KHTH!";
                }
            }
            if (item is OutwardDrugClinicDept)
            {
                OutwardDrugClinicDept detail = item as OutwardDrugClinicDept;
                if (!detail.GenMedProductItem.InCategoryCOVID)
                {
                    error = "Thuốc/ vật tư không nằm trong danh mục COVID. Vui lòng liên hệ Khoa dược!";
                }
            }
            return error;
        }

        private bool _mCheckCovid = true;
        public bool mCheckCovid
        {
            get
            {
                return _mCheckCovid;
            }
            set
            {
                if (_mCheckCovid == value)
                    return;
                _mCheckCovid = value;
                NotifyOfPropertyChange(() => mCheckCovid);
            }
        }

        private double? _HIBenefit;
        public double? HIBenefit
        {
            get { return _HIBenefit; }
            set
            {
                _HIBenefit = value;
                NotifyOfPropertyChange(() => HIBenefit);

            }
        }

        public void SetAllInPackage()
        {
            if (AllRegistrationItems == null || AllRegistrationItems.Count <= 0)
            {
                return;
            }

            foreach (MedRegItemBase item in AllRegistrationItems)
            {
                if (CheckAllInPackage && !item.IsPackageService)
                {
                    item.IsInPackage = true;
                }
                else
                {
                    item.IsInPackage = false;
                }
                CalcInvoiceItem(item);
                ModifiItemCmd(item);
                if (item.IsDiscounted)
                {
                    CurRegistration.ApplyDiscount(item, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
                }
            }
        }

        public string CheckHIError(MedRegItemBase item)
        {
            decimal HIAllowedPriceCheck = 0;

            if (item == null)
            {
                return string.Format("{0}!", eHCMSResources.Z1239_G1_ItemKgCoGTri);
            }

            if (item.ID > 0)
            {
                HIAllowedPriceCheck = item.ChargeableItem.HIAllowedPriceNew.GetValueOrDefault(0);
            }
            else
            {
                HIAllowedPriceCheck = item.ChargeableItem.HIAllowedPrice.GetValueOrDefault(0);
            }

            string error = "";

            if (HIBenefit.GetValueOrDefault() <= 0)
            {
                error = eHCMSResources.A0244_G1_Msg_InfoBNKhongCoBHYT;
            }
            else if (HIAllowedPriceCheck <= 0)
            {
                error = string.Format("{0}!", eHCMSResources.Z1099_G1_LoaiDVKgThuocDMBH);
            }
            // =====▼ #005
            else
            {
                bool bItemInstDateIsValid = false;

                if (CurRegistration.HealthInsurance != null && CurRegistration.PtInsuranceBenefit.GetValueOrDefault() > 0 && (item.MedicalInstructionDate >= CurRegistration.HealthInsurance.ValidDateFrom && item.MedicalInstructionDate <= CurRegistration.HealthInsurance.ValidDateTo))
                {
                    bItemInstDateIsValid = true;
                }
                else if (CurRegistration.HealthInsurance_2 != null && CurRegistration.PtInsuranceBenefit_2.GetValueOrDefault() > 0 && (item.MedicalInstructionDate >= CurRegistration.HealthInsurance_2.ValidDateFrom && item.MedicalInstructionDate <= CurRegistration.HealthInsurance_2.ValidDateTo))
                {
                    bItemInstDateIsValid = true;
                }
                // 2020117 #013 TNHX: Thêm số ngày BHYT đồng ý chi trả đối với BN hết hạn thẻ BHYT
                else if (CurRegistration.HealthInsurance_3 != null
                    && CurRegistration.PtInsuranceBenefit_3.GetValueOrDefault() > 0
                    && (item.MedicalInstructionDate >= CurRegistration.HealthInsurance_3.ValidDateFrom
                    && item.MedicalInstructionDate <= CurRegistration.HealthInsurance_3.ValidDateTo.GetValueOrDefault().AddDays(Globals.ServerConfigSection.InRegisElements.NumOfOverDaysForMedicalInstructDate)))
                {
                    bItemInstDateIsValid = true;
                }
                if (!bItemInstDateIsValid)
                {
                    error = string.Format("{0}!", "Item co ngay Y Lenh khong nam trong khoang thoi gian hop le cua the BHYT.");
                }
            }
            // =====▲ #005
            return error;
        }

        public void SetAllCountHI()
        {
            if (AllRegistrationItems == null || AllRegistrationItems.Count <= 0)
            {
                return;
            }

            if (CheckAllCountHI)
            {
                foreach (MedRegItemBase item in AllRegistrationItems)
                {
                    string error = "";
                    error = CheckHIError(item);

                    if (string.IsNullOrEmpty(error) && !item.IsCountPatientCOVID)
                    {
                        item.IsCountHI = true;
                    }
                    else
                    {
                        item.IsCountHI = false;
                    }

                    if (!IsNewCreateBill)
                    {
                        CalcInvoiceItem(item);
                    }
                    else
                    {
                        if (item is OutwardDrugClinicDept && (item as OutwardDrugClinicDept).LimQtyAndHIPrice != null)
                        {
                            item.TickTime = item.IsCountHI ? AllRegistrationItems.Where(x => x is OutwardDrugClinicDept).Where(x => (x as OutwardDrugClinicDept).LimQtyAndHIPrice != null && x.TickTime < 100).Max(y => y.TickTime).GetValueOrDefault(0) + 1 : 100;
                            CalcInvoiceItem(item, true, true);
                        }
                        else
                        {
                            CalcInvoiceItem(item, false, true);
                        }
                    }
                    ModifiItemCmd(item);
                    if (item.IsDiscounted)
                    {
                        CurRegistration.ApplyDiscount(item, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
                    }
                }
            }
            else
            {
                bool bExistEkip = false;
                foreach (PatientRegistrationDetail item in AllRegistrationItems)
                {
                    if (item.V_EkipIndex != null && item.V_EkipIndex.LookupID > 0)
                    {
                        bExistEkip = true;
                        break;
                    }
                }
                if (bExistEkip && MessageBox.Show(eHCMSResources.Z2977_G1_XoaEkip, eHCMSResources.G0442_G1_TBao, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    foreach (MedRegItemBase item in AllRegistrationItems)
                    {
                        (item as PatientRegistrationDetail).V_Ekip = new Lookup();
                        (item as PatientRegistrationDetail).V_EkipIndex = new Lookup();
                        item.IsCountHI = false;
                        CalcInvoiceItem(item, false, true);
                        ModifiItemCmd(item);
                        if (item.IsDiscounted)
                        {
                            CurRegistration.ApplyDiscount(item, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
                        }
                    }
                }
                else if (!bExistEkip)
                {
                    foreach (MedRegItemBase item in AllRegistrationItems)
                    {
                        item.IsCountHI = false;
                        CalcInvoiceItem(item, false, true);
                        ModifiItemCmd(item);
                        if (item.IsDiscounted)
                        {
                            CurRegistration.ApplyDiscount(item, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
                        }
                    }
                }
                else
                {
                    CheckAllCountHI = true;
                }
            }
        }

        public void SetAllCountPatient()
        {
            if (AllRegistrationItems == null || AllRegistrationItems.Count <= 0)
            {
                return;
            }

            foreach (MedRegItemBase item in AllRegistrationItems)
            {
                item.IsCountPatient = CheckAllCountPatient;
                CalcInvoiceItem(item);
                ModifiItemCmd(item);
                if (item.IsDiscounted)
                {
                    CurRegistration.ApplyDiscount(item, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
                }
            }
        }

        private bool _CheckAllInPackage;
        public bool CheckAllInPackage
        {
            get
            {
                return _CheckAllInPackage;
            }
            set
            {
                if (_CheckAllInPackage != value)
                {
                    _CheckAllInPackage = value;
                    NotifyOfPropertyChange(() => CheckAllInPackage);
                    SetAllInPackage();
                }
            }
        }

        private bool _CheckAllCountHI;
        public bool CheckAllCountHI
        {
            get
            {
                return _CheckAllCountHI;
            }
            set
            {
                if (_CheckAllCountHI != value)
                {
                    _CheckAllCountHI = value;
                    NotifyOfPropertyChange(() => CheckAllCountHI);
                    SetAllCountHI();
                }
            }
        }

        private bool _CheckAllCountPatient;
        public bool CheckAllCountPatient
        {
            get
            {
                return _CheckAllCountPatient;
            }
            set
            {
                if (_CheckAllCountPatient != value)
                {
                    _CheckAllCountPatient = value;
                    NotifyOfPropertyChange(() => CheckAllCountPatient);
                    SetAllCountPatient();
                }
            }
        }

        private bool _IsEnableCountPatient = !Globals.ServerConfigSection.InRegisElements.DisableBtnCheckCountPatientInPt;
        public bool IsEnableCountPatient
        {
            get
            {
                return _IsEnableCountPatient;
            }
            set
            {
                if (_IsEnableCountPatient != value)
                {
                    _IsEnableCountPatient = value;
                    NotifyOfPropertyChange(() => IsEnableCountPatient);
                }
            }
        }

        private DateTime _FromDate;
        public DateTime FromDate
        {
            get
            {
                return _FromDate;
            }
            set
            {
                _FromDate = value;
                NotifyOfPropertyChange(() => FromDate);
            }
        }

        private DateTime _ToDate;
        public DateTime ToDate
        {
            get
            {
                return _ToDate;
            }
            set
            {
                _ToDate = value;
                NotifyOfPropertyChange(() => ToDate);
            }
        }

        private bool _RequireDoctorAndDate = Globals.ServerConfigSection.ClinicDeptElements.RequireDoctorAndDateForMed;
        public bool RequireDoctorAndDate
        {
            get
            {
                return _RequireDoctorAndDate;
            }
            set
            {
                if (_RequireDoctorAndDate != value) return;
                _RequireDoctorAndDate = value;
                NotifyOfPropertyChange(() => RequireDoctorAndDate);
            }
        }

        private DeptLocation _SelLocationInDept = null;
        public DeptLocation SelLocationInDept
        {
            get
            {
                return _SelLocationInDept;
            }
            set
            {
                _SelLocationInDept = value;
                NotifyOfPropertyChange(() => SelLocationInDept);
            }
        }

        private ObservableCollection<DeptLocation> _LocationsInDept;
        public ObservableCollection<DeptLocation> LocationsInDept
        {
            get
            {
                return _LocationsInDept;
            }
            set
            {
                _LocationsInDept = value;
                NotifyOfPropertyChange(() => LocationsInDept);
            }
        }

        public void LoadLocations(long? deptId)
        {
            this.ShowBusyIndicator(eHCMSResources.K2887_G1_DangXuLy);
            var list = new List<refModule>();
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new ConfigurationManagerServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginGetAllLocationsByDeptIDOld(deptId, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                var allItems = contract.EndGetAllLocationsByDeptIDOld(asyncResult);

                                if (allItems != null)
                                {
                                    LocationsInDept = new ObservableCollection<DeptLocation>(allItems);
                                }
                                else
                                {
                                    LocationsInDept = new ObservableCollection<DeptLocation>();
                                }

                                var itemDefault = new DeptLocation();
                                itemDefault.DeptID = -1;
                                itemDefault.Location = new Location();
                                itemDefault.Location.LID = -1;
                                itemDefault.Location.LocationName = string.Format(eHCMSResources.Z0996_G1_Format1, eHCMSResources.Z0116_G1_HayChonPg);
                                LocationsInDept.Insert(0, itemDefault);

                                SelLocationInDept = itemDefault;
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

        private bool _UsedByTaiVuOffice = false;
        public bool UsedByTaiVuOffice
        {
            get { return _UsedByTaiVuOffice; }
            set
            {
                _UsedByTaiVuOffice = value;
                if (_UsedByTaiVuOffice)
                {
                    SearchRegistrationContent.CanSearhRegAllDept = true;
                }
                else
                {
                    SearchRegistrationContent.CanSearhRegAllDept = false;
                }

                InitSelDeptCombo();
            }
        }

        private bool _IsHighTechServiceBill = false;
        public bool IsHighTechServiceBill
        {
            get
            {
                return _IsHighTechServiceBill;
            }
            set
            {
                _IsHighTechServiceBill = value;
                if (EditingInvoiceDetailsContent != null)
                {
                    EditingInvoiceDetailsContent.IsHighTechServiceBill = _IsHighTechServiceBill;
                }
                if (OldBillingInvoiceContent != null)
                {
                    OldBillingInvoiceContent.ShowSupportHpl = _IsHighTechServiceBill;
                }
                NotifyOfPropertyChange(() => IsHighTechServiceBill);
            }
        }

        private bool _showInPackageColumn;
        public bool ShowInPackageColumn
        {
            get
            {
                return _showInPackageColumn;
            }
            set
            {
                _showInPackageColumn = value;

                if (EditingInvoiceDetailsContent != null)
                {
                    EditingInvoiceDetailsContent.ShowInPackageColumn = _showInPackageColumn;
                }

                NotifyOfPropertyChange(() => ShowInPackageColumn);
            }
        }

        // TxD 06/01/2015: Added the following to allow TaiVu Office to create bill for patients of all departments
        private void InitSelDeptCombo()
        {
            DepartmentContent.LstRefDepartment = new ObservableCollection<long>();

            if (UsedByTaiVuOffice)
            {
                foreach (var itemDept in Globals.AllRefDepartmentList)
                {
                    DepartmentContent.LstRefDepartment.Add(itemDept.DeptID);
                }
                DepartmentContent.AddSelectOneItem = false;
                DepartmentContent.AddSelectedAllItem = true;
            }
            else
            {
                if (Globals.LoggedUserAccount.DeptIDResponsibilityList.Count() > 0)
                {
                    if (Globals.AddDrugDeptToListRespDepts && Globals.DrugDeptID > 0)
                    {
                        Globals.LoggedUserAccount.DeptIDResponsibilityList.Add(Globals.DrugDeptID);
                    }
                }
                DepartmentContent.LstRefDepartment = Globals.LoggedUserAccount.DeptIDResponsibilityList;
            }
            DepartmentContent.LoadData();
        }

        private long? DeptID = 0;
        void InPatientRegistrationViewSummaryModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem")
            {
                SetConditionWhenChangeSelectedItem();

                if (DepartmentContent.SelectedItem != null)
                {
                    LoadLocations(DepartmentContent.SelectedItem.DeptID);
                    InPatientSelectServiceContent.DeptID = DepartmentContent.SelectedItem.DeptID;
                    SelectBedContent.DeptID = DepartmentContent.SelectedItem.DeptID;
                    aucHoldConsultDoctor.CurrentDeptID = DepartmentContent.SelectedItem.DeptID;
                    if (DepartmentContent.SelectedItem.DeptID != 0)
                    {
                        SelectBedContent.GetAllMedicalServiceItemsByType();
                    }
                    DeptID = DepartmentContent.SelectedItem.DeptID;
                }
            }
        }

        private void SetConditionWhenChangeSelectedItem()
        {
            // Xóa danh sách bill của khoa cũ có sẵn trên màn hình nếu chuyển khoa
            if (AllRegistrationItems != null && DepartmentContent != null)
            {
                if (AllRegistrationItems.Count() > 0)
                {
                    AllRegistrationItems = new ObservableCollection<MedRegItemBase>();
                    AllRegistrationItemsBackUp = new ObservableCollection<MedRegItemBase>();
                    AllRegistrationItemsFilter = new ObservableCollection<MedRegItemBase>();
                }
                //EditingBillingInvoice.Department = DepartmentContent.SelectedItem;
                //if (DepartmentContent.SelectedItem != null && DepartmentContent.SelectedItem.DeptID > 0)
                //{
                //    EditingBillingInvoice.DeptID = DepartmentContent.SelectedItem.DeptID;
                //    Coroutine.BeginExecute(DoGetStore_EXTERNAL(EditingBillingInvoice.DeptID));
                //    DeptID = DepartmentContent.SelectedItem.DeptID;
                //}
                //else
                //{
                //    DeptID = 0;
                //    if (WarehouseList != null)
                //    {
                //        WarehouseList.Clear();
                //    }
                //}
                //CreateNewBillCmd();
            }

            //KMx: Dời ra ngoài hàm InPatientRegistrationViewSummaryModel_PropertyChanged(). Vì hàm ShowOldRegistration() không cần load bill cũ (đã load rồi) (23/08/2014 15:53).
            //if (CurRegistration != null && CurRegistration.PtRegistrationID > 0)
            //{
            //    GetAllInPatientBillingInvoices();
            //}
        }

        private IDepartmentListing _departmentContent;
        public IDepartmentListing DepartmentContent
        {
            get { return _departmentContent; }
            set
            {
                _departmentContent = value;
                NotifyOfPropertyChange(() => DepartmentContent);
            }
        }

        //▼====== #010
        protected override void OnActivate()
        {
            base.OnActivate();
            Globals.EventAggregator.Subscribe(this);

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                LoadPatientClassifications();
            }
            //▼===== #015: Màn hình này xài chung giữa nội và ngoại trú khi tạo bill nên cần phải phân chia ra chỉ có nội trú mới được phép sử dụng OST.
            if (PatientFindBy == AllLookupValues.PatientFindBy.NGOAITRU)
            {
                TitleForm = eHCMSResources.Z2721_G1_TaoBillNgoaiTru.ToUpper();
            }
            else
            {
                var homeVm = Globals.GetViewModel<IHome>();
                IInPatientOutstandingTask ostvm = Globals.GetViewModel<IInPatientOutstandingTask>();
                homeVm.OutstandingTaskContent = ostvm;
                homeVm.IsExpandOST = true;
                ostvm.WhichVM = SetOutStandingTask.TAO_BILL_VP;
            }
            //▲====== #015
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            Globals.EventAggregator.Unsubscribe(this);
            var homeVm = Globals.GetViewModel<IHome>();
            homeVm.OutstandingTaskContent = null;
            homeVm.IsExpandOST = false;
        }

        //▲====== #010
        private ISearchPatientAndRegistration _searchRegistrationContent;
        public ISearchPatientAndRegistration SearchRegistrationContent
        {
            get { return _searchRegistrationContent; }
            set
            {
                _searchRegistrationContent = value;
                NotifyOfPropertyChange(() => SearchRegistrationContent);
            }
        }

        private IPatientSummaryInfoV2 _patientSummaryInfoContent;
        public IPatientSummaryInfoV2 PatientSummaryInfoContent
        {
            get { return _patientSummaryInfoContent; }
            set
            {
                _patientSummaryInfoContent = value;
                NotifyOfPropertyChange(() => PatientSummaryInfoContent);
            }
        }

        private IInPatientSelectService _inPatientSelectServiceContent;
        public IInPatientSelectService InPatientSelectServiceContent
        {
            get { return _inPatientSelectServiceContent; }
            set
            {
                _inPatientSelectServiceContent = value;
                NotifyOfPropertyChange(() => InPatientSelectServiceContent);
            }
        }

        private IInPatientSelectPcl _selectPCLContent;
        public IInPatientSelectPcl SelectPCLContent
        {
            get { return _selectPCLContent; }
            set
            {
                _selectPCLContent = value;
                NotifyOfPropertyChange(() => SelectPCLContent);
            }
        }

        private IInPatientSelectGeneralSugery _SelectGeneralSugeryContent;
        public IInPatientSelectGeneralSugery SelectGeneralSugeryContent
        {
            get
            {
                return _SelectGeneralSugeryContent;
            }
            set
            {
                _SelectGeneralSugeryContent = value;
                NotifyOfPropertyChange(() => SelectGeneralSugeryContent);
            }
        }
        private IInPatientSelectBed _SelectBedContent;
        public IInPatientSelectBed SelectBedContent
        {
            get
            {
                return _SelectBedContent;
            }
            set
            {
                _SelectBedContent = value;
                NotifyOfPropertyChange(() => SelectBedContent);
            }
        }
        private IInPatientSelectBlood _SelectBloodContent;
        public IInPatientSelectBlood SelectBloodContent
        {
            get
            {
                return _SelectBloodContent;
            }
            set
            {
                _SelectBloodContent = value;
                NotifyOfPropertyChange(() => SelectBloodContent);
            }
        }

        private IInPatientSelectPclLAB _selectPCLContentLAB;
        public IInPatientSelectPclLAB SelectPCLContentLAB
        {
            get { return _selectPCLContentLAB; }
            set
            {
                _selectPCLContentLAB = value;
                NotifyOfPropertyChange(() => SelectPCLContentLAB);
            }
        }

        private string _DeptLocTitle;
        public string DeptLocTitle
        {
            get
            {
                return _DeptLocTitle;
            }
            set
            {
                _DeptLocTitle = value;
                NotifyOfPropertyChange(() => DeptLocTitle);
            }
        }

        private bool _isChangeDept = true;
        public bool isChangeDept
        {
            get { return _isChangeDept; }
            set
            {
                _isChangeDept = value;
                NotifyOfPropertyChange(() => isChangeDept);
            }
        }

        private bool _isDischarged;
        public bool isDischarged
        {
            get { return _isDischarged; }
            set
            {
                _isDischarged = value;
                NotifyOfPropertyChange(() => isDischarged);
                isChangeDept = !isDischarged;
            }
        }

        private IDrugListing _selectDrugContent;
        public IDrugListing SelectDrugContent
        {
            get { return _selectDrugContent; }
            set
            {
                _selectDrugContent = value;
                NotifyOfPropertyChange(() => SelectDrugContent);
            }
        }
        private IMedItemListing _medItemContent;
        public IMedItemListing MedItemContent
        {
            get { return _medItemContent; }
            set
            {
                _medItemContent = value;
                NotifyOfPropertyChange(() => MedItemContent);
            }
        }

        private IChemicalListing _chemicalItemContent;
        public IChemicalListing ChemicalItemContent
        {
            get { return _chemicalItemContent; }
            set
            {
                _chemicalItemContent = value;
                NotifyOfPropertyChange(() => ChemicalItemContent);
            }
        }

        private bool _CanDelete;
        public bool CanDelete
        {
            get { return _CanDelete; }
            set
            {
                if (_CanDelete != value)
                {
                    _CanDelete = value;
                    NotifyOfPropertyChange(() => CanDelete);
                }
            }
        }

        public string EditingBillingInvoiceTitle
        {
            get
            {
                if (_editingBillingInvoice == null)
                {
                    return string.Empty;
                }
                if (_editingBillingInvoice.InPatientBillingInvID > 0)
                {
                    return string.Format("{0} ", eHCMSResources.Z0152_G1_CNhatBill) + _editingBillingInvoice.BillingInvNum;
                }
                else
                {
                    return eHCMSResources.Z0014_G1_ThemBillMoi;
                }
            }
        }

        private IInPatientBillingInvoiceDetailsListing _editingInvoiceDetailsContent;
        public IInPatientBillingInvoiceDetailsListing EditingInvoiceDetailsContent
        {
            get { return _editingInvoiceDetailsContent; }
            set
            {
                _editingInvoiceDetailsContent = value;
                NotifyOfPropertyChange(() => EditingInvoiceDetailsContent);
            }
        }

        //private IInPatientBillingInvoiceListing _oldBillingInvoiceContent;
        //public IInPatientBillingInvoiceListing OldBillingInvoiceContent
        //{
        //    get { return _oldBillingInvoiceContent; }
        //    set
        //    {
        //        _oldBillingInvoiceContent = value;
        //        NotifyOfPropertyChange(() => OldBillingInvoiceContent);
        //    }
        //}

        private IInPatientBillingInvoiceListingNew _oldBillingInvoiceContent;
        public IInPatientBillingInvoiceListingNew OldBillingInvoiceContent
        {
            get { return _oldBillingInvoiceContent; }
            set
            {
                _oldBillingInvoiceContent = value;
                NotifyOfPropertyChange(() => OldBillingInvoiceContent);
            }
        }

        public bool MedRegItemConfirmed
        {
            get
            {
                //Hiện tại là vậy.
                return true;
            }
        }

        private InPatientBillingInvoice _tempBillingInvoice;
        private InPatientBillingInvoice _editingBillingInvoice;
        public InPatientBillingInvoice EditingBillingInvoice
        {
            get
            {
                return _editingBillingInvoice;
            }
            set
            {
                if (_editingBillingInvoice != value)
                {
                    _editingBillingInvoice = value;
                    NotifyOfPropertyChange(() => EditingBillingInvoice);
                    NotifyOfPropertyChange(() => EditingBillingInvoiceTitle);
                    EditingInvoiceDetailsContent.BillingInvoice = _editingBillingInvoice;
                    EditingInvoiceDetailsContent.ResetView();
                }
            }
        }

        private ObservableCollection<PatientClassification> _patientClassifications;
        public ObservableCollection<PatientClassification> PatientClassifications
        {
            get { return _patientClassifications; }
            set
            {
                _patientClassifications = value;
                NotifyOfPropertyChange(() => PatientClassifications);
            }
        }

        private bool _calcPaymentToEndOfDay;
        public bool CalcPaymentToEndOfDay
        {
            get { return _calcPaymentToEndOfDay; }
            set
            {
                _calcPaymentToEndOfDay = value;
                NotifyOfPropertyChange(() => CalcPaymentToEndOfDay);
            }
        }

        private bool _canCalcPaymentToEndOfDay;
        public bool CanCalcPaymentToEndOfDay
        {
            get { return _canCalcPaymentToEndOfDay; }
            set
            {
                _canCalcPaymentToEndOfDay = value;
                NotifyOfPropertyChange(() => CanCalcPaymentToEndOfDay);
                if (!_canCalcPaymentToEndOfDay)
                {
                    CalcPaymentToEndOfDay = false;
                }
            }
        }

        private AllLookupValues.PatientFindBy _PatientFindBy;
        public AllLookupValues.PatientFindBy PatientFindBy
        {
            get
            {
                return _PatientFindBy;
            }
            set
            {
                _PatientFindBy = value;
                NotifyOfPropertyChange(() => PatientFindBy);
                // Hpt 27/11/2015: Đã gán giá trị trong hàm khởi tạo rồi nhưng không có thời gian xem lại nên cứ để thêm một lần nữa ở đây, có thời gian sẽ xem lại và điều chỉnh 
                if (SearchRegistrationContent != null)
                {
                    SearchRegistrationContent.PatientFindBy = PatientFindBy;
                }
            }
        }

        //KMx: Cấu hình có cho phép thêm Thuốc, Y Cụ, Hóa Chất vào bill hay không.
        //Hiện tại thì không cho add trực tiếp vào bill. Muốn chỉnh sửa thì qua kho nội trú chỉnh sửa.
        //Lý do: 1. Khi tạo bill mà thêm thuốc, y cụ, hóa chất thì sẽ tạo ra 1 phiếu xuất giống bên kho nội trú => không phân biệt được.
        //       2. Khi tạo bill mà xóa 1 phiếu xuất: Phiếu xuất chỉ xóa khỏi bill, nhưng vẫn còn tồn tại trong DB. Người dùng nghĩ rằng phiếu đó đã bị xóa khỏi DB => Báo cáo sai.
        //private bool _addMedProductToBillDirectly;

        public bool AddMedProductToBillDirectly
        {
            get { return Globals.ServerConfigSection.InRegisElements.AddMedProductToBillDirectly; }
        }

        /// <summary>
        /// Neu nguoi dung chon benh nhan, dang ky. Va dang ky hop le thi moi set bien nay true.
        /// </summary>
        public bool CanAddEditBill
        {
            // Hpt 22/10/2015: Anh tuấn nói không khóa cứng màn hình để người dùng còn xem lại bill cũ của bệnh nhân đã xuất viện hoặc các đăng ký đã đóng... (theo yêu cầu bv)
            get
            {
                return (CurRegistration != null && CurRegistration.PtRegistrationID > 0 && (CurRegistration.V_RegistrationType == AllLookupValues.RegistrationType.NOI_TRU || PatientFindBy == AllLookupValues.PatientFindBy.NGOAITRU));
            }
        }

        private ObservableCollection<RefStorageWarehouseLocation> _warehouseList;
        public ObservableCollection<RefStorageWarehouseLocation> WarehouseList
        {
            get
            {
                return _warehouseList;
            }
            set
            {
                _warehouseList = value;
                NotifyOfPropertyChange(() => WarehouseList);
            }
        }

        private RefStorageWarehouseLocation _selectedWarehouse;
        public RefStorageWarehouseLocation SelectedWarehouse
        {
            get
            {
                return _selectedWarehouse;
            }
            set
            {
                _selectedWarehouse = value;
                NotifyOfPropertyChange(() => SelectedWarehouse);

                SelectDrugContent.SearchCriteria.Storage = _selectedWarehouse;
                SelectDrugContent.Clear();
                ChemicalItemContent.SearchCriteria.Storage = _selectedWarehouse;
                ChemicalItemContent.Clear();
                MedItemContent.SearchCriteria.Storage = _selectedWarehouse;
                MedItemContent.Clear();
            }
        }

        //==== #002
        private bool _IsAllDept = false;
        public bool IsAllDept
        {
            get
            {
                return _IsAllDept;
            }
            set
            {
                _IsAllDept = value;
                NotifyOfPropertyChange(() => IsAllDept);
            }
        }
        //==== #002
        //private bool _canChangePatientType = false;
        //public bool CanChangePatientType
        //{
        //    get
        //    {
        //        return _canChangePatientType;
        //    }
        //    set
        //    {
        //        if (_canChangePatientType != value)
        //        {
        //            _canChangePatientType = value;
        //            NotifyOfPropertyChange(() => CanChangePatientType);
        //        }
        //    }
        //}

        private bool _canSaveRegistrationAndPay;
        public bool CanSaveRegistrationAndPay
        {
            get
            {
                return _canSaveRegistrationAndPay;
            }
            set
            {
                if (_canSaveRegistrationAndPay != value)
                {
                    _canSaveRegistrationAndPay = value;
                    NotifyOfPropertyChange(() => CanSaveRegistrationAndPay);
                }
            }
        }
        private bool _canSaveRegistration;
        public bool CanSaveRegistration
        {
            get
            {
                return _canSaveRegistration;
            }
            set
            {
                if (_canSaveRegistration != value)
                {
                    _canSaveRegistration = value;
                    NotifyOfPropertyChange(() => CanSaveRegistration);
                }
            }
        }

        private bool _canSearchPatient = true;
        public bool CanSearchPatient
        {
            get
            {
                return _canSearchPatient;
            }
            set
            {
                if (_canSearchPatient != value)
                {
                    _canSearchPatient = value;
                    NotifyOfPropertyChange(() => CanSearchPatient);
                }
            }
        }
        private bool _registrationInfoHasChanged;
        /// <summary>
        /// Cho biet thong tin dang ky tren form da duoc thay doi chua.
        /// </summary>
        public bool RegistrationInfoHasChanged
        {
            get
            {
                return _registrationInfoHasChanged;
            }
            set
            {
                if (_registrationInfoHasChanged != value)
                {
                    _registrationInfoHasChanged = value;
                    NotifyOfPropertyChange(() => RegistrationInfoHasChanged);

                    //ApplicationViewModel.Instance.IsProcessing = _registrationInfoHasChanged;
                    CanSearchPatient = !_registrationInfoHasChanged;
                    EditingInvoiceDetailsContent.CanShowPopupToModifyPrice = _registrationInfoHasChanged;

                    NotifyOfPropertyChange(() => CanCancelChangesCmd);
                    NotifyOfPropertyChange(() => CanSaveBillingInvoiceCmd);
                    NotifyOfPropertyChange(() => CanCreateNewBillCmd);
                    //NotifyOfPropertyChange(() => CanCreateBillingInvoiceFromExistingItemsCmd);
                    NotifyOfPropertyChange(() => CanLoadBillCmd);
                    NotifyOfPropertyChange(() => EditingInvoiceDetailsContent.CanShowPopupToModifyPrice);
                }
            }
        }


        private PatientClassification _curClassification;
        public PatientClassification CurClassification
        {
            get
            {
                return _curClassification;
            }
            set
            {
                if (_curClassification != value)
                {
                    _curClassification = value;
                    NotifyOfPropertyChange(() => CurClassification);
                    NotifyOfPropertyChange(() => HiServiceBeingUsed);
                    //RegistrationSummaryVM.HIServiceBeingUsed = HIServiceBeingUsed;
                }
            }
        }
        public bool HiServiceBeingUsed
        {
            get
            {
                if (_curClassification == null)
                {
                    return false;
                }
                return _curClassification.PatientType == PatientType.INSUARED_PATIENT;
            }
        }
        private HealthInsurance _confirmedHiItem;
        /// <summary>
        /// Thông tin thẻ bảo hiểm đã được confirm
        /// </summary>
        public HealthInsurance ConfirmedHiItem
        {
            get
            {
                return _confirmedHiItem;
            }
            set
            {
                _confirmedHiItem = value;
                NotifyOfPropertyChange(() => ConfirmedHiItem);

                CurClassification = CreateDefaultClassification();
                PatientSummaryInfoContent.CurrentPatientClassification = CreateDefaultClassification();
            }
        }

        private PatientRegistration _curRegistration;
        public PatientRegistration CurRegistration
        {
            get
            {
                return _curRegistration;
            }
            set
            {
                if (_curRegistration != value)
                {
                    _curRegistration = value;
                    NotifyOfPropertyChange(() => CurRegistration);

                    NotifyOfPropertyChange(() => CanLoadBillCmd);
                    NotifyOfPropertyChange(() => CanAddEditBill);
                    NotifyOfPropertyChange(() => CanRegister);

                    if (CurRegistration != null && CurRegistration.AdmissionInfo != null
                        && CurRegistration.AdmissionInfo.DischargeDate != null)
                    {
                        isDischarged = true;
                    }
                    else
                    {
                        isDischarged = false;
                    }
                    NotifyOfPropertyChange(() => isDischarged);

                    if (EditingInvoiceDetailsContent != null && OldBillingInvoiceContent != null)
                    {
                        if (CurRegistration != null && CurRegistration.HisID.GetValueOrDefault() > 0 && CurRegistration.PtInsuranceBenefit.GetValueOrDefault(0) > 0)
                        {
                            EditingInvoiceDetailsContent.ShowHIAppliedColumn = true;
                            OldBillingInvoiceContent.ShowHIAppliedColumn = true;
                        }
                        else
                        {
                            EditingInvoiceDetailsContent.ShowHIAppliedColumn = false;
                            OldBillingInvoiceContent.ShowHIAppliedColumn = false;
                        }
                    }
                    if (InPatientSelectServiceContent != null && CurRegistration != null)
                    {
                        InPatientSelectServiceContent.CurrentInPatientAdmDisDetail = CurRegistration.AdmissionInfo;
                    }
                    else
                    {
                        InPatientSelectServiceContent.CurrentInPatientAdmDisDetail = null;
                    }
                    if (SelectPCLContent != null && CurRegistration != null)
                    {
                        SelectPCLContent.CurrentInPatientAdmDisDetail = CurRegistration.AdmissionInfo;
                    }
                    else
                    {
                        SelectPCLContent.CurrentInPatientAdmDisDetail = null;
                    }
                    if (SelectPCLContentLAB != null && CurRegistration != null)
                    {
                        SelectPCLContentLAB.CurrentInPatientAdmDisDetail = CurRegistration.AdmissionInfo;
                    }
                    else
                    {
                        SelectPCLContentLAB.CurrentInPatientAdmDisDetail = null;
                    }
                    PatientSummaryInfoContent.CurrentPatientRegistration = CurRegistration;
                    if (CurRegistration.AdmissionInfo != null)
                    {
                        if (CurRegistration.AdmissionInfo.IsTreatmentCOVID)
                        {
                            IsShowPatientCOVID = Visibility.Visible;
                        }
                        else
                        {
                            IsShowPatientCOVID = Visibility.Collapsed;
                        }
                    }
                }
            }
        }
        private Patient _curPatient;
        public Patient CurPatient
        {
            get
            {
                return _curPatient;
            }
            set
            {
                _curPatient = value;
                NotifyOfPropertyChange(() => CurPatient);
            }
        }

        private PaperReferal _confirmedPaperReferal;
        /// <summary>
        /// Thông tin giấy chuyển viện đã được confirm
        /// </summary>
        public PaperReferal ConfirmedPaperReferal
        {
            get
            {
                return _confirmedPaperReferal;
            }
            set
            {
                _confirmedPaperReferal = value;
                NotifyOfPropertyChange(() => ConfirmedPaperReferal);
            }
        }
        public bool CanRegister
        {
            get
            {
                if (SelectedTabIndex == (int)InPatientRegistrationViewTab.EDITING_BILLING_INVOICE)
                {
                    // Hpt 22/10/2015: Chỉ đăng ký đã nhập viện chưa xuất viện hoặc đăng ký Vãng lai/Tiền Giải Phẫu chưa nhập viện - chưa quá hạn mới được tạo bill
                    return (CurRegistration != null && ((CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.DischargeDate == null)
                        || (CurRegistration.RegistrationStatus == AllLookupValues.RegistrationStatus.PENDING_INPT && Globals.Check_CasualAndPreOpReg_StillValid(CurRegistration))));
                }
                return false;
            }
        }
        private RegistrationFormMode _currentRegMode = RegistrationFormMode.PATIENT_NOT_SELECTED;
        public RegistrationFormMode CurrentRegMode
        {
            get
            {
                return _currentRegMode;
            }
            set
            {
                if (_currentRegMode != value)
                {
                    _currentRegMode = value;
                    NotifyOfPropertyChange(() => CurrentRegMode);
                }
            }
        }
        public void ResetPatientClassificationToDefaultValue()
        {
            CurClassification = CreateDefaultClassification();
        }
        private PatientClassification CreateDefaultClassification()
        {
            if (ConfirmedHiItem != null)
            {
                return PatientClassification.CreatePatientClassification((long)PatientType.INSUARED_PATIENT, "");
            }
            else
            {
                return PatientClassification.CreatePatientClassification((long)PatientType.NORMAL_PATIENT, "");
            }
        }

        //private DateTime? _selectedDate;
        //public DateTime? SelectedDate
        //{
        //    get { return _selectedDate; }
        //    set
        //    {
        //        _selectedDate = value;
        //        NotifyOfPropertyChange(() => SelectedDate);
        //    }
        //}

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

        private IMinHourDateControl _UseAtDateContent;
        public IMinHourDateControl UseAtDateContent
        {
            get { return _UseAtDateContent; }
            set
            {
                _UseAtDateContent = value;
                NotifyOfPropertyChange(() => UseAtDateContent);
            }
        }

        private IMinHourDateControl _MedicalInstructionDateContent;
        public IMinHourDateControl MedicalInstructionDateContent
        {
            get { return _MedicalInstructionDateContent; }
            set
            {
                _MedicalInstructionDateContent = value;
                NotifyOfPropertyChange(() => MedicalInstructionDateContent);
            }
        }

        private IMinHourDateControl _ResultDateContent;

        public IMinHourDateControl ResultDateContent
        {
            get { return _ResultDateContent; }
            set
            {
                _ResultDateContent = value;
                NotifyOfPropertyChange(() => ResultDateContent);
            }
        }

        private bool CheckDeptID()
        {
            if (DeptID.GetValueOrDefault(0) > 0)
            {
                if (DepartmentContent.SelectedItem != null && DepartmentContent.SelectedItem.DeptID > 0)
                {
                    if (DepartmentContent.SelectedItem.SelectDeptReqSelectRoom)
                    {
                        if (SelLocationInDept != null && SelLocationInDept.DeptLocationID > 0)
                        {
                            return true;
                        }
                        MessageBox.Show(eHCMSResources.A0355_G1_Msg_InfoChonPg, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                MessageBox.Show(eHCMSResources.K0340_G1_ChonKhoa, eHCMSResources.T0432_G1_Error, MessageBoxButton.OK);
            }
            return false;
        }

        public void LoadPatientClassifications()
        {
            var t = new Thread(() =>
            {
                Globals.EventAggregator.Publish(new BusyEvent
                {
                    IsBusy = true,
                    Message =
                        string.Format(eHCMSResources.Z0997_G1_Format2, eHCMSResources.Z0150_G1_DangLayDSLoaiBN)
                });
                try
                {
                    using (var serviceFactory = new PatientRegistrationServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;

                        contract.BeginGetAllClassifications(
                            Globals.DispatchCallback((asyncResult) =>
                            {
                                var allClassifications = contract.EndGetAllClassifications(asyncResult);

                                if (allClassifications != null)
                                {
                                    PatientClassifications = new ObservableCollection<PatientClassification>(allClassifications);
                                }
                                else
                                {
                                    PatientClassifications = null;
                                }
                            }), null);
                    }
                }
                catch (Exception ex)
                {
                    Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                }
                finally
                {
                    Globals.IsBusy = false;
                }
            });
            t.Start();
        }
        private DateTime _RegDate;
        public DateTime RegistrationDate
        {
            get
            {
                return _RegDate;
            }
            set
            {
                _RegDate = value;
                NotifyOfPropertyChange(() => RegistrationDate);
                if (CurRegistration != null && CurRegistration.PtRegistrationID <= 0)
                {
                    CurRegistration.ExamDate = _RegDate;
                    if (_tempRegistration != null && _tempRegistration.PtRegistrationID == CurRegistration.PtRegistrationID)
                    {
                        _tempRegistration.ExamDate = _RegDate;
                    }
                }
            }
        }
        private void InitFormData()
        {
            IsLoadNoBill = false;
            if (CurRegistration == null)
            {
                return;
            }

            //PatientRegItemsContent.SetRegistration(CurRegistration);
            var newBillingInvoiceList = new ObservableCollection<InPatientBillingInvoice>();
            var oldBillingInvoiceList = new ObservableCollection<InPatientBillingInvoice>();

            if (CurRegistration.InPatientBillingInvoices != null)
            {
                foreach (var inv in CurRegistration.InPatientBillingInvoices)
                {
                    //KMx: Chỉ add bill có loại đúng với màn hình (bill thường, bill phẫu thuật) đang thao tác (05/01/2016).
                    if (inv.IsHighTechServiceBill != IsHighTechServiceBill)
                    {
                        continue;
                    }

                    //KMx: Chỉ hiển thị những bill có cùng Khoa với Khoa đang chọn trong combobox. Tránh trường hợp Khoa này nhìn thấy Khoa khác(05/12/2014 11:11).
                    //if (inv.InPatientBillingInvID > 0)
                    if (inv.InPatientBillingInvID > 0)
                    {
                        if (UsedByTaiVuOffice || (!Globals.isAccountCheck) || (Globals.isAccountCheck && inv.DeptID == DeptID))
                        {
                            oldBillingInvoiceList.Add(inv);
                        }
                    }
                    else
                    {
                        newBillingInvoiceList.Add(inv);
                    }
                }
            }

            OldBillingInvoiceContent.BillingInvoices = oldBillingInvoiceList;
        }

        public void InitRegistrationForPatient()
        {
            if (_curPatient == null)
                return;

            /*<Code Tuyen>
            if (_curPatient.LatestRegistration == null)
            {
                MessageBox.Show(eHCMSResources.A0236_G1_Msg_InfoBNChuaDKLanNao);
                return;
            }
            if (_curPatient.LatestRegistration.V_RegistrationType != AllLookupValues.RegistrationType.NGOAI_TRU_NOI_TRU
                && _curPatient.LatestRegistration.V_RegistrationType != AllLookupValues.RegistrationType.NOI_TRU)
            {
                MessageBox.Show(eHCMSResources.A0485_G1_Msg_InfoDKCuoiKhPhaiNoiTru);
                return;
            }
            if (_curPatient.LatestRegistration.RegistrationStatus != AllLookupValues.RegistrationStatus.OPENED
                && _curPatient.LatestRegistration.RegistrationStatus != AllLookupValues.RegistrationStatus.PROCESSING)
            {
                //Thong bao trang thai khong dung
                var converter = new EnumValueToStringConverter();
                var enumDescription = (string)converter.Convert(_curPatient.LatestRegistration.RegistrationStatus, typeof(AllLookupValues.RegistrationStatus), null, Thread.CurrentThread.CurrentCulture);
                MessageBox.Show("Không thao tác với đăng ký này. Trạng thái đăng ký: " + enumDescription);
                return;
            }
            //Mở đăng ký còn đang sử dụng
            OpenRegistration(_curPatient.LatestRegistration.PtRegistrationID);
            Code Tuyen*/


            /*************************/
            /*Code Edit vi Tach Table*/
            /*************************/
            if (_curPatient.LatestRegistration_InPt == null)
            {
                MessageBox.Show(eHCMSResources.A0234_G1_Msg_InfoBNChuaCoDKNoiTru);
                return;
            }
            // Dang ky duoc xem la hop le khi thuoc mot trong hai truong hop duoi day:
            // 1. Dang ky noi tru da nhap vien (OPENED)
            // 2. Dang ky Vang Lai hoac Tien Giai Phau chua nhap vien (PENDING_INPT)
            if ((_curPatient.LatestRegistration_InPt.RegistrationStatus != AllLookupValues.RegistrationStatus.OPENED && _curPatient.LatestRegistration_InPt.AdmissionDate != null)
                || (_curPatient.LatestRegistration_InPt.RegistrationStatus != AllLookupValues.RegistrationStatus.PENDING_INPT && Globals.IsCasuaOrPreOpPt(_curPatient.LatestRegistration_InPt.V_RegForPatientOfType))
                //&& regInfo.RegistrationStatus != AllLookupValues.RegistrationStatus.PROCESSING
                )
            {
                //Thong bao trang thai khong dung
                var converter = new EnumValueToStringConverter();
                var enumDescription = (string)converter.Convert(_curPatient.LatestRegistration_InPt.RegistrationStatus, typeof(AllLookupValues.RegistrationStatus), null, Thread.CurrentThread.CurrentCulture);
                MessageBox.Show(string.Format(eHCMSResources.A0684_G1_Msg_InfoKhThaoTacVoiDKNay, enumDescription));
                return;
            }
            //Mở đăng ký còn đang sử dụng
            OpenRegistration(_curPatient.LatestRegistration_InPt.PtRegistrationID);

        }

        // TxD 02/08/2014 : The following method is nolonger required
        //private void GetCurrentDate()
        //{
        //    var t = new Thread(() =>
        //                           {
        //                               AxErrorEventArgs error = null;
        //                               try
        //                               {
        //                                   using (var serviceFactory = new CommonServiceClient())
        //                                   {
        //                                       var contract = serviceFactory.ServiceInstance;

        //                                       contract.BeginGetDate(Globals.DispatchCallback((asyncResult) =>
        //                                       {
        //                                           try
        //                                           {
        //                                               DateTime date = contract.EndGetDate(asyncResult);
        //                                               if (CurRegistration != null && CurRegistration.PtRegistrationID <= 0 && CurRegistration.ExamDate == DateTime.MinValue)//Đăng ký mới và chưa có ngày tháng.
        //                                               {
        //                                                   RegistrationDate = date.Date;
        //                                               }
        //                                               Globals.ServerDate = date;
        //                                           }
        //                                           catch (FaultException<AxException> fault)
        //                                           {
        //                                               error = new AxErrorEventArgs(fault);
        //                                           }
        //                                           catch (Exception ex)
        //                                           {
        //                                               error = new AxErrorEventArgs(ex);
        //                                           }

        //                                       }), null);
        //                                   }
        //                               }
        //                               catch (Exception ex)
        //                               {
        //                                   error = new AxErrorEventArgs(ex);
        //                               }
        //                               if (error != null)
        //                               {
        //                                   Globals.EventAggregator.Publish(new ErrorOccurred() { CurrentError = error });
        //                               }
        //                           });
        //    t.Start();
        //}
        private void ShowOldRegistration(PatientRegistration regInfo)
        {
            IsLoadNoBill = false;
            if (regInfo.V_RegistrationType != AllLookupValues.RegistrationType.NOI_TRU && PatientFindBy == AllLookupValues.PatientFindBy.NOITRU)
            {
                MessageBox.Show(eHCMSResources.Z0085_G1_DayKhongPhaiDKNoiTru);
                return;
            }
            CurRegistration = regInfo;

            CurrentRegMode = RegistrationFormMode.OLD_REGISTRATION_OPENED;
            ConfirmedHiItem = CurRegistration.HealthInsurance;
            ConfirmedPaperReferal = CurRegistration.PaperReferal;
            NotifyOfPropertyChange(() => ConfirmedHiItem);
            NotifyOfPropertyChange(() => ConfirmedPaperReferal);
            InitRegistration();
            HIBenefit = CurRegistration.PtInsuranceBenefit;

            if (PatientSummaryInfoContent != null)
            {
                PatientSummaryInfoContent.CurrentPatient = CurPatient;
                PatientSummaryInfoContent.SetPatientHISumInfo(CurRegistration.PtHISumInfo);
                PatientSummaryInfoContent.CurrentPatientClassification = CreateDefaultClassification();
            }
            if (CurRegistration.PatientClassification == null && CurRegistration.PatientClassID.HasValue)
            {
                CurClassification = PatientClassification.CreatePatientClassification(CurRegistration.PatientClassID.Value, "");
            }
            else
            {
                CurClassification = CurRegistration.PatientClassification;
            }

            SetConditionWhenChangeSelectedItem();
            if (Validate_RegistrationInfo(CurRegistration))
            {
                CanDelete = true;
            }
        }

        // Hpt 22/10/2015: Cần kiểm tra đăng ký nhiều lần nên đưa hàm kiểm tra ra ngoài để gọi lại cho tiện
        private bool Validate_RegistrationInfo(PatientRegistration regInfo)
        {
            if (regInfo.RegistrationStatus == AllLookupValues.RegistrationStatus.REFUND || regInfo.RegistrationStatus == AllLookupValues.RegistrationStatus.COMPLETED)
            {
                MessageBox.Show(eHCMSResources.A0483_G1_Msg_InfoKhTheTaoHayCNhatBill);
                return false;
            }
            // Dang ky noi tru chua nhap vien (AdmissionDate == null) hoac da nhap vien ma trang thai khac OPENED (= REFUND) thi khong hop le  
            if ((regInfo.AdmissionInfo != null && regInfo.RegistrationStatus != AllLookupValues.RegistrationStatus.OPENED)
                || (regInfo.AdmissionInfo == null && Globals.IsCasuaOrPreOpPt(regInfo.V_RegForPatientOfType) && regInfo.RegistrationStatus != AllLookupValues.RegistrationStatus.PENDING_INPT))
            {
                var converter = new EnumValueToStringConverter();
                var enumDescription = (string)converter.Convert(regInfo.RegistrationStatus, typeof(AllLookupValues.RegistrationStatus), null, Thread.CurrentThread.CurrentCulture);
                MessageBox.Show(string.Format("{0} {1}", eHCMSResources.A0684_G1_Msg_InfoKhThaoTacVoiDKNay, enumDescription));
                return false;
            }
            if (Globals.IsCasuaOrPreOpPt(regInfo.V_RegForPatientOfType) && regInfo.AdmissionInfo == null
                && regInfo.RegistrationStatus == AllLookupValues.RegistrationStatus.PENDING_INPT && !Globals.Check_CasualAndPreOpReg_StillValid(regInfo))
            {
                MessageBox.Show(eHCMSResources.A0491_G1_Msg_InfoKhTheTaoHayCNhatBill2, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                return false;
            }
            if (regInfo.AdmissionInfo != null && regInfo.AdmissionInfo.DischargeDate != null)
            {
                MessageBox.Show(string.Format("{0} {1}", eHCMSResources.A0226_G1_Msg_InfoBNDaXV, eHCMSResources.A0227_G1_Msg_KhTheTaoHoacCapNhapBill), eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Gọi hàm này khi tạo mới một đăng ký, hoặc load xong một đăng ký đã có.
        /// Khởi tạo những giá trị cần thiết để đưa lên form
        /// </summary>
        private void InitRegistration()
        {
            _curPatient = CurRegistration.Patient;
            NotifyOfPropertyChange(() => CurPatient);

            InitFormData();
        }

        //HPT: Thêm hàm kiểm tra đăng ký vãng lai hoặc tiền giải phẫu đã hết hạn hay chưa để cho phép hay không cho phép tạo bill

        //HPT - END - 10/09/2015
        public IEnumerator<IResult> DoOpenRegistration(long regID)
        {
            //KMx: Chỉ lấy những thông tin cần thiết của đăng ký thôi, không load hết (17/09/2014 17:31).
            LoadRegistrationSwitch LoadRegisSwitch = new LoadRegistrationSwitch
            {
                IsGetAdmissionInfo = true,
                IsGetBillingInvoices = true,
                IsGetPromoDiscountPrograms = true
            };

            var loadRegTask = new LoadRegistrationInfo_InPtTask(regID, (int)PatientFindBy, LoadRegisSwitch);
            yield return loadRegTask;
            if (loadRegTask.Registration == null)
            {
                //Thong bao khong load duoc dang ky
                Globals.EventAggregator.Publish(new ShowMessageEvent { Message = "Error!(7)" });
            }
            else
            {
                //khong can hoi->chon lai la load len thoi
                //if (_curPatient != null && regInfo.PatientID != _curPatient.PatientID)
                //{
                //    string newPatientName = regInfo != null &&
                //                            regInfo.Patient != null
                //                            ? regInfo.Patient.FullName : "";
                //    string message = string.Format("Bạn đang thao tác với bệnh nhân '{0}'. Bạn có muốn chuyển sang đăng ký của bệnh nhân '{1}'?", _curPatient.FullName, newPatientName);
                //    MessageBoxResult result = MessageBox.Show(message, eHCMSResources.G0442_G1_TBao,
                //                                              MessageBoxButton.OKCancel);
                //    if (result == MessageBoxResult.OK)
                //    {
                //        ShowOldRegistration(regInfo);

                //        //goi lai ham chon khoa phong de load lai
                //        SetConditionWhenChangeSelectedItem();
                //    }
                //}


                //KMx: Ở đây set CurRegistration rồi, trong hàm ShowOldRegistration lại set nữa là sao? (16/09/2014 11:33).
                CurRegistration = loadRegTask.Registration;
                PatientSummaryInfoContent.CurrentPatient = CurRegistration.Patient;
                //▼====== #011
                if (CurRegistration.AdmissionInfo != null)
                {
                    setDepartmentForPatient(CurRegistration);
                }
                //▲====== #011
                ShowOldRegistration(CurRegistration);
            }
        }

        //▼====== #011
        long DeptID_Inside = 0;
        public void setDepartmentForPatient(PatientRegistration CurRegistration)
        {
            ObservableCollection<RefDepartment> tmpListDepartment = new ObservableCollection<RefDepartment>();
            //▼====: #020
            RefDepartment KD_Department = DepartmentContent.Departments.Where(x => x.DeptID == (long)AllLookupValues.DeptID.KHOA_DUOC || x.DeptID == Globals.ServerConfigSection.Hospitals.KhoaPhongKham).FirstOrDefault();
            //▲====: #020
            DepartmentContent.Departments = new ObservableCollection<RefDepartment>();
            foreach (var item in CurRegistration.AdmissionInfo.InPatientDeptDetails)
            {
                tmpListDepartment.Add(item.DeptLocation.RefDepartment);
                if (item.V_InPatientDeptStatus == AllLookupValues.InPatientDeptStatus.NHAP_KHOA_PHONG)
                {
                    DeptID_Inside = item.DeptLocation.DeptID;
                }
            }
            //▼===== #014 
            if (tmpListDepartment.Count > 0)
            {
                foreach (var x in tmpListDepartment.GroupBy(x => x.DeptID).Select(y => y.FirstOrDefault()))
                {
                    DepartmentContent.Departments.Add(x);
                }
            }
            //▲===== #014
            if (KD_Department != null)
            {
                DepartmentContent.Departments.Add(KD_Department);
            }

            DepartmentContent.SelectedItem = DepartmentContent.Departments.Where(x => x.DeptID == DeptID_Inside).FirstOrDefault();
            if (Globals.IsUserAdmin)
            {
                RefDepartment firstItem = new RefDepartment();
                firstItem.DeptID = 0;
                firstItem.DeptName = string.Format(eHCMSResources.Z0996_G1_Format1, eHCMSResources.T0822_G1_TatCa);
                DepartmentContent.Departments.Insert(0, firstItem);
                DepartmentContent.SelectedItem = DepartmentContent.Departments.FirstOrDefault();
            }
        }
        //▲====== #011

        public void OpenRegistration(long regID)
        {
            //RegistrationLoading = true;
            Coroutine.BeginExecute(DoOpenRegistration(regID), null, (o, e) => { });
        }

        private PatientRegistration _tempRegistration;

        private int? _serviceQty;
        public int? ServiceQty
        {
            get { return _serviceQty; }
            set
            {
                _serviceQty = value;
                NotifyOfPropertyChange(() => ServiceQty);
            }
        }

        private int? _BedQty;
        public int? BedQty
        {
            get { return _BedQty; }
            set
            {
                _BedQty = value;
                NotifyOfPropertyChange(() => BedQty);
            }
        }

        private int? _BloodQty;
        public int? BloodQty
        {
            get { return _BloodQty; }
            set
            {
                _BloodQty = value;
                NotifyOfPropertyChange(() => BloodQty);
            }
        }

        private int? _SugeryQty;
        public int? SugeryQty
        {
            get { return _SugeryQty; }
            set
            {
                _SugeryQty = value;
                NotifyOfPropertyChange(() => SugeryQty);
            }
        }

        private int? _PclQty;
        public int? PclQty
        {
            get { return _PclQty; }
            set
            {
                _PclQty = value;
                NotifyOfPropertyChange(() => PclQty);
            }
        }

        private int? _PclQtyLAB;
        public int? PclQtyLAB
        {
            get { return _PclQtyLAB; }
            set
            {
                _PclQtyLAB = value;
                NotifyOfPropertyChange(() => PclQtyLAB);
            }
        }

        private decimal? _drugQty;
        public decimal? DrugQty
        {
            get { return _drugQty; }
            set
            {
                _drugQty = value;
                NotifyOfPropertyChange(() => DrugQty);
            }
        }
        private decimal? _medItemQty;
        public decimal? MedItemQty
        {
            get { return _medItemQty; }
            set
            {
                _medItemQty = value;
                NotifyOfPropertyChange(() => MedItemQty);
            }
        }
        private decimal? _chemicalQty;
        public decimal? ChemicalQty
        {
            get { return _chemicalQty; }
            set
            {
                _chemicalQty = value;
                NotifyOfPropertyChange(() => ChemicalQty);
            }
        }
        public void Handle(ResultFound<Patient> message)
        {
            if (GetView() != null && message != null)
            {
                CurPatient = message.Result;
                CurRegistration = null;

                if (CurPatient != null)
                {
                    SetCurrentPatient(CurPatient);
                }
            }
        }

        public void SetCurrentPatient(object patient)
        {
            Patient p = patient as Patient;
            if (p == null || p.PatientID <= 0)
            {
                return;
            }
            ConfirmedHiItem = null;
            ConfirmedPaperReferal = null;

            EditingBillingInvoice = null;
            OldBillingInvoiceContent.BillingInvoices = null;

            if (p.PatientID > 0)
            {
                GetPatientByID(p.PatientID);
            }
            else
            {
                CurPatient = null;
                CurrentRegMode = RegistrationFormMode.PATIENT_NOT_SELECTED;
            }
        }

        private bool _patientLoaded;
        public bool PatientLoaded
        {
            get
            {
                return _patientLoaded;
            }
            set
            {
                _patientLoaded = value;
                NotifyOfPropertyChange(() => PatientLoaded);
            }
        }

        private void GetPatientByID(long patientID)
        {
            this.ShowBusyIndicator(eHCMSResources.Z0119_G1_DangLayTTinBN);
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new PatientRegistrationServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;

                        contract.BeginGetPatientByID(patientID, false,
                            Globals.DispatchCallback((asyncResult) =>
                            {
                                try
                                {
                                    var patient = contract.EndGetPatientByID(asyncResult);
                                    CurPatient = patient;

                                    PatientLoaded = true;
                                    InitRegistrationForPatient();
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

        private void AddMedRegItemBaseToNewInvoice(MedRegItemBase item)
        {
            EditingInvoiceDetailsContent.AddItemToView(item);
        }

        //private void CalcInvoiceItem(IInvoiceItem item)
        //private void CalcInvoiceItem(MedRegItemBase item)
        //{
        //    //KMx: OnlyRoundResultForOutward = true: Tính tổng tiền BH trả rồi mới làm tròn. Ngược lại thì làm tròn tiền BH trả trên từng dòng rồi mới tính tổng(02/08/2014 18:24).
        //    bool onlyRoundResultForOutward = Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward;

        //    bool isHighTechServiceDetail = false;
        //    if (item is PatientRegistrationDetail)
        //    {
        //        var detailsItem = item as PatientRegistrationDetail;
        //        if (detailsItem.RefMedicalServiceItem != null && detailsItem.RefMedicalServiceItem.RefMedicalServiceType != null
        //            && detailsItem.RefMedicalServiceItem.RefMedicalServiceType.V_RefMedicalServiceTypes == (long)AllLookupValues.V_RefMedicalServiceTypes.KYTHUATCAO)
        //        {
        //            isHighTechServiceDetail = true;
        //        }
        //    }

        //    //double? hiBenefit = PatientSummaryInfoContent.HiBenefit;
        //    // TxD 02/03/2018 HiBenefit has been removed from interface of PatientSummaryInfoContent so use CurRegistration.PtInsuranceBenefit
        //    //item.HIBenefit = PatientSummaryInfoContent.HiBenefit;
        //    item.HIBenefit = CurRegistration.PtInsuranceBenefit;

        //    //if (hiBenefit > 0)
        //    //{
        //    //    //item.HiApplied = true;
        //    //}

        //    item.InvoicePrice = item.HIBenefit.HasValue ? item.ChargeableItem.HIPatientPrice : item.ChargeableItem.NormalPrice;
        //    item.HIAllowedPrice = item.ChargeableItem.HIAllowedPrice;
        //    item.PriceDifference = item.InvoicePrice - item.HIAllowedPrice.GetValueOrDefault(0);

        //    if (!onlyRoundResultForOutward)
        //    {
        //        item.TotalHIPayment = MathExt.Round(item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)item.HIBenefit.GetValueOrDefault(0.0) * item.Qty, MidpointRounding.AwayFromZero);
        //    }
        //    else
        //    {
        //        item.TotalHIPayment = item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)item.HIBenefit.GetValueOrDefault(0.0) * item.Qty;
        //    }

        //    //KMx: Nếu đang tạo Bill DVKTC và loại DV đó KHÔNG phải DVKTC thì mặc định tick "Trong Gói" (11/12/2015 09:38).
        //    if (IsHighTechServiceBill && !isHighTechServiceDetail)
        //    {
        //        item.IsInPackage = true;
        //        item.TotalInvoicePrice = 0;
        //        item.TotalPatientPayment = 0;
        //    }
        //    else
        //    {
        //        item.IsInPackage = false;
        //        item.TotalInvoicePrice = item.InvoicePrice * (decimal)item.Qty;
        //        item.TotalPatientPayment = item.TotalInvoicePrice - item.TotalHIPayment;
        //    }

        //    if (item.HIBenefit.GetValueOrDefault() > 0 && item.HIAllowedPrice.GetValueOrDefault() > 0)
        //    {
        //        item.IsCountHI = true;
        //    }
        //    else
        //    {
        //        item.IsCountHI = false;
        //    }

        //}
        public void AddGenMedService(object medicalService, int qty, DateTime createdDate)
        {
            if (!CheckDeptID())
            {
                return;
            }

            RefMedicalServiceItem curItem = medicalService as RefMedicalServiceItem;

            if (CurRegistration == null || curItem == null)
            {
                return;
            }

            if (curItem.RefMedicalServiceType.V_RefMedicalServiceTypes != (long)AllLookupValues.V_RefMedicalServiceTypes.CANLAMSANG)
            {
                PatientRegistrationDetail item;
                item = new PatientRegistrationDetail
                {
                    StaffID = Globals.LoggedUserAccount.StaffID,
                    CreatedDate = createdDate,
                    MedProductType = AllLookupValues.MedProductType.KCB,
                    RefMedicalServiceItem = curItem,
                    Qty = qty,
                    ReasonChangePrice = ReasonChangePrice,
                    V_NewPriceType = curItem.V_NewPriceType,
                    IsPackageService = curItem.IsPackageService,

                    DoctorStaff = new Staff
                    {
                        StaffID = aucHoldConsultDoctor.StaffID,
                        FullName = aucHoldConsultDoctor.StaffName,
                        /*▼====: #009*/
                        SCertificateNumber = aucHoldConsultDoctor.CertificateNumber
                    },
                    /*▲====: #009*/
                    MedicalInstructionDate = MedicalInstructionDateContent.DateTime,
                    ResultDate = ResultDateContent.DateTime
                };
                //KMx: Không sử dụng biến HiApplied nữa, thay thế bằng IsCountHI và được set trong hàm CalcInvoiceItem (12/12/2014 09:07).
                //if (CurRegistration.PtInsuranceBenefit.GetValueOrDefault(0) > 0 && CurRegistration.HisID.GetValueOrDefault(0) > 0)
                //{
                //    item.HiApplied = true;
                //}
                //else
                //{
                //    item.HiApplied = false;
                //}

                if (EditingBillingInvoice.RegistrationDetails == null)
                {
                    EditingBillingInvoice.RegistrationDetails = new ObservableCollection<PatientRegistrationDetail>();
                }
                else
                {
                    foreach (var row in EditingBillingInvoice.RegistrationDetails)
                    {
                        if (row.MedServiceID == item.MedServiceID && item.MedServiceID > 0)
                        {
                            if (MessageBox.Show(item.RefMedicalServiceItem.MedServiceName + string.Format(" {0}", eHCMSResources.W0949_G1_W), eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                            {
                                return;
                            }
                        }
                    }
                }
                //▼====: #025
                if (CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.IsTreatmentCOVID
                    && item.RefMedicalServiceItem.InCategoryCOVID)
                {
                    item.IsCountPatientCOVID = true;
                }
                //▲====: #025
                GlobalsNAV.CalcInvoiceItem(item, IsHighTechServiceBill, CurRegistration);
                EditingBillingInvoice.RegistrationDetails.Add(item);
                AddMedRegItemBaseToNewInvoice(item);
                RegistrationInfoHasChanged = true;
            }
            else
            {
                AddDefaultPCLRequest(curItem.MedServiceID, qty);
            }
        }

        public void AddPCLItem_Goi(object pclItem, int qty, DateTime createdDate, bool used)
        {
            if (!CheckDeptID())
            {
                return;
            }
            if (CurRegistration != null)
            {
                var curItem = pclItem as PCLExamType;
                if (curItem != null)
                {
                    PatientPCLRequestDetail item;
                    item = new PatientPCLRequestDetail
                    {
                        StaffID = Globals.LoggedUserAccount.StaffID,
                        CreatedDate = createdDate,
                        MedProductType = AllLookupValues.MedProductType.CAN_LAM_SANG,
                        PCLExamType = curItem,
                        Qty = qty
                    };

                    //KMx: Không sử dụng biến HiApplied nữa, thay thế bằng IsCountHI và được set trong hàm CalcInvoiceItem (12/12/2014 09:07).
                    //if (CurRegistration.PtInsuranceBenefit.GetValueOrDefault(0) > 0 && CurRegistration.HisID.GetValueOrDefault(0) > 0)
                    //{
                    //    item.HiApplied = true;
                    //}
                    //else
                    //{
                    //    item.HiApplied = false;
                    //}

                    if (used)
                    {
                        item.V_ExamRegStatus = (long)AllLookupValues.ExamRegStatus.HOAN_TAT;
                    }
                    else
                    {
                        item.V_ExamRegStatus = (long)AllLookupValues.ExamRegStatus.DANG_KY_KHAM;
                    }

                    if (EditingBillingInvoice.PclRequests == null)
                    {
                        EditingBillingInvoice.PclRequests = new ObservableCollection<PatientPCLRequest>();
                    }
                    PatientPCLRequest tempRequest = EditingBillingInvoice.PclRequests.Where(p => p.V_PCLRequestType == AllLookupValues.V_PCLRequestType.NOI_TRU
                                                                                            && p.StaffID == item.StaffID
                                                                                            && p.CreatedDate.Date == createdDate.Date
                                                                                            && p.RecordState == RecordState.DETACHED).FirstOrDefault();
                    if (tempRequest == null)
                    {
                        tempRequest = new PatientPCLRequest();
                        tempRequest.PatientPCLRequestIndicators = new ObservableCollection<PatientPCLRequestDetail>();
                        tempRequest.Diagnosis = eHCMSResources.Z0159_G1_CLSBNMoi;
                        tempRequest.StaffID = item.StaffID;
                        tempRequest.CreatedDate = createdDate;
                        tempRequest.V_PCLRequestType = AllLookupValues.V_PCLRequestType.NOI_TRU;

                        if (used)
                        {
                            tempRequest.V_PCLRequestStatus = AllLookupValues.V_PCLRequestStatus.CLOSE;
                        }
                        else
                        {
                            tempRequest.V_PCLRequestStatus = AllLookupValues.V_PCLRequestStatus.OPEN;
                        }
                        tempRequest.RecordState = RecordState.DETACHED;
                        tempRequest.EntityState = EntityState.DETACHED;

                        if (Globals.DeptLocation != null)
                        {
                            tempRequest.ReqFromDeptLocID = Globals.DeptLocation.DeptLocationID;
                        }

                        if (DepartmentContent != null && DepartmentContent.SelectedItem != null
                            && DepartmentContent.SelectedItem.SelectDeptReqSelectRoom)
                        {
                            if (SelLocationInDept != null && SelLocationInDept.DeptLocationID > 0)
                            {
                                tempRequest.ReqFromDeptLocID = SelLocationInDept.DeptLocationID;
                            }
                        }

                        EditingBillingInvoice.PclRequests.Add(tempRequest);
                    }
                    //▼====: #025
                    if (CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.IsTreatmentCOVID
                    && item.PCLExamType.InCategoryCOVID)
                    {
                        item.IsCountPatientCOVID = true;
                    }
                    //▲====: #025
                    GlobalsNAV.CalcInvoiceItem(item, IsHighTechServiceBill, CurRegistration);
                    tempRequest.PatientPCLRequestIndicators.Add(item);
                    AddMedRegItemBaseToNewInvoice(item);
                    RegistrationInfoHasChanged = true;
                }
            }
        }

        public void CheckAndAddAllPCL(ObservableCollection<PCLExamType> AllPCLExamType, int qty, DateTime createdDate, bool used)
        {
            if (AllPCLExamType == null || !CheckDeptID())
            {
                return;
            }

            ObservableCollection<PCLExamType> NewPCLList = new ObservableCollection<PCLExamType>();
            ObservableCollection<PCLExamType> ExistsPCLList = new ObservableCollection<PCLExamType>();

            if (EditingBillingInvoice != null && EditingBillingInvoice.PclRequests != null)
            {
                var lstpcldetails = EditingBillingInvoice.PclRequests.SelectMany(x => x.PatientPCLRequestIndicators);
                foreach (var item in AllPCLExamType)
                {
                    if (lstpcldetails.Any(x => x.PCLExamType != null && x.PCLExamType.PCLExamTypeID == item.PCLExamTypeID))
                    {
                        ExistsPCLList.Add(item);
                    }
                    else
                    {
                        NewPCLList.Add(item);
                    }
                }
            }

            if (ExistsPCLList != null && ExistsPCLList.Count > 0)
            {
                string strPCLName = "";
                foreach (PCLExamType existsPCL in ExistsPCLList)
                {
                    strPCLName += Environment.NewLine + existsPCL.PCLExamTypeName + ".";
                }

                if (MessageBox.Show(eHCMSResources.A0892_G1_Msg_InfoPCLDaTonTai + strPCLName + Environment.NewLine + eHCMSResources.T1986_G1_CoMuonTiepTucThemKhong, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    foreach (PCLExamType item in AllPCLExamType)
                    {
                        AddPCLItem(item, qty, createdDate, used);
                    }
                }
                else
                {
                    foreach (PCLExamType item in NewPCLList)
                    {
                        AddPCLItem(item, qty, createdDate, used);
                    }
                }
            }
            else
            {
                foreach (PCLExamType item in AllPCLExamType)
                {
                    AddPCLItem(item, qty, createdDate, used);
                }
            }
        }

        private void CheckAndAddPCL(PCLExamType pclItem, int qty, DateTime createdDate, bool used)
        {
            if (pclItem == null || !CheckDeptID())
            {
                return;
            }
            if (EditingBillingInvoice != null && EditingBillingInvoice.PclRequests != null)
            {
                //==== #001
                //var lstpcldetails = EditingBillingInvoice.PclRequests.SelectMany(x => x.PatientPCLRequestIndicators)
                var lstpcldetails = EditingBillingInvoice.PclRequests.SelectMany(x => x.PatientPCLRequestIndicators).Where(x => x.RecordState != RecordState.DELETED);
                //==== #001

                if (lstpcldetails.Any(x => x.PCLExamType != null && x.PCLExamType.PCLExamTypeID == pclItem.PCLExamTypeID))
                {
                    if (MessageBox.Show(pclItem.PCLExamTypeName + string.Format(" {0}", eHCMSResources.W0949_G1_W), eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                    {
                        return;
                    }
                }
            }

            if (pclItem.V_NewPriceType == (long)AllLookupValues.V_NewPriceType.Unknown_PriceType || pclItem.V_NewPriceType == (long)AllLookupValues.V_NewPriceType.Updatable_PriceType)
            {
                PatientPCLRequestDetail item = new PatientPCLRequestDetail
                {
                    PCLExamType = pclItem
                };
                void onInitDlg(IModifyBillingInvItem vm)
                {
                    vm.ModifyBillingInvItem.IsModItemOK = false;
                    vm.ModifyBillingInvItem = item;
                    vm.ModifyBillingInvItem.V_NewPriceType = item.PCLExamType.V_NewPriceType;
                    vm.PopupType = 1;
                    vm.Init();
                }
                GlobalsNAV.ShowDialog<IModifyBillingInvItem>(onInitDlg);
            }
            else
            {
                AddPCLItem(pclItem, qty, createdDate, used);
            }
        }

        private void AddPCLItem(PCLExamType pclItem, int qty, DateTime createdDate, bool used)
        {
            if (CurRegistration == null || pclItem == null)
            {
                return;
            }

            //KMx: Trước đây nếu số lượng > 1 thì chỉ có 1 dòng. Sửa lại nếu SL bao nhiêu thì sinh ra bấy nhiêu dòng, để 1 dòng sẽ có 1 kết quả CLS khác nhau (15/12/2014 10:29).
            for (int i = 0; i < qty; i++)
            {
                PatientPCLRequestDetail item;
                item = new PatientPCLRequestDetail
                {
                    StaffID = Globals.LoggedUserAccount.StaffID,
                    CreatedDate = createdDate,
                    MedProductType = AllLookupValues.MedProductType.CAN_LAM_SANG,
                    PCLExamType = pclItem,
                    //KMx: Nếu muốn sửa SL > 1 mà chỉ hiển thị 1 dòng thì sử dụng lại dòng code bên dưới và bỏ dòng item.Qty = 1 đi (15/12/2014 10:30).
                    //item.Qty = qty;
                    Qty = 1,
                    V_NewPriceType = pclItem.V_NewPriceType,
                    ReasonChangePrice = ReasonChangePrice,

                    DoctorStaff = new Staff
                    {
                        StaffID = aucHoldConsultDoctor.StaffID,
                        FullName = aucHoldConsultDoctor.StaffName,
                        /*▼====: #009*/
                        SCertificateNumber = aucHoldConsultDoctor.CertificateNumber
                    }
                };
                /*▲====: #009*/
                if (MedicalInstructionDateContent.DateTime != null)
                {
                    item.MedicalInstructionDate = Globals.ApplyValidMedicalInstructionDate(MedicalInstructionDateContent.DateTime.GetValueOrDefault(), CurRegistration);
                }
                else
                {
                    item.MedicalInstructionDate = MedicalInstructionDateContent.DateTime;
                }
                item.ResultDate = ResultDateContent.DateTime;
                //KMx: Không sử dụng biến HiApplied nữa, thay thế bằng IsCountHI và được set trong hàm CalcInvoiceItem (12/12/2014 09:07).
                //if (CurRegistration.PtInsuranceBenefit.GetValueOrDefault(0) > 0 && CurRegistration.HisID.GetValueOrDefault(0) > 0)
                //{
                //    item.HiApplied = true;
                //}
                //else
                //{
                //    item.HiApplied = false;
                //}

                if (used)
                {
                    item.V_ExamRegStatus = (long)AllLookupValues.ExamRegStatus.HOAN_TAT;
                }
                else
                {
                    item.V_ExamRegStatus = (long)AllLookupValues.ExamRegStatus.DANG_KY_KHAM;
                }

                if (EditingBillingInvoice.PclRequests == null)
                {
                    EditingBillingInvoice.PclRequests = new ObservableCollection<PatientPCLRequest>();
                }

                //▼===== #017: Loại bỏ việc đưa những dịch vụ mới vào cùng 1 phiếu chỉ định. Lý do phiếu chỉ định ban đầu là 2 dịch vụ, nếu đưa mới vào 3 dịch vụ nữa
                //             thì phiếu chỉ định sẽ tăng thành 5, không có vì nguyên nhân gì thay đổi chỉ định ban đầu của người khác.
                //PatientPCLRequest tempRequest = EditingBillingInvoice.PclRequests.Where(p => p.V_PCLRequestType == AllLookupValues.V_PCLRequestType.NOI_TRU
                //                                                                        && p.StaffID == item.StaffID
                //                                                                        && p.CreatedDate.Date == createdDate.Date
                //                                                                        && p.RecordState == RecordState.DETACHED).FirstOrDefault();
                //if (tempRequest == null)
                //{

                PatientPCLRequest tempRequest = new PatientPCLRequest
                {
                    PatientPCLRequestIndicators = new ObservableCollection<PatientPCLRequestDetail>(),
                    Diagnosis = eHCMSResources.Z0159_G1_CLSBNMoi,
                    StaffID = item.StaffID,
                    CreatedDate = createdDate,
                    V_PCLRequestType = AllLookupValues.V_PCLRequestType.NOI_TRU
                };

                if (used)
                {
                    tempRequest.V_PCLRequestStatus = AllLookupValues.V_PCLRequestStatus.CLOSE;
                }
                else
                {
                    tempRequest.V_PCLRequestStatus = AllLookupValues.V_PCLRequestStatus.OPEN;
                }
                tempRequest.RecordState = RecordState.DETACHED;
                tempRequest.EntityState = EntityState.DETACHED;
                tempRequest.MedicalInstructionDate = MedicalInstructionDateContent.DateTime;
                if (aucHoldConsultDoctor == null)
                {
                    tempRequest.DoctorStaffID = null;
                    tempRequest.DoctorStaff = null;
                }
                else
                {
                    tempRequest.DoctorStaffID = aucHoldConsultDoctor.StaffID;
                    if (tempRequest.DoctorStaff == null) tempRequest.DoctorStaff = new Staff();
                    tempRequest.DoctorStaff.StaffID = aucHoldConsultDoctor.StaffID;
                    tempRequest.DoctorStaff.FullName = aucHoldConsultDoctor.StaffName;
                }
                if (DepartmentContent != null && DepartmentContent.SelectedItem != null)
                {
                    if (SelLocationInDept != null && SelLocationInDept.DeptLocationID > 0)
                    {
                        tempRequest.ReqFromDeptLocID = SelLocationInDept.DeptLocationID;
                    }
                    //Bổ sung thuộc tính ReqFromDeptID để thực hiện chức năng chỉ định cận lâm sàng nội trú (Module Khám bệnh --> Cận lâm sàng nội trú). Nên thêm luôn bên này cho có dữ liệu trong bảng
                    tempRequest.ReqFromDeptID = DepartmentContent.SelectedItem.DeptID;
                }

                //▼===== #018:  Kiểm tra xem có phiếu chỉ định nào chưa lưu chưa nếu có thì add các dịch vụ thêm từ tạo bill vào chứ không phải
                //              mỗi dịch vụ tạo từ tạo bill lại phát sinh 1 phiếu chỉ định riêng.
                //bool IsOK = EditingBillingInvoice.PclRequests.Any(x => x.PatientPCLReqID == 0);
                bool IsOK = false;
                foreach (PatientPCLRequest req in EditingBillingInvoice.PclRequests)
                {
                    //20200601 TBL: Nếu khác BS chỉ định thì mới add phiếu mới
                    if (req.PatientPCLReqID == 0 && item.DoctorStaff.StaffID == req.DoctorStaffID)
                    {
                        IsOK = true;
                        break;
                    }
                }
                if (IsOK)
                {
                    EditingBillingInvoice.PclRequests.FirstOrDefault(x => x.PatientPCLReqID == 0 && x.DoctorStaffID == item.DoctorStaff.StaffID).PatientPCLRequestIndicators.Add(item);
                }
                else
                {
                    EditingBillingInvoice.PclRequests.Add(tempRequest);
                }
                //▲===== #018
                //}
                //▼====: #025
                if (CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.IsTreatmentCOVID
                && item.PCLExamType.InCategoryCOVID)
                {
                    item.IsCountPatientCOVID = true;
                }
                //▲====: #025
                GlobalsNAV.CalcInvoiceItem(item, IsHighTechServiceBill, CurRegistration);
                tempRequest.PatientPCLRequestIndicators.Add(item);
                AddMedRegItemBaseToNewInvoice(item);
                //▲===== #017
            }
            RegistrationInfoHasChanged = true;
        }

        //public void AddPCLItem(object pclItem, int qty, DateTime createdDate, bool used)
        //{
        //    if (!CheckDeptID())
        //    {
        //        return;
        //    }
        //    if (CurRegistration == null)
        //    {
        //        return;
        //    }
        //    var curItem = pclItem as PCLExamType;
        //    if (curItem != null)
        //    {
        //        PatientPCLRequestDetail item;
        //        item = new PatientPCLRequestDetail();
        //        item.StaffID = Globals.LoggedUserAccount.StaffID;
        //        item.CreatedDate = createdDate;
        //        item.MedProductType = AllLookupValues.MedProductType.CAN_LAM_SANG;
        //        item.PCLExamType = curItem;
        //        item.Qty = qty;


        //        if (CurRegistration.PtInsuranceBenefit.GetValueOrDefault(0) > 0 && CurRegistration.HisID.GetValueOrDefault(0) > 0)
        //        {
        //            item.HiApplied = true;
        //        }
        //        else
        //        {
        //            item.HiApplied = false;
        //        }

        //        if (used)
        //        {
        //            item.V_ExamRegStatus = (long)AllLookupValues.ExamRegStatus.HOAN_TAT;
        //        }
        //        else
        //        {
        //            item.V_ExamRegStatus = (long)AllLookupValues.ExamRegStatus.DANG_KY_KHAM;
        //        }
        //        if (EditingBillingInvoice.PclRequests != null)
        //        {
        //            var lstpcldetails = EditingBillingInvoice.PclRequests.SelectMany(x => x.PatientPCLRequestIndicators);
        //            if (lstpcldetails != null)
        //            {
        //                foreach (var row in lstpcldetails)
        //                {
        //                    if (row.PCLExamType.PCLExamTypeID == item.PCLExamType.PCLExamTypeID)
        //                    {
        //                        if (MessageBox.Show(item.PCLExamType.PCLExamTypeName + " này đã tồn tại.Bạn có muốn tiếp tục thêm không?", eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        //                        {
        //                            return;
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        if (EditingBillingInvoice.PclRequests == null)
        //        {
        //            EditingBillingInvoice.PclRequests = new ObservableCollection<PatientPCLRequest>();
        //        }
        //        PatientPCLRequest tempRequest = EditingBillingInvoice.PclRequests.Where(p => p.V_PCLRequestType == AllLookupValues.V_PCLRequestType.NOI_TRU
        //                                                                                && p.StaffID == item.StaffID
        //                                                                                && p.CreatedDate.Date == createdDate.Date
        //                                                                                && p.RecordState == RecordState.DETACHED).FirstOrDefault();
        //        if (tempRequest == null)
        //        {
        //            tempRequest = new PatientPCLRequest();
        //            tempRequest.PatientPCLRequestIndicators = new ObservableCollection<PatientPCLRequestDetail>();
        //            tempRequest.Diagnosis = eHCMSResources.Z0159_G1_CLSBNMoi;
        //            tempRequest.StaffID = item.StaffID;
        //            tempRequest.CreatedDate = createdDate;
        //            tempRequest.V_PCLRequestType = AllLookupValues.V_PCLRequestType.NOI_TRU;

        //            if (used)
        //            {
        //                tempRequest.V_PCLRequestStatus = AllLookupValues.V_PCLRequestStatus.CLOSE;
        //            }
        //            else
        //            {
        //                tempRequest.V_PCLRequestStatus = AllLookupValues.V_PCLRequestStatus.OPEN;
        //            }
        //            tempRequest.RecordState = RecordState.DETACHED;
        //            tempRequest.EntityState = EntityState.DETACHED;

        //            if (Globals.DeptLocation != null)
        //            {
        //                tempRequest.ReqFromDeptLocID = Globals.DeptLocation.DeptLocationID;
        //            }

        //            EditingBillingInvoice.PclRequests.Add(tempRequest);
        //        }
        //        CalcInvoiceItem(item);
        //        tempRequest.PatientPCLRequestIndicators.Add(item);
        //        AddMedRegItemBaseToNewInvoice(item);

        //        RegistrationInfoHasChanged = true;
        //    }
        //}

        public void AddGenMedDrug(object drug, decimal qty, DateTime createdDate)
        {
            if (!CheckDeptID())
            {
                return;
            }
            if (CurRegistration != null)
            {
                if (_selectedWarehouse == null || _selectedWarehouse.StoreID <= 0)
                {
                    MessageBox.Show(eHCMSResources.K0338_G1_ChonKho);
                    return;
                }
                RefGenMedProductDetails curItem = drug as RefGenMedProductDetails;
                if (curItem != null)
                {
                    if (curItem.Remaining < qty)
                    {
                        if (MessageBox.Show(eHCMSResources.A0975_G1_Msg_ConfTiepTucThem, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                    }
                    OutwardDrugClinicDept item = new OutwardDrugClinicDept
                    {
                        StaffID = Globals.LoggedUserAccount.StaffID,
                        CreatedDate = createdDate,
                        GenMedProductItem = curItem,
                        MedProductType = AllLookupValues.MedProductType.THUOC,
                        OutQuantity = qty,
                        Qty = qty
                    };

                    //KMx: Không sử dụng biến HiApplied nữa, thay thế bằng IsCountHI và được set trong hàm CalcInvoiceItem (12/12/2014 09:07).
                    //if (CurRegistration.PtInsuranceBenefit.GetValueOrDefault(0) > 0 && CurRegistration.HisID.GetValueOrDefault(0) > 0)
                    //{
                    //    item.HiApplied = true;
                    //}
                    //else
                    //{
                    //    item.HiApplied = false;
                    //}

                    if (EditingBillingInvoice.OutwardDrugClinicDeptInvoices != null)
                    {
                        var lstdrugDetails = EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Where(x => x.MedProductType == AllLookupValues.MedProductType.THUOC).SelectMany(x => x.OutwardDrugClinicDepts).Where(owd => owd.GenMedProductItem.GenMedProductID == item.GenMedProductItem.GenMedProductID);

                        if (lstdrugDetails != null && lstdrugDetails.Count() > 0)
                        {
                            if (MessageBox.Show(item.GenMedProductItem.BrandName + string.Format(" {0}", eHCMSResources.W0949_G1_W), eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                            {
                                return;
                            }
                        }
                        //if (lstdrugDetails != null)
                        //{
                        //    foreach (var row in lstdrugDetails)
                        //    {
                        //        if (row.GenMedProductItem.GenMedProductID == item.GenMedProductItem.GenMedProductID)
                        //        {
                        //            if (MessageBox.Show(row.GenMedProductItem.BrandName + " đã tồn tại. Bạn có muốn tiếp tục thêm không?", eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        //            {
                        //                return;
                        //            }
                        //        }
                        //    }

                        //}
                    }

                    if (EditingBillingInvoice.OutwardDrugClinicDeptInvoices == null)
                    {
                        EditingBillingInvoice.OutwardDrugClinicDeptInvoices = new ObservableCollection<OutwardDrugClinicDeptInvoice>();
                    }
                    var lastInvoice = EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Where(p => p.MedProductType == AllLookupValues.MedProductType.THUOC
                                                                    && p.outiID <= 0
                                                                    && p.CreatedDate.Date.ToShortDateString() == createdDate.Date.ToShortDateString()
                                                                    && p.StoreID == curItem.StoreID
                                                                    && p.StoreID == _selectedWarehouse.StoreID).FirstOrDefault();
                    if (lastInvoice == null)
                    {
                        lastInvoice = new OutwardDrugClinicDeptInvoice()
                        {
                            OutwardDrugClinicDepts = new ObservableCollection<OutwardDrugClinicDept>(),
                            MedProductType = AllLookupValues.MedProductType.THUOC,
                            OutDate = createdDate,
                            StaffID = Globals.LoggedUserAccount.StaffID,
                            StoreID = curItem.StoreID,
                            SelectedStorage = curItem.Storage
                        };
                        EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Add(lastInvoice);
                    }

                    item.DrugInvoice = lastInvoice;
                    //▼====: #025
                    if (CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.IsTreatmentCOVID
                    && item.GenMedProductItem.InCategoryCOVID)
                    {
                        item.IsCountPatientCOVID = true;
                    }
                    //▲====: #025
                    GlobalsNAV.CalcInvoiceItem(item, IsHighTechServiceBill, CurRegistration);
                    lastInvoice.OutwardDrugClinicDepts.Add(item);
                    AddMedRegItemBaseToNewInvoice(item);
                    RegistrationInfoHasChanged = true;
                }
            }
        }

        public void AddGenMedItem(object medItem, decimal qty, DateTime createdDate)
        {
            if (!CheckDeptID())
            {
                return;
            }
            if (CurRegistration != null)
            {
                if (_selectedWarehouse == null || _selectedWarehouse.StoreID <= 0)
                {
                    MessageBox.Show(eHCMSResources.K1973_G1_ChonKho);
                    return;
                }
                RefGenMedProductDetails curItem = medItem as RefGenMedProductDetails;
                if (curItem != null)
                {
                    if (curItem.Remaining < qty)
                    {
                        if (MessageBox.Show(eHCMSResources.A0975_G1_Msg_ConfTiepTucThem, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                    }
                    var item = new OutwardDrugClinicDept
                    {
                        StaffID = Globals.LoggedUserAccount.StaffID,
                        CreatedDate = createdDate,
                        GenMedProductItem = curItem,
                        MedProductType = AllLookupValues.MedProductType.Y_CU,
                        OutQuantity = qty,
                        Qty = qty
                    };

                    //KMx: Không sử dụng biến HiApplied nữa, thay thế bằng IsCountHI và được set trong hàm CalcInvoiceItem (12/12/2014 09:07).
                    //if (CurRegistration.PtInsuranceBenefit.GetValueOrDefault(0) > 0 && CurRegistration.HisID.GetValueOrDefault(0) > 0)
                    //{
                    //    item.HiApplied = true;
                    //}
                    //else
                    //{
                    //    item.HiApplied = false;
                    //}

                    if (EditingBillingInvoice.OutwardDrugClinicDeptInvoices != null)
                    {
                        var lstdrugDetails = EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Where(x => x.MedProductType == AllLookupValues.MedProductType.Y_CU).SelectMany(x => x.OutwardDrugClinicDepts).Where(owd => owd.GenMedProductItem.GenMedProductID == item.GenMedProductItem.GenMedProductID);

                        if (lstdrugDetails != null && lstdrugDetails.Count() > 0)
                        {
                            if (MessageBox.Show(item.GenMedProductItem.BrandName + string.Format(" {0}", eHCMSResources.W0949_G1_W), eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                            {
                                return;
                            }
                        }

                        //if (lstdrugDetails != null)
                        //{
                        //    foreach (var row in lstdrugDetails)
                        //    {
                        //        if (row.GenMedProductItem.GenMedProductID == item.GenMedProductItem.GenMedProductID)
                        //        {
                        //            if (MessageBox.Show(row.GenMedProductItem.BrandName + " đã tồn tại. Bạn có muốn tiếp tục thêm không?", eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        //            {
                        //                return;
                        //            }
                        //        }
                        //    }

                        //}
                    }

                    if (EditingBillingInvoice.OutwardDrugClinicDeptInvoices == null)
                    {
                        EditingBillingInvoice.OutwardDrugClinicDeptInvoices = new ObservableCollection<OutwardDrugClinicDeptInvoice>();
                    }
                    if (EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Where(p => p.MedProductType == AllLookupValues.MedProductType.Y_CU
                                                                    && p.outiID <= 0
                                                                    && p.CreatedDate.Date == createdDate.Date
                                                                    && p.StoreID == curItem.StoreID
                                                                    && p.StoreID == _selectedWarehouse.StoreID).FirstOrDefault() == null)
                    {

                        EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Add(new OutwardDrugClinicDeptInvoice()
                        {
                            OutwardDrugClinicDepts = new ObservableCollection<OutwardDrugClinicDept>(),
                            MedProductType = AllLookupValues.MedProductType.Y_CU,
                            OutDate = createdDate,
                            StaffID = Globals.LoggedUserAccount.StaffID,
                            StoreID = curItem.StoreID,
                            SelectedStorage = curItem.Storage
                        }
                       );
                    }
                    var lastInvoice = EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Last();

                    item.DrugInvoice = lastInvoice;
                    //▼====: #025
                    if (CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.IsTreatmentCOVID
                    && item.GenMedProductItem.InCategoryCOVID)
                    {
                        item.IsCountPatientCOVID = true;
                    }
                    //▲====: #025
                    GlobalsNAV.CalcInvoiceItem(item, IsHighTechServiceBill, CurRegistration);
                    lastInvoice.OutwardDrugClinicDepts.Add(item);
                    AddMedRegItemBaseToNewInvoice(item);
                    RegistrationInfoHasChanged = true;
                }
            }
        }

        public void AddGenChemical(object medItem, decimal qty, DateTime createdDate)
        {
            if (!CheckDeptID())
            {
                return;
            }
            if (CurRegistration != null)
            {
                if (_selectedWarehouse == null || _selectedWarehouse.StoreID <= 0)
                {
                    MessageBox.Show(eHCMSResources.K0338_G1_ChonKho);
                    return;
                }
                var curItem = medItem as RefGenMedProductDetails;
                if (curItem != null)
                {
                    if (curItem.Remaining < qty)
                    {
                        if (MessageBox.Show(eHCMSResources.A0975_G1_Msg_ConfTiepTucThem, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                    }
                    var item = new OutwardDrugClinicDept();
                    item.StaffID = Globals.LoggedUserAccount.StaffID;
                    item.CreatedDate = createdDate;
                    item.GenMedProductItem = curItem;
                    item.MedProductType = AllLookupValues.MedProductType.HOA_CHAT;
                    item.OutQuantity = qty;
                    item.Qty = qty;

                    //KMx: Không sử dụng biến HiApplied nữa, thay thế bằng IsCountHI và được set trong hàm CalcInvoiceItem (12/12/2014 09:07).
                    //if (CurRegistration.PtInsuranceBenefit.GetValueOrDefault(0) > 0 && CurRegistration.HisID.GetValueOrDefault(0) > 0)
                    //{
                    //    item.HiApplied = true;
                    //}
                    //else
                    //{
                    //    item.HiApplied = false;
                    //}
                    if (EditingBillingInvoice.OutwardDrugClinicDeptInvoices != null)
                    {
                        var lstdrugDetails = EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Where(x => x.MedProductType == AllLookupValues.MedProductType.HOA_CHAT).SelectMany(x => x.OutwardDrugClinicDepts).Where(owd => owd.GenMedProductItem.GenMedProductID == item.GenMedProductItem.GenMedProductID);
                        if (lstdrugDetails != null && lstdrugDetails.Count() > 0)
                        {
                            if (MessageBox.Show(item.GenMedProductItem.BrandName + string.Format(" {0}", eHCMSResources.W0949_G1_W), eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                            {
                                return;
                            }
                        }
                        //if (lstdrugDetails != null)
                        //{
                        //    foreach (var row in lstdrugDetails)
                        //    {
                        //        if (row.GenMedProductItem.GenMedProductID == item.GenMedProductItem.GenMedProductID)
                        //        {
                        //            if (MessageBox.Show(row.GenMedProductItem.BrandName + " đã tồn tại. Bạn có muốn tiếp tục thêm không?", eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        //            {
                        //                return;
                        //            }
                        //        }
                        //    }

                        //}
                    }

                    if (EditingBillingInvoice.OutwardDrugClinicDeptInvoices == null)
                    {
                        EditingBillingInvoice.OutwardDrugClinicDeptInvoices = new ObservableCollection<OutwardDrugClinicDeptInvoice>();
                    }
                    if (EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Where(p => p.MedProductType == AllLookupValues.MedProductType.HOA_CHAT
                                                                    && p.outiID <= 0
                                                                    && p.CreatedDate.Date == createdDate.Date
                                                                    && p.StoreID == curItem.StoreID
                                                                    && p.SelectedStorage.StoreID == _selectedWarehouse.StoreID).FirstOrDefault() == null)
                    {

                        EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Add(new OutwardDrugClinicDeptInvoice()
                        {
                            OutwardDrugClinicDepts = new ObservableCollection<OutwardDrugClinicDept>(),
                            MedProductType = AllLookupValues.MedProductType.HOA_CHAT,
                            OutDate = createdDate,
                            StaffID = Globals.LoggedUserAccount.StaffID,
                            StoreID = curItem.StoreID,
                            SelectedStorage = curItem.Storage
                        }
                        );
                    }
                    var lastInvoice = EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Last();

                    //▼====: #025
                    if (CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.IsTreatmentCOVID
                    && item.GenMedProductItem.InCategoryCOVID)
                    {
                        item.IsCountPatientCOVID = true;
                    }
                    //▲====: #025
                    item.DrugInvoice = lastInvoice;
                    GlobalsNAV.CalcInvoiceItem(item, IsHighTechServiceBill, CurRegistration);
                    lastInvoice.OutwardDrugClinicDepts.Add(item);
                    AddMedRegItemBaseToNewInvoice(item);
                    RegistrationInfoHasChanged = true;
                }
            }
        }

        public void AddDefaultPCLRequest(long medServiceID, int qty)
        {
            this.ShowBusyIndicator(eHCMSResources.K2887_G1_DangXuLy);
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new PatientRegistrationServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;

                        contract.BeginCreateNewPCLRequest(medServiceID,
                            Globals.DispatchCallback((asyncResult) =>
                            {
                                PatientPCLRequest item = null;
                                PatientPCLRequest externalRequest = null;

                                try
                                {
                                    contract.EndCreateNewPCLRequest(out item, out externalRequest, asyncResult);

                                    ObservableCollection<PCLExamType> PCLExamTypeList = new ObservableCollection<PCLExamType>();
                                    ObservableCollection<PCLExamType> PCLExamTypeList_Ext = new ObservableCollection<PCLExamType>();

                                    if (item != null && item.PatientPCLRequestIndicators != null)
                                    {
                                        foreach (var requestDetails in item.PatientPCLRequestIndicators)
                                        {
                                            PCLExamTypeList.Add(requestDetails.PCLExamType);
                                        }
                                        if (PCLExamTypeList != null && PCLExamTypeList.Count > 0)
                                        {
                                            CheckAndAddAllPCL(PCLExamTypeList, qty, (DateTime)UseAtDateContent.DateTime, false);
                                        }
                                    }

                                    if (externalRequest != null && externalRequest.PatientPCLRequestIndicators != null)
                                    {
                                        foreach (var requestDetails in externalRequest.PatientPCLRequestIndicators)
                                        {
                                            PCLExamTypeList_Ext.Add(requestDetails.PCLExamType);
                                        }
                                        if (PCLExamTypeList_Ext != null && PCLExamTypeList_Ext.Count > 0)
                                        {
                                            CheckAndAddAllPCL(PCLExamTypeList_Ext, qty, (DateTime)UseAtDateContent.DateTime, false);
                                        }
                                    }
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

        //KMx: Lỗi 1: Khi thêm 1 món, mà món đó đã có 2 dòng rồi, thì cảnh báo món đó 2 lần. Lỗi 2: Người dùng nhập SL > 1 mà chỉ add vào 1. Không sử dụng hàm này nữa (15/12/2014 11:32).
        //public void AddDefaultPCLRequest(long medServiceID)
        //{
        //    var t = new Thread(() =>
        //    {
        //        Globals.EventAggregator.Publish(new BusyEvent
        //        {
        //            IsBusy = true,
        //            Message = "Đang thêm các dịch vụ Cận Lâm Sàng..."
        //        });
        //        try
        //        {
        //            using (var serviceFactory = new PatientRegistrationServiceClient())
        //            {
        //                var contract = serviceFactory.ServiceInstance;

        //                contract.BeginCreateNewPCLRequest(medServiceID,
        //                    Globals.DispatchCallback((asyncResult) =>
        //                    {
        //                        PatientPCLRequest item = null;
        //                        PatientPCLRequest externalRequest = null;
        //                        try
        //                        {
        //                            contract.EndCreateNewPCLRequest(out item, out externalRequest, asyncResult);
        //                            string Mgsshow = "";

        //                            if (item != null && item.PatientPCLRequestIndicators != null)
        //                            {
        //                                if (EditingBillingInvoice.PclRequests != null)
        //                                {
        //                                    var lstpcls = EditingBillingInvoice.PclRequests.SelectMany(x => x.PatientPCLRequestIndicators);
        //                                    if (lstpcls != null)
        //                                    {
        //                                        foreach (var row1 in lstpcls)
        //                                        {
        //                                            foreach (var row2 in item.PatientPCLRequestIndicators)
        //                                            {
        //                                                if (row1.PCLExamType.PCLExamTypeID == row2.PCLExamType.PCLExamTypeID)
        //                                                {
        //                                                    Mgsshow = Mgsshow + row1.PCLExamType.PCLExamTypeName + ",";
        //                                                    break;
        //                                                }
        //                                            }
        //                                        }
        //                                        if (!string.IsNullOrEmpty(Mgsshow))
        //                                        {
        //                                            if (MessageBox.Show(Mgsshow + " đã tồn tại. Bạn có muốn tiếp tục thêm không?", eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        //                                            {
        //                                                return;
        //                                            }
        //                                        }
        //                                    }
        //                                }

        //                                foreach (var requestDetails in item.PatientPCLRequestIndicators)
        //                                {
        //                                    AddPCLItem_Goi(requestDetails.PCLExamType, 1, (DateTime)UseAtDateContent.DateTime, false);
        //                                }
        //                            }

        //                            if (externalRequest != null && externalRequest.PatientPCLRequestIndicators != null)
        //                            {
        //                                if (EditingBillingInvoice.PclRequests != null)
        //                                {
        //                                    var lstpcls = EditingBillingInvoice.PclRequests.SelectMany(x => x.PatientPCLRequestIndicators);
        //                                    if (lstpcls != null)
        //                                    {
        //                                        foreach (var row1 in lstpcls)
        //                                        {
        //                                            foreach (var row2 in externalRequest.PatientPCLRequestIndicators)
        //                                            {
        //                                                if (row1.PCLExamType.PCLExamTypeID == row2.PCLExamType.PCLExamTypeID)
        //                                                {
        //                                                    Mgsshow = Mgsshow + row1.PCLExamType.PCLExamTypeName + ",";
        //                                                    break;
        //                                                }
        //                                            }
        //                                        }
        //                                        if (!string.IsNullOrEmpty(Mgsshow))
        //                                        {
        //                                            if (MessageBox.Show(Mgsshow + " đã tồn tại.Bạn có muốn tiếp tục thêm không?", eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        //                                            {
        //                                                return;
        //                                            }
        //                                        }
        //                                    }
        //                                }

        //                                foreach (var requestDetails in externalRequest.PatientPCLRequestIndicators)
        //                                {
        //                                    AddPCLItem_Goi(requestDetails.PCLExamType, 1, (DateTime)UseAtDateContent.DateTime, false);
        //                                }
        //                            }
        //                        }
        //                        catch (FaultException<AxException> fault)
        //                        {
        //                            ClientLoggerHelper.LogInfo(fault.ToString());
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            ClientLoggerHelper.LogInfo(ex.ToString());
        //                        }

        //                    }), null);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
        //        }
        //        finally
        //        {
        //            Globals.IsBusy = false;
        //        }
        //    });
        //    t.Start();
        //}

        #region Danh sách bắt event từ màn hình khác
        public void Handle(ItemSelected<Patient> message)
        {
            if (GetView() != null && message != null)
            {
                CurPatient = message.Item;
                CurRegistration = null;
                if (CurPatient != null)
                {
                    SetCurrentPatient(CurPatient);
                }
            }
        }
        public void Handle(ItemSelected<PatientRegistration> message)
        {
            if (GetView() != null && message != null && message.Item != null)
            {
                OpenRegistration(message.Item.PtRegistrationID);
                MedicalInstructionDateContent.DateTime = new DateTime(Globals.GetCurServerDateTime().Year, Globals.GetCurServerDateTime().Month, Globals.GetCurServerDateTime().Day, Globals.GetCurServerDateTime().Hour, Globals.GetCurServerDateTime().Minute, 0);
                ResultDateContent.DateTime = new DateTime(Globals.GetCurServerDateTime().Year, Globals.GetCurServerDateTime().Month, Globals.GetCurServerDateTime().Day, Globals.GetCurServerDateTime().Hour, Globals.GetCurServerDateTime().Minute, 0);
            }
        }
        //▼====== #010
        public void Handle(InPatientRegistrationSelectedForInPatientRegistration message)
        {
            if (message != null && message.Source != null)
            {
                AllRegistrationItems = new ObservableCollection<MedRegItemBase>();
                AllRegistrationItemsFilter = new ObservableCollection<MedRegItemBase>();
                OpenRegistration(message.Source.PtRegistrationID);
            }
        }
        //▲====== #010
        public void Handle(ResultNotFound<Patient> message)
        {
            if (GetView() != null && message != null)
            {
                //Thông báo không tìm thấy bệnh nhân.
                MessageBoxResult result = MessageBox.Show(eHCMSResources.A0728_G1_Msg_ConfTiepTucTimBN, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    void onInitDlg(IFindPatient vm)
                    {
                        vm.SearchCriteria = message.SearchCriteria as PatientSearchCriteria;
                        var criteria = message.SearchCriteria as PatientSearchCriteria;
                        vm.SearchCriteria = criteria;
                    }
                    GlobalsNAV.ShowDialog<IFindPatient>(onInitDlg);
                }
            }
        }

        public void Handle(PayForRegistrationCompleted message)
        {
            if (GetView() == null || message == null)
            {
                return;
            }
            //Load lai dang ky:
            var payment = message.Payment;
            if (payment != null && payment.PatientTransaction != null && payment.PatientTransaction.PtRegistrationID.HasValue)
            {
                //Show Report:
                Action<IPaymentReport> onInitDlg = delegate (IPaymentReport reportVm)
                {
                    reportVm.PaymentID = payment.PtTranPaymtID;
                };
                GlobalsNAV.ShowDialog<IPaymentReport>(onInitDlg);
                OpenRegistration(payment.PatientTransaction.PtRegistrationID.Value);
            }
        }

        public void Handle(SaveAndPayForRegistrationCompleted message)
        {
            if (GetView() != null && message != null)
            {
                //Load lai dang ky:
                var payment = message.Payment;
                if (payment != null && payment.PatientTransaction != null && payment.PatientTransaction.PtRegistrationID.HasValue)
                {
                    //Show Report:
                    Action<IPaymentReport> onInitDlg = delegate (IPaymentReport reportVm)
                    {
                        reportVm.PaymentID = payment.PtTranPaymtID;
                    };
                    GlobalsNAV.ShowDialog<IPaymentReport>(onInitDlg);
                    if (message.RegistrationInfo != null)
                    {
                        ShowOldRegistration(message.RegistrationInfo);
                    }
                    else
                    {
                        OpenRegistration(payment.PatientTransaction.PtRegistrationID.Value);
                    }
                }
            }
        }
        //public void ConfirmHiItem(object hiItem)
        //{
        //    ConfirmedHiItem = hiItem as HealthInsurance;
        //    if (CurRegistration != null)
        //    {
        //        CurRegistration.HealthInsurance = ConfirmedHiItem;
        //    }

        //}
        //public void ConfirmPaperReferal(object referal)
        //{
        //    ConfirmedPaperReferal = referal as PaperReferal;

        //}

        public void Handle(ItemSelected<RefMedicalServiceItem> message)
        {
            if (GetView() != null)
            {
                //Reset so luong dich vu dang ky = 1
                ServiceQty = 1;
                BedQty = 1;
                BloodQty = 1;
                SugeryQty = 1;
            }
        }
        #endregion
        public void AddServiceCmd()
        {
            PopupModifyPrice_Type = AllLookupValues.PopupModifyPrice_Type.INSERT_DICHVU;
            ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults;
            if (!ValidateServiceItem(out validationResults))
            {
                Globals.EventAggregator.Publish(new ValidateFailedEvent() { ValidationResults = validationResults });
                return;
            }

            // TxD 15/07/2016: Added the following checking so the IModifyBillingInvItem dialog will only pop up when both DeptID and selected item (to be added) are valid  
            Int32 selItem_V_NewPricetype = 0;
            if (CheckDeptID() && (InPatientSelectServiceContent.MedServiceItem != null))   // TxD 15/07/2016 Check this just in case
            {
                selItem_V_NewPricetype = Convert.ToInt32(InPatientSelectServiceContent.MedServiceItem.V_NewPriceType);
            }
            else
            {
                return;
            }

            if (selItem_V_NewPricetype > 0 && (selItem_V_NewPricetype == Convert.ToInt32(AllLookupValues.V_NewPriceType.Unknown_PriceType) || selItem_V_NewPricetype == Convert.ToInt32(AllLookupValues.V_NewPriceType.Updatable_PriceType)))
            {
                PatientRegistrationDetail item = new PatientRegistrationDetail();
                item.RefMedicalServiceItem = InPatientSelectServiceContent.MedServiceItem.DeepCopy();

                // Hpt 21/11/2015: Nếu dịch vụ thuộc loại giá thay đổi hoặc không giá, sẽ cập nhật giá khi thêm dịch vụ vào bill nên phải lấy giá sau khi cập nhật chứ không thể lấy giá từ InPatientSelectServiceContent
                void onInitDlg(IModifyBillingInvItem vm)
                {
                    vm.ModifyBillingInvItem.IsModItemOK = false;
                    vm.ModifyBillingInvItem = item;
                    vm.ModifyBillingInvItem.V_NewPriceType = item.RefMedicalServiceItem.V_NewPriceType;
                    vm.PopupType = 1;
                    vm.Init();
                }
                GlobalsNAV.ShowDialog<IModifyBillingInvItem>(onInitDlg);
            }
            else
            {
                AddGenMedService(InPatientSelectServiceContent.MedServiceItem, ServiceQty.Value, (DateTime)UseAtDateContent.DateTime);
            }
        }
        public void AddGeneralSugeryItemCmd()
        {
            PopupModifyPrice_Type = AllLookupValues.PopupModifyPrice_Type.INSERT_PHAUTHUAT_THUTHUAT;
            ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults;
            if (!ValidateGeneralSugeryItem(out validationResults))
            {
                Globals.EventAggregator.Publish(new ValidateFailedEvent() { ValidationResults = validationResults });
                return;
            }

            // TxD 15/07/2016: Added the following checking so the IModifyBillingInvItem dialog will only pop up when both DeptID and selected item (to be added) are valid  
            Int32 selItem_V_NewPricetype = 0;
            if (CheckDeptID() && (SelectGeneralSugeryContent.MedServiceItem != null))   // TxD 15/07/2016 Check this just in case
            {
                selItem_V_NewPricetype = Convert.ToInt32(SelectGeneralSugeryContent.MedServiceItem.V_NewPriceType);
            }
            else
            {
                return;
            }

            if (selItem_V_NewPricetype > 0 && (selItem_V_NewPricetype == Convert.ToInt32(AllLookupValues.V_NewPriceType.Unknown_PriceType) || selItem_V_NewPricetype == Convert.ToInt32(AllLookupValues.V_NewPriceType.Updatable_PriceType)))
            {
                PatientRegistrationDetail item = new PatientRegistrationDetail();
                item.RefMedicalServiceItem = SelectGeneralSugeryContent.MedServiceItem.DeepCopy();

                // Hpt 21/11/2015: Nếu dịch vụ thuộc loại giá thay đổi hoặc không giá, sẽ cập nhật giá khi thêm dịch vụ vào bill nên phải lấy giá sau khi cập nhật chứ không thể lấy giá từ InPatientSelectServiceContent
                void onInitDlg(IModifyBillingInvItem vm)
                {
                    vm.ModifyBillingInvItem.IsModItemOK = false;
                    vm.ModifyBillingInvItem = item;
                    vm.ModifyBillingInvItem.V_NewPriceType = item.RefMedicalServiceItem.V_NewPriceType;
                    vm.PopupType = 1;
                    vm.Init();
                }
                GlobalsNAV.ShowDialog<IModifyBillingInvItem>(onInitDlg);
            }
            else
            {
                AddGenMedService(SelectGeneralSugeryContent.MedServiceItem, SugeryQty.Value, (DateTime)UseAtDateContent.DateTime);
            }
        }
        public void AddBedItemCmd()
        {
            PopupModifyPrice_Type = AllLookupValues.PopupModifyPrice_Type.INSERT_GIUONG;
            ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults;
            if (!ValidateBedItem(out validationResults))
            {
                Globals.EventAggregator.Publish(new ValidateFailedEvent() { ValidationResults = validationResults });
                return;
            }

            // TxD 15/07/2016: Added the following checking so the IModifyBillingInvItem dialog will only pop up when both DeptID and selected item (to be added) are valid  
            Int32 selItem_V_NewPricetype = 0;
            if (CheckDeptID() && (SelectBedContent.MedServiceItem != null))   // TxD 15/07/2016 Check this just in case
            {
                selItem_V_NewPricetype = Convert.ToInt32(SelectBedContent.MedServiceItem.V_NewPriceType);
            }
            else
            {
                return;
            }

            if (selItem_V_NewPricetype > 0 && (selItem_V_NewPricetype == Convert.ToInt32(AllLookupValues.V_NewPriceType.Unknown_PriceType) || selItem_V_NewPricetype == Convert.ToInt32(AllLookupValues.V_NewPriceType.Updatable_PriceType)))
            {
                PatientRegistrationDetail item = new PatientRegistrationDetail();
                item.RefMedicalServiceItem = SelectBedContent.MedServiceItem.DeepCopy();

                // Hpt 21/11/2015: Nếu dịch vụ thuộc loại giá thay đổi hoặc không giá, sẽ cập nhật giá khi thêm dịch vụ vào bill nên phải lấy giá sau khi cập nhật chứ không thể lấy giá từ InPatientSelectServiceContent
                void onInitDlg(IModifyBillingInvItem vm)
                {
                    vm.ModifyBillingInvItem.IsModItemOK = false;
                    vm.ModifyBillingInvItem = item;
                    vm.ModifyBillingInvItem.V_NewPriceType = item.RefMedicalServiceItem.V_NewPriceType;
                    vm.PopupType = 1;
                    vm.Init();
                }
                GlobalsNAV.ShowDialog<IModifyBillingInvItem>(onInitDlg);
            }
            else
            {
                AddGenMedService(SelectBedContent.MedServiceItem, BedQty.Value, (DateTime)UseAtDateContent.DateTime);
            }
        }
        public void AddBloodItemCmd()
        {
            PopupModifyPrice_Type = AllLookupValues.PopupModifyPrice_Type.INSERT_MAU;
            ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults;
            if (!ValidateBloodItem(out validationResults))
            {
                Globals.EventAggregator.Publish(new ValidateFailedEvent() { ValidationResults = validationResults });
                return;
            }

            // TxD 15/07/2016: Added the following checking so the IModifyBillingInvItem dialog will only pop up when both DeptID and selected item (to be added) are valid  
            Int32 selItem_V_NewPricetype = 0;
            if (CheckDeptID() && (SelectBloodContent.MedServiceItem != null))   // TxD 15/07/2016 Check this just in case
            {
                selItem_V_NewPricetype = Convert.ToInt32(SelectBloodContent.MedServiceItem.V_NewPriceType);
            }
            else
            {
                return;
            }

            if (selItem_V_NewPricetype > 0 && (selItem_V_NewPricetype == Convert.ToInt32(AllLookupValues.V_NewPriceType.Unknown_PriceType) || selItem_V_NewPricetype == Convert.ToInt32(AllLookupValues.V_NewPriceType.Updatable_PriceType)))
            {
                PatientRegistrationDetail item = new PatientRegistrationDetail();
                item.RefMedicalServiceItem = SelectBloodContent.MedServiceItem.DeepCopy();

                // Hpt 21/11/2015: Nếu dịch vụ thuộc loại giá thay đổi hoặc không giá, sẽ cập nhật giá khi thêm dịch vụ vào bill nên phải lấy giá sau khi cập nhật chứ không thể lấy giá từ InPatientSelectServiceContent
                void onInitDlg(IModifyBillingInvItem vm)
                {
                    vm.ModifyBillingInvItem.IsModItemOK = false;
                    vm.ModifyBillingInvItem = item;
                    vm.ModifyBillingInvItem.V_NewPriceType = item.RefMedicalServiceItem.V_NewPriceType;
                    vm.PopupType = 1;
                    vm.Init();
                }
                GlobalsNAV.ShowDialog<IModifyBillingInvItem>(onInitDlg);
            }
            else
            {
                AddGenMedService(SelectBloodContent.MedServiceItem, BloodQty.Value, (DateTime)UseAtDateContent.DateTime);
            }
        }
        private void CheckDoctorAndDate(ref ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> result)
        {
            if (RequireDoctorAndDate && (aucHoldConsultDoctor == null || aucHoldConsultDoctor.StaffID <= 0 || MedicalInstructionDateContent.DateTime == null))
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z2184_G1_NhapDayDuNgayYLVaBS, new string[] { "MedicalInstructionDate" });
                result.Add(item);
            }
            /*▼====: #009*/
            if (aucHoldConsultDoctor.StaffID > 0 && aucHoldConsultDoctor.CertificateNumber == null)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z2295_G1_BSKoCoCCHN, new string[] { "MedicalInstructionDate" });
                result.Add(item);
            }
            /*▲====: #009*/
            //▼====: #006
            if (RequireDoctorAndDate && MedicalInstructionDateContent.DateTime.HasValue && MedicalInstructionDateContent.DateTime != null)
            {
                MedicalInstructionDateContent.DateTime = Globals.ApplyValidMedicalInstructionDate(MedicalInstructionDateContent.DateTime.GetValueOrDefault(), CurRegistration);
                CheckValidMedicalInstructionDate(MedicalInstructionDateContent.DateTime.Value, true, ref result);
            }
            //▲====: #006
        }
        //▼====: #006
        private bool CheckValidMedicalInstructionDate(DateTime? aMedicalInstructionDate, bool IsReturnResult, ref ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> result, bool IsCheckNNV = true)
        {
            if (!aMedicalInstructionDate.HasValue || aMedicalInstructionDate == null || !RequireDoctorAndDate || CurRegistration == null)
                return true;
            if ((CurRegistration.AdmissionInfo == null || !CurRegistration.AdmissionInfo.AdmissionDate.HasValue || CurRegistration.AdmissionInfo.AdmissionDate == null)
                && CurRegistration.ExamDate != null && aMedicalInstructionDate < CurRegistration.ExamDate)
            {
                if (IsReturnResult)
                {
                    var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z2187_G1_NYLKhongNhoHonNDK, new string[] { "MedicalInstructionDate" });
                    result.Add(item);
                }
                else
                {
                    MessageBox.Show(eHCMSResources.Z2187_G1_NYLKhongNhoHonNDK, eHCMSResources.T0074_G1_I, MessageBoxButton.OK);
                }
                return false;
            }
            if (CurRegistration.AdmissionInfo == null) return true;
            if (CurRegistration.AdmissionInfo.DischargeDetailRecCreatedDate.HasValue
                && CurRegistration.AdmissionInfo.DischargeDetailRecCreatedDate != null
                && aMedicalInstructionDate > CurRegistration.AdmissionInfo.DischargeDetailRecCreatedDate)
            {
                if (IsReturnResult)
                {
                    var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z2188_G1_NYLKhongLonHonNXV, new string[] { "MedicalInstructionDate" });
                    result.Add(item);
                }
                else
                {
                    MessageBox.Show(eHCMSResources.Z2188_G1_NYLKhongLonHonNXV, eHCMSResources.T0074_G1_I, MessageBoxButton.OK);
                }
                return false;
            }
            if (CurRegistration.AdmissionInfo.AdmissionDate.HasValue
                && CurRegistration.AdmissionInfo.AdmissionDate != null
                && aMedicalInstructionDate < CurRegistration.AdmissionInfo.AdmissionDate && IsCheckNNV)
            {
                if (IsReturnResult)
                {
                    var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z2183_G1_NgayYLKhongNhoHonNNV, new string[] { "MedicalInstructionDate" });
                    result.Add(item);
                }
                else
                {
                    MessageBox.Show(eHCMSResources.Z2183_G1_NgayYLKhongNhoHonNNV, eHCMSResources.T0074_G1_I, MessageBoxButton.OK);
                }
                return false;
            }
            return true;
        }
        //▲====: #006
        private bool CheckDoctorAndDate(PatientPCLRequestDetail item)
        {
            return item != null && item.RecordState != RecordState.DELETED && (item.HIAllowedPrice > 0 || (item.ChargeableItem != null && item.ChargeableItem.HIAllowedPrice > 0)) && (item.DoctorStaff == null || item.DoctorStaff.StaffID <= 0 || item.MedicalInstructionDate == null);
        }
        private bool CheckDoctorAndDate(PatientPCLRequest item)
        {
            return item != null && item.RecordState != RecordState.DELETED && item.PatientPCLRequestIndicators != null && item.PatientPCLRequestIndicators.Any(x => CheckDoctorAndDate(x));
        }
        private bool CheckDoctorAndDate(PatientRegistrationDetail item)
        {
            return item != null && item.RecordState != RecordState.DELETED && (item.HIAllowedPrice > 0 || (item.ChargeableItem != null && item.ChargeableItem.HIAllowedPrice > 0)) && (item.DoctorStaff == null || item.DoctorStaff.StaffID <= 0 || item.MedicalInstructionDate == null);
        }
        private bool ValidateServiceItem(out ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> result)
        {
            result = new ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult>();

            //if (!SelectedDate.HasValue)
            //{
            //    var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0161_G1_HayChonNgSDDV, new string[] { "SelectedDate" });
            //    result.Add(item);
            //}

            if (InPatientSelectServiceContent.MedServiceItem == null || InPatientSelectServiceContent.MedServiceItem.MedServiceID < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0162_G1_HayChonDV, new string[] { "MedServiceItem" });
                result.Add(item);
            }
            if (ServiceQty.GetValueOrDefault(0) < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0163_G1_NhapGTriSLgDV, new string[] { "ServiceQty" });
                result.Add(item);
            }
            if (InPatientSelectServiceContent != null && InPatientSelectServiceContent.MedServiceItem != null && InPatientSelectServiceContent.MedServiceItem.HIAllowedPrice > 0)
            {
                CheckDoctorAndDate(ref result);
            }
            if (result.Count > 0)
            {
                return false;
            }
            return true;
        }
        private bool ValidateGeneralSugeryItem(out ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> result)
        {
            result = new ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult>();

            //if (!SelectedDate.HasValue)
            //{
            //    var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0161_G1_HayChonNgSDDV, new string[] { "SelectedDate" });
            //    result.Add(item);
            //}

            if (SelectGeneralSugeryContent.MedServiceItem == null || SelectGeneralSugeryContent.MedServiceItem.MedServiceID < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0162_G1_HayChonDV, new string[] { "MedServiceItem" });
                result.Add(item);
            }
            if (ServiceQty.GetValueOrDefault(0) < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0163_G1_NhapGTriSLgDV, new string[] { "ServiceQty" });
                result.Add(item);
            }
            if (SelectGeneralSugeryContent != null && SelectGeneralSugeryContent.MedServiceItem != null && SelectGeneralSugeryContent.MedServiceItem.HIAllowedPrice > 0)
            {
                CheckDoctorAndDate(ref result);
            }
            if (result.Count > 0)
            {
                return false;
            }
            return true;
        }
        private bool ValidateBedItem(out ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> result)
        {
            result = new ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult>();

            //if (!SelectedDate.HasValue)
            //{
            //    var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0161_G1_HayChonNgSDDV, new string[] { "SelectedDate" });
            //    result.Add(item);
            //}

            if (SelectBedContent.MedServiceItem == null || SelectBedContent.MedServiceItem.MedServiceID < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0162_G1_HayChonDV, new string[] { "MedServiceItem" });
                result.Add(item);
            }
            if (ServiceQty.GetValueOrDefault(0) < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0163_G1_NhapGTriSLgDV, new string[] { "ServiceQty" });
                result.Add(item);
            }
            if (SelectBedContent != null && SelectBedContent.MedServiceItem != null && SelectBedContent.MedServiceItem.HIAllowedPrice > 0)
            {
                CheckDoctorAndDate(ref result);
            }
            if (result.Count > 0)
            {
                return false;
            }
            return true;
        }
        private bool ValidateBloodItem(out ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> result)
        {
            result = new ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult>();

            //if (!SelectedDate.HasValue)
            //{
            //    var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0161_G1_HayChonNgSDDV, new string[] { "SelectedDate" });
            //    result.Add(item);
            //}

            if (SelectBloodContent.MedServiceItem == null || SelectBloodContent.MedServiceItem.MedServiceID < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0162_G1_HayChonDV, new string[] { "MedServiceItem" });
                result.Add(item);
            }
            if (ServiceQty.GetValueOrDefault(0) < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0163_G1_NhapGTriSLgDV, new string[] { "ServiceQty" });
                result.Add(item);
            }
            if (SelectBloodContent != null && SelectBloodContent.MedServiceItem != null && SelectBloodContent.MedServiceItem.HIAllowedPrice > 0)
            {
                CheckDoctorAndDate(ref result);
            }
            if (result.Count > 0)
            {
                return false;
            }
            return true;
        }

        public void Handle(ItemSelected<PCLExamType> message)
        {
            if (GetView() != null)
            {
                PclQty = 1;
                PclQtyLAB = 1;
            }
        }

        public void Handle(DoubleClick message)
        {
            if (message == null || message.Source == null || message.EventArgs == null || message.EventArgs.Value == null)
            {
                return;
            }
            //if (message.Source == SelectGeneralSugeryContent)
            //{
            //    AddPCLServiceItem(SelectGeneralSugeryContent);
            //}
            else if (message.Source == SelectPCLContent)
            {
                AddPCLServiceItem(SelectPCLContent);
            }
        }

        private void AddPCLServiceItem(IInPatientSelectPcl aSelectPCLContent)
        {
            PopupModifyPrice_Type = AllLookupValues.PopupModifyPrice_Type.INSERT_PCL_HINHANH;
            ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults;
            if (!ValidatePclItem(aSelectPCLContent, out validationResults))
            {
                Globals.EventAggregator.Publish(new ValidateFailedEvent() { ValidationResults = validationResults });
                return;
            }
            //bool used = SelectPCLContent != null && SelectPCLContent.Used;
            bool used = true;/*A Tuan quyet dinh lam phieu CLS noi tru tai man hinh nay thi la da thuc hien roi*/
            //AddPCLItem(SelectPCLContent.SelectedPCLExamType, PclQty.Value, (DateTime)UseAtDateContent.DateTime, used);
            CheckAndAddPCL(aSelectPCLContent.SelectedPCLExamType, PclQty.Value, (DateTime)UseAtDateContent.DateTime, used);
        }

        public void AddPclExamTypeCmd()
        {
            AddPCLServiceItem(SelectPCLContent);
        }

        private bool ValidatePclItem(IInPatientSelectPcl aSelectPCLContent, out ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> result)
        {
            result = new ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult>();

            //if (!SelectedDate.HasValue)
            //{
            //    var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0161_G1_HayChonNgSDDV, new string[] { "SelectedDate" });
            //    result.Add(item);
            //}

            if (aSelectPCLContent.SelectedPCLExamType == null || aSelectPCLContent.SelectedPCLExamType.PCLExamTypeID < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0164_G1_HayChonDVCLS, new string[] { "SelectedPclExamType" });
                result.Add(item);
            }
            if (PclQty.GetValueOrDefault(0) < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0163_G1_NhapGTriSLgDV, new string[] { "PclQty" });
                result.Add(item);
            }
            if (aSelectPCLContent != null && aSelectPCLContent.SelectedPCLExamType != null && aSelectPCLContent.SelectedPCLExamType.HIAllowedPrice > 0)
            {
                CheckDoctorAndDate(ref result);
            }
            if (result.Count > 0)
            {
                return false;
            }
            return true;
        }

        public void AddDrugItemCmd()
        {
            ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults;
            if (!ValidateDrugItem(out validationResults))
            {
                Globals.EventAggregator.Publish(new ValidateFailedEvent() { ValidationResults = validationResults });
                return;
            }

            AddGenMedDrug(SelectDrugContent.SelectedMedProduct, DrugQty.Value, (DateTime)UseAtDateContent.DateTime);
        }
        private bool ValidateDrugItem(out ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> result)
        {
            result = new ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult>();

            //if (!SelectedDate.HasValue)
            //{
            //    var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0161_G1_HayChonNgSDDV, new string[] { "SelectedDate" });
            //    result.Add(item);
            //}

            if (SelectDrugContent.SelectedMedProduct == null || SelectDrugContent.SelectedMedProduct.GenMedProductID < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0165_G1_HayChonThuoc, new string[] { "SelectedDrug" });
                result.Add(item);
            }
            if (DrugQty.GetValueOrDefault(0) < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0163_G1_NhapGTriSLgDV, new string[] { "DrugQty" });
                result.Add(item);
            }
            if (result.Count > 0)
            {
                return false;
            }
            return true;
        }

        public void Handle(ItemSelected<RefGenMedProductDetails> message)
        {
            if (GetView() != null && message.Item != null && message.Item != null)
            {
                long type = message.Item.V_MedProductType;
                if (type == (long)AllLookupValues.MedProductType.Y_CU)
                {
                    MedItemQty = 1;
                    AddMedItemCmd();
                }
                else if (type == (long)AllLookupValues.MedProductType.HOA_CHAT)
                {
                    ChemicalQty = 1;
                    AddChemicalCmd();
                }
                else if (type == (long)AllLookupValues.MedProductType.THUOC)
                {
                    DrugQty = 1;
                    AddDrugItemCmd();
                }
            }
        }

        public void AddMedItemCmd()
        {
            ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults;
            if (!ValidateMedItem(out validationResults))
            {
                Globals.EventAggregator.Publish(new ValidateFailedEvent() { ValidationResults = validationResults });
                return;
            }

            AddGenMedItem(MedItemContent.SelectedMedProduct, MedItemQty.Value, (DateTime)UseAtDateContent.DateTime);
        }

        private bool ValidateMedItem(out ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> result)
        {
            result = new ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult>();

            //if (!SelectedDate.HasValue)
            //{
            //    var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0161_G1_HayChonNgSDDV, new string[] { "SelectedDate" });
            //    result.Add(item);
            //}

            if (MedItemContent.SelectedMedProduct == null || MedItemContent.SelectedMedProduct.GenMedProductID < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0166_G1_HayChonYCu, new string[] { "SelectedInwardDrug" });
                result.Add(item);
            }
            if (MedItemQty.GetValueOrDefault(0) < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0167_G1_NhapGTriSLgYCu, new string[] { "MedItemQty" });
                result.Add(item);
            }
            if (result.Count > 0)
            {
                return false;
            }
            return true;
        }

        public void AddChemicalCmd()
        {
            ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults;
            if (!ValidateChemicalItem(out validationResults))
            {
                Globals.EventAggregator.Publish(new ValidateFailedEvent() { ValidationResults = validationResults });
                return;
            }

            AddGenChemical(ChemicalItemContent.SelectedMedProduct, ChemicalQty.Value, (DateTime)UseAtDateContent.DateTime);
        }

        private bool ValidateChemicalItem(out ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> result)
        {
            result = new ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult>();

            //if (!SelectedDate.HasValue)
            //{
            //    var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0161_G1_HayChonNgSDDV, new string[] { "SelectedDate" });
            //    result.Add(item);
            //}

            if (ChemicalItemContent.SelectedMedProduct == null || ChemicalItemContent.SelectedMedProduct.GenMedProductID < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0168_G1_HayChonHC, new string[] { "SelectedInwardDrug" });
                result.Add(item);
            }
            if (ChemicalQty.GetValueOrDefault(0) < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0169_G1_NhapGTriSLgHC, new string[] { "ChemicalQty" });
                result.Add(item);
            }
            if (result.Count > 0)
            {
                return false;
            }
            return true;
        }

        public bool ValidateRegistrationInfo(out ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> result)
        {
            result = new ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult>();

            if (PatientSummaryInfoContent.CurrentPatient == null)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0148_G1_HayChon1BN, new string[] { "CurrentPatient" });
                result.Add(item);
            }

            if (CurClassification == null)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0154_G1_HayChonLoaiBN, new string[] { "CurClassification" });
                result.Add(item);
            }
            if (CurRegistration.ExamDate == DateTime.MinValue)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0154_G1_NgDKKhongHopLe, new string[] { "ExamDate" });
                result.Add(item);
            }

            if (EditingBillingInvoice.V_BillingInvType < 0)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0155_G1_HayChonLoaiTToan, new string[] { "BillingType" });
                result.Add(item);
            }

            CurRegistration.PatientClassification = CurClassification;
            if (ConfirmedPaperReferal != null)
            {
                CurRegistration.PaperReferal = ConfirmedPaperReferal;
            }

            if (EditingInvoiceDetailsContent.BillingInvoice == null)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0156_G1_Chon1DV, new string[] { "AllRegistrationItems" });
                result.Add(item);
            }

            if (CurRegistration.V_RegistrationType != AllLookupValues.RegistrationType.NOI_TRU && PatientFindBy == AllLookupValues.PatientFindBy.NOITRU)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0085_G1_DayKhongPhaiDKNoiTru, new string[] { "AllRegistrationItems" });
                result.Add(item);
            }

            CurRegistration.StaffID = Globals.LoggedUserAccount.StaffID;

            if (HiServiceBeingUsed)
            {
                //Dang la benh nhan bao hiem.
                //Kiem tra neu chua confirm the bao hiem thi thong bao loi.
                if (ConfirmedHiItem == null)
                {
                    var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0157_G1_ChuaKTraTheBH, new string[] { "ConfirmedHiItem" });
                    result.Add(item);
                }
                CurRegistration.HealthInsurance = ConfirmedHiItem;

                long? hisID = ConfirmedHiItem == null ? null : ConfirmedHiItem.HisID;
                if (ConfirmedHiItem != null
                    && ConfirmedHiItem.HealthInsuranceHistories != null
                    && ConfirmedHiItem.HealthInsuranceHistories.Count > 0)
                {
                    hisID = ConfirmedHiItem.HealthInsuranceHistories[0].HisID;
                }

                foreach (PatientRegistrationDetail d in CurRegistration.PatientRegistrationDetails)
                {
                    d.HisID = hisID;
                }
                CurRegistration.HisID = hisID;
            }

            CurRegistration.PatientID = PatientSummaryInfoContent.CurrentPatient.PatientID;
            CurRegistration.RegTypeID = (byte)PatientRegistrationType.DK_KHAM_BENH_NOI_TRU;
            CurRegistration.RefDepartment = Globals.ObjRefDepartment == null ? null : Globals.ObjRefDepartment;

            if (result.Count > 0)
            {
                return false;
            }
            return true;
        }

        public bool CanSaveBillingInvoiceCmd
        {
            get
            {
                //return RegistrationInfoHasChanged;
                return true;
            }
        }

        private bool CheckValidMedicalInstructionDate(DateTime MedicalInstructionDate, PatientRegistration CurRegistration)
        {
            if (CurRegistration == null)
            {
                return true;
            }
            if (CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.AdmissionDate.HasValue && CurRegistration.AdmissionInfo.AdmissionDate != null
                && MedicalInstructionDate < CurRegistration.AdmissionInfo.AdmissionDate)
            {
                return false;
            }
            else if (CurRegistration.AdmissionDate.HasValue && CurRegistration.AdmissionDate != null && MedicalInstructionDate < CurRegistration.AdmissionDate)
            {
                return false;
            }
            else if ((CurRegistration.AdmissionInfo == null || !CurRegistration.AdmissionInfo.AdmissionDate.HasValue || CurRegistration.AdmissionInfo.AdmissionDate == null)
                        && CurRegistration.ExamDate != null
                        && MedicalInstructionDate < CurRegistration.ExamDate)
            {
                return false;
            }
            return true;
        }

        public bool CheckBeforeSaveBillingInvoice()
        {
            string error = "";
            string message = "";
            string error1 = "";
            string error2 = "";

            if (RequireDoctorAndDate)
            {
                //▼====: #020
                if (EditingBillingInvoice != null && EditingBillingInvoice.RegistrationDetails != null)
                {
                    foreach (var item in EditingBillingInvoice.RegistrationDetails.Where(x => x.RecordState != RecordState.DELETED && x.RecordState != RecordState.UNCHANGED && x.MedicalInstructionDate.HasValue && x.MedicalInstructionDate != null && x.ReqFromDeptID != Globals.ServerConfigSection.Hospitals.KhoaPhongKham))
                    {
                        if (!CheckValidMedicalInstructionDate(item.MedicalInstructionDate.Value, CurRegistration))
                        {
                            error2 += "     " + item.ChargeableItemName
                                + " - Ngày y lệnh: " + item.MedicalInstructionDate.Value.ToString("dd/MM/yyyy hh:mm:ss tt")
                                + "." + Environment.NewLine;
                        }
                    }
                }
                if (EditingBillingInvoice != null && EditingBillingInvoice.PclRequests != null)
                {
                    foreach (var item in EditingBillingInvoice.PclRequests.Where(x => x.RecordState != RecordState.DELETED && x.RecordState != RecordState.UNCHANGED && x.ReqFromDeptID != Globals.ServerConfigSection.Hospitals.KhoaPhongKham).Select(x => x.PatientPCLRequestIndicators))
                    {
                        foreach (var childitem in item.Where(x => x.RecordState != RecordState.DELETED && x.RecordState != RecordState.UNCHANGED && x.MedicalInstructionDate.HasValue && x.MedicalInstructionDate != null))
                        {
                            if (!CheckValidMedicalInstructionDate(childitem.MedicalInstructionDate.Value, CurRegistration))
                            {
                                error2 += "     " + childitem.ChargeableItemName + "." + Environment.NewLine;
                            }
                        }
                    }
                }
                //▲====: #020
                var mResults = new ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult>();
                if (RequireDoctorAndDate && EditingBillingInvoice != null && ((EditingBillingInvoice.RegistrationDetails != null && EditingBillingInvoice.RegistrationDetails.Any(x => x.RecordState != RecordState.DELETED && x.RecordState != RecordState.UNCHANGED && (x.HIAllowedPrice > 0 || (x.ChargeableItem != null && x.ChargeableItem.HIAllowedPrice > 0)) && !CheckValidMedicalInstructionDate(x.MedicalInstructionDate, false, ref mResults, false)))
                    || (EditingBillingInvoice.PclRequests != null && EditingBillingInvoice.PclRequests.Any(x => x.RecordState != RecordState.DELETED && x.RecordState != RecordState.UNCHANGED && x.PatientPCLRequestIndicators.Any(y => y.RecordState != RecordState.DELETED && y.RecordState != RecordState.UNCHANGED && (y.HIAllowedPrice > 0 || (y.ChargeableItem != null && y.ChargeableItem.HIAllowedPrice > 0)) && !CheckValidMedicalInstructionDate(y.MedicalInstructionDate, false, ref mResults, false))))))
                {
                    return false;
                }
            }

            if (EditingBillingInvoice.RegistrationDetails != null && EditingBillingInvoice.RegistrationDetails.Count > 0)
            {
                foreach (var item in EditingBillingInvoice.RegistrationDetails)
                {
                    if (item.V_NewPriceType > 0 && item.V_NewPriceType != (Int32)AllLookupValues.V_NewPriceType.Fixed_PriceType && item.InvoicePrice <= 0)
                    {
                        error += "\n - " + item.ChargeableItemName;
                    }

                    if (item.MedicalInstructionDate != null && item.ResultDate != null && item.RecordState != RecordState.DELETED && item.MedicalInstructionDate.GetValueOrDefault() > item.ResultDate.GetValueOrDefault())
                    {
                        message += "     " + item.ChargeableItemName + "." + Environment.NewLine;
                    }
                    if (Globals.ServerConfigSection.InRegisElements.CheckMedicalInstructDate && Globals.ServerConfigSection.InRegisElements.NumOfOverDaysForMedicalInstructDate >= 0)
                    {
                        int NumOfDays = (item.MedicalInstructionDate.GetValueOrDefault().Date - Globals.GetCurServerDateTime().Date).Days;
                        if (NumOfDays > Globals.ServerConfigSection.InRegisElements.NumOfOverDaysInDischargeForm)
                        {
                            error1 += "     " + item.ChargeableItemName + "." + Environment.NewLine;
                        }
                    }

                }
            }
            if (EditingBillingInvoice.PclRequests != null && EditingBillingInvoice.PclRequests.Count > 0)
            {
                foreach (PatientPCLRequest item in EditingBillingInvoice.PclRequests)
                {
                    if (item.PatientPCLRequestIndicators == null || item.PatientPCLRequestIndicators.Count <= 0)
                    {
                        continue;
                    }
                    foreach (PatientPCLRequestDetail PCLDetails in item.PatientPCLRequestIndicators)
                    {
                        if (PCLDetails.PCLExamType.V_NewPriceType > 0 && PCLDetails.V_NewPriceType != (Int32)AllLookupValues.V_NewPriceType.Fixed_PriceType && PCLDetails.InvoicePrice <= 0)
                        {
                            error += "\n - " + PCLDetails.ChargeableItemName;
                        }

                        if (PCLDetails.MedicalInstructionDate != null && PCLDetails.ResultDate != null)
                        {
                            if (PCLDetails.RecordState != RecordState.DELETED && PCLDetails.MedicalInstructionDate.GetValueOrDefault() > PCLDetails.ResultDate.GetValueOrDefault())
                            {
                                message += "     " + PCLDetails.ChargeableItemName + "." + Environment.NewLine;
                            }
                            if (Globals.ServerConfigSection.InRegisElements.CheckMedicalInstructDate && Globals.ServerConfigSection.InRegisElements.NumOfOverDaysForMedicalInstructDate >= 0)
                            {
                                int NumOfDays = (PCLDetails.MedicalInstructionDate.GetValueOrDefault().Date - Globals.GetCurServerDateTime().Date).Days;
                                if (NumOfDays > Globals.ServerConfigSection.InRegisElements.NumOfOverDaysInDischargeForm)
                                {
                                    error1 += "     " + PCLDetails.ChargeableItemName + "." + Environment.NewLine;
                                }
                            }
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(error2))
            {
                MessageBox.Show(string.Format("{0} \n{1}", eHCMSResources.Z3020_G1_MsgNgYLenhKhHopLe, error2), eHCMSResources.T0074_G1_I, MessageBoxButton.OK);
                return false;
            }
            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(string.Format("{0} {1}. \n{2}", eHCMSResources.A0074_G1_Msg_InfoNhapGiaChoCacDV, error, eHCMSResources.Z0448_G1_LuuYGiaLonHon0));
                return false;
            }

            if (!string.IsNullOrEmpty(message))
            {
                MessageBox.Show(string.Format("{0} \n{1}", eHCMSResources.A0895_G1_Msg_InfoNgYLenhKhHopLe, message), eHCMSResources.T0074_G1_I, MessageBoxButton.OK);
                return false;
            }
            if (!string.IsNullOrEmpty(error1))
            {
                string msg = Globals.ServerConfigSection.InRegisElements.NumOfOverDaysForMedicalInstructDate == 0
                        ? string.Format(eHCMSResources.Z1875_G1_NgYLenhKgLonHonNgHTai2, error1)
                        : string.Format(eHCMSResources.Z1874_G1_NgYLenhKgLonHonNgHTai, Globals.ServerConfigSection.InRegisElements.NumOfOverDaysForMedicalInstructDate, error1);
                MessageBox.Show(msg, eHCMSResources.K1576_G1_CBao, MessageBoxButton.OK);
                return false;
            }
            if (RequireDoctorAndDate && EditingBillingInvoice != null
                && ((EditingBillingInvoice.RegistrationDetails != null && EditingBillingInvoice.RegistrationDetails.Any(x => CheckDoctorAndDate(x)))
                || (EditingBillingInvoice.PclRequests != null && EditingBillingInvoice.PclRequests.Any(x => CheckDoctorAndDate(x)))))
            {
                MessageBox.Show(eHCMSResources.Z2184_G1_NhapDayDuNgayYLVaBS, eHCMSResources.K1576_G1_CBao, MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        public void SaveBillingInvoiceCmd()
        {
            if (Globals.IsLockRegistration(CurRegistration.RegLockFlag, eHCMSResources.Z0449_G1_LuuBill))
            {
                return;
            }
            if (isDischarged)
            {
                MessageBox.Show(eHCMSResources.A0240_G1_Msg_InfoBNDaXV_KTheLuuBill, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                return;
            }
            if (Globals.isAccountCheck && CurRegistration != null && CurRegistration.AdmissionInfo != null)
            {
                int NumOfDayAllowSaveBillAfterDischarge = Globals.ServerConfigSection.InRegisElements.NumOfDayAllowSaveBillAfterDischarge;
                DateTime? DischargeDetailRecCreatedDate = CurRegistration.AdmissionInfo.DischargeDetailRecCreatedDate;
                if (NumOfDayAllowSaveBillAfterDischarge > 0 && DischargeDetailRecCreatedDate != null
                    && (Globals.GetCurServerDateTime().Date - DischargeDetailRecCreatedDate.GetValueOrDefault().Date).TotalDays > NumOfDayAllowSaveBillAfterDischarge)
                {
                    MessageBox.Show(string.Format("{0} {1} {2} \n{3} {4}"
                        , eHCMSResources.A0845_G1_Msg_InfoCBaoNgLuuBill, NumOfDayAllowSaveBillAfterDischarge.ToString(), eHCMSResources.N0045_G1_Ng.ToLower()
                        , eHCMSResources.A0846_G1_Msg_NgXV, DischargeDetailRecCreatedDate.GetValueOrDefault().ToString("dd/MM/yyyy hh:mm:ss tt")), eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                    return;
                }
            }

            if (!CheckDeptID())
            {
                return;
            }
            SaveBillingInvoice();
        }

        //▼===== #012
        private bool CheckMedInstructionDateDVKTAndVTYTHighTechBill()
        {
            if ((EditingBillingInvoice.RegistrationDetails == null || EditingBillingInvoice.RegistrationDetails.Count == 0)
                || (EditingBillingInvoice.OutwardDrugClinicDeptInvoices == null || EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Count == 0))
            {
                return true;
            }
            var RegistrationDetails = EditingBillingInvoice.RegistrationDetails.Where(x => x.RefMedicalServiceItem.HITTypeID == (long)AllLookupValues.HITTypeID.DVKT);
            var OutwardDrugClinicDeptBillingInvoice = EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Where(x => x.MedProductType == AllLookupValues.MedProductType.Y_CU);
            if ((RegistrationDetails == null || RegistrationDetails.Count() == 0)
                || (OutwardDrugClinicDeptBillingInvoice == null || OutwardDrugClinicDeptBillingInvoice.Count() == 0))
            {
                return true;
            }
            foreach (var tempRegistrationDetails in RegistrationDetails)
            {
                foreach (var tempOutwardDrugClinicDeptBillingInvoice in OutwardDrugClinicDeptBillingInvoice)
                {
                    if (tempOutwardDrugClinicDeptBillingInvoice.OutwardDrugClinicDepts == null || tempOutwardDrugClinicDeptBillingInvoice.OutwardDrugClinicDepts.Count == 0)
                    {
                        continue;
                    }
                    foreach (var items in tempOutwardDrugClinicDeptBillingInvoice.OutwardDrugClinicDepts)
                    {
                        if (DateTime.Compare(tempRegistrationDetails.MedicalInstructionDate.Value.Date, items.MedicalInstructionDate.Value.Date) != 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        //▲===== #012
        //▼===== #021
        private void CheckDVKTGayTeAndDrugGayTe()
        {
            if (EditingBillingInvoice.OutwardDrugClinicDeptInvoices == null)
            {
                return;
            }
            long RefGenDrugCatID_2ForDrug = Globals.ServerConfigSection.InRegisElements.RefGenDrugCatID_2ForDrug;
            var OutwardDrugClinicDeptBillingInvoice = EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Where(x => x.MedProductType == AllLookupValues.MedProductType.THUOC);
            if (OutwardDrugClinicDeptBillingInvoice.Count() == 0)
            {
                return;
            }
            bool HasDrugAnalgesic = false;
            foreach (var tempOutwardDrugClinicDeptBillingInvoice in OutwardDrugClinicDeptBillingInvoice)
            {
                if (tempOutwardDrugClinicDeptBillingInvoice.OutwardDrugClinicDepts == null || tempOutwardDrugClinicDeptBillingInvoice.OutwardDrugClinicDepts.Count == 0)
                {
                    continue;
                }
                foreach (var items in tempOutwardDrugClinicDeptBillingInvoice.OutwardDrugClinicDepts)
                {
                    if (items.GenMedProductItem.RefGenDrugCatID_2 == RefGenDrugCatID_2ForDrug && items.IsCountHI)
                    {
                        HasDrugAnalgesic = true;
                    }
                }
            }
            if (HasDrugAnalgesic)
            {
                if (EditingBillingInvoice.RegistrationDetails == null || EditingBillingInvoice.RegistrationDetails.Count == 0)
                {
                    throw new Exception("Chưa có DVKT gây tê trong bill có thuốc gây tê");
                }
                var RegistrationDetails = EditingBillingInvoice.RegistrationDetails.Where(x => x.IsCountHI && x.RefMedicalServiceItem.UseAnalgesic);
                if (RegistrationDetails == null || RegistrationDetails.Count() == 0)
                {
                    throw new Exception("Chưa có DVKT gây tê thanh BHYT trong bill có thuốc gây tê");
                }
            }
            return;
        }
        //▲===== #021

        private void SaveBillingInvoice()
        {
            if (!CanAddEditBill)
            {
                MessageBox.Show(eHCMSResources.A0701_G1_Msg_InfoLuuFail);
                return;
            }
            if (!CheckMedInstructionDateDVKTAndVTYTHighTechBill() && IsHighTechServiceBill)
            {
                MessageBox.Show(eHCMSResources.Z2801_G1_VTYTKhacNgayYLenhDVKT);
                return;
            }
            try
            {
                CheckDVKTGayTeAndDrugGayTe();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                return;
            }
            //▲===== #021
            if (!CheckBeforeSaveBillingInvoice())
            {
                return;
            }

            //if (!CheckTotalBillInvoice())
            //{
            //    return;
            //}

            CurRegistration.ReqFromDeptLocID = Globals.DeptLocation.DeptLocationID;

            if (DepartmentContent != null && DepartmentContent.SelectedItem != null
                && DepartmentContent.SelectedItem.SelectDeptReqSelectRoom)
            {
                if (SelLocationInDept != null && SelLocationInDept.DeptLocationID > 0)
                {
                    CurRegistration.ReqFromDeptLocID = SelLocationInDept.DeptLocationID;
                }
            }

            //if (EditingBillingInvoice.InPatientBillingInvID <= 0)
            //{
            //    AddBillingInvoice();
            //}
            //else
            //{
            UpdateBillingInvoice();
            //}
            //}
        }

        public void ShowTotalBillInvoice(decimal totalPatientPayment, decimal totalHIPayment, decimal totalRealHIPayment
            , decimal totalBillInvoice, bool showErrorMessage, decimal totalCharitySupportFund = 0)
        {
            Action<ITotalBillInvoice> onInitDlg = delegate (ITotalBillInvoice vm)
            {
                vm.TotalPatientPayment = totalPatientPayment - totalCharitySupportFund;
                vm.TotalHIPayment = totalHIPayment;
                vm.TotalRealHIPayment = totalRealHIPayment;
                vm.TotalBillInvoice = totalBillInvoice;
                vm.ShowErrorMessage = showErrorMessage;
                vm.TotalCharitySupportFund = totalCharitySupportFund;
            };
            GlobalsNAV.ShowDialog<ITotalBillInvoice>(onInitDlg);
        }

        private void UpdateBillingInvoice()
        {
            DateTime AdmissionDate = CurRegistration.ExamDate;
            if (CurRegistration.AdmissionInfo != null)
            {
                AdmissionDate = CurRegistration.AdmissionInfo.AdmissionDate.GetValueOrDefault();
            }

            DateTime Now = Globals.GetCurServerDateTime();
            // bỏ qua kiểm tra thời gian tại chỗ này

            var modifiedRegDetails = new List<PatientRegistrationDetail>();
            var deletedRegDetails = new List<PatientRegistrationDetail>();
            var deletedPclRequestDetails = new List<PatientPCLRequestDetail>();
            var modifiedPclRequestDetails = new List<PatientPCLRequestDetail>();
            var modifiedOutwardDrugClinicDepts = new List<OutwardDrugClinicDept>();
            var deleteOutwardDrugClinicDepts = new List<OutwardDrugClinicDept>();

            foreach (var temp in AllRegistrationItems)
            {
                if (temp is PatientRegistrationDetail)
                {
                    var item = temp as PatientRegistrationDetail;
                    switch (item.RecordState)
                    {
                        case RecordState.DELETED:
                            deletedRegDetails.Add(item);
                            break;
                        case RecordState.MODIFIED:
                            modifiedRegDetails.Add(item);
                            break;
                    }
                }

                if (temp is PatientPCLRequestDetail)
                {
                    var child = temp as PatientPCLRequestDetail;
                    switch (child.RecordState)
                    {
                        case RecordState.DELETED:
                            deletedPclRequestDetails.Add(child);
                            break;
                        case RecordState.MODIFIED:
                            modifiedPclRequestDetails.Add(child);
                            break;
                    }
                }

                if (temp is OutwardDrugClinicDept)
                {
                    var child = temp as OutwardDrugClinicDept;
                    switch (child.RecordState)
                    {
                        case RecordState.DELETED:
                            deleteOutwardDrugClinicDepts.Add(child);
                            break;
                        case RecordState.MODIFIED:
                            modifiedOutwardDrugClinicDepts.Add(child);
                            break;
                    }
                }
            }


            this.ShowBusyIndicator(eHCMSResources.Z0172_G1_DangLuuDLieu);
            var t = new Thread(() =>
            {
                AxErrorEventArgs error = null;
                try
                {
                    using (var serviceFactory = new BillingPaymentWcfServiceLibClient())
                    {
                        var contract = serviceFactory.ServiceInstance;

                        contract.BeginUpdateInPatientBillingInvoiceByPtRegistrationID(Globals.LoggedUserAccount.StaffID
                            , CurRegistration.PtRegistrationID
                            , deletedRegDetails, deletedPclRequestDetails, deleteOutwardDrugClinicDepts
                            , modifiedRegDetails, modifiedPclRequestDetails, modifiedOutwardDrugClinicDepts
                            , IsNotCheckInvalid
                            , Globals.DispatchCallback((asyncResult) =>
                            {
                                bool bOK = false;
                                Dictionary<long, List<long>> drugIDList_Error = null;
                                try
                                {
                                    contract.EndUpdateInPatientBillingInvoiceByPtRegistrationID(out drugIDList_Error, asyncResult);
                                    if (drugIDList_Error == null || drugIDList_Error.Count == 0)
                                    {
                                        bOK = true;
                                        RegistrationInfoHasChanged = false;
                                        IsNotCheckInvalid = false;
                                    }
                                    else
                                    {
                                        bOK = false;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (ex.Message.Contains("19090601") && MessageBox.Show(ex.Message, eHCMSResources.G0442_G1_TBao, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                                    {
                                        //20191025 TBL: Nếu đồng ý lưu thì lưu lại và bỏ qua kiểm tra dưới store
                                        IsNotCheckInvalid = true;
                                        UpdateBillingInvoice();
                                    }
                                    else if (!ex.Message.Contains("19090601"))
                                    {
                                        Globals.ShowMessage(ex.Message, eHCMSResources.G0442_G1_TBao);
                                    }
                                }
                                finally
                                {
                                    if (drugIDList_Error != null && drugIDList_Error.Count > 0)
                                    {
                                        void onInitDlg(IMedProductRemaingListing vm)
                                        {
                                            vm.StartLoadingByIdList(drugIDList_Error);
                                            vm.Title = eHCMSResources.K2948_G1_DSLgoaiThuocKhongTheLuu;
                                        }
                                        GlobalsNAV.ShowDialog<IMedProductRemaingListing>(onInitDlg);
                                    }
                                    //if (bOK && ((regID > 0 && PatientFindBy == AllLookupValues.PatientFindBy.NOITRU) || PatientFindBy == AllLookupValues.PatientFindBy.NGOAITRU))
                                    if (bOK)
                                    {
                                        GetAllInPatientBillingInvoices();
                                        RegistrationInfoHasChanged = false;
                                        GlobalsNAV.ShowMessagePopup(eHCMSResources.A0279_G1_Msg_InfoCNhatOK);
                                    }
                                    this.HideBusyIndicator();
                                }
                            }), null);
                    }
                }
                catch (Exception ex)
                {
                    error = new AxErrorEventArgs(ex);
                    MessageBox.Show(ex.Message, eHCMSResources.T0074_G1_I, MessageBoxButton.OK);
                }
                finally
                {
                    this.HideBusyIndicator();
                    if (error != null)
                    {
                        Globals.EventAggregator.Publish(new ErrorOccurred() { CurrentError = error });
                    }
                }
            });

            t.Start();
        }

        public void Handle(RemoveItem<MedRegItemBase> message)
        {
            if (GetView() == null || !CanRegister || message == null || message.Item == null)
            {
                return;
            }
            if (message.Item is PatientRegistrationDetail)
            {
                var details = message.Item as PatientRegistrationDetail;
                if (details.RecordState == RecordState.ADDED || details.RecordState == RecordState.DETACHED)
                {
                    CurRegistration.PatientRegistrationDetails.Remove(details);
                }
                else
                {
                    details.RecordState = RecordState.DELETED;
                    details.MarkedAsDeleted = true;
                }
            }
            else if (message.Item is PatientPCLRequestDetail)
            {
                foreach (var request in CurRegistration.PCLRequests)
                {
                    if (request.PatientPCLRequestIndicators != null
                        && request.PatientPCLRequestIndicators.Remove(message.Item as PatientPCLRequestDetail))
                    {
                        break;
                    }
                }
            }
            else if (message.Item is OutwardDrugClinicDept)
            {
                foreach (var invoice in CurRegistration.InPatientInvoices)
                {
                    if (invoice.OutwardDrugClinicDepts != null
                        && invoice.OutwardDrugClinicDepts.Remove(message.Item as OutwardDrugClinicDept))
                    {
                        break;
                    }
                }
            }
            else
            {
                message.Item.RecordState = RecordState.DELETED;
            }
            OnDetailsChanged();
        }

        private void OnDetailsChanged()
        {
            RegistrationInfoHasChanged = true;
            if (_currentRegMode == RegistrationFormMode.OLD_REGISTRATION_OPENED)
            {
                CurrentRegMode = RegistrationFormMode.OLD_REGISTRATION_CHANGED;
            }
            else if (_currentRegMode == RegistrationFormMode.NEW_REGISTRATION_OPENED)
            {
                CurrentRegMode = RegistrationFormMode.NEW_REGISTRATION_CHANGED;
            }
        }

        public bool CanCancelChangesCmd
        {
            get
            {
                return true;
            }
        }

        public void CancelChangesCmd()
        {
            AllRegistrationItems = AllRegistrationItemsBackUp.DeepCopy();
            AllRegistrationItemsFilter = AllRegistrationItems.DeepCopy();
        }

        #region COROUTINES
        public IEnumerator<IResult> DoCalcHiBenefit(HealthInsurance hiItem, PaperReferal referal)
        {
            bool isEmergency = CurRegistration.EmergRecID > 0 ? true : false;
            var calcHiTask = new CalcHiBenefitTask(hiItem, referal, (long)AllLookupValues.RegistrationType.NOI_TRU, isEmergency);
            yield return calcHiTask;

            // TxD 02/03/2018 HiBenefit has been removed from interface of PatientSummaryInfoContent 
            //if (calcHiTask.Error == null)
            //{
            //    PatientSummaryInfoContent.HiBenefit = calcHiTask.HiBenefit;
            //}
        }
        #endregion

        //private IEnumerator<IResult> GetDateTimeFromServer()
        //{
        //    var loadCurrentDate = new LoadCurrentDateTask();
        //    yield return loadCurrentDate;

        //    if (loadCurrentDate.CurrentDate == DateTime.MinValue)
        //    {
        //        SelectedDate = DateTime.Now.Date;
        //        MessageBoxTask _msgTask = new MessageBoxTask("Không lấy được ngày tháng từ server.", eHCMSResources.G0442_G1_TBao);
        //        yield return _msgTask;
        //    }
        //    else
        //    {
        //        SelectedDate = loadCurrentDate.CurrentDate.Date;
        //    }
        //    yield break;
        //}

        private IEnumerator<IResult> DoGetStore_EXTERNAL(long? DeptID)
        {
            var paymentTypeTask = new LoadStoreListTask((long)AllLookupValues.StoreType.STORAGE_CLINIC, false, DeptID, false, true);
            yield return paymentTypeTask;
            WarehouseList = paymentTypeTask.LookupList;
            if (WarehouseList != null)
            {
                SelectedWarehouse = WarehouseList.FirstOrDefault();
            }
            yield break;
        }

        public void ShowBillingReport(long inPatientBillingInvId)
        {
            void onInitDlg(IBillingInvoiceDetailsReport reportVm)
            {
                reportVm.InPatientBillingInvID = inPatientBillingInvId;
            }
            GlobalsNAV.ShowDialog<IBillingInvoiceDetailsReport>(onInitDlg, null, false, true);
        }

        public bool CanPrintOldBillingInvoiceCmd
        {
            get
            {
                return true;
            }
        }

        //public void Handle(LoadBillingDetailsCompleted message)
        //{
        //    if (EditingSupportFund == null || EditingInvoiceDetailsContent == null)
        //    {
        //        return;
        //    }
        //    EditingSupportFund.EditingInvoiceDetailsContent = EditingInvoiceDetailsContent;
        //    EditingSupportFund.GetCharitySupportFunds();
        //}

        public void Handle(EditItem<InPatientBillingInvoice> message)
        {
            object curView = GetView();
            if (curView != null)
            {
                // Không cho người dùng chỉnh sửa các bill thuộc đăng ký của bệnh nhân trong các trường hợp sau:
                // 1. Đăng ký đã xuất viện
                // 2. Đăng ký Vãng Lai hoặc Tiền Giải Phẫu chưa nhập viện nhưng đã quá hạn???
                // 3. Đăng ký đã bị đóng (khi có một đăng ký mới được tạo ra theo xác nhận của người dùng)
                //if (CurRegistration != null && CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.DischargeDate != null)
                //{
                //    MessageBox.Show(eHCMSResources.A0685_G1_Msg_InfoKhTheSuaBillBNDaXV, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                //    return;
                //}

                if (!Validate_RegistrationInfo(CurRegistration))
                {
                    return;
                }
                EditingBillingInvoice = message.Item;
                if (curView is IInPatientRegistrationView)
                {
                    ((IInPatientRegistrationView)curView).SetActiveTab(InPatientRegistrationViewTab.EDITING_BILLING_INVOICE);
                }
                EditingInvoiceDetailsContent.LoadDetails(EditBillingInvoice);
                //KMx: Không được để ở đây, vì trong EditingInvoiceDetailsContent.LoadDetails có chạy về server. Phải đợi khi về server xong thì mới gọi BackUpBill() (12/01/2016 17:46)
                //BackUpBill();
            }
        }

        public void AddBillingInvoiceCompletedCallback(InPatientBillingInvoice inv)
        {
            if (CurRegistration != null && CurRegistration.PtRegistrationID > 0
                && (inv.PtRegistrationID == CurRegistration.PtRegistrationID || inv.OutPtRegistrationID == CurRegistration.PtRegistrationID))
            {
                bool bExists = false;
                if (CurRegistration.InPatientBillingInvoices != null)
                {
                    foreach (var temp in CurRegistration.InPatientBillingInvoices)
                    {
                        if (inv == temp)
                        {
                            bExists = true;
                            break;
                        }
                    }
                    if (!bExists)
                    {
                        CurRegistration.InPatientBillingInvoices.Add(inv);

                        if (OldBillingInvoiceContent.BillingInvoices != null)
                        {
                            OldBillingInvoiceContent.BillingInvoices.Add(inv);
                        }
                    }
                }
                if (EditingBillingInvoice != null && EditingBillingInvoice.InPatientBillingInvID == inv.InPatientBillingInvID)
                {
                    EditingBillingInvoice = inv;
                }
                BackUpBill();
            }
        }

        //KMx: Chỉnh sửa ngày 20/10/2014 11:28.
        //Lý do: Sau khi cập nhật bill, không load lại, vì index của CurRegistration.InPatientBillingInvoices và OldBillingInvoiceContent.BillingInvoices khác nhau.
        public void UpdateBillingInvoiceCompletedCallback(InPatientBillingInvoice inv)
        {
            if (CurRegistration == null || CurRegistration.PtRegistrationID <= 0 || CurRegistration.InPatientBillingInvoices == null
                || inv == null || (inv.PtRegistrationID != CurRegistration.PtRegistrationID && PatientFindBy == AllLookupValues.PatientFindBy.NOITRU))
            {
                return;
            }

            for (int i = 0; i < CurRegistration.InPatientBillingInvoices.Count; i++)
            {
                if (inv.InPatientBillingInvID == CurRegistration.InPatientBillingInvoices[i].InPatientBillingInvID)
                {
                    CurRegistration.InPatientBillingInvoices[i] = inv;
                    break;
                }
            }

            for (int j = 0; j < OldBillingInvoiceContent.BillingInvoices.Count; j++)
            {
                if (inv.InPatientBillingInvID == OldBillingInvoiceContent.BillingInvoices[j].InPatientBillingInvID)
                {
                    OldBillingInvoiceContent.BillingInvoices[j] = inv;
                    break;
                }
            }

            if (EditingBillingInvoice != null && EditingBillingInvoice.InPatientBillingInvID == inv.InPatientBillingInvID)
            {
                EditingBillingInvoice = inv;
            }
            BackUpBill();
        }

        public void EditBillingInvoice(InPatientBillingInvoice inv)
        {
            if (inv != null)
            {
                EditingBillingInvoice = inv;

                //KMx: Khi chọn 1 bill để sửa thì phải set RegistrationInfoHasChanged = true để nút "Lưu" hiện lên (08/01/2015 09:28).
                //Trước đây khi chọn 1 bill để sửa thì nút "Lưu" vẫn ẩn, khi nào thêm, xóa, sửa DV, CLS thì nút "Lưu" mới hiện lên. Dẫn đến không thể cập nhật từ ngày, đến ngày của bill được.
                RegistrationInfoHasChanged = true;

                //KMx: Khi chọn 1 bill để sửa thì phải set CanEditPeriodOfTime = true để enable "Từ ngày", "Đến ngày" tạo bill (08/01/2015 09:28).
                EditingBillingInvoice.CanEditPeriodOfTime = true;

                // TxD 12/05/2015: Set Room location to the DeptLocationID of the Billing Invoice if DeptLocationID > 0 ( this field was not saved previously ie. old bills)
                if (inv.DeptLocationID.HasValue && inv.DeptLocationID > 0)
                {
                    foreach (var locItem in LocationsInDept)
                    {
                        if (locItem.DeptLocationID == inv.DeptLocationID)
                        {
                            SelLocationInDept = locItem;
                        }
                    }
                }
            }
            BackUpBill();
        }

        public bool CanCreateNewBillCmd
        {
            get
            {
                //KMx: Luôn luôn hiện nút "Tạo mới" (09/01/2016 16:21).
                //return !RegistrationInfoHasChanged;
                return true;
            }
        }

        public void CreateNewBillCmd()
        {
            EditingBillingInvoice = new InPatientBillingInvoice();
            MedicalInstructionDateContent.DateTime = new DateTime(Globals.GetCurServerDateTime().Year, Globals.GetCurServerDateTime().Month, Globals.GetCurServerDateTime().Day, Globals.GetCurServerDateTime().Hour, Globals.GetCurServerDateTime().Minute, 0);
            ResultDateContent.DateTime = new DateTime(Globals.GetCurServerDateTime().Year, Globals.GetCurServerDateTime().Month, Globals.GetCurServerDateTime().Day, Globals.GetCurServerDateTime().Hour, Globals.GetCurServerDateTime().Minute, 0);
            UseAtDateContent.DateTime = new DateTime(Globals.GetCurServerDateTime().Year, Globals.GetCurServerDateTime().Month, Globals.GetCurServerDateTime().Day, Globals.GetCurServerDateTime().Hour, Globals.GetCurServerDateTime().Minute, 0);
            EditingBillingInvoice.DeptID = DeptID;
            if (EditingBillingInvoice.V_BillingInvType == 0)
            {
                EditingBillingInvoice.V_BillingInvType = AllLookupValues.V_BillingInvType.TINH_TIEN_NOI_TRU;
            }

            //BeginEdit();
            //KMx: Cắt 1 phần code trong BeginEdit() ra (13/01/2016 09:41).
            EditingBillingInvoice.InvDate = Globals.GetCurServerDateTime();
            EditingBillingInvoice.BillFromDate = Globals.GetCurServerDateTime();
            EditingBillingInvoice.BillToDate = Globals.GetCurServerDateTime();
            EditingBillingInvoice.V_InPatientBillingInvStatus = AllLookupValues.V_InPatientBillingInvStatus.NEW;
            /*==== #004 ====*/
            if (CurRegistration != null)
            {
                EditingBillingInvoice.IsHICard_FiveYearsCont_NoPaid = CurRegistration.IsHICard_FiveYearsCont_NoPaid;
                if (CurRegistration.HealthInsurance != null && !string.IsNullOrEmpty(CurRegistration.HealthInsurance.HICardNo) && CurRegistration.HealthInsurance.HICardNo.Length >= 3 && Globals.ServerConfigSection.HealthInsurances.NotPermittedHICard.Split(',').Contains(CurRegistration.HealthInsurance.HICardNo.Substring(0, 3)))
                    EditingInvoiceDetailsContent.IsNotPermitted = true;
                // Cập nhật lại tích BN COVID cho thông tin nội trú dựa trên thông tin nhập khoa hiện tại
                if (CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.InPatientDeptDetails != null
                    && CurRegistration.AdmissionInfo.InPatientDeptDetails.Count > 0)
                {
                    InPatientDeptDetail CurrentInPatientDeptDetail = CurRegistration.AdmissionInfo.InPatientDeptDetails.Where(x => x.V_InPatientDeptStatus == AllLookupValues.InPatientDeptStatus.NHAP_KHOA_PHONG).FirstOrDefault();
                    if (CurrentInPatientDeptDetail != null && DeptID == CurrentInPatientDeptDetail.DeptLocation.RefDepartment.DeptID)
                    {
                        CurRegistration.AdmissionInfo.IsTreatmentCOVID = CurrentInPatientDeptDetail.IsTreatmentCOVID;
                        if (CurrentInPatientDeptDetail.IsTreatmentCOVID)
                        {
                            EditingInvoiceDetailsContent.IsShowPatientCOVID = Visibility.Visible;
                        }
                        else
                        {
                            EditingInvoiceDetailsContent.IsShowPatientCOVID = Visibility.Collapsed;
                        }
                    }
                    // 20211220 TNHX: thêm điều kiện kiểm tra khoa khác khoa hiện tại mà cấu hình là khoa covid thì vẫn cho cập nhật lại bill của khoa đó
                    else if (DepartmentContent != null && DepartmentContent.SelectedItem != null && DepartmentContent.SelectedItem.IsTreatmentForCOVID)
                    {
                        CurRegistration.AdmissionInfo.IsTreatmentCOVID = CurrentInPatientDeptDetail.IsTreatmentCOVID;
                        if (CurrentInPatientDeptDetail.IsTreatmentCOVID)
                        {
                            EditingInvoiceDetailsContent.IsShowPatientCOVID = Visibility.Visible;
                        }
                        else
                        {
                            EditingInvoiceDetailsContent.IsShowPatientCOVID = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        CurRegistration.AdmissionInfo.IsTreatmentCOVID = false;
                        EditingInvoiceDetailsContent.IsShowPatientCOVID = Visibility.Collapsed;
                    }
                }
                else
                {
                    EditingInvoiceDetailsContent.IsShowPatientCOVID = Visibility.Collapsed;
                }
            }
            /*==== #004 ====*/
            if (LocationsInDept != null && LocationsInDept.Count() > 0)
            {
                foreach (var locItem in LocationsInDept)
                {
                    if (locItem.DeptLocationID == 0)
                    {
                        SelLocationInDept = locItem;
                    }
                }
            }
            BackUpBill();
        }

        private InPatientBillingInvoice _backUpBillingInvoice;
        public InPatientBillingInvoice BackUpBillingInvoice
        {
            get
            {
                return _backUpBillingInvoice;
            }
            set
            {
                if (_backUpBillingInvoice != value)
                {
                    _backUpBillingInvoice = value;
                    NotifyOfPropertyChange(() => BackUpBillingInvoice);
                    EditingInvoiceDetailsContent.OldBillingInvoice = BackUpBillingInvoice;
                }
            }
        }

        private void BackUpBill()
        {
            BackUpBillingInvoice = EditingBillingInvoice.DeepCopy();
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get
            {
                return _selectedTabIndex;
            }
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    NotifyOfPropertyChange(() => SelectedTabIndex);
                    NotifyOfPropertyChange(() => CanRegister);
                    NotifyOfPropertyChange(() => CanLoadBillCmd);
                }
            }
        }

        public void tabBillingInvoiceInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (RegistrationInfoHasChanged)
            //{
            //    //Giu lai tab index cu.
            //    var tabCtrl = sender as TabControl;
            //    if (tabCtrl != null && tabCtrl.SelectedIndex != SelectedTabIndex)
            //    {
            //        MessageBoxResult result = MessageBox.Show("Thông tin đã thay đổi. Bạn có muốn bỏ qua?", eHCMSResources.G0442_G1_TBao, MessageBoxButton.OKCancel);
            //        if (result == MessageBoxResult.OK)
            //        {
            //            CancelEdit();
            //            _tempRegistration = CurRegistration.DeepCopy();
            //        }
            //        else
            //        {
            //            tabCtrl.SelectedIndex = SelectedTabIndex;
            //        }
            //    }
            //}
        }

        public void Handle(ItemRemoved<MedRegItemBase, IInPatientBillingInvoiceDetailsListing> message)
        {
            if (GetView() != null)
            {
                if (message.Source == EditingInvoiceDetailsContent)
                {
                    RegistrationInfoHasChanged = true;
                }
            }
        }

        public void Handle(ItemChanged<MedRegItemBase, IInPatientBillingInvoiceDetailsListing> message)
        {
            if (GetView() != null)
            {
                if (message.Source == EditingInvoiceDetailsContent)
                {
                    RegistrationInfoHasChanged = true;
                }
            }
        }

        public void ReturnDrugCmd()
        {
            if (!CheckDeptID())
            {
                return;
            }
            if (GetView() != null)
            {
                void onInitDlg(IInPatientReturnDrug vm)
                {
                    vm.MedProductType = AllLookupValues.MedProductType.THUOC;
                    vm.Registration = CurRegistration;
                    vm.InitData(DeptID);
                }
                GlobalsNAV.ShowDialog<IInPatientReturnDrug>(onInitDlg);
            }
        }

        public bool CanLoadBillCmd
        {
            get
            {
                //return (CurRegistration != null && CurRegistration.PtRegistrationID > 0)
                //    && !RegistrationInfoHasChanged && SelectedTabIndex == (int)InPatientRegistrationViewTab.EDITING_BILLING_INVOICE;
                return (CurRegistration != null && CurRegistration.PtRegistrationID > 0)
                    && SelectedTabIndex == (int)InPatientRegistrationViewTab.EDITING_BILLING_INVOICE;
            }
        }

        /// <summary>
        /// La bill moi (chua insert vo db) cua nhung item da su dung.
        /// </summary>
        private bool _IsBillOfUsedItems = false;
        public bool IsBillOfUsedItems
        {
            get
            {
                return _IsBillOfUsedItems;
            }
            set
            {
                _IsBillOfUsedItems = value;
                NotifyOfPropertyChange(() => IsBillOfUsedItems);

                //KMx: Luôn luôn set EditingInvoiceDetailsContent.CanEditOnGrid = true để được sửa số lượng (25/01/2016 16:06).
                //EditingInvoiceDetailsContent.CanEditOnGrid = _isEditing || IsBillOfUsedItems;
                CanCalcPaymentToEndOfDay = _IsBillOfUsedItems;
            }
        }

        public bool CheckForPreloadingBills()
        {
            this.ShowBusyIndicator(eHCMSResources.Z0151_G1_DangLayTTinBill);

            bool valid = true;
            var t = new Thread(() =>
            {
                AxErrorEventArgs error = null;
                try
                {
                    using (var serviceFactory = new BillingPaymentWcfServiceLibClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        //HPT 13/09/2016: Đưa biến cấu hình load phiếu chỉ định cận lâm sàng xuống để biết load hay không load, load như thế nào
                        //HPT: tạm thời chỗ này lấy y nguyên cấu hình đưa xuống, tuy nhiên anh Tuấn nói sau này có thể kết hợp thêm một số điều kiện trên giao diện nữa nên cứ tạo đường đi trước.
                        contract.BeginCheckForPreloadingBills(CurRegistration.PtRegistrationID,
                            Globals.DispatchCallback((asyncResult) =>
                            {
                                try
                                {
                                    int result = contract.EndCheckForPreloadingBills(out string errMsg, asyncResult);
                                    valid = (2 != result);
                                    if (!String.IsNullOrEmpty(errMsg))
                                    {
                                        string[] errMsgs = errMsg.Split(';');
                                        StringBuilder stringBuilder = new StringBuilder();
                                        foreach (string str in errMsgs)
                                        {
                                            stringBuilder.Append(str).AppendLine();
                                        }
                                        MessageBox.Show(stringBuilder.ToString(), eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    error = new AxErrorEventArgs(ex);
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
                    error = new AxErrorEventArgs(ex);
                    this.HideBusyIndicator();
                }
                if (error != null)
                {
                    Globals.EventAggregator.Publish(new ErrorOccurred() { CurrentError = error });
                }
            });

            t.Start();

            return valid;
        }

        private bool CheckIfInvoiceIsEmpty(InPatientBillingInvoice inv)
        {
            if (inv.RegistrationDetails != null && inv.RegistrationDetails.Count > 0)
            {
                return false;
            }
            if (inv.PclRequests != null && inv.PclRequests.Count > 0)
            {
                if (inv.PclRequests.Any(req => req.PatientPCLRequestIndicators != null && req.PatientPCLRequestIndicators.Count > 0))
                {
                    return false;
                }
            }
            if (inv.OutwardDrugClinicDeptInvoices != null && inv.OutwardDrugClinicDeptInvoices.Count > 0)
            {
                return inv.OutwardDrugClinicDeptInvoices.All(drugInv => drugInv.OutwardDrugClinicDepts == null || drugInv.OutwardDrugClinicDepts.Count <= 0);
            }
            return true;
        }

        #region Các biến phân quyền chức năng

        private bool _mDangKyNoiTru_LoadBill = true;
        private bool _mDangKyNoiTru_BillMoi = true;
        private bool _mDangKyNoiTru_ThemDV = true;
        private bool _mDangKyNoiTru_SuaDV = true;
        private bool _mDangKyNoiTru_XemChiTiet = true;
        private bool _mDangKyNoiTru_In = true;
        private bool _mDangKyNoiTru_TraThuoc = true;


        public bool mDangKyNoiTru_LoadBill
        {
            get
            {
                return _mDangKyNoiTru_LoadBill;
            }
            set
            {
                if (_mDangKyNoiTru_LoadBill == value)
                    return;
                _mDangKyNoiTru_LoadBill = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_LoadBill);
            }
        }

        public bool mDangKyNoiTru_BillMoi
        {
            get
            {
                return _mDangKyNoiTru_BillMoi;
            }
            set
            {
                if (_mDangKyNoiTru_BillMoi == value)
                    return;
                _mDangKyNoiTru_BillMoi = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_BillMoi);
            }
        }

        public bool mDangKyNoiTru_ThemDV
        {
            get
            {
                return _mDangKyNoiTru_ThemDV;
            }
            set
            {
                if (_mDangKyNoiTru_ThemDV == value)
                    return;
                _mDangKyNoiTru_ThemDV = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_ThemDV);
            }
        }

        public bool mDangKyNoiTru_SuaDV
        {
            get
            {
                return _mDangKyNoiTru_SuaDV;
            }
            set
            {
                if (_mDangKyNoiTru_SuaDV == value)
                    return;
                _mDangKyNoiTru_SuaDV = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_SuaDV);
            }
        }

        public bool mDangKyNoiTru_XemChiTiet
        {
            get
            {
                return _mDangKyNoiTru_XemChiTiet;
            }
            set
            {
                if (_mDangKyNoiTru_XemChiTiet == value)
                    return;
                _mDangKyNoiTru_XemChiTiet = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_XemChiTiet);
            }
        }

        public bool mDangKyNoiTru_In
        {
            get
            {
                return _mDangKyNoiTru_In;
            }
            set
            {
                if (_mDangKyNoiTru_In == value)
                    return;
                _mDangKyNoiTru_In = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_In);
            }
        }

        public bool mDangKyNoiTru_TraThuoc
        {
            get
            {
                return _mDangKyNoiTru_TraThuoc;
            }
            set
            {
                if (_mDangKyNoiTru_TraThuoc == value)
                    return;
                _mDangKyNoiTru_TraThuoc = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_TraThuoc);
            }
        }

        //phan nay nam trong module chung
        private bool _mDangKyNoiTru_Patient_TimBN = true;
        private bool _mDangKyNoiTru_Patient_ThemBN = true;
        private bool _mDangKyNoiTru_Patient_TimDangKy = true;

        private bool _mDangKyNoiTru_Info_CapNhatThongTinBN = true;
        private bool _mDangKyNoiTru_Info_XacNhan = true;
        private bool _mDangKyNoiTru_Info_XoaThe = true;
        private bool _mDangKyNoiTru_Info_XemPhongKham = true;

        public bool mDangKyNoiTru_Patient_TimBN
        {
            get
            {
                return _mDangKyNoiTru_Patient_TimBN;
            }
            set
            {
                if (_mDangKyNoiTru_Patient_TimBN == value)
                    return;
                _mDangKyNoiTru_Patient_TimBN = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_Patient_TimBN);
            }
        }

        public bool mDangKyNoiTru_Patient_ThemBN
        {
            get
            {
                return _mDangKyNoiTru_Patient_ThemBN;
            }
            set
            {
                if (_mDangKyNoiTru_Patient_ThemBN == value)
                    return;
                _mDangKyNoiTru_Patient_ThemBN = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_Patient_ThemBN);
            }
        }

        public bool mDangKyNoiTru_Patient_TimDangKy
        {
            get
            {
                return _mDangKyNoiTru_Patient_TimDangKy;
            }
            set
            {
                if (_mDangKyNoiTru_Patient_TimDangKy == value)
                    return;
                _mDangKyNoiTru_Patient_TimDangKy = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_Patient_TimDangKy);
            }
        }

        public bool mDangKyNoiTru_Info_CapNhatThongTinBN
        {
            get
            {
                return _mDangKyNoiTru_Info_CapNhatThongTinBN;
            }
            set
            {
                if (_mDangKyNoiTru_Info_CapNhatThongTinBN == value)
                    return;
                _mDangKyNoiTru_Info_CapNhatThongTinBN = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_Info_CapNhatThongTinBN);
            }
        }

        public bool mDangKyNoiTru_Info_XacNhan
        {
            get
            {
                return _mDangKyNoiTru_Info_XacNhan;
            }
            set
            {
                if (_mDangKyNoiTru_Info_XacNhan == value)
                    return;
                _mDangKyNoiTru_Info_XacNhan = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_Info_XacNhan);
            }
        }

        public bool mDangKyNoiTru_Info_XoaThe
        {
            get
            {
                return _mDangKyNoiTru_Info_XoaThe;
            }
            set
            {
                if (_mDangKyNoiTru_Info_XoaThe == value)
                    return;
                _mDangKyNoiTru_Info_XoaThe = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_Info_XoaThe);
            }
        }

        public bool mDangKyNoiTru_Info_XemPhongKham
        {
            get
            {
                return _mDangKyNoiTru_Info_XemPhongKham;
            }
            set
            {
                if (_mDangKyNoiTru_Info_XemPhongKham == value)
                    return;
                _mDangKyNoiTru_Info_XemPhongKham = value;
                NotifyOfPropertyChange(() => mDangKyNoiTru_Info_XemPhongKham);
            }
        }
        #endregion

        public void Handle(DoubleClickAddReqLAB message)
        {
            if (message.Source != SelectPCLContentLAB)
            {
                return;
            }
            if (message.EventArgs.Value != SelectPCLContentLAB.SelectedPCLExamType)
            {
                return;
            }
            AddPclExamTypeCmdLAB();
        }

        #region Add LAB

        public void AddAllPclExamTypeCmdLAB()
        {
            ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults;
            if (!ValidatePclItemLAB(false, out validationResults))
            {
                Globals.EventAggregator.Publish(new ValidateFailedEvent() { ValidationResults = validationResults });
                return;
            }
            //bool used = SelectPCLContentLAB != null && SelectPCLContent.Used;

            bool used = true;/*A Tuan quyet dinh lam phieu CLS noi tru tai man hinh nay thi la da thuc hien roi*/

            CheckAndAddAllPCL(SelectPCLContentLAB.PclExamTypes, PclQtyLAB.Value, (DateTime)UseAtDateContent.DateTime, used);
        }

        public void AddPclExamTypeCmdLAB()
        {
            PopupModifyPrice_Type = AllLookupValues.PopupModifyPrice_Type.INSERT_PCL_XETNGHIEM;
            ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults;
            if (!ValidatePclItemLAB(false, out validationResults))
            {
                Globals.EventAggregator.Publish(new ValidateFailedEvent() { ValidationResults = validationResults });
                return;
            }
            //bool used = SelectPCLContentLAB != null && SelectPCLContent.Used;
            bool used = true;/*A Tuan quyet dinh lam phieu CLS noi tru tai man hinh nay thi la da thuc hien roi*/

            //AddPCLItem(SelectPCLContentLAB.SelectedPCLExamType, PclQtyLAB.Value, (DateTime)UseAtDateContent.DateTime, used);
            CheckAndAddPCL(SelectPCLContentLAB.SelectedPCLExamType, PclQtyLAB.Value, (DateTime)UseAtDateContent.DateTime, used);
        }

        private bool ValidatePclItemLAB(bool IsAddAll, out ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> result)
        {
            result = new ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult>();

            //if (!SelectedDate.HasValue)
            //{
            //    var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0161_G1_HayChonNgSDDV, new string[] { "SelectedDate" });
            //    result.Add(item);
            //}

            if ((SelectPCLContentLAB.PclExamTypes == null || SelectPCLContentLAB.PclExamTypes.Count <= 0
                || SelectPCLContentLAB.SelectedPCLExamType == null || SelectPCLContentLAB.SelectedPCLExamType.PCLExamTypeID < 1) && !IsAddAll)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0171_G1_HayChonLoaiCLSCanThem, new string[] { "SelectedPclExamType" });
                result.Add(item);
            }
            if (PclQtyLAB.GetValueOrDefault(0) < 1)
            {
                var item = new System.ComponentModel.DataAnnotations.ValidationResult(eHCMSResources.Z0163_G1_NhapGTriSLgDV, new string[] { "PclQtyLAB" });
                result.Add(item);
            }
            if (SelectPCLContentLAB != null && ((SelectPCLContentLAB.SelectedPCLExamType != null && SelectPCLContentLAB.SelectedPCLExamType.HIAllowedPrice > 0) || (SelectPCLContentLAB.PclExamTypes != null && SelectPCLContentLAB.PclExamTypes.Any(x => x.HIAllowedPrice > 0))))
            {
                CheckDoctorAndDate(ref result);
            }
            if (result.Count > 0)
            {
                return false;
            }
            return true;
        }

        #endregion

        public void GetAllInPatientBillingInvoices()
        {
            this.ShowBusyIndicator(eHCMSResources.Z1125_G1_DangTimKiem);
            var t = new Thread(() =>
            {
                AxErrorEventArgs error = null;
                try
                {
                    using (var serviceFactory = new BillingPaymentWcfServiceLibClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        //==== #002
                        //contract.BeginGetAllInPatientBillingInvoices(CurRegistration.PtRegistrationID, DeptID,
                        contract.BeginGetInPatientAllBillingInvoiceSummary(CurRegistration.PtRegistrationID, (long)DeptID
                            , FromDate, ToDate, false
                            , Globals.DispatchCallback((asyncResult) =>
                            {
                                try
                                {
                                    AllRegistrationItems = contract.EndGetInPatientAllBillingInvoiceSummary(asyncResult);
                                    AllRegistrationItemsBackUp = AllRegistrationItems.DeepCopy();
                                    AllRegistrationItemsFilter = AllRegistrationItems.DeepCopy();
                                }
                                catch (FaultException<AxException> fault)
                                {
                                    error = new AxErrorEventArgs(fault);
                                }
                                catch (Exception ex)
                                {
                                    error = new AxErrorEventArgs(ex);
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
                    error = new AxErrorEventArgs(ex);
                    this.HideBusyIndicator();
                }
                if (error != null)
                {
                    Globals.EventAggregator.Publish(new ErrorOccurred() { CurrentError = error });
                }
            });

            t.Start();
        }

        public void Handle(InPatientReturnMedProduct message)
        {
            if (IsActive && message != null)
            {
                GetAllInPatientBillingInvoices();
            }
        }

        public void Handle(ModifyPriceToInsert_Completed message)
        {
            if (message == null || message.ModifyItem == null)
            {
                return;
            }
            if (!message.ModifyItem.IsModItemOK)
            {
                return;
            }
            ReasonChangePrice = message.ModifyItem.ReasonChangePrice;
            if (PopupModifyPrice_Type == AllLookupValues.PopupModifyPrice_Type.INSERT_DICHVU)
            {
                InPatientSelectServiceContent.MedServiceItem.HIAllowedPrice = message.ModifyItem.ChargeableItem.HIAllowedPrice.GetValueOrDefault();
                InPatientSelectServiceContent.MedServiceItem.NormalPrice = InPatientSelectServiceContent.MedServiceItem.HIPatientPrice = message.ModifyItem.ChargeableItem.NormalPrice;
                AddGenMedService(InPatientSelectServiceContent.MedServiceItem, ServiceQty.Value, (DateTime)UseAtDateContent.DateTime);
            }
            if (PopupModifyPrice_Type == AllLookupValues.PopupModifyPrice_Type.INSERT_GIUONG)
            {
                SelectBedContent.MedServiceItem.HIAllowedPrice = message.ModifyItem.ChargeableItem.HIAllowedPrice.GetValueOrDefault();
                SelectBedContent.MedServiceItem.NormalPrice = SelectBedContent.MedServiceItem.HIPatientPrice = message.ModifyItem.ChargeableItem.NormalPrice;
                AddGenMedService(SelectBedContent.MedServiceItem, BedQty.Value, (DateTime)UseAtDateContent.DateTime);
            }
            if (PopupModifyPrice_Type == AllLookupValues.PopupModifyPrice_Type.INSERT_MAU)
            {
                SelectBloodContent.MedServiceItem.HIAllowedPrice = message.ModifyItem.ChargeableItem.HIAllowedPrice.GetValueOrDefault();
                SelectBloodContent.MedServiceItem.NormalPrice = SelectBloodContent.MedServiceItem.HIPatientPrice = message.ModifyItem.ChargeableItem.NormalPrice;
                AddGenMedService(SelectBloodContent.MedServiceItem, BloodQty.Value, (DateTime)UseAtDateContent.DateTime);
            }
            if (PopupModifyPrice_Type == AllLookupValues.PopupModifyPrice_Type.INSERT_PHAUTHUAT_THUTHUAT)
            {
                SelectGeneralSugeryContent.MedServiceItem.HIAllowedPrice = message.ModifyItem.ChargeableItem.HIAllowedPrice.GetValueOrDefault();
                SelectGeneralSugeryContent.MedServiceItem.NormalPrice = SelectGeneralSugeryContent.MedServiceItem.HIPatientPrice = message.ModifyItem.ChargeableItem.NormalPrice;
                AddGenMedService(SelectGeneralSugeryContent.MedServiceItem, SugeryQty.Value, (DateTime)UseAtDateContent.DateTime);
            }
            if (PopupModifyPrice_Type == AllLookupValues.PopupModifyPrice_Type.INSERT_PCL_HINHANH)
            {
                SelectPCLContent.SelectedPCLExamType.HIAllowedPrice = message.ModifyItem.ChargeableItem.HIAllowedPrice.GetValueOrDefault();
                SelectPCLContent.SelectedPCLExamType.NormalPrice = SelectPCLContent.SelectedPCLExamType.HIPatientPrice = message.ModifyItem.ChargeableItem.NormalPrice;
                AddPCLItem(SelectPCLContent.SelectedPCLExamType, PclQty.Value, (DateTime)UseAtDateContent.DateTime, true);
            }
            if (PopupModifyPrice_Type == AllLookupValues.PopupModifyPrice_Type.INSERT_PCL_XETNGHIEM)
            {
                SelectPCLContentLAB.SelectedPCLExamType.HIAllowedPrice = message.ModifyItem.ChargeableItem.HIAllowedPrice.GetValueOrDefault();
                SelectPCLContentLAB.SelectedPCLExamType.NormalPrice = SelectPCLContentLAB.SelectedPCLExamType.HIPatientPrice = message.ModifyItem.ChargeableItem.NormalPrice;
                AddPCLItem(SelectPCLContentLAB.SelectedPCLExamType, PclQty.Value, (DateTime)UseAtDateContent.DateTime, true);
            }
        }

        //▼====: #008
        public void CheckValidDate()
        {
            if (DateTime.Compare(EditingInvoiceDetailsContent.BillingInvoice.BillToDate.GetValueOrDefault(default(DateTime)).Date, EditingInvoiceDetailsContent.BillingInvoice.BillFromDate.GetValueOrDefault(default(DateTime)).Date) > 0)
            {
                MessageBox.Show(string.Format(eHCMSResources.Z2211_G1_KgCoGiDeLoadTuDen, EditingInvoiceDetailsContent.BillingInvoice.BillFromDate.GetValueOrDefault(default(DateTime)).ToString("dd/MM/yyyy"), EditingInvoiceDetailsContent.BillingInvoice.BillToDate.GetValueOrDefault(default(DateTime)).ToString("dd/MM/yyyy")));
            }
            else
            {
                MessageBox.Show(string.Format(eHCMSResources.Z2215_G1_KgCoGiDeLoadTrong, EditingInvoiceDetailsContent.BillingInvoice.BillToDate.GetValueOrDefault(default(DateTime)).ToString("dd/MM/yyyy")));
            }
        }
        //▲====: #008

        #region Định nghĩa các biến 
        private ObservableCollection<Staff> _StaffCatgs;
        public ObservableCollection<Staff> StaffCatgs
        {
            get
            {
                return _StaffCatgs;
            }
            set
            {
                if (_StaffCatgs != value)
                {
                    _StaffCatgs = value;
                    NotifyOfPropertyChange(() => StaffCatgs);
                }
            }
        }
        private string _ItemNameFilter;
        public string ItemNameFilter
        {
            get
            {
                return _ItemNameFilter;
            }
            set
            {
                _ItemNameFilter = value;
                AllRegistrationItemsFilter = AllRegistrationItems.Where(o => (Common.Utilities.VNConvertString.ConvertString(o.ChargeableItemName).ToLower().Contains(Common.Utilities.VNConvertString.ConvertString(ItemNameFilter).ToLower()))
                                                && o.RecordState != RecordState.DELETED).ToObservableCollection();
                NotifyOfPropertyChange(() => AllRegistrationItemsFilter);
                NotifyOfPropertyChange(() => ItemNameFilter);
            }
        }
        private bool IsLoadNoBill = false;
        private string _ReasonChangePrice;
        public string ReasonChangePrice
        {
            get
            {
                return _ReasonChangePrice;
            }
            set
            {
                _ReasonChangePrice = value;
                NotifyOfPropertyChange(() => ReasonChangePrice);
            }
        }

        private AllLookupValues.PopupModifyPrice_Type _PopupModifyPrice_Type;
        public AllLookupValues.PopupModifyPrice_Type PopupModifyPrice_Type
        {
            get
            {
                return _PopupModifyPrice_Type;
            }
            set
            {
                _PopupModifyPrice_Type = value;
                NotifyOfPropertyChange(() => PopupModifyPrice_Type);
            }
        }
        private string _TitleForm = eHCMSResources.T0784_G1_TaoBillNoiTru;
        public string TitleForm
        {
            get
            {
                return _TitleForm;
            }
            set
            {
                _TitleForm = value;
                NotifyOfPropertyChange(() => TitleForm);
            }
        }

        private bool _IsEnableTestFunction = Globals.ServerConfigSection.CommonItems.EnableTestFunction;
        public bool IsEnableTestFunction
        {
            get
            {
                return _IsEnableTestFunction;
            }
            set
            {
                _IsEnableTestFunction = value;
                NotifyOfPropertyChange(() => IsEnableTestFunction);
            }
        }

        private bool _IsNotCheckInvalid;
        public bool IsNotCheckInvalid
        {
            get { return _IsNotCheckInvalid; }
            set
            {
                _IsNotCheckInvalid = value;
                NotifyOfPropertyChange(() => IsNotCheckInvalid);
            }
        }

        private bool _IsNewCreateBill = false;
        public bool IsNewCreateBill
        {
            get
            {
                return _IsNewCreateBill;
            }
            set
            {
                if (_IsNewCreateBill != value)
                {
                    _IsNewCreateBill = value;
                    if (EditingInvoiceDetailsContent != null)
                    {
                        EditingInvoiceDetailsContent.IsNewCreateBill = _IsNewCreateBill;
                        OldBillingInvoiceContent.IsNewCreateBill = _IsNewCreateBill;
                    }
                    NotifyOfPropertyChange(() => IsNewCreateBill);
                }
            }
        }

        //▼====: #024
        private List<RefMedicalServiceGroups> _MedicalServiceGroupCollection;
        public List<RefMedicalServiceGroups> MedicalServiceGroupCollection
        {
            get => _MedicalServiceGroupCollection; set
            {
                _MedicalServiceGroupCollection = value;
                NotifyOfPropertyChange(() => MedicalServiceGroupCollection);
            }
        }
        private RefMedicalServiceGroups _RefMedicalServiceGroupObj;
        public RefMedicalServiceGroups RefMedicalServiceGroupObj
        {
            get => _RefMedicalServiceGroupObj; set
            {
                _RefMedicalServiceGroupObj = value;
                NotifyOfPropertyChange(() => RefMedicalServiceGroupObj);
            }
        }

        private ObservableCollection<MedRegItemBase> _allRegistrationItems;
        public ObservableCollection<MedRegItemBase> AllRegistrationItems
        {
            get { return _allRegistrationItems; }
            set
            {
                _allRegistrationItems = value;
                NotifyOfPropertyChange(() => AllRegistrationItems);
            }
        }

        private ObservableCollection<MedRegItemBase> _AllRegistrationItemsBackUp;
        public ObservableCollection<MedRegItemBase> AllRegistrationItemsBackUp
        {
            get { return _AllRegistrationItemsBackUp; }
            set
            {
                _AllRegistrationItemsBackUp = value;
                NotifyOfPropertyChange(() => AllRegistrationItemsBackUp);
            }
        }

        private ObservableCollection<MedRegItemBase> _allRegistrationItemsFilter;
        public ObservableCollection<MedRegItemBase> AllRegistrationItemsFilter
        {
            get { return _allRegistrationItemsFilter; }
            set
            {
                _allRegistrationItemsFilter = value;
                NotifyOfPropertyChange(() => AllRegistrationItemsFilter);
            }
        }

        private MedRegItemBase _SelectedRegistrationItem;
        public MedRegItemBase SelectedRegistrationItem
        {
            get { return _SelectedRegistrationItem; }
            set
            {
                _SelectedRegistrationItem = value;
                NotifyOfPropertyChange(() => SelectedRegistrationItem);
            }
        }

        public MedRegItemBase SelectedRegistrationItemFilter
        {
            get { return _SelectedRegistrationItem; }
            set
            {
                _SelectedRegistrationItem = value;
                NotifyOfPropertyChange(() => SelectedRegistrationItem);
            }
        }

        private Visibility _IsShowPatientCOVID = Visibility.Collapsed;
        public Visibility IsShowPatientCOVID
        {
            get
            {
                return _IsShowPatientCOVID;
            }
            set
            {
                if (_IsShowPatientCOVID != value)
                {
                    _IsShowPatientCOVID = value;
                    //▼====: #019
                    mCheckCovid = Globals.CheckOperation(Globals.listRefModule, (int)eModules.mPatient, (int)ePatient.mInPatientRegister_TV, (int)oCreateBill.mCheckCovid);
                    //▲====: #019
                    NotifyOfPropertyChange(() => IsShowPatientCOVID);
                }
            }
        }

        private MedRegItemBase _ModifyBillingInvItem;
        public MedRegItemBase ModifyBillingInvItem
        {
            get
            {
                return _ModifyBillingInvItem;
            }
            set
            {
                _ModifyBillingInvItem = value;
                NotifyOfPropertyChange(() => ModifyBillingInvItem);
            }
        }

        private AllLookupValues.PopupModifyPrice_Type _ModifyType;
        public AllLookupValues.PopupModifyPrice_Type ModifyType
        {
            get
            {
                return _ModifyType;
            }
            set
            {
                _ModifyType = value;
                NotifyOfPropertyChange(() => ModifyType);
            }
        }
        #endregion

        #region Định nghĩa các hàm trong XML
        public void CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var item = e.Row.DataContext as MedRegItemBase;
            if (item is PatientRegistrationDetail)
            {
                if ((item as PatientRegistrationDetail).PtRegDetailID > 0)
                {
                    item.RecordState = RecordState.MODIFIED;
                }
            }
            else if (item is PatientPCLRequestDetail)
            {
                if ((item as PatientPCLRequestDetail).PCLReqItemID > 0)
                {
                    ModifiItemCmd(item);
                }
            }

            else if (item is OutwardDrugClinicDept)
            {
                var inv = item as OutwardDrugClinicDept;
                inv.OutQuantity = inv.Qty + inv.QtyReturn;

                if (inv.DrugInvoice != null && inv.DrugInvoice.outiID > 0)
                {
                    inv.DrugInvoice.RecordState = RecordState.MODIFIED;
                }
            }
            if (item != null && !item.HasErrors)
            {
                CalcInvoiceItem(item, true);
            }
            if (item.IsDiscounted)
            {
                CurRegistration.ApplyDiscount(item, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
            }
            //Globals.EventAggregator.Publish(new ItemChanged<MedRegItemBase, IInPatientBillingInvoiceDetailsListing>() { Item = item, Source = this });
        }

        public void BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            object ctx = e.Row.DataContext;
            if ((ctx is PatientRegistrationDetail) && (ctx as PatientRegistrationDetail) != null && (ctx as PatientRegistrationDetail).IntPtDiagDrInstructionID.GetValueOrDefault(0) > 0 &&
                (e.Column.Equals(grid.GetColumnByName("colMedicalInstructionDate")) || e.Column.Equals(grid.GetColumnByName("colDoctorStaff"))))
            {
                e.Cancel = true;
            }
            else if (ctx is PatientPCLRequestDetail)
            {
                if (e.Column.Equals(grid.GetColumnByName("colQty")))
                {
                    MessageBox.Show(string.Format("{0}.", eHCMSResources.A0263_G1_Msg_InfoKhDcSuaSluongCLS), eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                    e.Cancel = true;
                }
                else if ((e.Column.Equals(grid.GetColumnByName("colMedicalInstructionDate")) || e.Column.Equals(grid.GetColumnByName("colDoctorStaff"))) && (ctx as PatientPCLRequestDetail).PatientPCLReqID > 0)
                {
                    e.Cancel = true;
                }
            }
            else if (ctx is OutwardDrugClinicDept)
            {
                OutwardDrugClinicDept owd = (OutwardDrugClinicDept)ctx;
                if (owd.outiID > 0 || owd.OutID > 0)
                {
                    MessageBox.Show(eHCMSResources.K0081_G1_I, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                    e.Cancel = true;
                }
            }
        }

        public void EditMedicalInstructionDate(MedRegItemBase source, object eventArgs)
        {
            IMinHourDateControl mMinHourCtr = Globals.GetViewModel<IMinHourDateControl>();
            mMinHourCtr.DateTime = source.MedicalInstructionDate;
            mMinHourCtr.bShowButton = true;
            GlobalsNAV.ShowDialog_V3<IMinHourDateControl>(mMinHourCtr);
            if (mMinHourCtr.bOK)
            {
                source.MedicalInstructionDate = mMinHourCtr.DateTime;
            }
        }

        public void EditResultDate(MedRegItemBase source, object eventArgs)
        {
            IMinHourDateControl mMinHourCtr = Globals.GetViewModel<IMinHourDateControl>();
            mMinHourCtr.DateTime = source.ResultDate;
            mMinHourCtr.bShowButton = true;
            GlobalsNAV.ShowDialog_V3<IMinHourDateControl>(mMinHourCtr);
            if (mMinHourCtr.bOK)
            {
                source.ResultDate = mMinHourCtr.DateTime;
            }
        }

        AutoCompleteBox AutoDoctor;
        public void AutoDoctor_Loaded(object sender, RoutedEventArgs e)
        {
            AutoDoctor = sender as AutoCompleteBox;
        }

        public void AutoDoctor_Populating(object sender, PopulatingEventArgs e)
        {
            if (Globals.AllStaffs == null || Globals.AllStaffs.Count <= 0)
            {
                return;
            }

            StaffCatgs = Globals.AllStaffs.Where(o => o.RefStaffCategory != null && o.RefStaffCategory.V_StaffCatType == (long)V_StaffCatType.BacSi
                                    && (Common.Utilities.VNConvertString.ConvertString(o.FullName).ToLower().Contains(Common.Utilities.VNConvertString.ConvertString(e.Parameter).ToLower()))
                                    && !o.IsStopUsing).ToObservableCollection();
            NotifyOfPropertyChange(() => StaffCatgs);
            AutoDoctor.ItemsSource = StaffCatgs;
            AutoDoctor.PopulateComplete();
        }

        public void AutoDoctor_DropDownClosed(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (SelectedRegistrationItem == null)
            {
                return;
            }

            if (SelectedRegistrationItem.DoctorStaff == null)
            {
                SelectedRegistrationItem.DoctorStaff = new Staff();
            }

            if (AutoDoctor != null && AutoDoctor.SelectedItem != null)
            {
                SelectedRegistrationItem.DoctorStaff.StaffID = (AutoDoctor.SelectedItem as Staff).StaffID;
                SelectedRegistrationItem.DoctorStaff.FullName = (AutoDoctor.SelectedItem as Staff).FullName;
            }
            else
            {
                SelectedRegistrationItem.DoctorStaff.StaffID = 0;
                SelectedRegistrationItem.DoctorStaff.FullName = "";
            }
        }

        public void chkCountHI_Click(object source, object sender)
        {
            if (SelectedRegistrationItem is PatientRegistrationDetail)
            {
                PatientRegistrationDetail detail = SelectedRegistrationItem as PatientRegistrationDetail;
                if (!detail.IsCountHI && detail.V_EkipIndex != null && detail.V_EkipIndex.LookupID > 0)
                {
                    if (MessageBox.Show(eHCMSResources.Z2977_G1_XoaEkip, eHCMSResources.G0442_G1_TBao, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        detail.V_Ekip = new Lookup();
                        detail.V_EkipIndex = new Lookup();
                    }
                    else
                    {
                        detail.IsCountHI = true;
                        return;
                    }
                }
            }
            string error = "";

            error = CheckHIError(SelectedRegistrationItem);

            //▼====: #020
            if (SelectedRegistrationItem.IsCountHI)
            {
                SelectedRegistrationItem.IsCountSE = false;
            }
            //▲====: #020

            if (SelectedRegistrationItem.IsCountHI && !string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                SelectedRegistrationItem.IsCountHI = false;
                return;
            }

            //20191217 TTM: Nếu là được tích bảo hiểm thì sẽ set thời gian tích bảo hiểm cho vật tư để tính toán số thứ tự của vật tư dựa vào thời gian tích (Tích trước xếp đầu tiên rồi lần lượt)
            if (!IsNewCreateBill)
            {
                CalcInvoiceItem(SelectedRegistrationItem);
            }
            else
            {
                //▼===== 20191230 Kiểm tra nếu là OutwardDrugClinicDept thì mới set ticktime không sẽ không làm. Vì nếu không kiểm tra mà tích BH cho dịch vụ hoặc CLS sẽ => Chết chương trình
                if (SelectedRegistrationItem is OutwardDrugClinicDept)
                {
                    if (SelectedRegistrationItem.IsCountHI)
                    {
                        SelectedRegistrationItem.TickTime = AllRegistrationItems.Where(x => x is OutwardDrugClinicDept).Where(x => (x as OutwardDrugClinicDept).LimQtyAndHIPrice != null && x.TickTime < 100).Max(y => y.TickTime).GetValueOrDefault(0) + 1;
                    }
                    else
                    {
                        var tmpitem = AllRegistrationItems.Where(x => x is OutwardDrugClinicDept).Where(x => (x as OutwardDrugClinicDept).LimQtyAndHIPrice != null
                        && (x as OutwardDrugClinicDept).OutID == (SelectedRegistrationItem as OutwardDrugClinicDept).OutID);
                        foreach (var tmp in tmpitem)
                        {
                            if ((tmp as OutwardDrugClinicDept).OutID == (SelectedRegistrationItem as OutwardDrugClinicDept).OutID)
                            {
                                tmp.TickTime = 0;
                            }
                        }
                    }
                }
                CalcInvoiceItem(SelectedRegistrationItem, true, true);
                foreach (var item in AllRegistrationItems.Where(x => x is OutwardDrugClinicDept).Where(x => (x as OutwardDrugClinicDept).LimQtyAndHIPrice != null))
                {
                    ModifiItemCmd(item);
                }
            }
            ModifiItemCmd(SelectedRegistrationItem);
            if (SelectedRegistrationItem.IsDiscounted)
            {
                CurRegistration.ApplyDiscount(SelectedRegistrationItem, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
            }
            // cập nhật item đang chỉnh vào danh sách gốc AllRegistrationItems
            UpdateDataItemFromGrid(SelectedRegistrationItem);
        }

        public void PrintOldBillingInvoiceCmd()
        {
            List<long> ids = OldBillingInvoiceContent.GetSelectedIds();
            if (ids != null && ids.Count > 0)
            {
                ShowBillingReport(ids[0]);
            }
            else
            {
                MessageBox.Show(eHCMSResources.A0591_G1_Msg_InfoChonDVDeIn);
            }
        }

        public void PrintBillingInvoiceCmd()
        {
            if (EditingBillingInvoice == null || EditingBillingInvoice.InPatientBillingInvID <= 0)
            {
                MessageBox.Show(eHCMSResources.A0653_G1_Msg_InfoKhCoHDDeIn, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                return;
            }

            ShowBillingReport(EditingBillingInvoice.InPatientBillingInvID);
        }

        public void CreateSuggestCashAdvanceCmd()
        {
            if (Globals.IsLockRegistration(CurRegistration.RegLockFlag, eHCMSResources.K3167_G1_DNghiTU.ToLower()))
            {
                return;
            }
            if (EditingBillingInvoice == null || EditingBillingInvoice.InPatientBillingInvID <= 0)
            {
                MessageBox.Show(eHCMSResources.A0654_G1_Msg_InfoKhCoHDDeDNTU, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                return;
            }

            if (MessageBox.Show(eHCMSResources.A0142_G1_Msg_ConfTaoPhDNTUChoBill, eHCMSResources.G2363_G1_XNhan, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                return;
            }

            long RptPtCashAdvRemID = 0;

            this.ShowBusyIndicator(eHCMSResources.K2887_G1_DangXuLy);
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new BillingPaymentWcfServiceLibClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginCreateSuggestCashAdvance(EditingBillingInvoice.InPatientBillingInvID, Globals.LoggedUserAccount.StaffID.GetValueOrDefault(0),
                            Globals.DispatchCallback((asyncResult) =>
                            {
                                try
                                {
                                    var res = contract.EndCreateSuggestCashAdvance(out RptPtCashAdvRemID, asyncResult);
                                    if (res)
                                    {
                                        EditingBillingInvoice.RptPtCashAdvRemID = RptPtCashAdvRemID;
                                        MessageBox.Show(eHCMSResources.A0759_G1_Msg_InfoLapPhDNTUOK, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                                    }
                                    else
                                    {
                                        MessageBox.Show(eHCMSResources.A0760_G1_Msg_InfoLapPhDNTUFail, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
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
                    MessageBox.Show(ex.Message, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                    this.HideBusyIndicator();
                }
            });

            t.Start();
        }

        public void PrintSuggestCashAdvanceCmd()
        {
            if (EditingBillingInvoice == null || EditingBillingInvoice.RptPtCashAdvRemID <= 0)
            {
                MessageBox.Show(eHCMSResources.A0260_G1_Msg_InfoBillKhCoPhDNTUDeIn, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                return;
            }

            void onInitDlg(ICommonPreviewView proAlloc)
            {
                proAlloc.ID = EditingBillingInvoice.RptPtCashAdvRemID;
                // 20181017 TNHX: [BM0002176] Change PHIEUDENGHITAMUNG -> PHIEUDENGHITAMUNG_TV
                proAlloc.eItem = ReportName.PHIEUDENGHITAMUNG_TV;
            }
            GlobalsNAV.ShowDialog<ICommonPreviewView>(onInitDlg);
        }

        public void LoadBillCmd()
        {
            if (!CheckForPreloadingBills())
            {
                return;
            }

            if (!CheckDeptID())
            {
                return;
            }
            if (SelectedWarehouse == null)
            {
                MessageBox.Show(eHCMSResources.K0338_G1_ChonKho, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK);
                return;
            }
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            if (EditingInvoiceDetailsContent != null && EditingInvoiceDetailsContent.BillingInvoice != null)
            {
                FromDate = EditingInvoiceDetailsContent.BillingInvoice.BillFromDate;
                ToDate = EditingInvoiceDetailsContent.BillingInvoice.BillToDate;
            }
            //▼====: #008
            if (FromDate == null)
            {
                MessageBox.Show(eHCMSResources.Z1656_G1_ChuaNhapTuNg);
                return;
            }
            if (ToDate == null)
            {
                MessageBox.Show(eHCMSResources.Z1657_G1_ChuaNhapDenNg);
                return;
            }
            //▲====: #008
            //KMx: Không cần thiết gọi hàm này, nếu gọi hàm này sẽ bị lỗi: Người dùng chọn BillFromDate, BillToDate, chương trình tự set về ngày hiện tại (02/01/2015 17:40).
            //BeginEdit();

            ISelectTypeLoadBill SelectedLoadBillType = Globals.GetViewModel<ISelectTypeLoadBill>();
            if (Globals.ServerConfigSection.CommonItems.AutoAddBedService)
            {
                GlobalsNAV.ShowDialog_V3(SelectedLoadBillType, (aView) =>
                {
                    aView.FromDate = Globals.GetCurServerDateTime();
                    aView.ToDate = Globals.GetCurServerDateTime();
                    aView.DischargeDate = CurRegistration.AdmissionInfo != null ? CurRegistration.AdmissionInfo.DischargeDetailRecCreatedDate : null;
                });
            }
            if (SelectedLoadBillType.IsCompleted)
            {
                this.ShowBusyIndicator(eHCMSResources.Z0151_G1_DangLayTTinBill);
                var t = new Thread(() =>
                {
                    AxErrorEventArgs error = null;
                    try
                    {
                        using (var serviceFactory = new BillingPaymentWcfServiceLibClient())
                        {
                            var contract = serviceFactory.ServiceInstance;
                            //HPT 13/09/2016: Đưa biến cấu hình load phiếu chỉ định cận lâm sàng xuống để biết load hay không load, load như thế nào
                            //HPT: tạm thời chỗ này lấy y nguyên cấu hình đưa xuống, tuy nhiên anh Tuấn nói sau này có thể kết hợp thêm một số điều kiện trên giao diện nữa nên cứ tạo đường đi trước.
                            contract.BeginLoadInPatientRegItemsIntoBill(CurRegistration.PtRegistrationID, DeptID, SelectedWarehouse.StoreID
                                , Globals.LoggedUserAccount.StaffID.GetValueOrDefault()
                                , !Globals.isAccountCheck, FromDate, ToDate, (int)PatientFindBy
                                , SelectedLoadBillType.LoadBillType
                                , CurRegistration.AdmissionInfo != null ? CurRegistration.AdmissionInfo.DischargeDetailRecCreatedDate : null
                                , Globals.DispatchCallback((asyncResult) =>
                                {
                                    try
                                    {
                                        var inv = contract.EndLoadInPatientRegItemsIntoBill(asyncResult);
                                        if (inv != null)
                                        {
                                            if (CheckIfInvoiceIsEmpty(inv))
                                            {
                                                //▼====: #008
                                                CheckValidDate();
                                                //▲====: #008
                                            }
                                            else
                                            {
                                                DateTime? billFromDate = EditingInvoiceDetailsContent.BillingInvoice.BillFromDate;
                                                DateTime? billToDate = EditingInvoiceDetailsContent.BillingInvoice.BillToDate;
                                                //▼====: #008
                                                bool checkAdd = false;
                                                //▲====: #008
                                                //KMx: Hiện tại load bill chỉ load thuốc/ y cụ/ hóa chất. Khi nào thêm chức năng load DV và CLS thì sửa lại (31/12/2015 12:00).
                                                ////KMx: Chỉ load những DV có ngày sử dụng <= Đến ngày (03/01/2015 10:57).
                                                if (inv.RegistrationDetails != null && inv.RegistrationDetails.Count > 0)
                                                {
                                                    foreach (var request in inv.RegistrationDetails)
                                                    {
                                                        if (request.InPatientBillingInvID != null && request.InPatientBillingInvID > 0)
                                                        {
                                                            continue;
                                                        }
                                                        if (EditingBillingInvoice == null)
                                                        {
                                                            EditingBillingInvoice = new InPatientBillingInvoice();
                                                        }
                                                        if (EditingBillingInvoice.RegistrationDetails == null)
                                                        {
                                                            EditingBillingInvoice.RegistrationDetails = new ObservableCollection<PatientRegistrationDetail>();
                                                        }
                                                        PatientRegistrationDetail tempRequest = EditingBillingInvoice.RegistrationDetails.FirstOrDefault(x => x.PtRegDetailID == request.PtRegDetailID);
                                                        //▼====: #025
                                                        if (CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.IsTreatmentCOVID
                                                        && request.RefMedicalServiceItem.InCategoryCOVID)
                                                        {
                                                            request.IsCountPatientCOVID = true;
                                                        }
                                                        //▲====: #025
                                                        if (SelectedLoadBillType.LoadBillType == (int)AllLookupValues.LoadBillType.BINHTHUONG)
                                                        {
                                                            if (tempRequest == null || (tempRequest != null && tempRequest.RecordState == RecordState.DELETED))
                                                            {
                                                                if (tempRequest != null && tempRequest.RecordState == RecordState.DELETED)
                                                                {
                                                                    EditingBillingInvoice.RegistrationDetails.Remove(tempRequest);
                                                                }
                                                                request.RecordState = RecordState.DETACHED;
                                                                //▼====: #008
                                                                checkAdd = true;
                                                                //▲====: #008

                                                                EditingBillingInvoice.RegistrationDetails.Add(request);
                                                                //▼===== #019:  Kiểm tra thì thấy khi vào AddMedRegItemBaseToNewInvoice để chuyển dữ liệu sang cho 
                                                                //              InPatientBillingInvoiceDetailsListingView thì trong view này cũng có tính toán lại 
                                                                //              => Không cần vào globals tính toán làm gì nữa.
                                                                //GlobalsNAV.CalcInvoiceItem(request, IsHighTechServiceBill, CurRegistration);
                                                                //▲===== #019
                                                                AddMedRegItemBaseToNewInvoice(request);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (request.MedProductType == AllLookupValues.MedProductType.TIEN_GIUONG)
                                                            {
                                                                if (request != null && request.RecordState == RecordState.DELETED)
                                                                {
                                                                    EditingBillingInvoice.RegistrationDetails.Remove(request);
                                                                }
                                                                if (EditingBillingInvoice.RegistrationDetails.Where(x => x.BedPatientID != request.BedPatientID).ToList().Count == 0)
                                                                {
                                                                    request.RecordState = RecordState.DETACHED;
                                                                    checkAdd = true;
                                                                    EditingBillingInvoice.RegistrationDetails.Add(request);
                                                                    AddMedRegItemBaseToNewInvoice(request);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                ////KMx: Chỉ load những CLS có ngày sử dụng <= Đến ngày (03/01/2015 10:57).
                                                //HPT 13/09/2016: Chỗ này sửa lại để load được phiếu chỉ định cận lâm sàng lên. Code kiểm tra trùng mã phiếu chỉ định làm giống như khi load toa thuốc (bên dưới)
                                                if (inv.PclRequests != null && inv.PclRequests.Count > 0)
                                                {
                                                    ObservableCollection<PatientPCLRequest> FilterItem = new ObservableCollection<PatientPCLRequest>();
                                                    foreach (PatientPCLRequest request in inv.PclRequests)
                                                    {
                                                        //▼====: #007
                                                        //if (request.CreatedDate.Date > billToDate.GetValueOrDefault().Date)
                                                        //{
                                                        //    continue;
                                                        //}
                                                        //▲====: #007
                                                        if (request.InPatientBillingInvID != null && request.InPatientBillingInvID > 0)
                                                        {
                                                            continue;
                                                        }
                                                        if (EditingBillingInvoice == null)
                                                        {
                                                            EditingBillingInvoice = new InPatientBillingInvoice();
                                                        }
                                                        if (EditingBillingInvoice.PclRequests == null)
                                                        {
                                                            EditingBillingInvoice.PclRequests = new ObservableCollection<PatientPCLRequest>();
                                                        }
                                                        PatientPCLRequest tempRequest = EditingBillingInvoice.PclRequests.FirstOrDefault(x => x.PatientPCLReqID == request.PatientPCLReqID);
                                                        if (tempRequest == null || (tempRequest != null && tempRequest.RecordState == RecordState.DELETED))
                                                        {
                                                            if (tempRequest != null && tempRequest.RecordState == RecordState.DELETED)
                                                            {
                                                                EditingBillingInvoice.PclRequests.Remove(tempRequest);
                                                            }
                                                            request.RecordState = RecordState.DETACHED;
                                                            //▼====: #008
                                                            checkAdd = true;
                                                            //▲====: #008
                                                            EditingBillingInvoice.PclRequests.Add(request);
                                                            foreach (MedRegItemBase item in request.PatientPCLRequestIndicators)
                                                            {
                                                                //▼====: #025
                                                                if (CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.IsTreatmentCOVID
                                                                    && (item as PatientPCLRequestDetail).PCLExamType.InCategoryCOVID)
                                                                {
                                                                    item.IsCountPatientCOVID = true;
                                                                }
                                                                //▲====: #025
                                                                GlobalsNAV.CalcInvoiceItem(item, IsHighTechServiceBill, CurRegistration);
                                                                AddMedRegItemBaseToNewInvoice(item);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (inv.OutwardDrugClinicDeptInvoices != null && inv.OutwardDrugClinicDeptInvoices.Count > 0)
                                                {
                                                    foreach (OutwardDrugClinicDeptInvoice outwardInvoice in inv.OutwardDrugClinicDeptInvoices)
                                                    {
                                                        if (outwardInvoice.OutwardDrugClinicDepts == null || outwardInvoice.OutwardDrugClinicDepts.Count <= 0)
                                                        {
                                                            continue;
                                                        }

                                                        if (EditingBillingInvoice == null)
                                                        {
                                                            EditingBillingInvoice = new InPatientBillingInvoice();
                                                        }

                                                        if (EditingBillingInvoice.OutwardDrugClinicDeptInvoices == null)
                                                        {
                                                            EditingBillingInvoice.OutwardDrugClinicDeptInvoices = new ObservableCollection<OutwardDrugClinicDeptInvoice>();
                                                        }

                                                        OutwardDrugClinicDeptInvoice outwardInvoiceInBill = EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Where(x => x.outiID == outwardInvoice.outiID).FirstOrDefault();

                                                        //KMx: Nếu phiếu xuất chưa có trong bill, hoặc nếu đã có rồi và đã bị delete thì phiếu xuất đó sẽ được add vào bill (31/12/2015 12:09).
                                                        if (outwardInvoiceInBill == null || (outwardInvoiceInBill != null && outwardInvoiceInBill.RecordState == RecordState.DELETED))
                                                        {
                                                            if (outwardInvoiceInBill != null && outwardInvoiceInBill.RecordState == RecordState.DELETED)
                                                            {
                                                                EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Remove(outwardInvoiceInBill);
                                                            }

                                                            //KMx: Để RecordState = DETACHED để khi UPDATE bill, phiếu xuất này được add vào List New đem đi lưu, nếu không thì phiếu xuất sẽ không được lưu vào bill (02/01/2016 10:06).
                                                            outwardInvoice.RecordState = RecordState.DETACHED;
                                                            //▼====: #008
                                                            checkAdd = true;
                                                            //▲====: #008
                                                            EditingBillingInvoice.OutwardDrugClinicDeptInvoices.Add(outwardInvoice);

                                                            //KMx: Add item vào view.
                                                            //Nếu là bill DVKTC thì mặc định chọn "Trong Gói" (05/01/2016 10:38).
                                                            if (IsHighTechServiceBill)
                                                            {
                                                                foreach (MedRegItemBase item in outwardInvoice.OutwardDrugClinicDepts)
                                                                {
                                                                    //▼====: #025
                                                                    if (CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.IsTreatmentCOVID
                                                                        && (item as OutwardDrugClinicDept).GenMedProductItem.InCategoryCOVID)
                                                                    {
                                                                        item.IsCountPatientCOVID = true;
                                                                    }
                                                                    //▲====: #025
                                                                    GlobalsNAV.CalcInvoiceItem(item, IsHighTechServiceBill, CurRegistration);
                                                                    AddMedRegItemBaseToNewInvoice(item);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                foreach (MedRegItemBase item in outwardInvoice.OutwardDrugClinicDepts)
                                                                {
                                                                    //▼====: #025
                                                                    if (CurRegistration.AdmissionInfo != null && CurRegistration.AdmissionInfo.IsTreatmentCOVID
                                                                        && (item as OutwardDrugClinicDept).GenMedProductItem.InCategoryCOVID)
                                                                    {
                                                                        item.IsCountPatientCOVID = true;
                                                                    }
                                                                    //▲====: #025
                                                                    AddMedRegItemBaseToNewInvoice(item);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                RegistrationInfoHasChanged = true;
                                                IsBillOfUsedItems = true;
                                                IsLoadNoBill = true;
                                                //▼====: #008
                                                if (!checkAdd)
                                                {
                                                    CheckValidDate();
                                                }
                                                //▲====: #008
                                            }
                                        }
                                    }
                                    catch (FaultException<AxException> fault)
                                    {
                                        error = new AxErrorEventArgs(fault);
                                    }
                                    catch (Exception ex)
                                    {
                                        error = new AxErrorEventArgs(ex);
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
                        error = new AxErrorEventArgs(ex);
                        this.HideBusyIndicator();
                    }
                    if (error != null)
                    {
                        Globals.EventAggregator.Publish(new ErrorOccurred() { CurrentError = error });
                    }
                });

                t.Start();
            }
        }

        public void SupportNormalBill()
        {
            if (CurRegistration == null || CurRegistration.PtRegistrationID <= 0)
            {
                return;
            }
            void onInitDlg(ICharitySupportFund vm)
            {
                vm.PtRegistrationID = CurRegistration.PtRegistrationID;
                vm.IsHighTechServiceBill = IsHighTechServiceBill;
                vm.BillingInvoiceListingContent = OldBillingInvoiceContent;
                vm.GetAllCharityOrganization();
                vm.GetCharitySupportFunds();
            }
            GlobalsNAV.ShowDialog<ICharitySupportFund>(onInitDlg);
        }

        //▼====: #016
        public void SetEkip()
        {
            if (EditingInvoiceDetailsContent.BillingInvoice == null || EditingInvoiceDetailsContent.BillingInvoice.RegistrationDetails == null)
            {
                return;
            }

            //20200509 TBL: Không cần phải lưu trước rồi mới thiết lập ekip
            //ObservableCollection<PatientRegistrationDetail> ObsPatientRegistrationDetail = new ObservableCollection<PatientRegistrationDetail>(EditingInvoiceDetailsContent.BillingInvoice.RegistrationDetails.Where(x => x.V_ExamRegStatus != (long)AllLookupValues.ExamRegStatus.NGUNG_TRA_TIEN_LAI
            //                                && x.RefMedicalServiceItem.RefMedicalServiceType.V_RefMedicalServiceTypes == (long)AllLookupValues.V_RefMedicalServiceTypes.THUTHUAT && x.PtRegDetailID > 0
            //                                && x.RefMedicalServiceItem.HIAllowedPrice > 0 && x.IsCountHI));

            ObservableCollection<PatientRegistrationDetail> ObsPatientRegistrationDetail = new ObservableCollection<PatientRegistrationDetail>(EditingInvoiceDetailsContent.BillingInvoice.RegistrationDetails.Where(x => x.V_ExamRegStatus != (long)AllLookupValues.ExamRegStatus.NGUNG_TRA_TIEN_LAI
                                            && x.RefMedicalServiceItem.RefMedicalServiceType.V_RefMedicalServiceTypes == (long)AllLookupValues.V_RefMedicalServiceTypes.THUTHUAT
                                            && x.RecordState != RecordState.DELETED && x.RefMedicalServiceItem.HIAllowedPrice > 0 && x.IsCountHI));
            ObservableCollection<PatientRegistrationDetail> ObsPatientRegistrationDetailCopy = new ObservableCollection<PatientRegistrationDetail>(EditingInvoiceDetailsContent.BillingInvoice.RegistrationDetails).DeepCopy();
            if (ObsPatientRegistrationDetail.Count <= 0)
            {
                MessageBox.Show(eHCMSResources.Z2733_G1_KhongCoDVThietLapEkip, eHCMSResources.G0442_G1_TBao);
                return;
            }

            ISetEkipForMedicalService aView = Globals.GetViewModel<ISetEkipForMedicalService>();
            aView.CurrentRegistration = new PatientRegistration();
            aView.CurrentRegistration.V_RegistrationType = CurRegistration.V_RegistrationType;
            aView.CurrentRegistration.PtRegistrationID = CurRegistration.PtRegistrationID;
            aView.CurrentRegistration.PatientRegistrationDetails = ObsPatientRegistrationDetail;
            GlobalsNAV.ShowDialog_V3<ISetEkipForMedicalService>(aView, null, null, false, false);
            if (!aView.SaveOK) //20200509 TBL: Nếu không lưu thì trả về như trước lúc thiết lập
            {
                //foreach (PatientRegistrationDetail item in aView.CurrentRegistration.PatientRegistrationDetails)
                //{
                //    item.V_Ekip = null;
                //    item.V_EkipIndex = null;
                //    item.HIPaymentPercent = 1;
                //    item.TotalHIPayment = item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)item.HIBenefit * item.Qty;
                //    if (item.IsDiscounted)
                //    {
                //        CurRegistration.ApplyDiscount(item, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
                //    }
                //    item.TotalPatientPayment = item.TotalInvoicePrice - item.TotalHIPayment - item.DiscountAmt;
                //}
                EditingInvoiceDetailsContent.BillingInvoice.RegistrationDetails = ObsPatientRegistrationDetailCopy;
                EditingInvoiceDetailsContent.ResetView();
            }
            else //20200509 TBL: Nếu lưu những ekip đã thiệt lập thì tính lại HIPaymentPercent, TotalHIPayment, TotalPatientPayment
            {
                foreach (PatientRegistrationDetail item in aView.CurrentRegistration.PatientRegistrationDetails)
                {
                    if (item.V_EkipIndex != null && item.V_EkipIndex.LookupID == (long)AllLookupValues.V_EkipIndex.CungEkip)
                    {
                        item.TotalHIPayment = item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)item.HIBenefit * item.Qty; //20200512 TBL: Tính lại BH trả để khi thêm ekip 2 lần tính sai giá BH trả
                        item.HIPaymentPercent = Math.Round(Globals.ServerConfigSection.HealthInsurances.PercentForEkip, 2);
                        item.TotalHIPayment = item.TotalHIPayment * (decimal)item.HIPaymentPercent;
                        if (item.IsDiscounted)
                        {
                            CurRegistration.ApplyDiscount(item, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
                        }
                        //▼====: #023
                        item.TotalPatientPayment = item.TotalInvoicePrice - item.TotalHIPayment - item.DiscountAmt - item.OtherAmt;
                        //▲====: #023
                    }
                    else if (item.V_EkipIndex != null && item.V_EkipIndex.LookupID == (long)AllLookupValues.V_EkipIndex.KhacEkip)
                    {
                        item.TotalHIPayment = item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)item.HIBenefit * item.Qty; //20200512 TBL: Tính lại BH trả để khi thêm ekip 2 lần tính sai giá BH trả
                        item.HIPaymentPercent = Math.Round(Globals.ServerConfigSection.HealthInsurances.PercentForOtherEkip, 2);
                        item.TotalHIPayment = item.TotalHIPayment * (decimal)item.HIPaymentPercent;
                        if (item.IsDiscounted)
                        {
                            CurRegistration.ApplyDiscount(item, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
                        }
                        //▼====: #023
                        item.TotalPatientPayment = item.TotalInvoicePrice - item.TotalHIPayment - item.DiscountAmt - item.OtherAmt;
                        //▲====: #023
                    }
                    else
                    {
                        item.HIPaymentPercent = 1;
                        item.TotalHIPayment = item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)item.HIBenefit * item.Qty;
                        if (item.IsDiscounted)
                        {
                            CurRegistration.ApplyDiscount(item, Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, false);
                        }
                        //▼====: #023
                        item.TotalPatientPayment = item.TotalInvoicePrice - item.TotalHIPayment - item.DiscountAmt - item.OtherAmt;
                        //▲====: #023
                    }
                    (item as MedRegItemBase).RecordState = RecordState.MODIFIED;
                }
            }
        }
        //▲====: #016

        public void AddRegPackCmd()
        {
            ISearchMedicalServiceGroups SearchView = Globals.GetViewModel<ISearchMedicalServiceGroups>();
            SearchView.V_RegistrationType = (long)AllLookupValues.RegistrationType.NOI_TRU;
            SearchView.ApplySearchContent(MedicalServiceGroupCollection, "", null);
            GlobalsNAV.ShowDialog_V3(SearchView, null, null, false, true, Globals.GetTwoFourthWidthDefaultDialogViewSize());
            if (SearchView.SelectedRefMedicalServiceGroup != null)
            {
                ApplyRefMedicalServiceGroup(SearchView.SelectedRefMedicalServiceGroup, Globals.DeptLocation);
            }
        }

        public void RemoveItemCmd(MedRegItemBase item, object eventArgs)
        {
            Coroutine.BeginExecute(CoroutineRemoveItem(item, eventArgs));
        }

        public void btnDiscount()
        {
            if (CurRegistration == null)
            {
                CurRegistration = new PatientRegistration();
            }
            IPromoDiscountProgramCollection DiscountProgramCollectionView = Globals.GetViewModel<IPromoDiscountProgramCollection>();
            DiscountProgramCollectionView.PtRegistrationID = CurRegistration.PtRegistrationID;
            DiscountProgramCollectionView.V_RegistrationType = (long)AllLookupValues.RegistrationType.NOI_TRU;
            if (CurRegistration.DiscountProgramCollection == null)
            {
                CurRegistration.DiscountProgramCollection = new List<PromoDiscountProgram>();
            }
            DiscountProgramCollectionView.DiscountProgramCollection = new ObservableCollection<PromoDiscountProgram>(CurRegistration.DiscountProgramCollection);
            GlobalsNAV.ShowDialog_V3(DiscountProgramCollectionView, null, null, false, true, new Size(1000, 600));
            if (DiscountProgramCollectionView.IsUpdated)
            {
                CurRegistration.DiscountProgramCollection = new List<PromoDiscountProgram>(DiscountProgramCollectionView.DiscountProgramCollection);
            }
            if (DiscountProgramCollectionView.IsChoosed && DiscountProgramCollectionView.SelectedPromoDiscountProgram != null && CurRegistration.DiscountProgramCollection != null && CurRegistration.DiscountProgramCollection.Count > 0)
            {
                CurRegistration.PromoDiscountProgramObj = CurRegistration.DiscountProgramCollection.FirstOrDefault(x => x.PromoDiscProgID == DiscountProgramCollectionView.SelectedPromoDiscountProgram.PromoDiscProgID);
                EditingInvoiceDetailsContent.NotifyOfCanApplyIsOnPriceDiscount();
                CommonGlobals.CorrectRegistrationDetails(CurRegistration);
            }
        }

        int index = 0;
        public void gridRegDetails_DbClick(object source, EventArgs<object> eventArgs)
        {
            if (source == null)
            {
                return;
            }

            if (((DataGrid)source).CurrentColumn == null) return;
            if (((DataGrid)source).CurrentColumn != ((DataGrid)source).GetColumnByName("colInvoicePrice")
                && ((DataGrid)source).CurrentColumn != ((DataGrid)source).GetColumnByName("colHIAllowedPrice"))
            {
                return;
            }

            ModifyBillingInvItem = eventArgs.Value as MedRegItemBase;

            index = AllRegistrationItems.IndexOf(ModifyBillingInvItem);

            if (ModifyBillingInvItem.V_NewPriceType != (Int32)AllLookupValues.V_NewPriceType.Unknown_PriceType
                && ModifyBillingInvItem.V_NewPriceType != (Int32)AllLookupValues.V_NewPriceType.Updatable_PriceType)
            {
                MessageBox.Show(ModifyBillingInvItem.ChargeableItemName + string.Format(" {0}", eHCMSResources.W0950_G1_W));
                return;
            }

            ModifyBillingInvItem.IsModItemOK = false;

            void onInitDlg(IModifyBillingInvItem vm)
            {
                vm.SetParentInPtBillingInvDetailsLst(this);
                vm.ModifyBillingInvItem = ModifyBillingInvItem;
                vm.PopupType = 2;
                vm.Init();
            }
            GlobalsNAV.ShowDialog<IModifyBillingInvItem>(onInitDlg);
        }

        #endregion

        public void ApplyRefMedicalServiceGroup(RefMedicalServiceGroups aRefMedicalServiceGroupObj, DeptLocation aDeptLocation)
        {
            RefMedicalServiceGroupObj = aRefMedicalServiceGroupObj;
            this.ShowBusyIndicator(eHCMSResources.Z0125_G1_DangXuLi);
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new CommonService_V2Client())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginGetRefMedicalServiceGroupItemsByID(RefMedicalServiceGroupObj.MedicalServiceGroupID,
                            Globals.DispatchCallback((asyncResult) =>
                            {
                                try
                                {
                                    var mItemCollection = contract.EndGetRefMedicalServiceGroupItemsByID(asyncResult);
                                    if (mItemCollection != null)
                                    {
                                        RefMedicalServiceGroupObj.RefMedicalServiceGroupItems = mItemCollection.ToList();
                                        foreach (var item in RefMedicalServiceGroupObj.RefMedicalServiceGroupItems.Where(x => x.MedServiceID.HasValue && x.MedServiceID > 0))
                                        {
                                            PopupModifyPrice_Type = AllLookupValues.PopupModifyPrice_Type.INSERT_DICHVU;
                                            InPatientSelectServiceContent.MedServiceItem = item.RefMedicalServiceItemObj;
                                            ServiceQty = item.Qty;
                                            ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults;
                                            if (!ValidateServiceItem(out validationResults))
                                            {
                                                Globals.EventAggregator.Publish(new ValidateFailedEvent() { ValidationResults = validationResults });
                                                return;
                                            }
                                            AddGenMedService(InPatientSelectServiceContent.MedServiceItem, ServiceQty.Value, (DateTime)UseAtDateContent.DateTime);
                                        }
                                        foreach (var item in RefMedicalServiceGroupObj.RefMedicalServiceGroupItems.Where(x => x.PCLExamTypeID.HasValue && x.PCLExamTypeID > 0))
                                        {
                                            bool used = true;
                                            if (Globals.ListPclExamTypesAllPCLFormImages.Any(x => x.PCLExamTypeID == item.PCLExamTypeID))
                                            {
                                                PopupModifyPrice_Type = AllLookupValues.PopupModifyPrice_Type.INSERT_PCL_HINHANH;
                                                PclQty = item.Qty;
                                                SelectPCLContent.SelectedPCLExamType = Globals.ListPclExamTypesAllPCLFormImages.Where(x => x.PCLExamTypeID == item.PCLExamTypeID).FirstOrDefault();
                                                ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults;
                                                if (!ValidatePclItem(SelectPCLContent, out validationResults))
                                                {
                                                    Globals.EventAggregator.Publish(new ValidateFailedEvent() { ValidationResults = validationResults });
                                                    return;
                                                }
                                                CheckAndAddPCL(SelectPCLContent.SelectedPCLExamType, PclQty.Value, (DateTime)UseAtDateContent.DateTime, used);
                                            }
                                            if (Globals.ListPclExamTypesAllPCLForms.Any(x => x.PCLExamTypeID == item.PCLExamTypeID))
                                            {
                                                PopupModifyPrice_Type = AllLookupValues.PopupModifyPrice_Type.INSERT_PCL_XETNGHIEM;
                                                PclQtyLAB = item.Qty;
                                                SelectPCLContentLAB.SelectedPCLExamType = Globals.ListPclExamTypesAllPCLForms.Where(x => x.PCLExamTypeID == item.PCLExamTypeID).FirstOrDefault();
                                                ObservableCollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults;
                                                if (!ValidatePclItemLAB(true, out validationResults))
                                                {
                                                    Globals.EventAggregator.Publish(new ValidateFailedEvent() { ValidationResults = validationResults });
                                                    return;
                                                }
                                                CheckAndAddPCL(SelectPCLContentLAB.SelectedPCLExamType, PclQtyLAB.Value, (DateTime)UseAtDateContent.DateTime, used);
                                            }
                                        }
                                    }
                                    //InitViewForServiceItems();
                                    //InitViewForPCLRequests();
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

        private void LoadMedicalServiceGroupCollection()
        {
            this.ShowBusyIndicator();
            var t = new Thread(() =>
            {
                try
                {
                    using (var serviceFactory = new CommonService_V2Client())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginGetRefMedicalServiceGroups("",
                            Globals.DispatchCallback((asyncResult) =>
                            {
                                try
                                {
                                    MedicalServiceGroupCollection = new List<RefMedicalServiceGroups>();
                                    var mItemCollection = contract.EndGetRefMedicalServiceGroups(asyncResult);
                                    if (mItemCollection != null)
                                    {
                                        MedicalServiceGroupCollection = mItemCollection.ToList();
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
        //▲====: #024

        DataGrid grid = null;
        public void grid_Loaded(object sender, RoutedEventArgs e)
        {
            grid = sender as DataGrid;

            if (grid == null)
            {
                return;
            }

            //var colIsInPackage = grid.GetColumnByName("colIsInPackage");
            var colIsInPackage = grid.Columns.FirstOrDefault(x => x.Header != null && x.Header.ToString().Equals("colIsInPackage"));

            if (colIsInPackage == null)
            {
                return;
            }

            //if (ShowInPackageColumn || (BillingInvoice != null && BillingInvoice.IsHighTechServiceBill)) //20191213 TBL: Nếu xem chi tiết bill là bill KTC thì phải hiển thị cột Trong gói
            //{
            //    colIsInPackage.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    colIsInPackage.Visibility = Visibility.Collapsed;
            //}
        }

        private void CalcInvoiceItem(MedRegItemBase item, bool IsCellEditEnding = false, bool IsFromCountHI = false, bool IsFromCountPatientCovid = false)
        //▲====== #007
        {
            //▼====: #016
            bool IsCountPatientCOVID = false;
            if (CurRegistration.AdmissionInfo != null)
            {
                IsCountPatientCOVID = CurRegistration.AdmissionInfo.IsTreatmentCOVID;
            }
            // =====▼ #005
            item.HIAllowedPrice = item.ChargeableItem.HIAllowedPrice.GetValueOrDefault();
            //▼====== #007
            if (!IsCellEditEnding)
            {
                //▼===== #011: Vì lý do hàm này xài chung giữa thuốc, dịch vụ, CLS nên phải tách ra nếu là dịch vụ thì nhân thêm tỷ lệ thanh toán dịch vụ (nếu có).
                if (item is PatientRegistrationDetail)
                {
                    // nếu từ chỗ tích chọn/ bỏ chọn covid thì chạy thêm phần này. k thì chạy code cũ
                    if (IsFromCountPatientCovid)
                    {
                        if (item.IsCountPatientCOVID)
                        {
                            if (item.ChargeableItem.HIAllowedPrice > 0)
                            {
                                item.InvoicePrice = (decimal)item.ChargeableItem.HIAllowedPrice * (decimal)(item as PatientRegistrationDetail).PaymentPercent;
                            }
                            else
                            {
                                item.InvoicePrice = item.ChargeableItem.NormalPrice * (decimal)(item as PatientRegistrationDetail).PaymentPercent;
                            }
                            item.IsCountHI = false;
                            item.OtherAmt = item.InvoicePrice * item.Qty;
                        }
                        else
                        {
                            if (item.IsCountHI && HIBenefit != null)
                            {
                                item.InvoicePrice = item.ChargeableItem.HIPatientPrice * (decimal)(item as PatientRegistrationDetail).PaymentPercent;
                            }
                            else if (HIBenefit.GetValueOrDefault(0) > 0 && item.ChargeableItem.HIPatientPrice > 0)
                            {
                                item.InvoicePrice = item.ChargeableItem.HIPatientPrice * (decimal)(item as PatientRegistrationDetail).PaymentPercent;
                                item.IsCountHI = true;
                            }
                            else
                            {
                                item.InvoicePrice = item.ChargeableItem.NormalPrice * (decimal)(item as PatientRegistrationDetail).PaymentPercent;
                            }
                            item.OtherAmt = 0;
                        }
                    }
                    // kiểm tra đăng ký được xác nhận là bn điều trị COVID thì tính cách mới không thì đi như cũ
                    else if (IsCountPatientCOVID)
                    {
                        // nếu DVKT/ thuốc nằm trong danh mục COVID thì
                        // sau 1 hồi nhìn code + vd thì cảm thấy không cần thay đổi gì hết. giữ nguyên
                        if (item.IsCountPatientCOVID)
                        {
                            if (item.ChargeableItem.HIAllowedPrice > 0)
                            {
                                item.InvoicePrice = (decimal)item.ChargeableItem.HIAllowedPrice * (decimal)(item as PatientRegistrationDetail).PaymentPercent;
                            }
                            else
                            {
                                item.InvoicePrice = item.ChargeableItem.NormalPrice * (decimal)(item as PatientRegistrationDetail).PaymentPercent;
                            }
                            item.IsCountHI = false;
                            item.OtherAmt = item.InvoicePrice * item.Qty;
                        }
                        else
                        {
                            if (item.IsCountHI && HIBenefit != null)
                            {
                                item.InvoicePrice = item.ChargeableItem.HIPatientPrice * (decimal)(item as PatientRegistrationDetail).PaymentPercent;
                            }
                            else if (HIBenefit.GetValueOrDefault(0) > 0 && item.ChargeableItem.HIPatientPrice > 0)
                            {
                                item.InvoicePrice = item.ChargeableItem.HIPatientPrice * (decimal)(item as PatientRegistrationDetail).PaymentPercent;
                            }
                            else
                            {
                                item.InvoicePrice = item.ChargeableItem.NormalPrice * (decimal)(item as PatientRegistrationDetail).PaymentPercent;
                            }
                        }
                    }
                    else
                    {
                        if (item.IsCountHI && HIBenefit != null)
                        {
                            item.InvoicePrice = item.ChargeableItem.HIPatientPrice * (decimal)(item as PatientRegistrationDetail).PaymentPercent;
                        }
                        else if (HIBenefit.GetValueOrDefault(0) > 0 && item.ChargeableItem.HIPatientPrice > 0)
                        {
                            item.InvoicePrice = item.ChargeableItem.HIPatientPrice * (decimal)(item as PatientRegistrationDetail).PaymentPercent;
                        }
                        else
                        {
                            item.InvoicePrice = item.ChargeableItem.NormalPrice * (decimal)(item as PatientRegistrationDetail).PaymentPercent;
                        }
                    }
                }
                else if (item is PatientPCLRequestDetail)
                {
                    // nếu từ chỗ tích chọn/ bỏ chọn covid thì chạy thêm phần này. k thì chạy code cũ
                    if (IsFromCountPatientCovid)
                    {
                        if (item.IsCountPatientCOVID)
                        {
                            if (item.ChargeableItem.HIAllowedPrice > 0)
                            {
                                item.InvoicePrice = (decimal)item.ChargeableItem.HIAllowedPrice;
                            }
                            else
                            {
                                item.InvoicePrice = item.ChargeableItem.NormalPrice;
                            }
                            item.IsCountHI = false;
                            item.IsCountPatientCOVID = true;
                            item.OtherAmt = item.InvoicePrice * item.Qty;
                        }
                        else
                        {
                            if (item.IsCountHI && HIBenefit != null)
                            {
                                item.InvoicePrice = item.ChargeableItem.HIPatientPrice;
                            }
                            else if (HIBenefit.GetValueOrDefault(0) > 0 && item.ChargeableItem.HIPatientPrice > 0)
                            {
                                item.InvoicePrice = item.ChargeableItem.HIPatientPrice;
                            }
                            else
                            {
                                item.InvoicePrice = item.ChargeableItem.NormalPrice;
                            }
                            item.IsCountPatientCOVID = false;
                            item.OtherAmt = 0;
                        }
                    }
                    // kiểm tra đăng ký được xác nhận là bn điều trị COVID thì tính cách mới không thì đi như cũ
                    else if (IsCountPatientCOVID)
                    {
                        if (item.IsCountPatientCOVID)
                        {
                            if (item.ChargeableItem.HIAllowedPrice > 0)
                            {
                                item.InvoicePrice = (decimal)item.ChargeableItem.HIAllowedPrice;
                            }
                            else
                            {
                                item.InvoicePrice = item.ChargeableItem.NormalPrice;
                            }
                            item.IsCountHI = false;
                            item.OtherAmt = item.InvoicePrice * item.Qty;
                        }
                        else
                        {
                            item.OtherAmt = 0;
                            item.InvoicePrice = item.IsCountHI && HIBenefit != null ? item.ChargeableItem.HIPatientPrice : (HIBenefit.GetValueOrDefault(0) > 0 && item.ChargeableItem.HIPatientPrice > 0 ? item.ChargeableItem.HIPatientPrice : item.ChargeableItem.NormalPrice);
                        }
                    }
                    else
                    {
                        item.OtherAmt = 0;
                        item.InvoicePrice = item.IsCountHI && HIBenefit != null ? item.ChargeableItem.HIPatientPrice : (HIBenefit.GetValueOrDefault(0) > 0 && item.ChargeableItem.HIPatientPrice > 0 ? item.ChargeableItem.HIPatientPrice : item.ChargeableItem.NormalPrice);
                    }

                }
                else if (item is OutwardDrugClinicDept)
                {
                    // nếu từ chỗ tích chọn/ bỏ chọn covid thì chạy thêm phần này. k thì chạy code cũ
                    if (IsFromCountPatientCovid)
                    {
                        if (item.IsCountPatientCOVID)
                        {
                            item.InvoicePrice = ((RefGenMedProductDetails)item.ChargeableItem).InCost;
                            item.IsCountHI = false;
                            item.IsCountPatientCOVID = true;
                            item.OtherAmt = item.InvoicePrice * item.Qty;
                        }
                        else
                        {
                            if (item.IsCountHI && HIBenefit != null)
                            {
                                item.InvoicePrice = item.ChargeableItem.HIPatientPrice;
                            }
                            else if (HIBenefit.GetValueOrDefault(0) > 0 && item.ChargeableItem.HIPatientPrice > 0)
                            {
                                item.InvoicePrice = item.ChargeableItem.HIPatientPrice;
                            }
                            else
                            {
                                item.InvoicePrice = item.ChargeableItem.NormalPrice;
                            }
                            item.OtherAmt = 0;
                            item.IsCountPatientCOVID = false;
                        }
                    }
                    else if (IsCountPatientCOVID)
                    {
                        if (item.IsCountPatientCOVID)
                        {
                            //▼====: #018
                            item.InvoicePrice = ((RefGenMedProductDetails)item.ChargeableItem).InCost;
                            //▲====: #018
                            item.IsCountHI = false;
                            item.OtherAmt = item.InvoicePrice * item.Qty;
                        }
                        else
                        {
                            item.OtherAmt = 0;
                            item.InvoicePrice = item.IsCountHI && HIBenefit != null ? item.ChargeableItem.HIPatientPrice : (HIBenefit.GetValueOrDefault(0) > 0 && item.ChargeableItem.HIPatientPrice > 0 ? item.ChargeableItem.HIPatientPrice : item.ChargeableItem.NormalPrice);
                        }
                    }
                    else
                    {
                        item.OtherAmt = 0;
                        item.InvoicePrice = item.IsCountHI && HIBenefit != null ? item.ChargeableItem.HIPatientPrice : (HIBenefit.GetValueOrDefault(0) > 0 && item.ChargeableItem.HIPatientPrice > 0 ? item.ChargeableItem.HIPatientPrice : item.ChargeableItem.NormalPrice);
                    }
                }
                else
                {
                    item.OtherAmt = 0;
                    item.InvoicePrice = item.IsCountHI && HIBenefit != null ? item.ChargeableItem.HIPatientPrice : (HIBenefit.GetValueOrDefault(0) > 0 && item.ChargeableItem.HIPatientPrice > 0 ? item.ChargeableItem.HIPatientPrice : item.ChargeableItem.NormalPrice);
                }
                //▲===== #011
            }
            //▲====== #007
            //▲====: #016
            item.PriceDifference = item.InvoicePrice - item.HIAllowedPrice.GetValueOrDefault(0);
            if (item.IsCountPatientCOVID)
            {
                item.OtherAmt = item.InvoicePrice * item.Qty;
            }
            bool bItemInstDateIsValidWithHICard_1 = false;
            bool bItemInstDateIsValidWithHICard_2 = false;
            bool bItemInstDateIsValidWithHICard_3 = false;
            if (CurRegistration.HealthInsurance != null && CurRegistration.PtInsuranceBenefit > 0 && (item.MedicalInstructionDate >= CurRegistration.HealthInsurance.ValidDateFrom && item.MedicalInstructionDate <= CurRegistration.HealthInsurance.ValidDateTo))
            {
                bItemInstDateIsValidWithHICard_1 = true;
            }
            else
            {
                if (CurRegistration.HealthInsurance_3 != null && item.MedicalInstructionDate >= CurRegistration.HealthInsurance_3.ValidDateFrom && item.MedicalInstructionDate <= CurRegistration.HealthInsurance_3.ValidDateTo)
                {
                    bItemInstDateIsValidWithHICard_3 = true;
                }
                else if (CurRegistration.HealthInsurance_2 != null && item.MedicalInstructionDate >= CurRegistration.HealthInsurance_2.ValidDateFrom && item.MedicalInstructionDate <= CurRegistration.HealthInsurance_2.ValidDateTo)

                {
                    bItemInstDateIsValidWithHICard_2 = true;
                }
            }

            //if (!onlyRoundResultForOutward)
            //{
            //    item.TotalHIPayment = MathExt.Round(item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)HIBenefit.GetValueOrDefault(0.0) * item.Qty, MidpointRounding.AwayFromZero);
            //}
            //else
            //{
            //    item.TotalHIPayment = item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)HIBenefit.GetValueOrDefault(0.0) * item.Qty;
            //}

            /*▼====: #003*/
            if (item.IsCountHI && bItemInstDateIsValidWithHICard_1)
            {
                //item.TotalHIPayment = item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)CurRegistration.PtInsuranceBenefit.GetValueOrDefault(0.0) * item.Qty;
                //item.HIBenefit = CurRegistration.PtInsuranceBenefit.GetValueOrDefault(0);
                //item.HisID = CurRegistration.HisID;
                CalcTotalHIPaymentWithInsuranceBenefit(item, CurRegistration.PtInsuranceBenefit, CurRegistration.HisID);
            }
            else if (item.IsCountHI && bItemInstDateIsValidWithHICard_3)
            {
                //item.TotalHIPayment = item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)CurRegistration.PtInsuranceBenefit_3.GetValueOrDefault(0.0) * item.Qty;
                //item.HIBenefit = CurRegistration.PtInsuranceBenefit_3.GetValueOrDefault(0.0);
                //item.HisID = CurRegistration.HisID_3;
                CalcTotalHIPaymentWithInsuranceBenefit(item, CurRegistration.PtInsuranceBenefit_3, CurRegistration.HisID_3);
            }
            else if (item.IsCountHI && bItemInstDateIsValidWithHICard_2)
            {
                //item.TotalHIPayment = item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)CurRegistration.PtInsuranceBenefit_2.GetValueOrDefault(0.0) * item.Qty;
                //item.HIBenefit = CurRegistration.PtInsuranceBenefit_2.GetValueOrDefault(0.0);
                //item.HisID = CurRegistration.HisID_2;
                CalcTotalHIPaymentWithInsuranceBenefit(item, CurRegistration.PtInsuranceBenefit_2, CurRegistration.HisID_2);
            }
            else
            {
                item.HisID = null;
                item.HIBenefit = null;
                // =====▼ #005
                item.IsCountHI = false;
                item.TotalHIPayment = 0;
                // =====▲ #005
            }

            //▼=====20191219 TTM: Đổi code anh Công lại thành code của hàm RecalForLimProduct
            /*==== #002 ====*/
            if (Globals.ServerConfigSection.InRegisElements.NotCountHIOnPackItem && HIBenefit != null && item.IsCountHI && item.IsInPackage)
            {
                item.IsCountHI = false;
                item.TotalHIPayment = 0;
            }
            if (!IsNewCreateBill)
            {
                if (item.MaxQtyHIAllowItem > 1 && item.IsCountHI == true && item is OutwardDrugClinicDept)
                {
                    var ModifyItem = (item as OutwardDrugClinicDept);
                    var AddedItem = AllRegistrationItems.Where(x => x.ID != item.ID && x.IsCountHI && x is OutwardDrugClinicDept && (x as OutwardDrugClinicDept).MaxQtyHIAllowItem > 1);
                    if (AddedItem.Count() >= item.MaxQtyHIAllowItem)
                    {
                        ModifyItem.IsCountHI = false;
                        ModifyItem.TotalHIPayment = 0;
                        ModifyItem.HIPaymentPercent = 0;
                    }
                    else if (AddedItem.Count() > 1 && AddedItem.Count() < item.MaxQtyHIAllowItem)
                    {
                        if (AddedItem.Where(x => (x as OutwardDrugClinicDept).HIPaymentPercent == 1).Count() == 1)
                        {
                            ModifyItem.HIPaymentPercent = item.PaymentRateOfHIAddedItem;
                            ModifyItem.TotalHIPayment = Math.Min(ModifyItem.GenMedProductItem == null ? 0 : ModifyItem.GenMedProductItem.CeilingPrice2ndItem.GetValueOrDefault(0), item.ChargeableItem.HIAllowedPrice.GetValueOrDefault(0) * (decimal)item.PaymentRateOfHIAddedItem) * (CurRegistration.IsCrossRegion.GetValueOrDefault(false) ? (decimal)Globals.ServerConfigSection.HealthInsurances.RebatePercentageLevel1 / (item.PaymentRateOfHIAddedItem != 0 ? (decimal)item.PaymentRateOfHIAddedItem.GetValueOrDefault(1) : 1.0m) : 1.0m);
                        }
                    }
                    else if (AddedItem.Count() == 1)
                    {
                        if ((AddedItem.FirstOrDefault() as OutwardDrugClinicDept).HIPaymentPercent.GetValueOrDefault(1) == 1)
                        {
                            ModifyItem.HIPaymentPercent = item.PaymentRateOfHIAddedItem;
                            ModifyItem.TotalHIPayment = Math.Min(ModifyItem.GenMedProductItem == null ? 0 : ModifyItem.GenMedProductItem.CeilingPrice2ndItem.GetValueOrDefault(0), item.ChargeableItem.HIAllowedPrice.GetValueOrDefault(0) * (decimal)item.PaymentRateOfHIAddedItem) * (CurRegistration.IsCrossRegion.GetValueOrDefault(false) ? (decimal)Globals.ServerConfigSection.HealthInsurances.RebatePercentageLevel1 / (item.PaymentRateOfHIAddedItem != 0 ? (decimal)item.PaymentRateOfHIAddedItem.GetValueOrDefault(1) : 1.0m) : 1.0m);
                        }
                    }
                    else if (AddedItem.Count() == 0 && item.Qty == 1)
                    {
                        ModifyItem.HIPaymentPercent = 1;
                        ModifyItem.TotalHIPayment = item.Qty > 1 ? (item.TotalHIPayment / item.Qty) + Math.Min(ModifyItem.GenMedProductItem == null ? 0 : ModifyItem.GenMedProductItem.CeilingPrice2ndItem.GetValueOrDefault(0), item.ChargeableItem.HIAllowedPrice.GetValueOrDefault(0) * (decimal)item.PaymentRateOfHIAddedItem) : item.TotalHIPayment;
                    }
                    item = ModifyItem;
                }
                else if (item.MaxQtyHIAllowItem > 1 && item.IsCountHI == false && item is OutwardDrugClinicDept)
                {
                    var ModifyItem = (item as OutwardDrugClinicDept);
                    ModifyItem.HIPaymentPercent = 1;
                    foreach (var RevertItem in AllRegistrationItems.Where(x => x.ID != item.ID && x.IsCountHI && x is OutwardDrugClinicDept && (x as OutwardDrugClinicDept).GenMedProductID == (item as OutwardDrugClinicDept).GenMedProductID))
                    {
                        CalcInvoiceItem(RevertItem);
                        ModifiItemCmd(RevertItem);
                    }
                    item = ModifyItem;
                }
                else if (item is OutwardDrugClinicDept)
                {
                    var ModifyItem = (item as OutwardDrugClinicDept);
                    ModifyItem.HIPaymentPercent = 1;
                    item = ModifyItem;
                }
            }
            else
            {
                if (item is OutwardDrugClinicDept && (item as OutwardDrugClinicDept).LimQtyAndHIPrice != null)
                {
                    bool btValOrderType = item.IsCountHI ? true : false;
                    RecalForLimProduct(btValOrderType, IsFromCountHI);
                }
            }
            /*==== #002 ====*/
            //▲===== 

            //KMx: Nếu dịch vụ đó đã có trong gói rồi thì chỉ tính BH thôi, còn "Tổng tiền VT thu = 0" và "Tổng tiền BN trả = 0"
            if (item.IsInPackage)
            {
                item.TotalInvoicePrice = 0;
                item.TotalPatientPayment = 0;
            }
            else
            {
                //KMx: Nếu có tính cho BN thì Tổng tiền = Đơn giá * Số lượng. Ngược lại thì Tổng tiền = Tổng tiền BH trả (11/12/2014).
                if (item.IsCountPatient)
                {
                    item.TotalInvoicePrice = item.InvoicePrice * item.Qty;
                }
                else
                {
                    item.TotalInvoicePrice = item.TotalHIPayment;
                }

                //▼====: #016
                item.TotalPatientPayment = item.TotalInvoicePrice - item.TotalHIPayment - item.OtherAmt;
                //▲====: #016
            }
            /*▼====: #003*/
            // =====▼ #005
            //if (!onlyRoundResultForOutward)
            //    {
            //    item.TotalHIPayment = MathExt.Round(item.TotalHIPayment, MidpointRounding.AwayFromZero);
            //}
            // =====▲ #005
            if (CurRegistration != null && item.IsCountHI && item.TotalHIPayment > 0 && item.HisID.GetValueOrDefault(0) == 0)
                item.HisID = CurRegistration.HisID;
            /*▲====: #003*/
        }
        //20200511 TBL: Hàm tính lại TotalHIPayment theo Benefit của thẻ theo ngày y lệnh
        private void CalcTotalHIPaymentWithInsuranceBenefit(MedRegItemBase item, double? HIBenefit, long? HisID)
        {
            if (item is PatientRegistrationDetail)
            {
                PatientRegistrationDetail PtRegDetail = item as PatientRegistrationDetail;
                if (PtRegDetail.V_EkipIndex != null && PtRegDetail.V_EkipIndex.LookupID == (long)AllLookupValues.V_EkipIndex.CungEkip)
                {
                    item.TotalHIPayment = item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)HIBenefit * item.Qty * (decimal)Globals.ServerConfigSection.HealthInsurances.PercentForEkip;
                }
                else if (PtRegDetail.V_EkipIndex != null && PtRegDetail.V_EkipIndex.LookupID == (long)AllLookupValues.V_EkipIndex.KhacEkip)
                {
                    item.TotalHIPayment = item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)HIBenefit * item.Qty * (decimal)Globals.ServerConfigSection.HealthInsurances.PercentForOtherEkip;
                }
                else
                {
                    //▼===== #011: Vì lý do hàm này xài chung giữa thuốc, dịch vụ, CLS nên phải tách ra nếu là dịch vụ thì nhân thêm tỷ lệ thanh toán dịch vụ (nếu có).
                    item.TotalHIPayment = item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)HIBenefit * item.Qty * (decimal)(item as PatientRegistrationDetail).HIPaymentPercent;
                    //▲===== #011
                }
            }
            else
            {
                item.TotalHIPayment = item.HIAllowedPrice.GetValueOrDefault(0) * (decimal)HIBenefit * item.Qty;
            }
            item.HIBenefit = HIBenefit.GetValueOrDefault(0);
            item.HisID = HisID;
        }

        public void ModifiItemCmd(MedRegItemBase item)
        {
            if (item != null)
            {
                if (item is PatientRegistrationDetail)
                {
                    var detailsItem = item as PatientRegistrationDetail;
                    foreach (var temp in AllRegistrationItems)
                    {
                        if (temp is PatientRegistrationDetail)
                        {
                            var detailsTemp = temp as PatientRegistrationDetail;
                            if (detailsTemp.PtRegDetailID == detailsItem.PtRegDetailID)
                            {
                                detailsTemp.RecordState = RecordState.MODIFIED;
                            }
                        }
                    }
                }
                else if (item is PatientPCLRequestDetail)
                {
                    var detailsItem = item as PatientPCLRequestDetail;
                    foreach (var temp in AllRegistrationItems)
                    {
                        if (temp is PatientPCLRequestDetail)
                        {
                            var detailsTemp = temp as PatientPCLRequestDetail;
                            if (detailsTemp.PCLReqItemID == detailsItem.PCLReqItemID)
                            {
                                detailsTemp.RecordState = RecordState.MODIFIED;
                            }
                        }
                    }

                }
                else
                {
                    var detailsItem = item as OutwardDrugClinicDept;
                    foreach (var temp in AllRegistrationItems)
                    {
                        if (temp is OutwardDrugClinicDept)
                        {
                            var detailsTemp = temp as OutwardDrugClinicDept;
                            if (detailsTemp.OutID == detailsItem.OutID)
                            {
                                detailsTemp.RecordState = RecordState.MODIFIED;
                            }
                        }
                    }
                }
            }
        }

        public void DeletedItemCmd(MedRegItemBase item)
        {
            if (item != null)
            {
                if (item is PatientRegistrationDetail)
                {
                    var detailsItem = item as PatientRegistrationDetail;
                    foreach (var temp in AllRegistrationItems)
                    {
                        if (temp is PatientRegistrationDetail)
                        {
                            var detailsTemp = temp as PatientRegistrationDetail;
                            if (detailsTemp.PtRegDetailID == detailsItem.PtRegDetailID)
                            {
                                detailsTemp.RecordState = RecordState.DELETED;
                            }
                        }
                    }
                }
                else if (item is PatientPCLRequestDetail)
                {
                    var detailsItem = item as PatientPCLRequestDetail;
                    foreach (var temp in AllRegistrationItems)
                    {
                        if (temp is PatientPCLRequestDetail)
                        {
                            var detailsTemp = temp as PatientPCLRequestDetail;
                            if (detailsTemp.PCLReqItemID == detailsItem.PCLReqItemID)
                            {
                                detailsTemp.RecordState = RecordState.DELETED;
                            }
                        }
                    }

                }
                else
                {
                    var detailsItem = item as OutwardDrugClinicDept;
                    foreach (var temp in AllRegistrationItems)
                    {
                        if (temp is OutwardDrugClinicDept)
                        {
                            var detailsTemp = temp as OutwardDrugClinicDept;
                            if (detailsTemp.OutID == detailsItem.OutID)
                            {
                                detailsTemp.RecordState = RecordState.DELETED;
                            }
                        }
                    }
                }
            }
        }

        public void RecalForLimProduct(bool OrderType = true, bool IsFromCountHI = false)
        {
            //20191219 TTM: Chỉ có tick bảo hiểm mới làm thay đổi số thứ tự của các y cụ có giới hạn tính giá BH.
            if (IsFromCountHI)
            {
                int ncounter = 1;
                int IsChangeSeqNum = 0;
                int checkcount = 0;
                List<MedRegItemBase> tmpListItemBase = new List<MedRegItemBase>();
                if (OrderType)
                {
                    tmpListItemBase = AllRegistrationItems.Where(x => x is OutwardDrugClinicDept).Where(x => (x as OutwardDrugClinicDept).LimQtyAndHIPrice != null).OrderBy(y => y.TickTime).ToList();
                }
                else
                {
                    tmpListItemBase = AllRegistrationItems.Where(x => x is OutwardDrugClinicDept).Where(x => (x as OutwardDrugClinicDept).LimQtyAndHIPrice != null).OrderBy(y => y.CountValue).ToList();
                }
                foreach (var detail in tmpListItemBase)
                {
                    checkcount++;
                    if (detail.IsCountHI && detail.CountValue <= 0)
                    {
                        detail.CountValue = ncounter;
                        ncounter++;
                    }
                    else
                    {
                        if (!detail.IsCountHI && detail.CountValue > 0)
                        {
                            detail.CountValue = 0;
                            detail.TickTime = 100;
                            IsChangeSeqNum = -1;
                        }
                        else if (IsChangeSeqNum < 0)
                        {
                            detail.CountValue += IsChangeSeqNum;
                        }
                    }
                    if (checkcount == ncounter)
                    {
                        ncounter++;
                    }
                }
            }

            //20191219 TTM: Dựa vào thứ tự đã xếp ở trên để tính bảo hiểm.
            var listItemToRecal = AllRegistrationItems.Where(x => x is OutwardDrugClinicDept).Where(x => (x as OutwardDrugClinicDept).LimQtyAndHIPrice != null);
            foreach (var itemdetail in listItemToRecal)
            {
                OutwardDrugClinicDept value = itemdetail as OutwardDrugClinicDept;
                if (value.CountValue == 1)
                {
                    value.HIPaymentPercent = Convert.ToDouble(value.LimQtyAndHIPrice.ItemNumber1MaxPayPerc);
                    if (value.GenMedProductID == null)
                    {
                        value.TotalHIPayment = 0;
                    }
                    else
                    {
                        decimal PtHiBenefit = (decimal)HIBenefit;
                        if (value.LimQtyAndHIPrice.ItemNumber1MaxBenefit > 0)
                        {
                            if (CurRegistration.IsCrossRegion == false)
                            {
                                PtHiBenefit = value.LimQtyAndHIPrice.ItemNumber1MaxBenefit;
                            }
                            else
                            {
                                PtHiBenefit = (decimal)Globals.ServerConfigSection.HealthInsurances.RebatePercentage2015Level1_InPt;
                            }
                        }
                        value.TotalHIPayment = Math.Min(value.LimQtyAndHIPrice.ItemNumber1MaxPayAmt, value.ChargeableItem.HIAllowedPrice.GetValueOrDefault(0) * (decimal)value.HIPaymentPercent) * PtHiBenefit;
                    }
                }
                else if (value.CountValue == 2)
                {
                    value.HIPaymentPercent = Convert.ToDouble(value.LimQtyAndHIPrice.ItemNumber2MaxPayPerc);
                    if (value.GenMedProductID == null)
                    {
                        value.TotalHIPayment = 0;
                    }
                    else
                    {
                        decimal PtHiBenefit = (decimal)HIBenefit;
                        if (value.LimQtyAndHIPrice.ItemNumber2MaxBenefit > 0)
                        {
                            if (CurRegistration.IsCrossRegion == false)
                            {
                                PtHiBenefit = value.LimQtyAndHIPrice.ItemNumber2MaxBenefit;
                            }
                            else
                            {
                                PtHiBenefit = (decimal)Globals.ServerConfigSection.HealthInsurances.RebatePercentage2015Level1_InPt;
                            }
                        }

                        value.TotalHIPayment = Math.Min(value.LimQtyAndHIPrice.ItemNumber2MaxPayAmt, value.ChargeableItem.HIAllowedPrice.GetValueOrDefault(0) * (decimal)value.HIPaymentPercent) * PtHiBenefit;
                    }
                }
                else if (value.CountValue == 3)
                {
                    value.HIPaymentPercent = Convert.ToDouble(value.LimQtyAndHIPrice.ItemNumber3MaxPayPerc);
                    if (value.GenMedProductID == null)
                    {
                        value.TotalHIPayment = 0;
                    }
                    else
                    {
                        decimal PtHiBenefit = (decimal)HIBenefit;
                        if (value.LimQtyAndHIPrice.ItemNumber3MaxBenefit > 0)
                        {
                            if (CurRegistration.IsCrossRegion == false)
                            {
                                PtHiBenefit = value.LimQtyAndHIPrice.ItemNumber3MaxBenefit;
                            }
                            else
                            {
                                PtHiBenefit = (decimal)Globals.ServerConfigSection.HealthInsurances.RebatePercentage2015Level1_InPt;
                            }
                        }
                        //value.TotalHIPayment = Math.Min(value.LimQtyAndHIPrice.ItemNumber1MaxPayAmt2, value.ChargeableItem.HIAllowedPrice.GetValueOrDefault(0) * (decimal)value.HIPaymentPercent) * (CurRegistration.IsCrossRegion.GetValueOrDefault(false) ? (decimal)Globals.ServerConfigSection.HealthInsurances.RebatePercentageLevel1 / (value.HIPaymentPercent != 0 ? (decimal)value.HIPaymentPercent : 1.0m) : 1.0m);
                        value.TotalHIPayment = Math.Min(value.LimQtyAndHIPrice.ItemNumber3MaxPayAmt, value.ChargeableItem.HIAllowedPrice.GetValueOrDefault(0) * (decimal)value.HIPaymentPercent) * PtHiBenefit;
                    }
                }
                else
                {
                    value.IsCountHI = false;
                    value.HIPaymentPercent = 0;
                    value.TotalHIPayment = 0;
                    value.HIPayment = 0;
                }
                if (value.TotalHIPayment == 0)
                {
                    value.IsCountHI = false;
                }
                //20191219 TTM: Đối với những dòng trong gói rồi thì luôn mặc định bệnh nhân chi trả = 0. 
                if (value.IsInPackage)
                {
                    value.TotalPatientPayment = 0;
                }
                else
                {
                    //20191219 TTM: TotalInvoicePrice phải được kiểm tra < 0 vì TotalInvoicePrice là thành tiền <=> Tổng bảo hiểm chi + tổng bệnh nhân chi trả <=> Số lượng * đơn giá.
                    //              Nên không thể bé hơn 0 được vì thế giá trị min trong trường hợp tính cho tiền bệnh nhân chi trả phải từ tiền bảo hiểm chi trả trở lên.
                    value.TotalPatientPayment = value.TotalInvoicePrice < value.TotalHIPayment ? 0 : value.TotalInvoicePrice - value.TotalHIPayment;
                }
            }
        }

        public void ckbDiscount_Click(object source, object sender)
        {
            if (!(source is CheckBox))
            {
                return;
            }
            CheckBox ckbDiscount = source as CheckBox;
            bool? copierChecked = ckbDiscount.IsChecked;
            if (!(ckbDiscount.DataContext is MedRegItemBase) || CurRegistration == null || CurRegistration.PromoDiscountProgramObj == null)
            {
                ckbDiscount.IsChecked = false;
                (ckbDiscount.DataContext as MedRegItemBase).DiscountAmt = 0;
                CommonGlobals.ChangeItemsRecordState((ckbDiscount.DataContext as MedRegItemBase));
                return;
            }
            (ckbDiscount.DataContext as MedRegItemBase).PromoDiscProgID = CurRegistration.PromoDiscountProgramObj == null ? null : (long?)CurRegistration.PromoDiscountProgramObj.PromoDiscProgID;
            if (ckbDiscount.DataContext is PatientPCLRequestDetail)
            {
                PatientPCLRequestDetail detail = ckbDiscount.DataContext as PatientPCLRequestDetail;
                //BLQ: Kiểm tra miễn giảm không có cls nào thì return
                if (CurRegistration.PromoDiscountProgramObj != null && CurRegistration.PromoDiscountProgramObj.PromoDiscountItems != null
                    && CurRegistration.PromoDiscountProgramObj.PromoDiscountItems.Count() > 0
                    && CurRegistration.PromoDiscountProgramObj.PromoDiscountItems.Where(x => x.PCLExamTypeID > 0).Count() == 0)
                {
                    ckbDiscount.IsChecked = !ckbDiscount.IsChecked;
                    MessageBox.Show(eHCMSResources.Z3231_G1_ThongBaoKhongCoMienGiam, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                //BLQ: Kiểm tra CLS đang check có trong phiếu miễn giảm không
                if (CurRegistration.PromoDiscountProgramObj != null && CurRegistration.PromoDiscountProgramObj.PromoDiscountItems != null
                    && CurRegistration.PromoDiscountProgramObj.PromoDiscountItems.Count() > 0
                    && CurRegistration.PromoDiscountProgramObj.PromoDiscountItems.Where(x => x.ObjPCLExamType != null && x.ObjPCLExamType.PCLExamTypeName == detail.PCLExamType.PCLExamTypeName).Count() == 0)
                {
                    ckbDiscount.IsChecked = !ckbDiscount.IsChecked;
                    MessageBox.Show(eHCMSResources.Z3231_G1_ThongBaoKhongCoMienGiam, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            if (ckbDiscount.DataContext is PatientRegistrationDetail)
            {
                PatientRegistrationDetail detail = ckbDiscount.DataContext as PatientRegistrationDetail;
                //BLQ: Kiểm tra không có dịch vụ thì return
                if (CurRegistration.PromoDiscountProgramObj != null && CurRegistration.PromoDiscountProgramObj.PromoDiscountItems != null
                    && CurRegistration.PromoDiscountProgramObj.PromoDiscountItems.Count() > 0
                    && CurRegistration.PromoDiscountProgramObj.PromoDiscountItems.Where(x => x.MedServiceID > 0).Count() == 0)
                {
                    ckbDiscount.IsChecked = !ckbDiscount.IsChecked;
                    MessageBox.Show(eHCMSResources.Z3231_G1_ThongBaoKhongCoMienGiam, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                //BLQ: Kiểm tra dịch vụ đang check có trong phiếu miễn giảm không
                if (CurRegistration.PromoDiscountProgramObj != null && CurRegistration.PromoDiscountProgramObj.PromoDiscountItems != null
                    && CurRegistration.PromoDiscountProgramObj.PromoDiscountItems.Count() > 0
                    && CurRegistration.PromoDiscountProgramObj.PromoDiscountItems.Where(x => x.ObjRefMedicalServiceItem.MedServiceName == detail.MedServiceName).Count() == 0)
                {
                    ckbDiscount.IsChecked = !ckbDiscount.IsChecked;
                    MessageBox.Show(eHCMSResources.Z3231_G1_ThongBaoKhongCoMienGiam, eHCMSResources.G0442_G1_TBao, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            ModifiItemCmd(ckbDiscount.DataContext as MedRegItemBase);
            if (CurRegistration.PromoDiscountProgramObj != null && CurRegistration.PromoDiscountProgramObj.IsOnPriceDiscount)
            {
                (ckbDiscount.DataContext as MedRegItemBase).IsDiscountChecked = ckbDiscount.IsChecked.GetValueOrDefault(false);
                if (!(ckbDiscount.DataContext as MedRegItemBase).IsDiscountChecked)
                {
                    if ((MessageBox.Show(eHCMSResources.Z2976_G1_XoaThongTinMienGiam, eHCMSResources.G0442_G1_TBao, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)) == MessageBoxResult.No)
                    {
                        ckbDiscount.IsChecked = !(bool)copierChecked;
                        (ckbDiscount.DataContext as MedRegItemBase).IsDiscountChecked = !(bool)copierChecked;
                        CurRegistration.ApplyDiscount((ckbDiscount.DataContext as MedRegItemBase), Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, !((source as CheckBox).IsChecked == true));
                        CommonGlobals.ChangeItemsRecordState((ckbDiscount.DataContext as MedRegItemBase));
                        return;
                    }
                }
                CurRegistration.ApplyDiscount((ckbDiscount.DataContext as MedRegItemBase), Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, !((source as CheckBox).IsChecked == true));
                CommonGlobals.ChangeItemsRecordState((ckbDiscount.DataContext as MedRegItemBase));
                if (copierChecked == true)
                {
                    ckbDiscount.IsChecked = true;
                }
                return;
            }
            if ((ckbDiscount.DataContext as MedRegItemBase).DiscountAmt == 0 && CurRegistration.PromoDiscountProgramObj != null && CurRegistration.PromoDiscountProgramObj.DiscountPercent == 0)
            {
                ckbDiscount.IsChecked = false;
                (ckbDiscount.DataContext as MedRegItemBase).DiscountAmt = 0;
                CommonGlobals.ChangeItemsRecordState((ckbDiscount.DataContext as MedRegItemBase));
                return;
            }
            if ((ckbDiscount.DataContext as MedRegItemBase).PromoDiscProgID != null && (ckbDiscount.DataContext as MedRegItemBase).PromoDiscProgID != CurRegistration.PromoDiscountProgramObj.PromoDiscProgID)
            {
                (ckbDiscount.DataContext as MedRegItemBase).PromoDiscProgID = 0;
            }
            if (!ckbDiscount.IsChecked.GetValueOrDefault(false))
            {
                if ((MessageBox.Show(eHCMSResources.Z2976_G1_XoaThongTinMienGiam, eHCMSResources.G0442_G1_TBao, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)) == MessageBoxResult.No)
                {
                    ckbDiscount.IsChecked = !(bool)copierChecked;
                    (ckbDiscount.DataContext as MedRegItemBase).IsDiscountChecked = !(bool)copierChecked;
                }
            }
            CurRegistration.ApplyDiscount((ckbDiscount.DataContext as MedRegItemBase), Globals.ServerConfigSection.PharmacyElements.OnlyRoundResultForOutward, !((source as CheckBox).IsChecked == true));
            if ((ckbDiscount.DataContext as MedRegItemBase) is PatientPCLRequestDetail)
            {
                ModifiItemCmd((ckbDiscount.DataContext as MedRegItemBase));
            }
        }

        public IEnumerator<IResult> CoroutineRemoveItem(MedRegItemBase item, object eventArgs)
        {
            bool exists = false;
            if (item is PatientRegistrationDetail)
            {
                var detailsItem = item as PatientRegistrationDetail;
                if (detailsItem.RecordState == RecordState.ADDED || detailsItem.RecordState == RecordState.DETACHED)
                {
                    exists = AllRegistrationItems.Remove(detailsItem);
                }
                else
                {
                    DeletedItemCmd(detailsItem);
                    RemoveItemFromView(detailsItem);
                }
            }
            else if (item is PatientPCLRequestDetail)
            {
                var detailsItem = item as PatientPCLRequestDetail;

                if (detailsItem.PCLReqItemID <= 0 && detailsItem.PatientPCLReqID <= 0)
                {
                    AllRegistrationItems.Remove(detailsItem);
                    RemoveItemFromView(detailsItem);
                    yield break;
                }
                if (detailsItem.PatientPCLReqID >= 0)
                {
                    foreach (var PCLDetails in AllRegistrationItems)
                    {
                        if (PCLDetails is PatientPCLRequestDetail)
                        {
                            var itemPCLDetails = PCLDetails as PatientPCLRequestDetail;
                            if (itemPCLDetails.PatientPCLReqID == detailsItem.PatientPCLReqID)
                            {
                                DeletedItemCmd(itemPCLDetails);
                                RemoveItemFromView(itemPCLDetails);
                            }
                        }
                    }
                    yield break;
                }
            }
            else
            {
                var detailsItem = item as OutwardDrugClinicDept;
                foreach (var inv in AllRegistrationItems)
                {
                    if (inv is OutwardDrugClinicDept)
                    {
                        var detailsItemProduct = inv as OutwardDrugClinicDept;
                        if (detailsItemProduct.outiID == detailsItem.outiID)
                        {
                            DeletedItemCmd(detailsItemProduct);
                            RemoveItemFromView(detailsItemProduct);
                        }
                    }
                }
                yield break;
            }
        }

        public void RemoveItemFromView(MedRegItemBase item)
        {
            if (AllRegistrationItemsFilter != null)
            {
                AllRegistrationItemsFilter.Remove(item);
            }
        }

        public void FilterListItemCmd()
        {
            AllRegistrationItems = new ObservableCollection<MedRegItemBase>();
            AllRegistrationItemsFilter = new ObservableCollection<MedRegItemBase>();
            GetAllInPatientBillingInvoices();
        }

        private void UpdateDataItemFromGrid(MedRegItemBase item)
        {
            foreach (var temp in AllRegistrationItems)
            {
                if (temp is PatientRegistrationDetail && item is PatientRegistrationDetail)
                {
                    var itemTemp = temp as PatientRegistrationDetail;
                    if (itemTemp.PtRegDetailID == (item as PatientRegistrationDetail).PtRegDetailID)
                    {
                        temp.IsCountHI = item.IsCountHI;
                        temp.InvoicePrice = item.InvoicePrice;
                        temp.TotalCoPayment = item.TotalCoPayment;
                        temp.TotalHIPayment = item.TotalHIPayment;
                        temp.TotalPriceDifference = item.TotalPriceDifference;
                        temp.PriceDifference = item.PriceDifference;
                        temp.HisID = item.HisID;
                        temp.OtherAmt = item.OtherAmt;
                        temp.IsCountPatientCOVID = item.IsCountPatientCOVID;
                        //=====▼ #026
                        temp.IsCountSE = item.IsCountSE;
                        //=====▲ #026
                        break;
                    }
                }

                if (temp is PatientPCLRequestDetail && item is PatientPCLRequestDetail)
                {
                    var child = temp as PatientPCLRequestDetail;
                    if (child.PCLReqItemID == (item as PatientPCLRequestDetail).PCLReqItemID)
                    {
                        temp.IsCountHI = item.IsCountHI;
                        temp.InvoicePrice = item.InvoicePrice;
                        temp.TotalCoPayment = item.TotalCoPayment;
                        temp.TotalHIPayment = item.TotalHIPayment;
                        temp.TotalPriceDifference = item.TotalPriceDifference;
                        temp.PriceDifference = item.PriceDifference;
                        temp.HisID = item.HisID;
                        temp.OtherAmt = item.OtherAmt;
                        temp.IsCountPatientCOVID = item.IsCountPatientCOVID;
                        //=====▼ #026
                        temp.IsCountSE = item.IsCountSE;
                        //=====▲ #026
                        break;
                    }
                }

                if (temp is OutwardDrugClinicDept && item is OutwardDrugClinicDept)
                {
                    var child = temp as OutwardDrugClinicDept;
                    if (child.OutID == (item as OutwardDrugClinicDept).OutID)
                    {
                        temp.IsCountHI = item.IsCountHI;
                        temp.InvoicePrice = item.InvoicePrice;
                        temp.TotalCoPayment = item.TotalCoPayment;
                        temp.TotalHIPayment = item.TotalHIPayment;
                        temp.TotalPriceDifference = item.TotalPriceDifference;
                        temp.PriceDifference = item.PriceDifference;
                        temp.HisID = item.HisID;
                        temp.OtherAmt = item.OtherAmt;
                        temp.IsCountPatientCOVID = item.IsCountPatientCOVID;
                        //=====▼ #026
                        temp.IsCountSE = item.IsCountSE;
                        //=====▲ #026
                        break;
                    }
                }
            }
        }
        //=====▼ #026
        public void chkCountSE_Click(object source, object sender)
        {
            if (SelectedRegistrationItem == null)
            {
                return;
            }
            if (HIBenefit.GetValueOrDefault(0) > 0)
            {
                SelectedRegistrationItem.IsCountHI = !SelectedRegistrationItem.IsCountSE;
                chkCountHI_Click(source, sender);
            }
            else
            {
                ModifiItemCmd(SelectedRegistrationItem);
                UpdateDataItemFromGrid(SelectedRegistrationItem);
            }
        }

        public bool IsEnableCountSE
        {
            get
            {
                return EditingInvoiceDetailsContent.IsEnableCountSE;
            }
        }
        //=====▲ #026
    }
}
