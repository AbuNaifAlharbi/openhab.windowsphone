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
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace openHABWP7.Pages
{
    public partial class UpdateFromMapping : BasePage
    {
        public UpdateFromMapping()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private string mIcon;
        public string Icon
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(mIcon))
                    return string.Format("../Images/{0}.png", mIcon);

                return null;
            }
        }

        private Uri mItem;
        private string mState;
        private readonly WebClient mWebClient = new WebClient();

        void OnLoaded(object aSender, RoutedEventArgs aArgs)
        {
            Title = NavigationContext.QueryString["title"];
            mIcon = NavigationContext.QueryString["icon"];
            mItem = new Uri(NavigationContext.QueryString["item"]);
            mState = NavigationContext.QueryString["state"];

            RaisePropertyChanged("Title");
            RaisePropertyChanged("Icon");

            mWebClient.UploadStringCompleted += OnUploadStringCompleted;
            mWebClient.DownloadStringCompleted += OnDownloadStringCompleted;

            mWebClient.UploadStringAsync(mItem, "POST", mState);
        }

        private void OnUploadStringCompleted(object aSender, UploadStringCompletedEventArgs aArgs)
        {
            mProgressState.Text = "Checking";
            mWebClient.DownloadStringAsync(mItem);
        }

        private void OnDownloadStringCompleted(object aSender, DownloadStringCompletedEventArgs aArgs)
        {
            mProgressState.Text = "Done";

            var doc = XDocument.Parse(aArgs.Result);
            var state = doc.Element("item").Element("state").Value;

            if (state.Equals(mState))
                mProgressState.Text = "Set";
        }
    }
}