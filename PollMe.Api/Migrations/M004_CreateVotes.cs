using FluentMigrator;

namespace PollMe.Api.Migrations;

[Migration(4)]
public class M004_CreateVotes : Migration
{
    public override void Up()
    {
        Create.Table("votes")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("poll_id").AsInt64().NotNullable().ForeignKey("polls", "id")
            .WithColumn("session_token").AsString().NotNullable()
            .WithColumn("created_at").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("votes");
    }
}
