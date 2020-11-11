using NucleoCompartilhado.EventBus;
using System.Threading.Tasks;
using Tarefas.Dados.Contextos;
using Tarefas.Dominio.Entities;

namespace Tarefas.Eventos
{
    internal sealed class UsuarioCriadoManipuladorEventoIntegracao : IManipuladorEventoIntegracao<UsuarioCriadoEventoIntegracao>
    {
        private readonly TarefasContexto _contexto;

        public UsuarioCriadoManipuladorEventoIntegracao(TarefasContexto contexto)
            => _contexto = contexto;

        public async Task ManipularAsync(UsuarioCriadoEventoIntegracao mensagem)
        {
            await _contexto.Usuários.AddAsync(new Usuário(mensagem.Id, mensagem.Nome));
            await _contexto.SaveChangesAsync();
        }
    }
}