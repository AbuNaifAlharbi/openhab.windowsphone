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
    public class SplitText : Framework.NotifyPropertyChanged
    {
        public SplitText()
            : this(Colors.White, Colors.White)
        {
        }

        public SplitText(Color aLeftColor, Color aRightColor)
        {
            LeftColor = new SolidColorBrush(aLeftColor);
            RightColor = new SolidColorBrush(aRightColor);
        }

        private string mText = string.Empty;
        public string Text
        {
            get { return mText; }
            set { mText = value; UpdateText(); }
        }

        public string Left { get; private set; }
        public string Right { get; private set; }

        public Brush LeftColor { get; private set; }
        public Brush RightColor { get; private set; }

        private void UpdateText()
        {
            if (Text.Contains("["))
            {
                var idx = Text.IndexOf('[');
                Left = Text.Substring(0, idx);
                Right = Text.Substring(idx + 1, Text.IndexOf(']') - idx - 1);
            }
            else
            {
                Left = Text;
                Right = string.Empty;
            }

            RaisePropertyChanged("Text");
            RaisePropertyChanged("Left");
            RaisePropertyChanged("Right");
        }
    }
}
