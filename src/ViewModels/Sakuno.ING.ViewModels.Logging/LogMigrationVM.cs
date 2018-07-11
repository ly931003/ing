﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sakuno.ING.Composition;
using Sakuno.ING.Game.Logger;
using Sakuno.ING.Game.Logger.Entities;
using Sakuno.ING.IO;
using Sakuno.ING.Shell;

namespace Sakuno.ING.ViewModels.Logging
{
    [Export(typeof(LogMigrationVM), SingleInstance = false)]
    public class LogMigrationVM : BindableObject
    {
        private readonly Logger logger;
        private readonly IShellContextService shellContextService;
        public IBindableCollection<ILogMigrator> Migrators { get; }

        private ILogMigrator _selectedMigrator;
        public ILogMigrator SelectedMigrator
        {
            get => _selectedMigrator;
            set
            {
                if (_selectedMigrator != value)
                {
                    _selectedMigrator = value;
                    SelectedPath = null;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(SupportShipCreation));
                    NotifyPropertyChanged(nameof(SupportEquipmentCreation));
                    NotifyPropertyChanged(nameof(SupportExpeditionCompletion));
                }
            }
        }

        public bool Ready
            => SelectedMigrator != null
            && SelectedPath != null;

        private IFileSystemFacade _selectedFs;
        public IFileSystemFacade SelectedPath
        {
            get => _selectedFs;
            set
            {
                if (_selectedFs != value)
                {
                    _selectedFs = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(Ready));
                }
            }
        }

        public bool SupportShipCreation => SelectedMigrator is ILogProvider<ShipCreationEntity>;
        public bool SelectShipCreation { get; set; }

        public bool SupportEquipmentCreation => SelectedMigrator is ILogProvider<EquipmentCreationEntity>;
        public bool SelectEquipmentCreation { get; set; }

        public bool SupportExpeditionCompletion => SelectedMigrator is ILogProvider<ExpeditionCompletionEntity>;
        public bool SelectExpeditionCompletion { get; set; }

        public LogMigrationVM(Logger logger, ILogMigrator[] migrators, IShellContextService shellContextService)
        {
            this.logger = logger;
            this.shellContextService = shellContextService;
            Migrators = migrators.ToBindable();
        }

        private bool _ranged;
        public bool Ranged
        {
            get => _ranged;
            set => Set(ref _ranged, value);
        }

        public double TimeZoneOffset { get; set; }
        public DateTime DateFrom = DateTime.Now;
        public DateTime DateTo = DateTime.Now;

        private bool _running;
        public bool Running
        {
            get => _running;
            private set => Set(ref _running, value);
        }

        public async void PickPath()
        {
            IFileSystemFacade fs;
            if (SelectedMigrator.RequireFolder)
                fs = await shellContextService.Capture().PickFolderAsync();
            else
                fs = await shellContextService.Capture().OpenFileAsync();

            if (fs != null)
                SelectedPath = fs;
        }

        private async ValueTask TryMigrate<T>(DbSet<T> dbSet, ILogProvider<T> provider)
            where T : class, ITimedEntity
        {
            if (provider == null) return;

            var timeZone = TimeSpan.FromHours(TimeZoneOffset);
            IEnumerable<T> source = await provider.GetLogsAsync(SelectedPath, timeZone);
            if(Ranged)
            {
                var timeFrom = DateTime.SpecifyKind(DateFrom, DateTimeKind.Utc) - timeZone;
                var timeTo = DateTime.SpecifyKind(DateTo, DateTimeKind.Utc) - timeZone;
                source = source.Where(e => e.TimeStamp > timeFrom && e.TimeStamp < timeTo);
            }

            var index = new HashSet<long>(dbSet.Select(e => e.TimeStamp.ToUnixTimeSeconds()));
            foreach(T e in source)
            {
                long time = e.TimeStamp.ToUnixTimeSeconds();
                if(!index.Contains(time))
                {
                    await dbSet.AddAsync(e);
                    index.Add(time);
                }
            }
        }

        public async void DoMigration()
        {
            var shellContext = shellContextService.Capture();

            if(!logger.PlayerLoaded)
            {
                await shellContext.ShowMessageAsync("Game player not loaded", "Cannot migrate");
                return;
            }

            Running = true;

            try
            {
                using (var context = logger.CreateContext())
                {
                    if (SelectShipCreation)
                        await TryMigrate(context.ShipCreationTable, SelectedMigrator as ILogProvider<ShipCreationEntity>).ConfigureAwait(false);
                    if (SelectEquipmentCreation)
                        await TryMigrate(context.EquipmentCreationTable, SelectedMigrator as ILogProvider<EquipmentCreationEntity>).ConfigureAwait(false);
                    if (SelectExpeditionCompletion)
                        await TryMigrate(context.ExpeditionCompletionTable, SelectedMigrator as ILogProvider<ExpeditionCompletionEntity>).ConfigureAwait(false);

                    await context.SaveChangesAsync().ConfigureAwait(false);

                    Running = false;
                    await shellContext.ShowMessageAsync("Log migration completed successfully.", "Migration Completed");
                }
            }
            catch(Exception ex)
            {
                Running = false;
                await shellContext.ShowMessageAsync(ex.ToString(), "Migration Failed");
            }
        }
    }
}
