import { Component } from '@angular/core';
import { AsyncPipe, DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DynamicDialogModule, DialogService } from 'primeng/dynamicdialog';
import { NotaFiscalService } from '../../../services/nota-fiscal';
import { StatusNotaFiscal } from '../../../models/nota-fiscal.model';
import { FormNota } from '../form-nota/form-nota';

@Component({
  selector: 'app-lista-notas',
  imports: [AsyncPipe, RouterLink, DatePipe, DecimalPipe, TableModule, ButtonModule, CardModule, TagModule, TooltipModule, DynamicDialogModule],
  templateUrl: './lista-notas.html',
  styleUrl: './lista-notas.scss',
})
export class ListaNotas {
  readonly notas$ = this.notaService.notas$;
  readonly carregando$ = this.notaService.carregando$;
  readonly erroCarregamento$ = this.notaService.erro$;
  erro: string | null = null;
  StatusNotaFiscal = StatusNotaFiscal;

  constructor(
    private notaService: NotaFiscalService,
    private dialogService: DialogService,
  ) {}

  recarregar(): void {
    this.notaService.carregar();
  }

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
    if (!confirm('Deseja cancelar esta nota fiscal?')) return;
    this.erro = null;
    this.notaService.cancelarNota(id).subscribe({
      error: () => (this.erro = 'Erro ao cancelar nota fiscal'),
    });
  }

  severityStatus(status: StatusNotaFiscal): 'info' | 'secondary' {
    return status === StatusNotaFiscal.Aberta ? 'info' : 'secondary';
  }

  labelStatus(status: StatusNotaFiscal): string {
    return status === StatusNotaFiscal.Aberta ? 'Aberta' : 'Fechada';
  }
}
