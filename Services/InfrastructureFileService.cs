using CleanArchitectureCodeGenerator.Constants;
using CleanArchitectureCodeGenerator.Models;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
namespace CleanArchitectureCodeGenerator.Services
{
    public class InfrastructureFileService
    {
        #region CREATE REPOSITORY
        public static void CreateIRepository(CreateFileParameters fileParameters)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var addedItem = fileParameters.ProjectItem.ProjectItems.AddFromTemplate(fileParameters.ProjectTemplate,
                                                                        $"{fileParameters.FileNameWithoutExtension}Repository.cs");
                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(GenerateHelpers.GetFileContent(fileParameters.FileNameWithoutExtension, FileContents.InfraStructureRepository));
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
