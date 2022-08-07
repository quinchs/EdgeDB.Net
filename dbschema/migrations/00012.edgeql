CREATE MIGRATION m1mqksv77zeiafcinnoo4ppb4gpoxchrogef3etb6altobsj72qvca
    ONTO m1ekxv5ihvbr5g5v6um2ubsoonwunr6u2l3nzfup2pmjgcxvpzamlq
{
  CREATE TYPE default::PropertyConstraintPerson {
      CREATE REQUIRED PROPERTY email -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY name -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
  };
};
