CREATE MIGRATION m1ekxv5ihvbr5g5v6um2ubsoonwunr6u2l3nzfup2pmjgcxvpzamlq
    ONTO m1um3yt7qj7ewz7tlflovxejbnkefpq2he5fwfvms2ucodqo5rupfq
{
  CREATE TYPE default::ConstraintPerson {
      CREATE REQUIRED PROPERTY email -> std::str;
      CREATE REQUIRED PROPERTY name -> std::str;
      CREATE CONSTRAINT std::exclusive ON ((.name, .email));
  };
};
