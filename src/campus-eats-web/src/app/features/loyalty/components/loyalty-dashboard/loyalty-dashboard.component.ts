import { Component, inject, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoyaltyService } from '../../services/loyalty.service';
import { RewardCardComponent } from '../reward-card/reward-card.component';
import { TransactionType } from '../../models/loyalty.model';

@Component({
  selector: 'app-loyalty-dashboard',
  imports: [CommonModule, RewardCardComponent],
  templateUrl: './loyalty-dashboard.component.html',
  styleUrl: './loyalty-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoyaltyDashboardComponent {
  loyaltyService = inject(LoyaltyService);
  
  TransactionType = TransactionType;

  tierProgress = computed(() => this.loyaltyService.getTierProgress());
  tierColor = computed(() => this.loyaltyService.getTierColor(this.loyaltyService.tier()));
  tierIcon = computed(() => this.loyaltyService.getTierIcon(this.loyaltyService.tier()));

  handleRedeemReward(rewardId: string): void {
    const success = this.loyaltyService.redeemReward(rewardId);
    if (!success) {
      alert('Unable to redeem reward. Please check your points balance.');
    }
  }
}
