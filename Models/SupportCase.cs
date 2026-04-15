using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BiblioLinx.Models;

public partial class SupportCase : ObservableObject
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _originalText = string.Empty;
    [ObservableProperty] private string _summaryText = string.Empty;
    [ObservableProperty] private DateTime _createdAt = DateTime.Now;
}