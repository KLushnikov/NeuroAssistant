using Microsoft.VisualStudio.Shell;
using NeuroAssistant.Ai;
using NeuroAssistant.Ai.Connection;
using NeuroAssistant.Ai.Profiles;
using NeuroAssistant.Core;
using NeuroAssistant.Core.Enum;
using NeuroAssistant.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeuroAssistant.UI
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("1ac05b52-bbde-458c-8fab-d0bca63818e8")]
    public class NeuroAssistantWindow : ToolWindowPane, INotifyPropertyChanged
    {
        public static Guid WindowGuid = new Guid("1ac05b52-bbde-458c-8fab-d0bca63818e8");

        private string _aiMessage, _aiResultContent, _aiProfileNameSelected;
        private IAiConnectionSettings _aiProfileSelected = null;

        private Visibility _aiMessageGridVisibility = Visibility.Visible,
            _settingGridVisibility = Visibility.Collapsed;

        private readonly AiAssistedService _aiAssistedService;
        private readonly IAiProfileManager _aiProfileManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuroAssistantWindow"/> class.
        /// </summary>
        public NeuroAssistantWindow() : base(null)
        {
            EncryptionService encryptionService = new EncryptionService();
            VsSettingsStoreService vsSettingsStoreService = new VsSettingsStoreService(encryptionService);
            _aiProfileManager = new AiProfileManager(vsSettingsStoreService);

            Caption = "Neuro Assistant";
            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            var control = new NeuroAssistantWindowControl
            {
                DataContext = this
            };

            var profiles = _aiProfileManager.GetProfileIds();

            if (profiles.Count > 0)
            {
                AiProfileNames = new ObservableCollection<string>(profiles);

                string lastProfileName = _aiProfileManager.GetLastProfile();
                if (!string.IsNullOrWhiteSpace(lastProfileName))
                {
                    AiProfileNameSelected = lastProfileName;
                }
                else
                {
                    AiProfileNameSelected = AiProfileNames[0];
                }
            }
            else
            {
                AiProfileNames = new ObservableCollection<string>();
                AiProfileSelected = new AiConnectionSettings();
                SwitchShowSetting();
            }

            AiConnectionSettings aiConnectionSettings = _aiProfileManager.LoadProfile<AiConnectionSettings>(AiProfileNameSelected) as AiConnectionSettings;
            AiAssistedService aiAssistedService = new AiAssistedService(aiConnectionSettings);
            _aiAssistedService = aiAssistedService;

            Content = control;

            SaveSettingCommand = CommandFactory.CreateCommand(SaveSetting);
            CancelSettingCommand = CommandFactory.CreateCommand(CancelSetting);
            SwitchShowSettingCommand = CommandFactory.CreateCommand(SwitchShowSetting);
            SendToAiCommand = CommandFactory.CreateCommand(SentToAiMessage);
        }

        public ICommand SaveSettingCommand { get; set; }
        public ICommand CancelSettingCommand { get; set; }
        public ICommand SwitchShowSettingCommand { get; set; }
        public ICommand SendToAiCommand { get; set; }

        public ObservableCollection<string> AiProfileNames { get; private set; }

        public string AiProfileNameSelected
        {
            get => _aiProfileNameSelected;
            set
            {
                _aiProfileNameSelected = value;
                AiProfileSelected = _aiProfileManager.LoadProfile<AiConnectionSettings>(value);
                _aiProfileManager.SetLastProfile(value);
                NotifyPropertyChanged(nameof(AiProfileNameSelected));
            }
        }

        public string AiMessage
        {
            get => _aiMessage;
            set
            {
                _aiMessage = value;
                NotifyPropertyChanged(nameof(AiMessage));
            }
        }

        public string AiResultContent
        {
            get => _aiResultContent;
            set
            {
                _aiResultContent = value;
                NotifyPropertyChanged(nameof(AiResultContent));
            }
        }

        public Visibility AiMessageGridVisibility
        {
            get => _aiMessageGridVisibility;
            set
            {
                _aiMessageGridVisibility = value;
                NotifyPropertyChanged(nameof(AiMessageGridVisibility));
            }
        }
        public Visibility SettingGridVisibility
        {
            get => _settingGridVisibility;
            set
            {
                _settingGridVisibility = value;
                NotifyPropertyChanged(nameof(SettingGridVisibility));
            }
        }

        public IAiConnectionSettings AiProfileSelected
        {
            get => _aiProfileSelected;
            set
            {
                _aiProfileSelected = value;
                NotifyPropertyChanged(nameof(AiProfileSelected));
            }
        }

        private void SwitchShowSetting()
        {
            AiMessageGridVisibility = AiMessageGridVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            SettingGridVisibility = SettingGridVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CancelSetting()
        {
            SwitchShowSetting();
        }

        private void SaveSetting()
        {
            var result = _aiProfileManager.SaveProfile(_aiProfileSelected);

            if (result == NeuroAssistantResult.AddNewItem)
            {
                AiProfileNames.Add(_aiProfileSelected.ProfileName);
                AiProfileNameSelected = _aiProfileSelected.ProfileName;
            }

            SwitchShowSetting();
        }

        private void SentToAiMessage()
        {
            var result = Task.Run(async () =>
            {
                return await SentToAiMessageAsync(AiMessage);
            }).ConfigureAwait(false);
        }

        public async Task<string> SentToAiMessageAsync(string message)
        {
            var result = await _aiAssistedService.SentMessageAsync(message).ConfigureAwait(false);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            AiResultContent = result;
            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
