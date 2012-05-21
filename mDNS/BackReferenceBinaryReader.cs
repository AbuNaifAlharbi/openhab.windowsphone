/* Based on ZeroConf
 *
 * see http://zeroconf.codeplex.com for details
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Network
{
    class BackReferenceBinaryReader : BinaryReader
    {
        public BackReferenceBinaryReader(Stream input)
            : base(input)
        {

        }

        public BackReferenceBinaryReader(Stream input, Encoding encoding)
            : base(input, encoding)
        {

        }

        Dictionary<int, object> registeredElements = new Dictionary<int, object>();

        public T Get<T>(int p)
        {
            return (T)registeredElements[p];
        }

        public void Register(int p, object value)
        {
            registeredElements.Add(p,value);
        }
    }
}
