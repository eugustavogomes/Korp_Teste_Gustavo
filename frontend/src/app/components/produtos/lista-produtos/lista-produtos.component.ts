import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UpperCasePipe } from '@angular/common';
import { TableModule } from 'primeng/table';
import { Table } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { DynamicDialogModule, DialogService } from 'primeng/dynamicdialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ProdutoService } from '../../../services/produto.service';
import { NotaFiscalService } from '../../../services/nota-fiscal.service';
import { Produto } from '../../../models/produto.model';
import { FormProduto } from '../form-produto/form-produto.component';

@Component({
  selector: 'app-lista-produtos',
  imports: [TableModule, ButtonModule, CardModule, TooltipModule, DialogModule, DynamicDialogModule, ConfirmDialogModule, UpperCasePipe],
  providers: [ConfirmationService],
  templateUrl: './lista-produtos.component.html',
  styleUrl: './lista-produtos.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListaProdutos implements OnInit {
  @ViewChild('dt') table!: Table;

  private produtoService      = inject(ProdutoService);
  private notaService         = inject(NotaFiscalService);
  private dialogService       = inject(DialogService);
  private confirmationService = inject(ConfirmationService);
  private messageService      = inject(MessageService);
  private destroyRef          = inject(DestroyRef);

  readonly produtos          = this.produtoService.produtos;
  readonly carregando        = this.produtoService.carregando;
  readonly erroCarregamento  = this.produtoService.erro;
  erro: string | null = null;
  mostrarRanking = false;

  readonly busca = signal('');

  readonly produtosFiltrados = computed(() => {
    const todos = this.produtos();
    const q = this.busca().toLowerCase().trim();
    if (!q) return todos;
    return todos.filter(p =>
      p.descricao.toLowerCase().includes(q) ||
      p.codigo.toLowerCase().includes(q)
    );
  });

  get kpiTotal()      { return this.produtos().length; }
  get kpiSemEstoque() { return this.produtos().filter(p => p.saldoDisponivel === 0).length; }
  get kpiCritico()    { return this.produtos().filter(p => p.saldoDisponivel > 0 && p.saldoDisponivel < 5).length; }

  saldoClass(saldo: number): string {
    if (saldo === 0)  return 'saldo-zero';
    if (saldo < 5)    return 'saldo-critical';
    if (saldo < 10)   return 'saldo-low';
    return 'saldo-ok';
  }

  rowClass(saldo: number): string {
    if (saldo < 5)  return 'tr-stock-critical';
    if (saldo < 10) return 'tr-stock-low';
    return '';
  }

  private get _rankingCompleto(): { descricao: string; quantidade: number }[] {
    const map = new Map<number, { descricao: string; quantidade: number }>();
    for (const nota of this.notaService.notas()) {
      for (const item of nota.itens) {
        const cur = map.get(item.produtoId) ?? { descricao: item.descricao, quantidade: 0 };
        map.set(item.produtoId, { descricao: item.descricao, quantidade: cur.quantidade + item.quantidade });
      }
    }
    return [...map.values()].sort((a, b) => b.quantidade - a.quantidade);
  }

  get kpiTopProdutos()     { return this._rankingCompleto.slice(0, 3); }
  get kpiRankingCompleto() { return this._rankingCompleto; }

  private _lastSort: { field: string; order: number } | null = null;

  ngOnInit(): void {
    if (this.notaService.notas().length === 0) this.notaService.carregar();
  }

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
    ref!.onClose.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((salvo: boolean) => {
      if (salvo) {
        this.produtoService.carregar();
        this.messageService.add({ severity: 'success', summary: 'Produto adicionado', detail: 'Produto cadastrado com sucesso.' });
      }
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
    ref!.onClose.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((salvo: boolean) => {
      if (salvo) {
        this.produtoService.carregar();
        this.messageService.add({ severity: 'success', summary: 'Produto atualizado', detail: 'Alterações salvas com sucesso.' });
      }
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
        this.produtoService.excluirProduto(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
          next: () => this.messageService.add({ severity: 'warn', summary: 'Produto excluído', detail: 'Produto removido do catálogo.' }),
          error: () => (this.erro = 'Erro ao excluir produto'),
        });
      },
    });
  }
}
