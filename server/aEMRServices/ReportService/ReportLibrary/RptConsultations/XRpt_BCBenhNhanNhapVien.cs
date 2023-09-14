using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
namespace eHCMS.ReportLib.RptConsultations
{
    public partial class XRpt_BCBenhNhanNhapVien : DevExpress.XtraReports.UI.XtraReport
    {
        public XRpt_BCBenhNhanNhapVien()
        {
            InitializeComponent();
            FillData();
        }

        public void FillData()
        {
            spXRpt_BenhNhanNhapVienTableAdapter.Fill(baoCaoDanhSachNhapVien1.spXRpt_BenhNhanNhapVien, Convert.ToDateTime(this.parFromDate.Value), Convert.ToDateTime(this.parToDate.Value), Convert.ToInt32(this.FindPatient.Value));
        }
        private void XRpt_BCBenhNhanNhapVien_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            FillData();
        }
    }
}
