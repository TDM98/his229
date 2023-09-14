using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;

namespace aEMR.Configuration.TimeSegment.Views
{
    [Export(typeof(TimeSegment_V2View)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TimeSegment_V2View : UserControl
    {
        public TimeSegment_V2View()
        {
            InitializeComponent();
        }

        public event EventHandler DblClick;

        private void DataGridDoubleClickBehavior_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DblClick != null)
            {
                DblClick(this, null);
            }
        }

     
    }
}
