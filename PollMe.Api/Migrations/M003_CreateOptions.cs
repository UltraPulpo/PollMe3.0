using FluentMigrator;

namespace PollMe.Api.Migrations;

[Migration(3)]
public class M003_CreateOptions : Migration
{
    public override void Up()
    {
        Create.Table("options")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("poll_id").AsInt64().NotNullable().ForeignKey("polls", "id")
            .WithColumn("text").AsString().NotNullable()
            .WithColumn("position").AsInt32().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("options");
    }
}
