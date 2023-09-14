﻿using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.Data;

namespace eHCMS.ReportLib.RptConsultations.XtraReports
{
    public partial class XRptTransferForm : DevExpress.XtraReports.UI.XtraReport
    {
        public XRptTransferForm()
        {
            InitializeComponent();
        }

        private void XRptTransferForm_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            FillData();
            xrLabel13.Text = parHospitalName.Value.ToString() + " trân trọng giới thiệu:";
            xrLabel19.Text = "Đã được khám bệnh/điều trị tại " + parHospitalName.Value.ToString();
            if (dsTransferForm1.spGetTransferForm != null && dsTransferForm1.spGetTransferForm.Rows.Count > 0)
            {
                foreach (DataRow row in dsTransferForm1.spGetTransferForm.Rows)
                {
                    row["Gender"] = row["Gender"].ToString() == "M" ? "Nam" : "Nữ";
                    if (row["V_CMKTID"].ToString() == "62801")
                    {
                        cb1.Checked = true;
                        cb2.Checked = false;
                    }
                    else
                    {
                        cb2.Checked = true;
                        cb1.Checked = false;
                    }
                }
            }
        }

        private void FillData()
        {
            spGetTransferFormTableAdapter.Fill(dsTransferForm1.spGetTransferForm, Convert.ToInt32(TransferFormID.Value), Convert.ToInt32(PtRegistrationID.Value), Convert.ToInt32(V_TransferFormType.Value), Convert.ToInt32(V_PatientFindBy.Value));
        }
    }
}
