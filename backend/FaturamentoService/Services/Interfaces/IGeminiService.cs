using FaturamentoService.DTOs;

namespace FaturamentoService.Services.Interfaces;

public interface IGeminiService
{
    Task<InterpretarPedidoResponse> InterpretarPedidoAsync(InterpretarPedidoRequest request);
}
