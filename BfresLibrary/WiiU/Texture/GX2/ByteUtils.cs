using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BfresLibrary.Swizzling
{
    public class ByteUtils
    {
        public static byte[] CombineArray(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        public static byte[] SubArray(byte[] data, uint offset) {
            return data.Skip((int)offset).Take((int)(data.Length - offset)).ToArray();
        }

        public static byte[] SubArray(byte[] data, int offset, int length) {
            return SubArray(data, (uint)offset, (uint)length);
        }

        public static byte[] SubArray(byte[] data, uint offset, uint length)
        {
            return data.Skip((int)offset).Take((int)length).ToArray();
        }
    }
}
