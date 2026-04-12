import { Routes } from '@angular/router';
import { ListaNotas } from './components/notas/lista-notas/lista-notas.component';
import { FormNota } from './components/notas/form-nota/form-nota.component';
import { ImpressaoNotaComponent } from './components/notas/impressao-nota/impressao-nota.component';
import { ListaProdutos } from './components/produtos/lista-produtos/lista-produtos.component';
import { FormProduto } from './components/produtos/form-produto/form-produto.component';

export const routes: Routes = [
  { path: '', redirectTo: 'notas', pathMatch: 'full' },
  { path: 'notas', component: ListaNotas },
  { path: 'notas/:id/editar', component: FormNota },
  { path: 'notas/:id/impressao', component: ImpressaoNotaComponent },
  { path: 'produtos', component: ListaProdutos },
  { path: 'produtos/:id/editar', component: FormProduto },
];
