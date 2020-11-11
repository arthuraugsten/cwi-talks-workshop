using System;

namespace NucleoCompartilhado.EventBus
{
    public sealed class InformacaoInscricao
    {
        public Type TipoManipulador { get; }

        private InformacaoInscricao(Type tipoManipulador)
             => TipoManipulador = tipoManipulador;

        public static InformacaoInscricao Criar(Type tipoManipulador)
             => new InformacaoInscricao(tipoManipulador);
    }
}