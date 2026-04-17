using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vista.Core.Models;

namespace Vista.Core.Data.Configurations;

public class ChatRaumConfiguration : IEntityTypeConfiguration<ChatRaum>
{
    public void Configure(EntityTypeBuilder<ChatRaum> builder)
    {
        builder.HasOne(x => x.Projekt)
            .WithMany()
            .HasForeignKey(x => x.ProjektId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Ticket)
            .WithMany()
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Nachrichten)
            .WithOne(x => x.Raum)
            .HasForeignKey(x => x.RaumId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
