﻿using DataEntities;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace aEMR.ViewContracts.Configuration
{
    public interface IPrescriptionMaxHIPayGroupAddEdit
    {
        string TitleForm { get; set; }
        PrescriptionMaxHIPayGroup ObjPrescriptionMaxHIPayGroup_Current { get; set; }
        bool IsEdit { get; set; }
        void InitializeNewItem();
    }
}