using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using BusinessIdea.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BusinessIdea.Infrastructure.Persistence;

/// <summary>
/// Populates the database with a large, realistic-looking mock dataset (users,
/// ideas, threaded comments and votes) so features like the infinite-scroll feed
/// can be exercised. Run it via <c>dotnet run seed</c>. Each run appends a fresh
/// batch (users get unique emails), so it is safe to run multiple times.
/// </summary>
public class DatabaseSeeder
{
    private const int UserCount = 40;
    private const int PostCount = 200;
    private const string SeedPassword = "Password123";

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly Random _random = new(20260703);

    public DatabaseSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Seeding {UserCount} users…", UserCount);
        var userIds = await CreateUsersAsync();

        _logger.LogInformation("Seeding {PostCount} ideas with comments and votes…", PostCount);
        await CreatePostsAsync(userIds);

        _logger.LogInformation(
            "Seed complete. Totals — users: {Users}, ideas: {Ideas}, comments: {Comments}, " +
            "post votes: {PostVotes}, comment votes: {CommentVotes}.",
            await _context.Users.CountAsync(),
            await _context.BusinessIdeas.CountAsync(),
            await _context.Comments.CountAsync(),
            await _context.PostVotes.CountAsync(),
            await _context.CommentVotes.CountAsync());
    }

    private async Task<List<string>> CreateUsersAsync()
    {
        var ids = new List<string>(UserCount);

        for (var i = 0; i < UserCount; i++)
        {
            var first = Pick(FirstNames);
            var last = Pick(LastNames);
            var handle = Guid.NewGuid().ToString("N")[..8];

            var user = new ApplicationUser
            {
                UserName = $"{first.ToLowerInvariant()}.{handle}@example.com",
                Email = $"{first.ToLowerInvariant()}.{handle}@example.com",
                EmailConfirmed = true,
                DisplayName = $"{first} {last}",
                AvatarId = Pick(AvatarIds),
            };

            var result = await _userManager.CreateAsync(user, SeedPassword);
            if (result.Succeeded)
            {
                ids.Add(user.Id);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to create seed user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        return ids;
    }

    private async Task CreatePostsAsync(IReadOnlyList<string> userIds)
    {
        if (userIds.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        for (var i = 0; i < PostCount; i++)
        {
            var authorId = Pick(userIds);
            var createdAt = now.AddMinutes(-_random.Next(0, 90 * 24 * 60)); // up to 90 days ago
            var noun = Pick(Nouns);

            var post = new BusinessIdeaPost
            {
                Name = $"{Pick(Adjectives)}{noun}",
                UniqueValueProposition =
                    $"The only {noun.ToLowerInvariant()} platform that {Pick(ValueProps)}.",
                Problem = $"Today, {Pick(Audiences)} struggle because {Pick(Problems)}.",
                Solution = $"We {Pick(Solutions)}, all in one place.",
                Competition = Maybe(0.7) ? $"Unlike {Pick(Competitors)}, we focus on {Pick(Focuses)}." : null,
                IncomeStrategy = Maybe(0.7) ? $"{Pick(IncomeModels)}." : null,
                ExitStrategy = Maybe(0.5) ? $"Acquisition by {Pick(Acquirers)} within 5 years." : null,
                VideoPitchUrl = Maybe(0.25) ? $"https://videos.example.com/pitch/{Guid.NewGuid():N}" : null,
                AuthorId = authorId,
                CreatedAtUtc = createdAt,
            };

            _context.BusinessIdeas.Add(post);
            AddVotes(userIds, post.Id, isComment: false, createdAt);
            AddComments(userIds, post, createdAt);

            // Flush periodically to keep the change tracker from growing unbounded.
            if ((i + 1) % 20 == 0)
            {
                await _context.SaveChangesAsync(default);
            }
        }

        await _context.SaveChangesAsync(default);
    }

    private void AddComments(IReadOnlyList<string> userIds, BusinessIdeaPost post, DateTimeOffset postCreatedAt)
    {
        var topLevelCount = _random.Next(0, 9);

        for (var c = 0; c < topLevelCount; c++)
        {
            var metric = (IdeaMetric)_random.Next(0, 8);
            var commentAt = postCreatedAt.AddMinutes(_random.Next(1, 5 * 24 * 60));

            var comment = new Comment
            {
                PostId = post.Id,
                AuthorId = Pick(userIds),
                Content = Pick(CommentTexts),
                TargetMetric = metric,
                CreatedAtUtc = commentAt,
            };
            _context.Comments.Add(comment);
            AddVotes(userIds, comment.Id, isComment: true, commentAt);

            // A few replies, inheriting the parent's metric.
            var replyCount = _random.Next(0, 4);
            for (var r = 0; r < replyCount; r++)
            {
                var replyAt = commentAt.AddMinutes(_random.Next(1, 2 * 24 * 60));
                var reply = new Comment
                {
                    PostId = post.Id,
                    ParentCommentId = comment.Id,
                    AuthorId = Pick(userIds),
                    Content = Pick(ReplyTexts),
                    TargetMetric = metric,
                    CreatedAtUtc = replyAt,
                };
                _context.Comments.Add(reply);
                AddVotes(userIds, reply.Id, isComment: true, replyAt);
            }
        }
    }

    private void AddVotes(IReadOnlyList<string> userIds, Guid targetId, bool isComment, DateTimeOffset after)
    {
        var maxVoters = isComment ? Math.Min(12, userIds.Count) : Math.Min(30, userIds.Count);
        var voterCount = _random.Next(0, maxVoters + 1);

        foreach (var userId in DistinctSample(userIds, voterCount))
        {
            // Skew toward upvotes for a realistic score distribution.
            var direction = _random.NextDouble() < 0.75 ? VoteDirection.Up : VoteDirection.Down;

            if (isComment)
            {
                _context.CommentVotes.Add(new CommentVote
                {
                    CommentId = targetId,
                    UserId = userId,
                    Direction = direction,
                    CreatedAtUtc = after,
                });
            }
            else
            {
                _context.PostVotes.Add(new PostVote
                {
                    PostId = targetId,
                    UserId = userId,
                    Direction = direction,
                    CreatedAtUtc = after,
                });
            }
        }
    }

    private IEnumerable<string> DistinctSample(IReadOnlyList<string> source, int count)
    {
        if (count <= 0)
        {
            yield break;
        }

        var indices = Enumerable.Range(0, source.Count).ToList();
        for (var i = 0; i < count && indices.Count > 0; i++)
        {
            var j = _random.Next(indices.Count);
            yield return source[indices[j]];
            indices.RemoveAt(j);
        }
    }

    private bool Maybe(double probability) => _random.NextDouble() < probability;

    private T Pick<T>(IReadOnlyList<T> items) => items[_random.Next(items.Count)];

    // --- Content pools --------------------------------------------------------

    // Must match the avatar catalog ids in the frontend (core/avatars.ts).
    private static readonly string[] AvatarIds =
    {
        "rocket", "bulb", "fox", "panda", "robot", "alien", "cat", "owl",
        "tiger", "koala", "dragon", "unicorn", "bear", "penguin", "frog", "octopus",
    };

    private static readonly string[] FirstNames =
    {
        "Ava", "Liam", "Noah", "Emma", "Mia", "Ethan", "Sofia", "Lucas", "Isla", "Leo",
        "Nina", "Marco", "Elena", "Hugo", "Zara", "Omar", "Ivy", "Felix", "Luna", "Theo",
        "Ana", "Ken", "Priya", "Diego", "Yuki", "Sara", "Ivan", "Maya", "Nikola", "Petra",
    };

    private static readonly string[] LastNames =
    {
        "Novak", "Ilic", "Petrov", "Kim", "Rossi", "Hansen", "Silva", "Adams", "Cohen", "Marin",
        "Popov", "Weber", "Costa", "Nakamura", "Ford", "Bauer", "Reed", "Vidal", "Stone", "Kraft",
    };

    private static readonly string[] Adjectives =
    {
        "Nimbus", "Quantum", "Bright", "Swift", "Green", "Bold", "Clever", "Prime", "Nova", "Zen",
        "Vivid", "Loop", "Peak", "Echo", "Flux", "Spark", "Orbit", "Pixel", "Hyper", "Solid",
    };

    private static readonly string[] Nouns =
    {
        "Notes", "Cart", "Health", "Flow", "Desk", "Wallet", "Farm", "Learn", "Fit", "Grid",
        "Chef", "Route", "Bank", "Care", "Nest", "Hub", "Lab", "Deck", "Path", "Forge",
    };

    private static readonly string[] ValueProps =
    {
        "organizes itself with AI", "cuts costs by 40%", "works fully offline",
        "pays creators instantly", "needs zero setup", "learns your habits",
        "connects local suppliers", "turns data into decisions", "runs on any device",
    };

    private static readonly string[] Audiences =
    {
        "small businesses", "students", "freelancers", "clinics", "farmers",
        "remote teams", "creators", "landlords", "restaurants", "nonprofits",
    };

    private static readonly string[] Problems =
    {
        "the tools are too expensive", "everything is scattered across apps",
        "setup takes weeks", "data lives in spreadsheets", "support is nonexistent",
        "onboarding is painful", "nothing works offline", "reporting is a nightmare",
    };

    private static readonly string[] Solutions =
    {
        "bundle scheduling, billing and analytics", "automate the boring parts",
        "sync everything in real time", "replace five apps with one",
        "give you templates that just work", "handle payments end to end",
    };

    private static readonly string[] Competitors =
    {
        "the big incumbents", "generic spreadsheets", "legacy software", "manual processes",
    };

    private static readonly string[] Focuses =
    {
        "speed", "simplicity", "privacy", "affordability", "the local market", "mobile-first design",
    };

    private static readonly string[] IncomeModels =
    {
        "Freemium with a $12/mo pro tier", "Flat monthly subscription",
        "Transaction fee of 1.5%", "Annual licenses for teams", "Usage-based pricing",
    };

    private static readonly string[] Acquirers =
    {
        "a major SaaS suite", "a fintech leader", "a productivity giant", "a retail platform",
    };

    private static readonly string[] CommentTexts =
    {
        "Love this idea — the market timing feels right.",
        "How do you plan to handle churn?",
        "The value proposition is strong, but pricing worries me.",
        "Have you validated this with real customers yet?",
        "This could work great in emerging markets.",
        "Competitors will copy this fast. What's the moat?",
        "Solid solution, but the problem might be niche.",
        "I'd pay for this today. Where do I sign up?",
        "The exit strategy seems optimistic to me.",
        "Great UVP. Distribution is going to be the hard part.",
    };

    private static readonly string[] ReplyTexts =
    {
        "Good point — we're tackling that with onboarding automation.",
        "Agreed, retention is the real challenge here.",
        "We ran 20 interviews; the demand is definitely there.",
        "Fair concern. The moat is the local supplier network.",
        "Thanks! Early access opens next month.",
        "Totally — distribution is our top priority now.",
        "That's exactly why we went mobile-first.",
    };
}
