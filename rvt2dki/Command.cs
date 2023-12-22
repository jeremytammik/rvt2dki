#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace rvt2dki
{
  [Transaction(TransactionMode.ReadOnly)]
  public class Command : IExternalCommand
  {
    const BuiltInParameter bipArea 
      = BuiltInParameter.HOST_AREA_COMPUTED;
    const double inch = 0.0254; // metres
    const double foot = 12 * inch;
    const double foot2 = foot * foot;

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;

      Dictionary<ElementId, ElementArea> wallAreas
        = new Dictionary<ElementId, ElementArea>();

      FilteredElementCollector walls
        = new FilteredElementCollector(doc)
          .WhereElementIsNotElementType()
          .OfCategory(BuiltInCategory.OST_Walls);

      Debug.Print($"{walls.GetElementCount()} walls:");
      foreach (Element e in walls)
      {
        double a = e.get_Parameter(bipArea).AsDouble();
        Debug.Print($"{e.Name}: {(a * foot2):#0.00}");
        ElementArea elarea = new ElementArea(e.Id.Value, a);
        wallAreas.Add(e.Id, elarea);
      }

      Dictionary<ElementId, ElementArea> floorAreas
        = new Dictionary<ElementId, ElementArea>();

      FilteredElementCollector floors
        = new FilteredElementCollector(doc)
          .WhereElementIsNotElementType()
          .OfCategory(BuiltInCategory.OST_Floors);

      Debug.Print($"{floors.GetElementCount()} floors:");
      foreach (Element e in floors)
      {
        double a = e.get_Parameter(bipArea).AsDouble();
        Debug.Print($"{e.Name}: {(a * foot2):#0.00}");
        ElementArea elarea = new ElementArea(e.Id.Value, a);
        wallAreas.Add(e.Id, elarea);
      }

      Dictionary<ElementId, ElementArea> roofAreas
        = new Dictionary<ElementId, ElementArea>();

      FilteredElementCollector roofs
        = new FilteredElementCollector(doc)
          .WhereElementIsNotElementType()
          .OfCategory(BuiltInCategory.OST_Roofs);

      Debug.Print($"{roofs.GetElementCount()} roofs:");
      foreach (Element e in roofs)
      {
        double a = e.get_Parameter(bipArea).AsDouble();
        Debug.Print($"{e.Name}: {(a*foot2):#0.00}");
        ElementArea elarea = new ElementArea(e.Id.Value, a);
        roofAreas.Add(e.Id, elarea);

      }
      return Result.Succeeded;
    }
  }
}
