
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows;

namespace aEMR.Controls
{
    public class AxRadioButton : RadioButton
    {
        public object RadioValue
        {
            get { return (object)GetValue(RadioValueProperty); }
            set { SetValue(RadioValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RadioValue.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadioValueProperty =
            DependencyProperty.Register(
                "RadioValue",
                typeof(object),
                typeof(AxRadioButton),
                new UIPropertyMetadata(null));

        public object RadioBinding
        {
            get { return (object)GetValue(RadioBindingProperty); }
            set { SetValue(RadioBindingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RadioBinding.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadioBindingProperty =
            DependencyProperty.Register(
                "RadioBinding",
                typeof(object),
                typeof(AxRadioButton),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnRadioBindingChanged));

        private static void OnRadioBindingChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            AxRadioButton rb = (AxRadioButton)d;
            if (rb.RadioValue.Equals(e.NewValue != null ? e.NewValue.ToString():""))
                rb.SetCurrentValue(RadioButton.IsCheckedProperty, true);
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            SetCurrentValue(RadioBindingProperty, RadioValue);
        }
    }
}