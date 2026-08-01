namespace urlShortener.Models
{
    /// <summary>
    /// Um acesso a uma URL curta. Guarda apenas metadados derivados: o IP e o
    /// user-agent originais sao usados para preencher estes campos e descartados,
    /// pois sao dados pessoais sob a LGPD.
    /// </summary>
    public class Click
    {
        public Guid Id { get; set; }

        public Guid AddressId { get; set; }

        public DateTime ClickedAt { get; set; }

        /// <summary>
        /// Apenas o host de origem ("google.com"), nunca a URL completa: caminhos
        /// de origem costumam carregar termos de busca e identificadores.
        /// </summary>
        public string? RefererHost { get; set; }

        public string DeviceType { get; set; } = string.Empty;

        public string Browser { get; set; } = string.Empty;

        public string OperatingSystem { get; set; } = string.Empty;

        public Address? Address { get; set; }
    }
}
