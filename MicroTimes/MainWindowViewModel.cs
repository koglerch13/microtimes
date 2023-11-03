using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MicroTimes.MessageBox;
using ReactiveUI;

namespace MicroTimes;

public class MainWindowViewModel : ReactiveObject, IDisposable
{
    private readonly DateOnly TODAY = DateOnly.FromDateTime(DateTime.Now);
    private readonly TimeSpan _timerInterval = TimeSpan.FromSeconds(10);
    
    private readonly MainWindow _mainWindow;
    private readonly DispatcherTimer _timer;
    private TimeEntryCollection _timeEntryCollection;
    private readonly Parser _parser;
    
    private readonly IMessageBoxService _messageBoxService;
    
    private DayViewModel _otherEntriesToday;
    private TimeEntryViewModel _activeEntry;
    private DateTime _selectedDay;
    private bool _isLoading;
    private string? _filePath;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public DateTime SelectedDay
    {
        get => _selectedDay;
        set => this.RaiseAndSetIfChanged(ref _selectedDay, value);
    }
    
    public DayViewModel OtherEntriesToday
    {
        get => _otherEntriesToday;
        set => this.RaiseAndSetIfChanged(ref _otherEntriesToday, value);
    }

    public TimeEntryViewModel ActiveEntry
    {
        get => _activeEntry;
        set => this.RaiseAndSetIfChanged(ref _activeEntry, value);
    }
    
    public ReactiveCommand<Unit, Unit> StartActiveEntryCommand { get; }
    public ReactiveCommand<Unit, Unit> StopActiveEntryCommand { get; }
    public ReactiveCommand<TimeEntryViewModel, Unit> RestartEntryCommand { get; }
    public ReactiveCommand<TimeEntryViewModel, Unit> StartEntryCommand { get; }
    public ReactiveCommand<TimeEntryViewModel, Unit> ResumeEntryCommand { get; }
    public ReactiveCommand<TimeEntryViewModel, Unit> DeleteEntryCommand { get; }

    public ReactiveCommand<Unit, Unit> AddTodoEntryCommand { get; }

    public MainWindowViewModel(MainWindow mainWindow, IMessageBoxService messageBoxService)
    {
        _mainWindow = mainWindow;
        _messageBoxService = messageBoxService;
        _timer = new DispatcherTimer
        {
            Interval = _timerInterval
        };
        _parser = new Parser();

        // this will be replaced during loading.
        _timeEntryCollection = new TimeEntryCollection(new());
        _otherEntriesToday = _timeEntryCollection.GetForDate(TODAY);
        
        _activeEntry = new TimeEntryViewModel(TODAY);
        _selectedDay = DateTime.Now;

        StartActiveEntryCommand = ReactiveCommand.Create(StartActiveEntry);
        StopActiveEntryCommand = ReactiveCommand.Create(StopActiveEntry);
        RestartEntryCommand = ReactiveCommand.Create<TimeEntryViewModel>(RestartEntry);
        StartEntryCommand = ReactiveCommand.Create<TimeEntryViewModel>(StartEntry);
        ResumeEntryCommand = ReactiveCommand.Create<TimeEntryViewModel>(ResumeEntry);
        DeleteEntryCommand = ReactiveCommand.CreateFromTask<TimeEntryViewModel>(DeleteEntry);
        AddTodoEntryCommand = ReactiveCommand.Create(AddTodoEntry);
        
        
        PropertyChanged += OnPropertyChanged;
        _timer.Tick += OnTimerTick;
        _mainWindow.Activated += OnMainWindowActivated;
        _mainWindow.Deactivated += OnMainWindowOnDeactivated;

        _ = LoadEntries();
    }


    
    public void Dispose()
    {
        PropertyChanged -= OnPropertyChanged;
        _timer.Tick -= OnTimerTick;
        _mainWindow.Activated -= OnMainWindowActivated;
        _mainWindow.Deactivated -= OnMainWindowOnDeactivated;
        
        _timer.Stop();
    }

    private void StartActiveEntry()
    {
        if (ActiveEntry.IsRunning)
            return;
        
        ActiveEntry.Start();
        
        // TODO: focus description (if empty)
    }

    private void StopActiveEntry()
    {
        if (!ActiveEntry.IsRunning)
            return;
        
        ActiveEntry.Stop();

        _timeEntryCollection.Add(ActiveEntry);
        ActiveEntry = new TimeEntryViewModel(TODAY);
    }

    private void RestartEntry(TimeEntryViewModel timeEntryViewModel)
    {
        if (!timeEntryViewModel.IsFinished)
            return;
        
        if (ActiveEntry.IsRunning) 
            StopActiveEntry();

        // a new entry is created automatically
        ActiveEntry.Description = timeEntryViewModel.Description;
        StartActiveEntry();
    }

    private void StartEntry(TimeEntryViewModel timeEntryViewModel)
    {
        if (!timeEntryViewModel.IsNotStarted)
            return;
        
        if (ActiveEntry.IsRunning) 
            StopActiveEntry();

        _timeEntryCollection.Remove(timeEntryViewModel);
        ActiveEntry = timeEntryViewModel;
        StartActiveEntry();
    }

    private void ResumeEntry(TimeEntryViewModel timeEntryViewModel)
    {
        if (!timeEntryViewModel.IsRunning)
            return;
        
        if (ActiveEntry.IsRunning)
            StopActiveEntry();

        _timeEntryCollection.Remove(timeEntryViewModel);
        ActiveEntry = timeEntryViewModel;
        StartActiveEntry();
    }

    private async Task DeleteEntry(TimeEntryViewModel timeEntryViewModel)
    {
        var confirm = await _messageBoxService.ConfirmDelete(timeEntryViewModel.Description);
        if (!confirm)
            return;

        _timeEntryCollection.Remove(timeEntryViewModel);
    }

    private void AddTodoEntry()
    {
        var selectedDay = DateOnly.FromDateTime(SelectedDay);
        var newEntry = new TimeEntryViewModel(selectedDay);
        _timeEntryCollection.Add(newEntry);
        // TODO: focus description
    }
    
    private void OnTimerTick(object? sender, EventArgs e)
    {
        ActiveEntry.TriggerElapsedTimeUpdate();
    }
    
    private void OnMainWindowOnDeactivated(object? sender, EventArgs e)
    {
        _timer.Stop();
    }

    private void OnMainWindowActivated(object? sender, EventArgs e)
    {
        _timer.Start();
        ActiveEntry.TriggerElapsedTimeUpdate();
    }
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedDay):
                OnSelectedDayChanged();
                break;
        }
    }
    
    private void OnSelectedDayChanged()
    {
        var date = DateOnly.FromDateTime(SelectedDay);
        OtherEntriesToday = _timeEntryCollection.GetForDate(date);
    }

    private async Task LoadEntries()
    {
        IsLoading = true;
        _filePath = await OpenFile();
        _timeEntryCollection = await _parser.ParseFile(_filePath);

        Observable.FromEventPattern<EventHandler, EventArgs>(
            h => _timeEntryCollection.Changed += h,
            h => _timeEntryCollection.Changed -= h)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Subscribe(OnTimeEntryCollectionChanged);

        OtherEntriesToday = _timeEntryCollection.GetForDate(TODAY);
        IsLoading = false;
    }

    private void OnTimeEntryCollectionChanged(EventPattern<EventArgs>e )
    {
        if (_filePath == null || !File.Exists(_filePath))
            return;
        
        _ = _parser.WriteFile(ActiveEntry, _timeEntryCollection, _filePath);
    }

    private async Task<string> OpenFile()
    {
        var result = await _mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            SuggestedStartLocation = null, 
            AllowMultiple = false, 
            FileTypeFilter = new []{ FilePickerFileTypes.TextPlain }
        });

        if (result.Count != 1)
            throw new Exception("Only one file can be opened.");
        
        var path = result.First().TryGetLocalPath();
        if (path == null || !File.Exists(path))
            throw new Exception("Cannot open file.");

        return path;
    }
}