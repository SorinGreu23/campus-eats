export interface LoyaltyAccount {
  id: string;
  userId: string;
  pointsBalance: number;
  lifetimePoints: number;
  tier: LoyaltyTier;
  createdAt: Date;
  updatedAt: Date;
}

export interface LoyaltyTransaction {
  id: string;
  loyaltyAccountId: string;
  type: TransactionType;
  points: number;
  description: string;
  orderId?: string;
  rewardId?: string;
  createdAt: Date;
}

export interface Reward {
  id: string;
  name: string;
  description: string;
  pointsCost: number;
  discountValue?: number;
  imageUrl?: string;
  isActive: boolean;
  category: RewardCategory;
  expiryDays?: number;
}

export interface UserReward {
  id: string;
  userId: string;
  rewardId: string;
  reward?: Reward;
  redeemedAt: Date;
  expiresAt?: Date;
  isUsed: boolean;
  usedAt?: Date;
  orderId?: string;
}

export enum LoyaltyTier {
  Bronze = 'Bronze',
  Silver = 'Silver',
  Gold = 'Gold',
  Platinum = 'Platinum'
}

export enum TransactionType {
  Earned = 'Earned',
  Redeemed = 'Redeemed',
  Expired = 'Expired',
  Bonus = 'Bonus'
}

export enum RewardCategory {
  Discount = 'Discount',
  FreeMeal = 'FreeMeal',
  FreeItem = 'FreeItem',
  Upgrade = 'Upgrade',
  Special = 'Special'
}
