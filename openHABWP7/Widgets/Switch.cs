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
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace openHABWP7.Widgets
{
    public class Switch : Base
    {
        protected Switch(openHAB.Widget aWidget)
            : base(aWidget)
        {
        }
    }

    public class SwitchRollerShutter : Switch
    {
        public SwitchRollerShutter(openHAB.Widget aWidget)
            : base(aWidget)
        {
            StopCommand = new Framework.DelegateCommand(OnStop);
        }

        public ICommand StopCommand { get; private set; }
        private void OnStop(object aParameter)
        {
            Widget.Item.Update("STOP", (r, i) => { });
        }

        private bool mIsDownPressed;
        private int mIsDownPressedAt;
        public bool IsDownPressed
        {
            get
            {
                return mIsDownPressed;
            }
            set
            {
                if (mIsDownPressed != value)
                {
                    mIsDownPressed = value;

                    if (value)
                    {
                        mIsDownPressedAt = Environment.TickCount;
                        Widget.Item.Update("DOWN", (r, i) => { });
                    }
                    else
                    {
                        var diff = Environment.TickCount - mIsDownPressedAt;
                        if(diff < 500)
                            Widget.Item.Update("100", (r, i) => { });
                        else
                            Widget.Item.Update("STOP", (r, i) => { });
                    }
                }
            }
        }

        private bool mIsUpPressed;
        private int mIsUpPressedAt;
        public bool IsUpPressed
        {
            get
            {
                return mIsUpPressed;
            }
            set
            {
                if (mIsUpPressed != value)
                {
                    mIsUpPressed = value;

                    if (value)
                    {
                        mIsUpPressedAt = Environment.TickCount;
                        Widget.Item.Update("UP", (r, i) => { });
                    }
                    else
                    {
                        var diff = Environment.TickCount - mIsUpPressedAt;
                        if (diff < 500)
                            Widget.Item.Update("0", (r, i) => { });
                        else
                            Widget.Item.Update("STOP", (r, i) => { });
                    }
                }
            }
        }
    }

    public class SwitchNumber : Switch
    {
        public SwitchNumber(openHAB.Widget aWidget)
            : base(aWidget)
        {
            mMappings = new Mappings(aWidget);
        }

        private readonly Mappings mMappings;
        public Mappings Mappings
        {
            get { return mMappings; }
        }

        public override void OnItemChanged()
        {
            base.OnItemChanged();

            foreach (var mapping in Mappings)
                mapping.OnItemChanged();
        }
    }

    public class SwitchButton : Switch
    {
        public SwitchButton(openHAB.Widget aWidget)
            : base(aWidget)
        {
            ClickCommand = new Framework.DelegateCommand(OnClick);
        }

        public ICommand ClickCommand
        {
            get;
            private set;
        }

        private void OnClick(object aParameter)
        {
            Widget.Item.Update(Widget.Mappings.Values.First(), (r, i) => { });
        }

        public bool IsActive { get { return Widget.Item.State == Widget.Mappings.Values.First(); } }
        public bool IsInactive { get { return Widget.Item.State != Widget.Mappings.Values.First(); } }

        public string State
        {
            get { return Widget.Mappings.Keys.First(); }
        }

        public override void OnItemChanged()
        {
            base.OnItemChanged();
            RaisePropertyChanged("IsActive");
            RaisePropertyChanged("IsInactive");
            RaisePropertyChanged("State");
        }
    }

    public class SwitchToggle : Switch
    {
        public SwitchToggle(openHAB.Widget aWidget)
            : base(aWidget)
        {
        }
        public bool IsActive 
        { 
            get { return Widget.Item.State == "ON"; }
            set { Widget.Item.Update(value ? "ON" : "OFF", (r, i) => { }); }
        }
        public bool IsInactive { get { return Widget.Item.State == "OFF"; } }

        public override void OnItemChanged()
        {
            base.OnItemChanged();
            RaisePropertyChanged("IsActive");
            RaisePropertyChanged("IsInactive");
        }
    }

    public class SwitchList : Switch
    {
        public SwitchList(openHAB.Widget aWidget)
            : base(aWidget)
        {
            mMappings = new Mappings(aWidget);
        }

        private readonly Mappings mMappings;
        public Mappings Mappings
        {
            get { return mMappings; }
        }

        public override void OnItemChanged()
        {
            base.OnItemChanged();

            foreach (var mapping in Mappings)
                mapping.OnItemChanged();
        }
    }

    public class SwitchGroup : Switch
    {
        public SwitchGroup(openHAB.Widget aWidget)
            : base(aWidget)
        {
        }
    }
}
