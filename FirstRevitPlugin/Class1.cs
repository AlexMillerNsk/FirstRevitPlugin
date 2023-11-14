using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autodesk.Revit.UI;

namespace FirstRevitPlugin
{
    [Autodesk.Revit.Attributes.TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class FirstRevitCommand : IExternalCommand
    {
        static AddInId addinId = new AddInId(new Guid("2421F661-DB8D-4D7F-A005-EBA8BA15BFD7"));
        public Result Execute(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Reference myRef = uidoc.Selection.PickObject(ObjectType.Element, "Выберите элемент для вывода его Id");
            Element element = doc.GetElement(myRef);
            ElementId id = element.Id;
            TaskDialog.Show("Hello world!", id.ToString());
            var view = new UserControl1();
            view.ShowDialog();
            return Result.Succeeded;
        }
    }
}
