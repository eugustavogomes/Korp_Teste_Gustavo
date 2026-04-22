using FaturamentoService.DTOs;

namespace FaturamentoService.Services.Interfaces;

public interface IGeminiService
{
    Task<InterpretOrderResponse> InterpretOrderAsync(InterpretOrderRequest request);
}
