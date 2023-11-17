using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;

namespace FirstRevitPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class Lab1PlaceGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Получение объектов приложения и документа
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            //Определение объекта-ссылки для занесения результата указания
            try
            {
                Reference pickedRef = null;
                //Указание группы
                Selection sel = uiApp.ActiveUIDocument.Selection;
                GroupPickFilter selFilter = new GroupPickFilter();
                pickedRef = sel.PickObject(ObjectType.Element, selFilter, "Выберите группу");
                Element elem = doc.GetElement(pickedRef);
                Group group = elem as Group;
                XYZ origin = GetElementCenter(group);
                Room room = GetRoomOfGroup(doc, origin);
                //Указание точки
                XYZ sourceCenter = GetRoomCenter(room);
                XYZ groupLocation = sourceCenter + new XYZ(13.12, 0, 0);

                string coords =
                 "X = " + sourceCenter.X.ToString() + "\r\n" +
                 "Y = " + sourceCenter.Y.ToString() + "\r\n" +
                 "Z = " + sourceCenter.Z.ToString();
                TaskDialog.Show("Центр исходной комнаты", coords);
                //Размещение группы
                Transaction trans = new Transaction(doc);
                trans.Start("Lab");
                doc.Create.PlaceGroup(sourceCenter, group.GroupType);
                trans.Commit();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
        public XYZ GetElementCenter(Element elem)
        {
            BoundingBoxXYZ bounding = elem.get_BoundingBox(null);
            XYZ center = (bounding.Max + bounding.Min) * 0.5;
            return center;
        }
        Room GetRoomOfGroup(Document doc, XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);
            Room room = null;
            foreach (Element elem in collector)
            {
                room = elem as Room;
                if (room != null)
                {
                    // Точка в указанной комнате?
                    if (room.IsPointInRoom(point))
                    {
                        break;
                    }
                }
            }
            return room;
        }
        public XYZ GetRoomCenter(Room room)
        {
            // Получение центра комнаты
            XYZ boundCenter = GetElementCenter(room);
            LocationPoint locPt = (LocationPoint)room.Location;
            XYZ roomCenter =
            new XYZ(boundCenter.X, boundCenter.Y, locPt.Point.Z);
            return roomCenter;
        }

    }
    public class GroupPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element e)
        {
            return (e.Category.Id.IntegerValue.Equals(
            (int)BuiltInCategory.OST_IOSModelGroups));
        }
        public bool AllowReference(Reference r, XYZ p)
        {
            return false;
        }
    }

}
