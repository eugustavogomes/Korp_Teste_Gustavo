import { ChangeDetectionStrategy, Component, DestroyRef, ViewChild, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe, DecimalPipe, NgClass } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { Table } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DynamicDialogModule, DialogService } from 'primeng/dynamicdialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { NotaFiscalService } from '../../../services/nota-fiscal.service';
import { StatusNotaFiscal } from '../../../models/nota-fiscal.model';
import { FormNota } from '../form-nota/form-nota.component';

@Component({
  selector: 'app-lista-notas',
  imports: [RouterLink, DatePipe, DecimalPipe, TableModule, ButtonModule, CardModule, TagModule, TooltipModule, DynamicDialogModule, ConfirmDialogModule],
  providers: [ConfirmationService],
  templateUrl: './lista-notas.component.html',
  styleUrl: './lista-notas.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListaNotas {
  @ViewChild('dt') table!: Table;

  private notaService         = inject(NotaFiscalService);
  private dialogService       = inject(DialogService);
  private confirmationService = inject(ConfirmationService);
  private messageService      = inject(MessageService);
  private destroyRef          = inject(DestroyRef);

  readonly notas             = this.notaService.notas;
  readonly carregando        = this.notaService.carregando;
  readonly erroCarregamento  = this.notaService.erro;
  erro: string | null = null;
  StatusNotaFiscal = StatusNotaFiscal;

  readonly filtro = signal<'hoje' | '7d' | '30d' | 'todos'>('todos');

  readonly notasFiltradas = computed(() => {
    const todas = this.notas();
    const f = this.filtro();
    if (f === 'todos') return todas;
    const agora = new Date();
    const inicio = new Date(agora);
    if (f === 'hoje') {
      inicio.setHours(0, 0, 0, 0);
    } else if (f === '7d') {
      inicio.setDate(agora.getDate() - 7);
      inicio.setHours(0, 0, 0, 0);
    } else {
      inicio.setDate(agora.getDate() - 30);
      inicio.setHours(0, 0, 0, 0);
    }
    return todas.filter(n => new Date(n.dataEmissao) >= inicio);
  });

  readonly busca = signal('');

  readonly notasVisiveis = computed(() => {
    const notas = this.notasFiltradas();
    const q = this.busca().toLowerCase().trim();
    if (!q) return notas;
    return notas.filter(n =>
      String(n.numero).includes(q) ||
      n.itens.some(i => i.descricao.toLowerCase().includes(q))
    );
  });

  readonly kpiTotal   = computed(() => this.notasFiltradas().length);
  readonly kpiAbertas = computed(() => this.notasFiltradas().filter(n => n.status === StatusNotaFiscal.Aberta).length);
  readonly kpiFechadas= computed(() => this.notasFiltradas().filter(n => n.status === StatusNotaFiscal.Fechada).length);
  readonly kpiReceita = computed(() => this.notasFiltradas().reduce((s, n) => s + n.total, 0));

  private _lastSort: { field: string; order: number } | null = null;

  onSort(event: { field: string; order: number }): void {
    if (this._lastSort?.field === event.field && this._lastSort.order === -1 && event.order === 1) {
      this._lastSort = null;
      setTimeout(() => this.table.reset());
    } else {
      this._lastSort = { field: event.field, order: event.order };
    }
  }

  abrirNova(): void {
    this.erro = null;
    const ref = this.dialogService.open(FormNota, {
      header: 'Nova Nota Fiscal',
      width: '720px',
      modal: true,
      closable: true,
      dismissableMask: true,
    });
    ref!.onClose.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((salvo: boolean) => {
      if (salvo) {
        this.notaService.carregar();
        this.messageService.add({ severity: 'success', summary: 'Nota emitida', detail: 'Nota fiscal emitida com sucesso.' });
      }
    });
  }

  cancelar(id: number): void {
    this.confirmationService.confirm({
      message: 'Tem certeza que deseja cancelar esta nota fiscal? Esta ação não pode ser desfeita.',
      header: 'Confirmar cancelamento',
      acceptLabel: 'Cancelar nota',
      rejectLabel: 'Voltar',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-outlined p-button-secondary',
      accept: () => {
        this.erro = null;
        this.notaService.cancelarNota(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
          next: () => this.messageService.add({ severity: 'warn', summary: 'Nota cancelada', detail: 'Nota fiscal cancelada com sucesso.' }),
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Erro ao cancelar', detail: err?.mensagem || 'Erro ao cancelar nota fiscal', life: 6000 }),
        });
      },
    });
  }

  severityStatus(status: StatusNotaFiscal): 'success' | 'secondary' | 'danger' {
    if (status === StatusNotaFiscal.Aberta)    return 'success';
    if (status === StatusNotaFiscal.Cancelada) return 'danger';
    return 'secondary';
  }

  labelStatus(status: StatusNotaFiscal): string {
    if (status === StatusNotaFiscal.Aberta)    return 'Aberta';
    if (status === StatusNotaFiscal.Cancelada) return 'Cancelada';
    return 'Fechada';
  }
}
