namespace EstoqueService.Exceptions;

public class ProdutoComReservaAtivaException : Exception
{
    public ProdutoComReservaAtivaException(string descricao, int saldoReservado)
        : base($"Produto '{descricao}' possui {saldoReservado} unidade(s) reservada(s) em notas fiscais em aberto e não pode ser excluído.")
    {
    }
}
