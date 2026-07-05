export interface CreateIdeaRequest {
  name: string;
  uniqueValueProposition: string;
  problem: string;
  solution: string;
  competition?: string | null;
  incomeStrategy?: string | null;
  exitStrategy?: string | null;
  videoPitchUrl?: string | null;
}
