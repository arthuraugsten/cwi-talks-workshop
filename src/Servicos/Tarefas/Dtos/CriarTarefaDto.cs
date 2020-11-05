namespace Tarefas.Dtos
{
    public sealed class CriarTarefaDto
    {
        public CriarTarefaDto(string descrição) => Descrição = descrição;

        public string Descrição { get; }
    }
}
