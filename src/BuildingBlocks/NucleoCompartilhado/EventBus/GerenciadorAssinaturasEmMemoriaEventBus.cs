using System;
using System.Collections.Generic;
using System.Linq;

namespace NucleoCompartilhado.EventBus
{
    public sealed class GerenciadorAssinaturasEmMemoriaEventBus : IGerenciadorAssinaturasEventBus
    {
        private readonly Dictionary<string, List<InformacaoInscricao>> _manipuladores;
        private readonly List<Type> _tiposEventos;

        public GerenciadorAssinaturasEmMemoriaEventBus()
        {
            _manipuladores = new Dictionary<string, List<InformacaoInscricao>>();
            _tiposEventos = new List<Type>();
        }

        public bool EstaVazio => !_manipuladores.Keys.Any();

        public event EventHandler<string> AoRemoverEvento;

        public void AdicionarInscricao<T, TH>()
             where T : EventoIntegracao
             where TH : IManipuladorEventoIntegracao<T>
        {
            var nomeEvento = ObterChaveEvento<T>();

            AdicionarInscricaoInterna(typeof(TH), nomeEvento);

            if (!_tiposEventos.Contains(typeof(T)))
                _tiposEventos.Add(typeof(T));
        }

        private void AdicionarInscricaoInterna(Type tipoManipulador, string nomeEvento)
        {
            if (!ExisteInscricaoParaEvento(nomeEvento))
                _manipuladores.Add(nomeEvento, new List<InformacaoInscricao>());

            if (_manipuladores[nomeEvento].Any(s => s.TipoManipulador == tipoManipulador))
            {
                throw new ArgumentException(
                     $"Manipulador do tipo {tipoManipulador.Name} já registrado para '{nomeEvento}'", nameof(tipoManipulador));
            }

            _manipuladores[nomeEvento].Add(InformacaoInscricao.Criar(tipoManipulador));
        }

        public void RemoverInscricao<T, TH>()
             where T : EventoIntegracao
             where TH : IManipuladorEventoIntegracao<T>
        {
            var manipuladorPararemover = BuscarInscricaoParaRemover<T, TH>();
            var nomeEvento = ObterChaveEvento<T>();
            RemoverManipuladorInterno(nomeEvento, manipuladorPararemover);
        }

        private InformacaoInscricao BuscarInscricaoParaRemover<T, TH>()
              where T : EventoIntegracao
              where TH : IManipuladorEventoIntegracao<T>
        {
            var nomeEvento = ObterChaveEvento<T>();

            if (!ExisteInscricaoParaEvento(nomeEvento))
                return null;

            var tipoManipulador = typeof(TH);

            return _manipuladores[nomeEvento].SingleOrDefault(s => s.TipoManipulador == tipoManipulador);
        }

        private void RemoverManipuladorInterno(string nomeEvento, InformacaoInscricao assinaturasParaRemover)
        {
            if (assinaturasParaRemover == null)
                return;

            _manipuladores[nomeEvento].Remove(assinaturasParaRemover);

            if (!_manipuladores[nomeEvento].Any())
            {
                _manipuladores.Remove(nomeEvento);

                var tipoEvento = _tiposEventos.SingleOrDefault(e => e.Name == nomeEvento);

                if (tipoEvento != null)
                    _tiposEventos.Remove(tipoEvento);

                DispararAoRemoverEvento(nomeEvento);
            }
        }

        private void DispararAoRemoverEvento(string nomeEvento)
             => AoRemoverEvento?.Invoke(this, nomeEvento);

        public bool ExisteInscricaoParaEvento<T>() where T : EventoIntegracao
             => ExisteInscricaoParaEvento(ObterChaveEvento<T>());

        public bool ExisteInscricaoParaEvento(string nomeEvento)
             => _manipuladores.ContainsKey(nomeEvento);

        public Type ObterTipoEventoPorNome(string nomeEvento)
             => _tiposEventos.SingleOrDefault(t => t.Name == nomeEvento);

        public void Limpar()
             => _manipuladores.Clear();

        public IEnumerable<InformacaoInscricao> ObterManipuladoresParaEvento<T>() where T : EventoIntegracao
             => ObterManipuladoresParaEvento(ObterChaveEvento<T>());

        public IEnumerable<InformacaoInscricao> ObterManipuladoresParaEvento(string nomeEvento)
             => _manipuladores[nomeEvento];

        public string ObterChaveEvento<T>()
             => typeof(T).Name;
    }
}