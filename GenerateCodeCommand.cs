using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
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
                        //GenerateDataAccess(projectItem, solution2, project);
                        GenerateHelpers.ShowMessageBox((IServiceProvider)ServiceProvider, "Test asd", "Generate etmeye hazir :)");
                        //GenerateBusiness(projectItem, solution2, project);
                    }

                    dialog.EndWaitDialog();
                }
            }
        }
    }
}
