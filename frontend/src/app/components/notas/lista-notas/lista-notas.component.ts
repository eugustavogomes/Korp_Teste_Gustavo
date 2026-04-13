import { ChangeDetectionStrategy, Component, ViewChild, computed, inject, signal } from '@angular/core';
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
import { ConfirmationService } from 'primeng/api';
import { NotaFiscalService } from '../../../services/nota-fiscal.service';
import { StatusNotaFiscal } from '../../../models/nota-fiscal.model';
import { FormNota } from '../form-nota/form-nota.component';

@Component({
  selector: 'app-lista-notas',
  imports: [RouterLink, DatePipe, DecimalPipe, NgClass, TableModule, ButtonModule, CardModule, TagModule, TooltipModule, DynamicDialogModule, ConfirmDialogModule],
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

  readonly notas             = this.notaService.notas;
  readonly carregando        = this.notaService.carregando;
  readonly erroCarregamento  = this.notaService.erro;
  erro: string | null = null;
  StatusNotaFiscal = StatusNotaFiscal;

  get kpiTotal()   { return this.notas().length; }
  get kpiAbertas() { return this.notas().filter(n => n.status === StatusNotaFiscal.Aberta).length; }
  get kpiFechadas(){ return this.notas().filter(n => n.status === StatusNotaFiscal.Fechada).length; }
  get kpiReceita() { return this.notas().reduce((s, n) => s + n.total, 0); }

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
    ref!.onClose.subscribe((salvo: boolean) => {
      if (salvo) this.notaService.carregar();
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
        this.notaService.cancelarNota(id).subscribe({
          error: () => (this.erro = 'Erro ao cancelar nota fiscal'),
        });
      },
    });
  }

  severityStatus(status: StatusNotaFiscal): 'success' | 'secondary' {
    return status === StatusNotaFiscal.Aberta ? 'success' : 'secondary';
  }

  labelStatus(status: StatusNotaFiscal): string {
    return status === StatusNotaFiscal.Aberta ? 'Aberta' : 'Fechada';
  }
}
