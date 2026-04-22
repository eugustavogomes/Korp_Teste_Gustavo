namespace FaturamentoService.Exceptions;

public class GeminiRateLimitException()
    : Exception("AI request limit reached. Please wait a few seconds and try again.");
