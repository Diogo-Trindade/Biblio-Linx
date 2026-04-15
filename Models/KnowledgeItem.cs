using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BiblioLinx.Models;

public partial class KnowledgeItem : ObservableObject
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; } 
    public int GroupId { get; set; } 

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _content = string.Empty;
    [ObservableProperty] private DateTime _lastModified = DateTime.Now;

    [ObservableProperty] private bool _isFavorite = false;
    
    // NOVO CAMPO: Para subir e descer a página
    [ObservableProperty] private int _order = 0;
}