using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Image = System.Windows.Media.ImageSource;

namespace ForRina
{
    public class ViewModel: ViewModelBase
    {

        private Image[] currImages = new Image[3] { null, null, null  };
        public Image CurrentPic1 { get=> currImages[0]; set { currImages[0] = value; OnPropertyChanged("CurrentPic1"); } }

        public Image CurrentPic2 { get => currImages[1]; set { currImages[1] = value; OnPropertyChanged("CurrentPic2"); } }

        public Image CurrentPic3 { get => currImages[2]; set { currImages[2] = value; OnPropertyChanged("CurrentPic3"); } }

        private void RefreshAll()
        {
            OnPropertyChanged("CurrentPic1");
            OnPropertyChanged("CurrentPic2");
            OnPropertyChanged("CurrentPic3");
        }

        private ICommand start;
        public ICommand StartWork { get=> start; set { start = value; OnPropertyChanged("StartWork"); } }

        private ICommand changePic;
        public ICommand ChangePic { get => changePic; set { changePic = value; OnPropertyChanged("ChangePic"); } }

        private List<Image> allImages;
        public ViewModel() 
        {
            StartWork = new RelayCommand(Start);
            ChangePic = new RelayCommand(ChangePicForRandom);
        }

        private void Start(object obj)
        {
            try
            {
                currImages = new Image[3] { null, null, null };
                RefreshAll();
                allImages = ReadImageFiles(Path.Combine(Directory.GetCurrentDirectory(), "Images"));

                if (allImages.Count < 4)
                {
                    MessageBox.Show("Добавьте больше картинок >:)", "Сделай этой!");
                    Environment.Exit(0);
                }

                while (currImages.Contains(null))
                {

                    foreach (var image in allImages)
                    {
                        if (currImages.Contains(image))
                        {
                            continue;
                        }

                        Random rnd = new Random();
                        int i = rnd.Next(3);
                        if (currImages[i] == null)
                        {
                            currImages[i] = image;
                        }
                    }

                }
                RefreshAll();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
        }

        private void ChangePicForRandom(object obj)
        {
            try
            {
                if (obj is Image pic)
                {

                    int number = -1;

                    for (int i = 0; i < currImages.Length; i++)
                    {
                        if (currImages[i] == pic)
                        {
                            number = i;
                            break;
                        }
                    }


                    Bitmap bitPic = Extension.BitmapSourceToBitmap(currImages[number] as BitmapSource);
                    Bitmap copy = (Bitmap)bitPic.Clone();

                    int EqualPartsOutOf256 = Extension.GetHash(bitPic).Zip(Extension.GetHash(copy), (i, j) => i == j).Count(eq => eq);

                    Random rnd = new Random();
                    while (EqualPartsOutOf256 > 250)
                    {
                        foreach (var image in allImages)
                        {
                            if (currImages.Contains(image))
                            {
                                continue;
                            }

                            if (rnd.Next(2) == 1)
                            {
                                currImages[number] = image;
                                bitPic = Extension.BitmapSourceToBitmap(currImages[number] as BitmapSource);
                                EqualPartsOutOf256 = Extension.GetHash(bitPic).Zip(Extension.GetHash(copy), (i, j) => i == j).Count(eq => eq);
                            }
                        }
                    }
                    RefreshAll();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
        }

        private List<Image> ReadImageFiles(string path)
        {
            Directory.CreateDirectory(path);
            var files = Directory.GetFiles(path);
            List<Image> list = new List<Image>();
            foreach (var file in files)
            {

                try
                {
                    Bitmap bitmapImage = new Bitmap(file);
                    MemoryStream ms = new MemoryStream();
                    bitmapImage.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();
                    list.Add(bi);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }

            return list;


        }
    }

    public static class Extension
    {
        public static List<bool> GetHash(Bitmap bmpSource)
        {
            List<bool> lResult = new List<bool>();
            //create new image with 16x16 pixel
            Bitmap bmpMin = new Bitmap(bmpSource, new System.Drawing.Size(16, 16));
            for (int j = 0; j < bmpMin.Height; j++)
            {
                for (int i = 0; i < bmpMin.Width; i++)
                {
                    //reduce colors to true / false                
                    lResult.Add(bmpMin.GetPixel(i, j).GetBrightness() < 0.5f);
                }
            }
            return lResult;
        }


        public static System.Drawing.Bitmap BitmapSourceToBitmap(BitmapSource srs)
        {
            int width = srs.PixelWidth;
            int height = srs.PixelHeight;
            int stride = width * ((srs.Format.BitsPerPixel + 7) / 8);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(height * stride);
                srs.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height * stride, stride);
                using (var btm = new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format1bppIndexed, ptr))
                {
                    // Clone the bitmap so that we can dispose it and
                    // release the unmanaged memory at ptr
                    return new System.Drawing.Bitmap(btm);
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
