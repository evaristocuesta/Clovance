import { Component, computed, input, output } from '@angular/core';
import { UserInfo } from '@core/models/auth.models';
import { TranslocoModule } from '@jsverse/transloco';
import { Icon } from "@shared/ui/icon/icon";

@Component({
  selector: 'app-user-card',
  imports: [Icon, TranslocoModule],
  templateUrl: './user-card.html',
  styleUrl: './user-card.css',
})
export class UserCard {
  readonly user = input.required<UserInfo>();
  readonly isOwner = input.required<boolean>();
  readonly deleteUser = output<string>();
  
  protected userInitials = computed(() => {
    const firstName = this.user().firstName.trim();
    const lastName = this.user().lastName.trim();

    return firstName && lastName ? firstName.charAt(0).toUpperCase() + lastName.charAt(0).toUpperCase() : 'AA';
  });

  protected onDelete(): void {
    this.deleteUser.emit(this.user().id);
  }
}
