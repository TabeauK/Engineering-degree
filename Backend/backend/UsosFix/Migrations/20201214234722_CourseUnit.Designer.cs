﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using UsosFix;

namespace UsosFix.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20201214234722_CourseUnit")]
    partial class CourseUnit
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("ExchangeRelation", b =>
                {
                    b.Property<int>("ExchangesId")
                        .HasColumnType("integer");

                    b.Property<int>("RelationsId")
                        .HasColumnType("integer");

                    b.HasKey("ExchangesId", "RelationsId");

                    b.HasIndex("RelationsId");

                    b.ToTable("ExchangeRelation");
                });

            modelBuilder.Entity("GroupUser", b =>
                {
                    b.Property<int>("GroupsId")
                        .HasColumnType("integer");

                    b.Property<int>("StudentsId")
                        .HasColumnType("integer");

                    b.HasKey("GroupsId", "StudentsId");

                    b.HasIndex("StudentsId");

                    b.ToTable("GroupUser");
                });

            modelBuilder.Entity("TeamUser", b =>
                {
                    b.Property<int>("TeamsId")
                        .HasColumnType("integer");

                    b.Property<int>("UsersId")
                        .HasColumnType("integer");

                    b.HasKey("TeamsId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("TeamUser");
                });

            modelBuilder.Entity("UsosFix.Models.AccessToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<bool>("Authorized")
                        .HasColumnType("boolean");

                    b.Property<int>("Callback")
                        .HasColumnType("integer");

                    b.Property<string>("Secret")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("ValidFrom")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("Token");

                    b.HasIndex("UserId");

                    b.ToTable("Tokens");
                });

            modelBuilder.Entity("UsosFix.Models.Exchange", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("SourceGroupId")
                        .HasColumnType("integer");

                    b.Property<int>("State")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0);

                    b.Property<int>("SubjectId")
                        .HasColumnType("integer");

                    b.Property<int>("TargetGroupId")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("SourceGroupId");

                    b.HasIndex("SubjectId");

                    b.HasIndex("TargetGroupId");

                    b.HasIndex("UserId");

                    b.ToTable("Exchanges");
                });

            modelBuilder.Entity("UsosFix.Models.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("ClassType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("GroupNumber")
                        .HasColumnType("integer");

                    b.Property<string>("Lecturers")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("MaxMembers")
                        .HasColumnType("integer");

                    b.Property<int>("SubjectId")
                        .HasColumnType("integer");

                    b.Property<string>("UsosUnitId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("SubjectId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("UsosFix.Models.GroupMeeting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Frequency")
                        .HasColumnType("integer");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer");

                    b.Property<string>("Room")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.ToTable("GroupMeetings");
                });

            modelBuilder.Entity("UsosFix.Models.Invitation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("InvitedId")
                        .HasColumnType("integer");

                    b.Property<int?>("InvitingId")
                        .HasColumnType("integer");

                    b.Property<int?>("SubjectId")
                        .HasColumnType("integer");

                    b.Property<int>("TeamId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("InvitedId");

                    b.HasIndex("InvitingId");

                    b.HasIndex("SubjectId");

                    b.HasIndex("TeamId");

                    b.ToTable("Invitations");
                });

            modelBuilder.Entity("UsosFix.Models.Relation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("RelationType")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Relations");
                });

            modelBuilder.Entity("UsosFix.Models.Subject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.HasKey("Id");

                    b.ToTable("Subjects");
                });

            modelBuilder.Entity("UsosFix.Models.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<int?>("SubjectId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("SubjectId");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("UsosFix.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("PreferredLanguage")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(1);

                    b.Property<int>("Role")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(3);

                    b.Property<string>("StudentNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Surname")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<bool>("Visible")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.HasKey("Id");

                    b.HasIndex("StudentNumber");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ExchangeRelation", b =>
                {
                    b.HasOne("UsosFix.Models.Exchange", null)
                        .WithMany()
                        .HasForeignKey("ExchangesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("UsosFix.Models.Relation", null)
                        .WithMany()
                        .HasForeignKey("RelationsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GroupUser", b =>
                {
                    b.HasOne("UsosFix.Models.Group", null)
                        .WithMany()
                        .HasForeignKey("GroupsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("UsosFix.Models.User", null)
                        .WithMany()
                        .HasForeignKey("StudentsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TeamUser", b =>
                {
                    b.HasOne("UsosFix.Models.Team", null)
                        .WithMany()
                        .HasForeignKey("TeamsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("UsosFix.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UsosFix.Models.AccessToken", b =>
                {
                    b.HasOne("UsosFix.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("UsosFix.Models.Exchange", b =>
                {
                    b.HasOne("UsosFix.Models.Group", "SourceGroup")
                        .WithMany("ExchangesFrom")
                        .HasForeignKey("SourceGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("UsosFix.Models.Subject", "Subject")
                        .WithMany("Exchanges")
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("UsosFix.Models.Group", "TargetGroup")
                        .WithMany("ExchangesTo")
                        .HasForeignKey("TargetGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("UsosFix.Models.User", "User")
                        .WithMany("Exchanges")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SourceGroup");

                    b.Navigation("Subject");

                    b.Navigation("TargetGroup");

                    b.Navigation("User");
                });

            modelBuilder.Entity("UsosFix.Models.Group", b =>
                {
                    b.HasOne("UsosFix.Models.Subject", "Subject")
                        .WithMany("Groups")
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Subject");
                });

            modelBuilder.Entity("UsosFix.Models.GroupMeeting", b =>
                {
                    b.HasOne("UsosFix.Models.Group", "Group")
                        .WithMany("Meetings")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("UsosFix.ViewModels.LanguageString", "Building", b1 =>
                        {
                            b1.Property<int>("GroupMeetingId")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer")
                                .UseIdentityByDefaultColumn();

                            b1.Property<string>("English")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Polish")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<int>("SubjectId")
                                .HasColumnType("integer");

                            b1.HasKey("GroupMeetingId");

                            b1.ToTable("GroupMeetings");

                            b1.WithOwner()
                                .HasForeignKey("GroupMeetingId");
                        });

                    b.Navigation("Building")
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("UsosFix.Models.Invitation", b =>
                {
                    b.HasOne("UsosFix.Models.User", "Invited")
                        .WithMany("ReceivedInvitations")
                        .HasForeignKey("InvitedId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("UsosFix.Models.User", "Inviting")
                        .WithMany()
                        .HasForeignKey("InvitingId");

                    b.HasOne("UsosFix.Models.Subject", "Subject")
                        .WithMany()
                        .HasForeignKey("SubjectId");

                    b.HasOne("UsosFix.Models.Team", "Team")
                        .WithMany("Invitations")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Invited");

                    b.Navigation("Inviting");

                    b.Navigation("Subject");

                    b.Navigation("Team");
                });

            modelBuilder.Entity("UsosFix.Models.Subject", b =>
                {
                    b.OwnsOne("UsosFix.ViewModels.LanguageString", "Name", b1 =>
                        {
                            b1.Property<int>("SubjectId")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer")
                                .UseIdentityByDefaultColumn();

                            b1.Property<string>("English")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Polish")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("SubjectId");

                            b1.ToTable("Subjects");

                            b1.WithOwner()
                                .HasForeignKey("SubjectId");
                        });

                    b.Navigation("Name")
                        .IsRequired();
                });

            modelBuilder.Entity("UsosFix.Models.Team", b =>
                {
                    b.HasOne("UsosFix.Models.Subject", "Subject")
                        .WithMany()
                        .HasForeignKey("SubjectId");

                    b.Navigation("Subject");
                });

            modelBuilder.Entity("UsosFix.Models.Group", b =>
                {
                    b.Navigation("ExchangesFrom");

                    b.Navigation("ExchangesTo");

                    b.Navigation("Meetings");
                });

            modelBuilder.Entity("UsosFix.Models.Subject", b =>
                {
                    b.Navigation("Exchanges");

                    b.Navigation("Groups");
                });

            modelBuilder.Entity("UsosFix.Models.Team", b =>
                {
                    b.Navigation("Invitations");
                });

            modelBuilder.Entity("UsosFix.Models.User", b =>
                {
                    b.Navigation("Exchanges");

                    b.Navigation("ReceivedInvitations");
                });
#pragma warning restore 612, 618
        }
    }
}
