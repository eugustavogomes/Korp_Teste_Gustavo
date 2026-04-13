namespace FaturamentoService.Exceptions;

public class GeminiRateLimitException()
    : Exception("Limite de requisições da IA atingido. Aguarde alguns segundos e tente novamente.");
