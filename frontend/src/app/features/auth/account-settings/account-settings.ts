import { ChangeDetectionStrategy, Component } from '@angular/core';
import { UpdateUser } from '../update-user/update-user';
import { ChangePassword } from '../change-password/change-password';

@Component({
  selector: 'app-account-settings',
  imports: [UpdateUser, ChangePassword],
  templateUrl: './account-settings.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './account-settings.css',
})
export class AccountSettings {
  
}
