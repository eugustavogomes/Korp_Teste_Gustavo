import { ChangeDetectionStrategy, Component, ViewChild, inject } from '@angular/core';
import { TableModule } from 'primeng/table';
import { Table } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TooltipModule } from 'primeng/tooltip';
import { DynamicDialogModule, DialogService } from 'primeng/dynamicdialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { ProdutoService } from '../../../services/produto.service';
import { Produto } from '../../../models/produto.model';
import { FormProduto } from '../form-produto/form-produto.component';

@Component({
  selector: 'app-lista-produtos',
  imports: [TableModule, ButtonModule, CardModule, TooltipModule, DynamicDialogModule, ConfirmDialogModule],
  providers: [ConfirmationService],
  templateUrl: './lista-produtos.component.html',
  styleUrl: './lista-produtos.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListaProdutos {
  @ViewChild('dt') table!: Table;

  private produtoService      = inject(ProdutoService);
  private dialogService       = inject(DialogService);
  private confirmationService = inject(ConfirmationService);

  readonly produtos          = this.produtoService.produtos;
  readonly carregando        = this.produtoService.carregando;
  readonly erroCarregamento  = this.produtoService.erro;
  erro: string | null = null;

  private _lastSort: { field: string; order: number } | null = null;

  onSort(event: { field: string; order: number }): void {
    if (this._lastSort?.field === event.field && this._lastSort.order === -1 && event.order === 1) {
      this._lastSort = null;
      setTimeout(() => this.table.reset());
    } else {
      this._lastSort = { field: event.field, order: event.order };
    }
  }

  abrirNovo(): void {
    this.erro = null;
    const ref = this.dialogService.open(FormProduto, {
      header: 'Novo Produto',
      width: '560px',
      modal: true,
      closable: true,
      dismissableMask: true,
    });
    ref!.onClose.subscribe((salvo: boolean) => {
      if (salvo) this.produtoService.carregar();
    });
  }

  abrirEditar(produto: Produto): void {
    this.erro = null;
    const ref = this.dialogService.open(FormProduto, {
      header: 'Editar Produto',
      width: '560px',
      modal: true,
      closable: true,
      dismissableMask: true,
      data: { id: produto.id },
    });
    ref!.onClose.subscribe((salvo: boolean) => {
      if (salvo) this.produtoService.carregar();
    });
  }

  excluir(id: number): void {
    this.confirmationService.confirm({
      message: 'Tem certeza que deseja excluir este produto?',
      header: 'Confirmar exclusão',
      acceptLabel: 'Excluir',
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-outlined p-button-secondary',
      accept: () => {
        this.erro = null;
        this.produtoService.excluirProduto(id).subscribe({
          error: () => (this.erro = 'Erro ao excluir produto'),
        });
      },
    });
  }
}
