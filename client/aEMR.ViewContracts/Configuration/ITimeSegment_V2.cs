using System;
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
    public interface ITimeSegment_V2
    {
        void AdmissionCriteria_MarkDeleted(DataEntities.AdmissionCriteria obj);

        Visibility hplAddNewVisible { get; set; }

        object TimeSegment_Current { get; set; }
    }
}
