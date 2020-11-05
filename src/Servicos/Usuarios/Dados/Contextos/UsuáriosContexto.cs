using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using NucleoCompartilhado.Extensions;
using NucleoCompartilhado.Generators;
using Usuarios.Dominio.Entities;

namespace Usuarios.Dados.Contextos
{
    public sealed class UsuáriosContexto : DbContext
    {
        public UsuáriosContexto(DbContextOptions<UsuáriosContexto> opcoes)
          : base(opcoes) { }

        public DbSet<Usuário> Usuários { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, CustomSqlServerMigrationsSqlGenerator>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsuáriosContexto).Assembly);
            modelBuilder.ConfigurarStrings();
        }
    }
}