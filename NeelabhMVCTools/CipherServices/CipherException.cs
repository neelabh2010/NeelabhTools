using System;

namespace NeelabhMVCTools.CipherServices
{
    // Use to catch Cipher Exception --
    public class CipherException : Exception
    {
        public CipherException(string Message) : base(Message)
        {

        }
    }
}
