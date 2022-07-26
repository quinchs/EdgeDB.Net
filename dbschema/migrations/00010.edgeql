CREATE MIGRATION m1um3yt7qj7ewz7tlflovxejbnkefpq2he5fwfvms2ucodqo5rupfq
    ONTO m1yp62tfavybznmg6ywpi7xkbf77ue7ezaubpye7btyv2pco4okf7q
{
  CREATE TYPE default::ArrayPerson {
      CREATE REQUIRED PROPERTY email -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY name -> std::str;
      CREATE REQUIRED PROPERTY roles -> array<std::str>;
  };
  CREATE TYPE default::LinkPerson {
      CREATE LINK best_friend -> default::LinkPerson;
      CREATE REQUIRED PROPERTY email -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY name -> std::str;
  };
  CREATE TYPE default::MultiLinkPerson {
      CREATE MULTI LINK best_friends -> default::MultiLinkPerson;
      CREATE REQUIRED PROPERTY email -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY name -> std::str;
  };
};
