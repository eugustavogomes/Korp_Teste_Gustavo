import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { timeout, tap } from 'rxjs/operators';
import { NotaFiscal } from '../models/nota-fiscal.model';

const REQUEST_TIMEOUT_MS = 8000;

@Injectable({ providedIn: 'root' })
export class NotaFiscalService {
  private apiUrl = 'http://localhost:5002/api/notas-fiscais';

  private _notas = new BehaviorSubject<NotaFiscal[]>([]);
  readonly notas$ = this._notas.asObservable();

  private _carregando = new BehaviorSubject<boolean>(false);
  readonly carregando$ = this._carregando.asObservable();

  private _erro = new BehaviorSubject<string | null>(null);
  readonly erro$ = this._erro.asObservable();

  constructor(private http: HttpClient) {}

  carregar(): void {
    this._carregando.next(true);
    this._erro.next(null);
    this.http.get<NotaFiscal[]>(this.apiUrl).pipe(timeout(REQUEST_TIMEOUT_MS)).subscribe({
      next: n => { this._notas.next(n); this._carregando.next(false); },
      error: () => { this._erro.next('Não foi possível carregar as notas fiscais.'); this._carregando.next(false); },
    });
  }

  buscarNota(id: number): Observable<NotaFiscal> {
    return this.http.get<NotaFiscal>(`${this.apiUrl}/${id}`).pipe(timeout(REQUEST_TIMEOUT_MS));
  }

  criarNota(request: { itens: { produtoId: number; descricao: string; quantidade: number; precoUnitario: number }[] }): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.apiUrl, request).pipe(
      timeout(REQUEST_TIMEOUT_MS),
      tap(() => this.carregar()),
    );
  }

  imprimirNota(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/imprimir`, {}).pipe(
      timeout(REQUEST_TIMEOUT_MS),
      tap(() => this.carregar()),
    );
  }

  cancelarNota(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/cancelar`, {}).pipe(
      timeout(REQUEST_TIMEOUT_MS),
      tap(() => this.carregar()),
    );
  }
}
