namespace MapGridCrossesGenerator.Helpers
{
    using System.IO;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;

    internal static class BlockHelper
    {
        public static ObjectId[] PromptForBlockSelection(Editor editor, string message)
        {
            TypedValue[] blockFilterList = new TypedValue[1]
            {
                new TypedValue((int)DxfCode.Start, "INSERT")
            };
            SelectionFilter blockelectionFilter = new SelectionFilter(blockFilterList);

            PromptSelectionOptions promptSelectionOptions = new PromptSelectionOptions();
            promptSelectionOptions.MessageForAdding = message;

            PromptSelectionResult promptSelectionResult = editor.GetSelection(promptSelectionOptions, blockelectionFilter);
            if (promptSelectionResult.Status != PromptStatus.OK)
            {
                return null;
            }

            SelectionSet blockSelectionSet = promptSelectionResult.Value;
            ObjectId[] objectIdCollection = blockSelectionSet.GetObjectIds();

            return objectIdCollection;
        }

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
    }
}