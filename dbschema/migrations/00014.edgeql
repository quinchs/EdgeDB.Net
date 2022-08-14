CREATE MIGRATION m1y7mps3wkv2jm6w5qlrlqdbsimsvjcsfjl6jppmthbnj4kvpm3dlq
    ONTO m1pnpccusnd6k465bvotfp64pgkmrsas3sbunubc2t2k2r3b3vp6va
{
  ALTER TYPE syzuna::Auditable {
      DROP PROPERTY created_at;
      DROP PROPERTY updated_at;
  };
  ALTER TYPE syzuna::BgsData {
      DROP LINK conflict;
      DROP LINK faction_presence;
      DROP PROPERTY active_states;
      DROP PROPERTY happiness;
      DROP PROPERTY influence;
      DROP PROPERTY pending_states;
      DROP PROPERTY recovering_states;
  };
  ALTER TYPE syzuna::FactionPresence {
      DROP LINK bgs_data;
  };
  DROP TYPE syzuna::BgsData;
  ALTER TYPE syzuna::Commodity {
      DROP LINK listings;
      DROP PROPERTY category;
      DROP PROPERTY commodity_id64;
      DROP PROPERTY name;
      DROP PROPERTY symbol;
  };
  ALTER TYPE syzuna::MarketListing {
      DROP LINK commodity;
      DROP LINK market;
      DROP PROPERTY buy_price;
      DROP PROPERTY demand;
      DROP PROPERTY sell_price;
      DROP PROPERTY supply;
  };
  DROP TYPE syzuna::Commodity;
  DROP TYPE syzuna::Conflict;
  ALTER TYPE syzuna::Faction {
      DROP LINK controlled_stations;
      DROP LINK controlled_systems;
      DROP LINK faction_presences;
      DROP LINK home_system;
      DROP PROPERTY allegiance;
      DROP PROPERTY eddb_id;
      DROP PROPERTY government;
      DROP PROPERTY is_player_faction;
      DROP PROPERTY name;
  };
  ALTER TYPE syzuna::FactionPresence {
      DROP LINK faction;
      DROP LINK star_system;
      DROP PROPERTY is_active;
  };
  DROP TYPE syzuna::StarSystem;
  ALTER TYPE syzuna::Station {
      DROP LINK faction;
      ALTER LINK market {
          DROP CONSTRAINT std::exclusive;
      };
  };
  DROP TYPE syzuna::Faction;
  DROP TYPE syzuna::FactionPresence;
  ALTER TYPE syzuna::Market {
      DROP LINK listings;
      DROP LINK station;
      DROP PROPERTY market_id64;
      DROP PROPERTY prohibited_commodities;
  };
  ALTER TYPE syzuna::Station {
      DROP LINK market;
      ALTER LINK outfitting {
          DROP CONSTRAINT std::exclusive;
      };
  };
  DROP TYPE syzuna::Market;
  DROP TYPE syzuna::MarketListing;
  ALTER TYPE syzuna::Outfitting {
      DROP LINK modules;
      DROP LINK station;
      DROP PROPERTY market_id64;
  };
  ALTER TYPE syzuna::Station {
      DROP LINK outfitting;
      ALTER LINK shipyard {
          DROP CONSTRAINT std::exclusive;
      };
  };
  DROP TYPE syzuna::Outfitting;
  DROP TYPE syzuna::OutfittingModule;
  ALTER TYPE syzuna::Ship {
      DROP PROPERTY name;
      DROP PROPERTY ship_id64;
      DROP PROPERTY symbol;
  };
  ALTER TYPE syzuna::Shipyard {
      DROP LINK ships;
      DROP LINK station;
      DROP PROPERTY market_id64;
  };
  DROP TYPE syzuna::Ship;
  DROP TYPE syzuna::Station;
  DROP TYPE syzuna::Shipyard;
  DROP TYPE syzuna::Auditable;
  DROP SCALAR TYPE syzuna::Happiness;
  DROP MODULE syzuna;
};
