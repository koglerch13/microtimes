using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ReactiveUI;

namespace MicroTimes;

public class DayViewModel : ReactiveObject, IDisposable
{
    public DateOnly Date { get; }
    private readonly ObservableCollection<TimeEntryViewModel> _entries;
    public TimeSpan TotalTime => CalculateTime();
    
    public ReadOnlyObservableCollection<TimeEntryViewModel> Entries => new(_entries);

    public DayViewModel(DateOnly date, IList<TimeEntryViewModel> entries)
    {
        Date = date;
        _entries = new ObservableCollection<TimeEntryViewModel>(entries);
        _entries.CollectionChanged += OnCollectionChanged;
        _entries
            .ToList()
            .ForEach(item => item.PropertyChanged += OnItemPropertyChanged);
    }

    public void Dispose()
    {
        _entries.CollectionChanged -= OnCollectionChanged;
        _entries
            .ToList()
            .ForEach(item => item.PropertyChanged -= OnItemPropertyChanged);
    }
    
    public void Add(TimeEntryViewModel timeEntryViewModel)
    {
        if (timeEntryViewModel.Date != Date)
            return;
        
        if (HasEntry(timeEntryViewModel))
            return;
        
        _entries.Add(timeEntryViewModel);
    }
    
    public void Remove(TimeEntryViewModel timeEntryViewModel)
    {
        if (timeEntryViewModel.Date != Date)
            return;
        
        if (!HasEntry(timeEntryViewModel))
            return;
        
        _entries.Remove(timeEntryViewModel);
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        e.NewItems?
            .Cast<TimeEntryViewModel>()
            .ToList()
            .ForEach(item => item.PropertyChanged += OnItemPropertyChanged);
    
        e.OldItems?
            .Cast<TimeEntryViewModel>()
            .ToList()
            .ForEach(item => item.PropertyChanged -= OnItemPropertyChanged);
        
        this.RaisePropertyChanged(nameof(TotalTime));
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(TimeEntryViewModel.StartTime):
            case nameof(TimeEntryViewModel.EndTime):
                this.RaisePropertyChanged(nameof(TotalTime));
                break;
        }
    }
    
    private TimeSpan CalculateTime()
    {
        var totalSeconds = _entries
            .Where(x => x.StartTime.HasValue && x.EndTime.HasValue)
            .Select(x => x.EndTime!.Value - x.StartTime!.Value)
            .Sum(x => x.TotalSeconds);

        return TimeSpan.FromSeconds(totalSeconds);
    }
    
    private bool HasEntry(TimeEntryViewModel timeEntryViewModel)
    {
        return _entries.IndexOf(timeEntryViewModel) >= 0;
    }
}