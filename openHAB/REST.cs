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
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Linq;

namespace openHAB
{
    public class RESTResult
    {
        internal RESTResult(DownloadStringCompletedEventArgs aArgs)
            : this(aArgs, aArgs.Error == null ? aArgs.Result : string.Empty)
        {
        }

        internal RESTResult(UploadStringCompletedEventArgs aArgs)
            : this(aArgs, aArgs.Error == null ? aArgs.Result : string.Empty)
        {
        }

        internal RESTResult(AsyncCompletedEventArgs aArgs, string aResult)
        {
            Cancelled = aArgs.Cancelled;
            Error = aArgs.Error;
            Result = aResult;
        }

        public readonly bool Cancelled;
        public readonly Exception Error;
        public readonly string Result;
    }

    internal class QueryData
    {
        public Uri Uri;
        public string PostData;
        public Action<REST, RESTResult> Handler;
    }

    public class REST : IDisposable
    {
        public REST()
        {
            mWebClient.Headers["Cache-Control"] = "no-cache";

            mWebClient.UploadProgressChanged += OnUploadProgressChanged;
            mWebClient.UploadStringCompleted += OnUploadStringCompleted;

            mWebClient.DownloadProgressChanged += OnDownloadProgressChanged;
            mWebClient.DownloadStringCompleted += OnDownloadStringCompleted;
        }

        void OnUploadProgressChanged(object aSender, UploadProgressChangedEventArgs aArgs)
        {
        }

        void OnDownloadStringCompleted(object aSender, DownloadStringCompletedEventArgs aArgs)
        {
            QueryNextSite();

            var handler = (Action<REST, RESTResult>)aArgs.UserState;

            var result = new RESTResult(aArgs);

            if (result.Error == null && !result.Cancelled && !string.IsNullOrEmpty(aArgs.Result))
                ParseData(XDocument.Parse(aArgs.Result, LoadOptions.None));

            handler(this, result);
        }

        void OnDownloadProgressChanged(object aSender, DownloadProgressChangedEventArgs aArgs)
        {
        }

        void OnUploadStringCompleted(object aSender, UploadStringCompletedEventArgs aArgs)
        {
            QueryNextSite();

            var handler = (Action<REST, RESTResult>)aArgs.UserState;

            var result = new RESTResult(aArgs);

            if (result.Error == null && !result.Cancelled && !string.IsNullOrEmpty(aArgs.Result))
                ParseData(XDocument.Parse(aArgs.Result, LoadOptions.None));

            handler(this, result);
        }

        public bool IsBusy
        {
            get
            {
                return mQueryQueue.Count > 0 || mWebClient.IsBusy;
            }
        }

        private readonly WebClient mWebClient = new WebClient();
        private readonly Object mLock = new object();
        private readonly Queue<QueryData> mQueryQueue = new Queue<QueryData>();

        internal void QuerySite(string aSite, Action<REST, RESTResult> aHandler)
        {
            QuerySite(aSite, aHandler, string.Empty);
        }

        private int mQueryId = 0;
        internal Uri GetUri(string aSite)
        {
            var ub = new UriBuilder(aSite);

            ub.Query = "?q=" + mQueryId++;

            return ub.Uri;
        }

        internal void QuerySite(string aSite, Action<REST, RESTResult> aHandler, string aPostData)
        {
            lock (mLock)
            {
                if (mWebClient.IsBusy)
                {
                    mQueryQueue.Enqueue(new QueryData
                    {
                        Uri = string.IsNullOrEmpty(aPostData) ? GetUri(aSite) : new Uri(aSite),
                        Handler = aHandler,
                        PostData = aPostData
                    });
                }
                else
                {
                    if (string.IsNullOrEmpty(aPostData))
                        mWebClient.DownloadStringAsync(GetUri(aSite), aHandler);
                    else
                        mWebClient.UploadStringAsync(new Uri(aSite), "POST", aPostData, aHandler);
                }
            }
        }

        private void QueryNextSite()
        {
            lock (mLock)
            {
                if (!mWebClient.IsBusy)
                {
                    if (mQueryQueue.Count > 0)
                    {
                        var data = mQueryQueue.Dequeue();

                        if (string.IsNullOrEmpty(data.PostData))
                            mWebClient.DownloadStringAsync(data.Uri, data.Handler);
                        else
                            mWebClient.UploadStringAsync(data.Uri, "POST", data.PostData, data.Handler);
                    }
                }
            }
        }

        private void ParseData(XDocument aDocument)
        {
            switch(aDocument.Root.Name.LocalName)
            {
                case "openhab":
                    ParseOpenHAB(aDocument.Root);
                    break;

                case "sitemaps":
                    ParseSitemaps(aDocument.Root);
                    break;

                case "sitemap":
                    ParseSitemap(aDocument.Root);
                    break;

                case "page":
                    ParsePage(aDocument.Root);
                    break;

                case "item":
                    ParseItem(aDocument.Root);
                    break;

                default:
                    throw new ArgumentException("Value not valid");
            }
        }

        private void ParseOpenHAB(XElement aRoot)
        {
            foreach (var link in aRoot.Elements("link"))
            {
                var type = link.Attribute("type").Value;
                var uri = link.Value;

                Links[type] = new Uri(uri);
            }
        }

        private void ParseSitemaps(XElement aRoot)
        {
            Sitemaps.Clear();

            foreach (var sitemap in aRoot.Elements("sitemap"))
            {
                var name = sitemap.Element("name").Value;
                var link = sitemap.Element("link").Value;
                var homepage = sitemap.Element("homepage").Element("link").Value;

                Sitemaps[name] = new Sitemap(name, new Uri(link), new Uri(homepage));
            }
        }

        private void ParseSitemap(XElement aRoot)
        {
            var name = aRoot.Element("name").Value;
            var link = aRoot.Element("link").Value;
            var homepage = aRoot.Element("homepage").Element("link").Value;

            var sitemap = Sitemaps[name];

            if (sitemap.Name != name)
                throw new FormatException("name");
            if (sitemap.Link.AbsoluteUri != link)
                throw new FormatException("link");
            if (sitemap.Homepage.AbsoluteUri != homepage)
                throw new FormatException("homepage");

            sitemap.Page = ParsePage(sitemap, aRoot.Element("homepage"));
        }

        private void UpdateItem(Item aItem, XElement aRoot)
        {
            aItem.mState = aRoot.Element("state").Value;
        }

        private void UpdateWidget(Widget aWidget, XElement aRoot)
        {
            aWidget.Label = aRoot.Element("label").Value;
            aWidget.Icon = aRoot.Element("icon").Value;

            if (aWidget.Type == Widget.WidgetType.Image)
                aWidget.Url = new Uri(new UriBuilder(aWidget.Url.Scheme, aWidget.Url.Host, aWidget.Url.Port).Uri, aRoot.Element("url").Value);

            if (aWidget.Type == Widget.WidgetType.Chart)
            {
                aWidget.Period = aRoot.Element("period").Value;
                aWidget.Refresh = int.Parse(aRoot.Element("refresh").Value);
            }

            if (aWidget.Type == Widget.WidgetType.Setpoint)
            {
                aWidget.MinValue = double.Parse(aRoot.Element("minValue").Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                aWidget.MaxValue = double.Parse(aRoot.Element("maxValue").Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                aWidget.Step = double.Parse(aRoot.Element("step").Value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }

            if (aWidget.Type == Widget.WidgetType.Video)
                aWidget.Url = new Uri(new UriBuilder(aWidget.Url.Scheme, aWidget.Url.Host, aWidget.Url.Port).Uri, aRoot.Element("url").Value);

            if (aWidget.Type == Widget.WidgetType.Webview)
                aWidget.Url = new Uri(new UriBuilder(aWidget.Url.Scheme, aWidget.Url.Host, aWidget.Url.Port).Uri, aRoot.Element("url").Value);

            var item = aRoot.Element("item");
            if (item != null)
            {
                UpdateItem(aWidget.Item, item);
            }

            var widgets = aRoot.Elements("widget").ToArray();

            for (var idx = 0; idx < widgets.Length; ++idx)
                UpdateWidget(aWidget.Widgets[idx], widgets[idx]);
        }

        private Page ParsePage(XElement aRoot)
        {
            var id = aRoot.Element("id").Value;

            var page = Pages[id];

            var widgets = aRoot.Elements("widget").ToArray();

            for(var idx=0;idx<widgets.Length;++idx)
                UpdateWidget(page.Widgets[idx], widgets[idx]);

            return page;
        }

        private Page ParsePage(Sitemap aSitemap, XElement aRoot)
        {
            var page = new Page(this);

            var id = aRoot.Element("id").Value;
            var title = aRoot.Element("title").Value;
            var link = aRoot.Element("link").Value;

            page.Id = id;
            page.Title = title;
            page.Link = new Uri(link);

            foreach (var widget in aRoot.Elements("widget"))
            {
                page.mWidgets.Add(ParseWidget(aSitemap, widget));
            }

            aSitemap.mPages[id] = page;
            Pages[id] = page;

            return page;
        }

        private Widget ParseWidget(Sitemap aSitemap, XElement aRoot)
        {
            var type = aRoot.Element("type").Value;
            var label = aRoot.Element("label").Value;
            var icon = aRoot.Element("icon").Value;

            var widget = new Widget(type, label, icon);

            if (widget.Type == Widget.WidgetType.Image)
                widget.Url = new Uri(new UriBuilder(aSitemap.Link.Scheme, aSitemap.Link.Host, aSitemap.Link.Port).Uri, aRoot.Element("url").Value);

            if (widget.Type == Widget.WidgetType.Chart)
            {
                widget.Period = aRoot.Element("period").Value;
                widget.Refresh = int.Parse(aRoot.Element("refresh").Value);
            }

            if (widget.Type == Widget.WidgetType.Setpoint)
            {
                widget.MinValue = double.Parse( aRoot.Element("minValue").Value,System.Globalization.NumberFormatInfo.InvariantInfo);
                widget.MaxValue = double.Parse( aRoot.Element("maxValue").Value,System.Globalization.NumberFormatInfo.InvariantInfo);
                widget.Step = double.Parse( aRoot.Element("step").Value,System.Globalization.NumberFormatInfo.InvariantInfo);
            }

            if (widget.Type == Widget.WidgetType.Video)
                widget.Url = new Uri(new UriBuilder(aSitemap.Link.Scheme, aSitemap.Link.Host, aSitemap.Link.Port).Uri, aRoot.Element("url").Value);

            if (widget.Type == Widget.WidgetType.Webview)
                widget.Url = new Uri(new UriBuilder(aSitemap.Link.Scheme, aSitemap.Link.Host, aSitemap.Link.Port).Uri, aRoot.Element("url").Value);

            if (widget.Type == Widget.WidgetType.Switch || widget.Type == Widget.WidgetType.Selection)
            {
                var mappings = aRoot.Elements("mapping");

                foreach(var mapping in mappings)
                {
                    var map_command = mapping.Element("command").Value;
                    var map_label = mapping.Element("label").Value;

                    widget.Mappings[map_label] = map_command;
                }
            }

            foreach (var child in aRoot.Elements("widget"))
            {
                widget.mWidgets.Add(ParseWidget(aSitemap, child));
            }

            var item = aRoot.Element("item");
            if (item != null)
            {
                widget.Item = ParseItem(aSitemap, item);
            }

            var linkedPage = aRoot.Element("linkedPage");
            if (linkedPage != null)
            {
                widget.LinkedPage = ParsePage(aSitemap, linkedPage);
            }
            return widget;
        }

        private Item ParseItem(XElement aRoot)
        {
            var name = aRoot.Element("name").Value;
            var state = aRoot.Element("state").Value;

            var item = Items[name];

            item.mState = state;

            return item;
        }

        private Item ParseItem(Sitemap aSitemap, XElement aRoot)
        {
            var type = aRoot.Element("type").Value;
            var name = aRoot.Element("name").Value;
            var state = aRoot.Element("state").Value;
            var link = aRoot.Element("link").Value;

            Item item;

            if (!Items.TryGetValue(name, out item))
                item = new Item(this, type, name, state, link);

            aSitemap.mItems[name] = item;
            Items[name] = item;
            
            return item;
        }

        public readonly Dictionary<string, Uri> Links = new Dictionary<string, Uri>();
        public readonly Dictionary<string, Item> Items = new Dictionary<string, Item>();
        public readonly Dictionary<string, Page> Pages = new Dictionary<string, Page>();
        public readonly Dictionary<string, Sitemap> Sitemaps = new Dictionary<string, Sitemap>();

        public void GetREST(Uri aServer, Action<REST, RESTResult> aCallback)
        {
            GetREST(aServer.AbsoluteUri, aCallback);
        }

        public void GetREST(string aServer, Action<REST, RESTResult> aCallback)
        {
            QuerySite(aServer, aCallback);
        }

        public void GetSitemaps(Action<REST, RESTResult> aCallback)
        {
            QuerySite(Links["sitemaps"].AbsoluteUri, aCallback);
        }

        public void GetSitemap(Sitemap aSitemap, Action<REST, RESTResult> aCallback)
        {
            QuerySite(aSitemap.Link.AbsoluteUri, aCallback);
        }

        public void Dispose()
        {
            if (mWebClient.IsBusy)
                mWebClient.CancelAsync();
        }

        public static REST FromDataStore(byte[] aData)
        {
            using (var ms = new MemoryStream(aData))
            {
                using (var reader = new BinaryReader(ms))
                {
                    var result = new REST();

                    result.mQueryId = reader.ReadInt32();

                    var count = reader.ReadInt32();
                    for (var idx = 0; idx < count; ++idx)
                    {
                        var k = reader.ReadString();
                        var v = reader.ReadString();

                        result.Links[k] = new Uri(v);
                    }

                    count = reader.ReadInt32();
                    for (var idx = 0; idx < count; ++idx)
                    {
                        var k = reader.ReadString();

                        result.Items[k] = new Item(result, reader);
                    }

                    count = reader.ReadInt32();
                    for (var idx = 0; idx < count; ++idx)
                    {
                        var k = reader.ReadString();
                        result.Pages[k] = new Page(result);
                    }

                    count = reader.ReadInt32();
                    for (var idx = 0; idx < count; ++idx)
                    {
                        var k = reader.ReadString();

                        result.Pages[k].FromStream(reader);
                    }

                    count = reader.ReadInt32();
                    for (var idx = 0; idx < count; ++idx)
                    {
                        var k = reader.ReadString();
                        result.Sitemaps[k] = new Sitemap(result, reader);
                    }


                    return result;
                }
            }
        }

        public byte[] GetDataStore()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(mQueryId);

                    writer.Write(Links.Keys.Count);
                    foreach (var link in Links)
                    {
                        writer.Write(link.Key);
                        writer.Write(link.Value.AbsoluteUri);
                    }

                    writer.Write(Items.Keys.Count);
                    foreach (var item in Items)
                    {
                        writer.Write(item.Key);
                        item.Value.Write(writer);
                    }

                    writer.Write(Pages.Keys.Count);
                    foreach (var page in Pages)
                    {
                        writer.Write(page.Key);
                    }

                    writer.Write(Pages.Keys.Count);
                    foreach (var page in Pages)
                    {
                        writer.Write(page.Key);
                        page.Value.Write(writer);
                    }

                    writer.Write(Sitemaps.Keys.Count);
                    foreach (var sitemap in Sitemaps)
                    {
                        writer.Write(sitemap.Key);
                        sitemap.Value.Write(writer);
                    }
                }
                return ms.ToArray();
            }
        }
    }
}
