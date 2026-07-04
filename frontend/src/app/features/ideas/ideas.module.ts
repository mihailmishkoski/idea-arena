import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { CommentFormComponent } from './components/comment-form/comment-form.component';
import { CommentItemComponent } from './components/comment-item/comment-item.component';
import { IdeaCardComponent } from './components/idea-card/idea-card.component';
import { IdeaCreateComponent } from './idea-create/idea-create.component';
import { IdeaDetailComponent } from './idea-detail/idea-detail.component';
import { IdeaListComponent } from './idea-list/idea-list.component';
import { IdeasRoutingModule } from './ideas-routing.module';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { WinnersComponent } from './winners/winners.component';

@NgModule({
  declarations: [
    IdeaListComponent,
    IdeaDetailComponent,
    IdeaCreateComponent,
    IdeaCardComponent,
    CommentItemComponent,
    CommentFormComponent,
    WinnersComponent,
    UserProfileComponent,
  ],
  imports: [SharedModule, IdeasRoutingModule],
})
export class IdeasModule {}
