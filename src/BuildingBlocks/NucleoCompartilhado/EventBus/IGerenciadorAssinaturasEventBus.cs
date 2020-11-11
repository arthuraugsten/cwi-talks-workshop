using System;
using System.Collections.Generic;

namespace NucleoCompartilhado.EventBus
{
    public interface IGerenciadorAssinaturasEventBus
    {
        bool EstaVazio { get; }
        event EventHandler<string> AoRemoverEvento;

        void AdicionarInscricao<T, TH>()
            where T : EventoIntegracao
            where TH : IManipuladorEventoIntegracao<T>;

        void RemoverInscricao<T, TH>()
              where TH : IManipuladorEventoIntegracao<T>
              where T : EventoIntegracao;

        bool ExisteInscricaoParaEvento<T>() where T : EventoIntegracao;
        bool ExisteInscricaoParaEvento(string nomeEvento);
        Type ObterTipoEventoPorNome(string nomeEvento);
        void Limpar();
        IEnumerable<InformacaoInscricao> ObterManipuladoresParaEvento<T>() where T : EventoIntegracao;
        IEnumerable<InformacaoInscricao> ObterManipuladoresParaEvento(string nomeEvento);
        string ObterChaveEvento<T>();
    }
}