WITH vdoi AS (
	SELECT
		case_id,
		MAX(
			CASE victim_degree_of_injury
				WHEN 'killed' THEN '4-dead'
				WHEN 'severe injury' THEN '3-severe'
				WHEN 'suspected serious injury' THEN '3-severe'
				WHEN 'suspected minor injury' THEN '2-minor'
				WHEN 'other visible injury' THEN '2-minor'
				WHEN 'possible injury' THEN '1-possible'
				WHEN 'complaint of pain' THEN '1-possible'
				WHEN 'no injury' THEN '0-none'
			END
		) AS victim_degree_of_injury
	FROM victims AS v
	GROUP BY case_id
)
SELECT
	c.case_id,
	v.victim_degree_of_injury as worst_degree_of_injury,
	c.latitude,
	c.longitude,
	CAST(STRFTIME('%w', c.collision_date) as int) as collision_week_day,
	CAST(STRFTIME('%j', c.collision_date) as int) as collision_year_day, 
	unixepoch('1970-01-01T' || c.collision_time) as collision_time
FROM collisions AS c
LEFT JOIN vdoi AS v
ON (c.case_id = v.case_id)
WHERE
	c.case_id IS NOT NULL AND
	c.latitude IS NOT NULL AND
	c.longitude IS NOT NULL AND
	c.collision_date IS NOT NULL AND
	c.collision_time IS NOT NULL
--LIMIT 100000