using LocalEats.Domain.Entities;
using LocalEats.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalEats.Infrastructure.Persistence.Configurations;

public sealed class ManagerRestaurantConfiguration : IEntityTypeConfiguration<ManagerRestaurant>
{
    public void Configure(EntityTypeBuilder<ManagerRestaurant> builder)
    {
        builder.HasKey(assignment => assignment.ManagerId);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(assignment => assignment.ManagerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(assignment => assignment.Restaurant).WithMany().HasForeignKey(assignment => assignment.RestaurantId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(assignment => assignment.CreatedAt).IsRequired();
    }
}
