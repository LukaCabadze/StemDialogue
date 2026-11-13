using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StemDialogue.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChangesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Disciplines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Name_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disciplines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AltText_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AltText_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Caption_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Caption_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Credit_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Credit_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SlugRedirects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentType = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OldSlug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlugRedirects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Name_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name_En = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedReason = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    ReplacedByHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortName_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortName_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    LogoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LogoOnDarkId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizations_Media_LogoId",
                        column: x => x.LogoId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organizations_Media_LogoOnDarkId",
                        column: x => x.LogoOnDarkId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    FullName_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcademicTitle_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AcademicTitle_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Position_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Organization_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Organization_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortBio_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortBio_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Biography_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Biography_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contribution_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contribution_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    SeoTitle_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeoTitle_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoDescription_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeoDescription_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShareImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                    table.ForeignKey(
                        name: "FK_People_Media_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_People_Media_ShareImageId",
                        column: x => x.ShareImageId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortName_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortName_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tagline_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tagline_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    FundingShortText_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FundingShortText_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FundingFullText_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FundingFullText_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContactPerson_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPerson_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ContactAddress_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactAddress_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoTitle_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeoTitle_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoDescription_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeoDescription_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShareImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Media_LogoId",
                        column: x => x.LogoId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Media_ShareImageId",
                        column: x => x.ShareImageId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonDisciplines",
                columns: table => new
                {
                    DisciplinesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonDisciplines", x => new { x.DisciplinesId, x.PersonId });
                    table.ForeignKey(
                        name: "FK_PersonDisciplines_Disciplines_DisciplinesId",
                        column: x => x.DisciplinesId,
                        principalTable: "Disciplines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonDisciplines_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Label_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Label_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonLinks_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Format = table.Column<int>(type: "int", nullable: false),
                    Venue_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Venue_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreamUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    RegistrationUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    RegistrationDeadlineUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusNote_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusNote_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalStartsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Recap_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Recap_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordingUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Publication = table.Column<int>(type: "int", nullable: false),
                    FirstPublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    CoverImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SeoTitle_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeoTitle_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoDescription_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeoDescription_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShareImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Media_CoverImageId",
                        column: x => x.CoverImageId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_Media_ShareImageId",
                        column: x => x.ShareImageId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Title_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonRoles_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonRoles_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PeriodLabel_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodLabel_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMilestones_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPartners",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    RoleLabel_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleLabel_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ShowInHeader = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPartners", x => new { x.ProjectId, x.OrganizationId });
                    table.ForeignKey(
                        name: "FK_ProjectPartners_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectPartners_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Title_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolName_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchoolName_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Region_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActivityDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CoverImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MediaConsentConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    ConsentReference = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolActivities_Media_CoverImageId",
                        column: x => x.CoverImageId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolActivities_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SitePages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Title_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NavigationLabel_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NavigationLabel_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Lead_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lead_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeroImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrimaryActionLabel_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryActionLabel_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryActionUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    SecondaryActionLabel_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecondaryActionLabel_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondaryActionUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ShowInNavigation = table.Column<bool>(type: "bit", nullable: false),
                    NavigationOrder = table.Column<int>(type: "int", nullable: false),
                    SeoTitle_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeoTitle_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoDescription_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeoDescription_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShareImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SitePages_Media_HeroImageId",
                        column: x => x.HeroImageId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SitePages_Media_ShareImageId",
                        column: x => x.ShareImageId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SitePages_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SocialLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialLinks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventDisciplines",
                columns: table => new
                {
                    DisciplinesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventDisciplines", x => new { x.DisciplinesId, x.EventId });
                    table.ForeignKey(
                        name: "FK_EventDisciplines_Disciplines_DisciplinesId",
                        column: x => x.DisciplinesId,
                        principalTable: "Disciplines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventDisciplines_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventGallery",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventGallery", x => new { x.EventId, x.MediaAssetId });
                    table.ForeignKey(
                        name: "FK_EventGallery_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventGallery_Media_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventParticipants",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventParticipants", x => new { x.EventId, x.PersonId });
                    table.ForeignKey(
                        name: "FK_EventParticipants_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventParticipants_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventTags",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTags", x => new { x.EventId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_EventTags_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTags_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Title_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PublishAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirstPublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    CoverImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SeoTitle_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeoTitle_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoDescription_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeoDescription_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShareImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                    table.ForeignKey(
                        name: "FK_News_Events_RelatedEventId",
                        column: x => x.RelatedEventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_News_Media_CoverImageId",
                        column: x => x.CoverImageId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_News_Media_ShareImageId",
                        column: x => x.ShareImageId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_News_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Title_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExternalUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorName_Ka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorName_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThumbnailId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Resources_Media_FileId",
                        column: x => x.FileId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resources_Media_ThumbnailId",
                        column: x => x.ThumbnailId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resources_People_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Resources_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolActivityGallery",
                columns: table => new
                {
                    SchoolActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolActivityGallery", x => new { x.SchoolActivityId, x.MediaAssetId });
                    table.ForeignKey(
                        name: "FK_SchoolActivityGallery_Media_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolActivityGallery_SchoolActivities_SchoolActivityId",
                        column: x => x.SchoolActivityId,
                        principalTable: "SchoolActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsGallery",
                columns: table => new
                {
                    NewsArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsGallery", x => new { x.NewsArticleId, x.MediaAssetId });
                    table.ForeignKey(
                        name: "FK_NewsGallery_Media_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NewsGallery_News_NewsArticleId",
                        column: x => x.NewsArticleId,
                        principalTable: "News",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsTags",
                columns: table => new
                {
                    NewsArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsTags", x => new { x.NewsArticleId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_NewsTags_News_NewsArticleId",
                        column: x => x.NewsArticleId,
                        principalTable: "News",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsTags_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceDisciplines",
                columns: table => new
                {
                    DisciplinesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceDisciplines", x => new { x.DisciplinesId, x.ResourceId });
                    table.ForeignKey(
                        name: "FK_ResourceDisciplines_Disciplines_DisciplinesId",
                        column: x => x.DisciplinesId,
                        principalTable: "Disciplines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceDisciplines_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceTags",
                columns: table => new
                {
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceTags", x => new { x.ResourceId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ResourceTags_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceTags_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityType_EntityId",
                table: "AuditLog",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_OccurredAtUtc",
                table: "AuditLog",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Disciplines_Slug",
                table: "Disciplines",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventDisciplines_EventId",
                table: "EventDisciplines",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventGallery_MediaAssetId",
                table: "EventGallery",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipants_PersonId",
                table: "EventParticipants",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CoverImageId",
                table: "Events",
                column: "CoverImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ProjectId_Publication_StartsAtUtc",
                table: "Events",
                columns: new[] { "ProjectId", "Publication", "StartsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_ProjectId_Slug",
                table: "Events",
                columns: new[] { "ProjectId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_ShareImageId",
                table: "Events",
                column: "ShareImageId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTags_TagsId",
                table: "EventTags",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_CreatedAtUtc",
                table: "Media",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Media_Sha256",
                table: "Media",
                column: "Sha256");

            migrationBuilder.CreateIndex(
                name: "IX_Media_StoragePath",
                table: "Media",
                column: "StoragePath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_News_CoverImageId",
                table: "News",
                column: "CoverImageId");

            migrationBuilder.CreateIndex(
                name: "IX_News_ProjectId_Slug",
                table: "News",
                columns: new[] { "ProjectId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_News_ProjectId_Status_PublishAtUtc",
                table: "News",
                columns: new[] { "ProjectId", "Status", "PublishAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_News_RelatedEventId",
                table: "News",
                column: "RelatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_News_ShareImageId",
                table: "News",
                column: "ShareImageId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsGallery_MediaAssetId",
                table: "NewsGallery",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsTags_TagsId",
                table: "NewsTags",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_LogoId",
                table: "Organizations",
                column: "LogoId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_LogoOnDarkId",
                table: "Organizations",
                column: "LogoOnDarkId");

            migrationBuilder.CreateIndex(
                name: "IX_People_PhotoId",
                table: "People",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_People_ShareImageId",
                table: "People",
                column: "ShareImageId");

            migrationBuilder.CreateIndex(
                name: "IX_People_Slug",
                table: "People",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonDisciplines_PersonId",
                table: "PersonDisciplines",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonLinks_PersonId",
                table: "PersonLinks",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonRoles_PersonId_ProjectId_Role",
                table: "PersonRoles",
                columns: new[] { "PersonId", "ProjectId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonRoles_ProjectId_Role",
                table: "PersonRoles",
                columns: new[] { "ProjectId", "Role" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_ProjectId_DisplayOrder",
                table: "ProjectMilestones",
                columns: new[] { "ProjectId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPartners_OrganizationId",
                table: "ProjectPartners",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_LogoId",
                table: "Projects",
                column: "LogoId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ShareImageId",
                table: "Projects",
                column: "ShareImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Slug",
                table: "Projects",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_FamilyId",
                table: "RefreshTokens",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceDisciplines_ResourceId",
                table: "ResourceDisciplines",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_AuthorId",
                table: "Resources",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_EventId",
                table: "Resources",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_FileId",
                table: "Resources",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ProjectId_Slug",
                table: "Resources",
                columns: new[] { "ProjectId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ProjectId_Status_Type",
                table: "Resources",
                columns: new[] { "ProjectId", "Status", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ThumbnailId",
                table: "Resources",
                column: "ThumbnailId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceTags_TagsId",
                table: "ResourceTags",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolActivities_CoverImageId",
                table: "SchoolActivities",
                column: "CoverImageId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolActivities_ProjectId_Slug",
                table: "SchoolActivities",
                columns: new[] { "ProjectId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolActivityGallery_MediaAssetId",
                table: "SchoolActivityGallery",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_SitePages_HeroImageId",
                table: "SitePages",
                column: "HeroImageId");

            migrationBuilder.CreateIndex(
                name: "IX_SitePages_ProjectId_Key",
                table: "SitePages",
                columns: new[] { "ProjectId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SitePages_ShareImageId",
                table: "SitePages",
                column: "ShareImageId");

            migrationBuilder.CreateIndex(
                name: "IX_SlugRedirects_ContentType_ProjectId_OldSlug",
                table: "SlugRedirects",
                columns: new[] { "ContentType", "ProjectId", "OldSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialLinks_ProjectId",
                table: "SocialLinks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Slug",
                table: "Tags",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "EventDisciplines");

            migrationBuilder.DropTable(
                name: "EventGallery");

            migrationBuilder.DropTable(
                name: "EventParticipants");

            migrationBuilder.DropTable(
                name: "EventTags");

            migrationBuilder.DropTable(
                name: "NewsGallery");

            migrationBuilder.DropTable(
                name: "NewsTags");

            migrationBuilder.DropTable(
                name: "PersonDisciplines");

            migrationBuilder.DropTable(
                name: "PersonLinks");

            migrationBuilder.DropTable(
                name: "PersonRoles");

            migrationBuilder.DropTable(
                name: "ProjectMilestones");

            migrationBuilder.DropTable(
                name: "ProjectPartners");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ResourceDisciplines");

            migrationBuilder.DropTable(
                name: "ResourceTags");

            migrationBuilder.DropTable(
                name: "SchoolActivityGallery");

            migrationBuilder.DropTable(
                name: "SitePages");

            migrationBuilder.DropTable(
                name: "SlugRedirects");

            migrationBuilder.DropTable(
                name: "SocialLinks");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Disciplines");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "SchoolActivities");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Media");
        }
    }
}
