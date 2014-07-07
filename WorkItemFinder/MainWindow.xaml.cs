using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

using WorkItemFinder.Annotations;

using Clipboard = System.Windows.Clipboard;
using DataFormats = System.Windows.DataFormats;
using ListViewItem = System.Windows.Controls.ListViewItem;
using TextBox = System.Windows.Controls.TextBox;

namespace WorkItemFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Domäne _domäne = null;
        NotifyIcon taskIcon = new NotifyIcon();

        private bool _cachingFinished = false;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            ResultSearchWorkItems = new ObservableCollection<SearchWorkItem>();
            Status = "Cache wird aufgebaut...";
            _domäne= new Domäne(() =>
            {
                Status = _domäne.CacheSize + " Workitems im Cache.";
                OnPropertyChanged("Status");
                _cachingFinished = true;
            });

            taskIcon.Icon = Properties.Resources.Lupe;
            taskIcon.Click += (sender, args) => toggleVisibility();
            taskIcon.Visible = true;
        }

        private void toggleVisibility()
        {
            this.ShowInTaskbar = !this.ShowInTaskbar;
            if (this.Visibility == Visibility.Visible)
            {
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                this.Visibility = Visibility.Visible;
            }
        }

        public string Status { get; set; }

        public ObservableCollection<SearchWorkItem> ResultSearchWorkItems { get; set; }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ResultSearchWorkItems.Clear();
            var searchString = ((TextBox)sender).Text;
            var searchResult = _domäne.FilterItems(searchString);
            if (searchResult != null)
            {
                foreach (var searchWorkItem in searchResult)
                {
                    ResultSearchWorkItems.Add(searchWorkItem);
                }
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
            {
                var searchWorkItem = item.DataContext as SearchWorkItem;
                if (searchWorkItem != null)
                {
                    Trace.WriteLine(searchWorkItem.Id);
                    Clipboard.SetData(DataFormats.Text, searchWorkItem.Id);
                }
            }
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
            {
                var searchWorkItem = item.DataContext as SearchWorkItem;
                if (searchWorkItem != null)
                {
                    Trace.WriteLine(searchWorkItem.Id);
                    Process.Start(searchWorkItem.Link);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_cachingFinished)
            {
                _cachingFinished = false;
                Status = "Cache wird aktualisiert...";
                OnPropertyChanged("Status");
                _domäne.RefreshCache();
            }
        }
    }
}
