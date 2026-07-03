import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.module').then((m) => m.AuthModule),
  },
  {
    path: 'messages',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./features/messages/messages.module').then((m) => m.MessagesModule),
  },
  {
    path: '',
    loadChildren: () => import('./features/ideas/ideas.module').then((m) => m.IdeasModule),
  },
  { path: '**', redirectTo: '' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { scrollPositionRestoration: 'top' })],
  exports: [RouterModule],
})
export class AppRoutingModule {}
