import { environment } from '../../environments/environment';

const estoque = environment.estoqueServiceUrl;
const faturamento = environment.faturamentoServiceUrl;

export const HEALTH_ENDPOINTS = {
  estoque:     `${estoque}/health`,
  faturamento: `${faturamento}/health`,
} as const;

export const API_ENDPOINTS = {
  produtos: {
    base:         `${estoque}/api/produtos`,
    byId:         (id: number) => `${estoque}/api/produtos/${id}`,
    baixaEstoque: `${estoque}/api/produtos/baixa-estoque`,
  },
  notasFiscais: {
    base:     `${faturamento}/api/notas-fiscais`,
    byId:     (id: number) => `${faturamento}/api/notas-fiscais/${id}`,
    imprimir: (id: number) => `${faturamento}/api/notas-fiscais/${id}/imprimir`,
    cancelar: (id: number) => `${faturamento}/api/notas-fiscais/${id}/cancelar`,
  },
  insights: {
    interpretarPedido: `${faturamento}/api/insights/interpretar-pedido`,
  },
} as const;
