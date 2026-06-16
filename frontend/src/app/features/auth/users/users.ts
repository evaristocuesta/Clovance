import { Component, inject, OnInit } from '@angular/core';
import { AuthService } from '@core/services/auth.service';

@Component({
  selector: 'app-users',
  imports: [],
  templateUrl: './users.html',
  styleUrl: './users.css',
})
export class Users implements OnInit {
  private readonly authService = inject(AuthService);

  ngOnInit(): void {
    this.authService.loadCurrentUser().subscribe({
      next: (user) => {
        console.log('Current user loaded:', user);
      },
      error: (error) => {
        console.error('Error loading current user:', error);
      }
    });
  }
}
