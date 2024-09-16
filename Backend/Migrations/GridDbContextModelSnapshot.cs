﻿// <auto-generated />
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace WebApplication1.Migrations
{
    [DbContext(typeof(GridDbContext))]
    partial class GridDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("Backend.Data.GridElement", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("GridPromotionalGameId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("HasBeenFlipped")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Index")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PrizeId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GridPromotionalGameId");

                    b.HasIndex("PrizeId");

                    b.ToTable("GridElements");
                });

            modelBuilder.Entity("Backend.Data.GridPromotionalGame", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("PromotionalGames");
                });

            modelBuilder.Entity("Backend.Data.Prize", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Prizes");

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("Backend.Data.MonetaryPrize", b =>
                {
                    b.HasBaseType("Backend.Data.Prize");

                    b.Property<decimal>("MonetaryValue")
                        .HasColumnType("TEXT");

                    b.ToTable("MonetaryPrizes", (string)null);
                });

            modelBuilder.Entity("Backend.Data.NonMonetaryPrize", b =>
                {
                    b.HasBaseType("Backend.Data.Prize");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.ToTable("NonMonetaryPrizes", (string)null);
                });

            modelBuilder.Entity("Backend.Data.GridElement", b =>
                {
                    b.HasOne("Backend.Data.GridPromotionalGame", "GridPromotionalGame")
                        .WithMany("GridElements")
                        .HasForeignKey("GridPromotionalGameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Data.Prize", "Prize")
                        .WithMany()
                        .HasForeignKey("PrizeId");

                    b.Navigation("GridPromotionalGame");

                    b.Navigation("Prize");
                });

            modelBuilder.Entity("Backend.Data.MonetaryPrize", b =>
                {
                    b.HasOne("Backend.Data.Prize", null)
                        .WithOne()
                        .HasForeignKey("Backend.Data.MonetaryPrize", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Backend.Data.NonMonetaryPrize", b =>
                {
                    b.HasOne("Backend.Data.Prize", null)
                        .WithOne()
                        .HasForeignKey("Backend.Data.NonMonetaryPrize", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Backend.Data.GridPromotionalGame", b =>
                {
                    b.Navigation("GridElements");
                });
#pragma warning restore 612, 618
        }
    }
}
