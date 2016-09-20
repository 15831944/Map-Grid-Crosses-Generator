namespace MapGridCrossesGenerator.Helpers
{
    using System.IO;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Contracts;

    internal static class BlockHelper
    {
        public static void CopyBlockFromDwg(string blockName, string filePath, Database destinationDatabase)
        {
            using (Database sourceDatabase = new Database(false, true))
            {
                sourceDatabase.ReadDwgFile(filePath, FileShare.ReadWrite, true, string.Empty);

                ObjectIdCollection ids = new ObjectIdCollection();

                using (Transaction sourceTransaction = sourceDatabase.TransactionManager.StartTransaction())
                {
                    BlockTable blockTable = (BlockTable)sourceTransaction.GetObject(sourceDatabase.BlockTableId, OpenMode.ForRead);

                    if (blockTable.Has(blockName))
                    {
                        ids.Add(blockTable[blockName]);
                    }

                    sourceTransaction.Commit();
                }

                if (ids.Count != 0)
                {
                    IdMapping mapping = new IdMapping();

                    destinationDatabase.WblockCloneObjects(ids, destinationDatabase.BlockTableId, mapping, DuplicateRecordCloning.Ignore, false);
                }
            }
        }

        public static void InsertBlock(string name, IPoint insertPoint, double scale)
        {
            Database database = HostApplicationServices.WorkingDatabase;
            Transaction transaction = database.TransactionManager.StartTransaction();

            BlockTable blockTable = database.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            BlockTableRecord blockDefinition = blockTable[name].GetObject(OpenMode.ForWrite) as BlockTableRecord;
            BlockTableRecord modelSpaceBlockTableRecord = blockTable[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForWrite) as BlockTableRecord;

            Point3d blockReferenceInsertPoint = new Point3d(insertPoint.X, insertPoint.Y, 0);

            using (BlockReference blockReference = new BlockReference(blockReferenceInsertPoint, blockDefinition.ObjectId))
            {
                blockReference.ScaleFactors = new Scale3d(scale);

                modelSpaceBlockTableRecord.AppendEntity(blockReference);

                transaction.AddNewlyCreatedDBObject(blockReference, true);

                transaction.Commit();
            }
        }
    }
}