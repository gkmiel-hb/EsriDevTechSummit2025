using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

namespace TableViewDemo
{
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("TableViewDemo_Module");

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

        /// <summary>
        /// utility function to find the TablePane for a given MapMember
        /// </summary>
        /// <param name="mapMember">table to have the table view activated</param>
        internal static ITablePane GetTablePaneForMapMember(MapMember mapMember)
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
                return FrameworkApplication.Panes.OpenTablePane(mapMember);
            }
            return null;
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
