using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tarefas.Dominio.Entities;

namespace Tarefas.Dados.Mapeamentos
{
    internal sealed class UsuárioMapeamento : IEntityTypeConfiguration<Usuário>
    {
        public void Configure(EntityTypeBuilder<Usuário> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Nome).IsRequired().HasMaxLength(128);
            builder.Property(p => p.Ativo).IsRequired().HasDefaultValue();

            builder.HasMany(p => p.Tarefas).WithOne().HasForeignKey("IdUsuário");
            builder.Metadata.FindNavigation(nameof(Usuário.Tarefas)).SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}