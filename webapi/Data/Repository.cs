using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using webapi.Data.Interfaces;
using webapi.Models;

namespace webapi.Data
{
    public class Repository<TEntidade> : IRepository<TEntidade> where TEntidade : Entidade
    {

        protected readonly DataContext _context;
        protected readonly DbSet<TEntidade> _dataset;

        public Repository(DataContext context)
        {
            _context = context;
            _dataset = context.Set<TEntidade>();
        }

        public ValueTask<TEntidade> BuscarPorId(int id)
        {
            return _dataset.FindAsync(id);
        }


        public Task<int> BuscarCount(Expression<System.Func<TEntidade, bool>> filtro)
        {
            return _dataset.CountAsync(filtro);
        }

        public Task<List<TEntidade>> BuscarTodos(Expression<System.Func<TEntidade, bool>> filtro, int paginaAtual, int itensPorPagina)
        {
            int take = itensPorPagina;
            int skip = (paginaAtual - 1) * take;

            return _dataset
                .AsNoTracking()
                .Where(filtro)
                .Skip(skip)
                .Take(take)
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public Task<bool> RegistroExiste(int id)
        {
            return _dataset.AnyAsync(i => i.Id == id);
        }        

        public async Task<bool> Excluir(int id)
        {
            var entidade = await BuscarPorId(id);

            if (entidade == null)
            {   
                return false;
            }

            _dataset.Remove(entidade);

            var sucesso = await _context.SaveChangesAsync();

            return sucesso > 0;
        }

        public async Task<bool> Salvar(TEntidade entidade)
        {
            if (entidade == null)
            {
                return false;
            }

            await _dataset.AddAsync(entidade);

            var sucesso = await _context.SaveChangesAsync();

            return sucesso > 0;
        }
        public async Task<bool> Atualizar(TEntidade entidade)
        {
            if (entidade == null)
            {
                return false;
            }

            var entidadeDB = await BuscarPorId(entidade.Id);

            if (entidadeDB == null)
            {
                return false;
            }

            _context.Entry(entidadeDB).CurrentValues.SetValues(entidade);

            var sucesso = await _context.SaveChangesAsync();

            return sucesso > 0;
        }
    }
}

