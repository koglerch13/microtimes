using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MicroTimes;

public class TimeEntryCollection : IDisposable
{
    public event EventHandler? Changed;
    
    private readonly Dictionary<DateOnly, DayViewModel> _allTimeEntries;

    public TimeEntryCollection(Dictionary<DateOnly, DayViewModel> allTimeEntries)
    {
        _allTimeEntries = allTimeEntries;
        foreach (var entry in _allTimeEntries)
        {
            entry.Value.DataChanged += OnDayChanged;
        }
    }

    public void Dispose()
    {
        foreach (var entry in _allTimeEntries)
        {
            entry.Value.DataChanged -= OnDayChanged;
        }
    }

    public ReadOnlyDictionary<DateOnly, DayViewModel> GetAllEntries()
    {
        return _allTimeEntries.AsReadOnly();
    }

    public DayViewModel GetForDate(DateOnly date)
    {
        return GetDayViewModel(date);
    }

    public void Add(TimeEntryViewModel timeEntryViewModel)
    {
        var day = GetDayViewModel(timeEntryViewModel.Date);
        day.Add(timeEntryViewModel);
    }

    public void Remove(TimeEntryViewModel timeEntryViewModel)
    {
        var day = GetDayViewModel(timeEntryViewModel.Date);
        day.Remove(timeEntryViewModel);
    }

    private DayViewModel GetDayViewModel(DateOnly date)
    {
        var day = _allTimeEntries.GetValueOrDefault(date);
        if (day == null)
        {
            day = new DayViewModel(date, new ObservableCollection<TimeEntryViewModel>());
            day.DataChanged += OnDayChanged;
            _allTimeEntries[date] = day;
        }

        return day;
    }
    
    private void OnDayChanged(object? sender, EventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
}