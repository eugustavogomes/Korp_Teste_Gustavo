import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { timeout } from 'rxjs/operators';
import { Produto } from '../models/produto.model';
import { API_ENDPOINTS } from '../core/api-endpoints';

export interface ItemInterpretado {
  produtoId: number;
  codigo: string;
  descricao: string;
  quantidade: number;
  precoUnitario: number;
}

export interface InterpretarPedidoResponse {
  itens: ItemInterpretado[];
  naoEncontrados: string[];
}

@Injectable({ providedIn: 'root' })
export class InsightsService {
  constructor(private http: HttpClient) {}

  interpretarPedido(texto: string, produtos: Produto[]): Observable<InterpretarPedidoResponse> {
    const catalogo = produtos
      .filter(p => p.saldoDisponivel > 0)
      .map(p => ({
        id: p.id,
        codigo: p.codigo,
        descricao: p.descricao,
        saldoDisponivel: p.saldoDisponivel,
      }));

    return this.http
      .post<InterpretarPedidoResponse>(API_ENDPOINTS.insights.interpretarPedido, {
        texto,
        produtos: catalogo,
      })
      .pipe(timeout(20000));
  }
}
