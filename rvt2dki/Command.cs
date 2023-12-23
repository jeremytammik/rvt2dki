#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace rvt2dki
{
  [Transaction(TransactionMode.Manual)]
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
        Debug.Print($"{e.Name}: net {(a * foot2):#0.00}");
        ElementArea elarea = new ElementArea(e.Id.Value, a);
        wallAreas.Add(e.Id, elarea);
      }

      using (Transaction tx = new Transaction(doc))
      {
        tx.Start("Delete openings for gross wall area determination");
        DeleteAllCuttingElements(doc);
        doc.Regenerate();
        foreach (Element e in walls)
        {
          double a = e.get_Parameter(bipArea).AsDouble();
          Debug.Print($"{e.Name}: gross {(a * foot2):#0.00}");
          wallAreas[e.Id].AreaGross = a;
        }
        tx.RollBack();
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
        Debug.Print($"{e.Name}: {(a * foot2):#0.00}");
        ElementArea elarea = new ElementArea(e.Id.Value, a);
        roofAreas.Add(e.Id, elarea);

      }
      return Result.Succeeded;
    }

    /// <summary>
    /// Delete all elements that cut out of target elements, 
    /// to allow for calculation of gross material quantities.
    /// </summary>
    private void DeleteAllCuttingElements(Document doc)
    {
      FilteredElementCollector collector = new FilteredElementCollector(doc);

      // (Type == FamilyInstance && (Category == Door || Category == Window) || Type == Opening
      ElementClassFilter filterFamilyInstance = new ElementClassFilter(typeof(FamilyInstance));
      ElementCategoryFilter filterWindowCategory = new ElementCategoryFilter(BuiltInCategory.OST_Windows);
      ElementCategoryFilter filterDoorCategory = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
      LogicalOrFilter filterDoorOrWindowCategory = new LogicalOrFilter(filterWindowCategory, filterDoorCategory);
      LogicalAndFilter filterDoorWindowInstance = new LogicalAndFilter(filterDoorOrWindowCategory, filterFamilyInstance);

      ElementClassFilter filterOpening = new ElementClassFilter(typeof(Opening));

      LogicalOrFilter filterCuttingElements = new LogicalOrFilter(filterOpening, filterDoorWindowInstance);

      // Must convert to list, because we cannot iterate
      // over the collector and delete elements at the same time

      ICollection<Element> cuttingElementsList = collector.WherePasses(filterCuttingElements).ToElements();

      foreach (Element e in cuttingElementsList)
      {
        // Doors in curtain grid systems cannot be deleted.
        // This doesn't actually affect the calculations because
        // material quantities are not extracted for curtain systems.

        if (e.Category != null)
        {
          if (e.Category.BuiltInCategory == BuiltInCategory.OST_Doors)
          {
            FamilyInstance door = e as FamilyInstance;
            Element host = door.Host;

            if (null != host && host is Wall && ((Wall)host).CurtainGrid != null)
              continue;
          }
          ICollection<ElementId> deletedElements = doc.Delete(e.Id);

          // Log failed deletion attempts to the output.
          // There may be other situations where deletion is not possible
          // but the failure doesn't really affect the results.

          if (deletedElements == null || deletedElements.Count < 1)
          {
            Debug.Print("The tool was unable to delete the {0} named {2} (id {1})", 
              e.GetType().Name, e.Id, e.Name);
          }
        }
      }
    }
  }
}
