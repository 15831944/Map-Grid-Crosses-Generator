namespace MapGridCrossesGenerator
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Map;

    public class MapGridCrossesGenerator
    {
        [CommandMethod("DrawCrosses")]
        public static void DrawCrosses()
        {
            Database database = HostApplicationServices.WorkingDatabase;
            Editor editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

            PromptIntegerOptions promptIntegerOptions = new PromptIntegerOptions("Въведете мащаб: ")
            {
                DefaultValue = 1000,
                AllowNegative = false,
                AllowZero = false,
                UseDefaultValue = true,
                AllowNone = false
            };

            PromptIntegerResult promptIntegerResult = editor.GetInteger(promptIntegerOptions);
            if (promptIntegerResult.Status != PromptStatus.OK)
            {
                editor.WriteMessage("Невалиден мащаб");

                return;
            }

            int scale = promptIntegerResult.Value;

            PromptPointOptions firstPointOptions = new PromptPointOptions("\nИзберете долен ляв ъгъл на картата: ");

            PromptPointResult firstPointResult = editor.GetPoint(firstPointOptions);
            if (firstPointResult.Status != PromptStatus.OK)
            {
                editor.WriteMessage("Невалидна точка");

                return;
            }

            Point3d firstPoint = firstPointResult.Value;

            PromptPointOptions secondPointOptions = new PromptPointOptions("\nИзберете горен десен ъгъл на картата: ");

            PromptPointResult secondPointResult = editor.GetPoint(secondPointOptions);
            if (secondPointResult.Status != PromptStatus.OK)
            {
                editor.WriteMessage("Невалидна точка");

                return;
            }

            Point3d secondPoint = secondPointResult.Value;

            var crosses = MapGrid.GenerateCrosses(new Map.Point(firstPoint.X, firstPoint.Y), new Map.Point(secondPoint.X, secondPoint.Y), scale);

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord blockTableRecord = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                foreach (var cross in crosses)
                {
                    DBPoint point = new DBPoint(new Point3d(cross.X, cross.Y, 0));

                    blockTableRecord.AppendEntity(point);
                    transaction.AddNewlyCreatedDBObject(point, true);
                }

                transaction.Commit();
            }
        }
    }
}