export interface TestSummary {
  id: number;
  name: string;
  description: string;
  teacherName: string;
  teacherId: string;
  maxScore: number;
  passed?: {
    score: number;
    passedAt: string;
  };
}

// Keeping these for other components you might use later
export interface TestDetail extends TestSummary {
  questionCount: number;
  createdAt: string;
  validUntil?: string;
  isOwner?: boolean;
}

export interface TestCreate {
  name: string;
  description: string;
  password?: string;
  validUntil?: string;
  questions: any[];
}