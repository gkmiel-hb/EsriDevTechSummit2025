using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Controls;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Button = ArcGIS.Desktop.Framework.Contracts.Button;

namespace TableControlDemo
{
    internal class TableControlDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "TableControlDemo_TableControlDockpane";

        protected TableControlDockpaneViewModel() 
        {
            ProjectWindowSelectedItemsChangedEvent.Subscribe(OnProjectWindowSelectedItem);

            MenuItem zoomItem = new MenuItem()
            {
                Header = "Zoom to Feature",
                Command = ZoomToRowCommand,
                CommandParameter = this
            };
            RowContextMenu = new ContextMenu();
            RowContextMenu.Items.Add(zoomItem);
        }


        private TableControl _tableControl;
        public TableControl TableControl 
        {
            get { return _tableControl; }
            set { SetProperty(ref _tableControl, value); }
        }

        private TableControlContent _tableContent;
        public TableControlContent TableContent
        {
            get { return _tableContent; }
            set { SetProperty(ref _tableContent, value); }
        }

        private Item _selectedItem = null;
        public bool IsItemSelected
        {
            get
            {
                return _selectedItem != null;
            }
        }

        private MapMember SelectedMapMember = null;
        private void OnProjectWindowSelectedItem(ProjectWindowSelectedItemsChangedEventArgs args)
        {
            if (args.IProjectWindow.SelectionCount > 0)
            {
                SelectedMapMember = null;

                // get the first selected item
                _selectedItem = args.IProjectWindow.SelectedItems.First();
                NotifyPropertyChanged(nameof(IsItemSelected));

                // check if it's supported by the TableControl
                if (!TableControlContentFactory.IsItemSupported(_selectedItem))
                    return;

                // create the content
                var tableContent = TableControlContentFactory.Create(_selectedItem);

                // assign it
                if (tableContent != null)
                {
                    this.TableContent = tableContent;
                }
            }
        }

        private ICommand _addToMapCommand = null;
        public ICommand AddToMapCommand
        {
            get
            {
                if (_addToMapCommand == null)
                {
                    _addToMapCommand = new RelayCommand(() =>
                    {
                        var map = MapView.Active?.Map;
                        if (map == null)
                            return;

                        QueuedTask.Run(() =>
                        {
                            StandaloneTableCreationParams tableCreationParams = new StandaloneTableCreationParams(_selectedItem);
                            // test if the selected Catalog item can create a layer
                            if (LayerFactory.Instance.CanCreateLayerFrom(_selectedItem))
                                SelectedMapMember = LayerFactory.Instance.CreateLayer<Layer>(new LayerCreationParams(_selectedItem), map);
                            // test if the selected Catalog item can create a table
                            else if (StandaloneTableFactory.Instance.CanCreateStandaloneTableFrom(_selectedItem))
                                SelectedMapMember = StandaloneTableFactory.Instance.CreateStandaloneTable(tableCreationParams, map);

                            else
                                SelectedMapMember = null;
                        });

                    });
                }
                return _addToMapCommand;
            }
        }


        public ContextMenu RowContextMenu { get; set; }

        private int _activeRowIdx;
        public int ActiveRowIdx
        {
            get { return _activeRowIdx; }
            set { SetProperty(ref _activeRowIdx, value, nameof(ActiveRowIdx)); }
        }

        public static T GetChildOfType<T>(DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) return null;
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                System.Diagnostics.Debug.WriteLine(child.GetType());
                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        protected override void OnActivate(bool isActive)
        {
            if (_tableControl == null)
            {
                _tableControl = GetChildOfType<TableControl>(this.Content);
            }

            base.OnActivate(isActive);
        }

        private ICommand _zoomToRowCommand = null;
        public ICommand ZoomToRowCommand
        {
            get
            {
                if (_zoomToRowCommand == null)
                {
                    _zoomToRowCommand = new RelayCommand(() =>
                    {
                        // if we have some content, a map and our data is added to the map
                        if (_tableControl?.TableContent != null && MapView.Active != null && SelectedMapMember is Layer)
                        {
                            // get the oid of the active row
                            var oid = _tableControl.GetObjectIdAsync(_tableControl.ActiveRowIndex).Result;
                            // load into an inspector to obtain the Shape
                            var insp = new Inspector();
                            insp.LoadAsync(SelectedMapMember, oid).ContinueWith((t) =>
                            {
                                // zoom
                                MapView.Active.ZoomToAsync(insp.Shape.Extent, new TimeSpan(0, 0, 0, 1));
                            });
                        }
                    });
                }
                return _zoomToRowCommand;
            }
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class TableControlDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            TableControlDockpaneViewModel.Show();
        }
    }
}
