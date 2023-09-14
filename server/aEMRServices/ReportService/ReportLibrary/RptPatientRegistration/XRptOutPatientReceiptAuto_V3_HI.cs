﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using AxLogging;
using DataEntities;
using DevExpress.XtraReports.UI;
using eHCMS.Services.Core;
using eHCMSLanguage;

namespace eHCMS.ReportLib.RptPatientRegistration
{
    public partial class XRptOutPatientReceiptAuto_V3_HI : DevExpress.XtraReports.UI.XtraReport
    {
        public XRptOutPatientReceiptAuto_V3_HI()
        {
            AxLogger.Instance.LogInfo("XRptOutPatientReceipt HAM KHOI TAO");  

            InitializeComponent();
            PrintingSystem.ShowPrintStatusDialog = false;
        }
        void XRptPatientPayment_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            outPatientReceipt1.EnforceConstraints = false;
            sp_Rpt_spReportOutPatientCashReceipt_ByPaymentIDTableAdapter.ClearBeforeFill=true;
            sp_Rpt_spReportOutPatientCashReceipt_ByPaymentIDTableAdapter.Fill(outPatientReceipt1.sp_Rpt_spReportOutPatientCashReceipt_ByPaymentID, Convert.ToInt64(this.param_PaymentID.Value), null);
            decimal totalBN = 0;
            decimal totalBH = 0;
            decimal totalAmount = 0;
            string strservices = "";
            if (outPatientReceipt1.sp_Rpt_spReportOutPatientCashReceipt_ByPaymentID != null && outPatientReceipt1.sp_Rpt_spReportOutPatientCashReceipt_ByPaymentID.Rows.Count > 0)
            {
                foreach (DataRow row in outPatientReceipt1.sp_Rpt_spReportOutPatientCashReceipt_ByPaymentID.Rows)
                {
                    if (!string.IsNullOrEmpty(strservices))
                    {
                        strservices += ", ";
                    }
                    strservices += row["ServiceName"].ToString();

                    totalAmount += (decimal)row["Amount"];
                    totalBN += (decimal)row["PatientAmount"];

                }

                Parameters["parame_SequenceNumberString"].Value = AxHelper.GetSequenceNumber((byte)outPatientReceipt1.sp_Rpt_spReportOutPatientCashReceipt_ByPaymentID.Rows[0]["ServiceSeqNumType"],(int)outPatientReceipt1.sp_Rpt_spReportOutPatientCashReceipt_ByPaymentID.Rows[0]["ServiceSeqNum"]);
            }

            totalBH = totalAmount - Math.Round(totalBN, 0);

            NumberToLetterConverter converter = new NumberToLetterConverter();
            decimal temp = 0;
            string prefix = "";
            if (totalBH < 0)
            {
                temp = 0 - totalBH;
                prefix = string.Format(" {0} ",  eHCMSResources.Z0873_G1_Am.ToLower());
            }
            else
            {
                temp = totalBH;
                prefix = "";
            }

            Parameters["parServiceString"].Value = strservices;

            Parameters["parTotalHIPayment"].Value = totalBH.ToString("N0") + eHCMSResources.G1616_G1_VND.ToLower();

            Parameters["parTotalHIPaymentInWords"].Value = prefix + converter.Convert(temp.ToString(), '.', eHCMSResources.Z0871_G1_Le.ToLower(), eHCMSResources.G1616_G1_VND.ToUpper());
        }

    }
}
