﻿namespace MapGridCrossesGenerator
{
    using System.IO;
    using System.Reflection;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Helpers;
    using Map;
    
    public class MapGridCrossesGenerator
    {
        [CommandMethod("DrawCrosses")]
        public static void DrawCrosses()
        {
            Database database = HostApplicationServices.WorkingDatabase;
            Editor editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

            string mapGridCrossFilePath = string.Format("{0}{1}Resources{1}Map_Grid_Cross.dwg", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.DirectorySeparatorChar);
            string mapGridCrossBlockName = "MAP-GRID-CROSS";

            BlockHelper.CopyBlockFromDwg(mapGridCrossBlockName, mapGridCrossFilePath, database);

            PromptIntegerOptions promptScaleOptions = new PromptIntegerOptions("Въведете мащаб: ")
            {
                DefaultValue = 1000,
                AllowNegative = false,
                AllowZero = false,
                UseDefaultValue = true,
                AllowNone = false
            };

            PromptIntegerResult promptScaleResult = editor.GetInteger(promptScaleOptions);
            if (promptScaleResult.Status != PromptStatus.OK)
            {
                editor.WriteMessage("Невалиден мащаб");

                return;
            }

            int scale = promptScaleResult.Value;

            PromptPointOptions promptLowerLeftPointOptions = new PromptPointOptions("\nИзберете долен ляв ъгъл на картата: ");

            PromptPointResult promptLowerLeftPointResult = editor.GetPoint(promptLowerLeftPointOptions);
            if (promptLowerLeftPointResult.Status != PromptStatus.OK)
            {
                editor.WriteMessage("Невалидна точка");

                return;
            }

            Point3d lowerLeftPoint = promptLowerLeftPointResult.Value;

            PromptPointOptions promptUpperRightPointOptions = new PromptPointOptions("\nИзберете горен десен ъгъл на картата: ");

            PromptPointResult promptUpperRightPointResult = editor.GetPoint(promptUpperRightPointOptions);
            if (promptUpperRightPointResult.Status != PromptStatus.OK)
            {
                editor.WriteMessage("Невалидна точка");

                return;
            }

            Point3d upperRightPoint = promptUpperRightPointResult.Value;

            var crosses = MapGrid.GenerateCrosses(new Point(lowerLeftPoint.X, lowerLeftPoint.Y), new Point(upperRightPoint.X, upperRightPoint.Y), scale);

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = database.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                BlockTableRecord blockDefinition = blockTable[mapGridCrossBlockName].GetObject(OpenMode.ForWrite) as BlockTableRecord;
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