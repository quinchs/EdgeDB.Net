CREATE MIGRATION m1hk5lddsbraets3p3xtsj2rumcpydkdpbyoy3ssh3vqmudexna3ya
    ONTO m1ckoojrq2xwxscsdfrsrrc3rem4afqpvjit65vawiavkhsfaigzna
{
  CREATE MODULE lexertest IF NOT EXISTS;
  CREATE MODULE `💯💯💯` IF NOT EXISTS;
  CREATE ABSTRACT ANNOTATION lexertest::`🍿`;
  CREATE FUNCTION lexertest::`💯`(NAMED ONLY `🙀`: std::int64) ->  std::int64 {
      SET volatility := 'Immutable';
      CREATE ANNOTATION lexertest::`🍿` := 'fun!🚀';
      USING (SELECT
          (100 - `🙀`)
      )
  ;};
  CREATE SCALAR TYPE lexertest::Genre EXTENDING enum<Horror, Action, RomCom>;
  CREATE ABSTRACT TYPE lexertest::HasAge {
      CREATE PROPERTY age -> std::int64;
  };
  CREATE ABSTRACT TYPE lexertest::HasName {
      CREATE PROPERTY name -> std::str;
  };
  CREATE SCALAR TYPE lexertest::bag_seq EXTENDING std::sequence;
  CREATE TYPE lexertest::Bag EXTENDING lexertest::HasName, lexertest::HasAge {
      CREATE PROPERTY enumArr -> array<lexertest::Genre>;
      CREATE PROPERTY bigintField -> std::bigint;
      CREATE PROPERTY boolField -> std::bool;
      CREATE PROPERTY datetimeField -> std::datetime;
      CREATE PROPERTY decimalField -> std::decimal;
      CREATE PROPERTY durationField -> std::duration;
      CREATE PROPERTY float32Field -> std::float32;
      CREATE PROPERTY float64Field -> std::float64;
      CREATE PROPERTY genre -> lexertest::Genre;
      CREATE PROPERTY int16Field -> std::int16;
      CREATE PROPERTY int32Field -> std::int32;
      CREATE PROPERTY int64Field -> std::int64;
      CREATE PROPERTY jsonField -> std::json;
      CREATE PROPERTY localDateField -> cal::local_date;
      CREATE PROPERTY localDateTimeField -> cal::local_datetime;
      CREATE PROPERTY localTimeField -> cal::local_time;
      CREATE PROPERTY namedTuple -> tuple<x: std::str, y: std::int64>;
      CREATE PROPERTY secret_identity -> std::str;
      CREATE PROPERTY seqField -> lexertest::bag_seq;
      CREATE MULTI PROPERTY stringMultiArr -> array<std::str>;
      CREATE PROPERTY stringsArr -> array<std::str>;
      CREATE REQUIRED MULTI PROPERTY stringsMulti -> std::str;
      CREATE PROPERTY unnamedTuple -> tuple<std::str, std::int64>;
  };
  CREATE ABSTRACT CONSTRAINT lexertest::`🚀🍿`(max: std::int64) EXTENDING std::max_len_value;
  CREATE TYPE lexertest::A;
  CREATE SCALAR TYPE lexertest::你好 EXTENDING std::str;
  CREATE SCALAR TYPE lexertest::مرحبا EXTENDING lexertest::你好 {
      CREATE CONSTRAINT lexertest::`🚀🍿`(100);
  };
  CREATE SCALAR TYPE lexertest::`🚀🚀🚀` EXTENDING lexertest::مرحبا;
  CREATE TYPE lexertest::Łukasz {
      CREATE LINK `Ł💯` -> lexertest::A {
          CREATE PROPERTY `🙀مرحبا🙀` -> lexertest::مرحبا {
              CREATE CONSTRAINT lexertest::`🚀🍿`(200);
          };
          CREATE PROPERTY `🙀🚀🚀🚀🙀` -> lexertest::`🚀🚀🚀`;
      };
      CREATE REQUIRED PROPERTY `Ł🤞` -> lexertest::`🚀🚀🚀` {
          SET default := (<lexertest::`🚀🚀🚀`>'你好🤞');
      };
      CREATE INDEX ON (.`Ł🤞`);
  };
  CREATE TYPE lexertest::`S p a M` {
      CREATE REQUIRED PROPERTY `🚀` -> std::int32;
      CREATE PROPERTY c100 := (SELECT
          lexertest::`💯`(`🙀` := .`🚀`)
      );
  };
  CREATE FUNCTION `💯💯💯`::`🚀🙀🚀`(`🤞`: lexertest::`🚀🚀🚀`) ->  lexertest::`🚀🚀🚀` USING (SELECT
      <lexertest::`🚀🚀🚀`>(`🤞` ++ 'Ł🙀')
  );
  CREATE ABSTRACT LINK lexertest::movie_character {
      CREATE PROPERTY character_name -> std::str;
  };
  CREATE ABSTRACT TYPE lexertest::Person {
      CREATE REQUIRED PROPERTY name -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
  };
  CREATE TYPE lexertest::Profile {
      CREATE PROPERTY plot_summary -> std::str;
      CREATE PROPERTY slug -> std::str {
          SET readonly := true;
      };
  };
  CREATE TYPE lexertest::Movie {
      CREATE MULTI LINK characters EXTENDING lexertest::movie_character -> lexertest::Person;
      CREATE LINK profile -> lexertest::Profile {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE PROPERTY genre -> lexertest::Genre;
      CREATE PROPERTY rating -> std::float64;
      CREATE REQUIRED PROPERTY release_year -> std::int16 {
          SET default := (<std::int16>std::datetime_get(std::datetime_current(), 'year'));
      };
      CREATE REQUIRED PROPERTY title -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
  };
  ALTER TYPE lexertest::A {
      CREATE REQUIRED LINK `s p A m 🤞` -> lexertest::`S p a M`;
  };
  CREATE TYPE lexertest::Simple EXTENDING lexertest::HasName, lexertest::HasAge;
  CREATE TYPE lexertest::Hero EXTENDING lexertest::Person {
      CREATE PROPERTY number_of_movies -> std::int64;
      CREATE PROPERTY secret_identity -> std::str;
  };
  CREATE TYPE lexertest::Villain EXTENDING lexertest::Person {
      CREATE LINK nemesis -> lexertest::Hero;
  };
  ALTER TYPE lexertest::Hero {
      CREATE MULTI LINK villains := (.<nemesis[IS lexertest::Villain]);
  };
  CREATE TYPE lexertest::User {
      CREATE REQUIRED MULTI LINK favourite_movies -> lexertest::Movie;
      CREATE REQUIRED PROPERTY username -> std::str;
  };
  CREATE TYPE lexertest::MovieShape;
};
