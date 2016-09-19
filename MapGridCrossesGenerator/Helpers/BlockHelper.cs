namespace MapGridCrossesGenerator.Helpers
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Contracts;

    internal static class BlockHelper
    {
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