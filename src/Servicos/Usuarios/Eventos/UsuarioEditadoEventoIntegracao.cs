using NucleoCompartilhado.EventBus;
using System;

namespace Usuarios.Eventos
{
    internal sealed class UsuarioEditadoEventoIntegracao : EventoIntegracao
    {
        public UsuarioEditadoEventoIntegracao(Guid id, string nome, bool ativo)
        {
            Id = id;
            Nome = nome;
            Ativo = ativo;
        }

        public Guid Id { get; }
        public string Nome { get; }
        public bool Ativo { get; }
    }
}