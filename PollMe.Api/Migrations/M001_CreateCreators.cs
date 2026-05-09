using FluentMigrator;

namespace PollMe.Api.Migrations;

[Migration(1)]
public class M001_CreateCreators : Migration
{
    public override void Up()
    {
        Create.Table("creators")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("username").AsString().Unique().NotNullable()
            .WithColumn("password_hash").AsString().NotNullable()
            .WithColumn("created_at").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("creators");
    }
}
