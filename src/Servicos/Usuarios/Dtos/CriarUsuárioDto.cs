namespace Usuarios.Dtos
{
    public sealed class CriarUsuárioDto
    {
        public CriarUsuárioDto(string nome, string senha)
        {
            Nome = nome;
            Senha = senha;
        }

        public string Nome { get; }
        public string Senha { get; }
    }
}
