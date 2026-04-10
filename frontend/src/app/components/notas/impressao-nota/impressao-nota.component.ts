import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Subject, finalize, takeUntil } from 'rxjs';
import { NotaFiscal, StatusNotaFiscal } from '../../../models/nota-fiscal.model';
import { NotaFiscalService } from '../../../services/nota-fiscal';
import { ApiError } from '../../../services/error-handler';

@Component({
  selector: 'app-impressao-nota',
  standalone: true,
  imports: [RouterLink, DatePipe, DecimalPipe, MatButtonModule, MatIconModule],
  templateUrl: './impressao-nota.html',
})
export class ImpressaoNotaComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  nota: NotaFiscal | null = null;
  processando = false;
  erro: ApiError | null = null;
  StatusNotaFiscal = StatusNotaFiscal;

  constructor(
    private route: ActivatedRoute,
    private notaService: NotaFiscalService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    this.carregarNota(+id);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  carregarNota(id: number): void {
    this.processando = true;
    this.notaService
      .buscarNota(id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => (this.processando = false))
      )
      .subscribe({
        next: nota => (this.nota = nota),
        error: (err: ApiError) => (this.erro = err),
      });
  }

  imprimir(): void {
    if (!this.nota || this.nota.status !== StatusNotaFiscal.Aberta) return;

    this.processando = true;
    this.erro = null;

    this.notaService
      .imprimirNota(this.nota.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => (this.processando = false))
      )
      .subscribe({
        next: () => {
          this.nota!.status = StatusNotaFiscal.Fechada;
          window.print();
        },
        error: (err: ApiError) => {
          this.erro = err;
        },
      });
  }
}
