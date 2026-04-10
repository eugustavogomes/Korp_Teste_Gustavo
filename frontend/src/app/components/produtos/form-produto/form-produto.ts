import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { ProdutoService } from '../../../services/produto.service';
import { Produto } from '../../../models/produto.model';

@Component({
  selector: 'app-form-produto',
  imports: [FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatDialogModule],
  templateUrl: './form-produto.html',
  styleUrl: './form-produto.scss',
})
export class FormProduto implements OnInit {
  private dialogRef = inject<MatDialogRef<FormProduto>>(MatDialogRef, { optional: true });
  private dialogData = inject<{ id?: number }>(MAT_DIALOG_DATA, { optional: true });

  produto: Partial<Produto> = { codigo: '', descricao: '', saldo: 0 };
  editando = false;
  salvando = false;
  erro: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private produtoService: ProdutoService
  ) {}

  ngOnInit(): void {
    const id = this.dialogData?.id ?? this.route.snapshot.params['id'];
    if (id) {
      this.editando = true;
      this.produtoService.buscarProduto(+id).subscribe({
        next: p => (this.produto = p),
        error: () => (this.erro = 'Erro ao carregar produto'),
      });
    }
  }

  cancelar(): void {
    if (this.dialogRef) {
      this.dialogRef.close(false);
    } else {
      this.router.navigate(['/produtos']);
    }
  }

  salvar(): void {
    this.salvando = true;
    this.erro = null;
    const op: Observable<unknown> = this.editando
      ? this.produtoService.atualizarProduto(this.produto as Produto)
      : this.produtoService.cadastrarProduto(this.produto as Omit<Produto, 'id'>);

    op.subscribe({
      next: () => {
        if (this.dialogRef) {
          this.dialogRef.close(true);
        } else {
          this.router.navigate(['/produtos']);
        }
      },
      error: () => {
        this.erro = 'Erro ao salvar produto';
        this.salvando = false;
      },
    });
  }
}
