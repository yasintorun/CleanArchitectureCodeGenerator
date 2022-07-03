using CleanArchitectureCodeGenerator.Models;
using CleanArchitectureCodeGenerator.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CleanArchitectureCodeGenerator
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GenerateCodeCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("b46d3f7e-6ab4-4785-8484-67a5139d3a30");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateCodeCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private GenerateCodeCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GenerateCodeCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }
        private IServiceProvider SyncServiceProvider
        {
            get
            {
                return package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in GenerateCodeCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new GenerateCodeCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE dte = SyncServiceProvider.GetService(typeof(DTE)) as DTE ?? throw new ArgumentNullException();

            SelectedItems selectedItems = dte.SelectedItems;
            ProjectItem projectItem;
            Solution2 solution2 = (Solution2)dte.Solution;
            var dialogFactory = SyncServiceProvider.GetService(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory;

            if (selectedItems == null)
            {
                GenerateHelpers.ShowMessageBox((IServiceProvider)ServiceProvider, Messages.ModelNotSelected, Messages.Name);
                return;
            }
            foreach (SelectedItem selectedItem in selectedItems)
            {
                projectItem = selectedItem.ProjectItem;
                if (!GenerateHelpers.IsIEntityImplementation(projectItem))
                {
                    GenerateHelpers.ShowMessageBox((IServiceProvider)ServiceProvider, Messages.BeModel, Messages.Name);
                    return;
                }

                if (projectItem != null)
                {

                    var dialog = GenerateHelpers.ShowWaitDialog(dialogFactory);

                    foreach (Project project in dte.Solution.Projects)
                    {
                        GenerateApplications(projectItem, solution2, project);
                        GenerateInfrastructure(projectItem, solution2, project);
                    }

                    dialog.EndWaitDialog();
                }
            }
        }

        private void GenerateApplications(ProjectItem projectItem, Solution2 solution2, Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectTemplate = solution2.GetProjectItemTemplate("Interface", "CSharp");
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectItem.Name);

            if (project.Name.EndsWith("Application"))
            {
                foreach (ProjectItem item in project.ProjectItems)
                {
                    //GenerateHelpers.ShowMessageBox((IServiceProvider)ServiceProvider, item.Name, "as12asd");
                    //if (item.ProjectItems.Count > 1)
                    //{
                    //    GenerateHelpers.ShowMessageBox((IServiceProvider)ServiceProvider, item.ProjectItems.Item(1).Name, "asd");
                    //}
                    var fileParameters = new CreateFileParameters
                    {
                        FileNameWithoutExtension = fileNameWithoutExtension,
                        ProjectName = project.Name,
                        ProjectTemplate = projectTemplate
                    };

                    foreach (ProjectItem item2 in item.ProjectItems)
                    {
                        fileParameters.ProjectItem = item2;
                        if (item2.Name == "Repositories")
                        {
                            try
                            {
                                ApplicationFileService.CreateIRepository(fileParameters);

                            } catch(Exception ex)
                            {
                                GenerateHelpers.ShowMessageBox((IServiceProvider)ServiceProvider, item.ProjectItems.Item(1).Name, ex.Message);
                            }
                        }

                        if (item2.Name == "Services")
                        {
                            try
                            {
                                ApplicationFileService.CreateIService(fileParameters);

                            }
                            catch (Exception ex)
                            {
                                GenerateHelpers.ShowMessageBox((IServiceProvider)ServiceProvider, item.ProjectItems.Item(1).Name, ex.Message);
                            }
                        }
                    }

                    fileParameters.ProjectItem = item;


                    if (item.Name == "Managers")
                    {
                        try
                        {
                            ApplicationFileService.CreateManager(fileParameters);
                        }
                        catch (Exception ex)
                        {
                            GenerateHelpers.ShowMessageBox((IServiceProvider)ServiceProvider, "Hata", ex.Message);
                        }
                    }
                }
            }
        }

        private void GenerateInfrastructure(ProjectItem projectItem, Solution2 solution2, Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectTemplate = solution2.GetProjectItemTemplate("Interface", "CSharp");
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectItem.Name);

            if (project.Name.EndsWith("Infrastructure"))
            {
                foreach (ProjectItem item in project.ProjectItems)
                {
                    foreach (ProjectItem item2 in item.ProjectItems)
                    {
                        foreach (ProjectItem item3 in item2.ProjectItems)
                        {
                            var fileParameters = new CreateFileParameters
                            {
                                FileNameWithoutExtension = fileNameWithoutExtension,
                                ProjectName = project.Name,
                                ProjectItem = item3,
                                ProjectTemplate = projectTemplate
                            };
                            if (item3.Name == "Repositories")
                            {
                                try
                                {
                                    InfrastructureFileService.CreateIRepository(fileParameters);

                                }
                                catch (Exception ex)
                                {
                                    GenerateHelpers.ShowMessageBox((IServiceProvider)ServiceProvider, item.ProjectItems.Item(1).Name, ex.Message);
                                }
                            }
                        }
                        
                    }
                }
            }
        }
    }
}
