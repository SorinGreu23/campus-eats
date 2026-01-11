import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  LoyaltyAccount, 
  LoyaltyTransaction, 
  Reward, 
  UserReward,
  LoyaltyTier,
  TransactionType,
  RewardCategory
} from '../models/loyalty.model';
import { AuthStateService } from '../../../shared/services/auth-state.service';

const API_BASE_URL = 'http://localhost:5001/api';

@Injectable({
  providedIn: 'root'
})
export class LoyaltyService {
  private http = inject(HttpClient);
  private authState = inject(AuthStateService);

  private loyaltyAccount = signal<LoyaltyAccount | null>(null);
  private transactions = signal<LoyaltyTransaction[]>([]);
  private availableRewards = signal<Reward[]>([]);
  private userRewards = signal<UserReward[]>([]);

  // Public computed signals
  account = this.loyaltyAccount.asReadonly();
  pointsBalance = computed(() => this.loyaltyAccount()?.pointsBalance ?? 0);
  tier = computed(() => this.loyaltyAccount()?.tier ?? LoyaltyTier.Bronze);
  recentTransactions = computed(() => 
    this.transactions().slice().sort((a, b) => 
      new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    ).slice(0, 10)
  );
  rewards = this.availableRewards.asReadonly();
  myRewards = computed(() => 
    this.userRewards().filter(r => !r.isUsed).sort((a, b) =>
      new Date(b.redeemedAt).getTime() - new Date(a.redeemedAt).getTime()
    )
  );

  constructor() {
    // Load data from API when service is initialized
    this.loadLoyaltyData();
  }

  private loadLoyaltyData(): void {
    const userId = this.authState.userId();
    console.log('Loading loyalty data for user:', userId);
    
    // Always fetch available rewards (doesn't require user login)
    this.fetchRewards().subscribe({
      next: (rewards) => {
        console.log('Raw rewards from API:', rewards);
        // Map API response to frontend Reward model
        const mappedRewards = rewards.map(r => ({
          id: r.id,
          name: r.name,
          description: r.description || '',
          pointsCost: r.pointsCost,
          discountValue: r.discountValue,
          imageUrl: this.getRewardImageUrl(r.name),
          isActive: r.isActive,
          category: this.getRewardCategory(r.name, r.discountValue),
          expiryDays: 30
        }));
        
        // Remove duplicates based on name and pointsCost
        const uniqueRewards = mappedRewards.filter((reward, index, self) =>
          index === self.findIndex((r) => (
            r.name === reward.name && r.pointsCost === reward.pointsCost
          ))
        );
        
        console.log('Mapped rewards (duplicates removed):', uniqueRewards);
        this.availableRewards.set(uniqueRewards);
      },
      error: (err) => {
        console.error('Failed to fetch rewards:', err);
      }
    });

    // Only fetch user-specific data if logged in
    if (!userId) {
      console.warn('No user ID found, skipping user-specific loyalty data');
      return;
    }

    // Fetch loyalty account
    this.fetchLoyaltyAccount(userId).subscribe({
      next: (account) => {
        console.log('Loyalty account loaded:', account);
        this.loyaltyAccount.set(account);
      },
      error: (err) => {
        console.error('Failed to load loyalty account:', err);
      }
    });

    // Fetch user's claimed rewards
    this.fetchClaimedRewards(userId).subscribe({
      next: (claims) => {
        console.log('Claimed rewards loaded:', claims);
        this.userRewards.set(claims);
      },
      error: (err) => {
        console.error('Failed to load claimed rewards:', err);
      }
    });
  }

  fetchLoyaltyAccount(userId: string): Observable<any> {
    const token = this.authState.token();
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;
    return this.http.get(`${API_BASE_URL}/loyalty/account/${userId}`, { headers });
  }

  fetchRewards(): Observable<any[]> {
    const token = this.authState.token();
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;
    return this.http.get<any[]>(`${API_BASE_URL}/loyalty/rewards`, { headers });
  }

  fetchClaimedRewards(userId: string): Observable<UserReward[]> {
    const token = this.authState.token();
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;
    return this.http.get<UserReward[]>(`${API_BASE_URL}/loyalty/claims/${userId}`, { headers });
  }

  redeemReward(rewardId: string): void {
    const reward = this.availableRewards().find(r => r.id === rewardId);
    const account = this.loyaltyAccount();
    
    if (!reward || !account) {
      alert('Unable to redeem reward. Please try again.');
      return;
    }
    
    if (account.pointsBalance < reward.pointsCost) {
      alert(`Insufficient points. You need ${reward.pointsCost} points but have ${account.pointsBalance}.`);
      return;
    }

    const token = this.authState.token();
    if (!token) {
      alert('Please sign in to redeem rewards.');
      return;
    }

    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
    
    // Call the API to redeem the reward
    this.http.post(`${API_BASE_URL}/loyalty/redeem`, 
      { userId: account.userId, rewardId: reward.id },
      { headers }
    ).subscribe({
      next: () => {
        // Refresh the loyalty account to get updated points
        this.fetchLoyaltyAccount(account.userId).subscribe({
          next: (updatedAccount) => {
            this.loyaltyAccount.set(updatedAccount);
            // Also refresh claimed rewards
            this.fetchClaimedRewards(account.userId).subscribe({
              next: (claims) => {
                this.userRewards.set(claims);
                alert(`Successfully redeemed ${reward.name}!`);
              },
              error: (err) => {
                console.error('Failed to refresh claimed rewards:', err);
                alert(`Successfully redeemed ${reward.name}!`);
              }
            });
          },
          error: (err) => {
            console.error('Failed to refresh account:', err);
            alert('Reward redeemed but failed to refresh account. Please refresh the page.');
          }
        });
      },
      error: (err) => {
        console.error('Failed to redeem reward:', err);
        alert('Failed to redeem reward. Please try again.');
      }
    });
  }

  getTierProgress(): { current: number; next: number; percentage: number } {
    const lifetimePoints = this.loyaltyAccount()?.lifetimePoints ?? 0;
    const tier = this.tier();

    const thresholds = {
      [LoyaltyTier.Bronze]: { min: 0, max: 2000 },
      [LoyaltyTier.Silver]: { min: 2000, max: 5000 },
      [LoyaltyTier.Gold]: { min: 5000, max: 10000 },
      [LoyaltyTier.Platinum]: { min: 10000, max: 10000 }
    };

    const threshold = thresholds[tier];
    const current = lifetimePoints - threshold.min;
    const next = threshold.max - threshold.min;
    const percentage = tier === LoyaltyTier.Platinum ? 100 : (current / next) * 100;

    return { current, next, percentage };
  }

  getTierColor(tier: LoyaltyTier): string {
    const colors = {
      [LoyaltyTier.Bronze]: '#cd7f32',
      [LoyaltyTier.Silver]: '#c0c0c0',
      [LoyaltyTier.Gold]: '#ffd700',
      [LoyaltyTier.Platinum]: '#e5e4e2'
    };
    return colors[tier];
  }

  getTierIcon(tier: LoyaltyTier): string {
    const icons = {
      [LoyaltyTier.Bronze]: 'pi-star',
      [LoyaltyTier.Silver]: 'pi-star-fill',
      [LoyaltyTier.Gold]: 'pi-crown',
      [LoyaltyTier.Platinum]: 'pi-sparkles'
    };
    return icons[tier];
  }

  getRewardImageUrl(rewardName: string): string {
    const name = rewardName.toLowerCase();
    
    if (name.includes('coffee')) {
      return 'https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=400';
    } else if (name.includes('sandwich')) {
      return 'https://images.unsplash.com/photo-1528735602780-2552fd46c7af?w=400';
    } else if (name.includes('dessert')) {
      return 'https://images.unsplash.com/photo-1551024506-0bccd828d307?w=400';
    } else if (name.includes('meal')) {
      return 'https://images.unsplash.com/photo-1504754524776-8f4f37790ca0?w=400';
    } else if (name.includes('pizza')) {
      return 'https://images.unsplash.com/photo-1513104890138-7c749659a591?w=400';
    } else if (name.includes('burger')) {
      return 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400';
    } else {
      return 'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400';
    }
  }

  private getRewardCategory(rewardName: string, discountValue?: number): RewardCategory {
    const name = rewardName.toLowerCase();
    
    if (name.includes('free') && (name.includes('meal') || name.includes('lunch') || name.includes('dinner'))) {
      return RewardCategory.FreeMeal;
    } else if (name.includes('free')) {
      return RewardCategory.FreeItem;
    } else if (name.includes('upgrade')) {
      return RewardCategory.Upgrade;
    } else if (name.includes('special') || name.includes('exclusive')) {
      return RewardCategory.Special;
    } else if (discountValue || name.includes('%') || name.includes('discount') || name.includes('off')) {
      return RewardCategory.Discount;
    }
    
    return RewardCategory.Discount;
  }
}
