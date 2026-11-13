using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;
using StemDialogue.Domain.News;
using StemDialogue.Domain.People;
using StemDialogue.Domain.Projects;
using StemDialogue.Domain.Resources;

namespace StemDialogue.Application.Common;

/// <summary>Georgian/English labels for enum values shown on the public site and in admin forms.</summary>
public static class EnumLabels
{
    private static readonly Dictionary<Enum, (string Ka, string En)> Labels = new()
    {
        [NewsCategory.Announcement] = ("განცხადება", "Announcement"),
        [NewsCategory.EventAnnouncement] = ("შეხვედრის ანონსი", "Event announcement"),
        [NewsCategory.EventSummary] = ("შეხვედრის შეჯამება", "Event summary"),
        [NewsCategory.Opportunity] = ("კონკურსი / შესაძლებლობა", "Competition / opportunity"),
        [NewsCategory.Achievement] = ("მიღწევა / შედეგი", "Achievement / result"),
        [NewsCategory.General] = ("ზოგადი სიახლე", "General news"),

        [EventType.StemSaturday] = ("STEM შაბათობა", "STEM Saturday"),
        [EventType.Workshop] = ("ვორქშოპი", "Workshop"),
        [EventType.Lecture] = ("ლექცია", "Lecture"),
        [EventType.Forum] = ("ფორუმი", "Forum"),
        [EventType.FinalEvent] = ("ფინალური ღონისძიება", "Final event"),
        [EventType.Other] = ("სხვა", "Other"),

        [EventFormat.Online] = ("ონლაინ", "Online"),
        [EventFormat.InPerson] = ("ფიზიკური", "In person"),
        [EventFormat.Hybrid] = ("ჰიბრიდული", "Hybrid"),

        [EventStatus.Planned] = ("დაგეგმილი", "Planned"),
        [EventStatus.Postponed] = ("გადატანილი", "Postponed"),
        [EventStatus.Completed] = ("ჩატარებული", "Completed"),
        [EventStatus.Cancelled] = ("გაუქმებული", "Cancelled"),

        [EventParticipantRole.Speaker] = ("მომხსენებელი", "Speaker"),
        [EventParticipantRole.Researcher] = ("მკვლევარი", "Researcher"),
        [EventParticipantRole.Mentor] = ("მენტორი", "Mentor"),
        [EventParticipantRole.Moderator] = ("მოდერატორი", "Moderator"),
        [EventParticipantRole.Guest] = ("სტუმარი", "Guest"),

        [PersonRole.CoreTeam] = ("პროექტის შემსრულებელი გუნდი", "Project team"),
        [PersonRole.Consultant] = ("მოწვეული კონსულტანტები", "Invited consultants"),
        [PersonRole.TechnologyTeam] = ("ტექნოლოგიური გუნდი", "Technology team"),
        [PersonRole.OrganizationalStaff] = ("საორგანიზაციო და ტექნიკური პერსონალი", "Organizational and technical staff"),
        [PersonRole.Researcher] = ("მკვლევარი", "Researcher"),
        [PersonRole.Mentor] = ("მენტორი", "Mentor"),

        [ResourceType.Document] = ("დოკუმენტი", "Document"),
        [ResourceType.Presentation] = ("პრეზენტაცია", "Presentation"),
        [ResourceType.Video] = ("ვიდეო", "Video"),
        [ResourceType.Photo] = ("ფოტო", "Photo"),
        [ResourceType.Link] = ("ბმული", "Link"),
        [ResourceType.Other] = ("სხვა", "Other"),

        [PartnerRole.Funder] = ("დამფინანსებელი", "Funder"),
        [PartnerRole.PartnerUniversity] = ("პარტნიორი უნივერსიტეტი", "Partner university"),
        [PartnerRole.Supporter] = ("მხარდამჭერი", "Supporter"),

        [PublicationStatus.Draft] = ("დრაფტი", "Draft"),
        [PublicationStatus.Published] = ("გამოქვეყნებული", "Published"),
        [PublicationStatus.Archived] = ("დაარქივებული", "Archived"),
    };

    public static string Of(Enum value, string language) =>
        Labels.TryGetValue(value, out var label) ? (language == LocalizedText.English ? label.En : label.Ka) : value.ToString();

    public static IReadOnlyList<LabeledValue> All<TEnum>(string language) where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>().Select(v => new LabeledValue(v.ToString(), Of(v, language))).ToList();
}
