using System;
using System.Collections;
using System.Linq;
using ServoLink.Contracts;

namespace ServoLink
{
    public class BinaryHelper : IBinaryHelper
    {
        public byte[] ConvertToByteArray(params object[] data)
        {
            var ms = new System.IO.MemoryStream();
            var bw = new System.IO.BinaryWriter(ms);
            foreach (var dataItem in data)
            {
                if (dataItem is IEnumerable)
                {
                    var arr = dataItem as IEnumerable;
                    bw.Write(ConvertToByteArray(arr.Cast<object>().ToArray()));
                }
                else if (dataItem is char || dataItem is byte)
                {
                    bw.Write(Convert.ToByte(dataItem));
                }
                else if (dataItem is short || dataItem is ushort)
                {
                    bw.Write(Convert.ToUInt16(dataItem));
                }
                else if (dataItem is int || dataItem is uint)
                {
                    bw.Write(Convert.ToUInt32(dataItem));
                }
                else if (dataItem is long || dataItem is ulong)
                {
                    bw.Write(Convert.ToUInt64(dataItem));
                }
                else if (dataItem is decimal)
                {
                    bw.Write((decimal)dataItem);
                }
                else if (dataItem is double)
                {
                    bw.Write((double)dataItem);
                }
                else if (dataItem is float)
                {
                    bw.Write((float)dataItem);
                }
            }
            bw.Flush();
            return ms.ToArray();
        }
    }
}
