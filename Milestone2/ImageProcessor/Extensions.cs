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
    }
}
