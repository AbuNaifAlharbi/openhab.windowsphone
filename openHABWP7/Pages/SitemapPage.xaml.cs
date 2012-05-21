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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Phone.Controls;

namespace openHABWP7.Pages
{
    public static class Extensions
    {
        public static string Default(this string aThis, string aDefault)
        {
            if (string.Compare(aThis, "uninitialized", StringComparison.InvariantCultureIgnoreCase) == 0)
                return aDefault;

            return aThis;
        }
    };

    public partial class SitemapPage : BasePage
    {
        public SitemapPage()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
            DataContext = this;
        }

        private readonly Dictionary<openHAB.Widget, Widgets.Base> mW2W = new Dictionary<openHAB.Widget,Widgets.Base>();

        private openHAB.Sitemap mSitemap;
        private openHAB.Page mPage;

        void OnLoaded(object aSender, RoutedEventArgs aArgs)
        {
            if (mSitemap != null)
                return;

            var name = NavigationContext.QueryString["sitemap"];

            mSitemap = REST.Sitemaps[name];

            ApplicationTitle.Text = NavigationContext.QueryString["title"];

            REST.GetSitemap(mSitemap, OnSitemap);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            mPage.StopUpdateNotification();
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (mPage != null)
            {
                mPage.Update((r, p) => { });
                mPage.StartUpdateNotification((r, p, c) =>
                {
                });
            }
        }

        private void OnSitemap(openHAB.REST aREST, openHAB.RESTResult aResult)
        {
            if(NavigationContext.QueryString.ContainsKey("page"))
            {
                var pageId = NavigationContext.QueryString["page"];
                mPage = mSitemap.Pages[pageId];
            }
            else
            {
                mPage = mSitemap.Page;
            }

            mPage.StartUpdateNotification((r, p, c) =>
                {
                });

            UpdateSitemap();
        }

        private void UpdateSitemap()
        {
            PageTitle.Text = RemoveMarkup(mPage.Title);

            Widgets.Clear();
            var added = AddWidgets(mPage.Widgets);
            foreach (var widget in added)
                Widgets.Add(widget);

            RaisePropertyChanged("Widgets");
        }
        
        private readonly ObservableCollection<Widgets.Base> mWidgets = new ObservableCollection<Widgets.Base>();
        public ObservableCollection<Widgets.Base> Widgets
        {
            get
            {
                return mWidgets;
            }
        }

        private IEnumerable<Widgets.Base> AddWidgets(IEnumerable<openHAB.Widget> aWidgets)
        {
            var widgets = new List<Widgets.Base>();

            foreach (var widget in aWidgets)
            {
                switch (widget.Type)
                {
                    case openHAB.Widget.WidgetType.Frame:
                        {
                            var frame = new Widgets.Frame(widget);
                            widgets.Add(frame);
                            foreach (var child in AddWidgets(widget.Widgets))
                                frame.Widgets.Add(child);
                        }
                        break;

                    case openHAB.Widget.WidgetType.Group:
                        widgets.Add(new Widgets.Group(widget));
                        break;

                    case openHAB.Widget.WidgetType.Text:
                        widgets.Add(new Widgets.Text(widget));
                        break;

                    case openHAB.Widget.WidgetType.Switch:
                        {
                            switch (widget.Item.Type)
                            {
                                case openHAB.Item.ItemType.RollershutterItem:
                                    widgets.Add(new Widgets.SwitchRollerShutter(widget));
                                    break;

                                case openHAB.Item.ItemType.NumberItem:
                                    widgets.Add(new Widgets.SwitchNumber(widget));
                                    break;

                                case openHAB.Item.ItemType.SwitchItem:
                                case openHAB.Item.ItemType.GroupItem:
                                    if (widget.Mappings.Count == 0)
                                        widgets.Add(new Widgets.SwitchToggle(widget));
                                    else if (widget.Mappings.Count == 1)
                                        widgets.Add(new Widgets.SwitchButton(widget));
                                    else
                                        widgets.Add(new Widgets.SwitchList(widget));
                                    break;

                                default:
                                    break;
                            }
                        }
                        break;

                    case openHAB.Widget.WidgetType.Slider:
                        widgets.Add(new Widgets.Slider(widget));
                        break;

                    case openHAB.Widget.WidgetType.Image:
                        widgets.Add(new Widgets.Image(widget));
                        break;

                    case openHAB.Widget.WidgetType.Selection:
                        widgets.Add(new Widgets.Selection(widget));
                        break;

                    case openHAB.Widget.WidgetType.Chart:
                        widgets.Add(new Widgets.Chart(widget));
                        break;

                    case openHAB.Widget.WidgetType.Setpoint:
                        widgets.Add(new Widgets.Setpoint(widget));
                        break;

                    case openHAB.Widget.WidgetType.Video:
                        widgets.Add(new Widgets.Video(widget));
                        break;

                    case openHAB.Widget.WidgetType.Webview:
                        widgets.Add(new Widgets.Webview(widget));
                        break;

                    default:
                        break;
                }
            }

            foreach (var widget in widgets)
                mW2W[widget.Widget] = widget;

            return widgets;
        }

        private void UpdateWidget(openHAB.Widget aWidget)
        {
            mW2W[aWidget].OnItemChanged();

            foreach (var widget in aWidget.Widgets)
                UpdateWidget(widget);
        }

        private string RemoveMarkup(string aText)
        {
            if (aText.Contains('['))
                return aText.Substring(0, aText.IndexOf('['));
            else
                return aText;
        }

        public override void NavigateToPage(string aPageId)
        {
            GotoPage("/Sitemap", "sitemap", mSitemap.Name, "page", aPageId, "title", ApplicationTitle.Text + " > " + RemoveMarkup(PageTitle.Text));
        }

        public override void SelectItemFromList(openHAB.Widget aWidget)
        {
            var listdata = string.Empty;

            foreach (var mapping in aWidget.Mappings)
                listdata += mapping.Key + '\u0001' + mapping.Value + '\u0001';

            GotoPage("/Selection", "sitemap", mSitemap.Name, "page", mPage.Id, "title", ApplicationTitle.Text + " > " + PageTitle.Text, "item", aWidget.Item.Name, "label", aWidget.Label, "list", listdata);
        }
    }
}