import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { TimeoutError } from 'rxjs';

export interface ApiError {
  mensagem: string;
  tipo: 'servico_indisponivel' | 'negocio' | 'servidor' | 'rede' | 'timeout';
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: unknown) => {
      const apiError = mapearErro(error);
      return throwError(() => apiError);
    })
  );
};

function mapearErro(error: unknown): ApiError {
  if (error instanceof TimeoutError) {
    return {
      mensagem: 'O servidor demorou demais para responder. Verifique se o backend está em execução.',
      tipo: 'timeout',
    };
  }

  if (!(error instanceof HttpErrorResponse)) {
    return { mensagem: 'Ocorreu um erro inesperado.', tipo: 'servidor' };
  }

  if (error.status === 0) {
    return {
      mensagem: 'Não foi possível conectar ao servidor. Verifique sua conexão.',
      tipo: 'rede',
    };
  }

  if (error.status === 503) {
    return {
      mensagem: error.error?.mensagem ?? 'Serviço temporariamente indisponível. Tente novamente em instantes.',
      tipo: 'servico_indisponivel',
    };
  }

  if (error.status === 429) {
    return {
      mensagem: error.error?.mensagem ?? 'Muitas requisições. Aguarde alguns segundos e tente novamente.',
      tipo: 'negocio',
    };
  }

  if (error.status === 400 || error.status === 422) {
    return {
      mensagem: error.error?.mensagem ?? 'Requisição inválida.',
      tipo: 'negocio',
    };
  }

  if (error.status === 409) {
    return {
      mensagem: error.error?.mensagem ?? 'Conflito: a operação não pode ser concluída.',
      tipo: 'negocio',
    };
  }

  if (error.status === 404) {
    return {
      mensagem: error.error?.mensagem ?? 'Recurso não encontrado.',
      tipo: 'negocio',
    };
  }

  return {
    mensagem: error.error?.mensagem ?? 'Ocorreu um erro inesperado.',
    tipo: 'servidor',
  };
}
