CREATE MIGRATION m1fmzelzxxda652eddles3g56rysvccxzivewujttti4radwepsy5q
    ONTO initial
{
  CREATE ABSTRACT TYPE default::AbstractThing {
      CREATE REQUIRED PROPERTY name -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
  };
  CREATE TYPE default::OtherThing EXTENDING default::AbstractThing {
      CREATE REQUIRED PROPERTY attribute -> std::str;
  };
  CREATE TYPE default::Thing EXTENDING default::AbstractThing {
      CREATE REQUIRED PROPERTY description -> std::str;
  };
  CREATE TYPE default::LinkPerson {
      CREATE LINK best_friend -> default::LinkPerson;
      CREATE REQUIRED PROPERTY email -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY name -> std::str;
  };
  CREATE TYPE default::Person {
      CREATE REQUIRED PROPERTY email -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY name -> std::str;
  };
  CREATE TYPE default::Movie {
      CREATE REQUIRED MULTI LINK actors -> default::Person;
      CREATE REQUIRED LINK director -> default::Person;
      CREATE REQUIRED PROPERTY title -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY year -> std::int32;
  };
  CREATE TYPE default::MultiLinkPerson {
      CREATE MULTI LINK best_friends -> default::MultiLinkPerson;
      CREATE REQUIRED PROPERTY email -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY name -> std::str;
  };
  CREATE SCALAR TYPE default::State EXTENDING enum<NotStarted, InProgress, Complete>;
  CREATE TYPE default::TODO {
      CREATE REQUIRED PROPERTY date_created -> std::datetime {
          SET default := (std::datetime_current());
      };
      CREATE REQUIRED PROPERTY description -> std::str;
      CREATE REQUIRED PROPERTY state -> default::State;
      CREATE REQUIRED PROPERTY title -> std::str;
  };
};
