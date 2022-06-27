select schema::ObjectType {
  id,
  name,
  is_abstract,
  pointers: {
    real_cardinality := ("One" IF .required ELSE "AtMostOne") IF <str>.cardinality = "One" ELSE ("AtLeastOne" IF .required ELSE "Many"),
    name,
    target_id := .target.id,
    is_link := exists [IS schema::Link],
    is_exclusive := exists (select .constraints filter .name = 'std::exclusive'),
    is_computed := len(.computed_fields) != 0,
    is_readonly := .readonly,
    has_default := EXISTS .default or ("std::sequence" in .target[IS schema::ScalarType].ancestors.name),
  }
}
filter not .builtin;