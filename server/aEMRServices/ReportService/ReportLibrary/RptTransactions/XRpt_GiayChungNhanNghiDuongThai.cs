using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace eHCMS.ReportLib.RptTransactions
{
    public partial class XRpt_GiayChungNhanNghiDuongThai : DevExpress.XtraReports.UI.XtraReport
    {
        public XRpt_GiayChungNhanNghiDuongThai()
        {
            InitializeComponent();
        }
        private void xrSubreport2_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            ((XRpt_GiayChungNhanNghiDuongThaiSubReport)((XRSubreport)sender).ReportSource).VacationInsuranceCertificateID.Value = Convert.ToInt32(VacationInsuranceCertificateID.Value);
            ((XRpt_GiayChungNhanNghiDuongThaiSubReport)((XRSubreport)sender).ReportSource).parHospitalName.Value = parHospitalName.Value;
            ((XRpt_GiayChungNhanNghiDuongThaiSubReport)((XRSubreport)sender).ReportSource).parHospitalCode.Value = parHospitalCode.Value;
        }

        private void xrSubreport1_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            ((XRpt_GiayChungNhanNghiDuongThaiSubReport)((XRSubreport)sender).ReportSource).VacationInsuranceCertificateID.Value = Convert.ToInt32(VacationInsuranceCertificateID.Value);
            ((XRpt_GiayChungNhanNghiDuongThaiSubReport)((XRSubreport)sender).ReportSource).parHospitalName.Value = parHospitalName.Value;
            ((XRpt_GiayChungNhanNghiDuongThaiSubReport)((XRSubreport)sender).ReportSource).parHospitalCode.Value = parHospitalCode.Value;
        }
    }
}
