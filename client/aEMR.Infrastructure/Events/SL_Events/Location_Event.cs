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

namespace aEMR.Infrastructure.Events
{
    public class Location_Event
    {
    }

    public class dgListDblClickSelectLocation_Event
    {
        public object Location_Current { get; set; }
    }

    public class dgListClickSelectionChanged_Event
    {
        public object Location_Current { get; set; }
    }

    public class Location_Event_Save
    {
        public object Result { get; set; }
    }
}