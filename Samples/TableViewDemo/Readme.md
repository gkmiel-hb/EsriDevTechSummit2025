# 1. Create project
We are starting demo with new ArcGIS Pro Add-in module project "TableViewDemo"

# 2. Add button to open table pane
Now I will add button for table pane opening. OpenTablePane

# 3. Add code for created button
Let's open tool class code and fill OnClick method:

```c#
            if (MapView.Active?.Map == null)
            {
                return;
            }

            // activate the Tax table view as the active view
            var layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault();
            if (layer == null)
            {
                return;
            }

            Module1.OpenAndActivateTablePane(layer);
```

We will add OpenAndActivateTablePane method code to Add-in module class. It would be accessible for our next tool too.

# 4. Add method to Module1.cs
Open Module1.cs file. Add content of OpenAndActivateTablePane method

```c#
        /// <summary>
        /// utility function to open and activate the TablePane for a given MapMember
        /// </summary>
        /// <param name="mapMember">table to have the table view activated</param>
        internal static ITablePane OpenAndActivateTablePane(MapMember mapMember)
        {
            // check the open panes to see if it's open but just needs activating
            IEnumerable<ITablePane> tablePanes = FrameworkApplication.Panes.OfType<ITablePane>();
            foreach (var tablePane in tablePanes)
            {
                if (tablePane.MapMember != mapMember) continue;
                var pane = tablePane as Pane;
                pane?.Activate();
                return pane as ITablePane;
            }

            // it's not currently open... so open it
            if (FrameworkApplication.Panes.CanOpenTablePane(mapMember))
            {
                var iTablePane = FrameworkApplication.Panes.OpenTablePane(mapMember);
                return iTablePane;
            }

            return null;
        }
```

Code checks all open panes to see if it's open but just needs activating. If table pane for layer was not opened before, it opens new table pane

# 5. Add button ManageTableView
Let's create another one button which will change TableView

# 6. Add code for new created button
I will add code to OnClick method:

```c#
            if (MapView.Active?.Map == null)
            {
                return;
            }

            // activate the Tax table view as the active view
            var layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault();
            if (layer == null)
            {
                return;
            }

            var tablePane = Module1.OpenAndActivateTablePane(layer);
            if (tablePane is ITablePaneEx tablePaneEx)
            {
                TableView tableView = tablePaneEx.TableView;
                if (tableView != null && tableView.IsReady)
                {
                    // set visible and hidden columns
                    tableView.ShowAllFields();
                    // adds to hidden
                    tableView.SetHiddenFields(new List<string> { "CREATEDBYRECORD", "RETIREDBYRECORD", "CalculatedArea", "MiscloseRatio", "MiscloseDistance", "IsSeed", "created_user", "created_date", "last_edited_user", "last_edited_date" });
                    // clear and reset frozen fields
                    var frozenFields = tableView.GetFrozenFields();
                    if (!frozenFields.Contains("Name"))
                    {
                        await tableView.ClearAllFrozenFieldsAsync();
                        await tableView.SetFrozenFieldsAsync(new List<string> { "OBJECTID", "Name" });

		    // show selected items only
                    await tableView.SetViewMode(TableViewMode.eSelectedRecords);
		// zoom 
                    tableView.SetZoomLevel(150);


                    }
                }
            }
```

Added code will hide some fields from the list, freeze OBJECTID and Name fields.
Then switch tavle view to show only selected mode and zoom tableview

# 7. Test in ArcGIS Pro
