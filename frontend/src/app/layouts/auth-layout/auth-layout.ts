import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { initFlowbite } from 'flowbite';

@Component({
  selector: 'app-auth-layout',
  imports: [RouterOutlet],
  templateUrl: './auth-layout.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './auth-layout.css',
})
export class AuthLayout implements OnInit {
  ngOnInit(): void {
    // Initialize Flowbite after layout is rendered
    setTimeout(() => {
      initFlowbite();
    }, 0); // Initialization logic here
  }
}
