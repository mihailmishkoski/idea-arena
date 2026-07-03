import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';
import { IdeaCreateComponent } from './idea-create/idea-create.component';
import { IdeaDetailComponent } from './idea-detail/idea-detail.component';
import { IdeaListComponent } from './idea-list/idea-list.component';

const routes: Routes = [
  { path: '', component: IdeaListComponent, title: 'BusinessIdea · Feed' },
  {
    path: 'submit',
    component: IdeaCreateComponent,
    canActivate: [AuthGuard],
    title: 'Create an Idea · BusinessIdea',
  },
  { path: 'ideas/:id', component: IdeaDetailComponent, title: 'Idea · BusinessIdea' },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class IdeasRoutingModule {}
