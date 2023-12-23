#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion

namespace rvt2dki
{
  [Transaction(TransactionMode.Manual)]
  public class Command : IExternalCommand
  {
    const BuiltInParameter bipArea
      = BuiltInParameter.HOST_AREA_COMPUTED;
    const BuiltInParameter bipComment
      = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS;

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

      Dictionary<ElementId, ElementArea> elAreas
        = new Dictionary<ElementId, ElementArea>();

      // Determine net wall, floor and roof areas

      FilteredElementCollector walls
        = new FilteredElementCollector(doc)
          .WhereElementIsNotElementType()
          .OfCategory(BuiltInCategory.OST_Walls);

      Debug.Print($"{walls.GetElementCount()} walls:");
      foreach (Element e in walls)
      {
        double a = e.get_Parameter(bipArea).AsDouble();
        string c = e.get_Parameter(bipComment).AsString();
        Debug.Print($"{c} - {e.Name}: net {(a * foot2):#0.00}");
        ElementArea elarea = new ElementArea(e.Id.Value, c, a);
        elAreas.Add(e.Id, elarea);
      }

      FilteredElementCollector floors
        = new FilteredElementCollector(doc)
          .WhereElementIsNotElementType()
          .OfCategory(BuiltInCategory.OST_Floors);

      Debug.Print($"{floors.GetElementCount()} floors:");
      foreach (Element e in floors)
      {
        double a = e.get_Parameter(bipArea).AsDouble();
        string c = e.get_Parameter(bipComment).AsString();
        Debug.Print($"{c} - {e.Name}: net {(a * foot2):#0.00}");
        ElementArea elarea = new ElementArea(e.Id.Value, c, a);
        elAreas.Add(e.Id, elarea);
      }

      FilteredElementCollector roofs
        = new FilteredElementCollector(doc)
          .WhereElementIsNotElementType()
          .OfCategory(BuiltInCategory.OST_Roofs);

      Debug.Print($"{roofs.GetElementCount()} roofs:");
      foreach (Element e in roofs)
      {
        double a = e.get_Parameter(bipArea).AsDouble();
        string c = e.get_Parameter(bipComment).AsString();
        Debug.Print($"{c} - {e.Name}: net {(a * foot2):#0.00}");
        ElementArea elarea = new ElementArea(e.Id.Value, c, a);
        elAreas.Add(e.Id, elarea);
      }

      // Determine gross wall and roof areas

      using (Transaction tx = new Transaction(doc))
      {
        tx.Start("Delete openings for gross wall and roof area determination");
        DeleteAllCuttingElements(doc);
        doc.Regenerate();
        foreach (Element e in walls)
        {
          double a = e.get_Parameter(bipArea).AsDouble();
          Debug.Print($"{e.Name}: gross {(a * foot2):#0.00}");
          elAreas[e.Id].AreaGross = a;
        }
        foreach (Element e in roofs)
        {
          double a = e.get_Parameter(bipArea).AsDouble();
          Debug.Print($"{e.Name}: gross {(a * foot2):#0.00}");
          elAreas[e.Id].AreaGross = a;
        }
        tx.RollBack();
      }

      // Sort results

      SortedDictionary<string, List<ElementArea>> resultAreas
        = new SortedDictionary<string, List<ElementArea>>();

      foreach (ElementArea ea in elAreas.Values)
      {
        string c = ea.Comment;
        if(null == c)
        {
          c = "<nil>";
        }
        if(!resultAreas.ContainsKey(c))
        {
          resultAreas[c] = new List<ElementArea>();
        }
        resultAreas[c].Add(ea);
      }

      // Print results

      foreach (string key in resultAreas.Keys)
      {
        int n = resultAreas[key].Count;
        List<ElementArea> eas = resultAreas[key];
        double anet = eas.Sum<ElementArea>(ea => ea.AreaNet);
        double agross = eas.Sum<ElementArea>(ea => ea.AreaGross);
        Debug.Print($"{key}: {n} elements, area gross/net {agross * foot2:0.0}/{anet * foot2:0.0}");
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
