import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { initFlowbite } from 'flowbite';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet],
  templateUrl: './main-layout.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './main-layout.css',
})
export class MainLayout implements OnInit {
  ngOnInit(): void {
    // Initialize Flowbite after layout is rendered
    setTimeout(() => {
      initFlowbite();
    }, 0); // Initialization logic here
  }
}
