using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tarefas.Dominio.Entities;

namespace Tarefas.Dados.Mapeamentos
{
    internal sealed class TarefaMapeamento : IEntityTypeConfiguration<Tarefa>
    {
        public void Configure(EntityTypeBuilder<Tarefa> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Descrição).IsRequired().HasMaxLength(256);
        }
    }
}