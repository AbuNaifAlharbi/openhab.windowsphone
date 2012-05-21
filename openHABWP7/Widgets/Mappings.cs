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
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Microsoft.Phone.Shell;


namespace openHABWP7.Widgets
{
    public class Mapping : Framework.NotifyPropertyChanged
    {
        public Mapping(openHAB.Widget aWidget, KeyValuePair<string,string> aPair)
        {
            Widget = aWidget;
            Name = aPair.Key;
            State = aPair.Value;

            ClickCommand = new Framework.DelegateCommand(OnClick);
            HoldCommand = new Framework.DelegateCommand(OnHold);
        }

        public readonly openHAB.Widget Widget;

        public string Name { get; private set; }
        public string State { get; private set; }

        public bool IsActive { get { return Widget.Item.State == State; } }
        public bool IsInactive { get { return Widget.Item.State != State; } }

        public ICommand ClickCommand { get; private set; }

        private bool mIgnoreNextClick;

        private void OnClick(object aParameter)
        {
            if (mIgnoreNextClick)
                mIgnoreNextClick = false;
            else
                Widget.Item.Update(State, (r, i) => { });
        }

        public ICommand HoldCommand { get; private set; }

        private void OnHold(object aParameter)
        {
            mIgnoreNextClick = true;

            var tileUri = new Uri("/DeepLink/UpdateState?title=" + Widget.Label + "&icon=" + Widget.Icon + "&item=" + Widget.Item.Link.AbsoluteUri + "&state=" + State, UriKind.Relative);

            var tile = ShellTile.ActiveTiles.FirstOrDefault(t => t.NavigationUri.Equals(tileUri));
            if (tile != null)
                return;

            /*
            if (System.Diagnostics.Debugger.IsAttached)
            {
                SitemapPage.CurrentPage.NavigationService.Navigate(tileUri);
            }
            else
            */
            {
                var tileData = new StandardTileData
                {
                    Title = Name,
                    BackgroundImage = new Uri(string.Format("/Images/{0}.png", Widget.Icon), UriKind.Relative),
                    BackTitle = Name,
                    BackContent = Widget.Label,
                    BackBackgroundImage = new Uri(string.Format("/Images/{0}.png", Widget.Icon), UriKind.Relative),
                };

                ShellTile.Create(tileUri, tileData);
            }
        }

        public void OnItemChanged()
        {
            RaisePropertyChanged("IsActive");
            RaisePropertyChanged("IsInactive");
        }
    }

    public class Mappings : IEnumerable<Mapping>
    {
        public Mappings(openHAB.Widget aWidget)
        {
            foreach (var mapping in aWidget.Mappings)
                mMappings.Add(new Mapping(aWidget, mapping));
        }
        public IEnumerator<Mapping> GetEnumerator()
        {
            return mMappings.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private readonly List<Mapping> mMappings = new List<Mapping>();
    }
}
