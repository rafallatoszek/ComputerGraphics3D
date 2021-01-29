using System.Globalization;

namespace gk
{
    public class Converter
    {
        public static float StringToFloat(string s)
        {
            return (float)double.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
        }
    }
}
