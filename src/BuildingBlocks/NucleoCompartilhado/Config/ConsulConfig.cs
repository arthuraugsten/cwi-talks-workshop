namespace NucleoCompartilhado.Config
{
    public sealed class ConsulConfig
    {
        public string NomeServico { get; set; }
        public string UrlConsul { get; set; }
        public string AclToken { get; set; }

        internal void Deconstruct(out string urlConsul, out string aclToken)
        {
            urlConsul = UrlConsul;
            aclToken = AclToken;
        }
    }
}
