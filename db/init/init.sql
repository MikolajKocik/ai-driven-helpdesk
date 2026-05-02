-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create search function for help articles
-- This uses cosine similarity (<=>)
CREATE OR REPLACE FUNCTION search_help_articles(
  query_embedding vector(384),
  match_threshold float,
  match_count int
)
RETURNS TABLE (
  id uuid,
  title text,
  content text,
  similarity float
)
LANGUAGE plpgsql
AS $$
BEGIN
  RETURN QUERY
  SELECT
    ha.id,
    ha.title,
    ha.content,
    1 - (ha.embedding <=> query_embedding) AS similarity
  FROM "HelpArticles" ha
  WHERE 1 - (ha.embedding <=> query_embedding) > match_threshold
  ORDER BY ha.embedding <=> query_embedding
  LIMIT match_count;
END;
$$;

-- Create mock customer database schema for testing integration
-- This can be used to test CustomerSqlPlugin locally
CREATE TABLE IF NOT EXISTS orders (
    id SERIAL PRIMARY KEY,
    customer_email TEXT NOT NULL,
    status TEXT NOT NULL,
    total_amount DECIMAL(10, 2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS products (
    id SERIAL PRIMARY KEY,
    sku TEXT UNIQUE,
    name TEXT NOT NULL,
    stock_quantity INT,
    location TEXT
);

-- Seed some mock data
INSERT INTO products (sku, name, stock_quantity, location) 
VALUES ('LAP-001', 'Enterprise Laptop X1', 15, 'Warehouse A-12')
ON CONFLICT DO NOTHING;

INSERT INTO orders (customer_email, status, total_amount)
VALUES ('test@example.com', 'Shipped', 1200.50)
ON CONFLICT DO NOTHING;
