namespace NucleoCompartilhado.EventBus
{
    public interface IEventBus
    {
        void Publicar(EventoIntegracao message);

        void AdicionarInscricao<T, TH>()
             where T : EventoIntegracao
             where TH : IManipuladorEventoIntegracao<T>;

        void RemoverInscricao<T, TH>()
             where T : EventoIntegracao
             where TH : IManipuladorEventoIntegracao<T>;
    }
}