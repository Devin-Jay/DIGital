// imports
import pkg from 'pg';

// variables
const { Pool } = pkg;

// database connection
export const pool = new Pool({
    connectionString: process.env.DATABASE_URL,
    ssl: false
});
