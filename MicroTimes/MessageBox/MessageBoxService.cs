using System.Threading.Tasks;
using CustomMessageBox.Avalonia;

namespace MicroTimes.MessageBox;

public class MessageBoxService : IMessageBoxService
{
    public async Task<bool> ConfirmDelete(string title)
    {
        var result = await CustomMessageBox.Avalonia.MessageBox.Show($"Do you want to delete the entry '{title}'", "Delete", MessageBoxButtons.YesNo);
        return result == MessageBoxResult.Yes;
    }
}

public interface IMessageBoxService
{
    Task<bool> ConfirmDelete(string title);
}