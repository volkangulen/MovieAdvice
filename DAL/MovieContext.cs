using MAdvice.Models;
using Microsoft.EntityFrameworkCore;

namespace MAdvice.DAL
{
    public class MovieContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<MovieRating> MovieRatings { get; set; }

        public MovieContext(DbContextOptions<MovieContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.ToTable("Movies");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .HasName("IX_MovieId")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("Id");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("Title")
                    .HasColumnType("varchar(128)");

                entity.Property(e => e.Description)
                    .IsRequired(false)
                    .HasColumnName("Description")
                    .HasColumnType("text");

                entity.Property(e => e.ReleaseDate)
                    .IsRequired()
                    .HasColumnName("ReleaseDate")
                    .HasColumnType("date");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .HasName("IX_UserId")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("Id");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("Username")
                    .HasColumnType("varchar(16)");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("Email")
                    .HasColumnType("varchar(324)");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("Password")
                    .HasColumnType("varchar(256)");

            });

            modelBuilder.Entity<MovieRating>(entity =>
            {
                entity.ToTable("MovieRatings");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .HasName("IX_MovieRatingId")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("Id");

                entity.Property(e => e.Vote)
                    .IsRequired()
                    .HasColumnName("Vote")
                    .HasColumnType("smallint");

                entity.Property(e => e.Note)
                    .IsRequired(false)
                    .HasColumnName("Note")
                    .HasColumnType("text");

                entity.HasOne(x => x.User)
                    .WithMany(x => x.MovieRatings)
                    .HasForeignKey(x => x.UserId)
                    .IsRequired();

                entity.HasOne(x => x.Movie)
                    .WithMany(x => x.MovieRatings)
                    .HasForeignKey(x => x.MovieId)
                    .IsRequired();
            });
        }
    }
}