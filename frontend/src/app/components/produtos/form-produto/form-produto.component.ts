import { ChangeDetectionStrategy, ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ProdutoService } from '../../../services/produto.service';
import { Produto } from '../../../models/produto.model';

@Component({
  selector: 'app-form-produto',
  imports: [FormsModule, ButtonModule, InputTextModule, InputNumberModule],
  templateUrl: './form-produto.component.html',
  styleUrl: './form-produto.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormProduto implements OnInit {
  private ref            = inject<DynamicDialogRef>(DynamicDialogRef,       { optional: true });
  private config         = inject<DynamicDialogConfig>(DynamicDialogConfig, { optional: true });
  private route          = inject(ActivatedRoute);
  private router         = inject(Router);
  private produtoService = inject(ProdutoService);
  private cdr            = inject(ChangeDetectorRef);
  private destroyRef     = inject(DestroyRef);

  produto: Partial<Produto> = { codigo: '', descricao: '', saldo: 0 };
  editando = false;
  salvando = false;
  erro: string | null = null;

  ngOnInit(): void {
    const id = this.config?.data?.id ?? this.route.snapshot.params['id'];
    if (id) {
      this.editando = true;
      this.produtoService.buscarProduto(+id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: p => { this.produto = p; this.cdr.markForCheck(); },
        error: () => { this.erro = 'Erro ao carregar produto'; this.cdr.markForCheck(); },
      });
    }
  }

  cancelar(): void {
    if (this.ref) this.ref.close(false);
    else this.router.navigate(['/produtos']);
  }

  salvar(): void {
    this.produto.descricao = (this.produto.descricao ?? '').trim().toUpperCase();
    this.produto.codigo    = (this.produto.codigo    ?? '').trim().toUpperCase();

    const descricaoNorm = this.produto.descricao;
    const idAtual = (this.produto as Produto).id;
    const duplicado = this.produtoService.produtos().some(
      p => p.descricao.toUpperCase() === descricaoNorm && p.id !== idAtual
    );

    if (duplicado) {
      this.erro = `Já existe um produto com a descrição "${descricaoNorm}".`;
      this.cdr.markForCheck();
      return;
    }

    this.salvando = true;
    this.erro = null;
    const op: Observable<unknown> = this.editando
      ? this.produtoService.atualizarProduto(this.produto as Produto)
      : this.produtoService.cadastrarProduto(this.produto as Omit<Produto, 'id'>);

    const ref = this.ref;
    op.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        if (ref) ref.close(true);
        else this.router.navigate(['/produtos']);
      },
      error: (err) => {
        this.erro = err?.mensagem || 'Erro ao salvar produto';
        this.salvando = false;
        this.cdr.markForCheck();
      },
    });
  }
}
