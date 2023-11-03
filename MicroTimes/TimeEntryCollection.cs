using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MicroTimes;

public class TimeEntryCollection
{
    private readonly Dictionary<DateOnly, DayViewModel> _allTimeEntries;

    public TimeEntryCollection(Dictionary<DateOnly, DayViewModel> allTimeEntries)
    {
        _allTimeEntries = allTimeEntries;
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
            _allTimeEntries[date] = day;
        }

        return day;
    }
}