using System;
using System.Text;

namespace NeelabhCoreTools
{
    public class Tools
    {
        // Random values generation --
        public static int RandomDigit()
        {
            return new Random().Next(0, 9);
        }

        public static int RandomNumber(int minValue = int.MinValue, int MaxValue = int.MaxValue)
        {
            return new Random().Next(minValue, MaxValue);
        }

        public static string RandomLowerString(int size)
        {
            var sb = new StringBuilder();

            for (int i = 1; i <= size; i++)
            {
                sb.Append(RandomNumber('a', 'z'));
            }

            return sb.ToString();
        }

        public static string RandomUpperString(int size)
        {
            var sb = new StringBuilder();

            for (int i = 1; i <= size; i++)
            {
                sb.Append(RandomNumber('A', 'Z'));
            }

            return sb.ToString();
        }

        public static string RandomString(int digits = 2, int lowerChars = 3, int upperChars = 3, string specialChars = null)
        {
            var sb = new StringBuilder();

            if (specialChars.IsNotNullOrEmpty())
            {
                sb.Append(specialChars);
            }

            if (digits > 0)
            {
                for (int i = 1; i <= digits; i++)
                {
                    sb.Append(RandomDigit());
                }
            }

            if (lowerChars > 0)
            {
                sb.Append(RandomLowerString(lowerChars));
            }

            if (upperChars > 0)
            {
                sb.Append(RandomUpperString(lowerChars));
            }

            return sb.ToString().Shuffle();
        }
    }
}
