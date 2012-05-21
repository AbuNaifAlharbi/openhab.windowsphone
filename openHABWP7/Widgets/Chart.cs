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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace openHABWP7.Widgets
{
    public class Chart : Base
    {
        public Chart(openHAB.Widget aWidget)
            : base(aWidget)
        {
            ClickCommand = new Framework.DelegateCommand(OnClick);
            ShowImageCommand = new Framework.DelegateCommand(OnShowImage);

            mTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(Widget.Refresh)
            };

            mTimer.Tick += OnTick;
            mTimer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            RaisePropertyChanged("ImageUri");
        }

        private readonly DispatcherTimer mTimer;
        public Uri ImageUri
        {
            get
            {
                if (Widget.Item.Type == openHAB.Item.ItemType.GroupItem)
                    return new Uri(Widget.Item.Link, string.Format("/rrdchart.png?groups={0}&period={1}&q={2}", Widget.Item.Name, Widget.Period, Environment.TickCount));
                
                return null;
            }
        }

        public ICommand ClickCommand
        {
            get;
            private set;
        }

        private void OnClick(object aParameter)
        {
            if (Widget.LinkedPage != null)
                Pages.BasePage.CurrentPage.NavigateToPage(Widget.LinkedPage.Id);
        }

        public ICommand ShowImageCommand
        {
            get;
            private set;
        }

        private void OnShowImage(object aParameter)
        {
            Pages.BasePage.CurrentPage.ShowImage(new Uri(Widget.Item.Link, string.Format("/rrdchart.png?groups={0}&period={1}&q={2}&w=800&h=480", Widget.Item.Name, Widget.Period, Environment.TickCount)));
        }

        public override void OnItemChanged()
        {
            base.OnItemChanged();
            //RaisePropertyChanged("ImageUri");
            mTimer.Interval = TimeSpan.FromMilliseconds(Widget.Refresh);
        }
    }
}
