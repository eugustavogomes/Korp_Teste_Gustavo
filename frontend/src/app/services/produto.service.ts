import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { timeout, tap } from 'rxjs/operators';
import { Produto } from '../models/produto.model';

const REQUEST_TIMEOUT_MS = 8000;

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private apiUrl = 'http://localhost:5189/api/produtos';

  private _produtos = new BehaviorSubject<Produto[]>([]);
  readonly produtos$ = this._produtos.asObservable();

  private _carregando = new BehaviorSubject<boolean>(false);
  readonly carregando$ = this._carregando.asObservable();

  private _erro = new BehaviorSubject<string | null>(null);
  readonly erro$ = this._erro.asObservable();

  constructor(private http: HttpClient) {}

  carregar(): void {
    this._carregando.next(true);
    this._erro.next(null);
    this.http.get<Produto[]>(this.apiUrl).pipe(timeout(REQUEST_TIMEOUT_MS)).subscribe({
      next: p => { this._produtos.next(p); this._carregando.next(false); },
      error: () => { this._erro.next('Não foi possível carregar os produtos.'); this._carregando.next(false); },
    });
  }

  buscarProduto(id: number): Observable<Produto> {
    return this.http.get<Produto>(`${this.apiUrl}/${id}`).pipe(timeout(REQUEST_TIMEOUT_MS));
  }

  cadastrarProduto(produto: Omit<Produto, 'id'>): Observable<Produto> {
    return this.http.post<Produto>(this.apiUrl, produto).pipe(
      timeout(REQUEST_TIMEOUT_MS),
      tap(() => this.carregar()),
    );
  }

  atualizarProduto(produto: Produto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${produto.id}`, produto).pipe(
      timeout(REQUEST_TIMEOUT_MS),
      tap(() => this.carregar()),
    );
  }

  excluirProduto(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      timeout(REQUEST_TIMEOUT_MS),
      tap(() => this.carregar()),
    );
  }
}
