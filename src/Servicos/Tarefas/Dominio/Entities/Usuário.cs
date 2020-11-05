using NucleoCompartilhado.Dominio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tarefas.Dominio.Entities
{
    public sealed class Usuário : Entity
    {
        private readonly List<Tarefa> _tarefas = new List<Tarefa>();

        private Usuário() { }

        public Usuário(Guid id, string nome)
            => (Id, Nome) = (id, nome);

        public string Nome { get; private set; }

        public bool Ativo { get; private set; } = true;

        public IReadOnlyCollection<Tarefa> Tarefas => _tarefas;

        public void Atualizar(string nome, bool ativo)
            => (Nome, Ativo) = (nome, ativo);

        public void AdicionarTarefa(string descrição)
            => _tarefas.Add(new Tarefa(descrição));

        public bool RemoverTarefa(Guid id)
        {
            if (_tarefas.FirstOrDefault(t => t.Id == id) is { } tarefa)
                return _tarefas.Remove(tarefa);

            return false;
        }
    }
}