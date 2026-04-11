import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './shared/header/header.component';
import { ProdutoService } from './services/produto.service';
import { NotaFiscalService } from './services/nota-fiscal';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  constructor(
    private produtoService: ProdutoService,
    private notaFiscalService: NotaFiscalService,
  ) {}

  ngOnInit(): void {
    this.produtoService.carregar();
    this.notaFiscalService.carregar();
  }
}
