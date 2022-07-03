CREATE MIGRATION m1vs5zqu667wwcqfbuugfklx4jcnnu5wnzigwcvyohumszc2mddjeq
    ONTO m1hk5lddsbraets3p3xtsj2rumcpydkdpbyoy3ssh3vqmudexna3ya
{
  ALTER FUNCTION lexertest::`💯`(NAMED ONLY `🙀`: std::int64) {
      DROP ANNOTATION lexertest::`🍿`;
  };
  DROP ABSTRACT ANNOTATION lexertest::`🍿`;
  ALTER TYPE lexertest::Bag {
      DROP PROPERTY enumArr;
      DROP PROPERTY bigintField;
      DROP PROPERTY boolField;
      DROP PROPERTY datetimeField;
      DROP PROPERTY decimalField;
      DROP PROPERTY durationField;
      DROP PROPERTY float32Field;
      DROP PROPERTY float64Field;
      DROP PROPERTY genre;
      DROP PROPERTY int16Field;
      DROP PROPERTY int32Field;
      DROP PROPERTY int64Field;
      DROP PROPERTY jsonField;
      DROP PROPERTY localDateField;
      DROP PROPERTY localDateTimeField;
      DROP PROPERTY localTimeField;
      DROP PROPERTY namedTuple;
      DROP PROPERTY secret_identity;
      DROP PROPERTY seqField;
      DROP PROPERTY stringMultiArr;
      DROP PROPERTY stringsArr;
      DROP PROPERTY stringsMulti;
      DROP PROPERTY unnamedTuple;
  };
  DROP TYPE lexertest::Łukasz;
  ALTER SCALAR TYPE lexertest::مرحبا {
      DROP CONSTRAINT lexertest::`🚀🍿`(100);
  };
  DROP ABSTRACT CONSTRAINT lexertest::`🚀🍿`;
  ALTER TYPE lexertest::`S p a M` {
      DROP PROPERTY c100;
      DROP PROPERTY `🚀`;
  };
  DROP FUNCTION lexertest::`💯`(NAMED ONLY `🙀`: std::int64);
  DROP FUNCTION `💯💯💯`::`🚀🙀🚀`(`🤞`: lexertest::`🚀🚀🚀`);
  ALTER ABSTRACT LINK lexertest::movie_character {
      DROP PROPERTY character_name;
  };
  ALTER TYPE lexertest::Movie {
      DROP LINK characters;
      DROP LINK profile;
      DROP PROPERTY genre;
      DROP PROPERTY rating;
      DROP PROPERTY release_year;
      DROP PROPERTY title;
  };
  DROP ABSTRACT LINK lexertest::movie_character;
  DROP TYPE lexertest::A;
  ALTER TYPE lexertest::HasAge {
      DROP PROPERTY age;
  };
  ALTER TYPE lexertest::HasName {
      DROP PROPERTY name;
  };
  DROP TYPE lexertest::Bag;
  DROP TYPE lexertest::Simple;
  DROP TYPE lexertest::HasAge;
  DROP TYPE lexertest::HasName;
  ALTER TYPE lexertest::Person {
      DROP PROPERTY name;
  };
  ALTER TYPE lexertest::Hero {
      DROP LINK villains;
      DROP PROPERTY number_of_movies;
      DROP PROPERTY secret_identity;
  };
  DROP TYPE lexertest::Villain;
  DROP TYPE lexertest::Hero;
  DROP TYPE lexertest::User;
  DROP TYPE lexertest::Movie;
  DROP TYPE lexertest::MovieShape;
  DROP TYPE lexertest::Person;
  DROP TYPE lexertest::Profile;
  DROP TYPE lexertest::`S p a M`;
  DROP SCALAR TYPE lexertest::Genre;
  DROP SCALAR TYPE lexertest::bag_seq;
  DROP SCALAR TYPE lexertest::`🚀🚀🚀`;
  DROP SCALAR TYPE lexertest::مرحبا;
  DROP SCALAR TYPE lexertest::你好;
  DROP MODULE lexertest;
  DROP MODULE `💯💯💯`;
};
