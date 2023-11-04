using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MicroTimes;

public class Parser
{
    private static readonly Regex LineRegex = new(@"(\d\d\.\d\d\.\d\d\d\d)\s+(\d\d:\d\d\s+)?(\d\d:\d\d\s+)?(.*)");

    public async Task WriteFile(TimeEntryViewModel activeTimeEntry, TimeEntryCollection timeEntryCollection, string path)
    {
        var allLines = CreateLines(timeEntryCollection);
        if (IsEntryWorthSaving(activeTimeEntry))
        {
            var activeEntryLine = CreateLine(activeTimeEntry);
            allLines.Add(activeEntryLine);
        }
        
        await File.WriteAllLinesAsync(path, allLines);
    }

    private List<string> CreateLines(TimeEntryCollection timeEntryCollection)
    {
        var entries = timeEntryCollection
            .GetAllEntries()
            .OrderBy(x => x.Key);
        
        var lines = new List<string>();
        foreach (var item in entries)
        {
            var linesForDay = CreateLines(item.Value);
            lines.AddRange(linesForDay);
        }

        return lines;
    }

    private List<string> CreateLines(DayViewModel day)
    {
        var entries = day
            .Entries
            .Where(IsEntryWorthSaving)
            .OrderBy(x => x.StartTime);
        
        var lines = new List<string>();
        foreach (var item in entries)
        {
            var line = CreateLine(item);
            lines.Add(line);
        }

        return lines;
    }

    private bool IsEntryWorthSaving(TimeEntryViewModel timeEntryViewModel)
    {
        return timeEntryViewModel.StartTime.HasValue 
               || timeEntryViewModel.EndTime.HasValue 
               || !string.IsNullOrEmpty(timeEntryViewModel.Description);
    }

    private string CreateLine(TimeEntryViewModel timeEntryViewModel)
    {
        var parts = new string?[]
        {
            timeEntryViewModel.Date.ToString("dd.MM.yyyy"),
            timeEntryViewModel.StartTime?.ToString("HH:mm"),
            timeEntryViewModel.EndTime?.ToString("HH:mm"),
            timeEntryViewModel.Description
        }.Where(x => x != null);

        return string.Join(" ", parts);
    }
    
    public async Task<TimeEntryCollection> ParseFile(string path)
    {
        var timeEntries = await ParseLines(path);

        var dictionary = timeEntries
            .GroupBy(x => x.Date)
            .ToDictionary(x => x.Key, x => new DayViewModel(x.Key, x.ToList()));

        return new TimeEntryCollection(dictionary);
    }
    
    private async Task<List<TimeEntryViewModel>> ParseLines(string path)
    {
        var lines = await File.ReadAllLinesAsync(path);

        var timeEntries = new List<TimeEntryViewModel>();
        foreach (var line in lines)
        {
            var timeEntry = ParseLine(line);
            if (timeEntry == null)
                continue;
            
            timeEntries.Add(timeEntry);
        }

        return timeEntries;
    }

    private TimeEntryViewModel? ParseLine(string line)
    {
        var match = LineRegex.Match(line);
        if (!match.Success)
            return null;

        var date = match.Groups[1]?.Value?.Trim();
        var startTime = match.Groups[2]?.Value?.Trim();
        var endTime = match.Groups[3]?.Value?.Trim();
        var description = match.Groups[4]?.Value?.Trim();

        if (string.IsNullOrEmpty(date))
            return null;

        var canParseDate = DateOnly.TryParseExact(date, "dd.MM.yyyy", out var parsedDate);
        if (!canParseDate)
            return null;

        var timeEntry = new TimeEntryViewModel(parsedDate)
        {
            Description = description ?? string.Empty
        };

        if (!string.IsNullOrEmpty(startTime))
        {
            var canParseTime = TimeOnly.TryParseExact(startTime, "HH:mm", out var parsedTime);
            if (!canParseTime)
                return null;

            timeEntry.StartTime = parsedTime;
        }
        
        if (!string.IsNullOrEmpty(endTime))
        {
            var canParseTime = TimeOnly.TryParseExact(endTime, "HH:mm", out var parsedTime);
            if (!canParseTime)
                return null;

            timeEntry.EndTime = parsedTime;
        }

        return timeEntry;
    }
}