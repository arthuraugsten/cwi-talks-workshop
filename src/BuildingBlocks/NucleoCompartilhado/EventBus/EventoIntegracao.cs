using Newtonsoft.Json;
using System;

namespace NucleoCompartilhado.EventBus
{
    public abstract class EventoIntegracao
    {
        public EventoIntegracao()
        {
            EventoId = Guid.NewGuid();
            DataCriacao = DateTime.UtcNow;
        }

        [JsonConstructor]
        public EventoIntegracao(Guid eventoId, DateTime createDate)
        {
            EventoId = eventoId;
            DataCriacao = createDate;
        }

        [JsonProperty]
        public Guid EventoId { get; private set; }

        [JsonProperty]
        public DateTime DataCriacao { get; private set; }
    }
}