import { Component, Input } from '@angular/core';

/** Small centered loading indicator used while data is being fetched. */
@Component({
  selector: 'app-spinner',
  templateUrl: './spinner.component.html',
  styleUrls: ['./spinner.component.scss'],
})
export class SpinnerComponent {
  @Input() label = 'Loading…';
}
