/*
 * GET market page.
 */
import { log } from 'console';
import express = require('express');
import fs = require('fs');
import path = require('path');
import { off } from 'process';
import { ContractId } from '../../types/ContractId';
import { ContractList } from '../../types/ContractLists';
import { ContractParameters } from '../../types/ContractParameters';
import { Registration } from '../../types/Registration';
const router = express.Router();

router.post('/create', async (req: express.Request, res: express.Response) => {
    try {
        const authorization: Registration = JSON.parse(req.headers.authorization);

        if (!fs.existsSync(`/home/coi/market/users/${authorization?.CreationTime ?? 0}`)) {
            res.status(401);
            res.send("Missing authorization token");
            return;
        }

        const entityId: number = JSON.parse(fs.readFileSync(`/home/coi/market/users/${authorization.CreationTime}`).toString());

        if (entityId !== authorization.EntityId) {
            res.status(401);
            res.send("Invalid pass combination");
            return;
        }

        const body: ContractParameters = req.body;
        const date = Date.now();

        fs.mkdirSync(`/home/coi/market/offers`, { recursive: true });
        fs.writeFileSync(`/home/coi/market/offers/${authorization.CreationTime}.${date}.open`, JSON.stringify(body, null, ""));

        res.send(date.toString());
    } catch (e) {
        console.log(e);
        res.sendStatus(500);
    }
});

router.get('/list', async (req: express.Request, res: express.Response) => {
    try {
        const authorization: Registration = JSON.parse(req.headers.authorization);

        const userId = authorization?.CreationTime ?? 0;

        fs.mkdirSync(`/home/coi/market/offers`, { recursive: true });
        const files: string[] = fs.readdirSync(`/home/coi/market/offers`);

        const contracts: ContractList = { Available: [], Claimable: [], Owned: [], Entries: [] };
        for (const fileKey in files) {
            const file = files[fileKey];
            const partSplit = file.split(".");
            const user = Number.parseInt(partSplit[0]);
            const offer = Number.parseInt(partSplit[1]);
            const open = partSplit[2] === "open";

            if (!open && user !== userId)
                continue;

            const id: number = offer;

            if (user === userId && open)
                contracts.Owned.push(id);
            else if (user === userId && !open)
                contracts.Claimable.push(id);
            else
                contracts.Available.push(id);

            contracts.Entries.push({
                Id: id,
                Params: JSON.parse(fs.readFileSync(`/home/coi/market/offers/${file}`).toString())
            })
        }
        res.json(contracts);
    } catch (e) {
        console.log(e);
        res.status(500);
    }
})

router.get('/take/:id', async (req: express.Request, res: express.Response) => {
    try {
        const authorization: Registration = JSON.parse(req.headers.authorization);

        if (!fs.existsSync(`/home/coi/market/users/${authorization?.CreationTime ?? 0}`)) {
            res.status(401);
            res.send("Missing authorization token");
            return;
        }

        const entityId: number = JSON.parse(fs.readFileSync(`/home/coi/market/users/${authorization.CreationTime}`).toString());

        if (entityId !== authorization.EntityId) {
            res.status(401);
            res.send("Invalid pass combination");
            return;
        }

        const id: number = Number.parseInt(req.params.id);

        fs.mkdirSync(`/home/coi/market/offers`, { recursive: true });
        const files: string[] = fs.readdirSync(`/home/coi/market/offers`);

        for (const fileKey in files) {
            const file = files[fileKey];
            const partSplit = file.split(".");
            const user = Number.parseInt(partSplit[0]);
            const offer = Number.parseInt(partSplit[1]);
            const open = partSplit[2] === "open";

            if (offer !== id)
                continue;

            if (user === authorization.CreationTime) {
                res.status(406);
                res.send("Can not buy own item");
                return;
            }

            if (!open) {
                res.status(410);
                res.send("Already sold");
                return;
            }

            fs.renameSync(`/home/coi/market/offers/${file}`, `/home/coi/market/offers/${user}.${offer}.used`);
            res.status(200).send();
            return;
        }

        res.status(404);
        res.send("Already sold");
    } catch (e) {
        console.log(e);
        res.sendStatus(500);
    }
});

router.get('/claim/:id', async (req: express.Request, res: express.Response) => {
    try {
        const authorization: Registration = JSON.parse(req.headers.authorization);

        if (!fs.existsSync(`/home/coi/market/users/${authorization?.CreationTime ?? 0}`)) {
            res.status(401);
            res.send("Missing authorization token");
            return;
        }

        const entityId: number = JSON.parse(fs.readFileSync(`/home/coi/market/users/${authorization.CreationTime}`).toString());

        if (entityId !== authorization.EntityId) {
            res.status(401);
            res.send("Invalid pass combination");
            return;
        }

        const canBeClaimed = fs.existsSync(`/home/coi/market/offers/${authorization.CreationTime}.${req.params.id}.used`)
        const wasNotSold = fs.existsSync(`/home/coi/market/offers/${authorization.CreationTime}.${req.params.id}.open`)
        const doesNotExist = !canBeClaimed && !wasNotSold;

        if (doesNotExist) {
            res.status(404);
            res.send("Item does not exists");
            return;
        }

        if (wasNotSold) {
            res.status(406);
            res.send("Item was not sold");
            return;
        }

        fs.rmSync(`/home/coi/market/offers/${authorization.CreationTime}.${req.params.id}.used`);
        res.status(200).send();

    } catch (e) {
        console.log(e);
        res.sendStatus(500);
    }
});

router.get('/remove/:id', async (req: express.Request, res: express.Response) => {
    try {
        const authorization: Registration = JSON.parse(req.headers.authorization);

        if (!fs.existsSync(`/home/coi/market/users/${authorization?.CreationTime ?? 0}`)) {
            res.status(401);
            res.send("Missing authorization token");
            return;
        }

        const entityId: number = JSON.parse(fs.readFileSync(`/home/coi/market/users/${authorization.CreationTime}`).toString());

        if (entityId !== authorization.EntityId) {
            res.status(401);
            res.send("Invalid pass combination");
            return;
        }

        const canBeClaimed = fs.existsSync(`/home/coi/market/offers/${authorization.CreationTime}.${req.params.id}.used`)
        const isCreated = fs.existsSync(`/home/coi/market/offers/${authorization.CreationTime}.${req.params.id}.open`)
        const doesNotExist = !canBeClaimed && !isCreated;

        if (doesNotExist) {
            res.status(404);
            res.send("Item does not exists");
            return;
        }

        if (canBeClaimed) {
            res.status(406);
            res.send("Item must be claimed");
            return;
        }

        fs.rmSync(`/home/coi/market/offers/${authorization.CreationTime}.${req.params.id}.open`);
        res.status(200).send();

    } catch (e) {
        console.log(e);
        res.sendStatus(500);
    }
});

export default router;