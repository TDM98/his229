using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;

namespace aEMR.Configuration.PrescriptionMaxHIPay.Views
{
    [Export(typeof(PrescriptionMaxHIPayGroupListFindView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PrescriptionMaxHIPayGroupListFindView : UserControl
    {
        public PrescriptionMaxHIPayGroupListFindView()
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
