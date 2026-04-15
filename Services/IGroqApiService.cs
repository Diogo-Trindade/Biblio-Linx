namespace BiblioLinx.Services;

public interface IGroqApiService
{
    Task<string> ResumirCasoAsync(string chatCliente, string baseDeConhecimento);
    Task<string> ResponderChatAsync(string mensagemUsuario, string baseDeConhecimento); // <- Nova linha
}