import { Component, Input } from '@angular/core';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { MenuItem } from '../../models/menu-item.model';

@Component({
  selector: 'app-menu-item-card',
  standalone: true,
  imports: [CardModule, ButtonModule, TagModule],
  templateUrl: './menu-item-card.component.html',
  styleUrl: './menu-item-card.component.scss'
})
export class MenuItemCardComponent {
  @Input({ required: true }) menuItem!: MenuItem;

  addToCart() {
    // TODO: Implement cart functionality
    console.log('Adding to cart:', this.menuItem.name);
  }
}
