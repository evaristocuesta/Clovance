import { Component, inject, OnInit, signal } from '@angular/core';
import { UserInfo, CreateInvitationCommand, CreateInvitationResult } from '@core/models/auth.models';
import { AuthService } from '@core/services/auth.service';
import { TranslocoModule } from '@jsverse/transloco';
import { email, form, FormRoot, required, FormField } from "@angular/forms/signals";
import { firstValueFrom } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-users',
  imports: [TranslocoModule, FormRoot, FormField, DatePipe],
  templateUrl: './users.html',
  styleUrl: './users.css',
})
export class Users implements OnInit {
  
  readonly authService = inject(AuthService);
  errorMessage = signal('');
  isInviteModalOpen = signal(false);
  isResultModalOpen = signal(false);
  users = signal<UserInfo[]>([]);

  userInvitationCommand = signal<CreateInvitationCommand>({
    email: '',
    isAdmin: false
  });

  userInvitationResult = signal<CreateInvitationResult>({
    id: '',
    email: '',
    expiresAt: new Date(),
    token: ''
  });

  ngOnInit(): void {
    this.loadUsers();
  }

  sendInvitationForm = form(
    this.userInvitationCommand,
    (schemaPath) => {
      required(schemaPath.email, { message: 'users.emailRequired' });
      email(schemaPath.email, { message: 'users.emailInvalid' });
    },
    {
      submission: {
        action: async (field) => {  
          this.errorMessage.set('');
          
          try {
            var result = await firstValueFrom(this.authService.sendInvitation(field().value()));
            this.userInvitationResult.set(result);
            this.closeInviteUserModal();
            this.openResultModal();
          } catch (err: HttpErrorResponse | any) {
            const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
            const key = errorCode ? errorCode : 'login.serverError';
            this.errorMessage.set(key);
          }
        },
      },
    }
  );

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

  deleteUser(id: string) {
    this.authService.deleteUser(id).subscribe({
      next: () => {
        this.loadUsers();
      },
      error: (error) => {
        console.error('Error deleting user:', error);
      }
    });
  }

  openInviteUserModal() {
    this.sendInvitationForm().reset({
      email: '',
      isAdmin: false
    });
    this.errorMessage.set('');
    this.isInviteModalOpen.set(true);
  }

  closeInviteUserModal() {
    this.sendInvitationForm().reset();
    this.isInviteModalOpen.set(false);
  }

  openResultModal() {
    this.isResultModalOpen.set(true);
  }

  closeResultModal() {
    this.isResultModalOpen.set(false);
  }
}
