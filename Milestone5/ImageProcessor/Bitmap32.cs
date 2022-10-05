using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageProcessing
{
    public class Bitmap32
    {
        // Provide public access to the picture's byte data.
        public byte[]? ImageBytes;
        public int RowSizeBytes;
        public const int PixelDataSize = 32;

        // A reference to the Bitmap.
        public Bitmap Bitmap;

        // True when locked.
        private bool m_IsLocked = false;

        public bool IsLocked
        {
            get
            {
                return m_IsLocked;
            }
        }

        // Save a reference to the bitmap.
        public Bitmap32(Bitmap bm)
        {
            Bitmap = bm;
        }

        // Bitmap data.
        private BitmapData? m_BitmapData;

        // Return the image's dimensions.
        public int Width
        {
            get
            {
                return Bitmap.Width;
            }
        }

        public int Height
        {
            get
            {
                return Bitmap.Height;
            }
        }

        // Provide easy access to the color values.
        public void GetPixel(int x, int y, out byte red, out byte green, out byte blue, out byte alpha)
        {
            int i = y * m_BitmapData!.Stride + x * 4;
            blue = ImageBytes![i++];
            green = ImageBytes[i++];
            red = ImageBytes[i++];
            alpha = ImageBytes[i];
        }

        public void SetPixel(int x, int y, byte red, byte green, byte blue, byte alpha)
        {
            int i = y * m_BitmapData!.Stride + x * 4;
            ImageBytes![i++] = blue;
            ImageBytes[i++] = green;
            ImageBytes[i++] = red;
            ImageBytes[i] = alpha;
        }

        public byte GetBlue(int x, int y)
        {
            int i = y * m_BitmapData!.Stride + x * 4;
            return ImageBytes![i];
        }

        public void SetBlue(int x, int y, byte blue)
        {
            int i = y * m_BitmapData!.Stride + x * 4;
            ImageBytes![i] = blue;
        }

        public byte GetGreen(int x, int y)
        {
            int i = y * m_BitmapData!.Stride + x * 4;
            return ImageBytes![i + 1];
        }

        public void SetGreen(int x, int y, byte green)
        {
            int i = y * m_BitmapData!.Stride + x * 4;
            ImageBytes![i + 1] = green;
        }

        public byte GetRed(int x, int y)
        {
            int i = y * m_BitmapData!.Stride + x * 4;
            return ImageBytes![i + 2];
        }

        public void SetRed(int x, int y, byte red)
        {
            int i = y * m_BitmapData!.Stride + x * 4;
            ImageBytes![i + 2] = red;
        }

        public byte GetAlpha(int x, int y)
        {
            int i = y * m_BitmapData!.Stride + x * 4;
            return ImageBytes![i + 3];
        }

        public void SetAlpha(int x, int y, byte alpha)
        {
            int i = y * m_BitmapData!.Stride + x * 4;
            ImageBytes![i + 3] = alpha;
        }



        // Lock the bitmap's data.

        public void LockBitmap()
        {
            // If it's already locked, do nothing.
            if (IsLocked) return;

            // Lock the bitmap data.
            Rectangle bounds = new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);

            m_BitmapData = Bitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            RowSizeBytes = m_BitmapData.Stride;

            // Allocate room for the data.
            int total_size = m_BitmapData.Stride * m_BitmapData.Height;

            ImageBytes = new byte[total_size];

            // Copy the data into the ImageBytes array.
            Marshal.Copy(m_BitmapData.Scan0, ImageBytes, 0, total_size);

            // It is now locked.
            m_IsLocked = true;
        }

        // Copy the data back into the Bitmap
        // and release resources.
        public void UnlockBitmap()
        {
            // If it's already unlocked, do nothing.
            if (!IsLocked) return;

            // Copy the data back into the bitmap.
            int total_size = m_BitmapData!.Stride * m_BitmapData.Height;
            Marshal.Copy(ImageBytes!, 0, m_BitmapData.Scan0, total_size);

            // Unlock the bitmap.
            Bitmap.UnlockBits(m_BitmapData);

            // Release resources.
            ImageBytes = null;
            m_BitmapData = null;

            // It is now unlocked.
            m_IsLocked = false;
        }

        public static Bitmap32 operator -(Bitmap32 bm1, Bitmap32 bm2)
        {
            int width = Math.Min(bm1.Width, bm2.Width);
            int height = Math.Min(bm1.Height, bm2.Height);
            Bitmap bm = new Bitmap(width, height);
            Bitmap32 bm3 = new Bitmap32(bm);
            bm1.LockBitmap();
            bm2.LockBitmap();
            bm3.LockBitmap();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    byte r1, g1, b1, a1, r2, g2, b2, a2;
                    bm1.GetPixel(x, y, out r1, out g1, out b1, out a1);
                    bm2.GetPixel(x, y, out r2, out g2, out b2, out a2);
                    bm3.SetPixel(x, y, (r1 - r2).ToByte(), (g1 - g2).ToByte(), (b1 - b2).ToByte(), (a1 - a2).ToByte());
                }
            }

            bm1.UnlockBitmap();
            bm2.UnlockBitmap();
            bm3.UnlockBitmap();
            return bm3;
        }

        public static Bitmap32 operator +(Bitmap32 bm1, Bitmap32 bm2)
        {
            var width = Math.Min(bm1.Width, bm2.Width);
            var height = Math.Min(bm1.Height, bm2.Height);
            Bitmap bm = new Bitmap(width, height);
            Bitmap32 bm3 = new Bitmap32(bm);
            bm1.LockBitmap();
            bm2.LockBitmap();
            bm3.LockBitmap();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    byte r1, g1, b1, a1, r2, g2, b2, a2;
                    bm1.GetPixel(x, y, out r1, out g1, out b1, out a1);
                    bm2.GetPixel(x, y, out r2, out g2, out b2, out a2);
                    bm3.SetPixel(x, y, (r1 + r2).ToByte(), (g1 + g2).ToByte(), (b1 + b2).ToByte(), (a1 + a2).ToByte());
                }
            }

            bm1.UnlockBitmap();
            bm2.UnlockBitmap();
            bm3.UnlockBitmap();
            return bm3;
        }

        public static Bitmap32 operator *(Bitmap32 bm1, float f)
        {
            var width = bm1.Width;
            var height = bm1.Height;
            Bitmap bm = new Bitmap(width, height);
            Bitmap32 bm2 = new Bitmap32(bm);
            bm1.LockBitmap();
            bm2.LockBitmap();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    byte r1, g1, b1, a1;
                    bm1.GetPixel(x, y, out r1, out g1, out b1, out a1);
                    bm2.SetPixel(x, y, (f * r1).ToByte(), (f * g1).ToByte(), (f * b1).ToByte(), (f * a1).ToByte());
                }
            }

            bm1.UnlockBitmap();
            bm2.UnlockBitmap();
            return bm2;
        }

        public static Bitmap32 operator *(float f, Bitmap32 bm1)
        {
            return bm1 * f;
        }
    }
}