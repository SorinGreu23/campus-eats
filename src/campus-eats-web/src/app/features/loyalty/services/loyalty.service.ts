import { Injectable, signal, computed } from '@angular/core';
import { 
  LoyaltyAccount, 
  LoyaltyTransaction, 
  Reward, 
  UserReward,
  LoyaltyTier,
  TransactionType,
  RewardCategory
} from '../models/loyalty.model';

@Injectable({
  providedIn: 'root'
})
export class LoyaltyService {
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
    this.initializeMockData();
  }

  private initializeMockData(): void {
    // Mock loyalty account
    const account: LoyaltyAccount = {
      id: '1',
      userId: 'user-1',
      pointsBalance: 450,
      lifetimePoints: 1250,
      tier: LoyaltyTier.Silver,
      createdAt: new Date('2024-01-15'),
      updatedAt: new Date()
    };
    this.loyaltyAccount.set(account);

    // Mock transactions
    const mockTransactions: LoyaltyTransaction[] = [
      {
        id: 't-1',
        loyaltyAccountId: '1',
        type: TransactionType.Earned,
        points: 50,
        description: 'Order #ORD-2024-1234',
        orderId: 'ord-1',
        createdAt: new Date('2024-11-20T14:30:00')
      },
      {
        id: 't-2',
        loyaltyAccountId: '1',
        type: TransactionType.Redeemed,
        points: -100,
        description: 'Redeemed: Free Coffee',
        rewardId: 'r-1',
        createdAt: new Date('2024-11-18T10:15:00')
      },
      {
        id: 't-3',
        loyaltyAccountId: '1',
        type: TransactionType.Earned,
        points: 75,
        description: 'Order #ORD-2024-1200',
        orderId: 'ord-2',
        createdAt: new Date('2024-11-15T12:00:00')
      },
      {
        id: 't-4',
        loyaltyAccountId: '1',
        type: TransactionType.Bonus,
        points: 200,
        description: 'Welcome Bonus',
        createdAt: new Date('2024-11-01T09:00:00')
      }
    ];
    this.transactions.set(mockTransactions);

    // Mock available rewards
    const mockRewards: Reward[] = [
      {
        id: 'r-1',
        name: 'Free Coffee',
        description: 'Get any coffee size on the house',
        pointsCost: 100,
        imageUrl: 'https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=400',
        isActive: true,
        category: RewardCategory.FreeItem,
        expiryDays: 30
      },
      {
        id: 'r-2',
        name: '10% Off Order',
        description: 'Get 10% discount on your next order',
        pointsCost: 150,
        imageUrl: 'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400',
        isActive: true,
        category: RewardCategory.Discount,
        expiryDays: 30
      },
      {
        id: 'r-3',
        name: 'Free Sandwich',
        description: 'Choose any sandwich from our menu',
        pointsCost: 300,
        imageUrl: 'https://images.unsplash.com/photo-1528735602780-2552fd46c7af?w=400',
        isActive: true,
        category: RewardCategory.FreeMeal,
        expiryDays: 14
      },
      {
        id: 'r-4',
        name: '20% Off Order',
        description: 'Get 20% discount on your next order',
        pointsCost: 250,
        imageUrl: 'https://images.unsplash.com/photo-1504754524776-8f4f37790ca0?w=400',
        isActive: true,
        category: RewardCategory.Discount,
        expiryDays: 30
      },
      {
        id: 'r-5',
        name: 'Free Dessert',
        description: 'Get any dessert from our menu for free',
        pointsCost: 200,
        imageUrl: 'https://images.unsplash.com/photo-1551024506-0bccd828d307?w=400',
        isActive: true,
        category: RewardCategory.FreeItem,
        expiryDays: 30
      },
      {
        id: 'r-6',
        name: 'Meal Upgrade',
        description: 'Upgrade any meal to premium size',
        pointsCost: 120,
        imageUrl: 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400',
        isActive: true,
        category: RewardCategory.Upgrade,
        expiryDays: 30
      }
    ];
    this.availableRewards.set(mockRewards);

    // Mock user rewards
    const mockUserRewards: UserReward[] = [
      {
        id: 'ur-1',
        userId: 'user-1',
        rewardId: 'r-2',
        reward: mockRewards[1],
        redeemedAt: new Date('2024-11-22T10:00:00'),
        expiresAt: new Date('2024-12-22T23:59:59'),
        isUsed: false
      }
    ];
    this.userRewards.set(mockUserRewards);
  }

  redeemReward(rewardId: string): boolean {
    const reward = this.availableRewards().find(r => r.id === rewardId);
    const account = this.loyaltyAccount();
    
    if (!reward || !account) return false;
    if (account.pointsBalance < reward.pointsCost) return false;

    // Deduct points
    const updatedAccount = {
      ...account,
      pointsBalance: account.pointsBalance - reward.pointsCost,
      updatedAt: new Date()
    };
    this.loyaltyAccount.set(updatedAccount);

    // Add transaction
    const transaction: LoyaltyTransaction = {
      id: `t-${Date.now()}`,
      loyaltyAccountId: account.id,
      type: TransactionType.Redeemed,
      points: -reward.pointsCost,
      description: `Redeemed: ${reward.name}`,
      rewardId: reward.id,
      createdAt: new Date()
    };
    this.transactions.update(t => [transaction, ...t]);

    // Add to user rewards
    const expiresAt = reward.expiryDays 
      ? new Date(Date.now() + reward.expiryDays * 24 * 60 * 60 * 1000)
      : undefined;

    const userReward: UserReward = {
      id: `ur-${Date.now()}`,
      userId: account.userId,
      rewardId: reward.id,
      reward: reward,
      redeemedAt: new Date(),
      expiresAt,
      isUsed: false
    };
    this.userRewards.update(r => [userReward, ...r]);

    return true;
  }

  getTierProgress(): { current: number; next: number; percentage: number } {
    const lifetimePoints = this.loyaltyAccount()?.lifetimePoints ?? 0;
    const tier = this.tier();

    const thresholds = {
      [LoyaltyTier.Bronze]: { min: 0, max: 500 },
      [LoyaltyTier.Silver]: { min: 500, max: 1500 },
      [LoyaltyTier.Gold]: { min: 1500, max: 3000 },
      [LoyaltyTier.Platinum]: { min: 3000, max: 3000 }
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
}
