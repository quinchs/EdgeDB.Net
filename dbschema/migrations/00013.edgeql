CREATE MIGRATION m1tcypfnfc5mkkldxbjqz5awkngj6gjleg7w3qrbagyq5k47uys6na
    ONTO m1qocyoinu75piizgrcf5veo5cq3tw5jzywpjf7exlhrb6rmz4ngua
{
  ALTER TYPE default::MultiLinkPerson {
      ALTER LINK best_friends {
          SET TYPE default::MultiLinkPerson USING (SELECT
              default::MultiLinkPerson
          );
      };
  };
};
