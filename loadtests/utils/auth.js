import http from "k6/http";
import { KEYCLOAK_URL } from "../config/config.js"

export function getToken(username, password) {
    const payload = {
        client_id: "saas-app",
        grant_type: "password",
        username: username,
        password: password
    };

    const headers = { "Content-Type": "application/x-www-form-urlencoded" };

    const res = http.post(KEYCLOAK_URL, payload, { headers });

    if (res.status !== 200) {
        throw new Error(`Failed to get token: ${res.status} ${res.body}`);
    }

    return res.json("access_token");
}
