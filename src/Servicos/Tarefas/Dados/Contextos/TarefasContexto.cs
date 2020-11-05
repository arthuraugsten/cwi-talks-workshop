using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using NucleoCompartilhado.Extensions;
using NucleoCompartilhado.Generators;
using Tarefas.Dominio.Entities;

namespace Tarefas.Dados.Contextos
{
    public sealed class TarefasContexto : DbContext
    {
        public TarefasContexto(DbContextOptions<TarefasContexto> opcoes)
            : base(opcoes) { }

        public DbSet<Tarefa> Tarefas { get; set; }
        public DbSet<Usuário> Usuários { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, CustomSqlServerMigrationsSqlGenerator>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TarefasContexto).Assembly);
            modelBuilder.ConfigurarStrings();
        }
    }
}