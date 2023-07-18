#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace RAA_M01C
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //Declare variables
            int numLevels = 250;
            double startElevation = 0;
            int floorHeight = 15;

            //Get view family types
            FilteredElementCollector vftCollector = new FilteredElementCollector(doc);
            vftCollector.OfClass(typeof(ViewFamilyType));

            ViewFamilyType fpVFT = null;
            ViewFamilyType cpVFT = null;


            foreach (ViewFamilyType curVFT in vftCollector)
            {
                if (curVFT.ViewFamily == ViewFamily.FloorPlan)
                {
                    fpVFT = curVFT;
                }
                else if (curVFT.ViewFamily == ViewFamily.CeilingPlan)
                {
                    cpVFT = curVFT;
                }
            }

            //Get titleblock 
            FilteredElementCollector titleBCollector = new FilteredElementCollector(doc);
            titleBCollector.OfCategory(BuiltInCategory.OST_TitleBlocks);
            ElementId tblockId = titleBCollector.FirstElementId();


            // Modify document within a transaction

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("FizzBuzz");

                //Counters
                int fizzbuzzCounter = 0;
                int fizzCounter = 0;
                int buzzCounter = 0;

                //4 Check Fizz Buzz
                for(int i = 0; i < numLevels; i++)
                {

                    //Create levels
                    Level curLevel = Level.Create(doc, startElevation);
                    curLevel.Name = "RAA_Level " + i.ToString();
                    startElevation += floorHeight;


                    //Check FizzBuzz
                    if (i % 3 == 0 && i % 5 ==0)
                    {
                        //Create floor plan
                        ViewPlan newFloorPlan = ViewPlan.Create(doc, fpVFT.Id, curLevel.Id);
                        newFloorPlan.Name = "FizzBuzz - " + i.ToString();

                        //Create sheet
                        ViewSheet newSheet = ViewSheet.Create(doc, tblockId);
                        newSheet.SheetNumber = "C01" + i.ToString();
                        newSheet.Name = "FizzBuzz - " + i.ToString();

                        //Create viewport
                        XYZ locPoint = new XYZ(1, 1, 0);
                        Viewport newVP = Viewport.Create(doc, newSheet.Id, newFloorPlan.Id, locPoint);
                        
                        fizzbuzzCounter++;
                    }

                    else if (i % 3 == 0)
                    {
                        //Fizz ---- create a floor plan
                        ViewPlan curFP = ViewPlan.Create(doc, fpVFT.Id, curLevel.Id);
                        curFP.Name = "Fizz - " + i.ToString();
                        fizzCounter++;
                    }

                    else if (i % 5 == 0)
                    {
                        //Buzz ---- create a ceiling plan view
                        ViewPlan curCeilingPlan = ViewPlan.Create(doc, cpVFT.Id, curLevel.Id);
                        curCeilingPlan.Name = "Buzz - " + i.ToString();
                        buzzCounter++;
                    }

                }

                tx.Commit();
                tx.Dispose();

                //Alert user
                TaskDialog.Show("RAA", $"Created {numLevels} levels. {fizzbuzzCounter} Sheets. {fizzCounter} Floor Plans. and {buzzCounter} Ceiling Plans ");
            }

            return Result.Succeeded;
        }
    }
}
