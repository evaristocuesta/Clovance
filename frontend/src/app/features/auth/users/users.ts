import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CreateInvitationResult, UserInfo } from '@core/models/auth.models';
import { AuthService } from '@core/services/auth.service';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { Icon } from "@shared/ui/icon/icon";
import { InviteUser } from '../invite-user/invite-user';
import { ShowUserInvitation } from '../show-user-invitation/show-user-invitation';
import { ConfirmDialog } from '@shared/ui/confirm-dialog/confirm-dialog';
import { UserCard } from "../user-card/user-card";
import { DialogService } from '@shared/ui/dialog/dialog.service';

@Component({
  selector: 'app-users',
  imports: [TranslocoModule, Icon, UserCard],
  templateUrl: './users.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './users.css',
})
export class Users implements OnInit {
  
  readonly authService = inject(AuthService);
  readonly translocoService = inject(TranslocoService);
  private readonly dialogService = inject(DialogService);

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
    var name = this.users()?.find((user) => user.id === id)?.email || '';

    const dialogRef = this.dialogService.open(ConfirmDialog, {
      height: 'auto',
      data: {
        title: this.translocoService.translate('users.confirmDeleteTitle'),
        message: this.translocoService.translate('users.confirmDeleteMessage', { name }),
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
    const dialogRef = this.dialogService.open<CreateInvitationResult>(InviteUser, {
      height: 'auto',
    });

    dialogRef.closed.subscribe((result) => {
      if (result) {
        this.openResultModal(result);
      }
    });
  }

  openResultModal(result: CreateInvitationResult) {
    this.dialogService.open<ShowUserInvitation, CreateInvitationResult, void>(ShowUserInvitation, {
      height: 'auto',
      data: result
    });
  }
}
