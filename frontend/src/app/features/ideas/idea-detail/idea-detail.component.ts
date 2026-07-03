import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin, Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { BusinessIdeaDetail } from '../../../core/models/business-idea.model';
import { ChatRequestStatus, ConversationDto } from '../../../core/models/chat.model';
import { CommentDto, CommentNode, CreateCommentRequest } from '../../../core/models/comment.model';
import { IdeaMetric, VoteDirection } from '../../../core/models/enums';
import {
  CommentReplyEvent,
  CommentVoteEvent,
} from '../components/comment-item/comment-item.component';
import { AuthService } from '../../../core/services/auth.service';
import { ChatService } from '../../../core/services/chat.service';
import { CommentsService } from '../../../core/services/comments.service';
import { IdeasService } from '../../../core/services/ideas.service';
import { VotesService } from '../../../core/services/votes.service';

interface MetricFilter {
  label: string;
  value: IdeaMetric | null;
}

@Component({
  selector: 'app-idea-detail',
  templateUrl: './idea-detail.component.html',
  styleUrls: ['./idea-detail.component.scss'],
})
export class IdeaDetailComponent implements OnInit, OnDestroy {
  idea: BusinessIdeaDetail | null = null;
  tree: CommentNode[] = [];

  loading = false;
  loadingComments = false;
  error = false;
  postingComment = false;

  filterMetric: IdeaMetric | null = null;
  currentUserId: string | null = null;

  // Co-founder application state
  showCofoundModal = false;
  applyingCofound = false;
  cofoundApplied = false;
  cofoundError: string | null = null;
  cofound = { role: '', skills: '', motivation: '', availability: '', contactLink: '' };

  /** What already exists between me and the author (drives the CTA). */
  cofoundState: 'none' | 'pending' | 'accepted' = 'none';
  existingConversationId: string | null = null;

  private conversationsSnapshot: ConversationDto[] = [];

  readonly Metric = IdeaMetric;
  readonly filters: MetricFilter[] = [
    { label: 'All', value: null },
    { label: 'General', value: IdeaMetric.General },
    { label: 'Unique Value', value: IdeaMetric.UniqueValueProposition },
    { label: 'Problem', value: IdeaMetric.Problem },
    { label: 'Solution', value: IdeaMetric.Solution },
    { label: 'Competition', value: IdeaMetric.Competition },
    { label: 'Income', value: IdeaMetric.IncomeStrategy },
    { label: 'Exit', value: IdeaMetric.ExitStrategy },
    { label: 'Video', value: IdeaMetric.VideoPitch },
  ];

  private ideaId = '';
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly ideasService: IdeasService,
    private readonly commentsService: CommentsService,
    private readonly votesService: VotesService,
    private readonly chatService: ChatService,
    private readonly auth: AuthService
  ) {}

  ngOnInit(): void {
    this.ideaId = this.route.snapshot.paramMap.get('id') ?? '';

    this.auth.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe((user) => (this.currentUserId = user?.id ?? null));

    // Keep the co-founder CTA in sync with what already exists between the
    // current user and the author (pending request, open conversation…).
    this.chatService.conversations$
      .pipe(takeUntil(this.destroy$))
      .subscribe((list) => {
        this.conversationsSnapshot = list;
        this.updateCofoundState();
      });

    this.fetchData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get isAuthenticated(): boolean {
    return this.currentUserId !== null;
  }

  get isAuthor(): boolean {
    return !!this.idea && this.idea.authorId === this.currentUserId;
  }

  /** Top-level comments visible under the current topic filter. */
  get visibleTree(): CommentNode[] {
    if (this.filterMetric === null) {
      return this.tree;
    }
    return this.tree.filter((c) => c.targetMetric === this.filterMetric);
  }

  setFilter(metric: IdeaMetric | null): void {
    this.filterMetric = metric;
  }

  onVoteIdea(direction: VoteDirection): void {
    if (!this.requireAuth() || !this.idea) {
      return;
    }

    this.votesService
      .voteOnIdea(this.idea.id, direction)
      .pipe(takeUntil(this.destroy$))
      .subscribe((result) => {
        if (!this.idea) {
          return;
        }
        this.idea.upVotes = result.upVotes;
        this.idea.downVotes = result.downVotes;
        this.idea.score = result.score;
        this.idea.currentUserVote = result.currentUserVote;
      });
  }

  onVoteComment(event: CommentVoteEvent): void {
    if (!this.requireAuth()) {
      return;
    }

    this.votesService
      .voteOnComment(event.comment.id, event.direction)
      .pipe(takeUntil(this.destroy$))
      .subscribe((result) => {
        const node = event.comment as CommentNode;
        node.upVotes = result.upVotes;
        node.downVotes = result.downVotes;
        node.score = result.score;
        node.currentUserVote = result.currentUserVote;
      });
  }

  onSubmitComment(request: CreateCommentRequest): void {
    if (!this.requireAuth()) {
      return;
    }

    this.postingComment = true;
    this.commentsService
      .createComment(this.ideaId, request)
      .pipe(
        finalize(() => (this.postingComment = false)),
        takeUntil(this.destroy$)
      )
      .subscribe(() => this.fetchComments());
  }

  onReply(event: CommentReplyEvent): void {
    if (!this.requireAuth()) {
      return;
    }

    this.commentsService
      .createComment(this.ideaId, {
        content: event.content,
        targetMetric: event.parent.targetMetric,
        parentCommentId: event.parent.id,
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.fetchComments());
  }

  onDeleteComment(comment: CommentDto): void {
    this.commentsService
      .deleteComment(comment.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.fetchComments());
  }

  /** Sends (or reuses) a plain chat request to the idea's author. */
  onChatWithAuthor(): void {
    if (!this.requireAuth() || !this.idea) {
      return;
    }

    this.chatService
      .requestChat(this.idea.authorId)
      .pipe(takeUntil(this.destroy$))
      .subscribe((conversationId) =>
        this.router.navigate(['/messages', conversationId])
      );
  }

  openCofoundModal(): void {
    if (!this.requireAuth()) {
      return;
    }
    this.cofoundError = null;
    this.showCofoundModal = true;
  }

  closeCofoundModal(): void {
    this.showCofoundModal = false;
  }

  /** Submits the application; the pitch fields travel to the author's email. */
  onApplyCofound(): void {
    if (!this.requireAuth() || !this.idea || this.applyingCofound) {
      return;
    }

    this.applyingCofound = true;
    this.cofoundError = null;

    this.chatService
      .applyCofound({
        postId: this.idea.id,
        role: this.cofound.role.trim() || null,
        skills: this.cofound.skills.trim() || null,
        motivation: this.cofound.motivation.trim() || null,
        availability: this.cofound.availability.trim() || null,
        contactLink: this.cofound.contactLink.trim() || null,
      })
      .pipe(
        finalize(() => (this.applyingCofound = false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => {
          this.cofoundApplied = true;
          this.showCofoundModal = false;
          // Refresh the conversation store so the CTA stays locked.
          this.chatService.load().subscribe();
        },
        error: (err) => {
          this.cofoundError =
            err?.error?.errors?.PostId?.[0] ??
            err?.error?.errors?.RecipientId?.[0] ??
            'The application could not be sent. Please try again.';
        },
      });
  }

  /** Derives the CTA state from the loaded conversations. */
  private updateCofoundState(): void {
    const authorId = this.idea?.authorId;
    if (!authorId) {
      return;
    }

    const existing = this.conversationsSnapshot.find(
      (c) => c.otherUserId === authorId && c.status !== ChatRequestStatus.Declined
    );

    this.existingConversationId = existing?.id ?? null;
    this.cofoundState =
      existing?.status === ChatRequestStatus.Pending
        ? 'pending'
        : existing?.status === ChatRequestStatus.Accepted
          ? 'accepted'
          : 'none';
  }

  onDeleteIdea(): void {
    if (!this.idea || !confirm('Delete this idea? This cannot be undone.')) {
      return;
    }

    this.ideasService
      .deleteIdea(this.idea.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.router.navigate(['/']));
  }

  trackByCommentId(_index: number, comment: CommentNode): string {
    return comment.id;
  }

  private fetchData(): void {
    this.loading = true;
    this.error = false;

    forkJoin({
      idea: this.ideasService.getIdea(this.ideaId),
      comments: this.commentsService.getComments(this.ideaId),
    })
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (result) => {
          this.idea = result.idea;
          this.setComments(result.comments);
          this.updateCofoundState();
        },
        error: () => (this.error = true),
      });
  }

  private fetchComments(): void {
    this.loadingComments = true;
    this.commentsService
      .getComments(this.ideaId)
      .pipe(
        finalize(() => (this.loadingComments = false)),
        takeUntil(this.destroy$)
      )
      .subscribe((comments) => this.setComments(comments));
  }

  private setComments(flat: CommentDto[]): void {
    this.tree = this.buildTree(flat);
    if (this.idea) {
      this.idea.commentCount = flat.length;
    }
  }

  /** Turns the flat comment list into a sorted, threaded tree. */
  private buildTree(flat: CommentDto[]): CommentNode[] {
    const byId = new Map<string, CommentNode>();
    flat.forEach((c) => byId.set(c.id, { ...c, replies: [], depth: 0 }));

    const roots: CommentNode[] = [];
    flat.forEach((c) => {
      const node = byId.get(c.id)!;
      const parent = c.parentCommentId ? byId.get(c.parentCommentId) : undefined;
      if (parent) {
        node.depth = parent.depth + 1;
        parent.replies.push(node);
      } else {
        roots.push(node);
      }
    });

    const createdMs = (c: CommentNode): number => new Date(c.createdAtUtc).getTime();

    // Highest score first (like posts); newest breaks ties. Applied to replies
    // recursively as well as top-level comments.
    const byScoreThenNew = (a: CommentNode, b: CommentNode): number =>
      b.score - a.score || createdMs(b) - createdMs(a);

    const sortRecursive = (node: CommentNode): void => {
      node.replies.sort(byScoreThenNew);
      node.replies.forEach(sortRecursive);
    };

    roots.sort(byScoreThenNew);
    roots.forEach(sortRecursive);
    return roots;
  }

  private requireAuth(): boolean {
    if (this.isAuthenticated) {
      return true;
    }
    this.router.navigate(['/auth/login']);
    return false;
  }
}
