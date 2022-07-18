CREATE MIGRATION m14rc3eoxb5cfgao72uec6ldnjpzsfen6jej74sv7bbwpatb7hmdva
    ONTO m1fmzelzxxda652eddles3g56rysvccxzivewujttti4radwepsy5q
{
  CREATE TYPE default::ArrayPerson {
      CREATE REQUIRED PROPERTY email -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY name -> std::str;
      CREATE REQUIRED PROPERTY roles -> array<std::str>;
  };
};
