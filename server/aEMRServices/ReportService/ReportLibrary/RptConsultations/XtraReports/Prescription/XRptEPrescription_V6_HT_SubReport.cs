﻿using System;
using DevExpress.XtraReports.UI;
/*
 * 20181101 #001 TTM: BM 0004220:   Thêm điều kiện chỉ FillData lúc cần thiết (khi bệnh nhân có toa hướng thần hoặc toa chứ TPCN/MP) và thêm toa TPCN/MP
 */
namespace eHCMS.ReportLib.RptConsultations.XtraReports
{
    public partial class XRptEPrescription_V6_HT_SubReport : XtraReport
    {
        public XRptEPrescription_V6_HT_SubReport()
        {
            InitializeComponent();
        }

        private void XRptEPrescription_V2_SubReport_TV_SubReport_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
        }

        private void xrSubreport1_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            //string parIssueIDstr = parIssueID.Value.ToString() + "2".ToString();
            //((XRptEPrescription_TV3)((XRSubreport)sender).ReportSource).Parameters["parIssueID"].Value = Convert.ToInt32(parIssueIDstr); // erro cast
            //((XRptEPrescription_TV3)((XRSubreport)sender).ReportSource).FilterString = "[RefGenDrugCatID_1] != 4 && [RefGenDrugCatID_1] != 5 && [RefGenDrugCatID_1] != 6"; // toa thuốc gây nghiện, toa thuốc khác hướng thần
            //((XRptEPrescription_TV3)((XRSubreport)sender).ReportSource).parHospitalCode.Value = parHospitalCode.Value.ToString();
            //((XRptEPrescription_TV3)((XRSubreport)sender).ReportSource).parPrescriptionMainRightHeader.Value = parPrescriptionMainRightHeader.Value.ToString();
            //((XRptEPrescription_TV3)((XRSubreport)sender).ReportSource).parPrescriptionSubRightHeader.Value = parPrescriptionSubRightHeader.Value.ToString();
            //((XRptEPrescription_TV3)((XRSubreport)sender).ReportSource).parPrescriptionSubRightHeader.Value = parPrescriptionSubRightHeader.Value.ToString();
            //((XRptEPrescription_TV3)((XRSubreport)sender).ReportSource).parPrescriptionSubRightHeader.Value = parPrescriptionSubRightHeader.Value.ToString();
        }

        private void xrSubreport2_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            string parIssueIDstr = parIssueID.Value.ToString() + "4".ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).Parameters["parIssueID"].Value = Convert.ToInt32(parIssueIDstr); // erro cast
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).FilterString = "[RefGenDrugCatID_1] = 4"; // toa thuốc gây nghiện
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parIsPsychotropicDrugs = parIsPsychotropicDrugs;
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalCode.Value = parHospitalCode.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parPrescriptionMainRightHeader.Value = parPrescriptionMainRightHeader.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parPrescriptionSubRightHeader.Value = parPrescriptionSubRightHeader.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parSubVersion.Value = 1;
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parDepartmentOfHealth.Value = parDepartmentOfHealth.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalAddress.Value = parHospitalAddress.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalName.Value = parHospitalName.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parKBYTLink.Value = parKBYTLink.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalPhone.Value = parHospitalPhone.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalHotline.Value = parHospitalHotline.Value.ToString();
        }

        private void xrSubreport3_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            string parIssueIDstr = parIssueID.Value.ToString() + "4".ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).Parameters["parIssueID"].Value = Convert.ToInt32(parIssueIDstr); // erro cast
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).FilterString = "[RefGenDrugCatID_1] = 4"; // toa thuốc gây nghiện
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parIsPsychotropicDrugs = parIsPsychotropicDrugs;
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalCode.Value = parHospitalCode.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parPrescriptionMainRightHeader.Value = parPrescriptionMainRightHeader.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parPrescriptionSubRightHeader.Value = parPrescriptionSubRightHeader.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parSubVersion.Value = 2;
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parDepartmentOfHealth.Value = parDepartmentOfHealth.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalAddress.Value = parHospitalAddress.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalName.Value = parHospitalName.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parKBYTLink.Value = parKBYTLink.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalPhone.Value = parHospitalPhone.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalHotline.Value = parHospitalHotline.Value.ToString();
        }

        private void xrSubreport4_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            string parIssueIDstr = parIssueID.Value.ToString() + "4".ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).Parameters["parIssueID"].Value = Convert.ToInt32(parIssueIDstr); // erro cast
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).FilterString = "[RefGenDrugCatID_1] = 4"; // toa thuốc gây nghiện
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parIsPsychotropicDrugs = parIsPsychotropicDrugs;
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalCode.Value = parHospitalCode.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parPrescriptionMainRightHeader.Value = parPrescriptionMainRightHeader.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parPrescriptionSubRightHeader.Value = parPrescriptionSubRightHeader.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parPrescriptionSubRightHeader.Value = parPrescriptionSubRightHeader.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parSubVersion.Value = 3;
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parDepartmentOfHealth.Value = parDepartmentOfHealth.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalAddress.Value = parHospitalAddress.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalName.Value = parHospitalName.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parKBYTLink.Value = parKBYTLink.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalPhone.Value = parHospitalPhone.Value.ToString();
            ((XRptEPrescription_V6_HT)((XRSubreport)sender).ReportSource).parHospitalHotline.Value = parHospitalHotline.Value.ToString();
        }
    }
}
