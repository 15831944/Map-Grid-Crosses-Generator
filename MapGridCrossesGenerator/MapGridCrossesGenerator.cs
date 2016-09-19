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

            using (Database sourceDatabase = new Database(false, true))
            {
                sourceDatabase.ReadDwgFile("Resources//Map_Grid_Cross.dwg", System.IO.FileShare.ReadWrite, true, "");

                ObjectIdCollection ids = new ObjectIdCollection();
                using (Transaction sourceTransaction = sourceDatabase.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)sourceTransaction.GetObject(sourceDatabase.BlockTableId, OpenMode.ForRead);

                    if (bt.Has("MAP-GRID-CROSS"))
                    {
                        ids.Add(bt["MAP-GRID-CROSS"]);
                    }

                    sourceTransaction.Commit();
                }

                if (ids.Count != 0)
                {
                    IdMapping iMap = new IdMapping();

                    database.WblockCloneObjects(ids, database.BlockTableId, iMap, DuplicateRecordCloning.Ignore, false);
                }

            }

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
                BlockTable blockTable = database.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                BlockTableRecord blockDefinition = blockTable["MAP-GRID-CROSS"].GetObject(OpenMode.ForWrite) as BlockTableRecord;
                BlockTableRecord modelSpaceBlockTableRecord = blockTable[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForWrite) as BlockTableRecord;

                foreach (var cross in crosses)
                {
                    Point3d blockReferenceInsertPoint = new Point3d(cross.X, cross.Y, 0);

                    using (BlockReference blockReference = new BlockReference(blockReferenceInsertPoint, blockDefinition.ObjectId))
                    {
                        blockReference.ScaleFactors = new Scale3d(scale / 1000.0);

                        modelSpaceBlockTableRecord.AppendEntity(blockReference);

                        transaction.AddNewlyCreatedDBObject(blockReference, true);
                    }
                }

                transaction.Commit();
            }
        }
    }
}