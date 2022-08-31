CREATE MIGRATION m1pnpccusnd6k465bvotfp64pgkmrsas3sbunubc2t2k2r3b3vp6va
    ONTO m1mqksv77zeiafcinnoo4ppb4gpoxchrogef3etb6altobsj72qvca
{
  CREATE MODULE syzuna IF NOT EXISTS;
  CREATE ABSTRACT TYPE syzuna::Auditable {
      CREATE REQUIRED PROPERTY created_at -> std::datetime {
          SET default := (std::datetime_of_statement());
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY updated_at -> std::datetime {
          SET default := (std::datetime_of_statement());
      };
  };
  CREATE TYPE syzuna::Conflict EXTENDING syzuna::Auditable {
      CREATE REQUIRED PROPERTY conflict_type -> std::str;
      CREATE REQUIRED PROPERTY faction1_name -> std::str;
      CREATE REQUIRED PROPERTY faction1_stake -> std::str;
      CREATE REQUIRED PROPERTY faction1_won_days -> std::int16;
      CREATE REQUIRED PROPERTY faction2_name -> std::str;
      CREATE REQUIRED PROPERTY faction2_stake -> std::str;
      CREATE REQUIRED PROPERTY faction2_won_days -> std::int16;
      CREATE REQUIRED PROPERTY status -> std::str;
  };
  CREATE SCALAR TYPE syzuna::Happiness EXTENDING enum<Elated, Happy, Discontented, Unhappy, Despondent, Unknown>;
  CREATE TYPE syzuna::BgsData EXTENDING syzuna::Auditable {
      CREATE LINK conflict -> syzuna::Conflict;
      CREATE REQUIRED PROPERTY active_states -> array<std::str>;
      CREATE REQUIRED PROPERTY happiness -> syzuna::Happiness;
      CREATE REQUIRED PROPERTY influence -> std::float64;
      CREATE REQUIRED PROPERTY pending_states -> array<std::str>;
      CREATE REQUIRED PROPERTY recovering_states -> array<std::str>;
  };
  CREATE TYPE syzuna::Commodity EXTENDING syzuna::Auditable {
      CREATE REQUIRED PROPERTY category -> std::str;
      CREATE REQUIRED PROPERTY commodity_id64 -> std::int64;
      CREATE REQUIRED PROPERTY name -> std::str;
      CREATE REQUIRED PROPERTY symbol -> std::str;
  };
  CREATE TYPE syzuna::Faction EXTENDING syzuna::Auditable {
      CREATE PROPERTY allegiance -> std::str;
      CREATE REQUIRED PROPERTY eddb_id -> std::int64;
      CREATE PROPERTY government -> std::str;
      CREATE REQUIRED PROPERTY is_player_faction -> std::bool;
      CREATE REQUIRED PROPERTY name -> std::str;
  };
  CREATE TYPE syzuna::FactionPresence EXTENDING syzuna::Auditable {
      CREATE REQUIRED PROPERTY is_native -> std::bool;
      CREATE REQUIRED MULTI LINK bgs_data -> syzuna::BgsData {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE LINK faction -> syzuna::Faction;
      CREATE REQUIRED PROPERTY is_active -> std::bool;
  };
  CREATE TYPE syzuna::Market EXTENDING syzuna::Auditable {
      CREATE PROPERTY market_id64 -> std::int64;
      CREATE REQUIRED PROPERTY prohibited_commodities -> array<std::str>;
  };
  CREATE TYPE syzuna::MarketListing EXTENDING syzuna::Auditable {
      CREATE REQUIRED LINK commodity -> syzuna::Commodity;
      CREATE REQUIRED PROPERTY buy_price -> std::int32;
      CREATE REQUIRED PROPERTY demand -> std::int32;
      CREATE REQUIRED PROPERTY sell_price -> std::int32;
      CREATE REQUIRED PROPERTY supply -> std::int32;
  };
  CREATE TYPE syzuna::OutfittingModule EXTENDING syzuna::Auditable {
      CREATE REQUIRED PROPERTY category -> std::str;
      CREATE REQUIRED PROPERTY class -> std::str;
      CREATE PROPERTY entitlement -> std::str;
      CREATE PROPERTY guidance -> std::str;
      CREATE PROPERTY mount -> std::str;
      CREATE REQUIRED PROPERTY name -> std::str;
      CREATE REQUIRED PROPERTY outfitting_module_id64 -> std::int64;
      CREATE REQUIRED PROPERTY rating -> std::str;
      CREATE PROPERTY ship -> std::str;
      CREATE REQUIRED PROPERTY symbol -> std::str;
  };
  CREATE TYPE syzuna::Outfitting EXTENDING syzuna::Auditable {
      CREATE MULTI LINK modules -> syzuna::OutfittingModule;
      CREATE PROPERTY market_id64 -> std::int64;
  };
  CREATE TYPE syzuna::Ship EXTENDING syzuna::Auditable {
      CREATE REQUIRED PROPERTY name -> std::str;
      CREATE REQUIRED PROPERTY ship_id64 -> std::int64;
      CREATE REQUIRED PROPERTY symbol -> std::str;
  };
  CREATE TYPE syzuna::Shipyard EXTENDING syzuna::Auditable {
      CREATE MULTI LINK ships -> syzuna::Ship;
      CREATE PROPERTY market_id64 -> std::int64;
  };
  CREATE TYPE syzuna::Station EXTENDING syzuna::Auditable {
      CREATE LINK faction -> syzuna::Faction;
      CREATE LINK market -> syzuna::Market {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE LINK outfitting -> syzuna::Outfitting {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE LINK shipyard -> syzuna::Shipyard {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY active_states -> array<std::str>;
      CREATE REQUIRED PROPERTY allegiance -> std::str;
      CREATE REQUIRED PROPERTY distance_to_arrival -> std::float64;
      CREATE REQUIRED PROPERTY government -> std::str;
      CREATE PROPERTY market_id64 -> std::float64;
      CREATE REQUIRED PROPERTY max_landing_pad_size -> std::str;
      CREATE REQUIRED PROPERTY name -> std::str;
      CREATE REQUIRED PROPERTY primary_economy -> std::str;
      CREATE REQUIRED PROPERTY secondary_economy -> std::str;
      CREATE REQUIRED PROPERTY services -> array<std::str>;
      CREATE REQUIRED PROPERTY station_type -> std::str;
  };
  CREATE TYPE syzuna::StarSystem EXTENDING syzuna::Auditable {
      CREATE LINK faction -> syzuna::Faction;
      CREATE MULTI LINK faction_presences -> syzuna::FactionPresence;
      CREATE LINK native_factions := (SELECT
          .faction_presences
      FILTER
          (.is_native = true)
      );
      CREATE MULTI LINK stations -> syzuna::Station;
      CREATE PROPERTY active_states -> array<std::str>;
      CREATE PROPERTY allegiance -> std::str;
      CREATE PROPERTY coord_x -> std::float64;
      CREATE PROPERTY coord_y -> std::float64;
      CREATE PROPERTY coordd_z -> std::float64;
      CREATE REQUIRED PROPERTY eddb_id -> std::int64;
      CREATE PROPERTY government -> std::str;
      CREATE REQUIRED PROPERTY id64 -> std::int64;
      CREATE REQUIRED PROPERTY name -> std::str;
      CREATE PROPERTY population -> std::int64;
      CREATE PROPERTY power_state -> std::str;
      CREATE PROPERTY powers -> array<std::str>;
      CREATE PROPERTY primary_economy -> std::str;
      CREATE PROPERTY secondary_economy -> std::str;
      CREATE PROPERTY security -> std::str;
  };
  ALTER TYPE syzuna::BgsData {
      CREATE LINK faction_presence := (.<bgs_data[IS syzuna::FactionPresence]);
  };
  ALTER TYPE syzuna::Commodity {
      CREATE LINK listings := (.<commodity[IS syzuna::MarketListing]);
  };
  ALTER TYPE syzuna::Faction {
      CREATE LINK controlled_stations := (.<faction[IS syzuna::Station]);
      CREATE LINK controlled_systems := (.<faction[IS syzuna::StarSystem]);
      CREATE LINK faction_presences := (.<faction[IS syzuna::FactionPresence]);
      CREATE LINK home_system -> syzuna::StarSystem;
  };
  ALTER TYPE syzuna::FactionPresence {
      CREATE LINK star_system -> syzuna::StarSystem;
  };
  ALTER TYPE syzuna::Market {
      CREATE MULTI LINK listings -> syzuna::MarketListing {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE LINK station := (.<market[IS syzuna::Station]);
  };
  ALTER TYPE syzuna::MarketListing {
      CREATE LINK market := (.<listings[IS syzuna::Market]);
  };
  ALTER TYPE syzuna::Outfitting {
      CREATE LINK station := (.<outfitting[IS syzuna::Station]);
  };
  ALTER TYPE syzuna::Shipyard {
      CREATE LINK station := (.<shipyard[IS syzuna::Station]);
  };
};
