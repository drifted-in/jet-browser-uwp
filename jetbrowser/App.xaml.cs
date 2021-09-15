using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace jetbrowser
{
    sealed partial class App : Application
    {
        public static StorageFile zipArchiveFile;

        public App()
        {
            InitializeComponent();
        }
        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            zipArchiveFile = ((StorageFile) args.Files[0]);
            InitApp();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            InitApp();   
        }

        void InitApp()
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), null);
            }
            Window.Current.Activate();
        }
    }
}
