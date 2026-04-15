using SQLite;
using BiblioLinx.Models;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Maui.Storage;
using System.Threading;

namespace BiblioLinx.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection _db;
    
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private async Task InitAsync()
    {
        if (_db != null) return;
        
        await _semaphore.WaitAsync();
        try
        {
            if (_db != null) return;
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "BiblioLinx.db3");
            _db = new SQLiteAsyncConnection(databasePath);
            
            await _db.CreateTableAsync<KnowledgeGroup>();
            await _db.CreateTableAsync<KnowledgeItem>();
            await _db.CreateTableAsync<SupportCase>();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<List<KnowledgeGroup>> GetBaseConhecimentoAsync()
    {
        await InitAsync();
        var grupos = await _db.Table<KnowledgeGroup>().ToListAsync();
        
        foreach (var grupo in grupos)
        {
            var paginas = await _db.Table<KnowledgeItem>().Where(p => p.GroupId == grupo.Id).ToListAsync();
            grupo.Items = new System.Collections.ObjectModel.ObservableCollection<KnowledgeItem>(paginas);
        }
        return grupos;
    }

    public async Task<List<SupportCase>> GetCasosAsync()
    {
        await InitAsync();
        return await _db.Table<SupportCase>().ToListAsync();
    }

    public async Task SalvarGrupoAsync(KnowledgeGroup grupo)
    {
        await InitAsync();
        if (grupo.Id != 0) await _db.UpdateAsync(grupo); else await _db.InsertAsync(grupo);
    }

    public async Task SalvarPaginaAsync(KnowledgeItem pagina)
    {
        await InitAsync();
        pagina.LastModified = DateTime.Now;
        if (pagina.Id != 0) await _db.UpdateAsync(pagina); else await _db.InsertAsync(pagina);
    }

    public async Task ExcluirPaginaAsync(KnowledgeItem pagina)
    {
        await InitAsync();
        await _db.DeleteAsync(pagina);
    }

    public async Task ExcluirGrupoAsync(KnowledgeGroup grupo)
    {
        await InitAsync();
        var paginas = await _db.Table<KnowledgeItem>().Where(p => p.GroupId == grupo.Id).ToListAsync();
        foreach (var pagina in paginas) await _db.DeleteAsync(pagina);
        await _db.DeleteAsync(grupo);
    }

    public async Task SalvarCasoAsync(SupportCase caso)
    {
        await InitAsync();
        caso.CreatedAt = DateTime.Now;
        if (caso.Id != 0) await _db.UpdateAsync(caso); else await _db.InsertAsync(caso);
    }

    public async Task ExcluirCasoAsync(SupportCase caso)
    {
        await InitAsync();
        await _db.DeleteAsync(caso);
    }
}
