module default {
  global current_user_id -> uuid;
  
  type Movie {
    required property title -> str {
      constraint exclusive;
    }
    required property year -> int32;
    required link director -> Person;
    required multi link actors -> Person;
  }
  type Person {
    required property name -> str;
    required property email -> str {
      constraint exclusive;
    }
  }
  type ArrayPerson {
    required property name -> str;
    required property roles -> array<str>;
    required property email -> str {
      constraint exclusive;
    }
  }

  # for example todo app
  scalar type State extending enum<NotStarted, InProgress, Complete>;
  type TODO {
    required property title -> str;
    required property description -> str;
    required property date_created -> std::datetime {
      default := std::datetime_current();
    }
    required property state -> State;
  }

  # for integration tests & examples
  abstract type AbstractThing {
    required property name -> str {
      constraint exclusive;
    }
  }
  type Thing extending AbstractThing {
    required property description -> str;
  }
  type OtherThing extending AbstractThing {
    required property attribute -> str;
  }

  # for query builder
  type LinkPerson {
    required property name -> str;
    required property email -> str {
      constraint exclusive;
    }
    link best_friend -> LinkPerson;
  }

  type MultiLinkPerson {
    required property name -> str;
    required property email -> str {
      constraint exclusive;
    }
    multi link best_friends -> MultiLinkPerson;
  }

  type ConstraintPerson {
    required property name -> str;
    required property email -> str;

    constraint exclusive on ((.name, .email));
  }
  type PropertyConstraintPerson {
    required property name -> str{
      constraint exclusive;
    }
    required property email -> str {
      constraint exclusive;
    }
  }
}