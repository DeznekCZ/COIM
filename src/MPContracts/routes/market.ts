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

        if ((body.CreationTime ?? 0) !== 0
            && fs.existsSync(`/home/coi/market/users/${(body.CreationTime ?? 0)}`)
        ) {
            if (fs.readFileSync(`/home/coi/market/users/${body.CreationTime}`)
                    .toString() === (body.EntityId ?? 0).toString()) {

                res.send(body.CreationTime.toString());
                return;
            }
        }

        const date = Date.now();
        fs.mkdirSync(`/home/coi/market/users`, { recursive: true });
        fs.writeFileSync(`/home/coi/market/users/${date}`, (body.EntityId ?? 0).toString());

        res.json(date);
    } catch (e) {
        console.log(e);
        res.status(500);
        res.send("Internal server error");
    }
});

router.get('/ping', async (req: express.Request, res: express.Response) => {
    try {
        res.status(200).send(process.env.MARKET_NAME);
    } catch (e) {
        console.log(e);
        res.status(500)
        res.send("Internal server error");
    }
});

export default router;