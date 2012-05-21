/* Based on ZeroConf
 *
 * see http://zeroconf.codeplex.com for details
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Network
{
    public interface IClientRequest
    {
        void WriteTo(Stream stream);
        byte[] GetBytes();
    }

    public interface IClientRequestWriter : IClientRequest
    {
        void WriteTo(BinaryWriter writer);
    }

    public interface IServerRequest<RequestType>
    {
        RequestType GetRequest(Stream stream);
        RequestType GetRequest(byte[] requestBytes);
    }

    public interface IServerRequestReader<TRequest> : IServerRequest<TRequest>
    {
        TRequest GetRequest(BinaryReader writer);
    }
}
