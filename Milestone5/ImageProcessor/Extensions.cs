using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageProcessing
{
    public static class Extensions
    {
        // Save the file with the appropriate format.
        public static void SaveImage(this Image image, string filename)
        {
            string extension = Path.GetExtension(filename);
            switch (extension.ToLower())
            {
                case ".bmp":
                    image.Save(filename, ImageFormat.Bmp);
                    break;
                case ".exif":
                    image.Save(filename, ImageFormat.Exif);
                    break;
                case ".gif":
                    image.Save(filename, ImageFormat.Gif);
                    break;
                case ".jpg":
                case ".jpeg":
                    image.Save(filename, ImageFormat.Jpeg);
                    break;
                case ".png":
                    image.Save(filename, ImageFormat.Png);
                    break;
                case ".tif":
                case ".tiff":
                    image.Save(filename, ImageFormat.Tiff);
                    break;
                default:
                    throw new NotSupportedException(
                        "Unknown file extension " + extension);
            }
        }

        // Rotate an image around its center.
        public static Bitmap RotateAtCenter(this Bitmap bm,
            float angle, Color bgColor, InterpolationMode mode)
        {
            Bitmap result = new(bm.Width, bm.Height);
            using (Graphics graphics = Graphics.FromImage(result))
            {
                graphics.Clear(bgColor);

                graphics.TranslateTransform(-bm.Width / 2, -bm.Height / 2, MatrixOrder.Append);
                graphics.RotateTransform(angle, MatrixOrder.Append);
                graphics.TranslateTransform(bm.Width / 2, bm.Height / 2, MatrixOrder.Append);
                graphics.InterpolationMode = mode;
                graphics.DrawImage(bm, new PointF());
            }
            return result;
        }


        // Scale an image uniformly
        public static Bitmap Scale(this Bitmap bm, float scale, InterpolationMode mode)
        {
            return bm.Scale(scale, scale, mode);
        }

        // Scale an image non-uniformly 
        public static Bitmap Scale(this Bitmap bm, float xscale, float yscale, InterpolationMode mode)
        {
            int width = (int)(xscale * bm.Width);
            int height = (int)(yscale * bm.Height);
            Point[] destinationPoints =
{
                    new Point(0, 0), // top left
                    new Point(width - 1, 0), //top right
                    new Point(0, height - 1) // bottom left
                };

            Bitmap result = new(width, height);
            using (Graphics graphics = Graphics.FromImage(result))
            {
                graphics.InterpolationMode = mode;
                graphics.DrawImage(bm, destinationPoints);
            }

            return result;
        }

        // Crop and image to a desired rectangle
        public static Bitmap Crop(this Image image, Rectangle rect, InterpolationMode mode)
        {
            Bitmap bm = new Bitmap(rect.Width, rect.Height);
            Point[] destinationPoints =
            {
                new Point(0,0),
                new Point(rect.Width - 1, 0),
                new Point(0, rect.Height - 1)
            };

            using (Graphics graphics = Graphics.FromImage(bm))
            {
                graphics.InterpolationMode = mode;
                graphics.DrawImage(image, destinationPoints, rect, GraphicsUnit.Pixel);
            }

            return bm;
        }

        // Draw a dashed rectangle with the given colours
        public static void DrawDashedRectange(
            this Graphics graphics,
            Color color1,
            Color color2,
            float thickness,
            float dashsize,
            Point point1,
            Point point2)
        {
            Rectangle rectangle = point1.ToRectangle(point2);
            using (Pen pen = new Pen(color1, thickness))
            {
                graphics.DrawRectangle(pen, rectangle);
                pen.DashPattern = new float[] { dashsize, dashsize };
                pen.Color = color2;
                graphics.DrawRectangle(pen, rectangle);
            }
        }

        // Make a rectangle from 2 corner points
        public static Rectangle ToRectangle(this Point point1, Point point2)
        {
            int x = Math.Min(point1.X, point2.X);
            int y = Math.Min(point1.Y, point2.Y);
            int width = Math.Abs(point1.X - point2.X);
            int height = Math.Abs(point1.Y - point2.Y);
            return new Rectangle(x, y, width, height);
        }

        // A function that takes as an input four bytes and modifies them.
        public delegate void PointOp(ref byte r, ref byte g, ref byte b, ref byte a);

        // Apply a function to the pixels in an image
        public static void ApplyPointOp(this Bitmap bm, PointOp op)
        {
            Bitmap32 bm32 = new(bm);
            bm32.LockBitmap();

            int height = bm32.Height;
            int width = bm32.Width;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    byte r, g, b, a;
                    bm32.GetPixel(x, y, out r, out g, out b, out a);
                    op(ref r, ref g, ref b, ref a);
                    bm32.SetPixel(x, y, r, g, b, a);
                }
            }
            bm32.UnlockBitmap();
        }

        // Convert a float into a byte keeping
        // its value between 0 and 255.
        public static byte ToByte(this float value)
        {
            if (value < 0) return (byte)0;
            if (value > 255) return (byte)255;
            return (byte)Math.Round(value);
        }

        // Convert a double into a byte keeping
        // its value between 0 and 255.
        public static byte ToByte(this double value)
        {
            if (value < 0) return (byte)0;
            if (value > 255) return (byte)255;
            return (byte)Math.Round(value);
        }

        // Convert an int into a byte keeping
        // its value between 0 and 255.
        public static byte ToByte(this int value)
        {
            if (value < 0) return (byte)0;
            if (value > 255) return (byte)255;
            return (byte)value;
        }

        public static Bitmap ApplyKernel(this Bitmap bm, float[,] kernel, float weight, float offset)
        {
            // check the kernel has odd dimensions
            var kernelRows = kernel.GetUpperBound(0) + 1;
            var kernelColumns = kernel.GetUpperBound(1) + 1;
            if (kernelRows % 2 != 1 || kernelColumns % 2 != 1) throw new ArgumentException("Kernel dimensions must be odd");

            var width = bm.Width;
            var height = bm.Height;
            var resultBm = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(resultBm))
            {
                graphics.Clear(Color.Black);
            }

            var bm32 = new Bitmap32(bm);
            var resultBm32 = new Bitmap32(resultBm);
            bm32.LockBitmap();
            resultBm32.LockBitmap();

            var xRadius = kernelRows / 2;
            var yRadius = kernelColumns / 2;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float rTotal = 0f;
                    float gTotal = 0f;
                    float bTotal = 0f;
                    for (int dx = -xRadius; dx < xRadius; dx++)
                    {
                        for (int dy = -yRadius; dy < yRadius; dy++)
                        {
                            int sourceX = x + dx;
                            if (sourceX < 0) sourceX = 0;
                            else if (sourceX > width) sourceX = width - 1;

                            int sourceY = y + dy;
                            if (sourceY < 0) sourceY = 0;
                            else if (sourceY > height) sourceY = height - 1;

                            byte r, g, b, a;
                            bm32.GetPixel(sourceX, sourceY, out r, out g, out b, out a);
                            var scale = kernel[dy + yRadius, dx + xRadius];
                            rTotal += r * scale;
                            gTotal += g * scale;
                            bTotal += b * scale;
                        }
                    }

                    var newR = (rTotal / weight + offset).ToByte();
                    var newG = (gTotal / weight + offset).ToByte();
                    var newB = (gTotal / weight + offset).ToByte();

                    resultBm32.SetPixel(x, y, newR, newG, newB, 255);
                }
            }

            bm32.UnlockBitmap();
            resultBm32.UnlockBitmap();

            return resultBm;
        }


        private static float[,] OnesArray(int radius)
        {
            var width = 2 * radius + 1;
            float[,] result = new float[width, width];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i, j] = 1f;
                }
            }
            return result;
        }

        public static Bitmap BoxBlur(this Bitmap bm, int radius)
        {
            // Make the kernel.
            float[,] kernel = OnesArray(radius);

            // Apply the kernel.
            int numRows = 2 * radius + 1;
            int numCols = numRows;
            float weight = numRows * numCols;
            float offset = 0;
            return bm.ApplyKernel(kernel, weight, offset);
        }
    }
}
