export interface Link {
  id: string;
  slug: string;
  destinationUrl: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateLinkRequest {
  destinationUrl: string;
  slug?: string;
}

export interface UpdateLinkRequest {
  destinationUrl: string;
}

export interface DailyClicks {
  date: string;
  clicks: number;
}

export interface LinkStats {
  linkId: string;
  totalClicks: number;
  daily: DailyClicks[];
}
