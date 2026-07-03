import { Component, Input, ChangeDetectionStrategy } from '@angular/core';

/** Small centered loading indicator used while data is being fetched. */
@Component({
    selector: 'app-spinner',
    templateUrl: './spinner.component.html',
    styleUrls: ['./spinner.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class SpinnerComponent {
  @Input() label = 'Loading…';
}
