using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace InspectorDemo
{
    /// <summary>
    /// Interaction logic for InspectorDockpaneView.xaml
    /// </summary>
    public partial class InspectorDockpaneView : UserControl
    {
        public InspectorDockpaneView()
        {
            InitializeComponent();
        }

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

    }
}
