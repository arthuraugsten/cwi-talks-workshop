using Microsoft.Extensions.Logging;
using NucleoCompartilhado.EventBus;
using System.Threading.Tasks;
using Tarefas.Dados.Contextos;

namespace Tarefas.Eventos
{
    internal sealed class UsuarioEditadoManipuladorEventoIntegracao : IManipuladorEventoIntegracao<UsuarioEditadoEventoIntegracao>
    {
        private readonly TarefasContexto _contexto;
        private readonly ILogger<UsuarioEditadoManipuladorEventoIntegracao> _logger;

        public UsuarioEditadoManipuladorEventoIntegracao(TarefasContexto contexto, ILogger<UsuarioEditadoManipuladorEventoIntegracao> logger)
            => (_contexto, _logger) = (contexto, logger);

        public async Task ManipularAsync(UsuarioEditadoEventoIntegracao mensagem)
        {
            var entidade = await _contexto.Usuários.FindAsync(mensagem.Id);

            if (entidade is null)
            {
                _logger.LogWarning($"Usuário não encontrado para atualização: {mensagem.Id}");
                return;
            }

            entidade.Atualizar(mensagem.Nome, mensagem.Ativo);

            await _contexto.SaveChangesAsync();
        }
    }
}