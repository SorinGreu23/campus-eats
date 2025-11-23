import { MenuItem } from '../../features/menu/models/menu-item.model';

export interface CartItem {
  menuItem: MenuItem;
  quantity: number;
}
