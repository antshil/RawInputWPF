using System.Windows;
using System.Windows.Interop;
using GalaSoft.MvvmLight;


namespace RawInputWPF.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static MainWindowViewModel _instance;


        public static MainWindowViewModel Instance
        {
            get { return _instance ?? (_instance = new MainWindowViewModel()); }
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                Set(ref _text, value);
            }
        }

        public MainWindowViewModel()
        {
            Text = "Main window is not loaded.";

            Application.Current.MainWindow.Loaded += OnMainWindowLoaded;
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Loaded -= OnMainWindowLoaded;

            var wih = new WindowInteropHelper(Application.Current.MainWindow);
            Text = "no button pressed";
        }
    }
}
