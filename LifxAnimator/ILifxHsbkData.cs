using SixLabors.ImageSharp.PixelFormats;
using System;

namespace LifxAnimator
{
    public interface ILifxHsbkData
    {
        UInt16 Hue { get; set; }
        UInt16 Saturation { get; set; }
        UInt16 Brightness { get; set; }
        UInt16 Kelvin { get; set; }
    }

    public static class ILifxHsbkDataExtensions
    {
        public static float GetHueDegrees(this ILifxHsbkData data)
            => data.Hue / (float)UInt16.MaxValue * 360f;

        public static T SetHueDegrees<T>(this T data, float value)
            where T : ILifxHsbkData
        {
            data.Hue = (UInt16)(value / 360f * UInt16.MaxValue);
            return data;
        }

        public static float GetSaturationPercent(this ILifxHsbkData data)
            => data.Saturation / (float)UInt16.MaxValue;

        public static T SetSaturationPercent<T>(this T data, float value)
            where T : ILifxHsbkData
        {
            data.Saturation = (UInt16)(value * UInt16.MaxValue);
            return data;
        }

        public static float GetBrightnessPercent(this ILifxHsbkData data)
            => data.Brightness / (float)UInt16.MaxValue;

        public static T SetBrightnessPercent<T>(this T data, float value)
            where T : ILifxHsbkData
        {
            data.Brightness = (UInt16)(value * UInt16.MaxValue);
            return data;
        }

        public static T SetRgb24<T>(this T data, Rgb24 color)
            where T : ILifxHsbkData
        {
            // Reference: https://en.wikipedia.org/wiki/HSL_and_HSV#Conversion_RGB_to_HSV_used_commonly_in_software_programming
            byte rgbMax = Math.Max(color.R, Math.Max(color.G, color.B));
            byte rgbMin = Math.Min(color.R, Math.Min(color.G, color.B));

            float hue = rgbMax == rgbMin ? 0f
                : rgbMax == color.R ? 60f * (0f + (color.G - color.B) / (float)(rgbMax - rgbMin))
                : rgbMax == color.G ? 60f * (2f + (color.B - color.R) / (float)(rgbMax - rgbMin))
                : /* rgbMax == color.B ? */ 60f * (4f + (color.R - color.G) / (float)(rgbMax - rgbMin));
            if (hue < 0) { hue += 360; }
            float saturation = rgbMax == 0 ? 0f : (rgbMax - rgbMin) / (float)rgbMax;
            float brightness = rgbMax / (float)byte.MaxValue;

            return data
                .SetHueDegrees(hue)
                .SetSaturationPercent(saturation)
                .SetBrightnessPercent(brightness);
        }

        public static Rgb24 GetRgb24(this ILifxHsbkData data)
        {
            // Reference: https://en.wikipedia.org/wiki/HSL_and_HSV#From_HSV
            float hue = data.GetHueDegrees();
            float saturation = data.GetSaturationPercent();
            float brightness = data.GetBrightnessPercent();

            float chroma = brightness * saturation;
            float huePrime = hue / 60f;
            float x = chroma * (1f - Math.Abs(huePrime % 2 - 1));
            var rgb1 = huePrime <= 1 ? (chroma, x, 0f)
                : huePrime <= 2 ? (x, chroma, 0f)
                : huePrime <= 3 ? (0f, chroma, x)
                : huePrime <= 4 ? (0f, x, chroma)
                : huePrime <= 5 ? (x, 0f, chroma)
                : /* huePrime <= 6 */ (chroma, 0f, x);
            float m = brightness - chroma;

            return new Rgb24(
                r: (byte)Math.Round((rgb1.Item1 + m) * byte.MaxValue, 0),
                g: (byte)Math.Round((rgb1.Item2 + m) * byte.MaxValue, 0),
                b: (byte)Math.Round((rgb1.Item3 + m) * byte.MaxValue, 0));
        }
    }
}
