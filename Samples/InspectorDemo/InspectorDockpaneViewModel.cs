using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Button = ArcGIS.Desktop.Framework.Contracts.Button;

namespace InspectorDemo
{
    internal class InspectorDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "InspectorDemo_InspectorDockpane";

        protected InspectorDockpaneViewModel() 
        {
            // the standard way of creating an inspector
            #region Inspector UI with out-of-box Provider
            //TODO: Use this line to create an inspector using the out-of-box provider
            // but make sure to comment out the 'Inspector UI with Provider' code below
            //_featureInspector = new Inspector();
            #endregion Inspector UI with out-of-box Provider

            #region Inspector UI with Provider
            //TODO: this block creates a customized inspector based on the provider
            // this allows to customize the grid view
            // but make sure to comment out the 'Inspector UI with out-of-box Provider' code above
            //create the custom provider
            DemoProvider provider = new DemoProvider();
            //create the inspector from the provider
            _featureInspector = provider.Create();
            #endregion Inspector UI with Provider

            // create the embeddable control from the inspector (to display on the pane)
            var icontrol = _featureInspector.CreateEmbeddableControl();

            // get view and viewmodel from the inspector
            InspectorViewModel = icontrol.Item1;
            InspectorView = icontrol.Item2;
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

        #region Binding properties: Business logic

        // Binding property: Property containing an instance for the inspector.
        private readonly Inspector _featureInspector = null;
        public Inspector AttributeInspector => _featureInspector;

        // Binding property: Property containing the inspector viewmodel.
        private EmbeddableControl _inspectorViewModel = null;
        public EmbeddableControl InspectorViewModel
        {
            get => _inspectorViewModel;
            set
            {
                if (value != null)
                {
                    _inspectorViewModel = value;
                    //Occurs when the control is hosted - this is where the control is opened
                    _inspectorViewModel.OpenAsync();
                }
                else if (_inspectorViewModel != null)
                {
                    //Occurs when the control is closed
                    _inspectorViewModel.CloseAsync();
                    _inspectorViewModel = value;
                }
                SetProperty(ref _inspectorViewModel, value, () => InspectorViewModel); //check this?
            }
        }

        // Binding property: Property containing the inspector view.
        private UserControl _inspectorView = null;
        public UserControl InspectorView
        {
            get => _inspectorView;
            set => SetProperty(ref _inspectorView, value, () => InspectorView);
        }

        // Binding property: Property containing the treeview itemsource
        private Dictionary<MapMember, List<long>> _selectedMapFeatures = new Dictionary<MapMember, List<long>>();
        public Dictionary<MapMember, List<long>> SelectedMapFeatures
        {
            get => _selectedMapFeatures;
            set => SetProperty(ref _selectedMapFeatures, value, () => SelectedMapFeatures);
        }

        #endregion


        #region Apply/Cancel business logic
        public bool IsApplyEnabled => AttributeInspector?.IsDirty ?? false;
        public bool IsCancelEnabled => AttributeInspector?.IsDirty ?? false;

        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new RelayCommand(OnCancel);
                return _cancelCommand;
            }
        }

        internal void OnCancel()
        {
            AttributeInspector?.Cancel();
        }

        private ICommand _applyCommand;
        public ICommand ApplyCommand
        {
            get
            {
                if (_applyCommand == null)
                    _applyCommand = new RelayCommand(OnApply);
                return _applyCommand;
            }
        }

        internal void OnApply()
        {
            QueuedTask.Run(() =>
            {
                //Apply the attribute changes.
                //Writing them back to the database in an Edit Operation.
                AttributeInspector?.Apply();
            });
        }
        #endregion


        private bool _subscribed = false;
        protected override async void OnShow(bool isVisible)
        {
            try
            {
                if (MapView.Active == null)
                {
                    if (isVisible) this.Hide();
                    return;
                }

                SelectedMapFeatures = null;

                if (isVisible)
                {
                    if (!_subscribed)
                    {
                        MapSelectionChangedEvent.Subscribe(OnMapSelectionChanged);
                        _subscribed = true;
                    }

                    var map = MapView.Active.Map;
                    var selectionDictionary = await QueuedTask.Run(() =>
                    {
                        //get the selected features
                        return map.GetSelection().ToDictionary();
                    });
                    // assign the dictionary to the view model - notifies the tree view to update
                    SelectedMapFeatures = selectionDictionary;

                    if (AttributeInspector != null)
                        AttributeInspector.PropertyChanged += FeatureInspector_PropertyChanged;
                }
                else
                {
                    if (_subscribed)
                    {
                        MapSelectionChangedEvent.Unsubscribe(OnMapSelectionChanged);
                        _subscribed = false;
                    }

                    if (AttributeInspector != null)
                        AttributeInspector.PropertyChanged -= FeatureInspector_PropertyChanged;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString(), System.Reflection.Assembly.GetExecutingAssembly().FullName);
            }
            base.OnShow(isVisible);
        }

        private void FeatureInspector_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDirty")
            {
                NotifyPropertyChanged(nameof(IsApplyEnabled));
                NotifyPropertyChanged(nameof(IsCancelEnabled));
            }
        }

        private async void OnMapSelectionChanged(MapSelectionChangedEventArgs args)
        {
            if (AttributeInspector == null) return;

            if (args.Map != MapView.Active.Map) return;

            var selectionDictionary = await QueuedTask.Run(() =>
            {
                //get the selected features
                return args.Map.GetSelection().ToDictionary();
            });

            SelectedMapFeatures = selectionDictionary;
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class InspectorDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            InspectorDockpaneViewModel.Show();
        }
    }
}
