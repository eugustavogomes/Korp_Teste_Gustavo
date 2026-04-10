import { Component, OnInit } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { ProdutoService } from '../../../services/produto.service';
import { Produto } from '../../../models/produto.model';
import { FormProduto } from '../form-produto/form-produto';

@Component({
  selector: 'app-lista-produtos',
  imports: [MatTableModule, MatButtonModule, MatIconModule, MatCardModule, MatTooltipModule],
  templateUrl: './lista-produtos.html',
  styleUrl: './lista-produtos.scss',
})
export class ListaProdutos implements OnInit {
  produtos: Produto[] = [];
  colunas = ['codigo', 'descricao', 'saldo', 'acoes'];
  erro: string | null = null;

  constructor(
    private produtoService: ProdutoService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.produtoService.listarProdutos().subscribe({
      next: p => (this.produtos = p),
      error: () => (this.erro = 'Erro ao carregar produtos'),
    });
  }

  abrirNovo(): void {
    this.dialog
      .open(FormProduto, { width: '480px' })
      .afterClosed()
      .subscribe(salvo => { if (salvo) this.carregar(); });
  }

  abrirEditar(id: number): void {
    this.dialog
      .open(FormProduto, { width: '480px', data: { id } })
      .afterClosed()
      .subscribe(salvo => { if (salvo) this.carregar(); });
  }

  excluir(id: number): void {
    if (!confirm('Deseja excluir este produto?')) return;
    this.produtoService.excluirProduto(id).subscribe({
      next: () => this.carregar(),
      error: () => (this.erro = 'Erro ao excluir produto'),
    });
  }
}
