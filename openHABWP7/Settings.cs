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
using System.IO.IsolatedStorage;

namespace openHABWP7
{
    public static class Settings
    {
        private static T GetSetting<T>(string aName, T aDefault)
        {
            T result;

            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(aName, out result))
                return aDefault;

            return result;
        }

        public static double PageUpdateMS
        {
            get { return GetSetting("PageUpdateMS", 500.0); }
            set { IsolatedStorageSettings.ApplicationSettings["PageUpdateMS"] = value; IsolatedStorageSettings.ApplicationSettings.Save(); }
        }

        public static Uri DefaultServer
        {
            get { return GetSetting("DefaultServer", (Uri)null); }
            set { IsolatedStorageSettings.ApplicationSettings["DefaultServer"] = value; IsolatedStorageSettings.ApplicationSettings.Save(); }
        }

        public static string DefaultSitemap
        {
            get { return GetSetting("DefaultSitemap", @"demo"); }
            set { IsolatedStorageSettings.ApplicationSettings["DefaultSitemap"] = value; IsolatedStorageSettings.ApplicationSettings.Save(); }
        }

        public static Uri[] ServerList
        {
            get { return GetSetting("ServerList", new Uri[0]); }
            set { IsolatedStorageSettings.ApplicationSettings["ServerList"] = value; IsolatedStorageSettings.ApplicationSettings.Save(); }
        }

        public static double FontSize
        {
            get { return GetSetting("FontSize", 24.0); }
            set { IsolatedStorageSettings.ApplicationSettings["FontSize"] = value; IsolatedStorageSettings.ApplicationSettings.Save(); }
        }

        public static double SliderUpdateIntervalMS
        {
            get { return GetSetting("SliderUpdateIntervalMS", 500.0); }
            set { IsolatedStorageSettings.ApplicationSettings["SliderUpdateIntervalMS"] = value; IsolatedStorageSettings.ApplicationSettings.Save(); }
        }

        public static string Language
        {
            get { return GetSetting("Language", "EN"); }
            set { IsolatedStorageSettings.ApplicationSettings["Language"] = value; IsolatedStorageSettings.ApplicationSettings.Save(); }
        }
    }
}
