-- enable pg vector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- enable fulltext search
CREATE EXTENSION IF NOT EXISTS pg_trgm;

DO $$
BEGIN
  RAISE NOTICE '=== ADH: Extensions installed (vector, pg_trgm) ===';
END
$$;