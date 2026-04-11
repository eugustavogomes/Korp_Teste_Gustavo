import { Component, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TooltipModule } from 'primeng/tooltip';
import { DynamicDialogModule, DialogService } from 'primeng/dynamicdialog';
import { ProdutoService } from '../../../services/produto.service';
import { Produto } from '../../../models/produto.model';
import { FormProduto } from '../form-produto/form-produto';

@Component({
  selector: 'app-lista-produtos',
  imports: [AsyncPipe, TableModule, ButtonModule, CardModule, TooltipModule, DynamicDialogModule],
  templateUrl: './lista-produtos.html',
  styleUrl: './lista-produtos.scss',
})
export class ListaProdutos {
  private produtoService = inject(ProdutoService);
  private dialogService = inject(DialogService);

  readonly produtos$ = this.produtoService.produtos$;
  readonly carregando$ = this.produtoService.carregando$;
  readonly erroCarregamento$ = this.produtoService.erro$;
  erro: string | null = null;

  abrirNovo(): void {
    this.erro = null;
    const ref = this.dialogService.open(FormProduto, {
      header: 'Novo Produto',
      width: '480px',
      modal: true,
    });
    ref!.onClose.subscribe((salvo: boolean) => {
      if (salvo) this.produtoService.carregar();
    });
  }

  abrirEditar(produto: Produto): void {
    this.erro = null;
    const ref = this.dialogService.open(FormProduto, {
      header: 'Editar Produto',
      width: '480px',
      modal: true,
      data: { id: produto.id },
    });
    ref!.onClose.subscribe((salvo: boolean) => {
      if (salvo) this.produtoService.carregar();
    });
  }

  recarregar(): void {
    this.produtoService.carregar();
  }

  excluir(id: number): void {
    if (!confirm('Deseja excluir este produto?')) return;
    this.erro = null;
    this.produtoService.excluirProduto(id).subscribe({
      error: () => (this.erro = 'Erro ao excluir produto'),
    });
  }
}
