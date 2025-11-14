using System.Collections.Generic;
using System.Linq;


namespace InspectorDemo
{
    internal class DemoProvider : ArcGIS.Desktop.Editing.InspectorProvider
    {
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
    }
}
