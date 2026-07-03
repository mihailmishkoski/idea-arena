import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AvatarComponent } from './components/avatar/avatar.component';
import { MetricBadgeComponent } from './components/metric-badge/metric-badge.component';
import { SpinnerComponent } from './components/spinner/spinner.component';
import { VoteButtonsComponent } from './components/vote-buttons/vote-buttons.component';
import { MetricLabelPipe } from './pipes/metric-label.pipe';
import { TimeAgoPipe } from './pipes/time-ago.pipe';

/**
 * Reusable, presentational building blocks shared across feature modules:
 * the vote control, the spinner, and the display pipes. Also re-exports the
 * common Angular modules feature modules always need so they don't repeat them.
 */
@NgModule({
  declarations: [
    VoteButtonsComponent,
    SpinnerComponent,
    MetricBadgeComponent,
    AvatarComponent,
    TimeAgoPipe,
    MetricLabelPipe,
  ],
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule],
  exports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    VoteButtonsComponent,
    SpinnerComponent,
    MetricBadgeComponent,
    AvatarComponent,
    TimeAgoPipe,
    MetricLabelPipe,
  ],
})
export class SharedModule {}
