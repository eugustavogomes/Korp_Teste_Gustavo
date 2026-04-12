import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DynamicDialogModule, DialogService } from 'primeng/dynamicdialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { NotaFiscalService } from '../../../services/nota-fiscal';
import { StatusNotaFiscal } from '../../../models/nota-fiscal.model';
import { FormNota } from '../form-nota/form-nota';

@Component({
  selector: 'app-lista-notas',
  imports: [RouterLink, DatePipe, DecimalPipe, TableModule, ButtonModule, CardModule, TagModule, TooltipModule, DynamicDialogModule, ConfirmDialogModule],
  providers: [ConfirmationService],
  templateUrl: './lista-notas.html',
  styleUrl: './lista-notas.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListaNotas {
  private notaService         = inject(NotaFiscalService);
  private dialogService       = inject(DialogService);
  private confirmationService = inject(ConfirmationService);

  readonly notas             = this.notaService.notas;
  readonly carregando        = this.notaService.carregando;
  readonly erroCarregamento  = this.notaService.erro;
  erro: string | null = null;
  StatusNotaFiscal = StatusNotaFiscal;

  abrirNova(): void {
    this.erro = null;
    const ref = this.dialogService.open(FormNota, {
      header: 'Nova Nota Fiscal',
      width: '720px',
      modal: true,
      closable: true,
    });
    ref!.onClose.subscribe((salvo: boolean) => {
      if (salvo) this.notaService.carregar();
    });
  }

  cancelar(id: number): void {
    this.confirmationService.confirm({
      message: 'Tem certeza que deseja cancelar esta nota fiscal? Esta ação não pode ser desfeita.',
      header: 'Confirmar cancelamento',
      icon: 'pi pi-times-circle',
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

  severityStatus(status: StatusNotaFiscal): 'info' | 'secondary' {
    return status === StatusNotaFiscal.Aberta ? 'info' : 'secondary';
  }

  labelStatus(status: StatusNotaFiscal): string {
    return status === StatusNotaFiscal.Aberta ? 'Aberta' : 'Fechada';
  }
}
