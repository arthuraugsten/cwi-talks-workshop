using NucleoCompartilhado.Dominio;

namespace Tarefas.Dominio.Entities
{
    public sealed class Tarefa : Entity
    {
        private Tarefa() { }

        public Tarefa(string descrição)
            => Descrição = descrição;

        public string Descrição { get; private set; }
    }
}