using ImageProcessing;
using System.Diagnostics;
using System.Numerics;

namespace ImageProcessing
{
    public class Converters
    {
        /// <summary>
        /// Converts an HSL color value to RGB.
        /// </summary>
        /// <returns>RGBA Color. Ranges [0, 255]</returns>
        public static void HslToRgba(
            double hue, double saturation, double lightness, double hslAlpha,
            out byte red, out byte green, out byte blue, out byte alpha)
        {
            double r, g, b;

            if (saturation == 0.0f)
                r = g = b = lightness;

            else
            {
                var q = lightness < 0.5f ? lightness * (1.0f + saturation) : lightness + saturation -lightness * saturation;
                var p = 2.0f * lightness - q;
                r = HueToRgb(p, q, hue + 1.0f / 3.0f);
                g = HueToRgb(p, q, hue);
                b = HueToRgb(p, q,hue - 1.0f / 3.0f);
            }

            red = (r * 255).ToByte();
            green = (g * 255).ToByte();
            blue = (b * 255).ToByte();
            alpha = (hslAlpha * 255).ToByte();
        }

        // Helper for HslToRgba
        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0.0f) t += 1.0f;
            if (t > 1.0f) t -= 1.0f;
            if (t < 1.0f / 6.0f) return p + (q - p) * 6.0d * t;
            if (t < 1.0f / 2.0f) return q;
            if (t < 2.0f / 3.0f) return p + (q - p) * (2.0d / 3.0d - t) * 6.0d;
            return p;
        }

        /// <summary>
        /// Converts an RGB color value to HSL.
        /// </summary>
        /// <param name="rgba"></param>
        /// <returns></returns>
        public static void RgbaToHsl(
             byte red,  byte green,  byte blue,  byte alpha,
             out double hue, out double saturation, out double lightness, out double hslAlpha)
        {
            float r = red / 255.0f;
            float g = green / 255.0f;
            float b = blue / 255.0f;

            float max = (r > g && r > b) ? r : (g > b) ? g : b;
            float min = (r < g && r < b) ? r : (g < b) ? g : b;

            float h, s, l;
            h = s = l = (max + min) / 2.0f;

            if (max == min)
                h = s = 0.0f;

            else
            {
                float d = max - min;
                s = (l > 0.5f) ? d / (2.0f - max - min) : d / (max + min);

                if (r > g && r > b)
                    h = (g - b) / d + (g < b ? 6.0f : 0.0f);

                else if (g > b)
                    h = (b - r) / d + 2.0f;

                else
                    h = (r - g) / d + 4.0f;

                h /= 6.0f;
            }

            hue = h;
            saturation = s;
            lightness = l;
            hslAlpha = alpha / 255.0f;
        }
    }
}
