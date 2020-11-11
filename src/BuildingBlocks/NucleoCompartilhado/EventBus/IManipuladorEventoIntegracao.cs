using System.Threading.Tasks;

namespace NucleoCompartilhado.EventBus
{
    public interface IManipuladorEventoIntegracao<in TEventoIntegracao>
          where TEventoIntegracao : EventoIntegracao
    {
        Task ManipularAsync(TEventoIntegracao mensagem);
    }
}