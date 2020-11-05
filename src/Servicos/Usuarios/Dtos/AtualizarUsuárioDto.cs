namespace Usuarios.Dtos
{
    public sealed class AtualizarUsuárioDto
    {
        public AtualizarUsuárioDto(string nome, bool ativo)
        {
            Nome = nome;
            Ativo = ativo;
        }

        public string Nome { get; }
        public bool Ativo { get; }
    }
}
