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
using System.ComponentModel;
using Microsoft.Phone.Controls;

namespace openHABWP7.Pages
{
    public class BasePage : PhoneApplicationPage, INotifyPropertyChanged
    {
        public static BasePage CurrentPage;

        public BasePage ()
	    {
            DataContext = this;
	    }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string aProperty)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(aProperty));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
         
            CurrentPage = this;
        }

        protected bool CheckAndHandleError(openHAB.RESTResult aResult)
        {
            return aResult == null || aResult.Error != null || aResult.Cancelled;
        }

        protected openHAB.REST REST
        {
            get { return App.REST; }
        }

        public void GotoPage(string aUri, params object[] aParameter)
        {
            var str = aUri;

            if (aParameter.Length > 0)
                str += "?";

            for (var idx = 0; idx < aParameter.Length; idx += 2)
                str += string.Format("{0}={1}&", aParameter[idx], System.Net.HttpUtility.UrlEncode((aParameter[idx + 1].ToString())));

            NavigationService.Navigate(new Uri(str, UriKind.Relative));
        }

        public void ShowImage(Uri aImageUri)
        {
            var url = aImageUri;

            if (url.AbsolutePath.Contains("/rrdchart.png"))
            {
                if (!string.IsNullOrWhiteSpace(url.Query))
                    url = new Uri(url.AbsoluteUri + "&w=800&h=480");
                else
                    url = new Uri(url.AbsoluteUri + "?w=800&h=480");
            }

            GotoPage("/Image", "uri", url.AbsoluteUri);
        }

        public virtual void NavigateToPage(string aPageId)
        {
            throw new NotImplementedException();
        }

        public virtual void SelectItemFromList(openHAB.Widget aWidget)
        {
            throw new NotImplementedException();
        }
    }
}
