using NucleoCompartilhado.EventBus;
using System;

namespace Tarefas.Eventos
{
    internal sealed class UsuarioCriadoEventoIntegracao : EventoIntegracao
    {
        public UsuarioCriadoEventoIntegracao(Guid id, string nome)
        {
            Id = id;
            Nome = nome;
        }

        public Guid Id { get; }
        public string Nome { get; }
    }
}