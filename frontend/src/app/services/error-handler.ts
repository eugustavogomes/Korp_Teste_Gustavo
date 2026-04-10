import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

export interface ApiError {
  mensagem: string;
  tipo: 'servico_indisponivel' | 'negocio' | 'servidor' | 'rede';
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const apiError = mapearErro(error);
      return throwError(() => apiError);
    })
  );
};

function mapearErro(error: HttpErrorResponse): ApiError {
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

  if (error.status === 400) {
    return {
      mensagem: error.error?.mensagem ?? 'Requisição inválida.',
      tipo: 'negocio',
    };
  }

  return {
    mensagem: error.error?.mensagem ?? 'Ocorreu um erro inesperado.',
    tipo: 'servidor',
  };
}
