import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NotaFiscal } from '../models/nota-fiscal.model';

@Injectable({ providedIn: 'root' })
export class NotaFiscalService {
  private apiUrl = 'http://localhost:5002/api/notas-fiscais';

  constructor(private http: HttpClient) {}

  listarNotas(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[]>(this.apiUrl);
  }

  buscarNota(id: number): Observable<NotaFiscal> {
    return this.http.get<NotaFiscal>(`${this.apiUrl}/${id}`);
  }

  criarNota(request: { itens: { produtoId: number; descricao: string; quantidade: number; precoUnitario: number }[] }): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.apiUrl, request);
  }

  imprimirNota(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/imprimir`, {});
  }

  cancelarNota(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/cancelar`, {});
  }
}
