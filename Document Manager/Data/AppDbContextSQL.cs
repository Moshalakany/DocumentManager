using Document_Manager.Models;
using Microsoft.EntityFrameworkCore;

namespace Document_Manager.Data
{
    public class AppDbContextSQL : DbContext
    {
        public AppDbContextSQL(DbContextOptions options) : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<GroupPermission> GroupPermissions { get; set; }
        public DbSet<GroupPermissionsUser> GroupPermissionsUsers { get; set; }
        public DbSet<DocumentAccessibilityList> DocumentAccessibilityLists { get; set; }
        public DbSet<UserAccessibilityList> UserAccessibilityLists { get; set; }
        public DbSet<UserAccessibilityListItem> UserAccessibilityListItems { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Annotation> Annotations { get; set; }
        public DbSet<FileValidation> FileValidations { get; set; }
        public DbSet<DocumentAccessibilityListItem> DocumentAccessibilityListItems { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships
            modelBuilder.Entity<Document>()
                .HasOne(d => d.CreatedBy)
                .WithMany()
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);
                
            modelBuilder.Entity<Document>()
                .HasOne(d => d.AccessibilityList)
                .WithOne(a => a.Document)
                .HasForeignKey<DocumentAccessibilityList>(a => a.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<User>()
                .HasOne(u => u.AccessibilityList)
                .WithOne(a => a.User)
                .HasForeignKey<UserAccessibilityList>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Document>()
                .HasMany(d => d.Tags)
                .WithMany(t => t.Documents)
                .UsingEntity(j => j.ToTable("DocumentTags"));
                
            // Configure Folder relationships
            modelBuilder.Entity<Folder>()
                .HasOne(f => f.ParentFolder)
                .WithMany(f => f.SubFolders)
                .HasForeignKey(f => f.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Folder)
                .WithMany(f => f.Documents)
                .HasForeignKey(d => d.FolderId)
                .OnDelete(DeleteBehavior.SetNull);
                
            // Configure Document versioning
            modelBuilder.Entity<Document>()
                .HasOne(d => d.OriginalDocument)
                .WithMany(d => d.Versions)
                .HasForeignKey(d => d.OriginalDocumentId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure Annotations
            modelBuilder.Entity<Annotation>()
                .HasOne(a => a.Document)
                .WithMany(d => d.Annotations)
                .HasForeignKey(a => a.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Annotation>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure Tag creation reference
            modelBuilder.Entity<Tag>()
                .HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);
                
            // Configure User relationships to avoid cascade cycles
            modelBuilder.Entity<UserAccessibilityListItem>()
                .HasOne(i => i.TargetUser)
                .WithMany()
                .HasForeignKey(i => i.TargetUserId)
                .OnDelete(DeleteBehavior.NoAction); // Change from CASCADE to NO ACTION
                
            // Configure UserAccessibilityListItem relationship with UserAccessibilityList
            modelBuilder.Entity<UserAccessibilityListItem>()
                .HasOne(i => i.AccessibilityList)
                .WithMany(l => l.AccessibilityListItems)
                .HasForeignKey(i => i.AccessibilityListId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure DocumentAccessibilityListItem relationship with DocumentAccessibilityList
            modelBuilder.Entity<DocumentAccessibilityListItem>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Change from CASCADE to NO ACTION
        }
    }
}
