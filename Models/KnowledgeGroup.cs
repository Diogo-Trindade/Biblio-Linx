using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BiblioLinx.Models;

public partial class KnowledgeGroup : ObservableObject
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; } 

    [ObservableProperty] private string _groupName = string.Empty;
    [ObservableProperty] private bool _isExpanded = true;
    
    [ObservableProperty] private bool _isFavorite = false;
    [ObservableProperty] private string _colorHex = "#FFFFFF"; // Cor inicial Branca

    // NOVO CAMPO: Controla a ordem na fila
    [ObservableProperty] private int _order = 0;

    [Ignore] 
    public ObservableCollection<KnowledgeItem> Items { get; set; } = new();
}