using System.Text;
using System.Windows;
using System.Windows.Interop;
using GalaSoft.MvvmLight;
using RawInputWPF.RawInput;


namespace RawInputWPF.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static MainWindowViewModel _instance;
        private readonly RawInputListener _rawInputListener;
        private string _gamepadDeviveName;
        private string _pressedButtons;
        private string _keyboardDeviveName;
        private string _pressedKey;

        public static MainWindowViewModel Instance
        {
            get { return _instance ?? (_instance = new MainWindowViewModel()); }
        }

        public string GamepadDeviveName
        {
            get { return _gamepadDeviveName; }
            set
            {
                Set(ref _gamepadDeviveName, value);
            }
        }

        public string PressedButtons
        {
            get { return _pressedButtons; }
            set
            {
                Set(ref _pressedButtons, value);
            }
        }

        public string KeyboardDeviveName
        {
            get { return _keyboardDeviveName; }
            set
            {
                Set(ref _keyboardDeviveName, value);
            }
        }

        public string PressedKey
        {
            get { return _pressedKey; }
            set
            {
                Set(ref _pressedKey, value);
            }
        }

        public MainWindowViewModel()
        {
            _rawInputListener = new RawInputListener();

            Application.Current.MainWindow.Loaded += OnMainWindowLoaded;
        }

        private void OnMainWindowUnloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Unloaded -= OnMainWindowUnloaded;
            if (_rawInputListener == null)
                return;

            _rawInputListener.ButtonsChanged -= OnButtonsChanged;
            _rawInputListener.KeyDown -= OnKeyDown;
            _rawInputListener.KeyDown -= OnKeyUp;
            _rawInputListener.Clear();
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Loaded -= OnMainWindowLoaded;

            var wih = new WindowInteropHelper(Application.Current.MainWindow);
            _rawInputListener.Init(wih.Handle);

            Application.Current.MainWindow.Unloaded += OnMainWindowUnloaded;
            _rawInputListener.ButtonsChanged += OnButtonsChanged;
            _rawInputListener.KeyDown += OnKeyDown;
            _rawInputListener.KeyUp += OnKeyUp;
        }

        private void OnKeyDown(object sender, KeyboardEventArgs e)
        {
            KeyboardDeviveName = string.Format(@"{0}", e.DeviceName);
            PressedKey = e.Key.ToString();
        }

        private void OnKeyUp(object sender, KeyboardEventArgs e)
        {
            KeyboardDeviveName = "";
            PressedKey = "";
        }

        private void OnButtonsChanged(object sender, GamepadEventArgs e)
        {
            if (e.Buttons.Count > 0)
            {
                GamepadDeviveName = string.Format(@"{0}", e.DeviceName);

                var sb = new StringBuilder();
                e.Buttons.ForEach(btn => sb.AppendFormat("{0} ", btn));
                PressedButtons = sb.ToString();
            }
            else
            {
                GamepadDeviveName = "";
                PressedButtons = "";
            }
        }
    }
}
