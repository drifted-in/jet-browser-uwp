using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace jetbrowser
{
    public sealed partial class MainPage : Page
    {
        private const double DELTA = 1.2;
        private List<string> filePathList = new List<string>();
        private string filePath;
        private string fileName;
        private int currentImageIndex;
        private BitmapImage imageSource;

        public MainPage()
        {
            InitializeComponent();
            InitData();
        }

        private async void InitData()
        {
            if (App.zipArchiveFile == null)
            {
                App.zipArchiveFile = await GetZipArchiveFile();
            }
            if (App.zipArchiveFile != null)
            {
                filePathList = await GetFilePathList(App.zipArchiveFile);
                currentImageIndex = 0;
                UpdateImage();
            }
        }

        private async Task<StorageFile> GetZipArchiveFile()
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeFilter = { ".zip" }
            };

            return await picker.PickSingleFileAsync();
        }

        private async Task<List<string>> GetFilePathList(StorageFile storageFile)
        {
            List<string> filePaths = new List<string>();

            using (Stream stream = await storageFile.OpenStreamForReadAsync())
            {
                ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);

                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    if (entry.Name.ToLower().EndsWith(".jpg"))
                    {
                        filePaths.Add(entry.FullName);
                    }
                }
            }

            return filePaths;
        }

        private async void UpdateImage()
        {
            filePath = filePathList[currentImageIndex];
            fileName = filePath.Contains("/") ? filePath.Substring(filePath.LastIndexOf("/") + 1) : fileName;
            ApplicationView.GetForCurrentView().Title = fileName;
            imageSource = await GetImageSource(filePath);
            image.Source = imageSource;
        }

        private async Task<BitmapImage> GetImageSource(string filePath)
        {
            BitmapImage bitmapImage = new BitmapImage();

            using (Stream zipArchiveStream = await App.zipArchiveFile.OpenStreamForReadAsync())
            {
                ZipArchive zipArchive = new ZipArchive(zipArchiveStream, ZipArchiveMode.Read);
                using (Stream stream = zipArchive.GetEntry(filePath).Open())
                {
                    MemoryStream memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    await bitmapImage.SetSourceAsync(memoryStream.AsRandomAccessStream());
                }
            }

            return bitmapImage;
        }

        private void KeyDownListener(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Right ||
                e.Key == VirtualKey.Left ||
                e.Key == VirtualKey.Home ||
                e.Key == VirtualKey.End)
            {
                int newImageIndex = currentImageIndex;

                if (e.Key == VirtualKey.Home)
                {
                    newImageIndex = 0;
                }
                else if (e.Key == VirtualKey.End)
                {
                    newImageIndex = filePathList.Count - 1;
                }
                else
                {
                    int delta = IsCtrlKeyPressed() ? 20 : IsShiftKeyPressed() ? 5 : 1;

                    if (e.Key == VirtualKey.Left)
                    {
                        newImageIndex = Math.Max(currentImageIndex - delta, 0);
                    }
                    else if (e.Key == VirtualKey.Right)
                    {
                        newImageIndex = Math.Min(currentImageIndex + delta, filePathList.Count - 1);
                    }
                }

                if (newImageIndex != currentImageIndex)
                {
                    currentImageIndex = newImageIndex;
                    UpdateImage();
                }
            }
            else if (e.Key == VirtualKey.W)
            {
                Vector2 size = (sender as UIElement).ActualSize;
                double offsetX = (size.X - image.ActualSize.X) / 2;
                double scale = size.X / image.ActualSize.X;

                SetPivot(-(scale * offsetX), 0, scale);
            }
            else if (e.Key == VirtualKey.H)
            {
                Vector2 size = (sender as UIElement).ActualSize;
                double scale = size.Y / image.ActualSize.Y;

                SetPivot(0, 0, scale);
            }
            else if (e.Key == VirtualKey.R)
            {
                Vector2 size = (sender as UIElement).ActualSize;
                double offsetX = (size.X - image.ActualSize.X) / 2;
                double scale = imageSource.PixelWidth / image.ActualSize.X;

                SetPivot(-(scale * offsetX), 0, scale);
            }
            else if (e.Key == VirtualKey.C)
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(fileName);
                Clipboard.SetContent(dataPackage);
            }
        }

        private static bool IsCtrlKeyPressed()
        {
            CoreVirtualKeyStates ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
            return (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        private static bool IsShiftKeyPressed()
        {
            CoreVirtualKeyStates shiftState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);
            return (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        private void ManipulationDeltaListener(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Transform.TranslateX += e.Delta.Translation.X;
            Transform.TranslateY += e.Delta.Translation.Y;
        }

        private void PointerWheelChangedListener(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pointerPoint = e.GetCurrentPoint(sender as UIElement);
            Point point = Transform.TransformPoint(pointerPoint.Position);
            Point offset = Transform.TransformPoint(new Point(0, 0));

            double scale = Transform.ScaleX;
            double oldScale = scale;
            double mouseWheelDelta = pointerPoint.Properties.MouseWheelDelta;

            scale = (mouseWheelDelta < 0) ? scale / DELTA : scale * DELTA;

            double f = (scale / oldScale) - 1;
            double dx = point.X - offset.X;
            double dy = point.Y - offset.Y;

            Transform.TranslateX -= f * dx;
            Transform.TranslateY -= f * dy;
            Transform.ScaleX = scale;
            Transform.ScaleY = scale;
        }

        private void SetPivot(double x, double y, double scale)
        {
            Transform.TranslateX = x;
            Transform.TranslateY = y;
            Transform.ScaleX = scale;
            Transform.ScaleY = scale;
        }
    }
}
