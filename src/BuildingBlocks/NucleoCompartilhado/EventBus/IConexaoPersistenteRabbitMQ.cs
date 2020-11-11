using RabbitMQ.Client;
using System;

namespace NucleoCompartilhado.EventBus
{
    public interface IConexaoPersistenteRabbitMQ : IDisposable
    {
        bool EstaConectado { get; }

        bool TentarConectar();

        IModel CriarModelo();
    }
}