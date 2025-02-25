﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lively.Common;
using Lively.Common.Factories;
using Lively.Grpc.Client;
using Lively.Models;
using Lively.UI.WinUI.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Lively.UI.WinUI.ViewModels.Settings
{
    public partial class SettingsPerformanceViewModel : ObservableObject
    {
        private readonly DispatcherQueue dispatcherQueue;

        private readonly IDialogService dialogService;
        private readonly IUserSettingsClient userSettings;
        private readonly IApplicationsRulesFactory appRuleFactory;

        public SettingsPerformanceViewModel(
            IUserSettingsClient userSettings,
            IDialogService dialogService,
            IApplicationsRulesFactory appRuleFactory)
        {
            this.userSettings = userSettings;
            this.dialogService = dialogService;
            this.appRuleFactory = appRuleFactory;

            //MainWindow dispatcher may not be ready yet, creating our own instead..
            dispatcherQueue = DispatcherQueue.GetForCurrentThread() ?? DispatcherQueueController.CreateOnCurrentThread().DispatcherQueue;

            SelectedAppFullScreenIndex = (int)userSettings.Settings.AppFullscreenPause;
            SelectedAppFocusIndex = (int)userSettings.Settings.AppFocusPause;
            SelectedBatteryPowerIndex = (int)userSettings.Settings.BatteryPause;
            SelectedRemoteDestopPowerIndex = (int)userSettings.Settings.RemoteDesktopPause;
            SelectedPowerSaveModeIndex = (int)userSettings.Settings.PowerSaveModePause;
            SelectedDisplayPauseRuleIndex = (int)userSettings.Settings.DisplayPauseSettings;
            SelectedPauseAlgorithmIndex = (int)userSettings.Settings.ProcessMonitorAlgorithm;
            //Only pause rules are shown to user, rest is internal use.
            AppRules = new ObservableCollection<ApplicationRulesModel>(userSettings.AppRules.Where(x => x.Rule == AppRulesEnum.pause));
        }

        private int _selectedAppFullScreenIndex;
        public int SelectedAppFullScreenIndex
        {
            get => _selectedAppFullScreenIndex;
            set
            {
                if (userSettings.Settings.AppFullscreenPause != (AppRulesEnum)value)
                {
                    userSettings.Settings.AppFullscreenPause = (AppRulesEnum)value;
                    UpdateSettingsConfigFile();
                }
                SetProperty(ref _selectedAppFullScreenIndex, value);
            }
        }

        private int _selectedAppFocusIndex;
        public int SelectedAppFocusIndex
        {
            get => _selectedAppFocusIndex;
            set
            {
                if (userSettings.Settings.AppFocusPause != (AppRulesEnum)value)
                {
                    userSettings.Settings.AppFocusPause = (AppRulesEnum)value;
                    UpdateSettingsConfigFile();
                }
                SetProperty(ref _selectedAppFocusIndex, value);
            }
        }

        private int _selectedBatteryPowerIndex;
        public int SelectedBatteryPowerIndex
        {
            get => _selectedBatteryPowerIndex;
            set
            {
                if (userSettings.Settings.BatteryPause != (AppRulesEnum)value)
                {
                    userSettings.Settings.BatteryPause = (AppRulesEnum)value;
                    UpdateSettingsConfigFile();
                }
                SetProperty(ref _selectedBatteryPowerIndex, value);
            }
        }

        private int _selectedPowerSaveModeIndex;
        public int SelectedPowerSaveModeIndex
        {
            get => _selectedPowerSaveModeIndex;
            set
            {
                if (userSettings.Settings.PowerSaveModePause != (AppRulesEnum)value)
                {
                    userSettings.Settings.PowerSaveModePause = (AppRulesEnum)value;
                    UpdateSettingsConfigFile();
                }
                SetProperty(ref _selectedPowerSaveModeIndex, value);
            }
        }

        private int _selectedRemoteDestopPowerIndex;
        public int SelectedRemoteDestopPowerIndex
        {
            get => _selectedRemoteDestopPowerIndex;
            set
            {
                if (userSettings.Settings.RemoteDesktopPause != (AppRulesEnum)value)
                {
                    userSettings.Settings.RemoteDesktopPause = (AppRulesEnum)value;
                    UpdateSettingsConfigFile();
                }
                SetProperty(ref _selectedRemoteDestopPowerIndex, value);
            }
        }

        private int _selectedDisplayPauseRuleIndex;
        public int SelectedDisplayPauseRuleIndex
        {
            get => _selectedDisplayPauseRuleIndex;
            set
            {
                if (userSettings.Settings.DisplayPauseSettings != (DisplayPauseEnum)value)
                {
                    userSettings.Settings.DisplayPauseSettings = (DisplayPauseEnum)value;
                    UpdateSettingsConfigFile();
                }
                SetProperty(ref _selectedDisplayPauseRuleIndex, value);
            }
        }

        private int _selectedPauseAlgorithmIndex;
        public int SelectedPauseAlgorithmIndex
        {
            get => _selectedPauseAlgorithmIndex;
            set
            {
                if (userSettings.Settings.ProcessMonitorAlgorithm != (ProcessMonitorAlgorithm)value)
                {
                    userSettings.Settings.ProcessMonitorAlgorithm = (ProcessMonitorAlgorithm)value;
                    UpdateSettingsConfigFile();
                }
                SetProperty(ref _selectedPauseAlgorithmIndex, value);
            }
        }

        #region apprules

        [ObservableProperty]
        private ObservableCollection<ApplicationRulesModel> appRules = [];

        private ApplicationRulesModel _selectedAppRuleItem;
        public ApplicationRulesModel SelectedAppRuleItem
        {
            get => _selectedAppRuleItem;
            set
            {
                SetProperty(ref _selectedAppRuleItem, value);
                RemoveAppRuleCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand]
        private async Task AddAppRule()
        {
            var result = await dialogService.ShowApplicationPickerDialogAsync();
            if (result is null)
                return;

            try
            {
                var rule = appRuleFactory.CreateAppPauseRule(result.AppPath, AppRulesEnum.pause);
                if (AppRules.Any(x => x.AppName.Equals(rule.AppName, StringComparison.Ordinal)))
                    return;

                userSettings.AppRules.Add(rule);
                AppRules.Add(rule);
                UpdateAppRulesConfigFile();
            }
            catch { /* Failed to parse program information, ignore. */ }
        }

        [RelayCommand(CanExecute = nameof(CanRemoveAppRule))]
        private void RemoveAppRule()
        {
            userSettings.AppRules.Remove(SelectedAppRuleItem);
            AppRules.Remove(SelectedAppRuleItem);
            UpdateAppRulesConfigFile();
        }

        private bool CanRemoveAppRule => SelectedAppRuleItem != null;

        #endregion //apprules

        public void UpdateSettingsConfigFile()
        {
            _ = dispatcherQueue.TryEnqueue(() =>
            {
                userSettings.Save<SettingsModel>();
            });
        }

        public void UpdateAppRulesConfigFile()
        {
            _ = dispatcherQueue.TryEnqueue(() =>
            {
                userSettings.Save<List<ApplicationRulesModel>>();
            });
        }
    }
}
