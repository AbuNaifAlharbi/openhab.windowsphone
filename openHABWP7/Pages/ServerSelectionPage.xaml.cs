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
using System.Windows.Threading;

namespace openHABWP7.Pages
{
    public partial class ServerSelection : BasePage
    {
        private readonly Network.MDnsClient mClient = Network.MDnsClient.CreateAndResolve("_openhab-server._tcp.local");
        private readonly DispatcherTimer mTimer = new DispatcherTimer();

        public class Server : Framework.NotifyPropertyChanged
        {
            public Server(ServerSelection aPage, string aName, string aDescription, Uri aUri)
            {
                mPage = aPage;

                Name = aName;
                Description = aDescription;
                Uri = aUri;
                IsOnline = false;
                Version = "?";

                var wc = new WebClient();
                wc.DownloadStringCompleted += OnServerAnswer;
                wc.DownloadStringAsync(new Uri(aUri, "static/version?q=" + Environment.TickCount));

                ConnectCommand = new Framework.DelegateCommand(OnConnect);
                ConnectAndSetDefaultCommand = new Framework.DelegateCommand(OnConnectAndSetDefault);
                AddCommand = new Framework.DelegateCommand(OnAdd);
                RemoveCommand = new Framework.DelegateCommand(OnRemove);
            }

            private void OnServerAnswer(object aSender, DownloadStringCompletedEventArgs aArgs)
            {
                IsOnline = aArgs.Error == null && !aArgs.Cancelled;

                if (IsOnline)
                {
                    Version = aArgs.Result;
                    RaisePropertyChanged("Version");
                }

                RaisePropertyChanged("IsOnline");
            }

            public string Version
            {
                get;
                private set;
            }

            public ICommand ConnectCommand
            {
                get;
                private set;
            }

            private bool mIgnoreNextClick;
            private void OnConnect(object aParameter)
            {
                if(!mIgnoreNextClick)
                    mPage.ConnectTo(this);
                mIgnoreNextClick = false;
            }

            public ICommand ConnectAndSetDefaultCommand
            {
                get;
                private set;
            }

            private void OnConnectAndSetDefault(object aParameter)
            {
                if (CanAdd && mPage.FoundServerList.Contains(this))
                {
                    mPage.FoundServerList.Remove(this);
                    mPage.LocalServerList.Add(this);
                    var list = new List<Uri>(Settings.ServerList);
                    list.Add(Uri);
                    Settings.ServerList = list.ToArray();
                }

                Settings.DefaultServer = Uri;

                mPage.ConnectTo(this);
            }

            public ICommand AddCommand
            {
                get;
                private set;
            }

            private void OnAdd(object aParameter)
            {
                if (CanAdd)
                {
                    mPage.FoundServerList.Remove(this);
                    mPage.LocalServerList.Add(this);
                    var list = new List<Uri>(Settings.ServerList);
                    list.Add(Uri);
                    Settings.ServerList = list.ToArray();
                }

                mIgnoreNextClick = true;
            }

            public ICommand RemoveCommand
            {
                get;
                private set;
            }

            private void OnRemove(object aParameter)
            {
                mPage.LocalServerList.Remove(this);
                var list = new List<Uri>(Settings.ServerList);
                list.Remove(Uri);
                Settings.ServerList = list.ToArray();

                if (Uri == Settings.DefaultServer)
                    Settings.DefaultServer = null;

                mIgnoreNextClick = true;
            }

            public bool CanAdd
            {
                get { return (string.Compare(Uri.Host, "demo.openhab.org", StringComparison.InvariantCultureIgnoreCase) != 0); }
            }

            public string Name
            {
                get;
                private set;
            }

            public string Description
            {
                get;
                private set;
            }

            public bool IsOnline
            {
                get;
                private set;
            }

            public Uri Uri
            {
                get;
                private set;
            }

            public Visibility FavVisibility
            {
                get
                {
                    return Uri == Settings.DefaultServer ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            private readonly ServerSelection mPage;
        }

        // Constructor
        public ServerSelection()
        {
            InitializeComponent();

            foreach (var server in Settings.ServerList)
            {
                mLocalServerList.Add(new Server(this, server.Host, new UriBuilder("http", server.Host, server.Port).Uri.AbsoluteUri, server));
            }

            mFoundServerList.Add(new Server(this, @"Demo Server", @"http://demo.openhab.org:8080", new Uri(@"http://demo.openhab.org:8080/rest")));

            mTimer.Interval = TimeSpan.FromSeconds(1.0);
            mTimer.Tick += OnTick;
            mTimer.Start();

            mClient.AnswerReceived += new Network.MDnsClient.ObjectEvent<Network.Message>(mClient_AnswerReceived);
            try
            {
                mClient.Start();
            }
            catch (System.Net.Sockets.SocketException)
            {
                // this could mean we are simply not in an adequate network for this
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // we may come from the settings...
            DataContext = null;

            mLocalServerList.Clear();
            foreach (var server in Settings.ServerList)
            {
                mLocalServerList.Add(new Server(this, server.Host, new UriBuilder("http", server.Host, server.Port).Uri.AbsoluteUri, server));
            }

            mFoundServerList.Clear();
            mFoundServerList.Add(new Server(this, @"Demo Server", @"http://demo.openhab.org:8080", new Uri(@"http://demo.openhab.org:8080/rest")));

            var loc = App.Current.Resources["LocaConv"] as Framework.LocaConverter;

            foreach(Microsoft.Phone.Shell.ApplicationBarIconButton btn in ApplicationBar.Buttons)
            {
                if (btn.Text == "Add")
                    btn.Text = loc.Get("ServerSelectionAddServer");
                if (btn.Text == "Settings")
                    btn.Text = loc.Get("ServerSelectionSettings");
            }
            
            DataContext = this;
        }

        public ICommand SettingsCommand { get; private set; }

        private readonly System.Collections.ObjectModel.ObservableCollection<Server> mFoundServerList = new System.Collections.ObjectModel.ObservableCollection<Server>();
        public System.Collections.ObjectModel.ObservableCollection<Server> FoundServerList
        {
            get
            {
                return mFoundServerList;
            }
        }

        private readonly System.Collections.ObjectModel.ObservableCollection<Server> mLocalServerList = new System.Collections.ObjectModel.ObservableCollection<Server>();
        public System.Collections.ObjectModel.ObservableCollection<Server> LocalServerList
        {
            get
            {
                return mLocalServerList;
            }
        }

        void OnTick(object aSender, EventArgs aArgs)
        {
            if (mTimer != null)
            {
                if (mClient.IsStarted)
                {
                    mClient.Resolve("_openhab-server._tcp");
                }
                else
                {
                    try
                    {
                        mClient.Start();
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        // this could mean we are simply not in an adequate network for this
                    }
                }
            }
        }

        void mClient_AnswerReceived(Network.Message aMessage)
        {
            if (App.REST == null)
            {
                var server = string.Empty;
                var port = -1;
                var path = string.Empty;

                foreach (var answer in aMessage.Answers)
                {
                    switch (answer.Type)
                    {
                        case Network.Type.PTR:
                            {
                                var name = ((Network.Ptr)answer.ResponseData).DomainName.ToString();

                                if (string.Compare(name, "openHAB._openhab-server._tcp.local", StringComparison.InvariantCultureIgnoreCase) != 0)
                                    return;
                            }
                            break;

                        case Network.Type.A:
                        case Network.Type.AAAA:
                            server = ((Network.HostAddress)answer.ResponseData).Address.ToString();
                            break;

                        case Network.Type.TXT:
                            if (((Network.Txt)answer.ResponseData).Properties.ContainsKey("uri"))
                                path = ((Network.Txt)answer.ResponseData).Properties["uri"];
                            break;

                        case Network.Type.SRV:
                            port = ((Network.Srv)answer.ResponseData).Port;
                            break;
                    }
                }

                if (port == -1 || string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(path))
                    return;

                var uri = new UriBuilder("http", server, port, path).Uri;

                if (mFoundServerList.Any(s => s.Uri == uri))
                    return;
                if (mLocalServerList.Any(s => s.Uri == uri))
                    return;

                Dispatcher.BeginInvoke(() =>
                    {
                        mFoundServerList.Add(new Server(this, server, new UriBuilder("http", server, port).Uri.AbsoluteUri, uri));
                    });
            }
        }

        private void OnREST(openHAB.REST aREST, openHAB.RESTResult aResult)
        {
            REST.GetSitemaps(OnSitemaps);
        }

        private void OnSitemaps(openHAB.REST aREST, openHAB.RESTResult aResult)
        {
            Dispatcher.BeginInvoke(() =>
                {

                    if (this.CheckAndHandleError(aResult))
                        return;

                    if (aREST.Sitemaps.Count == 0)
                    {
                        MessageBox.Show("Cannot find any Sitemap on that server!", "Error", MessageBoxButton.OK);
                    }
                    else if (aREST.Sitemaps.Count == 1)
                        this.GotoPage("/Sitemap", "sitemap", aREST.Sitemaps.Keys.First(), "title", aREST.Sitemaps.Values.First().Name);
                    else
                        this.GotoPage("/SelectSitemap");
                });
        }

        public void ConnectTo(Server aServer)
        {
            try
            {
                mClient.Stop();
            }
            catch (Exception)
            {
            }
             
            mTimer.Stop();

            Settings.DefaultServer = aServer.Uri;
            App.REST = new openHAB.REST();

            REST.GetREST(Settings.DefaultServer, OnREST);
        }

        private void OnSettingsClick(object aSender, EventArgs aArgs)
        {
            NavigationService.Navigate(new Uri("/Settings", UriKind.Relative));
        }

        private void OnAddServerClick(object aSender, EventArgs aArgs)
        {
            NavigationService.Navigate(new Uri("/AddServer", UriKind.Relative));
        }
    }
}