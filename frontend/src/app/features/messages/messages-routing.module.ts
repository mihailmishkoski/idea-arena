import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MessagesComponent } from './messages/messages.component';

const routes: Routes = [
  { path: '', component: MessagesComponent, title: 'Messages · BusinessIdea' },
  { path: ':id', component: MessagesComponent, title: 'Messages · BusinessIdea' },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class MessagesRoutingModule {}
