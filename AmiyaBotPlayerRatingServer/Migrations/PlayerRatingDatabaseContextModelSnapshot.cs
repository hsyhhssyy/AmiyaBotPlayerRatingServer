﻿// <auto-generated />
using System;
using AmiyaBotPlayerRatingServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AmiyaBotPlayerRatingServer.Migrations
{
    [DbContext(typeof(PlayerRatingDatabaseContext))]
    partial class PlayerRatingDatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("Avatar")
                        .HasColumnType("longtext");

                    b.Property<string>("AvatarType")
                        .HasColumnType("longtext");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime");

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.CharacterStatistics", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("AverageEquipLevel")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<double>("AverageLevel")
                        .HasColumnType("double");

                    b.Property<double>("AverageSkillLevel")
                        .HasColumnType("double");

                    b.Property<string>("AverageSpecializeLevel")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<long>("BatchCount")
                        .HasColumnType("bigint");

                    b.Property<string>("CharacterId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Rarity")
                        .HasColumnType("int");

                    b.Property<long>("SampleCount")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("VersionEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("VersionStart")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.ToTable("CharacterStatistics");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.ClientInfo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("FriendlyName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("IconBase64")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RedirectUri")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("ClientInfos");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.MAAConnection", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("DeviceIdentity")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("UserIdentity")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("MAAConnections");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.MAARepetitiveTask", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("AvailableFrom")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("AvailableTo")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("ConnectionId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsPaused")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime?>("LastRunAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Parameters")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("UtcCronString")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("ConnectionId");

                    b.ToTable("MAARepetitiveTasks");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.MAAResponse", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<byte[]>("ImagePayload")
                        .HasColumnType("longblob");

                    b.Property<byte[]>("ImagePayloadThumbnail")
                        .HasColumnType("longblob");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<Guid>("TaskId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("TaskId");

                    b.ToTable("MAAResponses");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.MAATask", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("AvailableAt")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("ConnectionId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsSystemGenerated")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Parameters")
                        .HasColumnType("longtext");

                    b.Property<Guid?>("ParentRepetitiveTaskId")
                        .HasColumnType("char(36)");

                    b.Property<Guid?>("ParentTaskId")
                        .HasColumnType("char(36)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("ConnectionId");

                    b.HasIndex("ParentRepetitiveTaskId");

                    b.HasIndex("ParentTaskId");

                    b.ToTable("MAATasks");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.SKLandCharacterBox", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("CharacterBoxJson")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("CredentialId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("RefreshedAt")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("CredentialId");

                    b.ToTable("SKLandCharacterBoxes");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.SKLandCredential", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("AvatarUrl")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Credential")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("RefreshSuccess")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("RefreshedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("SKLandUid")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("SKLandCredentials");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("RoleId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Value")
                        .HasColumnType("longtext");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreApplication", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ApplicationType")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("ClientId")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("ClientSecret")
                        .HasColumnType("longtext");

                    b.Property<string>("ClientType")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("ConsentType")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("DisplayName")
                        .HasColumnType("longtext");

                    b.Property<string>("DisplayNames")
                        .HasColumnType("longtext");

                    b.Property<string>("JsonWebKeySet")
                        .HasColumnType("longtext");

                    b.Property<string>("Permissions")
                        .HasColumnType("longtext");

                    b.Property<string>("PostLogoutRedirectUris")
                        .HasColumnType("longtext");

                    b.Property<string>("Properties")
                        .HasColumnType("longtext");

                    b.Property<string>("RedirectUris")
                        .HasColumnType("longtext");

                    b.Property<string>("Requirements")
                        .HasColumnType("longtext");

                    b.Property<string>("Settings")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("ClientId")
                        .IsUnique();

                    b.ToTable("OpenIddictApplications", (string)null);
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreAuthorization", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ApplicationId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Properties")
                        .HasColumnType("longtext");

                    b.Property<string>("Scopes")
                        .HasColumnType("longtext");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Subject")
                        .HasMaxLength(400)
                        .HasColumnType("varchar(400)");

                    b.Property<string>("Type")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationId", "Status", "Subject", "Type");

                    b.ToTable("OpenIddictAuthorizations", (string)null);
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreScope", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("Descriptions")
                        .HasColumnType("longtext");

                    b.Property<string>("DisplayName")
                        .HasColumnType("longtext");

                    b.Property<string>("DisplayNames")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("Properties")
                        .HasColumnType("longtext");

                    b.Property<string>("Resources")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("OpenIddictScopes", (string)null);
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreToken", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ApplicationId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("AuthorizationId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("ExpirationDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Payload")
                        .HasColumnType("longtext");

                    b.Property<string>("Properties")
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("RedemptionDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ReferenceId")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Subject")
                        .HasMaxLength(400)
                        .HasColumnType("varchar(400)");

                    b.Property<string>("Type")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("AuthorizationId");

                    b.HasIndex("ReferenceId")
                        .IsUnique();

                    b.HasIndex("ApplicationId", "Status", "Subject", "Type");

                    b.ToTable("OpenIddictTokens", (string)null);
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.MAAConnection", b =>
                {
                    b.HasOne("AmiyaBotPlayerRatingServer.Model.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.MAARepetitiveTask", b =>
                {
                    b.HasOne("AmiyaBotPlayerRatingServer.Model.MAAConnection", "Connection")
                        .WithMany()
                        .HasForeignKey("ConnectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Connection");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.MAAResponse", b =>
                {
                    b.HasOne("AmiyaBotPlayerRatingServer.Model.MAATask", "Task")
                        .WithMany()
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Task");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.MAATask", b =>
                {
                    b.HasOne("AmiyaBotPlayerRatingServer.Model.MAAConnection", "Connection")
                        .WithMany()
                        .HasForeignKey("ConnectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AmiyaBotPlayerRatingServer.Model.MAARepetitiveTask", "ParentRepetitiveTask")
                        .WithMany("SubTasks")
                        .HasForeignKey("ParentRepetitiveTaskId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("AmiyaBotPlayerRatingServer.Model.MAATask", "ParentTask")
                        .WithMany("SubTasks")
                        .HasForeignKey("ParentTaskId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Connection");

                    b.Navigation("ParentRepetitiveTask");

                    b.Navigation("ParentTask");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.SKLandCharacterBox", b =>
                {
                    b.HasOne("AmiyaBotPlayerRatingServer.Model.SKLandCredential", "Credential")
                        .WithMany()
                        .HasForeignKey("CredentialId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Credential");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.SKLandCredential", b =>
                {
                    b.HasOne("AmiyaBotPlayerRatingServer.Model.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("AmiyaBotPlayerRatingServer.Model.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("AmiyaBotPlayerRatingServer.Model.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AmiyaBotPlayerRatingServer.Model.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("AmiyaBotPlayerRatingServer.Model.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreAuthorization", b =>
                {
                    b.HasOne("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreApplication", "Application")
                        .WithMany("Authorizations")
                        .HasForeignKey("ApplicationId");

                    b.Navigation("Application");
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreToken", b =>
                {
                    b.HasOne("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreApplication", "Application")
                        .WithMany("Tokens")
                        .HasForeignKey("ApplicationId");

                    b.HasOne("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreAuthorization", "Authorization")
                        .WithMany("Tokens")
                        .HasForeignKey("AuthorizationId");

                    b.Navigation("Application");

                    b.Navigation("Authorization");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.MAARepetitiveTask", b =>
                {
                    b.Navigation("SubTasks");
                });

            modelBuilder.Entity("AmiyaBotPlayerRatingServer.Model.MAATask", b =>
                {
                    b.Navigation("SubTasks");
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreApplication", b =>
                {
                    b.Navigation("Authorizations");

                    b.Navigation("Tokens");
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreAuthorization", b =>
                {
                    b.Navigation("Tokens");
                });
#pragma warning restore 612, 618
        }
    }
}
