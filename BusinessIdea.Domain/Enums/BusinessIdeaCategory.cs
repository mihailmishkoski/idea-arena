namespace BusinessIdea.Domain.Enums;

/// <summary>
/// The category (or categories) a business idea belongs to. An idea may carry
/// up to 3 categories at once (enforced by the command validator), letting an
/// author tag an idea that genuinely spans more than one space — e.g. a
/// restaurant payments app is both Food and Fintech — without turning tagging
/// into a free-for-all.
/// </summary>
public enum BusinessIdeaCategory : byte
{
    Tech = 0,
    SaaS = 1,
    Fintech = 2,
    Health = 3,
    Food = 4,
    Education = 5,
    Ecommerce = 6,
    Entertainment = 7,
    Travel = 8,
    Sustainability = 9,
    Agriculture = 10,
    RealEstate = 11,
    Gaming = 12,
    Legal = 13
}