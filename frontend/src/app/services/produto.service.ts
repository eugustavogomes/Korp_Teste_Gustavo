import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { timeout, tap } from 'rxjs/operators';
import { Produto } from '../models/produto.model';
import { API_ENDPOINTS } from '../core/api-endpoints';

const REQUEST_TIMEOUT_MS = 8000;

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private readonly api = API_ENDPOINTS.produtos;

  readonly produtos  = signal<Produto[]>([]);
  readonly carregando = signal(false);
  readonly erro       = signal<string | null>(null);

  constructor(private http: HttpClient) {}

  carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);
    this.http.get<Produto[]>(this.api.base).pipe(timeout(REQUEST_TIMEOUT_MS)).subscribe({
      next: p => { this.produtos.set(p); this.carregando.set(false); },
      error: () => { this.erro.set('Não foi possível carregar os produtos.'); this.carregando.set(false); },
    });
  }

  buscarProduto(id: number): Observable<Produto> {
    return this.http.get<Produto>(this.api.byId(id)).pipe(timeout(REQUEST_TIMEOUT_MS));
  }

  cadastrarProduto(produto: Omit<Produto, 'id'>): Observable<Produto> {
    return this.http.post<Produto>(this.api.base, produto).pipe(
      timeout(REQUEST_TIMEOUT_MS),
      tap(() => this.carregar()),
    );
  }

  atualizarProduto(produto: Produto): Observable<void> {
    return this.http.put<void>(this.api.byId(produto.id), produto).pipe(
      timeout(REQUEST_TIMEOUT_MS),
      tap(() => this.carregar()),
    );
  }

  excluirProduto(id: number): Observable<void> {
    return this.http.delete<void>(this.api.byId(id)).pipe(
      timeout(REQUEST_TIMEOUT_MS),
      tap(() => this.carregar()),
    );
  }
}
