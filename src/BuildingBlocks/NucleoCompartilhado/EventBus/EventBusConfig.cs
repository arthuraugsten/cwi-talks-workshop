namespace NucleoCompartilhado.EventBus
{
    public sealed class EventBusConfig
    {
        public int QuantidadeTentativas { get; set; }
        public string Host { get; set; }
        public int Porta { get; set; }
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public string NomeInscricao { get; set; }
    }
}