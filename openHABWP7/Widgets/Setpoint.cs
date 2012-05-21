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

namespace openHABWP7.Widgets
{
    public class Setpoint : Base
    {
        public Setpoint(openHAB.Widget aWidget)
            : base(aWidget)
        {
            IncreaseCommand = new Framework.DelegateCommand(OnIncrease);
            DecreaseCommand = new Framework.DelegateCommand(OnDecrease);
        }

        public ICommand IncreaseCommand
        {
            get;
            private set;
        }

        private void OnIncrease(object aParameter)
        {
            Value += Widget.Step;
        }

        public ICommand DecreaseCommand
        {
            get;
            private set;
        }

        private void OnDecrease(object aParameter)
        {
            Value -= Widget.Step;
        }

        public double Value
        {
            get
            {
                double val;
                if (double.TryParse(Widget.Item.State, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out val))
                    return val;

                return double.NaN;
            }
            set 
            {
                value = Math.Min(Widget.MaxValue, value);
                value = Math.Max(Widget.MinValue, value);

                var val = (int)(value / Widget.Step);
                value = val * Widget.Step;

                Widget.Item.Update(value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo), (r, i) => { });
            }
        }
        public override void OnItemChanged()
        {
            base.OnItemChanged();
            RaisePropertyChanged("Value");
        }
    }
}
