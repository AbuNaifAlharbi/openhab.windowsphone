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
    public class Item
    {
        public enum ItemType
        {
            RollershutterItem,
            GroupItem,
            ContactItem,
            SwitchItem,
            NumberItem,
            DimmerItem,
            DateTimeItem
        }

        internal Item(REST aRest,string aType, string aName, string aState, string aLink)
        {
            mRest = aRest;
            Type = (ItemType)Enum.Parse(typeof(ItemType), aType, true);
            Name = aName;
            mState = aState;
            Link = new Uri(aLink);
        }

        private bool mPending;
        public void Update(string aNewState, Action<REST, Item> aCallback)
        {
            if (!mPending)
            {
                mPending = true;
                mRest.QuerySite(Link.AbsoluteUri, (rest, result) =>
                    {
                        mPending = false;
                        aCallback(rest, this);
                    }, aNewState);
            }
        }

        private readonly REST mRest;

        internal string mState;

        public ItemType Type { get; internal set; }
        public string State
        {
            get { return mState; }
        }
        public readonly string Name;
        public readonly Uri Link;

        internal void Write(System.IO.BinaryWriter aWriter)
        {
            aWriter.Write(State);
            aWriter.Write((int)Type);
            aWriter.Write(Name);
            aWriter.Write(Link.AbsoluteUri);
        }

        internal Item(REST aRest, System.IO.BinaryReader aReader)
        {
            mRest = aRest;

            mState = aReader.ReadString();
            Type = (ItemType)aReader.ReadInt32();
            Name = aReader.ReadString();
            Link = new Uri(aReader.ReadString());
        }

    }
}
