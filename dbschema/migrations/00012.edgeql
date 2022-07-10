CREATE MIGRATION m1qocyoinu75piizgrcf5veo5cq3tw5jzywpjf7exlhrb6rmz4ngua
    ONTO m1vs5zqu667wwcqfbuugfklx4jcnnu5wnzigwcvyohumszc2mddjeq
{
  CREATE TYPE default::MultiLinkPerson {
      CREATE MULTI LINK best_friends -> default::LinkPerson;
      CREATE REQUIRED PROPERTY email -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY name -> std::str;
  };
};
