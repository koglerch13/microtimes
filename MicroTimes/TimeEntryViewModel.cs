using System;
using ReactiveUI;

namespace MicroTimes;

public class TimeEntryViewModel : ReactiveObject
{
    private DateOnly _date;
    private TimeOnly? _startTime;
    private TimeOnly? _endTime;
    private string _description;

    public bool IsRunning => StartTime.HasValue && !EndTime.HasValue;
    public bool IsFinished => StartTime.HasValue && EndTime.HasValue;
    public bool IsNotStarted => !StartTime.HasValue;
    public bool IsValid => ComputeIsValid();

    public TimeSpan ElapsedTime => CalculateElapsedTime();
    
    public DateOnly Date
    {
        get => _date;
        set => this.RaiseAndSetIfChanged(ref _date,  value);
    }

    public TimeOnly? StartTime
    {
        get => _startTime;
        set
        {
            this.RaiseAndSetIfChanged(ref _startTime, value);
            this.RaisePropertyChanged(nameof(IsRunning));
            this.RaisePropertyChanged(nameof(IsFinished));
            this.RaisePropertyChanged(nameof(IsNotStarted));
            this.RaisePropertyChanged(nameof(ElapsedTime));
            this.RaisePropertyChanged(nameof(IsValid));
        }
    }

    public TimeOnly? EndTime
    {
        get => _endTime;
        set
        {
            this.RaiseAndSetIfChanged(ref _endTime, value);
            this.RaisePropertyChanged(nameof(IsRunning));
            this.RaisePropertyChanged(nameof(IsFinished));
            this.RaisePropertyChanged(nameof(IsNotStarted));
            this.RaisePropertyChanged(nameof(ElapsedTime));
            this.RaisePropertyChanged(nameof(IsValid));
        }
    }

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    public TimeEntryViewModel(DateOnly date)
    {
        _date = date;
        _description = string.Empty;
    }

    public void Start()
    {
        if (!IsNotStarted)
            return;
        
        StartTime = TimeOnly.FromDateTime(DateTime.Now);
    }
    
    public void Stop()
    {
        if (IsFinished)
            return;
        
        EndTime = TimeOnly.FromDateTime(DateTime.Now);
    }

    public void TriggerElapsedTimeUpdate()
    {
        this.RaisePropertyChanged(nameof(ElapsedTime));
    }

    private TimeSpan CalculateElapsedTime()
    {
        if (!StartTime.HasValue)
            return TimeSpan.Zero;
        
        if (!IsValid)
            return TimeSpan.Zero;

        if (!EndTime.HasValue)
            return TimeOnly.FromDateTime(DateTime.Now) - StartTime.Value;
        
        return EndTime.Value - StartTime.Value;
    }

    private bool ComputeIsValid()
    {
        if (!StartTime.HasValue && EndTime.HasValue)
            return false;

        if (StartTime.HasValue && EndTime.HasValue && EndTime.Value < StartTime.Value)
            return false;

        return true;
    }
}