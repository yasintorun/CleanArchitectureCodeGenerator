using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;

namespace CleanArchitectureCodeGenerator
{
    public static class GenerateHelpers
    {
        public static bool IsIEntityImplementation(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (CodeElement2 codeElement in projectItem.FileCodeModel.CodeElements)
            {

                if (codeElement is CodeNamespace)
                {
                    var nspace = codeElement as CodeNamespace;
                    foreach (CodeClass property in nspace.Members)
                    {

                        if (property is null)
                            continue;

                        if (property.Namespace.Name == "Infrastructure.DBTable") return true;
                    }
                }
            }
            return false;
        }
        public static void CreateFoldersIfNotExists(Project project, string[] folderNamesToCheck)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var projectPath = Path.GetDirectoryName(project.FullName);
            foreach (var folderName in folderNamesToCheck)
            {
                if (!Directory.Exists(Path.Combine(projectPath, folderName)))
                {
                    project.ProjectItems.AddFolder(folderName);
                }
            }

        }
        public static void ShowMessageBox(IServiceProvider serviceProvider, string message, string caption)
        {
            VsShellUtilities.ShowMessageBox(serviceProvider, message, caption, OLEMSGICON.OLEMSGICON_INFO,
                   OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        public static IVsThreadedWaitDialog2 ShowWaitDialog(IVsThreadedWaitDialogFactory dialogFactory)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsThreadedWaitDialog2 dialog = null;

            if (dialogFactory != null)
                dialogFactory.CreateInstance(out dialog);


            if (dialog != null)
                dialog.StartWaitDialog("Please waiting..", "Generating codes according to the selected model", "", null, "", 0, false, true);
            return dialog;
        }

        internal static void ShowMessageBox(IServiceProvider serviceProvider, object noItemSelected, string name)
        {
            throw new NotImplementedException();
        }
    }
}
