using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using BiblioLinx.Models;
using BiblioLinx.Services;

#if WINDOWS
using Microsoft.UI.Xaml;
#endif

using MauiApp = Microsoft.Maui.Controls.Application;

namespace BiblioLinx.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IGroqApiService _groqService;
        private readonly DatabaseService _dbService;

      
        [ObservableProperty] private string _appBackgroundColor = "#F4F0FB";
        [ObservableProperty] private string _sidebarColor = "#4C1D95";
        [ObservableProperty] private string _topbarColor = "#4C1D95";
        [ObservableProperty] private string _surfaceColor = "#F4F0FB";
        [ObservableProperty] private string _cardColor = "#FFFFFF";
        [ObservableProperty] private string _borderColor = "#DED3EF";
        [ObservableProperty] private string _buttonColor = "#FF8500";
        [ObservableProperty] private string _primaryTextColor = "#1F2430";
        [ObservableProperty] private string _secondaryTextColor = "#6B7280";
        [ObservableProperty] private string _sidebarTextColor = "#FFFFFF";
        [ObservableProperty] private string _activeItemColor = "#FF8500";
        [ObservableProperty] private string _activeItemTextColor = "#FFFFFF";
        [ObservableProperty] private string _hoverColor = "#5B21B6";

        // TEMA PADRÃO VOLTOU PARA ROXO/LARANJA
        [ObservableProperty] private string _selectedTheme = "RoxoLaranja";
        [ObservableProperty] private bool _startWithWindows = true;
        [ObservableProperty] private string _selectedLanguage = "PT";

      
        [ObservableProperty] private bool _isProAtivo = false; 
        [ObservableProperty] private bool _isProAlertVisible = false; 

        [ObservableProperty] private string _groqApiKey = string.Empty;
        partial void OnGroqApiKeyChanged(string value) => VerificarLicencaLocal();

        [ObservableProperty] private string _txtDashboard = "Dashboard";
        [ObservableProperty] private string _txtCasos = "Casos Salvos";
        [ObservableProperty] private string _txtCompartilhar = "Compartilhar";
        [ObservableProperty] private string _txtIA = "IA Assistente";
        [ObservableProperty] private string _txtResumo = "Resumir Caso";
        [ObservableProperty] private string _txtInstrucoes = "Instruções";
        [ObservableProperty] private string _txtConfiguracoes = "Configurações";
        [ObservableProperty] private string _txtPesquisar = "Pesquisar macros ou grupos...";
        [ObservableProperty] private string _txtAddSecao = "Adicionar seção";
        [ObservableProperty] private string _txtTema = "Tema Visual";
        [ObservableProperty] private string _txtIdioma = "Idioma / Language";
        [ObservableProperty] private string _txtIniciar = "Iniciar com o Windows";
        [ObservableProperty] private string _txtConcluido = "Concluído";

        [ObservableProperty] private string _currentView = "Information";
        [ObservableProperty] private bool _isInformationVisible = true;
        [ObservableProperty] private bool _isCasosVisible = false;
        [ObservableProperty] private bool _isAIAssistantVisible = false;
        [ObservableProperty] private bool _isInstructionsVisible = false;
        [ObservableProperty] private bool _isResumoVisible = false;
        [ObservableProperty] private bool _isShareModalVisible = false;
        [ObservableProperty] private bool _isSettingsModalVisible = false;

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { SetProperty(ref _isBusy, value); OnPropertyChanged(nameof(IsNotBusy)); } }
        public bool IsNotBusy => !IsBusy;

        private List<KnowledgeGroup> _todosOsGrupos = new();
        [ObservableProperty] private ObservableCollection<KnowledgeGroup> _knowledgeGroups = new();
        [ObservableProperty] private KnowledgeItem? _selectedKnowledgeItem;
        
        [ObservableProperty] private string _textoLeitura = string.Empty;
        [ObservableProperty] private string _editorHtmlText = string.Empty;
        
        [ObservableProperty] private bool _isEditorEnabled = false;
        [ObservableProperty] private bool _isReadMode = false;
        [ObservableProperty] private bool _isWelcomeVisible = true;
        [ObservableProperty] private string _searchText = string.Empty;
        partial void OnSearchTextChanged(string value) => FiltrarBaseDeConhecimento(value);

       
        [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasSavedCases))] private ObservableCollection<SupportCase> _supportCases = new();
        public bool HasSavedCases => SupportCases != null && SupportCases.Count > 0;

        [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsCasoSelected))] private SupportCase? _selectedSupportCase;
        public bool IsCasoSelected => SelectedSupportCase != null;

        [ObservableProperty] private ObservableCollection<ChatMessage> _chatMessages = new();
        [ObservableProperty] private string _currentChatMessage = string.Empty;
        [ObservableProperty] private string _chatInput = string.Empty;
        [ObservableProperty] private string _resumoOutput = string.Empty;
        [ObservableProperty] private bool _isGerandoResumo = false;
        [ObservableProperty] private bool _isImprovingText = false;
        private string _conhecimentoOcultoIA = string.Empty;
        private DateTime _ultimoClique = DateTime.MinValue;

        public MainViewModel(IGroqApiService groqService, DatabaseService dbService)
        {
            _groqService = groqService;
            _dbService = dbService;

            SelectedTheme = Preferences.Get(nameof(SelectedTheme), "RoxoLaranja");
            SelectedLanguage = Preferences.Get(nameof(SelectedLanguage), "PT");
            StartWithWindows = Preferences.Get(nameof(StartWithWindows), true);
            GroqApiKey = Preferences.Get("GroqApiKey", string.Empty);

            ApplyTheme(SelectedTheme);
            ApplyLanguage(SelectedLanguage);
            VerificarLicencaLocal();
        }

        private void VerificarLicencaLocal() => IsProAtivo = !string.IsNullOrWhiteSpace(GroqApiKey) && GroqApiKey.Length > 10;

        private bool _foiInicializado = false;

        public async Task InicializarAsync()
        {
            if (_foiInicializado) return; 
            _foiInicializado = true;

            try
            {
                await RecarregarBaseDeDadosInterna();
                await CarregarCasosDaBaseDeDados();
                await CarregarConhecimentoOcultoAsync();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private async Task CarregarCasosDaBaseDeDados()
        {
            var casos = await _dbService.GetCasosAsync();
            MainThread.BeginInvokeOnMainThread(() => 
            {
                SupportCases.Clear();
                foreach (var caso in casos) SupportCases.Add(caso);
                OnPropertyChanged(nameof(HasSavedCases)); 
            });
        }

        private async Task RecarregarBaseDeDadosInterna()
        {
            var grupos = await _dbService.GetBaseConhecimentoAsync();
            MainThread.BeginInvokeOnMainThread(() => 
            {
                _todosOsGrupos.Clear();
                foreach (var grupo in grupos) _todosOsGrupos.Add(grupo);
                FiltrarBaseDeConhecimento(SearchText);
            });
        }

        private static string RemoverLixoCSS(string content)
        {
            if (string.IsNullOrEmpty(content)) return content;
            int safeGuard = 0;
            while (safeGuard++ < 100)
            {
                int start = content.IndexOf("<style", StringComparison.OrdinalIgnoreCase);
                if (start == -1) break;
                int end = content.IndexOf("</style>", start, StringComparison.OrdinalIgnoreCase);
                if (end == -1) { content = content.Remove(start); break; }
                content = content.Remove(start, end - start + 8);
            }
            return content.Trim();
        }

        private static string StripHtmlFast(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            var sb = new StringBuilder(html.Length);
            bool inTag = false;
            foreach (char c in html)
            {
                if (c == '<') { inTag = true; continue; }
                if (c == '>') { inTag = false; continue; }
                if (!inTag) sb.Append(c);
            }
            return sb.ToString().Trim();
        }

        private void SetMainView(string viewName)
        {
            CurrentView = viewName;
            IsInformationVisible = viewName == "Information"; IsCasosVisible = viewName == "Casos";
            IsAIAssistantVisible = viewName == "AIAssistant"; IsInstructionsVisible = viewName == "Instructions";
            IsResumoVisible = viewName == "Resumo";
        }

        [RelayCommand]
        private void Navigate(string viewName)
        {
            IsShareModalVisible = false; IsSettingsModalVisible = false;
            if (viewName == "Share") { IsShareModalVisible = true; return; }
            if (viewName == "Settings") { IsSettingsModalVisible = true; return; }
            
            
            if ((viewName == "AIAssistant" || viewName == "Resumo") && !IsProAtivo) 
            { 
                IsProAlertVisible = true; 
                return; 
            }
            
            SetMainView(viewName);
        }

        [RelayCommand] private void CloseShareModal() => IsShareModalVisible = false;
        [RelayCommand] private void CloseProAlert() => IsProAlertVisible = false;
        [RelayCommand] private void CloseSettingsModal() { Preferences.Set("GroqApiKey", GroqApiKey); VerificarLicencaLocal(); IsSettingsModalVisible = false; }

        [RelayCommand] private void SelectKnowledgeItem(KnowledgeItem item) { _ultimoClique = DateTime.Now; SelectedKnowledgeItem = item; }
        [RelayCommand] private void ClearSelection() { if ((DateTime.Now - _ultimoClique).TotalMilliseconds > 300) SelectedKnowledgeItem = null; }

        partial void OnSelectedKnowledgeItemChanged(KnowledgeItem? value)
        {
            if (value != null)
            {
                value.PropertyChanged -= KnowledgeItem_PropertyChanged; 
                value.PropertyChanged += KnowledgeItem_PropertyChanged;
                value.Content = RemoverLixoCSS(value.Content);
                
                TextoLeitura = value.Content; 
                IsWelcomeVisible = false; IsReadMode = true; IsEditorEnabled = false;
                EditorHtmlText = string.Empty; 
            }
            else 
            { 
                TextoLeitura = string.Empty;
                IsWelcomeVisible = true; IsReadMode = false; IsEditorEnabled = false; 
            }
        }

        private async void KnowledgeItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(KnowledgeItem.Title) && SelectedKnowledgeItem != null)
                await _dbService.SalvarPaginaAsync(SelectedKnowledgeItem);
        }

        [RelayCommand] private void HabilitarEdicao() { EditorHtmlText = SelectedKnowledgeItem?.Content ?? string.Empty; IsReadMode = false; IsEditorEnabled = true; }
        
        [RelayCommand] 
        private async Task SalvarEdicao()
        {
            if (SelectedKnowledgeItem == null) return;
            SelectedKnowledgeItem.Content = RemoverLixoCSS(EditorHtmlText);
            await _dbService.SalvarPaginaAsync(SelectedKnowledgeItem);
            TextoLeitura = SelectedKnowledgeItem.Content;
            IsEditorEnabled = false; IsReadMode = true;
        }

        private void FiltrarBaseDeConhecimento(string query)
        {
            if (!string.IsNullOrWhiteSpace(query) && CurrentView != "Information") Navigate("Information");
            var queryLower = query?.ToLowerInvariant() ?? string.Empty;
            var gruposOrdenados = _todosOsGrupos.OrderByDescending(g => g.IsFavorite).ThenBy(g => g.Order).ThenBy(g => g.GroupName).ToList();
            
            MainThread.BeginInvokeOnMainThread(() => 
            {
                KnowledgeGroups.Clear();
                foreach (var grupo in gruposOrdenados)
                {
                    var itensFiltrados = grupo.Items?.Where(i => (i.Title != null && i.Title.ToLowerInvariant().Contains(queryLower)) || (i.Content != null && i.Content.ToLowerInvariant().Contains(queryLower))).ToList() ?? new List<KnowledgeItem>();
                    bool grupoVisivel = string.IsNullOrWhiteSpace(query) || (grupo.GroupName != null && grupo.GroupName.ToLowerInvariant().Contains(queryLower)) || itensFiltrados.Any();
                    if (!grupoVisivel) continue;
                    
                    var grupoVisual = new KnowledgeGroup { Id=grupo.Id, GroupName=grupo.GroupName??string.Empty, IsExpanded=grupo.IsExpanded, IsFavorite=grupo.IsFavorite, ColorHex=grupo.ColorHex, Order=grupo.Order, Items = new ObservableCollection<KnowledgeItem>() };
                    var itensParaMostrar = (string.IsNullOrWhiteSpace(query) || (grupo.GroupName != null && grupo.GroupName.ToLowerInvariant().Contains(queryLower))) ? grupo.Items?.ToList() ?? new List<KnowledgeItem>() : itensFiltrados;
                    foreach (var item in itensParaMostrar.OrderByDescending(i => i.IsFavorite).ThenBy(i => i.Order).ThenBy(i => i.Title)) grupoVisual.Items.Add(item);
                    KnowledgeGroups.Add(grupoVisual);
                }
            });
        }

        [RelayCommand] private async Task AddGroup() { string result = await ShowPrompt(TxtAddSecao, "Digite o nome da nova seção:"); if (!string.IsNullOrWhiteSpace(result)) { await _dbService.SalvarGrupoAsync(new KnowledgeGroup { GroupName=result.Trim(), IsExpanded=true, Order=_todosOsGrupos.Count }); await RecarregarBaseDeDadosInterna(); } }
        [RelayCommand] private async Task ChangeGroupColor(KnowledgeGroup grupo) { if (grupo==null) return; var cores = new[]{"#FFFFFF","#F3E8FF","#BBF7D0","#BFDBFE","#FECACA","#FFEDD5","#FEF08A"}; grupo.ColorHex = cores[(Array.IndexOf(cores,grupo.ColorHex)+1)%cores.Length]; await _dbService.SalvarGrupoAsync(grupo); await RecarregarBaseDeDadosInterna(); }
        [RelayCommand] private async Task MoveGroupUp(KnowledgeGroup grupo) { if (grupo==null||grupo.Order<=0) return; grupo.Order--; await _dbService.SalvarGrupoAsync(grupo); await RecarregarBaseDeDadosInterna(); }
        [RelayCommand] private async Task MoveGroupDown(KnowledgeGroup grupo) { if (grupo==null) return; grupo.Order++; await _dbService.SalvarGrupoAsync(grupo); await RecarregarBaseDeDadosInterna(); }
        [RelayCommand] private async Task ToggleFavoriteGroup(KnowledgeGroup grupo) { if (grupo==null) return; grupo.IsFavorite=!grupo.IsFavorite; await _dbService.SalvarGrupoAsync(grupo); await RecarregarBaseDeDadosInterna(); }
        [RelayCommand] private async Task EditGroup(KnowledgeGroup grupo) { if (grupo==null) return; string result = await ShowPrompt("Editar Seção","Novo nome:",initialValue:grupo.GroupName); if (!string.IsNullOrWhiteSpace(result)) { grupo.GroupName=result.Trim(); await _dbService.SalvarGrupoAsync(grupo); await RecarregarBaseDeDadosInterna(); } }
        [RelayCommand] private async Task DeleteGroup(KnowledgeGroup grupo) { if (grupo==null) return; bool confirm = await ShowConfirm("Excluir Seção",$"Deseja excluir '{grupo.GroupName}' e suas páginas?","Sim","Cancelar"); if (confirm) { await _dbService.ExcluirGrupoAsync(grupo); SelectedKnowledgeItem=null; await RecarregarBaseDeDadosInterna(); } }
        [RelayCommand] private async Task ToggleGroup(KnowledgeGroup group) { if (group==null) return; group.IsExpanded=!group.IsExpanded; await _dbService.SalvarGrupoAsync(group); }

        [RelayCommand] private async Task AddItem(KnowledgeGroup group) { if (group==null) return; int numItens = group.Items?.Count ?? 0; var newItem = new KnowledgeItem { Title="Nova Página", Content="", GroupId=group.Id, Order=numItens }; await _dbService.SalvarPaginaAsync(newItem); await RecarregarBaseDeDadosInterna(); var grupoAtualizado = _todosOsGrupos.FirstOrDefault(g=>g.Id==group.Id); var itemCriado = grupoAtualizado?.Items?.LastOrDefault(); if (itemCriado!=null) SelectKnowledgeItem(itemCriado); }
        [RelayCommand] private async Task MoveItemUp(KnowledgeItem item) { if (item==null||item.Order<=0) return; item.Order--; await _dbService.SalvarPaginaAsync(item); await RecarregarBaseDeDadosInterna(); }
        [RelayCommand] private async Task MoveItemDown(KnowledgeItem item) { if (item==null) return; item.Order++; await _dbService.SalvarPaginaAsync(item); await RecarregarBaseDeDadosInterna(); }
        [RelayCommand] private async Task EditItem(KnowledgeItem item) { if (item==null) return; string result = await ShowPrompt("Renomear","Novo nome:",initialValue:item.Title); if (!string.IsNullOrWhiteSpace(result)) { item.Title=result.Trim(); await _dbService.SalvarPaginaAsync(item); await RecarregarBaseDeDadosInterna(); } }
        [RelayCommand] private async Task DeleteItem(KnowledgeItem item) { if (item==null) return; bool confirm = await ShowConfirm("Excluir",$"Deseja excluir a página '{item.Title}'?","Sim","Cancelar"); if (confirm) { await _dbService.ExcluirPaginaAsync(item); if (SelectedKnowledgeItem?.Id==item.Id) SelectedKnowledgeItem=null; await RecarregarBaseDeDadosInterna(); } }
        [RelayCommand] private async Task ToggleFavoriteItem(KnowledgeItem item) { if (item==null) return; item.IsFavorite=!item.IsFavorite; await _dbService.SalvarPaginaAsync(item); await RecarregarBaseDeDadosInterna(); }

        private async Task CarregarConhecimentoOcultoAsync()
        {
            try { using var stream = await FileSystem.OpenAppPackageFileAsync("casos_ia.txt"); using var reader = new StreamReader(stream); _conhecimentoOcultoIA = await reader.ReadToEndAsync(); }
            catch { _conhecimentoOcultoIA = string.Empty; }
        }

        [RelayCommand]
        private async Task ImproveTextWithAI()
        {
            if (!IsProAtivo) { IsProAlertVisible = true; return; }
            if (string.IsNullOrWhiteSpace(EditorHtmlText) || IsImprovingText) return;
            IsImprovingText=true;
            try
            {
                string prompt = "Aja como um revisor profissional. Melhore a clareza, coesão e corrija a gramática do texto. IMPORTANTE: Retorne APENAS o texto melhorado.\nTexto: " + RemoverLixoCSS(EditorHtmlText);
                var response = await _groqService.ResponderChatAsync(prompt, string.Empty);
                if (!string.IsNullOrWhiteSpace(response)&&!response.StartsWith("⚠️")) EditorHtmlText = response.Replace("```html",string.Empty).Replace("```",string.Empty).Trim().Replace("\\n","<br>").Replace("\n","<br>");
            }
            catch { await ShowAlert("Ops!","Não foi possível conectar à IA.","OK"); }
            finally { IsImprovingText=false; }
        }

        private string GetBaseDeConhecimentoEnxuta(string pergunta)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(_conhecimentoOcultoIA)) sb.AppendLine(_conhecimentoOcultoIA);
            var palavrasChave = pergunta.ToLowerInvariant().Split(new[]{' ','?','!','.',','}, StringSplitOptions.RemoveEmptyEntries).Where(p=>p.Length>3).ToList();
            const int MaxCaracteres = 12_000;
            foreach (var grupo in _todosOsGrupos)
            {
                if (grupo.Items == null) continue;
                foreach (var item in grupo.Items)
                {
                    if (sb.Length>=MaxCaracteres) break;
                    string textoLimpo = StripHtmlFast(item.Content??string.Empty);
                    string textoBusca = (item.Title+" "+textoLimpo).ToLowerInvariant();
                    bool relevante = palavrasChave.Count==0||palavrasChave.Any(p=>textoBusca.Contains(p));
                    if (!relevante||string.IsNullOrWhiteSpace(textoLimpo)) continue;
                    sb.AppendLine($"--- {item.Title} ---");
                    sb.AppendLine(textoLimpo.Length>1500?textoLimpo[..1500]+"...":textoLimpo);
                    sb.AppendLine();
                }
                if (sb.Length>=MaxCaracteres) break;
            }
            return sb.ToString();
        }

        [RelayCommand] private async Task ClearChat() { bool confirm = await ShowConfirm("Limpar","Reiniciar conversa?","Sim","Cancelar"); if (confirm) ChatMessages.Clear(); }

        [RelayCommand]
        private async Task EnviarMensagemChat()
        {
            if (!IsProAtivo) { IsProAlertVisible = true; return; }
            if (string.IsNullOrWhiteSpace(CurrentChatMessage)) return;
            IsBusy=true;
            var userMsg=CurrentChatMessage; CurrentChatMessage=string.Empty;
            ChatMessages.Add(new ChatMessage{Text=userMsg,IsAi=false,Role="User"});
            var thinking=new ChatMessage{Text="A processar o pedido...",IsAi=true,Role="AI"};
            ChatMessages.Add(thinking);
            try
            {
                var baseFiltrada=GetBaseDeConhecimentoEnxuta(userMsg);
                var response=await _groqService.ResponderChatAsync(userMsg,baseFiltrada);
                response=response.Replace("\\n","<br>").Replace("\n","<br>");
                ChatMessages.Remove(thinking); ChatMessages.Add(new ChatMessage{Text=response,IsAi=true,Role="AI"});
            }
            catch { ChatMessages.Remove(thinking); ChatMessages.Add(new ChatMessage{Text="⚠️ Ocorreu um erro. Verifique a sua conexão ou licença.",IsAi=true,Role="AI"}); }
            finally { IsBusy=false; }
        }

        [RelayCommand]
        private async Task GerarResumo()
        {
            if (!IsProAtivo) { IsProAlertVisible = true; return; }
            if (string.IsNullOrWhiteSpace(ChatInput)) return;
            IsGerandoResumo=true; ResumoOutput="Analisando...";
            try { ResumoOutput=await _groqService.ResumirCasoAsync(ChatInput,_conhecimentoOcultoIA); }
            catch { ResumoOutput="⚠️ Erro ao processar resumo."; }
            finally { IsGerandoResumo=false; }
        }

        [RelayCommand] private void ExcluirResumo() { ChatInput=string.Empty; ResumoOutput=string.Empty; }
        [RelayCommand] private void SelectSupportCase(SupportCase caso) => SelectedSupportCase = caso;

        [RelayCommand]
        private async Task SalvarCaso()
        {
            if (string.IsNullOrWhiteSpace(ResumoOutput)||ResumoOutput.StartsWith("⚠️")) { await ShowAlert("Aviso","Gere um resumo válido primeiro antes de salvar.","OK"); return; }
            string result = await ShowPrompt("Salvar Caso","Título:");
            if (!string.IsNullOrWhiteSpace(result)) 
            { 
                await _dbService.SalvarCasoAsync(new SupportCase{Title=result.Trim(),OriginalText=ChatInput,SummaryText=ResumoOutput}); 
                await CarregarCasosDaBaseDeDados();
            }
        }

        [RelayCommand] private async Task EditCaso(SupportCase caso) { if (caso==null) return; string result = await ShowPrompt("Renomear","Novo título:",initialValue:caso.Title); if (!string.IsNullOrWhiteSpace(result)) { caso.Title=result.Trim(); await _dbService.SalvarCasoAsync(caso); } }
        [RelayCommand] private async Task DeleteCaso(SupportCase caso) { if (caso==null) return; bool confirm = await ShowConfirm("Excluir",$"Deseja excluir '{caso.Title}'?","Sim","Cancelar"); if (confirm) { await _dbService.ExcluirCasoAsync(caso); await CarregarCasosDaBaseDeDados(); if (SelectedSupportCase==caso) SelectedSupportCase=null; } }

        [RelayCommand]
        private async Task ExportarJson()
        {
            try
            {
                var grupos = await _dbService.GetBaseConhecimentoAsync();
                var exportData = new ExportData {
                    Grupos = grupos.Select(g => new GrupoExport {
                        GroupName=g.GroupName, ColorHex=g.ColorHex, Order=g.Order, IsFavorite=g.IsFavorite,
                        Items=g.Items?.Select(i=>new ItemExport{Title=i.Title,Content=i.Content,Order=i.Order,IsFavorite=i.IsFavorite}).ToList() ?? new List<ItemExport>()
                    }).ToList()
                };
                var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions{WriteIndented=true});
                var fileName = $"BiblioLinx_Export_{DateTime.Now:yyyyMMdd_HHmm}.json";
#if WINDOWS
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                var nativeWindow = MauiApp.Current!.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
                if (nativeWindow != null)
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                    WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
                }
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("JSON", new List<string>{".json"});
                savePicker.SuggestedFileName = fileName;
                var file = await savePicker.PickSaveFileAsync();
                if (file==null) return;
                await Windows.Storage.FileIO.WriteTextAsync(file, json);
                await ShowAlert("Sucesso",$"Ficheiro guardado: {file.Name}","OK");
#else
                var path = Path.Combine(FileSystem.AppDataDirectory, fileName);
                await File.WriteAllTextAsync(path, json);
                await Share.RequestAsync(new ShareFileRequest{Title="Exportar BiblioLinx",File=new ShareFile(path)});
#endif
                IsShareModalVisible=false;
            }
            catch (Exception ex) { await ShowAlert("Erro",$"Falha ao exportar: {ex.Message}","OK"); }
        }

        [RelayCommand]
        private async Task ImportarJson()
        {
            try
            {
                string json;
#if WINDOWS
                var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
                var nativeWindow = MauiApp.Current!.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
                if (nativeWindow != null)
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                    WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);
                }
                openPicker.FileTypeFilter.Add(".json");
                var winFile = await openPicker.PickSingleFileAsync();
                if (winFile==null) return;
                json = await Windows.Storage.FileIO.ReadTextAsync(winFile);
#else
                var result = await FilePicker.PickAsync(new PickOptions{PickerTitle="Selecionar ficheiro BiblioLinx",FileTypes=new FilePickerFileType(new Dictionary<DevicePlatform,IEnumerable<string>>{{DevicePlatform.WinUI,new[]{".json"}},{DevicePlatform.iOS,new[]{"public.json"}},{DevicePlatform.Android,new[]{"application/json"}}})});
                if (result==null) return;
                using var stream = await result.OpenReadAsync();
                using var reader = new StreamReader(stream);
                json = await reader.ReadToEndAsync();
#endif
                var exportData = JsonSerializer.Deserialize<ExportData>(json);
                if (exportData?.Grupos==null||exportData.Grupos.Count==0) { await ShowAlert("Aviso","O ficheiro não contém dados válidos.","OK"); return; }
                bool confirm = await ShowConfirm("Importar dados",$"Serão importados {exportData.Grupos.Count} grupo(s). Os dados existentes NÃO serão apagados. Continuar?","Importar","Cancelar");
                if (!confirm) return;
                foreach (var grupoExp in exportData.Grupos)
                {
                    var novoGrupo = new KnowledgeGroup{GroupName=grupoExp.GroupName,ColorHex=grupoExp.ColorHex,Order=_todosOsGrupos.Count+grupoExp.Order,IsFavorite=grupoExp.IsFavorite,IsExpanded=false};
                    await _dbService.SalvarGrupoAsync(novoGrupo);
                    foreach (var itemExp in grupoExp.Items)
                        await _dbService.SalvarPaginaAsync(new KnowledgeItem{Title=itemExp.Title,Content=itemExp.Content??string.Empty,GroupId=novoGrupo.Id,Order=itemExp.Order,IsFavorite=itemExp.IsFavorite});
                }
                await RecarregarBaseDeDadosInterna();
                IsShareModalVisible=false;
                await ShowAlert("Sucesso",$"{exportData.Grupos.Count} grupo(s) importado(s) com sucesso!","OK");
            }
            catch (Exception ex) { await ShowAlert("Erro",$"Falha ao importar: {ex.Message}","OK"); }
        }

        [RelayCommand] private void ChangeLanguage(string lang) { SelectedLanguage = lang; Preferences.Set(nameof(SelectedLanguage), lang); ApplyLanguage(lang); }
        private void ApplyLanguage(string lang)
        {
            if (lang == "EN") { TxtDashboard = "Dashboard"; TxtCasos = "Saved Cases"; TxtCompartilhar = "Share"; TxtIA = "AI Assistant"; TxtResumo = "Summarize Case"; TxtInstrucoes = "Instructions"; TxtConfiguracoes = "Settings"; TxtPesquisar = "Search macros or groups..."; TxtAddSecao = "Add section"; TxtTema = "Visual Theme"; TxtIdioma = "Language"; TxtIniciar = "Start with Windows"; TxtConcluido = "Done"; }
            else if (lang == "ES") { TxtDashboard = "Dashboard"; TxtCasos = "Casos Guardados"; TxtCompartilhar = "Compartir"; TxtIA = "Asistente IA"; TxtResumo = "Resumir Caso"; TxtInstrucoes = "Instrucciones"; TxtConfiguracoes = "Configuración"; TxtPesquisar = "Buscar macros o grupos..."; TxtAddSecao = "Añadir sección"; TxtTema = "Tema Visual"; TxtIdioma = "Idioma"; TxtIniciar = "Iniciar con Windows"; TxtConcluido = "Hecho"; }
            else { TxtDashboard = "Dashboard"; TxtCasos = "Casos Salvos"; TxtCompartilhar = "Compartilhar"; TxtIA = "IA Assistente"; TxtResumo = "Resumir Caso"; TxtInstrucoes = "Instruções"; TxtConfiguracoes = "Configurações"; TxtPesquisar = "Pesquisar macros ou grupos..."; TxtAddSecao = "Adicionar seção"; TxtTema = "Tema Visual"; TxtIdioma = "Idioma"; TxtIniciar = "Iniciar com o Windows"; TxtConcluido = "Concluído"; }
        }

        [RelayCommand] private void ChangeTheme(string theme) { SelectedTheme = theme; Preferences.Set(nameof(SelectedTheme), theme); ApplyTheme(theme); }
        private void ApplyTheme(string themeName)
        {
            switch (themeName)
            {
                case "Steam": AppBackgroundColor="#171A21";SidebarColor="#171A21";TopbarColor="#171A21";SurfaceColor="#2A475E";CardColor="#1B2838";BorderColor="#2A475E";ButtonColor="#66C0F4";PrimaryTextColor="#FFFFFF";SecondaryTextColor="#8F98A0";SidebarTextColor="#C7D5E0";ActiveItemColor="#2A475E";ActiveItemTextColor="#FFFFFF";HoverColor="#2A475E"; break;
                case "Green": AppBackgroundColor="#F4F9F4";SidebarColor="#2D6A4F";TopbarColor="#1B4332";SurfaceColor="#E8F5E9";CardColor="#FFFFFF";BorderColor="#C8E6C9";ButtonColor="#40916C";PrimaryTextColor="#1F2430";SecondaryTextColor="#52796F";SidebarTextColor="#D8F3DC";ActiveItemColor="#40916C";ActiveItemTextColor="#FFFFFF";HoverColor="#1B4332"; break;
                case "Red": AppBackgroundColor="#FDF2F2";SidebarColor="#7F1D1D";TopbarColor="#450A0A";SurfaceColor="#FEE2E2";CardColor="#FFFFFF";BorderColor="#FECACA";ButtonColor="#B91C1C";PrimaryTextColor="#1F2430";SecondaryTextColor="#7F1D1D";SidebarTextColor="#FCA5A5";ActiveItemColor="#B91C1C";ActiveItemTextColor="#FFFFFF";HoverColor="#450A0A"; break;
                case "BlueWhite": AppBackgroundColor="#F0F6FF";SidebarColor="#005A9E";TopbarColor="#003366";SurfaceColor="#E0F0FF";CardColor="#FFFFFF";BorderColor="#CCE0FF";ButtonColor="#0078D4";PrimaryTextColor="#001B3A";SecondaryTextColor="#4B6A90";SidebarTextColor="#B3D4FF";ActiveItemColor="#0078D4";ActiveItemTextColor="#FFFFFF";HoverColor="#003366"; break;
                case "Dark": AppBackgroundColor="#121212";SidebarColor="#1F1F1F";TopbarColor="#151515";SurfaceColor="#2D2D2D";CardColor="#1E1E1E";BorderColor="#333333";ButtonColor="#FF7A00";PrimaryTextColor="#E0E0E0";SecondaryTextColor="#A0A0A0";SidebarTextColor="#CCCCCC";ActiveItemColor="#333333";ActiveItemTextColor="#FFFFFF";HoverColor="#2D2D2D"; break;
                default: AppBackgroundColor="#F4F0FB";SidebarColor="#4C1D95";TopbarColor="#4C1D95";SurfaceColor="#F4F0FB";CardColor="#FFFFFF";BorderColor="#DED3EF";ButtonColor="#FF8500";PrimaryTextColor="#1F2430";SecondaryTextColor="#6B7280";SidebarTextColor="#FFFFFF";ActiveItemColor="#FF8500";ActiveItemTextColor="#FFFFFF";HoverColor="#5B21B6"; break;
            }
        }

  
        private static Page? GetCurrentPage()
        {
#pragma warning disable CS0618
            if (MauiApp.Current?.MainPage != null) return MauiApp.Current.MainPage;
#pragma warning restore CS0618
            if (Shell.Current?.CurrentPage is Page shellPage) return shellPage;
            if (MauiApp.Current?.Windows.FirstOrDefault()?.Page is Page winPage) return winPage;
            return null;
        }

        private async Task ShowAlert(string title, string message, string cancel)
        {
            if (GetCurrentPage() is Page page)
            {
#pragma warning disable CS0618
                await page.DisplayAlert(title, message, cancel);
#pragma warning restore CS0618
            }
        }

        private async Task<bool> ShowConfirm(string title, string message, string accept, string cancel)
        {
            if (GetCurrentPage() is Page page)
            {
#pragma warning disable CS0618
                return await page.DisplayAlert(title, message, accept, cancel);
#pragma warning restore CS0618
            }
            return false;
        }

        private async Task<string> ShowPrompt(string title, string message, string initialValue = "")
        {
            if (GetCurrentPage() is Page page)
            {
                return await page.DisplayPromptAsync(title, message, initialValue: initialValue) ?? string.Empty;
            }
            return string.Empty;
        }
    }

    public sealed class ExportData { [JsonPropertyName("grupos")] public List<GrupoExport> Grupos { get; set; } = new(); }
    public sealed class GrupoExport { [JsonPropertyName("groupName")] public string GroupName { get; set; } = string.Empty; [JsonPropertyName("colorHex")] public string? ColorHex { get; set; } [JsonPropertyName("order")] public int Order { get; set; } [JsonPropertyName("isFavorite")] public bool IsFavorite { get; set; } [JsonPropertyName("items")] public List<ItemExport> Items { get; set; } = new(); }
    public sealed class ItemExport { [JsonPropertyName("title")] public string Title { get; set; } = string.Empty; [JsonPropertyName("content")] public string? Content { get; set; } [JsonPropertyName("order")] public int Order { get; set; } [JsonPropertyName("isFavorite")] public bool IsFavorite { get; set; } }
}
