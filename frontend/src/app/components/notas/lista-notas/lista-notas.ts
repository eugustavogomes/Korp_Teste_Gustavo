import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { NotaFiscalService } from '../../../services/nota-fiscal';
import { NotaFiscal, StatusNotaFiscal } from '../../../models/nota-fiscal.model';
import { FormNota } from '../form-nota/form-nota';

@Component({
  selector: 'app-lista-notas',
  imports: [RouterLink, DatePipe, DecimalPipe, MatTableModule, MatButtonModule, MatIconModule, MatCardModule, MatTooltipModule],
  templateUrl: './lista-notas.html',
  styleUrl: './lista-notas.scss',
})
export class ListaNotas implements OnInit {
  notas: NotaFiscal[] = [];
  colunas = ['numero', 'dataEmissao', 'status', 'total', 'acoes'];
  erro: string | null = null;
  StatusNotaFiscal = StatusNotaFiscal;

  constructor(
    private notaService: NotaFiscalService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.notaService.listarNotas().subscribe({
      next: n => (this.notas = n),
      error: () => (this.erro = 'Erro ao carregar notas fiscais'),
    });
  }

  abrirNova(): void {
    this.dialog
      .open(FormNota, { maxWidth: '95vw' })
      .afterClosed()
      .subscribe(salvo => { if (salvo) this.carregar(); });
  }

  cancelar(id: number): void {
    if (!confirm('Deseja cancelar esta nota fiscal?')) return;
    this.notaService.cancelarNota(id).subscribe({
      next: () => this.carregar(),
      error: () => (this.erro = 'Erro ao cancelar nota'),
    });
  }
}
