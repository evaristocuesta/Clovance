import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { form, maxLength, required, email, FormRoot, FormField } from '@angular/forms/signals';
import { UpdateUserRequest } from '@core/models/auth.models';
import { AuthService } from '@core/services/auth.service';
import { TranslocoDirective } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';

@Component({
  selector: 'app-update-user',
  imports: [TranslocoDirective, FormField, FormRoot],
  templateUrl: './update-user.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './update-user.css',
})
export class UpdateUser implements OnInit {
  errorMessage = signal('');

  updateUserRequest = signal<UpdateUserRequest>({
    firstName: '',
    lastName: '',
    email: ''
  });

  private readonly authService = inject(AuthService);

  ngOnInit() {
    
    this.authService.loadCurrentUser().subscribe({
      next: (user) => {
        const currentUser = this.authService.getCurrentUser();
    
        if (currentUser) {
          this.updateUserRequest.set({
            firstName: currentUser.firstName,
            lastName: currentUser.lastName,
            email: currentUser.email
          });
        }  
      }
    });
  }

  updateUserForm = form(
    this.updateUserRequest,
    (schemaPath) => {
      maxLength(schemaPath.firstName, 100, { message: 'updateUser.firstNameMaxLength' });
      required(schemaPath.firstName, { message: 'updateUser.firstNameRequired' });
      maxLength(schemaPath.lastName, 100, { message: 'updateUser.lastNameMaxLength' });
      required(schemaPath.lastName, { message: 'updateUser.lastNameRequired' });
      required(schemaPath.email, { message: 'updateUser.emailRequired' });
      email(schemaPath.email, { message: 'updateUser.emailInvalid' });
    }, 
    {
      submission: {
        action: async (field) => {  
          this.errorMessage.set('');

          try {
            await firstValueFrom(this.authService.updateUser(field().value()));
          } catch (err: HttpErrorResponse | any) {
            const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
            const key = errorCode ? errorCode : 'updateUser.serverError';
            this.errorMessage.set(key);
          }
        },
      },
    },
  );
}