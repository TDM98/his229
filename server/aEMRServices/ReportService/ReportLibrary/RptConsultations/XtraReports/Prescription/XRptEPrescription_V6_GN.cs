﻿using System;
using DevExpress.XtraReports.UI;
using System.Linq;
/*
 * 20181029 #001 TTM: BM 0004199: Thêm cảnh báo nếu bác sĩ ra toa thuốc có thuốc ngoài danh mục cần phải có sự xác nhận của bệnh nhân
 * 20181101 #002 TTM: BM 0004220: Thêm điều kiện chỉ FillData lúc cần thiết (khi bệnh nhân có toa hướng thần hoặc toa chứ TPCN/MP) và thêm toa TPCN/MP
 * 20220823 #003 DatTB: Chỉnh sửa màn hình chờ nhận thuốc
 */
namespace eHCMS.ReportLib.RptConsultations.XtraReports
{
    public partial class XRptEPrescription_V6_GN : XtraReport
    {
        public XRptEPrescription_V6_GN()
        {
            InitializeComponent();
        }

        private void FillData()
        {
            dsPrescriptionNew1.EnforceConstraints = false;
            string Str1 = parIssueID.Value.ToString().Substring(0, parIssueID.Value.ToString().Length - 1);
            spPrescriptions_RptHeaderByIssueIDTableAdapter.Fill(dsPrescriptionNew1.spPrescriptions_RptHeaderByIssueID, Convert.ToInt32(Str1));
            spPrescriptions_RptViewByPrescriptIDTableAdapter.Fill(dsPrescriptionNew1.spPrescriptions_RptViewByPrescriptID, Convert.ToInt32(Str1), false, true);
        }

        private void XRptEPrescription_TV_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            string TypePrescription = parIssueID.Value.ToString().Remove(0, parIssueID.Value.ToString().Length - 1);
            if (!Convert.ToBoolean(parIsPsychotropicDrugs.Value) && TypePrescription == "4")
            {
                e.Cancel = true;
            }
            else if (!Convert.ToBoolean(parIsFuncfoodsOrCosmetics.Value) && TypePrescription == "5")
            {
                e.Cancel = true;
            }
            else
            {
                FillData();
                if (!string.IsNullOrEmpty(dsPrescriptionNew1.spPrescriptions_RptHeaderByIssueID.FirstOrDefault()["BHYT"].ToString()))
                {
                    xrLabel15.Text = "ĐƠN THUỐC \"N\"";
                    xrLabel17.Visible = true; //==== #003
                }
                else
                {
                    xrLabel15.Text = "ĐƠN THUỐC \"N\"";
                }
                switch (parSubVersion.Value.ToString())
                {
                    case "1":
                        xrLabel8.Text = "(Bản lưu tại cơ sở khám bệnh, chữa bệnh)";
                        break;
                    case "2":
                        xrLabel8.Text = "(Bản lưu tại cơ sở cấp, bán thuốc)";
                        xrLabel4.Visible = true;
                        xrLabel5.Visible = true;
                        break;
                    case "3":
                        xrLabel8.Text = "(Bản giao cho người bệnh)";
                        break;
                    default:
                        xrLabel8.Text = "(Bản lưu tại cơ sở khám bệnh, chữa bệnh)";
                        break;
                }

            }
           
        }

        private void Detail_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if (Convert.ToInt32(parDetailBeforePrintCount.Value) != 0)
            {
                e.Cancel = true;
            }
            else
            {
                parDetailBeforePrintCount.Value = Convert.ToInt32(parDetailBeforePrintCount.Value) + 1;
            }
        }

        private void XrSubreport2_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            ((XRptEPrescriptionDetails_V6_GN_HT_Drug)((XRSubreport)sender).ReportSource).Parameters["parIssueID"].Value = Convert.ToInt32(parIssueID.Value.ToString());
            ((XRptEPrescriptionDetails_V6_GN_HT_Drug)((XRSubreport)sender).ReportSource).FilterString = FilterString;
            ((XRptEPrescriptionDetails_V6_GN_HT_Drug)((XRSubreport)sender).ReportSource).parIsPsychotropicDrugs = parIsPsychotropicDrugs;
            ((XRptEPrescriptionDetails_V6_GN_HT_Drug)((XRSubreport)sender).ReportSource).parIsFuncfoodsOrCosmetics = parIsFuncfoodsOrCosmetics;
        }

        //private void XrSubreport1_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        //{
        //    //switch(parHospitalCode.Value.ToString())
        //    //{
        //    //    case "95076":
        //    //        xrSubreport1.ReportSource = new XRptEPrescription_SubLeft_TV1();
        //    //        xrSubreport1.ReportSource.Parameters["parPtRegDetailID"].Value = Convert.ToInt64(parIssueID.Value.ToString().Substring(0, parIssueID.Value.ToString().Length - 1));
        //    //        xrSubreport1.ReportSource.Parameters["parV_RegistrationType"].Value = 24001;
        //    //        break;
        //    //    case "95078":
        //    //        xrSubreport1.ReportSource = new XRptEPrescription_SubLeft_TV2();
        //    //        xrSubreport1.ReportSource.Parameters["parPtRegDetailID"].Value = Convert.ToInt64(parIssueID.Value.ToString().Substring(0, parIssueID.Value.ToString().Length - 1));
        //    //        xrSubreport1.ReportSource.Parameters["parV_RegistrationType"].Value = 24001;
        //    //        break;
        //    //    default:
        //    //        xrSubreport1.ReportSource = new XRptEPrescription_SubLeft_TV3();
        //    //        xrSubreport1.ReportSource.Parameters["parPtRegDetailID"].Value = Convert.ToInt64(parIssueID.Value.ToString().Substring(0, parIssueID.Value.ToString().Length - 1));
        //    //        xrSubreport1.ReportSource.Parameters["parV_RegistrationType"].Value = 24001;
        //    //        break;
        //    //}
        //    xrSubreport1.ReportSource = new XRptEPrescription_SubLeft_V5();
        //    xrSubreport1.ReportSource.Parameters["parPtRegDetailID"].Value = Convert.ToInt64(parIssueID.Value.ToString().Substring(0, parIssueID.Value.ToString().Length - 1));
        //    xrSubreport1.ReportSource.Parameters["parV_RegistrationType"].Value = 24001;
        //}

        private void XrLabel80_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            //bool IsOutCatConfirmed = false;
            //bool IsHIPatient = false;
            //if (!string.IsNullOrEmpty(dsPrescriptionNew1.spPrescriptions_RptHeaderByIssueID.FirstOrDefault()["BHYT"].ToString()))
            //{
            //    IsHIPatient = true;               
            //}
            //if (dsPrescriptionNew1.spPrescriptions_RptHeaderByIssueID.FirstOrDefault().IsOutCatConfirmed)
            //{
            //    IsOutCatConfirmed = true;
            //}
            //if (IsOutCatConfirmed && IsHIPatient)
            //{
            //    xrLabel80.Visible = true;                
            //}
            //else
            //{
            //    xrLabel80.Visible = false;
            //}
        }

        private void xrSubreport3_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            ((XRptEPrescriptionDetails_V6_GN_HT_Info)((XRSubreport)sender).ReportSource).Parameters["parIssueID"].Value = Convert.ToInt32(parIssueID.Value.ToString());
            ((XRptEPrescriptionDetails_V6_GN_HT_Info)((XRSubreport)sender).ReportSource).FilterString = FilterString;
            ((XRptEPrescriptionDetails_V6_GN_HT_Info)((XRSubreport)sender).ReportSource).parIsPsychotropicDrugs = parIsPsychotropicDrugs;
            ((XRptEPrescriptionDetails_V6_GN_HT_Info)((XRSubreport)sender).ReportSource).parIsFuncfoodsOrCosmetics = parIsFuncfoodsOrCosmetics;
            ((XRptEPrescriptionDetails_V6_GN_HT_Info)((XRSubreport)sender).ReportSource).parHospitalCode.Value = parHospitalCode;
            ((XRptEPrescriptionDetails_V6_GN_HT_Info)((XRSubreport)sender).ReportSource).parIsYHCTPrescript.Value = parIsYHCTPrescript;
        }

        private void xrTable2_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {

        }
    }
}
