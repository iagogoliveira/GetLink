namespace urlShortener.Repositories
{
    /// <summary>
    /// Totais por URL, agregados pelo banco. Existe para a listagem nao precisar
    /// trazer uma linha de clique por acesso so para conta-las.
    /// </summary>
    public class ClickAggregate
    {
        public Guid AddressId { get; set; }
        public int Total { get; set; }
        public DateTime? UltimoClique { get; set; }
    }
}
