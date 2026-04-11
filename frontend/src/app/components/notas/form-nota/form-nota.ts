import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { DynamicDialogRef } from 'primeng/dynamicdialog';
import { ProdutoService } from '../../../services/produto.service';
import { NotaFiscalService } from '../../../services/nota-fiscal';
import { Produto } from '../../../models/produto.model';

interface ItemForm {
  produto: Produto;
  quantidade: number;
  precoUnitario: number;
}

@Component({
  selector: 'app-form-nota',
  imports: [FormsModule, DecimalPipe, ButtonModule, SelectModule, InputNumberModule, TableModule, TooltipModule],
  templateUrl: './form-nota.html',
  styleUrl: './form-nota.scss',
})
export class FormNota implements OnInit {
  private ref            = inject<DynamicDialogRef>(DynamicDialogRef, { optional: true });
  private produtoService = inject(ProdutoService);
  private notaService    = inject(NotaFiscalService);
  private router         = inject(Router);

  readonly produtos = this.produtoService.produtos;

  itens: ItemForm[] = [];
  produtoSelecionado: Produto | null = null;
  quantidade = 1;
  precoUnitario = 0;
  salvando = false;
  erro: string | null = null;

  ngOnInit(): void {
    if (this.produtos().length === 0) {
      this.produtoService.carregar();
    }
  }

  get total(): number {
    return this.itens.reduce((s, i) => s + i.quantidade * i.precoUnitario, 0);
  }

  adicionarItem(): void {
    if (!this.produtoSelecionado || this.quantidade <= 0 || this.precoUnitario <= 0) return;
    this.itens = [
      ...this.itens,
      { produto: this.produtoSelecionado, quantidade: this.quantidade, precoUnitario: this.precoUnitario },
    ];
    this.produtoSelecionado = null;
    this.quantidade = 1;
    this.precoUnitario = 0;
  }

  removerItem(index: number): void {
    this.itens = this.itens.filter((_, i) => i !== index);
  }

  cancelar(): void {
    if (this.ref) this.ref.close(false);
    else this.router.navigate(['/notas']);
  }

  emitir(): void {
    if (this.itens.length === 0) return;
    this.salvando = true;
    this.erro = null;

    const request = {
      itens: this.itens.map(i => ({
        produtoId: i.produto.id,
        descricao: i.produto.descricao,
        quantidade: i.quantidade,
        precoUnitario: i.precoUnitario,
      })),
    };

    const ref = this.ref;
    this.notaService.criarNota(request).subscribe({
      next: () => {
        if (ref) ref.close(true);
        else this.router.navigate(['/notas']);
      },
      error: err => {
        this.erro = err.mensagem || 'Erro ao emitir nota fiscal';
        this.salvando = false;
      },
    });
  }
}
