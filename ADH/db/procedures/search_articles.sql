CREATE OR REPLACE FUNCTION search_help_articles(
    query_embedding vector(768),
    match_threshold float,
    match_count int
)
RETURNS TABLE (
    id uuid,
    title varchar,
    content text,
    similarity float
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        a."Id",
        a."Title"::varchar,
        a."Content",
        (1 - (a."Embedding" <=> query_embedding))::float AS similarity
    FROM
        "HelpArticles" a
    WHERE
        1 - (a."Embedding" <=> query_embedding) > match_threshold
    ORDER BY
        a."Embedding" <=> query_embedding
    LIMIT match_count;
END;
$$;
