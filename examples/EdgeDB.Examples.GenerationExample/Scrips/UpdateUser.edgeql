UPDATE Person FILTER .id = <uuid>$id SET {
  name := <str>$name,
  email := <str>$email,
}