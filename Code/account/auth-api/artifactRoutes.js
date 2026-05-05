// imports
import express from 'express';
import { pool } from './db.js';

const router = express.Router();

// post route to create a new artifact
router.post('/', async (req, res) => {
    try {
        const {
            date_discovered,
            investigator,
            area,
            unit,
            layer,
            site,
            associated_features,
            decorative_tech,
            material,
            firing,
            paint,
            cultural_affiliation,
            object_class,
            bag_number,
            artifact_id,
            userId
        } = req.body;

        // validate user input (required fields)
        if (
            !date_discovered ||
            !investigator ||
            !area ||
            !unit ||
            !layer ||
            !site ||
            !decorative_tech ||
            !material ||
            !firing ||
            !paint ||
            !cultural_affiliation ||
            !object_class ||
            !bag_number ||
            !artifact_id
        ) {
            return res.status(400).json({
                ok: false,
                error: 'All fields are required',
            });
        }

        // insert new artifact into database
        const insertArtifactQuery =
            `INSERT INTO artifacts 
        (date_discovered, investigator, area, unit, layer, site, 
         associated_features, decorative_tech, material, firing, 
         paint, cultural_affiliation, object_class, bag_number,
         artifact_id, user_id) 
         VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16)
         RETURNING *;
         `;

        const values = [
            date_discovered,
            investigator,
            area,
            unit,
            layer,
            site,
            associated_features || null,
            decorative_tech,
            material,
            firing,
            paint,
            cultural_affiliation,
            object_class,
            bag_number,
            artifact_id,
            userId || null
        ];

        // send query to database
        const result = await pool.query(insertArtifactQuery, values);

        // respond with success or error message
        res.status(201).json({
            ok: true,
            message: 'Artifact created successfully',
            artifact: result.rows[0]
        });
    } catch (error) {
        console.error('Error creating artifact:', error);
        res.status(500).json({
            ok: false,
            error: 'Internal server error'
        });
    }
});

// get route to show artifact to analyze
router.get('/latest/:userId', async (req, res) => {
    try {
        const { userId } = req.params;

        const selectArtifactQuery =
            `SELECT id, date_discovered, investigator, area,
                unit, layer, site, associated_features,
                decorative_tech, material, firing, paint,
                cultural_affiliation, object_class, bag_number,
                artifact_id, created_at, updated_at, user_id
         FROM artifacts
         WHERE user_id = $1 OR user_id IS NULL
         ORDER BY created_at DESC
         LIMIT 1;
         `;

        // send query to database
        const result = await pool.query(selectArtifactQuery, [userId]);

        // error handling
        if (result.rows.length === 0) {
            return res.status(404).json({
                ok: false, error: 'No artifact found',
            });
        }

        return res.status(200).json({
            ok: true, artifact: result.rows[0],
        });
    }
    catch (error) {
        console.error('Error fetching latest artifact:', error);
        return res.status(500).json({
            ok: false,
            error: 'Internal server error.',
        });
    }
});

// get artifacts that only specific users entered
router.get('/mine/:userId', async (req, res) => {

    // get userId
    try {
        const { userId } = req.params;

        // sql query
        const query =
            `SELECT id, date_discovered, investigator, area,
                unit, layer, site, associated_features,
                decorative_tech, material, firing, paint,
                cultural_affiliation, object_class, bag_number,
                artifact_id, created_at, updated_at, user_id
         FROM artifacts
         WHERE user_id = $1
         ORDER BY created_at DESC;
         `;

        const result = await pool.query(query, [userId]);

        // success
        return res.status(200).json({
            ok: true,
            artifacts: result.rows,
            error: null
        });

        // error handling
    } catch (error) {
        console.error("Error fetching user artifacts:", error);
        return res.status(500).json({
            ok: false,
            artifacts: [],
            error: "Internal server error."
        });
    }
});

// get all artifacts (for dropdown in analysis scene)
router.get('/', async (req, res) => {
    try {
        const selectAllArtifactsQuery =
            `SELECT id, date_discovered, investigator, area,
                unit, layer, site, associated_features,
                decorative_tech, material, firing, paint,
                cultural_affiliation, object_class, bag_number,
                artifact_id, created_at, updated_at, user_id
         FROM artifacts
         ORDER BY created_at DESC;
         `;

        // send query to database
        const result = await pool.query(selectAllArtifactsQuery);

        // error handling
        return res.status(200).json({
            ok: true, artifacts: result.rows, error: null,
        });
    }
    catch (error) {
        console.error('Error fetching artifacts list:', error);
        return res.status(500).json({
            ok: false, artifacts: [], error: "Internal server error.",
        });
    }
});

// get answer-key artifact by id
router.get('/reference/:artifactId', async (req, res) => {
    try {
        const { artifactId } = req.params;

        // query
        const query = `
            SELECT id, date_discovered, investigator, area,
                   unit, layer, site, associated_features,
                   decorative_tech, material, firing, paint,
                   cultural_affiliation, object_class, bag_number,
                   artifact_id, created_at, updated_at, user_id
            FROM artifacts
            WHERE artifact_id = $1
              AND user_id IS NULL
            ORDER BY created_at DESC
            LIMIT 1;
        `;

        const result = await pool.query(query, [artifactId]);

        // error handle
        if (result.rows.length == 0) {
            return res.status(404).json({
                ok: false,
                error: "No reference artifact found for this artifact ID."
            });
        }

        return res.status(200).json({
            ok: true,
            artifact: result.rows[0],
            error: null
        });

    } catch (error) {
        console.error("Error fetching reference artifact:", error);
        return res.status(500).json({
            ok: false,
            artifact: null,
            error: "Internatl server error."
        });
    }
});

// export the router
export default router;