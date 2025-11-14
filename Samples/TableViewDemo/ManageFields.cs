using System;
using System.Collections.Generic;
using System.Linq;

using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;

namespace TableViewDemo
{
    internal class ManageFields : Button
    {
        protected async override void OnClick()
        {
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

            var tablePane = Module1.GetTablePaneForMapMember(layer);
            if (tablePane is ITablePaneEx tablePaneEx)
            {
                TableView tableView = tablePaneEx.TableView;
                if (tableView != null && tableView.IsReady)
                {
                    // set visible and hidden columns
                    tableView.ShowAllFields();
                    // adds to hidden
                    tableView.SetHiddenFields(new List<string> { "CREATEDBYRECORD", "RETIREDBYRECORD", "CalculatedArea", "MiscloseRatio", 
                        "MiscloseDistance", "IsSeed", "created_user", "create_date", "last_edited_user", "last_edited_date" });
                    // clear and reset frozen fields
                    var frozenFields = tableView.GetFrozenFields();
                    if (!frozenFields.Contains("Name"))
                    {
                        await tableView.ClearAllFrozenFieldsAsync();
                        await tableView.SetFrozenFieldsAsync(new List<string> { "OBJECTID", "Name" });
                    }
                }
            }
        }
    }
}
