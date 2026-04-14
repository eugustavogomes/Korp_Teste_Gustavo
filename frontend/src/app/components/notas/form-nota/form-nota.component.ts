import { ChangeDetectionStrategy, ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { DynamicDialogRef } from 'primeng/dynamicdialog';
import { ProdutoService } from '../../../services/produto.service';
import { NotaFiscalService } from '../../../services/nota-fiscal.service';
import { InsightsService } from '../../../services/insights.service';
import { Produto } from '../../../models/produto.model';

interface ItemForm {
  produto: Produto;
  quantidade: number;
  precoUnitario: number;
}

@Component({
  selector: 'app-form-nota',
  imports: [FormsModule, DecimalPipe, ButtonModule, SelectModule, InputNumberModule, InputTextModule, TextareaModule, TableModule, TooltipModule],
  templateUrl: './form-nota.component.html',
  styleUrl: './form-nota.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormNota implements OnInit {
  private ref             = inject<DynamicDialogRef>(DynamicDialogRef, { optional: true });
  private produtoService  = inject(ProdutoService);
  private notaService     = inject(NotaFiscalService);
  private insightsService = inject(InsightsService);
  private router          = inject(Router);
  private cdr             = inject(ChangeDetectorRef);
  private destroyRef      = inject(DestroyRef);

  readonly produtos = this.produtoService.produtos;

  itens: ItemForm[] = [];
  produtoSelecionado: Produto | null = null;
  quantidade = 1;
  precoUnitario = 0;
  precoDisplay = '';
  salvando = false;
  erro: string | null = null;

  textoPedido = '';
  interpretando = false;
  avisoIA: string | null = null;
  iaBloqueadaAte: number | null = null;

  get iaEmCooldown(): boolean {
    return this.iaBloqueadaAte !== null && Date.now() < this.iaBloqueadaAte;
  }

  get iaCooldownSegundos(): number {
    if (!this.iaBloqueadaAte) return 0;
    return Math.ceil((this.iaBloqueadaAte - Date.now()) / 1000);
  }

  ngOnInit(): void {
    this.produtoService.carregar();
  }

  get total(): number {
    return this.itens.reduce((s, i) => s + i.quantidade * i.precoUnitario, 0);
  }

  get totalUnidades(): number {
    return this.itens.reduce((s, i) => s + i.quantidade, 0);
  }

  get saldoRestante(): number {
    if (!this.produtoSelecionado) return 0;
    const jaAdicionado = this.itens
      .filter(i => i.produto.id === this.produtoSelecionado!.id)
      .reduce((s, i) => s + i.quantidade, 0);
    return Math.max(0, (this.produtoSelecionado.saldoDisponivel ?? 0) - jaAdicionado);
  }

  onProdutoChange(produto: typeof this.produtoSelecionado): void {
    this.produtoSelecionado = produto;
    if (produto) {
      const jaAdicionado = this.itens
        .filter(i => i.produto.id === produto.id)
        .reduce((s, i) => s + i.quantidade, 0);
      const disponivel = Math.max(0, (produto.saldoDisponivel ?? 0) - jaAdicionado);
      if (disponivel > 0 && this.quantidade > disponivel) {
        this.quantidade = disponivel;
      }
    }
    this.cdr.markForCheck();
  }

  onQuantidadeChange(valor: number | null): void {
    const v = Math.max(1, valor ?? 1);
    if (!this.produtoSelecionado) {
      this.quantidade = v;
      return;
    }
    const max = this.saldoRestante;
    this.quantidade = max > 0 ? Math.min(v, max) : 1;
    this.cdr.detectChanges();
  }

  onPrecoInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '');
    if (!digits) {
      this.precoUnitario = 0;
      this.precoDisplay = '';
      input.value = '';
      return;
    }
    this.precoUnitario = Number(digits) / 100;
    this.precoDisplay = this.precoUnitario.toLocaleString('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    });
    input.value = this.precoDisplay;
    const len = input.value.length;
    input.setSelectionRange(len, len);
  }

  adicionarItem(): void {
    if (!this.produtoSelecionado || this.quantidade <= 0 || this.precoUnitario <= 0) return;

    const jaAdicionado = this.itens
      .filter(i => i.produto.id === this.produtoSelecionado!.id)
      .reduce((total, i) => total + i.quantidade, 0);

    const saldoDisponivel = this.produtoSelecionado.saldoDisponivel ?? 0;
    const disponivel = Math.max(0, saldoDisponivel - jaAdicionado);

    if (disponivel <= 0) {
      this.erro = `Sem saldo disponível para "${this.produtoSelecionado.descricao}".`;
      this.quantidade = 1;
      this.cdr.markForCheck();
      return;
    }

    if (this.quantidade > disponivel) {
      this.erro = `Quantidade inválida: apenas ${disponivel} unidade${disponivel === 1 ? '' : 's'} disponível${disponivel === 1 ? '' : 's'} para "${this.produtoSelecionado.descricao}".`;
      this.quantidade = disponivel;
      this.cdr.markForCheck();
      return;
    }

    this.erro = null;
    const indiceExistente = this.itens.findIndex(i => i.produto.id === this.produtoSelecionado!.id);
    if (indiceExistente >= 0) {
      this.itens = this.itens.map((item, idx) =>
        idx === indiceExistente
          ? { ...item, quantidade: item.quantidade + this.quantidade, precoUnitario: this.precoUnitario }
          : item
      );
    } else {
      this.itens = [
        ...this.itens,
        { produto: this.produtoSelecionado, quantidade: this.quantidade, precoUnitario: this.precoUnitario },
      ];
    }
    this.produtoSelecionado = null;
    this.quantidade = 1;
    this.precoUnitario = 0;
    this.precoDisplay = '';
  }

  removerItem(index: number): void {
    this.itens = this.itens.filter((_, i) => i !== index);
  }

  interpretarPedido(): void {
    if (!this.textoPedido.trim()) return;
    this.interpretando = true;
    this.avisoIA = null;
    this.erro = null;

    this.insightsService.interpretarPedido(this.textoPedido, this.produtos()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: resultado => {
        for (const item of resultado.itens) {
          const produto = this.produtos().find(p => p.id === item.produtoId);
          if (!produto) continue;

          const jaAdicionado = this.itens
            .filter(i => i.produto.id === produto.id)
            .reduce((s, i) => s + i.quantidade, 0);

          const quantidadeValida = Math.min(item.quantidade, produto.saldoDisponivel - jaAdicionado);
          if (quantidadeValida <= 0) continue;

          const indiceExistente = this.itens.findIndex(i => i.produto.id === produto.id);
          if (indiceExistente >= 0) {
            this.itens = this.itens.map((entry, idx) =>
              idx === indiceExistente
                ? { ...entry, quantidade: entry.quantidade + quantidadeValida, precoUnitario: item.precoUnitario }
                : entry
            );
          } else {
            this.itens = [
              ...this.itens,
              { produto, quantidade: quantidadeValida, precoUnitario: item.precoUnitario },
            ];
          }
        }

        if (resultado.naoEncontrados.length > 0) {
          this.avisoIA = `Não encontrado no catálogo: ${resultado.naoEncontrados.join(', ')}`;
        }

        this.textoPedido = '';
        this.interpretando = false;
        this.cdr.markForCheck();
      },
      error: err => {
        if (err.tipo === 'negocio' && err.mensagem?.includes('Limite')) {
          this.iaBloqueadaAte = Date.now() + 60_000;
          this.erro = `${err.mensagem} (disponível em ${this.iaCooldownSegundos}s)`;
        } else {
          this.erro = err.mensagem || 'Erro ao interpretar o pedido. Tente novamente.';
        }
        this.interpretando = false;
        this.cdr.markForCheck();
      },
    });
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
    this.notaService.criarNota(request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.produtoService.carregar();
        if (ref) ref.close(true);
        else this.router.navigate(['/notas']);
      },
      error: err => {
        this.erro = err.mensagem || 'Erro ao emitir nota fiscal';
        this.salvando = false;
        this.cdr.markForCheck();
      },
    });
  }
}
