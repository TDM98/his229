using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace aEMR.Consultation.Views.MedicalRecordCover
{
    public partial class MedicalRecordCoverViewSample2 : UserControl
    {
        public MedicalRecordCoverViewSample2()
        {
            InitializeComponent();
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void FloatValidationTextBox(object sender, TextCompositionEventArgs e)
        {
      
        }
    }
}
