﻿using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace eHCMS.ReportLib.PCLDepartment
{
    public partial class XRpt_PCLImagingResult_New_Dien_Tim : DevExpress.XtraReports.UI.XtraReport
    {
        public XRpt_PCLImagingResult_New_Dien_Tim()
        {
            InitializeComponent();
        }
        private void FillData()
        {
            dsXRpt_PCLImagingResult_New_Dien_Tim1.EnforceConstraints = false;
            spXRpt_PCLImagingResult_New_Dien_TimTableAdapter.Fill(dsXRpt_PCLImagingResult_New_Dien_Tim1.spXRpt_PCLImagingResult_New_Dien_Tim, Convert.ToInt64(PCLImgResultID.Value), Convert.ToInt64(V_PCLRequestType.Value));
           
        }

        private void XRpt_PCLImagingResult_New_Test_Nhanh_Cov_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            FillData();
        }
    }
}
