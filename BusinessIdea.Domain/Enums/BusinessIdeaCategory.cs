namespace BusinessIdea.Domain.Enums;

/// <summary>
/// The category (or categories) a business idea belongs to. An idea may carry
/// up to 3 categories at once (enforced by the command validator).
/// </summary>
public enum BusinessIdeaCategory : byte
{
    Tech = 0,
    Fintech = 2,
    Health = 3,
    Food = 4,
    Education = 5,
    Ecommerce = 6,
    Entertainment = 7,
    Sustainability = 9
}

