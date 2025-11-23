import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Reward } from '../../models/loyalty.model';

@Component({
  selector: 'app-reward-card',
  imports: [CommonModule],
  templateUrl: './reward-card.component.html',
  styleUrl: './reward-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RewardCardComponent {
  reward = input.required<Reward>();
  userPoints = input.required<number>();
  redeem = output<string>();

  canAfford(): boolean {
    return this.userPoints() >= this.reward().pointsCost;
  }

  onRedeem(): void {
    if (this.canAfford()) {
      this.redeem.emit(this.reward().id);
    }
  }
}
