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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace openHAB
{
    public class Page
    {
        [Flags]
        public enum ChangeFlags
        {
            None = 0,
            Id = 1<<0,
            Title = 1<<1,
            Link = 1<<2,
            Widgets = 1 << 3,
            WidgetData = 1 << 3,
        };

        internal Page(REST aRest)
        {
            mRest = aRest;
        }

        private readonly REST mRest;

        public string Id { get; internal set; }
        public string Title { get; internal set; }
        public Uri Link { get; internal set; }

        internal readonly List<Widget> mWidgets = new List<Widget>();
        public ReadOnlyCollection<Widget> Widgets
        {
            get
            {
                return mWidgets.AsReadOnly();
            }
        }

        private bool mPending;
        public void Update(Action<REST, Page> aCallback)
        {
            if (!mPending)
            {
                mPending = true;
                mRest.QuerySite(Link.AbsoluteUri, (rest, result) =>
                    {
                        mPending = false;
                        aCallback(rest, this);
                    });
            }
        }

        internal void Write(System.IO.BinaryWriter aWriter)
        {
            aWriter.Write(Id);
            aWriter.Write(Title);
            aWriter.Write(Link.AbsoluteUri);

            aWriter.Write(mWidgets.Count);
            foreach(var widget in mWidgets)
                widget.Write(aWriter);
        }

        internal void FromStream(System.IO.BinaryReader aReader)
        {
            Id = aReader.ReadString();
            Title = aReader.ReadString();
            Link = new Uri(aReader.ReadString());

            var count = aReader.ReadInt32();
            for (var idx = 0; idx < count; ++idx)
            {
                mWidgets.Add(new Widget(mRest, aReader));
            }
        }

        private RESTUpdater mRESTUpdater;
        private Action<REST, Page, ChangeFlags> mRESTUpdaterCallback;
        public void StartUpdateNotification(Action<REST, Page,ChangeFlags> aCallback)
        {
            if (mRESTUpdater != null)
                throw new InvalidOperationException("Notification already started");

            mRESTUpdater = new RESTUpdater();
            mRESTUpdater.UpdateReceived += OnUpdateReceived;

            mRESTUpdaterCallback = aCallback;

            mRESTUpdater.Connect(Link);
        }

        private void OnUpdateReceived(string aData)
        {
            if (!string.IsNullOrWhiteSpace(aData))
            {
                var data = aData.Substring(aData.IndexOf('{')).ParseJSON();
                if (data != null)
                {
                    var newId =(string)data["id"];
                    var newTitle = (string)data["title"];
                    var newLink = new Uri((string)data["link"]);

                    var changed = ChangeFlags.WidgetData; // this is sure

                    if(newId != Id)
                    {
                        Id=newId;
                        changed |= ChangeFlags.Id;
                    }
                    if(newTitle != Title)
                    {
                        Title=newTitle;
                        changed |= ChangeFlags.Title;
                    }
                    if(newLink != Link)
                    {
                        Link=newLink;
                        changed |= ChangeFlags.Link;
                    }

                    var widgets = data["widget"] as List<Dictionary<string, object>>;

                    if(widgets!=null)
                    {
                        var idx = 0;
                        for (idx = 0; idx < Math.Min(Widgets.Count, widgets.Count); ++idx)
                        {
                            if (UpdateWidget(mWidgets[idx], widgets[idx]))
                                changed |= ChangeFlags.Widgets;
                        }

                        if (idx < Widgets.Count)
                        {
                            mWidgets.RemoveRange(idx, mWidgets.Count - idx);
                            changed |= ChangeFlags.Widgets;
                        }

                        if (idx < widgets.Count)
                        {
                            // TODO: add additional widgets
                        }
                    }
                    else if (mWidgets.Count > 0)
                    {
                        // TODO: widgets removed?
                    }

                    mRESTUpdaterCallback(mRest, this, changed);
                }
            }
        }

        private bool UpdateItem(Item aOldItem, Dictionary<string, object> aNewItem)
        {
            var type = (Item.ItemType)Enum.Parse(typeof(Item.ItemType), (string)aNewItem["type"], true);
            var name = (string)aNewItem["name"];
            var state = (string)aNewItem["state"];
            var link = new Uri((string)aNewItem["link"]);

            if (name != aOldItem.Name)
            {
                // TODO: change item
            }

            aOldItem.mState = state;

            return false;
        }

        private bool UpdateWidget(Widget aOldWidget, Dictionary<string, object> aNewWidget)
        {
            var changed = false;

            var type = (Widget.WidgetType)Enum.Parse(typeof(Widget.WidgetType), (string)aNewWidget["type"], true);
            var label = (string)aNewWidget["label"];
            var icon = (string)aNewWidget["icon"];

            Dictionary<string, object> item = null;
            object obj = null;
            if (aNewWidget.TryGetValue("item", out obj))
                item = (Dictionary<string, object>)obj;

            List<Dictionary<string, object>> widgets = null;
            if (aNewWidget.TryGetValue("widget", out obj))
                widgets = obj as List<Dictionary<string, object>>;

            if (aOldWidget.Type != type)
            {
                changed = true;
                aOldWidget.Type = type;
            }

            aOldWidget.Label = label;
            aOldWidget.Icon = icon;

            if (aOldWidget.Item != null && item != null)
            {
                if (UpdateItem(aOldWidget.Item, item))
                    changed = true;
            }
            else if (aOldWidget.Item != null || item != null)
            {
                changed = true;

                if (aOldWidget.Item != null)
                {
                    aOldWidget.Item = null;
                }
                else
                {
                    // TODO: find/add item
                }
            }

            if (type == Widget.WidgetType.Video || type == Widget.WidgetType.Webview || type == Widget.WidgetType.Image)
            {
                var url = (string)aNewWidget["url"];
                aOldWidget.Url = new Uri(new UriBuilder(aOldWidget.Url.Scheme, aOldWidget.Url.Host, aOldWidget.Url.Port).Uri, url);
            }

            if (type == Widget.WidgetType.Chart)
            {
                aOldWidget.Period = (string)aNewWidget["period"];
                aOldWidget.Refresh = int.Parse((string)aNewWidget["refresh"]);
            }

            if (type == Widget.WidgetType.Setpoint)
            {
                aOldWidget.MinValue = double.Parse((string)aNewWidget["minValue"], System.Globalization.NumberFormatInfo.InvariantInfo);
                aOldWidget.MaxValue = double.Parse((string)aNewWidget["maxValue"], System.Globalization.NumberFormatInfo.InvariantInfo);
                aOldWidget.Step = double.Parse((string)aNewWidget["step"], System.Globalization.NumberFormatInfo.InvariantInfo);
            }

            if (widgets != null)
            {
                var idx = 0;
                for (idx = 0; idx < Math.Min(aOldWidget.mWidgets.Count, widgets.Count); ++idx)
                {
                    if (UpdateWidget(aOldWidget.mWidgets[idx], widgets[idx]))
                        changed = true;
                }

                if (idx < aOldWidget.mWidgets.Count)
                {
                    aOldWidget.mWidgets.RemoveRange(idx, aOldWidget.mWidgets.Count - idx);
                    changed = true;
                }

                if (idx < widgets.Count)
                {
                    // TODO: add additional widgets
                }
            }
            else if (aOldWidget.mWidgets.Count > 0)
            {
                // TODO: widgets removed?
            }

            aOldWidget.FireUpdated();

            return changed;   
        }

        public void StopUpdateNotification()
        {
            if (mRESTUpdater == null)
                throw new InvalidOperationException("Notification not started");

            mRESTUpdater.Dispose();
            mRESTUpdater = null;
            mRESTUpdaterCallback = null;
        }
    }
}
