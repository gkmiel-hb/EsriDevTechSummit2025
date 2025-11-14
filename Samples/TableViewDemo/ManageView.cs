using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableViewDemo
{
    internal class ManageView : Button
    {
        protected override async void OnClick()
        {
            if (MapView.Active == null)
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
                    await tableView.SetViewMode(TableViewMode.eSelectedRecords);
                    tableView.SetZoomLevel(150);
                }
            }
        }
    }
}
