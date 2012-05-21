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
    public class Slider : Base
    {
        public Slider(openHAB.Widget aWidget)
            : base(aWidget)
        {
            mTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Settings.SliderUpdateIntervalMS)
            };

            if (!double.TryParse(Widget.Item.State, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out mValue))
                mValue = 0.5;

            mTimer.Tick += OnTick;
        }

        void OnTick(object aSender, EventArgs aArgs)
        {
            Widget.Item.Update(mValue.ToString(System.Globalization.NumberFormatInfo.InvariantInfo), (r, i) => { });
        }

        private double mValue;
        private readonly DispatcherTimer mTimer;
        public double Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
            }
        }

        public override void OnItemChanged()
        {
            base.OnItemChanged();

            if (!IsPressed.Value)
            {
                if (!double.TryParse(Widget.Item.State, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out mValue))
                    mValue = 0.5;

                RaisePropertyChanged("Value");
            }
        }

        private bool? mIsPressed = false;
        public bool? IsPressed
        {
            get { return mIsPressed; }
            set
            {
                mIsPressed = value;

                if (value.HasValue)
                {
                    if (value.Value)
                    {
                        mTimer.Start();
                    }
                    else
                    {
                        mTimer.Stop();
                        Widget.Item.Update(mValue.ToString(System.Globalization.NumberFormatInfo.InvariantInfo), (r, i) => { });
                    }
                }
            }
        }
    }
}
