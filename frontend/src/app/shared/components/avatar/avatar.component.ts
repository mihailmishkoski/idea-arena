import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { avatarById, AvatarInfo } from '../../../core/avatars';

/** A round avatar rendered from the built-in collection by its id. */
@Component({
    selector: 'app-avatar',
    templateUrl: './avatar.component.html',
    styleUrls: ['./avatar.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class AvatarComponent {
  @Input() avatarId: string | null | undefined;
  @Input() size = 32;

  get info(): AvatarInfo {
    return avatarById(this.avatarId);
  }
}
