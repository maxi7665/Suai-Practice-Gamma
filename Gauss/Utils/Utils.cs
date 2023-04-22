using System.Drawing;
using System.Drawing.Imaging;

namespace Gauss.Utils
{
    internal static class Utils
    {
        /// <summary>
        /// Применить гамма-коррекцию
        /// </summary>
        /// <param name="bmp">Битовая карта изображения</param>
        /// <param name="redComponent">Коррекция красного канала</param>
        /// <param name="greenComponent">Коррекция зеленого канала</param>
        /// <param name="blueComponent">Коррекция синего канала</param>
        public static void ApplyGamma(
            ref Bitmap bmp, 
            double redComponent, 
            double greenComponent, 
            double blueComponent)
        {
            // допустимые значения коэффициента коррекции
            if (redComponent < 0.2 || redComponent > 5) return;
            if (greenComponent < 0.2 || greenComponent > 5) return;
            if (blueComponent < 0.2 || blueComponent > 5) return;

            // блокировка данных изображения в памяти
            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height), 
                ImageLockMode.ReadWrite, 
                PixelFormat.Format24bppRgb);

            unsafe
            {
                // указатель на начало данных изображения
                byte* ptr = (byte*)bmpData.Scan0.ToPointer();

                // расчет ограничения отрезка памяти для коррекции
                int stopAddress = (int)ptr + bmpData.Stride * bmpData.Height;

                // представление указателя в целочисленном виде
                int intPtr = (int)ptr;

                while (intPtr < stopAddress)
                {
                    // обработка одного пиксела - коррекция R,G,B
                    ptr[0] = (byte)Math.Min(
                        255, 
                        (int)((255.0 * Math.Pow(
                            ptr[0] / 255.0, 
                            1.0 / blueComponent)) + 0.5));

                    ptr[1] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(ptr[1] / 255.0, 1.0 / greenComponent)) + 0.5));
                    ptr[2] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(ptr[2] / 255.0, 1.0 / redComponent)) + 0.5));

                    // смещение на три байта
                    ptr += 3;

                    intPtr = (int)ptr;
                }
            }

            // разблокировка данных изображения
            bmp.UnlockBits(bmpData);
        }

        public static async Task<Bitmap?> PickAndShow(PickOptions options)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(options);

                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();

                    return (Bitmap) Bitmap.FromFile(result.FullPath);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return null;
        }
    }
}
