using Microsoft.EntityFrameworkCore;
using UsosFix.Models;

// Entity Framework invalidates those warnings
// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable 8618

namespace UsosFix
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; private set; }
        public DbSet<Group> Groups { get; private set; }
        public DbSet<GroupMeeting> GroupMeetings { get; private set; }
        public DbSet<Subject> Subjects { get; private set; }
        public DbSet<AccessToken> Tokens { get; private set; }
        public DbSet<Exchange> Exchanges { get; private set; }
        public DbSet<Relation> Relations { get; private set; }
        public DbSet<Team> Teams { get; private set; }
        public DbSet<Invitation> Invitations { get; private set; }
        public DbSet<Conversation> Conversations { get; private set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; private set; }
        public DbSet<Message> Messages { get; private set; }
        public DbSet<ChatConnection> ChatConnections { get; private set;  }
        public DbSet<Semester> Semesters { get; private set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>(e =>
            {
                e.HasKey(g => g.Id);
                e.HasOne(g => g.Subject).WithMany(s => s.Groups).IsRequired();
                e.HasMany(g => g.Students).WithMany(s => s.Groups);
                e.HasMany(g => g.Meetings).WithOne(m => m.Group);
            });

            modelBuilder.Entity<GroupMeeting>(e =>
            {
                e.HasKey(g => g.Id);
                e.OwnsOne(m => m.Building, e =>
                {
                    e.Property<int>("SubjectId").HasField("SubjectId");
                });
            });

            modelBuilder.Entity<Subject>(e =>
            {
                e.HasKey(s => s.Id);
                e.OwnsOne(s => s.Name, e => { e.Property<int>("SubjectId").HasField("SubjectId"); });
                e.HasOne(s => s.Semester).WithMany();
                e.Property($"{nameof(Subject.Semester)}Id").HasDefaultValue(1);
            });

            modelBuilder.Entity<AccessToken>(e =>
            {
                e.HasKey(t => t.Id);
                e.HasIndex(t => t.Token);
                e.HasOne(t => t.User);
            });

            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.StudentNumber);
                e.Property(u => u.Username).HasDefaultValue("Anonymous user");
                e.Property(u => u.Role).HasDefaultValue(Role.User);
                e.Property(u => u.PreferredLanguage).HasDefaultValue(Language.Polish);
                e.Property(u => u.Visible).HasDefaultValue(false);
                e.Property(u => u.DarkMode).HasDefaultValue(false);
            });

            modelBuilder.Entity<Exchange>(builder =>
            {
                builder.HasKey(e => e.Id);
                builder.HasOne(e => e.User).WithMany(u => u.Exchanges);
                builder.HasOne(e => e.Subject).WithMany(s => s.Exchanges);
                builder.HasOne(e => e.SourceGroup).WithMany(u => u.ExchangesFrom);
                builder.HasOne(e => e.TargetGroup).WithMany(u => u.ExchangesTo);
                builder.Property(e => e.State).HasDefaultValue(ExchangeState.Submitted);
            });

            modelBuilder.Entity<Relation>(builder =>
            {
                builder.HasKey(r => r.Id);
                builder.HasMany(r => r.Exchanges).WithMany(e => e.Relations);
            });

            modelBuilder.Entity<Team>(builder =>
            {
                builder.HasKey(t => t.Id);
                builder.HasMany(t => t.Users).WithMany(u => u.Teams);
                builder.HasMany(t => t.Invitations).WithOne(i => i.Team);
                builder.HasOne(t => t.Subject);
            });
            
            modelBuilder.Entity<Invitation>(builder =>
            {
                builder.HasKey(i => i.Id);
                builder.HasOne(i => i.Subject);
                builder.HasOne(i => i.Inviting);
                builder.HasOne(i => i.Invited).WithMany(u => u.ReceivedInvitations);
            });

            modelBuilder.Entity<Conversation>(builder =>
            {
                builder.HasKey(c => c.Id);
                builder.HasMany(c => c.Messages).WithOne(m => m.Conversation);
                builder.HasMany(c => c.Participants).WithOne(p => p.Conversation);
            });

            modelBuilder.Entity<ConversationParticipant>(builder =>
            {
                builder.HasKey(r => r.Id);
                builder.HasOne(p => p.User).WithMany(u => u.Conversations);
            });

            modelBuilder.Entity<Message>(builder =>
            {
                builder.HasKey(r => r.Id);
                builder.HasOne(m => m.Author).WithMany();
                builder.HasIndex(m => m.SentAt);
                builder.OwnsOne(m => m.Content, e =>
                {
                    e.Property<int>("SubjectId").HasField("SubjectId");
                });
            });

            modelBuilder.Entity<ChatConnection>(builder =>
            {
                builder.HasKey(c => c.Id);
                builder.HasOne(c => c.User).WithMany(u => u.Chats);
            });

            modelBuilder.Entity<Semester>(builder =>
            {
                builder.HasKey(s => s.Id);
                builder.HasIndex(s => s.IsCurrent);
                builder.HasData(new Semester(SemesterSeason.Winter, 2020, true, 1));
            });
        }
    }
}
