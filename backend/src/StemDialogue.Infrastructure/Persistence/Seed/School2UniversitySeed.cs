using StemDialogue.Domain.Common;
using StemDialogue.Domain.Organizations;
using StemDialogue.Domain.Pages;
using StemDialogue.Domain.People;
using StemDialogue.Domain.Projects;
using StemDialogue.Domain.Taxonomy;

namespace StemDialogue.Infrastructure.Persistence.Seed;

/// <summary>
/// Confirmed base content of the SPG-26-185 project, taken from the specification and the supplied
/// project materials. Only people confirmed in the specification are created (spec §7.1, §16.6).
/// Logos are uploaded through the CMS as original files.
/// </summary>
internal static class School2UniversitySeed
{
    public const string Slug = "school2university";
    private static readonly DateOnly Start = new(2026, 10, 1);

    public static Project Project(Organization funder, Organization btu, Organization atsu)
    {
        var project = new Project
        {
            Slug = Slug,
            Code = "SPG-26-185",
            Name = T("სამეცნიერო დიალოგი — სკოლიდან უნივერსიტეტამდე", "Scientific Dialogue — From School to University"),
            ShortName = T("სკოლიდან უნივერსიტეტამდე", "From School to University"),
            Tagline = T(
                "სივრცე, სადაც საქართველოს IX–XII კლასის მოსწავლეები, მეცნიერები და უნივერსიტეტები ერთმანეთს ხვდებიან.",
                "A space where grade 9–12 students, scientists and universities across Georgia meet."),
            Status = ProjectStatus.Active,
            StartDate = Start,
            EndDate = new DateOnly(2027, 6, 30),
            IsDefault = true,
            FundingShortText = T(
                "შოთა რუსთაველის ეროვნული სამეცნიერო ფონდის დაფინანსებით • BTU და ATSU-ის პარტნიორობით",
                "Funded by the Shota Rustaveli National Science Foundation of Georgia • In partnership with BTU and ATSU"),
            FundingFullText = T(
                "პროექტი ხორციელდება შოთა რუსთაველის საქართველოს ეროვნული სამეცნიერო ფონდის მხარდაჭერით, ბიზნესისა და ტექნოლოგიების უნივერსიტეტის (ბტუ) და აკაკი წერეთლის სახელმწიფო უნივერსიტეტის (აწსუ) ჩართულობით.",
                "The project is implemented with the support of the Shota Rustaveli National Science Foundation of Georgia, with the participation of Business and Technology University (BTU) and Akaki Tsereteli State University (ATSU)."),
            ContactPerson = T("გვანცა წულაია", "Gvantsa Tsulaia"),
            ContactEmail = "gvantsa.tsulaia@btu.edu.ge",
            ContactPhone = "+995 511 00 00 85",
            SeoDescription = T(
                "STEM ლექციები, ვორქშოპები და შეხვედრები მეცნიერებთან საქართველოს IX–XII კლასის მოსწავლეებისთვის.",
                "STEM lectures, workshops and meetings with scientists for grade 9–12 students in Georgia."),
        };

        project.Partners.Add(new ProjectPartner { ProjectId = project.Id, OrganizationId = funder.Id, Role = PartnerRole.Funder, DisplayOrder = 0 });
        project.Partners.Add(new ProjectPartner { ProjectId = project.Id, OrganizationId = btu.Id, Role = PartnerRole.PartnerUniversity, DisplayOrder = 1 });
        project.Partners.Add(new ProjectPartner { ProjectId = project.Id, OrganizationId = atsu.Id, Role = PartnerRole.PartnerUniversity, DisplayOrder = 2 });
        return project;
    }

    public static (Organization Funder, Organization Btu, Organization Atsu) Organizations() =>
    (
        new Organization
        {
            Name = T("შოთა რუსთაველის საქართველოს ეროვნული სამეცნიერო ფონდი", "Shota Rustaveli National Science Foundation of Georgia"),
            ShortName = T("რუსთაველის ფონდი", "SRNSFG"),
            WebsiteUrl = "https://rustaveli.org.ge",
        },
        new Organization
        {
            Name = T("ბიზნესისა და ტექნოლოგიების უნივერსიტეტი", "Business and Technology University"),
            ShortName = T("ბტუ", "BTU"),
            WebsiteUrl = "https://btu.edu.ge",
        },
        new Organization
        {
            Name = T("აკაკი წერეთლის სახელმწიფო უნივერსიტეტი", "Akaki Tsereteli State University"),
            ShortName = T("აწსუ", "ATSU"),
            WebsiteUrl = "https://atsu.edu.ge",
        });

    public static IEnumerable<SitePage> Pages(Guid projectId)
    {
        var root = "/projects/" + Slug;
        var order = 0;

        SitePage Page(string key, LocalizedText title, LocalizedText? lead = null, LocalizedText? nav = null) => new()
        {
            ProjectId = projectId,
            Key = key,
            Title = title,
            NavigationLabel = nav ?? new LocalizedText(),
            Lead = lead ?? new LocalizedText(),
            NavigationOrder = order++,
        };

        var home = Page(
            PageKeys.Home,
            T("სამეცნიერო დიალოგი — სკოლიდან უნივერსიტეტამდე", "Scientific Dialogue — From School to University"),
            T("აღმოაჩინე მეცნიერება მკვლევრებთან ერთად: ონლაინ STEM ლექციები, ვორქშოპები და STEM შაბათობები IX–XII კლასის მოსწავლეებისთვის.",
              "Discover science with researchers: online STEM lectures, workshops and STEM Saturdays for grade 9–12 students."),
            T("მთავარი", "Home"));
        home.PrimaryActionLabel = T("უახლოესი შეხვედრები", "Upcoming meetings");
        home.PrimaryActionUrl = root + "/stem-saturdays";
        home.SecondaryActionLabel = T("პროექტის შესახებ", "About the project");
        home.SecondaryActionUrl = root + "/about";
        yield return home;

        var about = Page(
            PageKeys.About,
            T("პროექტის შესახებ", "About the project"),
            T("მეცნიერების პოპულარიზაციის კამპანია საქართველოს IX–XII კლასის მოსწავლეებისთვის.",
              "A science-popularization campaign for grade 9–12 students in Georgia."));
        about.Body = T(
            "<p>პროექტი ითვალისწინებს ონლაინ STEM ლექციებსა და ვორქშოპებს, მკვლევრებთან შეხვედრებს და სასკოლო STEM ელჩების ქსელის შექმნას.</p>" +
            "<p>2027 წლის მაისში გაიმართება შუალედური ფორუმი, ხოლო ივნისში — ფინალური ღონისძიება.</p>",
            "<p>The project includes online STEM lectures and workshops, meetings with researchers and a network of school STEM ambassadors.</p>" +
            "<p>An interim forum takes place in May 2027 and the final event in June 2027.</p>");
        yield return about;

        yield return Page(PageKeys.News, T("სიახლეები", "News"), T("განცხადებები და პროექტის ქრონიკა.", "Announcements and project chronicle."));
        yield return Page(PageKeys.StemSaturdays, T("STEM შაბათობები", "STEM Saturdays"), T("მომავალი შეხვედრები და ჩატარებული შეხვედრების არქივი.", "Upcoming meetings and the archive of past ones."));
        yield return Page(PageKeys.Team, T("ჩვენი გუნდი", "Our team"));
        yield return Page(PageKeys.Researchers, T("მკვლევრები", "Researchers"));
        yield return Page(PageKeys.Mentors, T("მენტორები", "Mentors"));
        yield return Page(
            PageKeys.StemAmbassadors,
            T("STEM ელჩები", "STEM Ambassadors"),
            T("შერჩეული მოსწავლეები მიღებულ ცოდნას საკუთარ სკოლებში აზიარებენ და სამეცნიერო-საკომუნიკაციო აქტივობებს გეგმავენ.",
              "Selected students share what they learn in their schools and organize science-communication activities."));
        yield return Page(PageKeys.Resources, T("რესურსები", "Resources"), T("ვიდეოები, პრეზენტაციები და სასწავლო მასალები.", "Videos, presentations and learning materials."));
        yield return Page(PageKeys.Contact, T("კონტაქტი", "Contact"));
    }

    public static IEnumerable<ProjectMilestone> Milestones(Guid projectId)
    {
        var order = 0;
        ProjectMilestone M(LocalizedText title, LocalizedText description, LocalizedText period, LocalizedText location, DateOnly start, DateOnly end) => new()
        {
            ProjectId = projectId,
            Title = title,
            Description = description,
            PeriodLabel = period,
            Location = location,
            StartDate = start,
            EndDate = end,
            DisplayOrder = order++,
        };

        yield return M(
            T("ონლაინ STEM ლექციები და „STEM შაბათობები“", "Online STEM lectures & \"STEM Saturdays\""),
            T("პრაქტიკული ვორქშოპები და ინტერაქტიული შეხვედრები მეცნიერებთან, ახალგაზრდა მკვლევრებთან და სტუდენტებთან.",
              "Practical workshops and interactive sessions with scientists, young researchers and university students."),
            T("ოქტომბერი 2026 – აპრილი 2027", "October 2026 – April 2027"),
            T("ონლაინ პლატფორმა", "Online"),
            new DateOnly(2026, 10, 1), new DateOnly(2027, 4, 30));
        yield return M(
            T("STEM ელჩების ქსელი", "STEM Ambassador Network"),
            T("შერჩეული მოსწავლეები მიღებულ ცოდნას გააზიარებენ საკუთარ სკოლებში და დაგეგმავენ სამეცნიერო-საკომუნიკაციო აქტივობებს.",
              "Selected students share knowledge in their schools and organize local science-communication activities."),
            T("ზამთარი 2026 – გაზაფხული 2027", "Winter 2026 – Spring 2027"),
            T("საქართველოს რეგიონული სკოლები", "Schools across Georgia"),
            new DateOnly(2026, 12, 1), new DateOnly(2027, 5, 31));
        yield return M(
            T("შუალედური ფორუმი", "Interim forum"),
            T("მონაწილეები წარადგენენ საკუთარ ინიციატივებსა და იდეებს და მიიღებენ უკუკავშირს.",
              "Participants present their initiatives and ideas and receive feedback."),
            T("მაისი 2027", "May 2027"),
            T("ბიზნესისა და ტექნოლოგიების უნივერსიტეტი (თბილისი)", "Business and Technology University (Tbilisi)"),
            new DateOnly(2027, 5, 1), new DateOnly(2027, 5, 31));
        yield return M(
            T("ფინალური ღონისძიება და დაჯილდოება", "Final event & awards"),
            T("STEM ელჩების დაჯილდოება და პროექტების საჯარო პრეზენტაციები.",
              "STEM ambassador awards and public project presentations."),
            T("ივნისი 2027", "June 2027"),
            T("აკაკი წერეთლის სახელმწიფო უნივერსიტეტი (ქუთაისი)", "Akaki Tsereteli State University (Kutaisi)"),
            new DateOnly(2027, 6, 1), new DateOnly(2027, 6, 30));
    }

    public static IEnumerable<Discipline> Disciplines()
    {
        var order = 0;
        Discipline D(string slug, string ka, string en) => new() { Slug = slug, Name = T(ka, en), DisplayOrder = order++ };

        yield return D("mathematics-physics", "მათემატიკა და ფიზიკა", "Mathematics & Physics");
        yield return D("chemistry-biology", "ქიმია და ბიოლოგია", "Chemistry & Biology");
        yield return D("computer-science-ai", "კომპიუტერული მეცნიერება და ხელოვნური ინტელექტი", "Computer Science & AI");
        yield return D("engineering-technology", "ინჟინერია და ტექნოლოგიები", "Engineering & Technology");
        yield return D("health-life-sciences", "ჯანმრთელობისა და სიცოცხლის მეცნიერებები", "Health & Life Sciences");
    }

    /// <summary>The official core team named in the specification (spec §7.1).</summary>
    public static IEnumerable<Person> CoreTeam(Guid projectId)
    {
        var order = 0;
        Person P(string slug, LocalizedText name, LocalizedText title)
        {
            var person = new Person { Slug = slug, FullName = name, Status = PublicationStatus.Published, DisplayOrder = order };
            person.Roles.Add(new PersonRoleAssignment
            {
                PersonId = person.Id,
                ProjectId = projectId,
                Role = PersonRole.CoreTeam,
                Title = title,
                StartDate = Start,
                DisplayOrder = order++,
            });
            return person;
        }

        yield return P("gvantsa-tsulaia", T("გვანცა წულაია", "Gvantsa Tsulaia"), T("პროექტის ხელმძღვანელი", "Project leader"));
        yield return P("manana-chumburidze", T("მანანა ჭუმბურიძე", "Manana Chumburidze"), T("პროექტის კოორდინატორი", "Project coordinator"));
        yield return P("giorgi-chachua", T("გიორგი ჩაჩუა", "Giorgi Chachua"), T("შემსრულებელი გუნდის წევრი", "Project team member"));
        yield return P("keti-ketelauri", T("ქეთი ქეთელაური", "Qeti Qetelauri"), T("შემსრულებელი გუნდის წევრი", "Project team member"));
    }

    private static LocalizedText T(string ka, string en) => new(ka, en);
}
