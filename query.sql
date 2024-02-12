SELECT DISTINCT
	case_id,
	latitude,
	longitude,
	unixepoch(collision_date)/(3600*24) as collision_day, 
	unixepoch('1970-01-01T' || collision_time) as collision_time
FROM collisions
WHERE
	case_id IS NOT NULL AND
	latitude IS NOT NULL AND
	longitude IS NOT NULL AND
	collision_date IS NOT NULL AND
	collision_time IS NOT NULL
--LIMIT 100000