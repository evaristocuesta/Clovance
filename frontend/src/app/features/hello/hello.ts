import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-hello',
  imports: [],
  templateUrl: './hello.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './hello.css',
})
export class Hello {}
