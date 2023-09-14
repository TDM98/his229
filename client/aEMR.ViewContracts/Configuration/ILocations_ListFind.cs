﻿using System;
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
    public interface ILocations_ListFind
    {
        void RoomType_GetAll();
        void Locations_MarkDeleted(Int64 LID);

        Visibility hplAddNewVisible { get; set; }
        void DoubleClick(object args);
        object Locations_Current { get; set; }
    }
}
