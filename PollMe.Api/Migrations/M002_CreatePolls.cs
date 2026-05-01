using FluentMigrator;

namespace PollMe.Api.Migrations;

[Migration(2)]
public class M002_CreatePolls : Migration
{
    public override void Up()
    {
        Create.Table("polls")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("creator_id").AsInt64().NotNullable().ForeignKey("creators", "id")
            .WithColumn("question").AsString().NotNullable()
            .WithColumn("slug").AsString().Unique().NotNullable()
            .WithColumn("mode").AsString().NotNullable()
            .WithColumn("visibility").AsString().NotNullable()
            .WithColumn("created_at").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("polls");
    }
}
