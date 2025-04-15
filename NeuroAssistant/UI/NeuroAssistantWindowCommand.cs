using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace NeuroAssistant.UI
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class NeuroAssistantWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0200;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("3A8FF9E2-2ACA-4DA8-A50B-4AB86BF264EE");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuroAssistantWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private NeuroAssistantWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static NeuroAssistantWindowCommand Instance
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
                return _package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in NeuroAssistantWindowCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            // Initialize singleton instance with validated services
            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new NeuroAssistantWindowCommand(package, commandService);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            // Start async window management without blocking UI thread
            _package.JoinableTaskFactory.RunAsync(async delegate
            {
                // Request window creation from VS shell
                ToolWindowPane window = await _package.ShowToolWindowAsync(typeof(NeuroAssistantWindow), 0, true, _package.DisposalToken);

                // Validate window frame creation
                if (window?.Frame == null)
                {
                    throw new NotSupportedException("Cannot create tool window");
                }
            })
            // Register error handling for fire-and-forget operation
            .FileAndForget("NeuroAssistantWindowCommand");
        }
    }
}
