import { Component, signal, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/components/navbar/navbar.component';
import { CartService } from './shared/services/cart.service';
import { Toast } from 'primeng/toast';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, Toast],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('campus-eats-web');
}
