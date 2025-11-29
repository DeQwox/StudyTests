import { Component, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('ClientApp');

  currentYear: number = new Date().getUTCFullYear();
  isAuthenticated: boolean = true;
  isTeacher: boolean = true;
  username: string = 'Kikono';
}