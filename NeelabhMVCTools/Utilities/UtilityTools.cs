using System;

namespace NeelabhMVCTools.Utilities
{
    public class UtilityTools
    {
        public static string NewGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public static string RandomString(int Length = 6)
        {
            string Result = string.Empty;
            char[] chars = "1234567890abcdefghijklmnopqrstuvwxyz".ToCharArray();
            Random random;
            for (int i = 0; i < Length; i++)
            {
                random = new Random();
                int x = random.Next(1, chars.Length);
                if (!Result.Contains(chars.GetValue(x).ToString())) Result += chars.GetValue(x);
                else i--;
            }
            return Result;
        }

        public static string RandomNumber(int Length = 6)
        {
            string Result = string.Empty;
            char[] chars = "023456789".ToCharArray();
            Random random;
            for (int i = 0; i < Length; i++)
            {
                random = new Random();
                int x = random.Next(1, chars.Length);
                //---Don't Allow Repetition of Characters
                if (!Result.Contains(chars.GetValue(x).ToString())) Result += chars.GetValue(x);
                else i--;
            }
            return Result;
        }
    }
}
