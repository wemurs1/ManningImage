namespace ImageProcessor
{
    internal struct PixelData
    {
        public byte R, G, B, A;

        public PixelData(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public int Brightness
        {
            get
            {
                return R + G + B;
            }
        }
    }
}
