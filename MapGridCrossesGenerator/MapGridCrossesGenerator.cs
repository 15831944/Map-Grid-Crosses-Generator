namespace MapGridCrossesGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using Contracts;
    using Helpers;
    using Map;

    public class MapGridCrossesGenerator
    {
        [CommandMethod("ExportCrosses")]
        public static void ExportCrosses()
        {
            Database database = HostApplicationServices.WorkingDatabase;
            Editor editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                ObjectId[] blockReferenceIdCollection = BlockHelper.PromptForBlockSelection(editor, "Изберете координатни кръстове: ");
                if (blockReferenceIdCollection == null)
                {
                    editor.WriteMessage("Невалидна селекция!");

                    return;
                }

                string mapGridCrossBlockName = "MAP-GRID-CROSS";
                StringBuilder output = new StringBuilder();

                foreach (ObjectId blockReferenceId in blockReferenceIdCollection)
                {
                    BlockReference blockReference = (BlockReference)transaction.GetObject(blockReferenceId, OpenMode.ForWrite);

                    if (blockReference.Name != mapGridCrossBlockName)
                    {
                        continue;
                    }

                    output.AppendFormat("{0} {1}{2}", blockReference.Position.X, blockReference.Position.Y, Environment.NewLine);
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, output.ToString());
                }

                transaction.Commit();
            }
        }

        [CommandMethod("ImportCrosses")]
        public static void ImportCrosses()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog.FileName;

                ICollection<IPoint> crosses = FileHelper.ImportCrossesFromFile(path);

                if (crosses.Count == 0)
                {
                    return;
                }

                Database database = HostApplicationServices.WorkingDatabase;
                Editor editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

                string mapGridCrossFilePath = string.Format("{0}{1}Resources{1}Map_Grid_Cross.dwg", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.DirectorySeparatorChar);
                string mapGridCrossBlockName = "MAP-GRID-CROSS";

                BlockHelper.CopyBlockFromDwg(mapGridCrossBlockName, mapGridCrossFilePath, database);

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
                            blockReference.ScaleFactors = new Scale3d(1);

                            modelSpaceBlockTableRecord.AppendEntity(blockReference);

                            transaction.AddNewlyCreatedDBObject(blockReference, true);
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        [CommandMethod("DrawCrossesInPolygon")]
        public static void DrawCrossesInPolygon()
        {
            Database database = HostApplicationServices.WorkingDatabase;
            Editor editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

            string mapGridCrossFilePath = string.Format("{0}{1}Resources{1}Map_Grid_Cross.dwg", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.DirectorySeparatorChar);
            string mapGridCrossBlockName = "MAP-GRID-CROSS";

            BlockHelper.CopyBlockFromDwg(mapGridCrossBlockName, mapGridCrossFilePath, database);

            PromptIntegerOptions promptScaleOptions = new PromptIntegerOptions("\nВъведете мащаб: ")
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
                editor.WriteMessage("\nНевалиден мащаб");

                return;
            }

            int scale = promptScaleResult.Value;

            PromptEntityOptions promptBoundaryOptions = new PromptEntityOptions("\nИзберете граница: ");
            promptBoundaryOptions.SetRejectMessage("\nИзберете обект от тип Polyline");
            promptBoundaryOptions.AddAllowedClass(typeof(Polyline), true);

            PromptEntityResult promptBoundaryResult = editor.GetEntity(promptBoundaryOptions);

            if (promptBoundaryResult.Status != PromptStatus.OK)
            {
                return;
            }

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                Polyline boundary = transaction.GetObject(promptBoundaryResult.ObjectId, OpenMode.ForRead) as Polyline;

                if (boundary.NumberOfVertices < 3)
                {
                    editor.WriteMessage("\nНевалидна граница");

                    return;
                }

                IPoint[] polygon = GeometryHelper.CreatePolygonFromPolyline(boundary);

                GeometryHelper.InitializePolygon(polygon);

                Point3d lowerLeftPoint = boundary.Bounds.Value.MinPoint;
                Point3d upperRightPoint = boundary.Bounds.Value.MaxPoint;

                var crosses = MapGrid.GenerateCrosses(new BoundaryPoint(lowerLeftPoint.X, lowerLeftPoint.Y), new BoundaryPoint(upperRightPoint.X, upperRightPoint.Y), scale);

                BlockTable blockTable = database.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                BlockTableRecord blockDefinition = blockTable[mapGridCrossBlockName].GetObject(OpenMode.ForWrite) as BlockTableRecord;
                BlockTableRecord modelSpaceBlockTableRecord = blockTable[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForWrite) as BlockTableRecord;

                foreach (var cross in crosses)
                {
                    if (GeometryHelper.InsideComplexPolygon(polygon, cross))
                    {
                        Point3d blockReferenceInsertPoint = new Point3d(cross.X, cross.Y, 0);

                        using (BlockReference blockReference = new BlockReference(blockReferenceInsertPoint, blockDefinition.ObjectId))
                        {
                            blockReference.ScaleFactors = new Scale3d(scale / 1000.0);

                            modelSpaceBlockTableRecord.AppendEntity(blockReference);

                            transaction.AddNewlyCreatedDBObject(blockReference, true);
                        }
                    }
                }

                transaction.Commit();
            }
        }

        [CommandMethod("DrawCrosses")]
        public static void DrawCrosses()
        {
            Database database = HostApplicationServices.WorkingDatabase;
            Editor editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

            string mapGridCrossFilePath = string.Format("{0}{1}Resources{1}Map_Grid_Cross.dwg", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.DirectorySeparatorChar);
            string mapGridCrossBlockName = "MAP-GRID-CROSS";

            BlockHelper.CopyBlockFromDwg(mapGridCrossBlockName, mapGridCrossFilePath, database);

            PromptIntegerOptions promptScaleOptions = new PromptIntegerOptions("\nВъведете мащаб: ")
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
                editor.WriteMessage("\nНевалиден мащаб");

                return;
            }

            int scale = promptScaleResult.Value;

            PromptPointOptions promptLowerLeftPointOptions = new PromptPointOptions("\nИзберете долен ляв ъгъл на картата: ");

            PromptPointResult promptLowerLeftPointResult = editor.GetPoint(promptLowerLeftPointOptions);
            if (promptLowerLeftPointResult.Status != PromptStatus.OK)
            {
                editor.WriteMessage("\nНевалидна точка");

                return;
            }

            Point3d lowerLeftPoint = promptLowerLeftPointResult.Value;

            PromptPointOptions promptUpperRightPointOptions = new PromptPointOptions("\nИзберете горен десен ъгъл на картата: ");

            PromptPointResult promptUpperRightPointResult = editor.GetPoint(promptUpperRightPointOptions);
            if (promptUpperRightPointResult.Status != PromptStatus.OK)
            {
                editor.WriteMessage("\nНевалидна точка");

                return;
            }

            Point3d upperRightPoint = promptUpperRightPointResult.Value;

            if (upperRightPoint.X <= lowerLeftPoint.X || upperRightPoint.Y <= lowerLeftPoint.Y)
            {
                editor.WriteMessage("\nНевалидни точки");

                return;
            }

            var crosses = MapGrid.GenerateCrosses(new BoundaryPoint(lowerLeftPoint.X, lowerLeftPoint.Y), new BoundaryPoint(upperRightPoint.X, upperRightPoint.Y), scale);

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