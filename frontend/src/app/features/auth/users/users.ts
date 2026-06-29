import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CreateInvitationResult, UserInfo } from '@core/models/auth.models';
import { AuthService } from '@core/services/auth.service';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { Icon } from "@shared/ui/icon/icon";
import { Dialog } from '@angular/cdk/dialog';
import { InviteUser } from '../invite-user/invite-user';
import { ShowUserInvitation } from '../show-user-invitation/show-user-invitation';
import { ConfirmDialog } from '@shared/ui/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-users',
  imports: [TranslocoModule, Icon],
  templateUrl: './users.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './users.css',
})
export class Users implements OnInit {
  
  readonly authService = inject(AuthService);
  readonly translocoService = inject(TranslocoService);
  readonly dialog = inject(Dialog);

  users = signal<UserInfo[]>([]);

  ngOnInit(): void {
    this.loadUsers();
  }

  private loadUsers() {
    this.authService.getUsers().subscribe({
      next: (users) => {
        this.users.set(users);
      },
      error: (error) => {
        console.error('Error loading users:', error);
        this.users.set([]);
      }
    });
  }

  async deleteUser(id: string) {
    const dialogRef = this.dialog.open(ConfirmDialog, {
      width: '640px',
      height: 'auto',
      data: {
        title: this.translocoService.translate('users.confirmDeleteTitle'),
        message: this.translocoService.translate('users.confirmDeleteMessage'),
        confirmText: this.translocoService.translate('users.delete'),
        confirmIcon: 'trash-bin',
        cancelText: this.translocoService.translate('users.cancel'),
        danger: true
      } 
    });

    dialogRef.closed.subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.authService.deleteUser(id).subscribe({
        next: () => {
          this.loadUsers();
        },
        error: (error) => {
          console.error('Error deleting user:', error);
        }
      });
    });
  }

  openInviteUserModal() {
    const dialogRef = this.dialog.open<CreateInvitationResult>(InviteUser, {
      width: '640px',
      height: 'auto',
    });

    dialogRef.closed.subscribe((result) => {
      if (result) {
        this.openResultModal(result);
      }
    });
  }

  openResultModal(result: CreateInvitationResult) {
    this.dialog.open<ShowUserInvitation, CreateInvitationResult, void>(ShowUserInvitation, {
      width: '640px',
      height: 'auto',
      data: result
    });
  }
}
