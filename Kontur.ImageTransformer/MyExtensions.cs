using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer
{
    public static class MyExtensions
    {
        public static int CountBackslashesInString(this string stringToHandle)
        {
            char[] stringToHandleChars = stringToHandle.ToCharArray();
            int slashesCount = 0;
            int length = stringToHandleChars.Length;
            for (int n = length - 1; n >= 0; n--)
            {
                if (stringToHandleChars[n] == '/')
                    slashesCount++;
            }
            return slashesCount;
        }
    }
}
