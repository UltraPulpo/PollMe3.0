using FluentMigrator;

namespace PollMe.Api.Migrations;

[Migration(5)]
public class M005_CreateVoteSelections : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            CREATE TABLE vote_selections (
                vote_id   INTEGER NOT NULL,
                option_id INTEGER NOT NULL,
                PRIMARY KEY (vote_id, option_id),
                FOREIGN KEY (vote_id)   REFERENCES votes(id),
                FOREIGN KEY (option_id) REFERENCES options(id)
            )");
    }

    public override void Down()
    {
        Delete.Table("vote_selections");
    }
}
