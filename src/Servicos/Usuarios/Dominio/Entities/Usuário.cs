using NucleoCompartilhado.Dominio;

namespace Usuarios.Dominio.Entities
{
    public sealed class Usuário : Entity
    {
        private Usuário() { }

        public Usuário(string nome, string senha)
            => (Nome, Senha) = (nome, senha);

        public Usuário(string nome, string senha, bool ativo)
            : this(nome, senha)
            => Ativo = ativo;

        public string Nome { get; private set; }

        public string Senha { get; private set; }

        public bool Ativo { get; private set; } = true;

        public void Atualizar(string nome, bool ativo)
            => (Nome, Ativo) = (nome, ativo);

        public void Inativar() => Ativo = false;
    }
}