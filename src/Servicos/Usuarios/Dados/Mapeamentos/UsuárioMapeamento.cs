using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Usuarios.Dominio.Entities;

namespace Usuarios.Dados.Mapeamentos
{
    internal sealed class UsuárioMapeamento : IEntityTypeConfiguration<Usuário>
    {
        public void Configure(EntityTypeBuilder<Usuário> builder)
        {
            builder.ToTable("Usuarios");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Nome).IsRequired().HasMaxLength(128);
            builder.Property(p => p.Senha).IsRequired().HasMaxLength(64);
            builder.Property(p => p.Ativo).IsRequired().HasDefaultValue(true);
        }
    }
}
