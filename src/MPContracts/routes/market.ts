/*
 * GET market page.
 */
import express = require('express');
import fs = require('fs');
import { Registration } from '../types/Registration';
const router = express.Router();


router.post('/register', async (req: express.Request, res: express.Response) => {
    try {
        const body: Registration = req.body ?? {};

        const date = Date.now();
        fs.mkdirSync(`/home/coi/market/users`, { recursive: true });
        fs.writeFileSync(`/home/coi/market/users/${date}`, (body.EntityId ?? 0).toString());

        res.send(date.toString());
    } catch (e) {
        console.log(e);
        res.status(500);
    }
});

export default router;