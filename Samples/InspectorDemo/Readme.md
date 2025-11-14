# 1. Create project
We have to start demo from creating new ArcGIS Pro module Add-in project 
InspectorDemo
# 2. Adding dockpane
At first we need to add Dockpane where Inspector control will be placed
# 3. Adding ArcGIS Pro tools
I am going to add few standard ArcGIS Pro tools to save time for demonstration:

```xaml

          <button refID="esri_mapping_selectByRectangleTool" size="middle"/>
          <button refID="esri_mapping_clearSelectionButton" size="middle" />

```

I have added "select by rectangle" and "clear selection" tools as we need them to test selection changing in Inspector

# 4. Add controls to xaml
I will update dockpane xaml content from default to demo

```xaml

        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        <StackPanel Grid.Row="0" Margin="0,6" Orientation="Vertical">
            <TextBlock Text="Features:" Style="{Binding Esri_TextBlockH3}" FontWeight="Bold"/>
            <!--WPF Treeview control-->
            <TreeView x:Name="treeView"  ItemsSource="{Binding SelectedMapFeatures}" VerticalAlignment="Top" Width="Auto" Margin="0,6"
                      SelectedItemChanged="treeView_SelectedItemChanged">
                <TreeView.ItemTemplate >
                    <HierarchicalDataTemplate ItemsSource="{Binding Path=Value}">
                        <TreeViewItem Header="{Binding Path=Key}" Foreground="{DynamicResource Esri_Gray155}" FontStyle="Italic"/>
                        <HierarchicalDataTemplate.ItemTemplate>
                            <DataTemplate>
                                <TextBlock Text="{Binding }" />
                            </DataTemplate>
                        </HierarchicalDataTemplate.ItemTemplate>
                    </HierarchicalDataTemplate>
                </TreeView.ItemTemplate>
            </TreeView>
        </StackPanel>
        <Grid Grid.Row="1" Margin="0,6">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto" />
                <RowDefinition Height="*" />
            </Grid.RowDefinitions>
            <TextBlock Text="Attributes:" Style="{Binding Esri_TextBlockH3}" FontWeight="Bold"/>
            <!--This is the ArcGIS Pro SDK Inspector UI-->
            <ContentPresenter Grid.Row="1" Margin="0,6" Content="{Binding InspectorView}"
                              ScrollViewer.VerticalScrollBarVisibility="Visible" ScrollViewer.CanContentScroll="True"/>
        </Grid>
        <StackPanel Grid.Row="2" Orientation="Horizontal" Background="Transparent" Margin="0,6">
            <Button Margin="6,0,0,0" Content="Apply" Style="{DynamicResource Esri_Button}"
               IsEnabled="{Binding IsApplyEnabled}" Command="{Binding ApplyCommand}"/>
            <Button Margin="6,0,0,0" Content="Cancel" Style="{DynamicResource Esri_Button}"
               IsEnabled="{Binding IsCancelEnabled}" Command="{Binding CancelCommand}"/>
        </StackPanel>

```

I have added to dockpane content TreeView to show selected map objects. Then we have ContentPresenter control which is binded to InspectorView property, which enables to show selected object attributes. Last of controls are Buttons (Apply and Cancel), which allows manage editing session.

# 5. Setup bindings for TreeView and Inspector
 Let's go to dockpane ViewModel class and add some business logic for TreeView and ContentPresenter:

```c#
        #region Binding properties: Business logic

        // Binding property: Property containing the treeview itemsource
        private Dictionary<MapMember, List<long>> _selectedMapFeatures = new Dictionary<MapMember, List<long>>();
        public Dictionary<MapMember, List<long>> SelectedMapFeatures
        {
            get => _selectedMapFeatures;
            set => SetProperty(ref _selectedMapFeatures, value, () => SelectedMapFeatures);
        }

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

        #endregion
```

SelectedMapFeatures property is used as data source for TreeView and it contains selected map objects dictionary.
AttributeInspector, InspectorViewModel, InspectorView properties are needed for inspector part functionality.
They are related to each other. How are they related, we will see in next steps.


# 6. Create Inspector object
I will set inspector related properties in constructor of dockpane:

```c#
            // the standard way of creating an inspector
            #region Inspector UI with out-of-box Provider
            //TODO: Use this line to create an inspector using the out-of-box provider
            // but make sure to comment out the 'Inspector UI with Provider' code below
            _featureInspector = new Inspector();
            #endregion Inspector UI with out-of-box Provider

            #region Inspector UI with Provider
            //TODO: this block creates a customized inspector based on the provider
            // this allows to customize the grid view
            // but make sure to comment out the 'Inspector UI with out-of-box Provider' code above

            //create the custom provider
            //DemoProvider provider = new DemoProvider();
            //create the inspector from the provider
            //_featureInspector = provider.Create();
            #endregion Inspector UI with Provider

            // create the embeddable control from the inspector (to display on the pane)
            var icontrol = _featureInspector.CreateEmbeddableControl();

            // get view and viewmodel from the inspector
            InspectorViewModel = icontrol.Item1;
            InspectorView = icontrol.Item2;
```

In the first part of Inspector demo we create Inspector object using out-of-box provider. We will use custom inspector provider in second part of demo. Using created inspector we create EmbedabbleControl, which has properties associated to InspectorViewModel and InspectorView.

# 7. Setup buttons bindings
We have 2 buttons in our inspector dockpane so we need to add some business logic for buttons

```c#
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
```


Depending on button, code calls AttributeInspector relevant method: Cancel or Apply.

# 8. Override OnShow method
I will override OnShow method of dockpane to organize TreeView update on map selection changing and Inspector Buttons status update on attribute editing.

```c#
        private bool _subscribed = false;

        protected override async void OnShow(bool isVisible)
        {
                if (MapView.Active?.Map == null)
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

            base.OnShow(isVisible);
        }
```

Then dockpane starts be visible we subscribe to MapSelectionChangedEvent and PropertyChanged events and unsubscribe on hiding

# 9. Add action for MapSelectionChanged
I will add code for MapSelectionChanged Event handler action

```c#
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
```

New selection will be set to SelectedMapFeatures property

# 10. Add action for PropertyChanged
Now is time for AttributeInspector PropertyChanged event handling action

```c#
        private void FeatureInspector_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDirty")
            {
                NotifyPropertyChanged(nameof(IsApplyEnabled));
                NotifyPropertyChanged(nameof(IsCancelEnabled));
            }
        }
```


Code will update Apply and Cancel buttons statuses(Enable/Disable) depending on property IsDirty.

# 11. Add SelectedItemChanged in code behind
For TreeView we need to setup SelectedItemChanged event handler in code behind

```c#
        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var vm = this.DataContext as InspectorDockpaneViewModel;

            var layerTreeView = sender as TreeView;

            var currentItem = layerTreeView.Items.CurrentItem;
            if (currentItem == null)
            {
                vm.AttributeInspector?.ClearAsync();
                return;
            }
            var selection = (KeyValuePair<MapMember, List<long>>)currentItem;
            var layer = selection.Key;
            var selectedTreeviewItem = layerTreeView.SelectedItem;

            if (selectedTreeviewItem is long)
            {
                // load it
                var selectedOID = Convert.ToInt64(selectedTreeviewItem);
                //Load the selected feature into the inspector
                vm.AttributeInspector?.LoadAsync(layer, selectedOID);
            }
            // else layer is selected.  if there's some OIDs, clear the inspector
            else if (selection.Value.Count > 0)
            {
                if (MapView.Active?.Map == null) return;
                var map = MapView.Active.Map;
                QueuedTask.Run(() =>
                {
                    //get the selected features
                    var selectionInMap = map.GetSelection().ToDictionary();

                    if (layer == null) return;
                    if (selectionInMap.ContainsKey(layer))
                    {
                        //get the selected features oid list
                        vm.AttributeInspector.LoadAsync(layer, selectionInMap[layer].ToList());
                    }
                });
            }
        }
```

Depending on current selected TreeView item, code loads one feature attributes or all layer selected features attributes.

# 12. Now we can test code in ArcGIS Pro.

# 13. Custom Provider for Inspector
To add Custom provider to Inspector we will continue on our previous demo project. We need to create class DemoProvider which is derived from ArcGIS.Desktop.Editing.InspectorProvider class. Let's create it.

```
  ArcGIS.Desktop.Editing.InspectorProvider
```

# 14. Setup DemoProvider business logic
I will add some business logic to our DemoProvider class

```c#
        //Override this to highlight specific attributes
        public override bool? IsHighlighted(ArcGIS.Desktop.Editing.Attributes.Attribute attr)
        {
            if (attr.FieldName == "Name")
                return true;

            return false;
        }

        private readonly List<string> _fieldsToHide = new List<string>()
        {
            "GlobalID",
            "Shape",
            "Shape_Length",
            "Shape_Area",
            "PLSSID",
        };

        //Override this to hide specific attributes
        public override bool? IsVisible(ArcGIS.Desktop.Editing.Attributes.Attribute attr)
        {
            foreach (var item in _fieldsToHide)
            {
                if (attr.FieldName == item)
                    return false;
            }
            return true;
        }

        private readonly List<string> _fieldOrder = new List<string>()
        {
            "OBJECTID",
            "Name",
            "StatedArea",
            "CalculatedArea"
        };

        //Override this to display the attributes in a specific order
        public override IEnumerable<ArcGIS.Desktop.Editing.Attributes.Attribute> AttributesOrder(IEnumerable<ArcGIS.Desktop.Editing.Attributes.Attribute> attrs)
        {
            var newList = new List<ArcGIS.Desktop.Editing.Attributes.Attribute>();
            foreach (var field in _fieldOrder)
            {
                if (attrs.Any(a => a.FieldName == field))
                {
                    newList.Add(attrs.First(a => a.FieldName == field));
                }
            }
            return newList;
        }

        //Override this to display the attributes with a custom alias
        public override string CustomName(ArcGIS.Desktop.Editing.Attributes.Attribute attr)
        {
            if (attr.FieldName == "Type")
                return "Parcel type";

            return attr.FieldName;
        }

        //Override this to make specific attributes read-only
        public override bool? IsEditable(ArcGIS.Desktop.Editing.Attributes.Attribute attr)
        {
            if (attr.FieldName == "StatedAreaUnit" || attr.FieldName == "created_user" || 
                attr.FieldName == "create_date" || attr.FieldName == "last_edited_user" || 
                attr.FieldName == "last_edited_date")
                return false;

            return true;
        }
```

Our inspector provider will highlight some fields, some fields will be hidden. It will make some field ordering, rename some attribute names and disable editing on some fields. It is not required what your inspector provider must have all these features. You can leave Inspector provider features you need.

# 15. Update Inspector creation
Now we need to update creation of Inspector. I will comment default Inspector constructor and uncomment code for getting inspector from custom provider.

# 16. Test code in ArcGIS Pro
