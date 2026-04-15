using BiblioLinx.ViewModels;
using System.Collections.Specialized;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace BiblioLinx;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;

        
        _viewModel.ChatMessages.CollectionChanged += OnChatMessagesChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InicializarAsync();
    }

    private void OnChatMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && _viewModel.ChatMessages.Count > 0)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(50); 
                var lastItem = _viewModel.ChatMessages.LastOrDefault();
                if (lastItem != null)
                {
                    ChatMessagesCollectionView?.ScrollTo(lastItem, position: ScrollToPosition.End, animate: true);
                }
            });
        }
    }

    private void UndoEditor_Clicked(object? sender, EventArgs e) => MainRichTextEditor?.Undo();
    private void RedoEditor_Clicked(object? sender, EventArgs e) => MainRichTextEditor?.Redo();

    private void ExecuteCommand(System.Windows.Input.ICommand command, object parameter)
    {
        if (command?.CanExecute(parameter) == true) command.Execute(parameter);
    }


    private void OnToggleGroupTapped(object sender, TappedEventArgs e) => ExecuteCommand(_viewModel.ToggleGroupCommand, ((Element)sender).BindingContext);
    private void OnAddItemClicked(object sender, EventArgs e) => ExecuteCommand(_viewModel.AddItemCommand, ((Button)sender).CommandParameter);
    private void OnFavoriteGroupClicked(object sender, EventArgs e) => ExecuteCommand(_viewModel.ToggleFavoriteGroupCommand, ((Button)sender).CommandParameter);
    private void OnChangeGroupColorClicked(object sender, EventArgs e) => ExecuteCommand(_viewModel.ChangeGroupColorCommand, ((Button)sender).CommandParameter);
    private void OnMoveGroupUpClicked(object sender, EventArgs e) => ExecuteCommand(_viewModel.MoveGroupUpCommand, ((Button)sender).CommandParameter);
    private void OnMoveGroupDownClicked(object sender, EventArgs e) => ExecuteCommand(_viewModel.MoveGroupDownCommand, ((Button)sender).CommandParameter);
    private void OnEditGroupClicked(object sender, EventArgs e) => ExecuteCommand(_viewModel.EditGroupCommand, ((Button)sender).CommandParameter);
    private void OnDeleteGroupClicked(object sender, EventArgs e) => ExecuteCommand(_viewModel.DeleteGroupCommand, ((Button)sender).CommandParameter);


    private void OnFavoriteItemClicked(object sender, EventArgs e) => ExecuteCommand(_viewModel.ToggleFavoriteItemCommand, ((Button)sender).CommandParameter);
    private void OnSelectItemTapped(object sender, TappedEventArgs e) => ExecuteCommand(_viewModel.SelectKnowledgeItemCommand, ((Element)sender).BindingContext);
    private void OnMoveItemUpClicked(object sender, EventArgs e) => ExecuteCommand(_viewModel.MoveItemUpCommand, ((Button)sender).CommandParameter);
    private void OnMoveItemDownClicked(object sender, EventArgs e) => ExecuteCommand(_viewModel.MoveItemDownCommand, ((Button)sender).CommandParameter);
    private void OnDeleteItemClicked(object sender, EventArgs e) => ExecuteCommand(_viewModel.DeleteItemCommand, ((Button)sender).CommandParameter);

    
    private void OnSelectSupportCaseTapped(object sender, TappedEventArgs e) => ExecuteCommand(_viewModel.SelectSupportCaseCommand, ((Element)sender).BindingContext);
    private void OnEditCasoClicked(object sender, EventArgs e) => ExecuteCommand(_viewModel.EditCasoCommand, ((Button)sender).CommandParameter);
    private void OnDeleteCasoClicked(object sender, EventArgs e) => ExecuteCommand(_viewModel.DeleteCasoCommand, ((Button)sender).CommandParameter);
}
