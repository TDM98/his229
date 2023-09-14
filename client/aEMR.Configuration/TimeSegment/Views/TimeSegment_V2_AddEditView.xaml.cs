using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace aEMR.Configuration.TimeSegment.Views
{
    [Export(typeof(TimeSegment_V2_AddEditView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TimeSegment_V2_AddEditView : UserControl
    {
        public TimeSegment_V2_AddEditView()
        {
            InitializeComponent();
        }
    }
}
