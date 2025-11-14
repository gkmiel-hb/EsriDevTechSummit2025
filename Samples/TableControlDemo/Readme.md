# 1. Create new project
As usual we will start demo by creating project from scratch

```
    TableControlDemo
```

# 2. Add dockpane
Now we will add dockpane TableControlDockpane.

# 3. Setup dockpane docking
Open daml and change docking to dock="bottom". That type of docking will allow us to see both panes (catalog and demo) at the same time

# 4. Add controls to dockpane
Let's open dockpane xaml file. We will add namespace of ArcGIS.Desktop.Editing class which contains TableControl

```xaml
             xmlns:editing="clr-namespace:ArcGIS.Desktop.Editing.Controls;assembly=ArcGIS.Desktop.Editing"
```

Now I will replace default dockpane content with demo content

```xaml
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        <DockPanel LastChildFill="False" Grid.Row="0">
            <Button Content="Toggle Sel" Command="editing:TableControlCommands.SwitchSelection"
              CommandTarget="{Binding ElementName=tableControl}"
              Style="{DynamicResource Esri_Button}"/>

            <Button Content="Select All" Command="editing:TableControlCommands.SelectAll"
              CommandTarget="{Binding ElementName=tableControl}"
              Style="{DynamicResource Esri_Button}"/>

            <Button Content="Add To Map" Command="{Binding AddToMapCommand}"
              Style="{DynamicResource Esri_Button}"/>

            <Button Content="Find" Command="editing:TableControlCommands.Find"
              CommandTarget="{Binding ElementName=tableControl}"
              Style="{DynamicResource Esri_Button}"/>
        </DockPanel>
        <editing:TableControl x:Name="tableControl" Grid.Row="1" TableContent="{Binding Path=TableContent}" VerticalAlignment="Stretch" HorizontalAlignment="Stretch" 
					RowContextMenu="{Binding RowContextMenu}" SelectedRowContextMenu="{Binding RowContextMenu}" >
        </editing:TableControl>
```

My demo content consists of buttons group and TableControl
Some buttons will execute standard TableControl functionality as selection switching, selecting all rows, finding row. One button will implement custom functionality. In our case it would be adding project item to active map content. Loading to map is needed for our context menu action.

Buttons CommandTarget property must be bind to our TableControl using ElementName

TableControl rows could have different Context menus for selected and unselected rows. We will use same functionality for both cases in our demo.

# 5. Add business logic for TableControl in ViewModel. 
Let's add some properties for TableControl binding and accessing

```c#
    private TableControlContent _tableContent;
    public TableControlContent TableContent
    {
      get { return _tableContent; }
      set { SetProperty(ref _tableContent, value); }
    }

    private TableControl _tableControl;
    public TableControl TableControl 
    {
        get { return _tableControl; }
        set { SetProperty(ref _tableControl, value); }
    }

    private Item _selectedItem = null;
    public bool IsItemSelected
    {
        get { return _selectedItem != null; }
    }

    private MapMember SelectedMapMember = null;

    public ContextMenu RowContextMenu { get; set; }
```

TableControlContent property defines the content source for the TableControl.
TableControl property will be used to access TableControl from ViewModel
SelectedMapMember will be used for storing loaded layer from TableControl.
_selectedItem stores current selected project item in Catalog pane.
RowContextMenu property is used for TableControl binding. We will create ContextMenu in next step. 

# 6. Setup TableControl loading
TableControl needs content to show. I will use ProjectWindowSelectedItemsChangedEvent to get selected project item in catalog pane and create content from it for TableControl. I will add ProjectWindowSelectedItemsChangedEvent handling to ViewModel constructor

```c#
    ProjectWindowSelectedItemsChangedEvent.Subscribe(OnProjectWindowSelectedItem);
```

Now I will add action code for the event handler

```c#
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
```

Code gets first selected item in catalog pane and creates TableControlContent from it using TableControlContentFactory

# 7. Get TableControl
TableControlControl is set, It's time for TableControl property. There are few different ways to get TableControl in ViewModel. All of them are not MVVM style. In my demo I use getting control from Visual Tree by control type and I will do it on dockpane activation.

```c#
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
```

# 8. Add ContextMenu
Another one property we need to set is RowContextMenu. We will create ContextMenu in ViewModel constructor

```c#
            MenuItem zoomItem = new MenuItem()
            {
                Header = "Zoom to Feature",
                Command = ZoomToRowCommand,
                CommandParameter = this
            };
            RowContextMenu = new ContextMenu();
            RowContextMenu.Items.Add(zoomItem);
```

Our ContextMenu will have one item with ZoomToRowCommand

# 9. Setup "Add to Map" button
Next step is add business logic for Add to Map button

```c#
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
```

Command code will create FeatureLayer or StandaloneTable depending of project item data type and assign it to SelectedMapMember

# 10. Create Command for Menuitem
And last step before testing functionality in ArcGIS Pro is to add business logic for ContextMenu item command

```c#
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
```

 ZoomToRowCommand will zoom map to object extent. At first using TableControl we will get ObjectID from current active row. After using Inspector we will get object extent.

# 11. Test in ArcGIS Pro


