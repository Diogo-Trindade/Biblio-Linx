using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;

namespace BiblioLinx.Models;

public partial class ChatMessage : ObservableObject
{
    [ObservableProperty] private string _role = string.Empty; // "User" ou "AI"
    [ObservableProperty] private string _text = string.Empty;

    // Propriedades calculadas para facilitar o visual no XAML sem usar conversores
    public bool IsUser => Role == "User";
    public LayoutOptions Alignment => IsUser ? LayoutOptions.End : LayoutOptions.Start;
    public string BackgroundColor => IsUser ? "#FF6D00" : "#E5E7EB"; // Laranja para o usuário, Cinza para a IA
    public string TextColor => IsUser ? "White" : "#111827";

    
    // Adicione esta linha que estava faltando!
    public bool IsAi { get; set; } 
}