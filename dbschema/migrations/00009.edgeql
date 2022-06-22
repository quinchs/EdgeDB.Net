CREATE MIGRATION m1ckoojrq2xwxscsdfrsrrc3rem4afqpvjit65vawiavkhsfaigzna
    ONTO m1isiclyxqa32luj6hdazr4mft2mvvq4tmmuoygl7m7k2iimxm5y3a
{
  CREATE TYPE default::LinkPerson {
      CREATE LINK best_friend -> default::LinkPerson;
      CREATE REQUIRED PROPERTY email -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY name -> std::str;
  };
};
