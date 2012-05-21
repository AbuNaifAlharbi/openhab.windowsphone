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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace openHAB
{
    public class Widget
    {
        public enum WidgetType
        {
            Frame,
            Group,
            Switch,
            Text,
            Slider,
            Image,
            Selection,

            Chart,
            Setpoint,
            Video,
            Webview
        };

        internal Widget(string aType,string aLabel, string aIcon)
        {
            Type = (WidgetType)Enum.Parse(typeof(WidgetType), aType, true);
            Label = aLabel;
            Icon = aIcon;
            Period = string.Empty;
        }

        public event Action<Widget> Updated;

        public WidgetType Type { get; internal set; }
        public string Label { get; internal set; }
        public string Icon { get; internal set; }

        public int Refresh { get; internal set; }
        public string Period { get; internal set; }

        public double MinValue { get; internal set; }
        public double MaxValue { get; internal set; }
        public double Step { get; internal set; }

        public Item Item { get; internal set; }
        public Page LinkedPage { get; internal set; }
        public Uri Url { get; internal set; }
        public readonly Dictionary<string, string> Mappings = new Dictionary<string, string>();

        internal readonly List<Widget> mWidgets = new List<Widget>();
        public ReadOnlyCollection<Widget> Widgets
        {
            get
            {
                return mWidgets.AsReadOnly();
            }
        }

        internal void FireUpdated()
        {
            if (Updated != null)
                Updated(this);
        }

        internal void Write(System.IO.BinaryWriter aWriter)
        {
            aWriter.Write((int)Type);
            aWriter.Write(Label);
            aWriter.Write(Icon);
            aWriter.Write(Item != null ? Item.Name : string.Empty);
            aWriter.Write(LinkedPage != null ? LinkedPage.Id : string.Empty);
            aWriter.Write(Url != null ? Url.AbsoluteUri : string.Empty);

            aWriter.Write(Refresh);
            aWriter.Write(Period);

            aWriter.Write(MinValue);
            aWriter.Write(MaxValue);
            aWriter.Write(Step);
            
            aWriter.Write(Widgets.Count);
            foreach (var widget in Widgets)
                widget.Write(aWriter);
        }

        internal Widget(REST aRest, System.IO.BinaryReader aReader)
        {
            Type = (WidgetType)aReader.ReadInt32();
            Label = aReader.ReadString();
            Icon = aReader.ReadString();

            Item item;
            if(aRest.Items.TryGetValue(aReader.ReadString(), out item))
                Item = item;

            Page page;
            if (aRest.Pages.TryGetValue(aReader.ReadString(), out page))
                LinkedPage = page;

            var uri = aReader.ReadString();
            if (!string.IsNullOrWhiteSpace(uri))
                Url = new Uri(uri);

            Refresh = aReader.ReadInt32();
            Period = aReader.ReadString();

            MinValue = aReader.ReadDouble();
            MaxValue = aReader.ReadDouble();
            Step = aReader.ReadDouble();

            var count = aReader.ReadInt32();
            for (var idx = 0; idx < count; ++idx)
                mWidgets.Add(new Widget(aRest, aReader));
        }
    }
}
