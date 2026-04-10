export enum StatusNotaFiscal {
  Aberta = 1,
  Fechada = 2,
}

export interface ItemNotaFiscal {
  id: number;
  produtoId: number;
  descricao: string;
  quantidade: number;
  precoUnitario: number;
  subtotal: number;
}

export interface NotaFiscal {
  id: number;
  numero: string;
  status: StatusNotaFiscal;
  dataEmissao: string;
  total: number;
  itens: ItemNotaFiscal[];
}
