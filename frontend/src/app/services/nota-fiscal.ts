import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { timeout, tap } from 'rxjs/operators';
import { NotaFiscal } from '../models/nota-fiscal.model';
import { API_ENDPOINTS } from '../core/api-endpoints';

const REQUEST_TIMEOUT_MS = 8000;

@Injectable({ providedIn: 'root' })
export class NotaFiscalService {
  private readonly api = API_ENDPOINTS.notasFiscais;

  readonly notas      = signal<NotaFiscal[]>([]);
  readonly carregando = signal(false);
  readonly erro       = signal<string | null>(null);

  constructor(private http: HttpClient) {}

  carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);
    this.http.get<NotaFiscal[]>(this.api.base).pipe(timeout(REQUEST_TIMEOUT_MS)).subscribe({
      next: n => { this.notas.set(n); this.carregando.set(false); },
      error: () => { this.erro.set('Não foi possível carregar as notas fiscais.'); this.carregando.set(false); },
    });
  }

  buscarNota(id: number): Observable<NotaFiscal> {
    return this.http.get<NotaFiscal>(this.api.byId(id)).pipe(timeout(REQUEST_TIMEOUT_MS));
  }

  criarNota(request: { itens: { produtoId: number; descricao: string; quantidade: number; precoUnitario: number }[] }): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.api.base, request).pipe(
      timeout(REQUEST_TIMEOUT_MS),
      tap(() => this.carregar()),
    );
  }

  imprimirNota(id: number): Observable<void> {
    return this.http.post<void>(this.api.imprimir(id), {}).pipe(
      timeout(REQUEST_TIMEOUT_MS),
      tap(() => this.carregar()),
    );
  }

  cancelarNota(id: number): Observable<void> {
    return this.http.put<void>(this.api.cancelar(id), {}).pipe(
      timeout(REQUEST_TIMEOUT_MS),
      tap(() => this.carregar()),
    );
  }
}
