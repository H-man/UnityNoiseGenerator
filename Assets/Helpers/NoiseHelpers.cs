using UnityEngine;

namespace Assets.Helpers
{
    public static class NoiseHelpers  {
        public static Texture2D NormalMap(this Texture2D source, float strength)
        {
            strength = Mathf.Clamp(strength, 0.0F, 10.0F);
            float xLeft;
            float xRight;
            float yUp;
            float yDown;
            float yDelta;
            float xDelta;
            var result = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);
            for (var by = 0; by < result.height; by++)
            {
                for (var bx = 0; bx < result.width; bx++)
                {
                    var bxL = bx - 1;
                    var bxR = bx + 1;
                    var byU = by - 1;
                    var byD = by + 1;
                    xLeft = source.GetPixel(bxR, by).grayscale * strength;
                    xRight = source.GetPixel(bxL, by).grayscale * strength;
                    yUp = source.GetPixel(bx, byU).grayscale * strength;
                    yDown = source.GetPixel(bx, byD).grayscale * strength;
                    xDelta = ((xLeft - xRight) + 1) * 0.5f;
                    yDelta = ((yUp - yDown) + 1) * 0.5f;
                    result.SetPixel(bx, by, new Color(xDelta, yDelta, 1.0f, yDelta));
                }
            }
            result.Apply();
            return result;
        }
    }

    public class Blur
    {
        private float _avgR = 0;
        private float _avgG = 0;
        private float _avgB = 0;
        private float _avgA = 0;
        private float _blurPixelCount = 0;

        public Texture2D FastBlur(Texture2D image, int radius, int iterations)
        {
            var tex = image;
            for (var i = 0; i < iterations; i++)
            {
                tex = BlurImage(tex, radius, true);
                tex = BlurImage(tex, radius, false);
            }
            return tex;
        }



        Texture2D BlurImage(Texture2D image, int blurSize, bool horizontal)
        {

            var blurred = new Texture2D(image.width, image.height, TextureFormat.ARGB32, true);
            var w = image.width;
            var h = image.height;
            int xx, yy, x, y;

            if (horizontal)
            {
                for (yy = 0; yy < h; yy++)
                {
                    for (xx = 0; xx < w; xx++)
                    {
                        ResetPixel();

                        //Right side of pixel
                        for (x = xx; (x < xx + blurSize && x < w); x++)
                        {
                            AddPixel(image.GetPixel(x, yy));
                        }

                        //Left side of pixel
                        for (x = xx; (x > xx - blurSize && x > 0); x--)
                        {
                            AddPixel(image.GetPixel(x, yy));

                        }

                        CalcPixel();

                        for (x = xx; x < xx + blurSize && x < w; x++)
                        {
                            blurred.SetPixel(x, yy, new Color(_avgR, _avgG, _avgB, 1.0f));

                        }
                    }
                }
            }

            else
            {
                for (xx = 0; xx < w; xx++)
                {
                    for (yy = 0; yy < h; yy++)
                    {
                        ResetPixel();

                        //Over pixel
                        for (y = yy; (y < yy + blurSize && y < h); y++)
                        {
                            AddPixel(image.GetPixel(xx, y));
                        }
                        //Under pixel
                        for (y = yy; (y > yy - blurSize && y > 0); y--)
                        {
                            AddPixel(image.GetPixel(xx, y));
                        }
                        CalcPixel();
                        for (y = yy; y < yy + blurSize && y < h; y++)
                        {
                            blurred.SetPixel(xx, y, new Color(_avgR, _avgG, _avgB, 1.0f));

                        }
                    }
                }
            }

            blurred.Apply();
            return blurred;
        }
        void AddPixel(Color pixel)
        {
            _avgR += pixel.r;
            _avgG += pixel.g;
            _avgB += pixel.b;
            _blurPixelCount++;
        }

        void ResetPixel()
        {
            _avgR = 0.0f;
            _avgG = 0.0f;
            _avgB = 0.0f;
            _blurPixelCount = 0;
        }

        void CalcPixel()
        {
            _avgR = _avgR / _blurPixelCount;
            _avgG = _avgG / _blurPixelCount;
            _avgB = _avgB / _blurPixelCount;
        }
    }
}
