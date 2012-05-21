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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace openHAB
{
    public class Sitemap
    {
        internal Sitemap(string aName, Uri aLink, Uri aHomepage)
        {
            Name = aName;
            Link = aLink;
            Homepage = aHomepage;
        }

        public readonly string Name;
        public readonly Uri Link;
        public readonly Uri Homepage;

        internal readonly Dictionary<string, Page> mPages = new Dictionary<string, Page>();
        public Dictionary<string, Page> Pages
        {
            get { return mPages; }
        }

        internal readonly Dictionary<string, Item> mItems = new Dictionary<string, Item>();
        public Dictionary<string, Item> Items
        {
            get { return mItems; }
        }

        public Page Page { get; internal set; }

        internal void Write(System.IO.BinaryWriter aWriter)
        {
            aWriter.Write(Name);
            aWriter.Write(Link.AbsoluteUri);
            aWriter.Write(Homepage.AbsoluteUri);

            aWriter.Write(Items.Count);
            foreach (var item in Items)
                aWriter.Write(item.Value.Name);

            aWriter.Write(Pages.Count);
            foreach(var page in Pages)
                aWriter.Write(page.Value.Id);

            aWriter.Write(Page.Id);
        }

        internal Sitemap(REST aRest, System.IO.BinaryReader aReader)
        {
            Name = aReader.ReadString();
            Link = new Uri(aReader.ReadString());
            Homepage = new Uri(aReader.ReadString());

            var count = aReader.ReadInt32();
            for (var idx = 0; idx < count; ++idx)
            {
                Item item;
                if (aRest.Items.TryGetValue(aReader.ReadString(), out item))
                    Items[item.Name] = item;
            }

            Page page;
            count = aReader.ReadInt32();
            for (var idx = 0; idx < count; ++idx)
            {
                if (aRest.Pages.TryGetValue(aReader.ReadString(), out page))
                    Pages[page.Id] = page;
            }

            if (aRest.Pages.TryGetValue(aReader.ReadString(), out page))
                Page = page;
        }
    }
}
