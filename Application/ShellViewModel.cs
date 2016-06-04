﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AspectInjector.Broker;
using FomodInfrastructure;
using FomodInfrastructure.Aspect;
using FomodInfrastructure.Interface;
using FomodInfrastructure.MvvmLibrary.Commands;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Practices.ServiceLocation;
using Module.Editor.ViewModel;
using Prism.Regions;
using Module.Welcome.PrismEvent;
using Prism.Events;

namespace MainApplication
{
    public class ShellViewModel
    {
        private readonly string _defautlTitle;

        public ShellViewModel(IRegionManager regionManager, IAppService appService, IDialogCoordinator dialogCoordinator, IEventAggregator eventAggregator, IServiceLocator serviceLocator)
        {
            Title = _defautlTitle = $"FOMOD Creator beta v{appService.Version}";
            _regionManager = regionManager;
            _dialogCoordinator = dialogCoordinator;
            _eventAggregator = eventAggregator;
            _serviceLocator = serviceLocator;
            CloseTabCommand = new RelayCommand<object>(CloseTab);
            SaveProjectCommand = new RelayCommand(SaveProject, CanSaveProject);
            SaveProjectAsCommand = new RelayCommand(SaveProjectAs, CanSaveProject);
        }

        #region Services

        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IServiceLocator _serviceLocator;

        #endregion

        #region Commands

        public RelayCommand<object> CloseTabCommand { get; }

        public RelayCommand SaveProjectCommand { get; }

        public RelayCommand SaveProjectAsCommand { get; }

        #endregion

        #region Properties

        private object _curentSelectedItem;

        [Aspect(typeof(AspectINotifyPropertyChanged))]
        public object CurentSelectedItem
        {
            get { return _curentSelectedItem; }
            set
            {
                _curentSelectedItem = value;
                SaveProjectCommand.RaiseCanExecuteChanged();
                SaveProjectAsCommand.RaiseCanExecuteChanged();
                var b = (CurentSelectedItem as FrameworkElement)?.DataContext;
                if (b is MainEditorViewModel)
                    Title = $"{(b as MainEditorViewModel).FirstData.ModuleInformation.Name}: {_defautlTitle}";
                else
                    Title = _defautlTitle;
            }
        }

        [Aspect(typeof(AspectINotifyPropertyChanged))]
        public string Title { get; set; }

        #endregion

        #region Methods

        private async void CloseTab(object p)
        {
            if (!(p is MainEditorViewModel))
                return;
            var removeView = _regionManager.Regions[Names.MainContentRegion].Views.Cast<FrameworkElement>().FirstOrDefault(v => v.DataContext == p);
            if (removeView == null)
                return;
            var needSave = ((MainEditorViewModel)p).IsNeedSave;
            if (needSave)
            {
                var result = await CofirmDialogAsync();
                if (result)
                    SaveProject();
            }
            removeView.DataContext = null;
            _regionManager.Regions[Names.MainContentRegion].Remove(removeView);

            ////removeView.Finalize();
            ////GC.SuppressFinalize(removeView);
            ((MainEditorViewModel)p).Dispose();
            removeView = null;
            GC.Collect();
        }

        private void SaveProject()
        {
            var vm = (MainEditorViewModel)((FrameworkElement)CurentSelectedItem).DataContext;
            vm.IsNeedSave = false;
            vm.Save();
            foreach (var projectRoot in vm.Data)
                _eventAggregator.GetEvent<OpenProjectEvent>().Publish(projectRoot);
        }

        private void SaveProjectAs()
        {
            var vm = (MainEditorViewModel)((FrameworkElement)CurentSelectedItem).DataContext;
            vm.IsNeedSave = false;
            vm.SaveAs();
            foreach (var projectRoot in vm.Data)
                _eventAggregator.GetEvent<OpenProjectEvent>().Publish(projectRoot);
        }

        private bool CanSaveProject() => (CurentSelectedItem as FrameworkElement)?.DataContext is MainEditorViewModel;

        private async Task<bool> CofirmDialogAsync() => 
            await _dialogCoordinator.ShowMessageAsync(this, "Close", "Save project before closing?", MessageDialogStyle.AffirmativeAndNegative,
            _serviceLocator.GetInstance<MetroDialogSettings>()) == MessageDialogResult.Affirmative; //TODO: Localize

        #endregion
    }
}