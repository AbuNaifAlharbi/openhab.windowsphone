/* openHAB, the open Home Automation Bus.
 * Copyright (C) 2010-${year}, openHAB.org <admin@openhab.org>
 * 
 * See the contributors.txt file in the distribution for a
 * full listing of individual contributors.
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as
 * published by the Free Software Foundation; either version 3 of the
 * License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.gnu.org/licenses>.
 * 
 * Additional permission under GNU GPL version 3 section 7
 * 
 * If you modify this Program, or any covered work, by linking or 
 * combining it with Eclipse (or a modified version of that library),
 * containing parts covered by the terms of the Eclipse Public License
 * (EPL), the licensors of this Program grant you additional permission
 * to convey the resulting work.
 */
 
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace openHAB
{
    public class RESTUpdater : IDisposable
    {
        public enum UpdateStrategy
        {
            Normal,
            Polling,
            LongPolling,
            Streaming
        };

        public void Dispose()
        {
            if(mSocket != null)
                mSocket.Dispose();
            mSocket = null;
        }

        public bool Connect(string aUri)
        {
            return Connect(aUri, UpdateStrategy.Streaming);
        }
        public bool Connect(string aUri, UpdateStrategy aUpdateStrategy)
        {
            return Connect(new Uri(aUri), aUpdateStrategy);
        }

        public bool Connect(Uri aUri)
        {
            return Connect(aUri, UpdateStrategy.Streaming);
        }
        public bool Connect(Uri aUri, UpdateStrategy aUpdateStrategy)
        {
            if (IsConnected)
                throw new InvalidOperationException("Already connected.");

            mUpdateStrategy = aUpdateStrategy;

            var args = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = new DnsEndPoint(aUri.Host,aUri.Port),
                UserToken = aUri
            };

            args.Completed += OnConnected;

            return Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, args);
        }

        public bool IsConnected
        {
            get { return mSocket != null && mSocket.Connected; }
        }

        private void OnConnected(object aSender, SocketAsyncEventArgs aArgs)
        {
            var token = aArgs.UserToken;

            if (!CheckSocketError(aArgs.SocketError))
                return;

            mSocket = aArgs.ConnectSocket;

            aArgs.Dispose();

            if (!mSocket.Connected)
            {
                if (ConnectionError != null)
                    ConnectionError(SocketError.NotConnected);

                return;
            }

            var uri = (Uri)aArgs.UserToken;

            var request =
                string.Format("GET {0} HTTP/1.1", uri.AbsolutePath) + Environment.NewLine +
                string.Format("Host: {0}", uri.Host) + Environment.NewLine +
                "Accept: application/x-javascript" + Environment.NewLine +
                "Connection: Keep-Alive" + Environment.NewLine +
                "Accept-Charset: utf-8" + Environment.NewLine +
                "Cache-Control: no-cache" + Environment.NewLine +
                "User-Agent: openHABWP7" + Environment.NewLine;

            switch (mUpdateStrategy)
            {
                case UpdateStrategy.Normal:
                    break;

                case UpdateStrategy.Polling:
                    request += "X-Atmosphere-Transport: polling" + Environment.NewLine;
                    break;

                case UpdateStrategy.LongPolling:
                    request += "X-Atmosphere-Transport: long-polling" + Environment.NewLine;
                    break;

                case UpdateStrategy.Streaming:
                    request += "X-Atmosphere-Transport: streaming" + Environment.NewLine;
                    break;
            }
            
            request += Environment.NewLine;

            var reqBytes = Encoding.UTF8.GetBytes(request);

            var args = new SocketAsyncEventArgs()
            {
                UserToken = token
            };

            args.Completed += OnRequestSent;
            args.SetBuffer(reqBytes, 0, reqBytes.Length);

            if (!mSocket.SendAsync(args))
                CheckSocketError(SocketError.SocketError);
        }

        private void OnRequestSent(object aSender, SocketAsyncEventArgs aArgs)
        {
            var token = aArgs.UserToken;

            if (!CheckSocketError(aArgs.SocketError))
                return;

            aArgs.Dispose();

            var args = new SocketAsyncEventArgs()
            {
                UserToken = token
            };

            args.Completed += OnReceived;
            args.SetBuffer(mReceiveBuffer, 0, mReceiveBuffer.Length);

            if (!mSocket.ReceiveAsync(args))
                CheckSocketError(SocketError.SocketError);
        }

        private void OnReceived(object aSender, SocketAsyncEventArgs aArgs)
        {
            var token = aArgs.UserToken;
            
            if (!CheckSocketError(aArgs.SocketError))
                return;

            var str = Encoding.UTF8.GetString(aArgs.Buffer, 0, aArgs.BytesTransferred);

            mReceivedData += str;

            int idx;
            while ((idx = mReceivedData.IndexOf(Environment.NewLine)) > -1)
            {
                var line = mReceivedData.Substring(0, idx);
                ParseResponseLine(line);
                mReceivedData = mReceivedData.Substring(idx + Environment.NewLine.Length);
            }

            if (!mSocket.ReceiveAsync(aArgs))
                CheckSocketError(SocketError.SocketError);
        }

        private bool CheckSocketError(SocketError aError)
        {
            if (aError != SocketError.Success)
            {
                if (ConnectionError != null)
                    ConnectionError(aError);
            }

            return aError == SocketError.Success;
        }

        public event Action<SocketError> ConnectionError;
        public event Action<string> UpdateReceived;

        private UpdateStrategy mUpdateStrategy;

        private Socket mSocket;
        private readonly byte[] mReceiveBuffer = new byte[8192];
        private string mReceivedData;

        private bool mResponseStatusReceived;
        private float mResponseStatus;
        private bool mInResponseData;
        private int mResponseChunkSize;
        private string mResponseData;
        private void ParseResponseLine(string aLine)
        {
            if (!mResponseStatusReceived)
            {
                if (!aLine.StartsWith("HTTP/1.1"))
                    throw new Exception();

                var parts = aLine.Split(' ');

                mResponseStatus = float.Parse(parts[1], System.Globalization.NumberFormatInfo.InvariantInfo);

                mResponseStatusReceived = true;
            }
            else
            {
                if (mInResponseData)
                {
                    if (mResponseChunkSize == -1)
                    {
                        mResponseChunkSize = int.Parse(aLine, System.Globalization.NumberStyles.HexNumber);
                        mResponseData = string.Empty;

                        if (mResponseChunkSize == 0)
                        {
                            // merely a keep-alive
                            mResponseChunkSize = -1;
                        }
                    }
                    else
                    {
                        mResponseData += aLine;
                        mResponseChunkSize -= aLine.Length;

                        if (mResponseChunkSize < 0)
                        {
                            if (UpdateReceived != null)
                                UpdateReceived(mResponseData);
                            mResponseData = string.Empty;
                            mResponseChunkSize = int.Parse(aLine, System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (mResponseChunkSize == 0)
                        {
                            if (UpdateReceived != null)
                                UpdateReceived(mResponseData);
                            mResponseData = string.Empty;
                            mResponseChunkSize = -1;
                        }
                    }
                }
                else
                {
                    if (aLine.StartsWith("Transfer-Encoding: ") && aLine != "Transfer-Encoding: chunked")
                        throw new Exception("only chunked encoding is supported");

                    if (string.IsNullOrEmpty(aLine))
                    {
                        mInResponseData = true;
                        mResponseChunkSize = -1;
                    }
                }
            }
        }
    }
}
