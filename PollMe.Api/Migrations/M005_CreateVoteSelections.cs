using FluentMigrator;

namespace PollMe.Api.Migrations;

[Migration(5)]
public class M005_CreateVoteSelections : Migration
{
    public override void Up()
    {
        Create.Table("vote_selections")
            .WithColumn("vote_id").AsInt64().NotNullable().ForeignKey("votes", "id")
            .WithColumn("option_id").AsInt64().NotNullable().ForeignKey("options", "id");

        Create.PrimaryKey("pk_vote_selections").OnTable("vote_selections")
            .Columns("vote_id", "option_id");
    }

    public override void Down()
    {
        Delete.Table("vote_selections");
    }
}
